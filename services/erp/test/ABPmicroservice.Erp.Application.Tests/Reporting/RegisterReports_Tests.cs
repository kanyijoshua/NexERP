using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Pensions;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>The member, cash, fixed asset, employee and trial balance reports in their register form.</summary>
public class RegisterReports_Tests : ErpApplicationTestBase
{
    private readonly IStandardReportAppService _reports;

    public RegisterReports_Tests()
    {
        _reports = GetRequiredService<IStandardReportAppService>();
    }

    /// <summary>
    /// One member of scheme REG with January contributions: 5,000 of the member's own and 10,000 of
    /// the employer's (registered), 2,000 voluntary (unregistered), and then 1,000 of arrears for January.
    /// </summary>
    private async Task<PensionMemberDto> SetUpMemberAsync()
    {
        await GetRequiredService<IPensionSchemeAppService>().CreateAsync(new CreateUpdatePensionSchemeDto { Code = "REG", Description = "Register Scheme" });
        var sponsor = await GetRequiredService<IPensionSponsorAppService>().CreateAsync(new CreateUpdatePensionSponsorDto { Name = "Register Ltd", SchemeCode = "REG" });
        var member = await GetRequiredService<IPensionMemberAppService>().CreateAsync(new CreateUpdatePensionMemberDto
        {
            SponsorNo = sponsor.No,
            FirstName = "Akinyi",
            LastName = "Ochieng",
            PayrollNo = "P-77",
            DateOfBirth = new DateTime(1970, 3, 5),
            JoinSchemeDate = new DateTime(2010, 1, 1),
            CurrentSalary = 100000m,
        });

        var schedules = GetRequiredService<IPensionContributionAppService>();
        var lines = GetRequiredService<IPensionContributionLineAppService>();

        async Task PostAsync(PensionContributionMode mode, CreateUpdatePensionContributionLineDto line, DateTime postingDate)
        {
            var schedule = await schedules.CreateAsync(new CreateUpdatePensionContributionHeaderDto
            {
                SponsorNo = sponsor.No,
                PostingDate = postingDate,
                ContributionPeriod = new DateTime(2026, 1, 1),
                ContributionMode = mode,
            });
            line.DocumentNo = schedule.No;
            line.MemberNo = member.No;
            await lines.CreateAsync(line);
            await schedules.ReleaseAsync(schedule.Id);
            await schedules.RunPostingAsync(schedule.Id);
        }

        await PostAsync(
            PensionContributionMode.Normal,
            new CreateUpdatePensionContributionLineDto { BasicSalary = 100000m, EmployeeTaxExempt = 5000m, EmployerTaxExempt = 10000m, EmployeeAvcNonTaxExempt = 2000m },
            new DateTime(2026, 1, 28)
        );
        await PostAsync(PensionContributionMode.Arrears, new CreateUpdatePensionContributionLineDto { EmployeeTaxExempt = 1000m }, new DateTime(2026, 2, 10));

        return member;
    }

    [Fact]
    public async Task The_Member_Reports_Show_The_Fund_As_The_Registers_Do()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var member = await SetUpMemberAsync();

            var balances = await _reports.RunAsync(new RunStandardReportInput { Code = "MemberBalances", SchemeCode = "REG", ToDate = new DateTime(2026, 12, 31) });
            balances.Columns.Select(c => c.Key).ToArray().ShouldBe(
                new[] { "no", "name", "employeeContribution", "employeeInterest", "employerContribution", "employerInterest", "avcContribution", "avcInterest", "total" }
            );
            var line = balances.Rows[0].Values;
            line["employeeContribution"].ShouldBe(6000m);
            line["employerContribution"].ShouldBe(10000m);
            line["avcContribution"].ShouldBe(2000m);
            line["total"].ShouldBe(18000m);

            var register = await _reports.RunAsync(new RunStandardReportInput
            {
                Code = "ContributionsRegister",
                SchemeCode = "REG",
                FromDate = new DateTime(2026, 1, 1),
                ToDate = new DateTime(2026, 1, 31),
            });
            register.Columns.Count.ShouldBe(24);
            var january = register.Rows[0].Values;
            january["regEmployee"].ShouldBe(5000m);
            january["regEmployer"].ShouldBe(10000m);
            january["unregEmployeeAvc"].ShouldBe(2000m);
            january["closingRegEmployee"].ShouldBe(5000m);

            // The statement keeps arrears apart from the month's own contributions.
            var statement = await _reports.RunAsync(new RunStandardReportInput
            {
                Code = "MemberContributionsStatement",
                SchemeCode = "REG",
                FromDate = new DateTime(2026, 1, 1),
                ToDate = new DateTime(2026, 12, 31),
            });
            statement.Rows[0].Values["period"].ShouldBe($"PF No.: {member.No}   Name: Akinyi Ochieng");
            statement.Rows[2].Values["period"].ShouldBe("2026");
            var month = statement.Rows[3].Values;
            month["period"].ShouldBe("Jan 2026");
            month["regEmployee"].ShouldBe(5000m);
            month["regEmployeeArrears"].ShouldBe(1000m);
            month["regEmployer"].ShouldBe(10000m);
            month["unregEmployee"].ShouldBe(2000m);
            month["total"].ShouldBe(18000m);
            statement.Rows.Last().Values["period"].ShouldBe("Grand Totals");
            statement.Rows.Last().Values["total"].ShouldBe(18000m);

            var listing = await _reports.RunAsync(new RunStandardReportInput { Code = "MemberListing", SchemeCode = "REG", ToDate = new DateTime(2026, 12, 31) });
            listing.Columns.Select(c => c.Header).ToArray().ShouldBe(
                new[] { "No.", "Name", "Name of Sponsor", "Date of Birth", "Gender", "National ID", "Payroll No.", "Salary", "EE", "ER" }
            );
            listing.Rows[0].Values["name"].ShouldBe("AKINYI OCHIENG");
            listing.Rows[0].Values["sponsorName"].ShouldBe("REGISTER LTD");
            listing.Rows.Last().Values["name"].ShouldBe("Total: 1 member(s)");
            listing.Rows.Last().Values["salary"].ShouldBe(100000m);

            var active = await _reports.RunAsync(new RunStandardReportInput { Code = "ActiveMembers", SchemeCode = "REG", ToDate = new DateTime(2026, 12, 31) });
            active.Rows[0].Values["payrollNo"].ShouldBe("P-77");

            var retirees = await _reports.RunAsync(new RunStandardReportInput
            {
                Code = "ExpectedRetirees",
                SchemeCode = "REG",
                FromDate = new DateTime(2030, 1, 1),
                ToDate = new DateTime(2030, 12, 31),
            });
            retirees.Rows[0].Values["retirementDate"].ShouldBe(new DateTime(2030, 3, 5));
            retirees.Rows.Last().Values["name"].ShouldBe("Total: 1 member(s)");

            // The contributions above posted to the G/L, and the account-by-account trial balance agrees with itself.
            var trialBalance = await _reports.RunAsync(new RunStandardReportInput { Code = "TrialBalanceByAccount", FromDate = new DateTime(2026, 1, 1), ToDate = new DateTime(2026, 12, 31) });
            var totals = trialBalance.Rows.Last().Values;
            totals["name"].ShouldBe("Totals");
            ((decimal)totals["debit"]).ShouldBe((decimal)totals["credit"]);
            ((decimal)totals["debit"]).ShouldBeGreaterThanOrEqualTo(18000m);
            trialBalance.Rows.ShouldContain(r => (string)r.Values["no"] == "2610" && (decimal)r.Values["credit"] == 18000m);
        });
    }

    [Theory]
    [InlineData("CashBook", new[] { "postingDate", "documentNo", "payee", "description", "debit", "credit", "balance" })]
    [InlineData("EmployeeReport", new[] { "no", "name", "birthDate", "bankAccountNo", "bankBranchNo", "socialSecurityNo", "gender", "jobTitle", "employmentDate", "costCentre", "basicPay" })]
    [InlineData(
        "FixedAssetsRegister",
        new[] { "asset", "yearOfPurchase", "costStart", "additions", "dateOfPurchase", "disposals", "costEnd", "depreciationStart", "charge", "depreciationEnd", "netBookValue" }
    )]
    public async Task The_Register_Reports_Have_The_Columns_Of_The_Registers(string code, string[] columns)
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var result = await _reports.RunAsync(new RunStandardReportInput { Code = code, FromDate = new DateTime(2026, 1, 1), ToDate = new DateTime(2026, 12, 31) });
            result.Columns.Select(c => c.Key).ToArray().ShouldBe(columns);
        });
    }
}
