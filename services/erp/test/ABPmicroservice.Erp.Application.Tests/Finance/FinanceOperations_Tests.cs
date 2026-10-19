using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Applying and unapplying posted entries, payment discounts, bank reconciliation, reminders and
/// finance charge memos, deferrals, the cash flow forecast and the budget matrix. The seed has
/// sample customer C00010 (30 days), payment terms 1M(8D) with 2% discount, discounts granted on
/// 6300, fees on 4920, interest on 4930, deferred revenue on 2400, reminder terms DOMESTIC with
/// three levels, finance charge terms "1.5 PCT" and bank account WWB-OPERATING on 1020.
/// </summary>
public class FinanceOperations_Tests : ErpApplicationTestBase
{
    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly ICustomerAppService _customers;
    private readonly IGeneralJournalAppService _journals;
    private readonly ICustomerLedgerEntryAppService _customerLedger;
    private readonly IEntryApplicationAppService _application;
    private readonly ICurrencyExchangeRateAppService _rates;
    private readonly IBankAccReconciliationAppService _bankRecs;
    private readonly IBankAccReconciliationLineAppService _bankRecLines;
    private readonly IBankReconciliationAppService _bankRec;
    private readonly IReminderAppService _reminders;
    private readonly IFinanceChargeMemoAppService _memos;
    private readonly IDeferralTemplateAppService _deferralTemplates;
    private readonly IPostedDeferralAppService _postedDeferrals;
    private readonly ICashFlowForecastAppService _cashFlow;
    private readonly ICashFlowForecastEntryAppService _cashFlowEntries;
    private readonly IBudgetMatrixAppService _budgets;
    private readonly IGLBudgetNameAppService _budgetNames;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<GLEntry, Guid> _glEntries;

    public FinanceOperations_Tests()
    {
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
        _journals = GetRequiredService<IGeneralJournalAppService>();
        _customerLedger = GetRequiredService<ICustomerLedgerEntryAppService>();
        _application = GetRequiredService<IEntryApplicationAppService>();
        _rates = GetRequiredService<ICurrencyExchangeRateAppService>();
        _bankRecs = GetRequiredService<IBankAccReconciliationAppService>();
        _bankRecLines = GetRequiredService<IBankAccReconciliationLineAppService>();
        _bankRec = GetRequiredService<IBankReconciliationAppService>();
        _reminders = GetRequiredService<IReminderAppService>();
        _memos = GetRequiredService<IFinanceChargeMemoAppService>();
        _deferralTemplates = GetRequiredService<IDeferralTemplateAppService>();
        _postedDeferrals = GetRequiredService<IPostedDeferralAppService>();
        _cashFlow = GetRequiredService<ICashFlowForecastAppService>();
        _cashFlowEntries = GetRequiredService<ICashFlowForecastEntryAppService>();
        _budgets = GetRequiredService<IBudgetMatrixAppService>();
        _budgetNames = GetRequiredService<IGLBudgetNameAppService>();
        _customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
    }

    [Fact]
    public async Task A_Payment_Within_The_Discount_Period_Takes_The_Discount()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetCustomerAsync(c => c.SetPaymentTerms("1M(8D)"));
            var invoiceNo = await PostInvoiceAsync("PD-1", new DateTime(2026, 3, 1));

            var invoice = await EntryAsync(invoiceNo);
            invoice.PmtDiscountDate.ShouldBe(new DateTime(2026, 3, 9));
            invoice.RemainingPmtDiscPossible.ShouldBe(14.92m); // 2% of 746

            await PayAsync("PD-PAY-1", new DateTime(2026, 3, 5), 731.08m, invoiceNo);

            invoice = await EntryAsync(invoiceNo);
            invoice.Open.ShouldBeFalse();
            invoice.RemainingAmount.ShouldBe(0m);
            invoice.RemainingPmtDiscPossible.ShouldBe(0m);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == "PD-PAY-1");
            Sum(entries, "6300").ShouldBe(14.92m);
            Sum(entries, "1200").ShouldBe(-746m);
            entries.Sum(e => e.Amount).ShouldBe(0m);
            (await _customers.GetByNoAsync("C00010")).Balance.ShouldBe(0m);

            var details = await _application.GetDetailedEntriesAsync(new GetDetailedEntriesInput { PartyType = PartyLedgerType.Customer, LedgerEntryNo = invoice.EntryNo });
            details.Select(d => d.EntryType).ShouldBe([DetailedCVLedgerEntryType.InitialEntry, DetailedCVLedgerEntryType.PaymentDiscount, DetailedCVLedgerEntryType.Application]);
            details.Sum(d => d.Amount).ShouldBe(0m);
        });
    }

    [Fact]
    public async Task A_Payment_After_The_Discount_Date_Leaves_The_Discount_Open()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetCustomerAsync(c => c.SetPaymentTerms("1M(8D)"));
            var invoiceNo = await PostInvoiceAsync("PD-2", new DateTime(2026, 3, 1));

            await PayAsync("PD-PAY-2", new DateTime(2026, 3, 20), 731.08m, invoiceNo);

            var invoice = await EntryAsync(invoiceNo);
            invoice.Open.ShouldBeTrue();
            invoice.RemainingAmount.ShouldBe(14.92m);
            Sum(await _glEntries.GetListAsync(e => e.DocumentNo == "PD-PAY-2"), "6300").ShouldBe(0m);
        });
    }

    [Fact]
    public async Task Applying_Posted_Entries_Partially_And_Unapplying_Restores_Them()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var invoiceNo = await PostInvoiceAsync("APL-1", new DateTime(2026, 3, 1));
            await PayAsync("APL-PAY-1", new DateTime(2026, 3, 10), 500m, null);

            var invoice = await EntryAsync(invoiceNo);
            var payment = await EntryAsync("APL-PAY-1");
            payment.RemainingAmount.ShouldBe(-500m);

            var applicable = await _application.GetApplicableEntriesAsync(new GetApplicableEntriesInput { PartyType = PartyLedgerType.Customer, ApplyingEntryId = payment.Id });
            applicable.ShouldContain(e => e.Id == invoice.Id);

            // Apply 300 of the invoice only: the payment keeps 200 open.
            var result = await _application.ApplyAsync(new ApplyEntriesInput
            {
                PartyType = PartyLedgerType.Customer,
                ApplyingEntryId = payment.Id,
                PostingDate = new DateTime(2026, 3, 10),
                Entries = [new EntryToApplyDto { EntryId = invoice.Id, AmountToApply = 300m }],
            });
            result.ApplyingEntry.RemainingAmount.ShouldBe(-200m);
            (await EntryAsync(invoiceNo)).RemainingAmount.ShouldBe(446m);

            // Then the rest of the payment.
            await _application.ApplyAsync(new ApplyEntriesInput
            {
                PartyType = PartyLedgerType.Customer,
                ApplyingEntryId = payment.Id,
                PostingDate = new DateTime(2026, 3, 11),
                Entries = [new EntryToApplyDto { EntryId = invoice.Id }],
            });
            (await EntryAsync("APL-PAY-1")).Open.ShouldBeFalse();
            (await EntryAsync(invoiceNo)).RemainingAmount.ShouldBe(246m);

            // Unapplying undoes the last application only, then the first.
            await _application.UnapplyAsync(new UnapplyEntryInput { PartyType = PartyLedgerType.Customer, EntryId = payment.Id, PostingDate = new DateTime(2026, 3, 12) });
            (await EntryAsync("APL-PAY-1")).RemainingAmount.ShouldBe(-200m);
            (await EntryAsync(invoiceNo)).RemainingAmount.ShouldBe(446m);

            await _application.UnapplyAsync(new UnapplyEntryInput { PartyType = PartyLedgerType.Customer, EntryId = payment.Id, PostingDate = new DateTime(2026, 3, 12) });
            (await EntryAsync("APL-PAY-1")).RemainingAmount.ShouldBe(-500m);
            invoice = await EntryAsync(invoiceNo);
            invoice.RemainingAmount.ShouldBe(746m);
            invoice.Open.ShouldBeTrue();

            var nothing = await Should.ThrowAsync<BusinessException>(() =>
                _application.UnapplyAsync(new UnapplyEntryInput { PartyType = PartyLedgerType.Customer, EntryId = payment.Id, PostingDate = new DateTime(2026, 3, 12) }));
            nothing.Code.ShouldBe(ErpErrorCodes.FinanceOperations.NothingToUnapply);

            // Applying in LCY moves nothing on the G/L, and the balance is what is still open.
            (await _customers.GetByNoAsync("C00010")).Balance.ShouldBe(246m);
        });
    }

    [Fact]
    public async Task Applying_Foreign_Currency_Entries_Realizes_The_Difference_And_Unapplying_Reverses_It()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await RateAsync(new DateTime(2026, 2, 1), 130m);
            await SetCustomerAsync(c => c.SetCurrency("USD"));
            var invoiceNo = await PostInvoiceAsync("FXA-1", new DateTime(2026, 3, 1));
            await RateAsync(new DateTime(2026, 4, 1), 125m);
            await PayAsync("FXA-PAY-1", new DateTime(2026, 4, 10), 746m, null, "USD");

            var payment = await EntryAsync("FXA-PAY-1");
            var invoice = await EntryAsync(invoiceNo);
            var result = await _application.ApplyAsync(new ApplyEntriesInput
            {
                PartyType = PartyLedgerType.Customer,
                ApplyingEntryId = payment.Id,
                PostingDate = new DateTime(2026, 4, 10),
                Entries = [new EntryToApplyDto { EntryId = invoice.Id }],
            });

            var losses = await _glEntries.GetListAsync(e => e.GLAccountNo == "8910");
            losses.Sum(e => e.Amount).ShouldBe(3730m);
            (await _customers.GetByNoAsync("C00010")).Balance.ShouldBe(0m);

            await _application.UnapplyAsync(new UnapplyEntryInput { PartyType = PartyLedgerType.Customer, EntryId = invoice.Id, PostingDate = new DateTime(2026, 4, 11) });
            (await _glEntries.GetListAsync(e => e.GLAccountNo == "8910")).Sum(e => e.Amount).ShouldBe(0m);
            (await EntryAsync(invoiceNo)).RemainingAmountLcy.ShouldBe(96980m);
            (await _customers.GetByNoAsync("C00010")).Balance.ShouldBe(3730m); // 96,980 owed less 93,250 paid
            result.ApplicationNo.ShouldBeGreaterThan(0);
        });
    }

    [Fact]
    public async Task A_Bank_Statement_Is_Matched_Its_Charges_Posted_And_It_Posts_Only_When_It_Balances()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var batch = await BatchAsync("BANKREC");
            await _journals.CreateLineAsync(new CreateUpdateGenJournalLineDto
            {
                GenJournalBatchId = batch.Id,
                PostingDate = new DateTime(2026, 3, 3),
                DocumentNo = "DEP-1",
                AccountType = GenJournalAccountType.BankAccount,
                AccountNo = "WWB-OPERATING",
                Description = "Deposit",
                Amount = 1000m,
                BalAccountType = GenJournalAccountType.GLAccount,
                BalAccountNo = "1010",
            });
            await _journals.RunPostingAsync(batch.Id);

            var rec = await _bankRecs.CreateAsync(new CreateUpdateBankAccReconciliationDto
            {
                StatementType = BankAccRecStmtType.BankReconciliation,
                BankAccountNo = "WWB-OPERATING",
                StatementNo = "ST-1",
                StatementDate = new DateTime(2026, 3, 31),
                StatementEndingBalance = 990m,
            });

            var imported = await _bankRec.ImportStatementAsync(rec.Id, new ImportBankStatementInput
            {
                Content = "Date,Description,Amount,Document\n2026-03-04,Deposit,1000.00,DEP-1\n2026-03-31,Bank charges,-10.00,\n",
            });
            imported.ShouldBe(2);

            (await _bankRec.MatchAutomaticallyAsync(rec.Id, new MatchAutomaticallyInput { DateTolerance = 3 })).ShouldBe(1);

            var worksheet = await _bankRec.GetWorksheetAsync(rec.Id);
            worksheet.Lines[0].Difference.ShouldBe(0m);
            worksheet.Lines[1].Difference.ShouldBe(-10m);
            worksheet.Difference.ShouldBe(0m);

            // A line left unmatched blocks posting.
            var unmatched = await Should.ThrowAsync<BusinessException>(() => _bankRec.RunPostingAsync(rec.Id));
            unmatched.Code.ShouldBe(ErpErrorCodes.FinanceOperations.BankRecLineNotFullyApplied);

            // Charges go to the expense account, and the bank entry that writes is matched.
            var chargeLine = await _bankRecLines.GetAsync(worksheet.Lines[1].Id);
            await _bankRecLines.UpdateAsync(chargeLine.Id, new CreateUpdateBankAccReconciliationLineDto
            {
                StatementType = chargeLine.StatementType,
                BankAccountNo = chargeLine.BankAccountNo,
                StatementNo = chargeLine.StatementNo,
                StatementLineNo = chargeLine.StatementLineNo,
                TransactionDate = chargeLine.TransactionDate,
                Description = chargeLine.Description,
                StatementAmount = chargeLine.StatementAmount,
                AccountType = GenJournalAccountType.GLAccount,
                AccountNo = "8910",
            });
            (await _bankRec.TransferDifferencesAsync(rec.Id)).ShouldBe(1);
            Sum(await _glEntries.GetListAsync(e => e.GLAccountNo == "8910"), "8910").ShouldBe(10m);

            var statementNo = await _bankRec.RunPostingAsync(rec.Id);
            statementNo.ShouldBe("ST-1");

            var bankEntries = await GetRequiredService<IRepository<BankAccountLedgerEntry, Guid>>().GetListAsync(e => e.BankAccountNo == "WWB-OPERATING");
            bankEntries.ShouldAllBe(e => e.StatementStatus == BankEntryStatementStatus.Closed && !e.Open);
            (await GetRequiredService<IRepository<BankAccount, Guid>>().FirstAsync(b => b.No == "WWB-OPERATING")).LastStatementNo.ShouldBe("ST-1");
        });
    }

    [Fact]
    public async Task A_Statement_That_Does_Not_Add_Up_Is_Refused()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var rec = await _bankRecs.CreateAsync(new CreateUpdateBankAccReconciliationDto
            {
                StatementType = BankAccRecStmtType.BankReconciliation,
                BankAccountNo = "WWB-OPERATING",
                StatementNo = "ST-2",
                StatementDate = new DateTime(2026, 3, 31),
                StatementEndingBalance = 50m,
            });

            var ex = await Should.ThrowAsync<BusinessException>(() => _bankRec.RunPostingAsync(rec.Id));
            ex.Code.ShouldBe(ErpErrorCodes.FinanceOperations.BankRecDifference);
        });
    }

    [Fact]
    public async Task Reminders_Climb_Their_Levels_And_Post_Fees_And_Interest()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetCustomerAsync(c => c.SetCollectionTerms("DOMESTIC", "1.5 PCT"));
            var invoiceNo = await PostInvoiceAsync("REM-INV-1", new DateTime(2026, 1, 1)); // due 31 January

            (await _reminders.SuggestAsync(new SuggestCollectionDocumentsInput { DocumentDate = new DateTime(2026, 2, 3) })).ShouldBe(0); // still in grace

            (await _reminders.SuggestAsync(new SuggestCollectionDocumentsInput { DocumentDate = new DateTime(2026, 2, 10) })).ShouldBe(1);
            var first = (await _reminders.GetListAsync(new GetCollectionDocumentListInput { CustomerNo = "C00010", MaxResultCount = 10 })).Items.Single();
            first.ReminderLevel.ShouldBe(1);
            first.TotalAmount.ShouldBe(0m);
            first.Lines.ShouldHaveSingleItem().DocumentNo.ShouldBe(invoiceNo);

            await _reminders.IssueAsync(first.Id, new IssueCollectionDocumentInput());
            (await EntryAsync(invoiceNo)).LastIssuedReminderLevel.ShouldBe(1);

            (await _reminders.SuggestAsync(new SuggestCollectionDocumentsInput { DocumentDate = new DateTime(2026, 3, 2) })).ShouldBe(1);
            var second = (await _reminders.GetListAsync(new GetCollectionDocumentListInput { CustomerNo = "C00010", Status = CollectionDocumentStatus.Open, MaxResultCount = 10 })).Items.Single();
            second.ReminderLevel.ShouldBe(2);
            var interest = second.Lines.Single(l => l.Type == CollectionLineType.CustomerLedgerEntry).Amount;
            interest.ShouldBe(Math.Round(746m * 1.5m / 100m * 30 / 30, 2)); // 30 days overdue at 1.5% a month
            second.Lines.Single(l => l.Type == CollectionLineType.GLAccount).Amount.ShouldBe(10m);

            var issued = await _reminders.IssueAsync(second.Id, new IssueCollectionDocumentInput());
            issued.Status.ShouldBe(CollectionDocumentStatus.Issued);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == issued.IssuedNo);
            Sum(entries, "4920").ShouldBe(-10m);
            Sum(entries, "4930").ShouldBe(-interest);
            Sum(entries, "1200").ShouldBe(10m + interest);

            var changed = await Should.ThrowAsync<BusinessException>(() => _reminders.DeleteAsync(issued.Id));
            changed.Code.ShouldBe(ErpErrorCodes.FinanceOperations.CollectionDocumentIssued);
        });
    }

    [Fact]
    public async Task A_Finance_Charge_Memo_Charges_Interest_From_The_End_Of_The_Grace_Period()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetCustomerAsync(c => c.SetCollectionTerms(null, "1.5 PCT"));
            await PostInvoiceAsync("FIN-INV-1", new DateTime(2026, 1, 1)); // due 31 January, grace to 5 February

            (await _memos.SuggestAsync(new SuggestCollectionDocumentsInput { DocumentDate = new DateTime(2026, 3, 7) })).ShouldBe(1);
            var memo = (await _memos.GetListAsync(new GetCollectionDocumentListInput { CustomerNo = "C00010", MaxResultCount = 10 })).Items.Single();
            var line = memo.Lines.ShouldHaveSingleItem();
            line.OverdueDays.ShouldBe(30);
            line.Amount.ShouldBe(11.19m); // 746 x 1.5% x 30 / 30

            var issued = await _memos.IssueAsync(memo.Id, new IssueCollectionDocumentInput { PostingDate = new DateTime(2026, 3, 7) });
            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == issued.IssuedNo);
            Sum(entries, "4930").ShouldBe(-11.19m);
            entries.Sum(e => e.Amount).ShouldBe(0m);
        });
    }

    [Fact]
    public async Task A_Deferred_Line_Is_Released_Month_By_Month()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var customer = await _customers.GetByNoAsync("C00010");
            var invoice = await _salesDocuments.CreateAsync(new CreateUpdateSalesHeaderDto
            {
                DocumentType = SalesDocumentType.Invoice,
                No = "DEF-1",
                CustomerId = customer.Id,
                PostingDate = new DateTime(2026, 3, 1),
                Lines = [new SalesLineInputDto { Type = DocumentLineType.GLAccount, No = "4000", Description = "Annual support", Quantity = 1, UnitPrice = 1200m, DeferralCode = "12M-REV" }],
            });
            var postedNo = (await _salesDocuments.RunPostingAsync(invoice.Id)).PostedDocumentNo;

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == postedNo);
            Sum(entries, "4000").ShouldBe(-1200m);
            Sum(entries, "2400").ShouldBe(0m);
            entries.Where(e => e.GLAccountNo == "4000" && e.PostingDate == new DateTime(2026, 3, 1)).Sum(e => e.Amount).ShouldBe(-100m);
            entries.Where(e => e.GLAccountNo == "4000").Select(e => e.PostingDate).Distinct().Count().ShouldBe(12);
            entries.Sum(e => e.Amount).ShouldBe(0m);

            var schedule = (await _postedDeferrals.GetListAsync(new GetPostedDeferralListInput { Filter = postedNo, MaxResultCount = 10 })).Items.ShouldHaveSingleItem();
            (await _postedDeferrals.GetAsync(schedule.Id)).Lines.Count.ShouldBe(12);
            schedule.AmountToDefer.ShouldBe(-1200m);
        });
    }

    [Fact]
    public async Task A_Straight_Line_Schedule_Starting_Mid_Month_Prorates_And_Adds_Up()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _deferralTemplates.CreateAsync(new CreateUpdateDeferralTemplateDto
            {
                Code = "3M-MID",
                Description = "Three months from the posting date",
                DeferralAccountNo = "2400",
                CalcMethod = DeferralCalculationMethod.StraightLine,
                StartDate = DeferralStartDate.PostingDate,
                NoOfPeriods = 3,
            });

            var periods = await _deferralTemplates.PreviewScheduleAsync(new PreviewDeferralInput { DeferralCode = "3M-MID", Amount = 900m, PostingDate = new DateTime(2026, 4, 16) });
            periods.Count.ShouldBe(4);
            periods.Sum(p => p.Amount).ShouldBe(900m);
            periods[0].Amount.ShouldBe(150m); // 15 of 30 days of 300
            periods[3].PostingDate.ShouldBe(new DateTime(2026, 7, 1));
        });
    }

    [Fact]
    public async Task The_Cash_Flow_Forecast_Takes_Open_Receivables_On_Their_Due_Dates()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var invoiceNo = await PostInvoiceAsync("CF-INV-1", new DateTime(2026, 3, 1)); // due 31 March
            var forecast = (await _cashFlow.GetListAsync(new GetCodeTableListInput { Filter = "CF-MAIN" })).Items.Single();

            var suggested = await _cashFlow.SuggestWorksheetLinesAsync(forecast.Id, new SuggestCashFlowInput { FromDate = new DateTime(2026, 3, 1), ToDate = new DateTime(2026, 6, 30) });
            suggested.ShouldBeGreaterThan(0);
            (await _cashFlow.RegisterAsync(forecast.Id)).ShouldBe(suggested);

            var entries = await _cashFlowEntries.GetListAsync(new GetCashFlowLineListInput { CashFlowForecastNo = "CF-MAIN", MaxResultCount = 100 });
            var receivable = entries.Items.Single(e => e.DocumentNo == invoiceNo);
            receivable.SourceType.ShouldBe(CashFlowSourceType.Receivables);
            receivable.CashFlowDate.ShouldBe(new DateTime(2026, 3, 31));
            receivable.Amount.ShouldBe(746m);

            var byPeriod = await _cashFlow.GetByPeriodAsync(forecast.Id, new AnalysisMatrixInput { StartDate = new DateTime(2026, 3, 1), PeriodType = AnalysisPeriodType.Month, PeriodCount = 3 });
            byPeriod.Periods.Count.ShouldBe(3);
            byPeriod.Rows.Single(r => r.SourceType == CashFlowSourceType.Receivables).Amounts[0].ShouldBe(746m);
            byPeriod.Balances[^1].ShouldBe(byPeriod.Totals.Sum());
        });
    }

    [Fact]
    public async Task Budget_Cells_Are_Set_Compared_With_Actuals_And_Copied()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _budgetNames.CreateAsync(new CreateUpdateGLBudgetNameDto { Name = "B2026", Description = "Budget 2026" });
            await _budgetNames.CreateAsync(new CreateUpdateGLBudgetNameDto { Name = "B2027", Description = "Budget 2027" });

            await _budgets.SetCellAsync(new SetBudgetCellInput { BudgetName = "B2026", AccountNo = "4000", PeriodStart = new DateTime(2026, 3, 1), PeriodEnd = new DateTime(2026, 3, 31), Amount = -1000m });
            // Typing the cell again replaces it rather than adding to it.
            await _budgets.SetCellAsync(new SetBudgetCellInput { BudgetName = "B2026", AccountNo = "4000", PeriodStart = new DateTime(2026, 3, 1), PeriodEnd = new DateTime(2026, 3, 31), Amount = -800m });
            await PostInvoiceAsync("BUD-INV-1", new DateTime(2026, 3, 10));

            var matrix = await _budgets.GetMatrixAsync(new BudgetMatrixInput { BudgetName = "B2026", StartDate = new DateTime(2026, 1, 1), PeriodCount = 12, AccountFilter = "4000", WithActual = true });
            var row = matrix.Rows.Single(r => r.AccountNo == "4000");
            row.Budget[2].ShouldBe(-800m);
            row.Actual[2].ShouldBe(-650m);
            row.TotalBudget.ShouldBe(-800m);

            (await _budgets.CopyBudgetAsync(new CopyBudgetInput { FromBudgetName = "B2026", ToBudgetName = "B2027", DateChange = "1Y", Factor = 1.1m })).ShouldBe(1);
            var next = await _budgets.GetMatrixAsync(new BudgetMatrixInput { BudgetName = "B2027", StartDate = new DateTime(2027, 1, 1), PeriodCount = 12, AccountFilter = "4000" });
            next.Rows.Single(r => r.AccountNo == "4000").Budget[2].ShouldBe(-880m);
        });
    }

    // --- helpers --------------------------------------------------------------------------------

    private async Task SetCustomerAsync(Action<Customer> change)
    {
        var customer = await _customerRepository.FirstAsync(c => c.No == "C00010");
        change(customer);
        await _customerRepository.UpdateAsync(customer, autoSave: true);
    }

    private async Task<PartyLedgerEntryDto> EntryAsync(string documentNo) =>
        (await _customerLedger.GetListAsync(new GetPartyLedgerEntryListInput { Filter = documentNo, MaxResultCount = 10 })).Items.Single(e => e.DocumentNo == documentNo);

    /// <summary>Posts an invoice of 746 (650 plus VAT) to the sample customer and returns its posted number.</summary>
    private async Task<string> PostInvoiceAsync(string no, DateTime postingDate)
    {
        var customer = await _customers.GetByNoAsync("C00010");
        var invoice = await _salesDocuments.CreateAsync(new CreateUpdateSalesHeaderDto
        {
            DocumentType = SalesDocumentType.Invoice,
            No = no,
            CustomerId = customer.Id,
            PostingDate = postingDate,
            Lines = new List<SalesLineInputDto>
            {
                new() { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle Assembly", Quantity = 2, UnitPrice = 300m },
                new() { Type = DocumentLineType.GLAccount, No = "4000", Description = "Delivery charge", Quantity = 1, UnitPrice = 50m },
            },
        });
        return (await _salesDocuments.RunPostingAsync(invoice.Id)).PostedDocumentNo;
    }

    private async Task PayAsync(string documentNo, DateTime postingDate, decimal amount, string appliesToDocNo, string currencyCode = null)
    {
        var batch = await BatchAsync(documentNo.Replace("-", string.Empty)[..Math.Min(10, documentNo.Replace("-", string.Empty).Length)]);
        await _journals.CreateLineAsync(new CreateUpdateGenJournalLineDto
        {
            GenJournalBatchId = batch.Id,
            PostingDate = postingDate,
            DocumentType = GLEntryDocumentType.Payment,
            DocumentNo = documentNo,
            AccountType = GenJournalAccountType.Customer,
            AccountNo = "C00010",
            Description = "Payment",
            Amount = -amount,
            CurrencyCode = currencyCode,
            BalAccountType = GenJournalAccountType.GLAccount,
            BalAccountNo = "1010",
            AppliesToDocNo = appliesToDocNo,
        });
        await _journals.RunPostingAsync(batch.Id);
    }

    private async Task<GenJournalBatchDto> BatchAsync(string name) =>
        await _journals.CreateBatchAsync(new CreateGenJournalBatchDto { JournalTemplateName = "GENERAL", Name = name });

    private async Task RateAsync(DateTime startingDate, decimal lcyPerDollar) =>
        await _rates.CreateAsync(new CreateUpdateCurrencyExchangeRateDto { CurrencyCode = "USD", StartingDate = startingDate, ExchangeRateAmount = 1m, RelationalExchangeRateAmount = lcyPerDollar });

    private static decimal Sum(IEnumerable<GLEntry> entries, string accountNo) =>
        entries.Where(e => e.GLAccountNo == accountNo).Sum(e => e.Amount);
}
