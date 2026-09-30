using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Posting in foreign currency, payment application with realized and unrealized exchange
/// differences, VAT on journal lines, the VAT settlement and return, and the employee ledger.
/// The seed has no LCY code, a USD currency whose gains go to 4910 and losses to 8910, expense
/// account 6100 defaulting to purchase VAT (DOMESTIC/STANDARD, 16%), and employee payables 2140.
/// </summary>
public class ForeignCurrencyVatAndEmployees_Tests : ErpApplicationTestBase
{
    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly ICustomerAppService _customers;
    private readonly IGeneralJournalAppService _journals;
    private readonly IGLRegisterAppService _registers;
    private readonly ICurrencyExchangeRateAppService _rates;
    private readonly IExchRateAdjustmentAppService _adjustment;
    private readonly IVatReportingAppService _vatReporting;
    private readonly IBankAccountAppService _bankAccounts;
    private readonly IEmployeeAppService _employees;
    private readonly IEmployeeLedgerEntryAppService _employeeEntries;
    private readonly ICustomerLedgerEntryAppService _customerLedger;
    private readonly IRepository<GLEntry, Guid> _glEntries;
    private readonly IRepository<VatEntry, Guid> _vatEntries;

    public ForeignCurrencyVatAndEmployees_Tests()
    {
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
        _journals = GetRequiredService<IGeneralJournalAppService>();
        _registers = GetRequiredService<IGLRegisterAppService>();
        _rates = GetRequiredService<ICurrencyExchangeRateAppService>();
        _adjustment = GetRequiredService<IExchRateAdjustmentAppService>();
        _vatReporting = GetRequiredService<IVatReportingAppService>();
        _bankAccounts = GetRequiredService<IBankAccountAppService>();
        _employees = GetRequiredService<IEmployeeAppService>();
        _employeeEntries = GetRequiredService<IEmployeeLedgerEntryAppService>();
        _customerLedger = GetRequiredService<ICustomerLedgerEntryAppService>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
        _vatEntries = GetRequiredService<IRepository<VatEntry, Guid>>();
    }

    [Fact]
    public async Task A_Foreign_Currency_Invoice_Keeps_The_Currency_On_The_Customer_And_Posts_LCY_To_The_GL()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var postedNo = await PostUsdInvoiceAsync("FX-SI-1");

            var entry = (await _customerLedger.GetListAsync(new GetPartyLedgerEntryListInput { Filter = postedNo })).Items.ShouldHaveSingleItem();
            entry.CurrencyCode.ShouldBe("USD");
            entry.Amount.ShouldBe(746m);
            entry.AmountLcy.ShouldBe(96980m); // 746 x 130

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == postedNo);
            Sum(entries, "1200").ShouldBe(96980m);
            Sum(entries, "4000").ShouldBe(-84500m); // 650 x 130
            Sum(entries, "2310").ShouldBe(-12480m); // 96 x 130
            Sum(entries, "5000").ShouldBe(300m);    // cost is already LCY
            entries.Sum(e => e.Amount).ShouldBe(0m);

            var vat = (await _vatEntries.GetListAsync(e => e.DocumentNo == postedNo)).ShouldHaveSingleItem();
            vat.Base.ShouldBe(-78000m);
            vat.Amount.ShouldBe(-12480m);

            (await _customers.GetByNoAsync("C00010")).Balance.ShouldBe(96980m);
        });
    }

    [Fact]
    public async Task A_Payment_At_Another_Rate_Closes_The_Invoice_And_Realizes_The_Difference()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var postedNo = await PostUsdInvoiceAsync("FX-SI-2");
            await RateAsync(new DateTime(2026, 4, 1), 125m);

            var batch = await BatchAsync("FXPAY");
            var line = await _journals.CreateLineAsync(new CreateUpdateGenJournalLineDto
            {
                GenJournalBatchId = batch.Id,
                PostingDate = new DateTime(2026, 4, 10),
                DocumentType = GLEntryDocumentType.Payment,
                DocumentNo = "FXPAY-1",
                AccountType = GenJournalAccountType.Customer,
                AccountNo = "C00010",
                Description = "Payment in dollars",
                Amount = -746m,
                CurrencyCode = "usd",
                BalAccountType = GenJournalAccountType.GLAccount,
                BalAccountNo = "1010",
                AppliesToDocNo = postedNo,
            });
            line.CurrencyCode.ShouldBe("USD");
            line.AmountLcy.ShouldBe(-93250m); // 746 x 125

            await _journals.RunPostingAsync(batch.Id);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == "FXPAY-1");
            Sum(entries, "1010").ShouldBe(93250m);
            // Booked at 130, paid at 125: the 3,730 shortfall is a realized loss.
            Sum(entries, "8910").ShouldBe(3730m);
            Sum(entries, "1200").ShouldBe(-96980m);
            entries.Sum(e => e.Amount).ShouldBe(0m);

            var invoice = (await _customerLedger.GetListAsync(new GetPartyLedgerEntryListInput { Filter = postedNo })).Items.Single();
            invoice.Open.ShouldBeFalse();
            invoice.RemainingAmount.ShouldBe(0m);
            (await _customers.GetByNoAsync("C00010")).Balance.ShouldBe(0m);

            // An applied payment cannot be reversed without undoing the application first.
            var register = (await _registers.GetListAsync(new GetGLRegistersInput { MaxResultCount = 100 })).Items.First(r => r.JournalBatchName == "GENERAL/FXPAY");
            var ex = await Should.ThrowAsync<BusinessException>(() => _registers.RunReversalAsync(new ReverseRegisterInput { RegisterNo = register.No }));
            ex.Code.ShouldBe(ErpErrorCodes.Registers.AppliedEntryCannotBeReversed);
        });
    }

    [Fact]
    public async Task Adjusting_Exchange_Rates_Revalues_Open_Entries_And_Foreign_Bank_Accounts()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var postedNo = await PostUsdInvoiceAsync("FX-SI-3");

            var bank = await _bankAccounts.CreateAsync(new CreateUpdateBankAccountDto { Name = "Dollar account", BankAccPostingGroup = "CHECKING", CurrencyCode = "USD" });
            var batch = await BatchAsync("FXBANK");
            var deposit = new CreateUpdateGenJournalLineDto
            {
                GenJournalBatchId = batch.Id,
                PostingDate = new DateTime(2026, 3, 5),
                DocumentNo = "FXDEP-1",
                AccountType = GenJournalAccountType.BankAccount,
                AccountNo = bank.No,
                Description = "Dollar deposit",
                Amount = 100m,
                BalAccountType = GenJournalAccountType.GLAccount,
                BalAccountNo = "1010",
            };

            // A dollar account only takes dollar lines.
            await _journals.CreateLineAsync(deposit);
            var mismatch = await Should.ThrowAsync<BusinessException>(() => _journals.RunPostingAsync(batch.Id));
            mismatch.Code.ShouldBe(ErpErrorCodes.Journals.CurrencyMismatch);

            var lines = await _journals.GetLinesAsync(batch.Id);
            deposit.CurrencyCode = "USD";
            await _journals.UpdateLineAsync(lines.Items.Single().Id, deposit);
            await _journals.RunPostingAsync(batch.Id);

            var afterDeposit = await _bankAccounts.GetAsync(bank.Id);
            afterDeposit.Balance.ShouldBe(100m);
            afterDeposit.BalanceLcy.ShouldBe(13000m);

            await RateAsync(new DateTime(2026, 3, 31), 140m);
            var input = new ExchRateAdjustmentInput { EndingDate = new DateTime(2026, 3, 31), DocumentNo = "EXCH-0326" };

            var preview = await _adjustment.CalculateAsync(input);
            preview.Posted.ShouldBeFalse();
            var customerLine = preview.Lines.Single(l => l.AccountType == ExchRateAdjmtAccountType.Customer);
            customerLine.OldAmountLcy.ShouldBe(96980m);
            customerLine.NewAmountLcy.ShouldBe(104440m); // 746 x 140
            customerLine.Difference.ShouldBe(7460m);
            preview.Lines.Single(l => l.AccountType == ExchRateAdjmtAccountType.BankAccount).Difference.ShouldBe(1000m);

            var result = await _adjustment.AdjustAsync(input);
            result.Posted.ShouldBeTrue();
            result.TotalGains.ShouldBe(8460m);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == "EXCH-0326");
            Sum(entries, "1200").ShouldBe(7460m);
            Sum(entries, "1020").ShouldBe(1000m);
            Sum(entries, "4910").ShouldBe(-8460m);
            entries.Sum(e => e.Amount).ShouldBe(0m);

            (await _bankAccounts.GetAsync(bank.Id)).BalanceLcy.ShouldBe(14000m);
            var invoice = (await _customerLedger.GetListAsync(new GetPartyLedgerEntryListInput { Filter = postedNo })).Items.Single();
            invoice.RemainingAmountLcy.ShouldBe(104440m);

            // Run again at the same rate: nothing moved, so nothing is posted.
            var again = await Should.ThrowAsync<BusinessException>(() => _adjustment.AdjustAsync(input));
            again.Code.ShouldBe(ErpErrorCodes.GeneralLedger.NothingToAdjust);

            (await _adjustment.GetRegistersAsync(new GetExchRateAdjmtRegisterListInput { MaxResultCount = 10 })).TotalCount.ShouldBe(2);

            // An adjustment rewrote the carried LCY of entries it did not post, so it is not reversible.
            var register = (await _registers.GetListAsync(new GetGLRegistersInput { MaxResultCount = 100 })).Items.Single(r => r.No == result.RegisterNo);
            register.IsReversible.ShouldBeFalse();
        });
    }

    [Fact]
    public async Task A_Journal_Line_On_An_Expense_Account_Carries_Its_Purchase_VAT()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var batch = await BatchAsync("VATJNL");
            var line = await _journals.CreateLineAsync(ExpenseLine(batch.Id, "VATJ-1", 116m, GenJournalAccountType.GLAccount, "1010"));

            // The posting type and VAT groups come from G/L account 6100.
            line.GenPostingType.ShouldBe(GeneralPostingType.Purchase);
            line.VatProdPostingGroup.ShouldBe("STANDARD");
            line.VatAmount.ShouldBe(16m);
            line.VatBaseAmount.ShouldBe(100m);

            // A line told explicitly to carry no VAT posts the whole amount to the account.
            var noVat = ExpenseLine(batch.Id, "VATJ-2", 50m, GenJournalAccountType.GLAccount, "1010");
            noVat.GenPostingType = GeneralPostingType.None;
            (await _journals.CreateLineAsync(noVat)).VatAmount.ShouldBe(0m);

            await _journals.RunPostingAsync(batch.Id);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == "VATJ-1");
            Sum(entries, "6100").ShouldBe(100m);
            Sum(entries, "2320").ShouldBe(16m);
            Sum(entries, "1010").ShouldBe(-116m);

            var vat = (await _vatEntries.GetListAsync(e => e.DocumentNo == "VATJ-1")).ShouldHaveSingleItem();
            vat.Type.ShouldBe(VatEntryType.Purchase);
            vat.Base.ShouldBe(100m);
            vat.Amount.ShouldBe(16m);

            Sum(await _glEntries.GetListAsync(e => e.DocumentNo == "VATJ-2"), "6100").ShouldBe(50m);
            (await _vatEntries.GetListAsync(e => e.DocumentNo == "VATJ-2")).ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task The_VAT_Return_Adds_Up_The_Period_And_The_Settlement_Clears_It()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var invoice = await _salesDocuments.CreateAsync(await SalesInvoiceAsync("VATS-SI-1"));
            await _salesDocuments.RunPostingAsync(invoice.Id);

            var batch = await BatchAsync("VATSTL");
            await _journals.CreateLineAsync(ExpenseLine(batch.Id, "VATS-J-1", 116m, GenJournalAccountType.GLAccount, "1010"));
            await _journals.RunPostingAsync(batch.Id);

            var march = new VatReturnInput { StartingDate = new DateTime(2026, 3, 1), EndingDate = new DateTime(2026, 3, 31), Selection = VatEntrySelection.Open };
            var vatReturn = await _vatReporting.CalculateReturnAsync(march);
            vatReturn.OutputVatOnSales.ShouldBe(96m);
            vatReturn.InputVat.ShouldBe(16m);
            vatReturn.NetVatDue.ShouldBe(80m);
            vatReturn.SalesExcludingVat.ShouldBe(600m);
            vatReturn.PurchasesExcludingVat.ShouldBe(100m);

            var settlement = new VatSettlementInput
            {
                StartingDate = march.StartingDate,
                EndingDate = march.EndingDate,
                PostingDate = march.EndingDate,
                DocumentNo = "VATSTL-0326",
                SettlementAccountNo = "2340",
            };

            (await _vatReporting.CalculateSettlementAsync(settlement)).NetVatPayable.ShouldBe(80m);

            var posted = await _vatReporting.SettleAsync(settlement);
            posted.Posted.ShouldBeTrue();

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == "VATSTL-0326");
            Sum(entries, "2310").ShouldBe(96m);
            Sum(entries, "2320").ShouldBe(-16m);
            Sum(entries, "2340").ShouldBe(-80m);
            entries.Sum(e => e.Amount).ShouldBe(0m);

            // Settled entries are closed: the open return is empty, the closed one shows them.
            (await _vatReporting.CalculateReturnAsync(march)).NetVatDue.ShouldBe(0m);
            march.Selection = VatEntrySelection.Closed;
            (await _vatReporting.CalculateReturnAsync(march)).NetVatDue.ShouldBe(80m);

            var nothing = await Should.ThrowAsync<BusinessException>(() => _vatReporting.SettleAsync(settlement));
            nothing.Code.ShouldBe(ErpErrorCodes.GeneralLedger.NothingToSettle);
        });
    }

    [Fact]
    public async Task An_Employee_Expense_Claim_Is_Paid_Out_And_Closed()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var employee = await _employees.CreateAsync(new CreateUpdateEmployeeDto { FirstName = "Alicia", LastName = "Thornber", EmployeePostingGroup = "EMPLOYEES" });

            var batch = await BatchAsync("EXPENSE");
            await _journals.CreateLineAsync(ExpenseLine(batch.Id, "EXP-1", 116m, GenJournalAccountType.Employee, employee.No));
            await _journals.RunPostingAsync(batch.Id);

            var claim = await _glEntries.GetListAsync(e => e.DocumentNo == "EXP-1");
            Sum(claim, "6100").ShouldBe(100m);
            Sum(claim, "2320").ShouldBe(16m);
            Sum(claim, "2140").ShouldBe(-116m);
            (await _employees.GetAsync(employee.Id)).Balance.ShouldBe(-116m);

            // An employee is paid, not invoiced.
            var invoiceLine = ExpenseLine(batch.Id, "EXP-BAD", 10m, GenJournalAccountType.Employee, employee.No);
            invoiceLine.DocumentType = GLEntryDocumentType.Invoice;
            await _journals.CreateLineAsync(invoiceLine);
            var ex = await Should.ThrowAsync<BusinessException>(() => _journals.RunPostingAsync(batch.Id));
            ex.Code.ShouldBe(ErpErrorCodes.Journals.EmployeeDocumentTypeNotAllowed);
            await _journals.DeleteLineAsync((await _journals.GetLinesAsync(batch.Id)).Items.Single().Id);

            await _journals.CreateLineAsync(new CreateUpdateGenJournalLineDto
            {
                GenJournalBatchId = batch.Id,
                PostingDate = new DateTime(2026, 3, 20),
                DocumentType = GLEntryDocumentType.Payment,
                DocumentNo = "EXPPAY-1",
                AccountType = GenJournalAccountType.Employee,
                AccountNo = employee.No,
                Description = "Expense payout",
                Amount = 116m,
                BalAccountType = GenJournalAccountType.BankAccount,
                BalAccountNo = "WWB-OPERATING",
                AppliesToDocNo = "EXP-1",
            });
            await _journals.RunPostingAsync(batch.Id);

            (await _employees.GetAsync(employee.Id)).Balance.ShouldBe(0m);
            var open = await _employeeEntries.GetListAsync(new GetEmployeeLedgerEntryListInput { EmployeeNo = employee.No, OnlyOpen = true });
            open.TotalCount.ShouldBe(0);
            var all = await _employeeEntries.GetListAsync(new GetEmployeeLedgerEntryListInput { EmployeeNo = employee.No });
            all.Items.Single(e => e.DocumentNo == "EXP-1").ClosedByEntryNo.ShouldBe(all.Items.Single(e => e.DocumentNo == "EXPPAY-1").EntryNo);

            Sum(await _glEntries.GetListAsync(e => e.DocumentNo == "EXPPAY-1"), "1020").ShouldBe(-116m);

            var delete = await Should.ThrowAsync<BusinessException>(() => _employees.DeleteAsync(employee.Id));
            delete.Code.ShouldBe(ErpErrorCodes.HumanResources.CannotDeleteWithEntries);
        });
    }

    private static CreateUpdateGenJournalLineDto ExpenseLine(Guid batchId, string documentNo, decimal amount, GenJournalAccountType balType, string balNo) => new()
    {
        GenJournalBatchId = batchId,
        PostingDate = new DateTime(2026, 3, 15),
        DocumentNo = documentNo,
        AccountType = GenJournalAccountType.GLAccount,
        AccountNo = "6100",
        Description = "Travel expenses",
        Amount = amount,
        BalAccountType = balType,
        BalAccountNo = balNo,
    };

    private async Task<GenJournalBatchDto> BatchAsync(string name) =>
        await _journals.CreateBatchAsync(new CreateGenJournalBatchDto { JournalTemplateName = "GENERAL", Name = name });

    private async Task RateAsync(DateTime startingDate, decimal lcyPerDollar)
    {
        await _rates.CreateAsync(new CreateUpdateCurrencyExchangeRateDto
        {
            CurrencyCode = "USD",
            StartingDate = startingDate,
            ExchangeRateAmount = 1m,
            RelationalExchangeRateAmount = lcyPerDollar,
        });
    }

    /// <summary>Bills the sample customer in dollars at 130 and posts the invoice; returns its posted number.</summary>
    private async Task<string> PostUsdInvoiceAsync(string no)
    {
        await RateAsync(new DateTime(2026, 2, 1), 130m);

        var customer = await _customers.GetByNoAsync("C00010");
        await _customers.UpdateAsync(customer.Id, new CreateUpdateCustomerDto
        {
            No = customer.No,
            Name = customer.Name,
            CustomerPostingGroup = customer.CustomerPostingGroup,
            GenBusPostingGroup = customer.GenBusPostingGroup,
            VatBusPostingGroup = customer.VatBusPostingGroup,
            PaymentTermsCode = customer.PaymentTermsCode,
            CurrencyCode = "USD",
        });

        var invoice = await _salesDocuments.CreateAsync(await SalesInvoiceAsync(no));
        invoice.CurrencyCode.ShouldBe("USD");
        return (await _salesDocuments.RunPostingAsync(invoice.Id)).PostedDocumentNo;
    }

    private static decimal Sum(IEnumerable<GLEntry> entries, string accountNo) =>
        entries.Where(e => e.GLAccountNo == accountNo).Sum(e => e.Amount);

    private async Task<CreateUpdateSalesHeaderDto> SalesInvoiceAsync(string no)
    {
        var customer = await _customers.GetByNoAsync("C00010");

        return new CreateUpdateSalesHeaderDto
        {
            DocumentType = SalesDocumentType.Invoice,
            No = no,
            CustomerId = customer.Id,
            PostingDate = new DateTime(2026, 3, 1),
            Lines = new List<SalesLineInputDto>
            {
                new() { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle Assembly", Quantity = 2, UnitPrice = 300m },
                new() { Type = DocumentLineType.GLAccount, No = "4000", Description = "Delivery charge", Quantity = 1, UnitPrice = 50m },
            },
        };
    }
}
