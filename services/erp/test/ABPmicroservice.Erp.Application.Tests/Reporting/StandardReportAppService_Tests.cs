using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.HumanResources;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Reporting;

public class StandardReportAppService_Tests : ErpApplicationTestBase
{
    private readonly IStandardReportAppService _reports;

    public StandardReportAppService_Tests()
    {
        _reports = GetRequiredService<IStandardReportAppService>();
    }

    [Fact]
    public async Task The_Catalog_Lists_Every_Report_Once_Under_A_Known_Area()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var reports = await _reports.GetListAsync();

            reports.Count.ShouldBeGreaterThan(30);
            reports.Select(r => r.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count().ShouldBe(reports.Count);
            reports.Select(r => r.Id).Distinct().Count().ShouldBe(reports.Count);
            reports.ShouldAllBe(r => StandardReportAppService.PermissionOf(r.Area) != null);
        });
    }

    /// <summary>A report that throws on an ordinary request would only be found by the user who runs it.</summary>
    [Fact]
    public async Task Every_Report_Runs_For_A_Period()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            foreach (var report in await _reports.GetListAsync())
            {
                var result = await _reports.RunAsync(new RunStandardReportInput
                {
                    Code = report.Code,
                    FromDate = new DateTime(2026, 1, 1),
                    ToDate = new DateTime(2026, 12, 31),
                });

                result.Title.ShouldBe(report.Name);
                result.Columns.ShouldNotBeEmpty(report.Code);
            }
        });
    }

    [Fact]
    public async Task The_Employee_List_Honours_The_Number_Filter()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var employees = GetRequiredService<IEmployeeAppService>();
            var first = await employees.CreateAsync(new CreateUpdateEmployeeDto { FirstName = "Asha", LastName = "Njeri", JobTitle = "Clerk" });
            var second = await employees.CreateAsync(new CreateUpdateEmployeeDto { FirstName = "Peter", LastName = "Kamau" });

            var all = await _reports.RunAsync(new RunStandardReportInput { Code = "EmployeeList" });
            all.Rows.Select(r => r.Values["no"]).ShouldContain(first.No);
            all.Rows.Select(r => r.Values["no"]).ShouldContain(second.No);

            var one = await _reports.RunAsync(new RunStandardReportInput { Code = "employeelist", NoFilter = first.No });
            one.Rows.Count.ShouldBe(1);
            one.Rows[0].Values["name"].ShouldBe("Asha Njeri");
        });
    }

    [Fact]
    public async Task Trial_Balance_Budget_Shows_The_Budget_Of_The_Period()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var account = (await GetRequiredService<IGLAccountAppService>().GetListAsync(new GetGLAccountListInput { MaxResultCount = 1 })).Items[0];
            await GetRequiredService<IGLBudgetNameAppService>().CreateAsync(new CreateUpdateGLBudgetNameDto { Name = "PLAN" });

            var entries = GetRequiredService<IGLBudgetEntryAppService>();
            await entries.CreateAsync(new CreateUpdateGLBudgetEntryDto { BudgetName = "PLAN", GLAccountNo = account.No, Date = new DateTime(2026, 3, 1), Amount = 500m });
            await entries.CreateAsync(new CreateUpdateGLBudgetEntryDto { BudgetName = "PLAN", GLAccountNo = account.No, Date = new DateTime(2027, 3, 1), Amount = 900m });

            var result = await _reports.RunAsync(new RunStandardReportInput
            {
                Code = "TrialBalanceBudget",
                BudgetName = "plan",
                FromDate = new DateTime(2026, 1, 1),
                ToDate = new DateTime(2026, 12, 31),
            });

            var row = result.Rows.Single(r => (string)r.Values["accountNo"] == account.No);
            Convert.ToDecimal(row.Values["budget"]).ShouldBe(500m);
        });
    }

    [Fact]
    public async Task An_Unknown_Report_Is_Refused()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var exception = await Should.ThrowAsync<BusinessException>(() => _reports.RunAsync(new RunStandardReportInput { Code = "Nope" }));
            exception.Code.ShouldBe(ErpErrorCodes.Reports.UnknownStandardReport);
        });
    }
}
