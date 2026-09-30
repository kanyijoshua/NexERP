using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// VAT on documents, credit memos, cost of goods sold, bank accounts in journals, payment terms,
/// accounting periods and the sales and inventory rules. The seed gives item 1000 the STANDARD
/// VAT group (16% for DOMESTIC parties) and a unit cost of 150.
/// </summary>
public class TaxAndCashManagement_Tests : ErpApplicationTestBase
{
    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly IPurchaseDocumentAppService _purchaseDocuments;
    private readonly ICustomerAppService _customers;
    private readonly IVendorAppService _vendors;
    private readonly IGeneralJournalAppService _journals;
    private readonly IGLRegisterAppService _registers;
    private readonly IBankAccountAppService _bankAccounts;
    private readonly IAccountingPeriodAppService _periods;
    private readonly IInventorySetupAppService _inventorySetup;
    private readonly ISalesSetupAppService _salesSetup;
    private readonly IItemAppService _items;
    private readonly IRepository<GLEntry, Guid> _glEntries;
    private readonly IRepository<VatEntry, Guid> _vatEntries;
    private readonly IRepository<CustomerLedgerEntry, Guid> _customerEntries;
    private readonly IRepository<BankAccountLedgerEntry, Guid> _bankEntries;

    public TaxAndCashManagement_Tests()
    {
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _purchaseDocuments = GetRequiredService<IPurchaseDocumentAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
        _vendors = GetRequiredService<IVendorAppService>();
        _journals = GetRequiredService<IGeneralJournalAppService>();
        _registers = GetRequiredService<IGLRegisterAppService>();
        _bankAccounts = GetRequiredService<IBankAccountAppService>();
        _periods = GetRequiredService<IAccountingPeriodAppService>();
        _inventorySetup = GetRequiredService<IInventorySetupAppService>();
        _salesSetup = GetRequiredService<ISalesSetupAppService>();
        _items = GetRequiredService<IItemAppService>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
        _vatEntries = GetRequiredService<IRepository<VatEntry, Guid>>();
        _customerEntries = GetRequiredService<IRepository<CustomerLedgerEntry, Guid>>();
        _bankEntries = GetRequiredService<IRepository<BankAccountLedgerEntry, Guid>>();
    }

    [Fact]
    public async Task Document_Lines_Carry_The_VAT_Of_Their_Posting_Setup()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var invoice = await _salesDocuments.CreateAsync(await SalesInvoiceAsync("VAT-SI-1", SalesDocumentType.Invoice));

            var item = invoice.Lines.Single(l => l.Type == DocumentLineType.Item);
            item.VatPercent.ShouldBe(16m);
            item.VatIdentifier.ShouldBe("VAT16");
            item.VatAmount.ShouldBe(96m);
            item.LineAmountIncludingVat.ShouldBe(696m);

            // G/L account 4000 has no VAT product group, so its line carries no VAT.
            invoice.Lines.Single(l => l.Type == DocumentLineType.GLAccount).VatAmount.ShouldBe(0m);
            invoice.TotalAmount.ShouldBe(650m);
            invoice.TotalAmountIncludingVat.ShouldBe(746m);

            // The customer's payment terms (30D) set the due date.
            invoice.PaymentTermsCode.ShouldBe("30D");
            invoice.DueDate.ShouldBe(new DateTime(2026, 3, 31));
        });
    }

    [Fact]
    public async Task A_Sales_Invoice_Posts_Output_VAT_And_Cost_Of_Goods_Sold()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var invoice = await _salesDocuments.CreateAsync(await SalesInvoiceAsync("VAT-SI-2", SalesDocumentType.Invoice));
            var posted = await _salesDocuments.RunPostingAsync(invoice.Id);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == posted.PostedDocumentNo);
            Sum(entries, "1200").ShouldBe(746m);   // receivables, VAT included
            Sum(entries, "4000").ShouldBe(-650m);  // revenue
            Sum(entries, "2310").ShouldBe(-96m);   // output VAT
            Sum(entries, "5000").ShouldBe(300m);   // COGS: 2 x 150
            Sum(entries, "1400").ShouldBe(-300m);  // inventory
            entries.Sum(e => e.Amount).ShouldBe(0m);

            var vat = (await _vatEntries.GetListAsync(e => e.DocumentNo == posted.PostedDocumentNo)).ShouldHaveSingleItem();
            vat.Type.ShouldBe(VatEntryType.Sale);
            vat.Base.ShouldBe(-600m);
            vat.Amount.ShouldBe(-96m);

            var customerEntry = (await _customerEntries.GetListAsync(e => e.DocumentNo == posted.PostedDocumentNo)).ShouldHaveSingleItem();
            customerEntry.DueDate.ShouldBe(new DateTime(2026, 3, 31));
        });
    }

    [Fact]
    public async Task A_Credit_Memo_Posts_Every_Amount_The_Other_Way_And_Returns_The_Stock()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var before = (await _items.GetByNoAsync("1000")).Inventory;

            var memo = await _salesDocuments.CreateAsync(await SalesInvoiceAsync("VAT-SCM-1", SalesDocumentType.CreditMemo));
            var posted = await _salesDocuments.RunPostingAsync(memo.Id);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == posted.PostedDocumentNo);
            Sum(entries, "1200").ShouldBe(-746m);
            Sum(entries, "2310").ShouldBe(96m);
            Sum(entries, "1400").ShouldBe(300m);
            entries.Sum(e => e.Amount).ShouldBe(0m);
            entries.ShouldAllBe(e => e.DocumentType == GLEntryDocumentType.CreditMemo);

            (await _items.GetByNoAsync("1000")).Inventory.ShouldBe(before + 2);
        });
    }

    [Fact]
    public async Task A_Reverse_Charge_Purchase_Self_Assesses_The_VAT()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var vendor = await _vendors.GetByNoAsync("V00010");
            await _vendors.UpdateAsync(vendor.Id, new CreateUpdateVendorDto
            {
                No = vendor.No,
                Name = vendor.Name,
                VendorPostingGroup = vendor.VendorPostingGroup,
                GenBusPostingGroup = vendor.GenBusPostingGroup,
                VatBusPostingGroup = "EXPORT",
                PaymentTermsCode = vendor.PaymentTermsCode,
            });

            var invoice = await _purchaseDocuments.CreateAsync(new CreateUpdatePurchaseHeaderDto
            {
                DocumentType = PurchaseDocumentType.Invoice,
                No = "RC-PI-1",
                VendorId = vendor.Id,
                PostingDate = new DateTime(2026, 3, 1),
                VendorInvoiceNo = "EXT-778",
                Lines = new List<PurchaseLineInputDto>
                {
                    new() { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle Assembly", Quantity = 1, DirectUnitCost = 100m },
                },
            });

            // The vendor is paid the line amount only; the VAT is accounted for by the buyer.
            invoice.TotalAmountIncludingVat.ShouldBe(100m);

            var posted = await _purchaseDocuments.RunPostingAsync(invoice.Id);
            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == posted.PostedDocumentNo);
            Sum(entries, "2100").ShouldBe(-100m);  // payables
            Sum(entries, "1400").ShouldBe(100m);   // inventory
            Sum(entries, "2320").ShouldBe(16m);    // input VAT
            Sum(entries, "2330").ShouldBe(-16m);   // self-assessed output VAT
            entries.Sum(e => e.Amount).ShouldBe(0m);
        });
    }

    [Fact]
    public async Task A_Bank_Account_Posts_Its_Ledger_And_Its_GL_Account_And_Reverses()
    {
        long registerNo = 0;
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var created = await _bankAccounts.CreateAsync(new CreateUpdateBankAccountDto { Name = "Petty bank", BankAccPostingGroup = "checking" });
            created.No.ShouldBe("B010"); // from the Bank Account Nos. series

            var batch = await _journals.CreateBatchAsync(new CreateGenJournalBatchDto { JournalTemplateName = "GENERAL", Name = "BANK" });
            await _journals.CreateLineAsync(new CreateUpdateGenJournalLineDto
            {
                GenJournalBatchId = batch.Id,
                PostingDate = new DateTime(2026, 2, 1),
                DocumentType = GLEntryDocumentType.Payment,
                DocumentNo = "BNK-001",
                AccountType = GenJournalAccountType.BankAccount,
                AccountNo = created.No,
                Description = "Cash deposit",
                Amount = 400m,
                BalAccountType = GenJournalAccountType.GLAccount,
                BalAccountNo = "1010",
            });
            registerNo = (await _journals.RunPostingAsync(batch.Id)).RegisterNo;

            (await _bankAccounts.GetAsync(created.Id)).Balance.ShouldBe(400m);
            (await _bankEntries.GetListAsync(e => e.DocumentNo == "BNK-001")).ShouldHaveSingleItem().Amount.ShouldBe(400m);
            Sum(await _glEntries.GetListAsync(e => e.DocumentNo == "BNK-001"), "1020").ShouldBe(400m);

            await _registers.RunReversalAsync(new ReverseRegisterInput { RegisterNo = registerNo });
            (await _bankAccounts.GetAsync(created.Id)).Balance.ShouldBe(0m);

            var ex = await Should.ThrowAsync<BusinessException>(() => _bankAccounts.DeleteAsync(created.Id));
            ex.Code.ShouldBe(ErpErrorCodes.CashManagement.CannotDeleteWithEntries);
        });
    }

    [Fact]
    public async Task Accounting_Periods_Are_Created_A_Year_At_A_Time_And_Closed()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            // The seed created the current calendar year; add the next one so it can be closed.
            var next = new DateTime(DateTime.Today.Year + 1, 1, 1);
            var created = await _periods.NewFiscalYearAsync(new NewFiscalYearDto { StartingDate = next, NoOfPeriods = 12, PeriodLength = "1M" });
            // January of that year already exists as the end of the seeded year, so it is kept.
            created.Items.Count.ShouldBe(12);
            created.Items.Where(p => p.NewFiscalYear).ShouldHaveSingleItem().StartingDate.ShouldBe(next.AddYears(1));

            var duplicate = await Should.ThrowAsync<BusinessException>(
                () => _periods.NewFiscalYearAsync(new NewFiscalYearDto { StartingDate = next, NoOfPeriods = 12 })
            );
            duplicate.Code.ShouldBe(ErpErrorCodes.GeneralLedger.AccountingPeriodExists);

            var closed = await _periods.CloseFiscalYearAsync();
            closed.FromDate.ShouldBe(new DateTime(DateTime.Today.Year, 1, 1));
            closed.ToDate.ShouldBe(new DateTime(DateTime.Today.Year, 12, 31));

            var list = await _periods.GetListAsync(new GetAccountingPeriodListInput { MaxResultCount = 100 });
            list.Items.Where(p => p.StartingDate.Year == DateTime.Today.Year).ShouldAllBe(p => p.Closed);
        });
    }

    [Fact]
    public async Task Negative_Inventory_Can_Be_Prevented()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var setup = await _inventorySetup.GetAsync();
            setup.PreventNegativeInventory = true;
            await _inventorySetup.UpdateAsync(setup);

            var invoice = await _salesDocuments.CreateAsync(await SalesInvoiceAsync("NEG-SI-1", SalesDocumentType.Invoice));
            var ex = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.RunPostingAsync(invoice.Id));
            ex.Code.ShouldBe(ErpErrorCodes.Inventory.InsufficientInventory);
        });
    }

    [Fact]
    public async Task Releasing_Past_The_Credit_Limit_Is_Refused()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var customer = await _customers.GetByNoAsync("C00010");
            await _customers.UpdateAsync(customer.Id, new CreateUpdateCustomerDto
            {
                No = customer.No,
                Name = customer.Name,
                CreditLimit = 500m,
                CustomerPostingGroup = customer.CustomerPostingGroup,
                GenBusPostingGroup = customer.GenBusPostingGroup,
                VatBusPostingGroup = customer.VatBusPostingGroup,
                PaymentTermsCode = customer.PaymentTermsCode,
            });

            var invoice = await _salesDocuments.CreateAsync(await SalesInvoiceAsync("CL-SI-1", SalesDocumentType.Invoice));
            var ex = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.ReleaseAsync(invoice.Id));
            ex.Code.ShouldBe(ErpErrorCodes.Sales.CreditLimitExceeded);

            // With no credit warnings the same document releases.
            var setup = await _salesSetup.GetAsync();
            setup.CreditWarnings = CreditWarnings.NoWarning;
            await _salesSetup.UpdateAsync(setup);
            (await _salesDocuments.ReleaseAsync(invoice.Id)).Status.ShouldBe(DocumentStatus.Released);
        });
    }

    [Fact]
    public async Task A_New_Item_Takes_Its_Number_From_The_Item_Series()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await GetRequiredService<IUnitOfMeasureAppService>().CreateAsync(new CreateUpdateUnitOfMeasureDto { Code = "NUMW", Description = "Numbered" });
            var item = await _items.CreateAsync(new CreateUpdateItemDto
            {
                Description = "Numbered widget",
                Type = ItemType.Service,
                BaseUnitOfMeasureCode = "NUMW",
                VatProdPostingGroup = "zero",
            });

            item.No.ShouldBe("I00100");
            item.VatProdPostingGroup.ShouldBe("ZERO");
        });
    }

    private static decimal Sum(IEnumerable<GLEntry> entries, string accountNo) =>
        entries.Where(e => e.GLAccountNo == accountNo).Sum(e => e.Amount);

    private async Task<CreateUpdateSalesHeaderDto> SalesInvoiceAsync(string no, SalesDocumentType type)
    {
        var customer = await _customers.GetByNoAsync("C00010");

        return new CreateUpdateSalesHeaderDto
        {
            DocumentType = type,
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
