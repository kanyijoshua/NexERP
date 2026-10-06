using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Reporting;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Pensions;

public class PensionAdministration_Tests : ErpApplicationTestBase
{
    private readonly IPensionSchemeAppService _schemes;
    private readonly IPensionSponsorAppService _sponsors;
    private readonly IPensionMemberAppService _members;
    private readonly IPensionContributionAppService _contributions;
    private readonly IPensionContributionLineAppService _lines;
    private readonly IMemberExitAppService _exits;
    private readonly IPensionBeneficiaryAppService _beneficiaries;
    private readonly IPensionContributionRateAppService _rates;
    private readonly IPensionVestingScaleAppService _vesting;
    private readonly IPensionTaxReliefLimitAppService _limits;
    private readonly IMemberStatusEntryAppService _statusEntries;
    private readonly IMemberSalaryEntryAppService _salaryEntries;
    private readonly IPensionSetupAppService _setup;

    public PensionAdministration_Tests()
    {
        _schemes = GetRequiredService<IPensionSchemeAppService>();
        _sponsors = GetRequiredService<IPensionSponsorAppService>();
        _members = GetRequiredService<IPensionMemberAppService>();
        _contributions = GetRequiredService<IPensionContributionAppService>();
        _lines = GetRequiredService<IPensionContributionLineAppService>();
        _exits = GetRequiredService<IMemberExitAppService>();
        _beneficiaries = GetRequiredService<IPensionBeneficiaryAppService>();
        _rates = GetRequiredService<IPensionContributionRateAppService>();
        _vesting = GetRequiredService<IPensionVestingScaleAppService>();
        _limits = GetRequiredService<IPensionTaxReliefLimitAppService>();
        _statusEntries = GetRequiredService<IMemberStatusEntryAppService>();
        _salaryEntries = GetRequiredService<IMemberSalaryEntryAppService>();
        _setup = GetRequiredService<IPensionSetupAppService>();
    }

    /// <summary>A scheme with one sponsor at 5% + 10% and two members on 100,000 and 60,000 a month.</summary>
    private async Task<(PensionSponsorDto Sponsor, PensionMemberDto First, PensionMemberDto Second)> SetUpSchemeAsync(string scheme)
    {
        await _schemes.CreateAsync(new CreateUpdatePensionSchemeDto { Code = scheme, Description = scheme + " Staff Pension Scheme" });

        var sponsor = await _sponsors.CreateAsync(new CreateUpdatePensionSponsorDto { Name = scheme + " Employer Ltd", SchemeCode = scheme, EmployeeRatePct = 5m, EmployerRatePct = 10m });
        var first = await _members.CreateAsync(new CreateUpdatePensionMemberDto
        {
            SponsorNo = sponsor.No,
            FirstName = "Wanjiku",
            LastName = "Mwangi",
            DateOfBirth = new DateTime(1985, 6, 15),
            JoinSchemeDate = new DateTime(2016, 1, 1),
            CurrentSalary = 100000m,
        });
        var second = await _members.CreateAsync(new CreateUpdatePensionMemberDto
        {
            SponsorNo = sponsor.No,
            FirstName = "Otieno",
            LastName = "Odhiambo",
            DateOfBirth = new DateTime(1990, 2, 1),
            JoinSchemeDate = new DateTime(2020, 1, 1),
            CurrentSalary = 60000m,
        });

        return (sponsor, first, second);
    }

    /// <summary>Posts one member's fund: the member's own money and the employer's, all registered.</summary>
    private async Task<PensionContributionHeaderDto> PostFundAsync(PensionSponsorDto sponsor, PensionMemberDto member, decimal employee, decimal employer, PensionContributionMode mode = PensionContributionMode.Normal)
    {
        var schedule = await _contributions.CreateAsync(new CreateUpdatePensionContributionHeaderDto
        {
            SponsorNo = sponsor.No,
            PostingDate = new DateTime(2026, 1, 31),
            ContributionPeriod = new DateTime(2026, 1, 1),
            ContributionMode = mode,
        });
        await _lines.CreateAsync(new CreateUpdatePensionContributionLineDto { DocumentNo = schedule.No, MemberNo = member.No, EmployeeTaxExempt = employee, EmployerTaxExempt = employer });
        await _contributions.ReleaseAsync(schedule.Id);
        return await _contributions.RunPostingAsync(schedule.Id);
    }

    [Fact]
    public async Task Beneficiary_Shares_Stay_Within_100_And_A_Death_Benefit_Needs_Them_Complete()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var (sponsor, first, _) = await SetUpSchemeAsync("NOMINEE");
            await PostFundAsync(sponsor, first, 300000m, 600000m);

            var spouse = await _beneficiaries.CreateAsync(new CreateUpdatePensionBeneficiaryDto
            {
                MemberNo = first.No,
                Name = "Kamau Mwangi",
                Relationship = BeneficiaryRelationship.Spouse,
                BenefitPct = 60m,
            });
            spouse.LineNo.ShouldBe(10000);

            var tooMuch = await Should.ThrowAsync<BusinessException>(() => _beneficiaries.CreateAsync(new CreateUpdatePensionBeneficiaryDto
            {
                MemberNo = first.No,
                Name = "Neema Mwangi",
                Relationship = BeneficiaryRelationship.Child,
                BenefitPct = 50m,
            }));
            tooMuch.Code.ShouldBe(ErpErrorCodes.Pensions.BeneficiaryShareExceeded);

            var child = await _beneficiaries.CreateAsync(new CreateUpdatePensionBeneficiaryDto
            {
                MemberNo = first.No,
                Name = "Neema Mwangi",
                Relationship = BeneficiaryRelationship.Child,
                DateOfBirth = new DateTime(2015, 4, 1),
                GuardianName = "Kamau Mwangi",
                BenefitPct = 30m,
            });

            // The seeded DEATH reason pays everything to the beneficiaries, whose shares come to 90%.
            var exit = await _exits.CreateAsync(new CreateUpdateMemberExitDto { MemberNo = first.No, ReasonCode = "DEATH", ExitDate = new DateTime(2026, 3, 31) });
            exit.GrossLumpsum.ShouldBe(900000m);

            var incomplete = await Should.ThrowAsync<BusinessException>(() => _exits.ApproveAsync(exit.Id));
            incomplete.Code.ShouldBe(ErpErrorCodes.Pensions.BeneficiarySharesIncomplete);

            await _beneficiaries.UpdateAsync(child.Id, new CreateUpdatePensionBeneficiaryDto
            {
                MemberNo = first.No,
                Name = "Neema Mwangi",
                Relationship = BeneficiaryRelationship.Child,
                DateOfBirth = new DateTime(2015, 4, 1),
                GuardianName = "Kamau Mwangi",
                BenefitPct = 40m,
            });
            (await _exits.ApproveAsync(exit.Id)).Status.ShouldBe(MemberExitStatus.Approved);

            var shares = PensionBeneficiaryManager.Share(
                1000m,
                (await GetRequiredService<IRepository<PensionBeneficiary, Guid>>().GetListAsync(b => b.MemberNo == first.No)).OrderBy(b => b.LineNo).ToList()
            );
            shares.Select(s => s.Amount).ToArray().ShouldBe(new[] { 600m, 400m });

            var report = await GetRequiredService<IStandardReportAppService>().RunAsync(new RunStandardReportInput { Code = "Beneficiaries", SchemeCode = "NOMINEE", ToDate = new DateTime(2026, 12, 31) });
            report.Rows.Count.ShouldBe(3);
            report.Rows[2].Values["guardian"].ShouldBe("Kamau Mwangi");
        });
    }

    [Fact]
    public async Task Schedules_Take_The_Dated_Rates_And_Split_At_The_Tax_Relief_Limit()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var (sponsor, first, second) = await SetUpSchemeAsync("RELIEF");

            await _rates.CreateAsync(new CreateUpdatePensionContributionRateDto { SponsorNo = sponsor.No, StartDate = new DateTime(2026, 1, 1), EmployeeRatePct = 6m, EmployerRatePct = 12m });
            var overlap = await Should.ThrowAsync<BusinessException>(() => _rates.CreateAsync(new CreateUpdatePensionContributionRateDto
            {
                SponsorNo = sponsor.No,
                StartDate = new DateTime(2026, 6, 1),
                EmployeeRatePct = 7m,
            }));
            overlap.Code.ShouldBe(ErpErrorCodes.Pensions.ContributionRatePeriodOverlaps);

            await _limits.CreateAsync(new CreateUpdatePensionTaxReliefLimitDto { EffectiveDate = new DateTime(2025, 1, 1), MonthlyLimit = 15000m });

            var schedule = await _contributions.CreateAsync(new CreateUpdatePensionContributionHeaderDto
            {
                SponsorNo = sponsor.No,
                PostingDate = new DateTime(2026, 2, 27),
                ContributionPeriod = new DateTime(2026, 2, 1),
            });
            schedule = await _contributions.SuggestLinesAsync(schedule.Id);

            // 6% + 12% of 100,000 and of 60,000.
            schedule.TotalAmount.ShouldBe(28800m);
            var lines = (await _lines.GetListAsync(new GetPensionContributionLineListInput { DocumentNo = schedule.No })).Items;

            // The employee's 6,000 is registered first, then 9,000 of the employer's 12,000.
            var line = lines.Single(l => l.MemberNo == first.No);
            line.EmployeeTaxExempt.ShouldBe(6000m);
            line.EmployerTaxExempt.ShouldBe(9000m);
            line.EmployerNonTaxExempt.ShouldBe(3000m);
            lines.Single(l => l.MemberNo == second.No).EmployerNonTaxExempt.ShouldBe(0m);

            await _contributions.ReleaseAsync(schedule.Id);
            await _contributions.RunPostingAsync(schedule.Id);

            var balance = await _members.GetBalanceAsync(first.Id, new DateTime(2026, 12, 31));
            balance.Registered.ShouldBe(15000m);
            balance.Unregistered.ShouldBe(3000m);

            // Voluntary contributions for the same month find the limit used up.
            var voluntary = await _contributions.CreateAsync(new CreateUpdatePensionContributionHeaderDto
            {
                SponsorNo = sponsor.No,
                PostingDate = new DateTime(2026, 2, 28),
                ContributionPeriod = new DateTime(2026, 2, 1),
                ContributionMode = PensionContributionMode.Arrears,
            });
            await _lines.CreateAsync(new CreateUpdatePensionContributionLineDto { DocumentNo = voluntary.No, MemberNo = first.No, EmployeeAvcTaxExempt = 5000m });
            await _contributions.SplitLinesAsync(voluntary.Id);

            var avc = (await _lines.GetListAsync(new GetPensionContributionLineListInput { DocumentNo = voluntary.No })).Items.Single();
            avc.EmployeeAvcTaxExempt.ShouldBe(0m);
            avc.EmployeeAvcNonTaxExempt.ShouldBe(5000m);

            // Posting kept the salary each member was contributed on, and joining is the first movement.
            var salaries = await _salaryEntries.GetListAsync(new GetMemberSalaryEntryListInput { MemberNo = first.No });
            salaries.Items.Single().Salary.ShouldBe(100000m);
            salaries.Items.Single().Period.ShouldBe(new DateTime(2026, 2, 1));

            var movements = await _statusEntries.GetListAsync(new GetMemberStatusEntryListInput { SchemeCode = "RELIEF" });
            movements.TotalCount.ShouldBe(2);
            movements.Items.ShouldAllBe(e => e.FromStatus == MemberStatus.None && e.ToStatus == MemberStatus.Active);
        });
    }

    [Fact]
    public void The_Tax_Relief_Limit_Can_Be_Shared_By_Contribution_Rate()
    {
        var amounts = new ContributionAmounts(6000m, 0m, 12000m, 0m);

        ContributionSplitter.Split(amounts, null, ExcessContributionAllocation.EmployeePriority, 6m, 12m).ShouldBe(new[] { 6000m, 0m, 0m, 0m, 12000m, 0m, 0m, 0m });
        ContributionSplitter.Split(amounts, 15000m, ExcessContributionAllocation.EmployeePriority, 6m, 12m).ShouldBe(new[] { 6000m, 0m, 0m, 0m, 9000m, 3000m, 0m, 0m });
        // A third of the limit is the employee's, two thirds the employer's.
        ContributionSplitter.Split(amounts, 15000m, ExcessContributionAllocation.ContributionRate, 6m, 12m).ShouldBe(new[] { 5000m, 1000m, 0m, 0m, 10000m, 2000m, 0m, 0m });
    }

    [Fact]
    public async Task A_Withdrawal_Vests_By_The_Sponsors_Scale_And_A_Transfer_In_Is_Owed_By_The_Other_Scheme()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var (sponsor, first, _) = await SetUpSchemeAsync("VEST");

            foreach (var (from, pct) in new[] { (0m, 0m), (5m, 40m), (10m, 80m) })
            {
                await _vesting.CreateAsync(new CreateUpdatePensionVestingScaleDto { SponsorNo = sponsor.No, FromServiceYears = from, EmployerVestedPct = pct });
            }

            var setup = await _setup.GetAsync();
            setup.TransfersInAccountNo = "2330";
            await _setup.UpdateAsync(setup);

            var transfer = await PostFundAsync(sponsor, first, 1000000m, 2000000m, PensionContributionMode.TransferIn);
            var glEntries = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == transfer.No);
            glEntries.Single(e => e.GLAccountNo == "2330").Amount.ShouldBe(3000000m);
            glEntries.ShouldNotContain(e => e.GLAccountNo == "1310");

            // Ten and a half years of service: the 10-year band vests 80% of the employer's money.
            var withdrawal = await _exits.CreateAsync(new CreateUpdateMemberExitDto { MemberNo = first.No, ReasonCode = "WITHDRAWAL", ExitDate = new DateTime(2026, 6, 30) });
            withdrawal.EmployerPayable.ShouldBe(1600000m);
            withdrawal.DeferredAmount.ShouldBe(400000m);

            // Retirement does not use the scale: it vests in full.
            var retirement = await _exits.CreateAsync(new CreateUpdateMemberExitDto
            {
                MemberNo = first.No,
                ReasonCode = "RETIREMENT",
                ExitDate = new DateTime(2026, 6, 30),
                WithdrawalType = MemberWithdrawalType.Projection,
            });
            retirement.EmployerPayable.ShouldBe(2000000m);

            await _exits.ApproveAsync(withdrawal.Id);
            await _exits.RunPostingAsync(withdrawal.Id, new PostMemberExitInput());

            var movement = (await _statusEntries.GetListAsync(new GetMemberStatusEntryListInput { MemberNo = first.No })).Items.First();
            movement.FromStatus.ShouldBe(MemberStatus.Active);
            movement.ToStatus.ShouldBe(MemberStatus.Deferred);
            movement.DocumentNo.ShouldBe(withdrawal.No);

            var report = await GetRequiredService<IStandardReportAppService>().RunAsync(new RunStandardReportInput
            {
                Code = "MembershipMovement",
                SchemeCode = "VEST",
                FromDate = new DateTime(2026, 6, 1),
                ToDate = new DateTime(2026, 6, 30),
            });
            report.Rows.First().Values["toStatus"].ShouldBe("Deferred");
        });
    }

    [Fact]
    public async Task Pensions_Are_Raised_Suspended_And_Reinstated_With_Arrears_And_Taxed_Through_Bands()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var pensioners = GetRequiredService<IPensionerAppService>();
            var payrolls = GetRequiredService<IPensionPayrollAppService>();
            var increments = GetRequiredService<IPensionIncrementAppService>();
            var changes = GetRequiredService<IPensionerChangeEntryAppService>();

            await _schemes.CreateAsync(new CreateUpdatePensionSchemeDto { Code = "SIGMA", Description = "Sigma Pension Scheme" });
            await GetRequiredService<ILumpsumTaxTableAppService>().CreateAsync(new CreateUpdateLumpsumTaxTableDto { Code = "PAYE", Description = "Pay As You Earn" });
            var bandService = GetRequiredService<ILumpsumTaxBandAppService>();
            foreach (var (lower, upper, rate) in new[] { (0m, 24000m, 10m), (24000m, 32333m, 25m), (32333m, 0m, 30m) })
            {
                await bandService.CreateAsync(new CreateUpdateLumpsumTaxBandDto { TaxTableCode = "PAYE", LowerLimit = lower, UpperLimit = upper, RatePct = rate });
            }

            var njeri = await pensioners.CreateAsync(new CreateUpdatePensionerDto { SchemeCode = "SIGMA", Name = "Njeri Kamau", MonthlyPension = 40000m, StartDate = new DateTime(2025, 7, 1) });
            var barasa = await pensioners.CreateAsync(new CreateUpdatePensionerDto { SchemeCode = "SIGMA", Name = "Barasa Wafula", MonthlyPension = 20000m, StartDate = new DateTime(2025, 7, 1) });

            async Task<PensionPayrollHeaderDto> RunPayrollAsync(DateTime period)
            {
                var payroll = await payrolls.CreateAsync(new CreateUpdatePensionPayrollHeaderDto
                {
                    SchemeCode = "SIGMA",
                    PayPeriod = period,
                    PostingDate = period.AddDays(26),
                    TaxTableCode = "PAYE",
                    PersonalRelief = 2400m,
                });
                await payrolls.SuggestLinesAsync(payroll.Id);
                await payrolls.ReleaseAsync(payroll.Id);
                return await payrolls.RunPostingAsync(payroll.Id);
            }

            // 40,000: 2,400 + 2,083.25 + 2,300.10 less relief = 4,383.35. 20,000: 2,000 less relief = nothing.
            var january = await RunPayrollAsync(new DateTime(2026, 1, 1));
            january.TotalGross.ShouldBe(60000m);
            january.TotalTax.ShouldBe(4383.35m);

            // A 10% increment from January, with no pension below 25,000: January was paid at the old rate.
            var increment = await increments.CreateAsync(new CreateUpdatePensionIncrementDto
            {
                SchemeCode = "SIGMA",
                EffectiveDate = new DateTime(2026, 1, 1),
                IncrementPct = 10m,
                MinimumMonthlyPension = 25000m,
            });
            increment = await increments.ApplyAsync(increment.Id);
            increment.Status.ShouldBe(PensionIncrementStatus.Applied);
            increment.NoOfPensioners.ShouldBe(2);
            increment.TotalMonthlyIncrease.ShouldBe(9000m);
            increment.TotalArrears.ShouldBe(9000m);

            njeri = await pensioners.GetAsync(njeri.Id);
            njeri.MonthlyPension.ShouldBe(44000m);
            njeri.ArrearsAmount.ShouldBe(4000m);
            (await pensioners.GetAsync(barasa.Id)).MonthlyPension.ShouldBe(25000m);

            await Should.ThrowAsync<BusinessException>(() => increments.ApplyAsync(increment.Id));

            await pensioners.SuspendAsync(barasa.Id, new PensionerActionInput { Date = new DateTime(2026, 2, 10), Reason = "Bank account closed" });

            // February pays Njeri the new pension and the January arrears, taxed as January's own.
            var february = await RunPayrollAsync(new DateTime(2026, 2, 1));
            february.NoOfPensioners.ShouldBe(1);
            february.TotalGross.ShouldBe(48000m);
            february.TotalTax.ShouldBe(5583.35m);
            (await pensioners.GetAsync(njeri.Id)).ArrearsAmount.ShouldBe(0m);

            var gl = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == february.No);
            gl.Sum(e => e.Amount).ShouldBe(0m);
            gl.Single(e => e.GLAccountNo == "2630").Amount.ShouldBe(-5583.35m);

            // Reinstated in April: February and March were missed, on top of the increment arrears.
            barasa = await pensioners.ReinstateAsync(barasa.Id, new PensionerActionInput { Date = new DateTime(2026, 4, 15) });
            barasa.Status.ShouldBe(PensionerStatus.Active);
            barasa.ArrearsAmount.ShouldBe(55000m);
            barasa.ArrearsMonths.ShouldBe(3);

            var notSuspended = await Should.ThrowAsync<BusinessException>(() => pensioners.ReinstateAsync(barasa.Id, new PensionerActionInput()));
            notSuspended.Code.ShouldBe(ErpErrorCodes.Pensions.PensionerNotSuspended);

            // A year after the pensions started, no life certificate has come in.
            var overdue = await pensioners.SuspendOverdueAsync(new SuspendOverduePensionersInput { SchemeCode = "SIGMA", AsOfDate = new DateTime(2026, 8, 1) });
            overdue.NoOfPensioners.ShouldBe(2);

            njeri = await pensioners.RecordLifeCertificateAsync(njeri.Id, new PensionerActionInput { Date = new DateTime(2026, 8, 1) });
            njeri.Status.ShouldBe(PensionerStatus.Active);
            njeri.LifeCertificateDueDate.ShouldBe(new DateTime(2027, 8, 1));
            // March to July were not paid.
            njeri.ArrearsAmount.ShouldBe(220000m);

            var history = await changes.GetListAsync(new GetPensionerChangeEntryListInput { PensionerNo = njeri.No });
            history.Items.Select(h => h.ChangeType).ShouldBe(
                new[] { PensionerChangeType.Increment, PensionerChangeType.ArrearsPaid, PensionerChangeType.Suspension, PensionerChangeType.LifeCertificate, PensionerChangeType.Reinstatement },
                ignoreOrder: true
            );

            var reports = GetRequiredService<IStandardReportAppService>();
            var roll = await reports.RunAsync(new RunStandardReportInput { Code = "PensionersMasterRoll", SchemeCode = "SIGMA", ToDate = new DateTime(2026, 8, 31) });
            Convert.ToDecimal(roll.Rows.Last().Values["arrears"]).ShouldBe(275000m);

            var slips = await reports.RunAsync(new RunStandardReportInput
            {
                Code = "PensionerPayslip",
                SchemeCode = "SIGMA",
                FromDate = new DateTime(2026, 1, 1),
                ToDate = new DateTime(2026, 12, 31),
            });
            Convert.ToDecimal(slips.Rows.Last().Values["gross"]).ShouldBe(108000m);
        });
    }
}
