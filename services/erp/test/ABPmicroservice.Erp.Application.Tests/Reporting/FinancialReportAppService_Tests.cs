using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Reporting;

public class FinancialReportAppService_Tests : ErpApplicationTestBase
{
    private static readonly DateTime From = new(2026, 1, 1);
    private static readonly DateTime To = new(2026, 12, 31);

    private readonly IFinancialReportAppService _reports;
    private readonly IGeneralJournalAppService _journals;

    public FinancialReportAppService_Tests()
    {
        _reports = GetRequiredService<IFinancialReportAppService>();
        _journals = GetRequiredService<IGeneralJournalAppService>();
    }

    /// <summary>
    /// The one assertion a trial balance exists to support: once everything is posted, the totals
    /// row is zero. If it is not, something has written an unbalanced entry.
    /// </summary>
    [Fact]
    public async Task The_Trial_Balance_Totals_To_Zero_After_Posting()
    {
        await PostAsync("TBTEST", "TB-001", "1010", "4000", 750m);

        var report = await InCompanyAsync(DefaultCompanyName, () => _reports.GetTrialBalanceAsync(Period()));

        var totals = report.Rows.Last();
        Amount(totals, "netChange").ShouldBe(0m);
        Amount(totals, "closingBalance").ShouldBe(0m);

        // Debits and credits are each reported positive, and they match.
        Amount(totals, "debit").ShouldBe(Amount(totals, "credit"));
    }

    [Fact]
    public async Task The_Trial_Balance_Shows_Each_Account_It_Touched()
    {
        await PostAsync("TBACC", "TB-002", "1010", "4000", 200m);

        var report = await InCompanyAsync(DefaultCompanyName, () => _reports.GetTrialBalanceAsync(Period()));

        var cash = report.Rows.Single(r => Text(r, "accountNo") == "1010");
        Amount(cash, "debit").ShouldBeGreaterThanOrEqualTo(200m);

        var revenue = report.Rows.Single(r => Text(r, "accountNo") == "4000");
        Amount(revenue, "credit").ShouldBeGreaterThanOrEqualTo(200m);
    }

    /// <summary>Revenue carries a credit balance and has to read positive on a statement.</summary>
    [Fact]
    public async Task The_Income_Statement_Reads_Revenue_As_A_Positive_Figure()
    {
        await PostAsync("ISTEST", "IS-001", "1010", "4000", 500m);

        var report = await InCompanyAsync(DefaultCompanyName, () => _reports.GetIncomeStatementAsync(Period()));

        var revenue = report.Rows.Single(r => Text(r, "accountNo") == "4000");
        Amount(revenue, "amount").ShouldBeGreaterThan(0m);

        var netIncome = report.Rows.Last();
        Text(netIncome, "name").ShouldBe("Net Income");
        Amount(netIncome, "amount").ShouldBeGreaterThan(0m);
    }

    /// <summary>
    /// A balance sheet that does not balance is the symptom of a broken posting engine, so the
    /// report carries its own difference row and this pins it at zero.
    /// </summary>
    [Fact]
    public async Task The_Balance_Sheet_Balances()
    {
        await PostAsync("BSTEST", "BS-001", "1010", "4000", 900m);

        var report = await InCompanyAsync(DefaultCompanyName, () => _reports.GetBalanceSheetAsync(Period()));

        var difference = report.Rows.Single(r => Text(r, "name") == "Difference");
        Amount(difference, "amount").ShouldBe(0m);
    }

    [Fact]
    public async Task The_Detail_Report_Lists_The_Entries_Behind_An_Account()
    {
        await PostAsync("GLDET", "GL-001", "1010", "4000", 125m);

        var report = await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _reports.GetGLDetailAsync(
                    new FinancialReportPeriodInput
                    {
                        FromDate = From,
                        ToDate = To,
                        AccountFilter = "1010",
                    }
                )
        );

        report.Rows.ShouldContain(r => Text(r, "documentNo") == "GL-001");
        report.Rows.ShouldAllBe(r => Text(r, "accountNo") == "1010");
    }

    /// <summary>
    /// The seeded balance sheet schedule adds accounts up by range and then adds rows together
    /// with a formula; both paths have to agree with the ledger.
    /// </summary>
    [Fact]
    public async Task An_Account_Schedule_Adds_Up_Ranges_And_Row_Formulas()
    {
        await PostAsync("ASTEST", "AS-001", "1010", "4000", 400m);

        var report = await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _reports.RunScheduleAsync(
                    new RunAccountScheduleInput
                    {
                        ScheduleName = "BALANCE",
                        FromDate = From,
                        ToDate = To,
                    }
                )
        );

        var cash = report.Rows.Single(r => Text(r, "rowNo") == "R20");
        var totalAssets = report.Rows.Single(r => Text(r, "rowNo") == "R50");

        Amount(cash, "c1").ShouldBeGreaterThanOrEqualTo(400m);

        // R50 is "R20+R30+R40", so it must equal the sum of those three rows.
        var receivables = Amount(report.Rows.Single(r => Text(r, "rowNo") == "R30"), "c1");
        var inventory = Amount(report.Rows.Single(r => Text(r, "rowNo") == "R40"), "c1");
        Amount(totalAssets, "c1").ShouldBe(Amount(cash, "c1") + receivables + inventory);
    }

    [Fact]
    public async Task A_Column_Layout_Adds_A_Column_Per_Layout_Line()
    {
        var report = await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _reports.RunScheduleAsync(
                    new RunAccountScheduleInput
                    {
                        ScheduleName = "INCOME",
                        ColumnLayoutName = "LASTYEAR",
                        FromDate = From,
                        ToDate = To,
                    }
                )
        );

        // Row number, description, then one column per layout line.
        report.Columns.Select(c => c.Key).ShouldBe(new[] { "rowNo", "description", "c10", "c20" });
        report.Columns.Single(c => c.Key == "c20").Header.ShouldBe("Last Year");
    }

    [Fact]
    public async Task Aged_Receivables_Bucket_An_Open_Invoice_By_Its_Due_Date()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var batch = await _journals.CreateBatchAsync(
                new CreateGenJournalBatchDto { JournalTemplateName = "GENERAL", Name = "AGED" }
            );

            await _journals.CreateLineAsync(
                new CreateUpdateGenJournalLineDto
                {
                    GenJournalBatchId = batch.Id,
                    PostingDate = new DateTime(2026, 1, 15),
                    DocumentType = GLEntryDocumentType.Invoice,
                    DocumentNo = "AGED-001",
                    AccountType = GenJournalAccountType.Customer,
                    AccountNo = "C00010",
                    Description = "Aged test invoice",
                    Amount = 600m,
                    BalAccountType = GenJournalAccountType.GLAccount,
                    BalAccountNo = "4000",
                }
            );

            await _journals.RunPostingAsync(batch.Id);
        });

        // The entry falls due 30 days after 15 January, so by 31 March it is 45 days overdue.
        var report = await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _reports.GetAgedAccountsAsync(
                    new AgedAccountsInput { Kind = AgedLedgerKind.Receivables, AsOfDate = new DateTime(2026, 3, 31) }
                )
        );

        var customer = report.Rows.Single(r => Text(r, "partyNo") == "C00010");
        Amount(customer, "bucket2").ShouldBe(600m);
        Amount(customer, "balance").ShouldBe(600m);
    }

    private static FinancialReportPeriodInput Period()
    {
        return new FinancialReportPeriodInput { FromDate = From, ToDate = To };
    }

    private async Task PostAsync(string batchName, string documentNo, string debitAccount, string creditAccount, decimal amount)
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var batch = await _journals.CreateBatchAsync(
                new CreateGenJournalBatchDto { JournalTemplateName = "GENERAL", Name = batchName }
            );

            await AddAsync(batch.Id, documentNo, debitAccount, amount);
            await AddAsync(batch.Id, documentNo, creditAccount, -amount);

            await _journals.RunPostingAsync(batch.Id);
        });
    }

    private Task AddAsync(Guid batchId, string documentNo, string accountNo, decimal amount)
    {
        return _journals.CreateLineAsync(
            new CreateUpdateGenJournalLineDto
            {
                GenJournalBatchId = batchId,
                PostingDate = new DateTime(2026, 2, 1),
                DocumentType = GLEntryDocumentType.Invoice,
                DocumentNo = documentNo,
                AccountType = GenJournalAccountType.GLAccount,
                AccountNo = accountNo,
                Description = "Report test",
                Amount = amount,
            }
        );
    }

    private static decimal Amount(ReportRowDto row, string key)
    {
        return Convert.ToDecimal(row.Values[key]);
    }

    private static string Text(ReportRowDto row, string key)
    {
        return row.Values.TryGetValue(key, out var value) ? value?.ToString() : null;
    }
}
