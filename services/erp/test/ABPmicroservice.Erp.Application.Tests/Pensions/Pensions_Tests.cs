using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Reporting;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Pensions;

public class Pensions_Tests : ErpApplicationTestBase
{
    private readonly IPensionSchemeAppService _schemes;
    private readonly IPensionSponsorAppService _sponsors;
    private readonly IPensionMemberAppService _members;
    private readonly IPensionContributionAppService _contributions;
    private readonly IPensionContributionLineAppService _lines;
    private readonly IPensionInterestRateAppService _interest;
    private readonly IMemberExitAppService _exits;
    private readonly IMemberLedgerEntryAppService _ledger;

    public Pensions_Tests()
    {
        _schemes = GetRequiredService<IPensionSchemeAppService>();
        _sponsors = GetRequiredService<IPensionSponsorAppService>();
        _members = GetRequiredService<IPensionMemberAppService>();
        _contributions = GetRequiredService<IPensionContributionAppService>();
        _lines = GetRequiredService<IPensionContributionLineAppService>();
        _interest = GetRequiredService<IPensionInterestRateAppService>();
        _exits = GetRequiredService<IMemberExitAppService>();
        _ledger = GetRequiredService<IMemberLedgerEntryAppService>();
    }

    /// <summary>A scheme with one sponsor at 5% + 10% and two members on 100,000 and 60,000 a month.</summary>
    private async Task<(PensionSponsorDto Sponsor, PensionMemberDto First, PensionMemberDto Second)> SetUpSchemeAsync(string scheme)
    {
        await _schemes.CreateAsync(new CreateUpdatePensionSchemeDto { Code = scheme, Description = scheme + " Staff Pension Scheme" });

        var sponsor = await _sponsors.CreateAsync(new CreateUpdatePensionSponsorDto
        {
            Name = scheme + " Employer Ltd",
            SchemeCode = scheme,
            EmployeeRatePct = 5m,
            EmployerRatePct = 10m,
        });

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

    private async Task<PensionContributionHeaderDto> PostScheduleAsync(PensionSponsorDto sponsor, DateTime period)
    {
        var schedule = await _contributions.CreateAsync(new CreateUpdatePensionContributionHeaderDto
        {
            SponsorNo = sponsor.No,
            PostingDate = period.AddDays(27),
            ContributionPeriod = period,
        });

        await _contributions.SuggestLinesAsync(schedule.Id);
        await _contributions.ReleaseAsync(schedule.Id);
        return await _contributions.RunPostingAsync(schedule.Id);
    }

    [Fact]
    public async Task A_Scheme_Is_A_Value_Of_The_Scheme_Dimension()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var scheme = await _schemes.CreateAsync(new CreateUpdatePensionSchemeDto { Code = "alpha", Description = "Alpha Staff Pension Scheme", NormalRetirementAge = 60 });
            scheme.Code.ShouldBe("ALPHA");

            var values = await GetRequiredService<IRepository<DimensionValue, Guid>>().GetListAsync(v => v.DimensionCode == "SCHEME");
            values.Single().Code.ShouldBe("ALPHA");
            values.Single().Name.ShouldBe("Alpha Staff Pension Scheme");

            var sponsor = await _sponsors.CreateAsync(new CreateUpdatePensionSponsorDto { Name = "Alpha Ltd", SchemeCode = "ALPHA" });
            sponsor.No.ShouldBe("SP0010");

            var member = await _members.CreateAsync(new CreateUpdatePensionMemberDto
            {
                SponsorNo = sponsor.No,
                FirstName = "Amina",
                LastName = "Hassan",
                DateOfBirth = new DateTime(1980, 3, 10),
            });

            member.No.ShouldBe("M000010");
            member.SchemeCode.ShouldBe("ALPHA");
            member.FullName.ShouldBe("Amina Hassan");
            member.ExpectedRetirementDate.ShouldBe(new DateTime(2040, 3, 10));

            var inUse = await Should.ThrowAsync<BusinessException>(() => _schemes.DeleteAsync(scheme.Id));
            inUse.Code.ShouldBe(ErpErrorCodes.Pensions.SchemeInUse);
        });
    }

    [Fact]
    public async Task Posting_A_Schedule_Credits_Members_And_The_GL_Under_The_Scheme_Dimension()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var (sponsor, first, second) = await SetUpSchemeAsync("BETA");

            var schedule = await _contributions.CreateAsync(new CreateUpdatePensionContributionHeaderDto
            {
                SponsorNo = sponsor.No,
                PostingDate = new DateTime(2026, 1, 28),
                ContributionPeriod = new DateTime(2026, 1, 15),
            });
            schedule.No.ShouldBe("PC-00001");
            schedule.ContributionPeriod.ShouldBe(new DateTime(2026, 1, 1));

            schedule = await _contributions.SuggestLinesAsync(schedule.Id);
            schedule.NoOfMembers.ShouldBe(2);
            // 5% + 10% of 100,000 and of 60,000.
            schedule.TotalAmount.ShouldBe(24000m);

            // A voluntary contribution on top, for the first member only.
            var lines = await _lines.GetListAsync(new GetPensionContributionLineListInput { DocumentNo = schedule.No });
            var line = lines.Items.Single(l => l.MemberNo == first.No);
            await _lines.UpdateAsync(line.Id, new CreateUpdatePensionContributionLineDto
            {
                DocumentNo = schedule.No,
                MemberNo = first.No,
                BasicSalary = line.BasicSalary,
                EmployeeTaxExempt = line.EmployeeTaxExempt,
                EmployerTaxExempt = line.EmployerTaxExempt,
                EmployeeAvcNonTaxExempt = 2000m,
            });

            var notReleased = await Should.ThrowAsync<BusinessException>(() => _contributions.RunPostingAsync(schedule.Id));
            notReleased.Code.ShouldBe(ErpErrorCodes.Pensions.DocumentNotReleased);

            await _contributions.ReleaseAsync(schedule.Id);
            var posted = await _contributions.RunPostingAsync(schedule.Id);
            posted.Status.ShouldBe(PensionDocumentStatus.Posted);
            posted.TotalAmount.ShouldBe(26000m);

            var balance = await _members.GetBalanceAsync(first.Id, new DateTime(2026, 12, 31));
            balance.Total.ShouldBe(17000m);
            balance.Employee.ShouldBe(7000m);
            balance.Employer.ShouldBe(10000m);
            balance.Registered.ShouldBe(15000m);
            balance.Unregistered.ShouldBe(2000m);
            (await _members.GetBalanceAsync(second.Id, new DateTime(2026, 12, 31))).Total.ShouldBe(9000m);

            // The G/L balances, and both sides carry the scheme's dimension set.
            var schemeManager = GetRequiredService<PensionSchemeManager>();
            var dimensionSetId = await schemeManager.GetDimensionSetIdAsync("BETA");
            var glEntries = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == schedule.No);

            glEntries.Count.ShouldBe(2);
            glEntries.Sum(e => (double)e.Amount).ShouldBe(0d);
            glEntries.Single(e => e.GLAccountNo == "2610").Amount.ShouldBe(-26000m);
            glEntries.Single(e => e.GLAccountNo == "1310").Amount.ShouldBe(26000m);
            glEntries.ShouldAllBe(e => e.DimensionSetId == dimensionSetId);

            var set = await GetRequiredService<IRepository<DimensionSetEntry, Guid>>().GetListAsync(e => e.DimensionSetId == dimensionSetId);
            set.Single().DimensionValueCode.ShouldBe("BETA");

            // A posted schedule cannot be changed, and the period cannot be posted twice.
            var closed = await Should.ThrowAsync<BusinessException>(() => _contributions.DeleteAsync(schedule.Id));
            closed.Code.ShouldBe(ErpErrorCodes.Pensions.DocumentNotOpen);

            var again = await _contributions.CreateAsync(new CreateUpdatePensionContributionHeaderDto
            {
                SponsorNo = sponsor.No,
                PostingDate = new DateTime(2026, 1, 30),
                ContributionPeriod = new DateTime(2026, 1, 1),
            });
            await _contributions.SuggestLinesAsync(again.Id);
            var duplicate = await Should.ThrowAsync<BusinessException>(() => _contributions.ReleaseAsync(again.Id));
            duplicate.Code.ShouldBe(ErpErrorCodes.Pensions.PeriodAlreadyPosted);
        });
    }

    [Fact]
    public async Task Two_Schemes_Share_A_Company_And_Stay_Apart()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var gamma = await SetUpSchemeAsync("GAMMA");
            var delta = await SetUpSchemeAsync("DELTA");

            await PostScheduleAsync(gamma.Sponsor, new DateTime(2026, 2, 1));
            await PostScheduleAsync(delta.Sponsor, new DateTime(2026, 2, 1));
            await PostScheduleAsync(delta.Sponsor, new DateTime(2026, 3, 1));

            (await _ledger.GetListAsync(new GetMemberLedgerEntryListInput { SchemeCode = "gamma" })).TotalCount.ShouldBe(4);
            (await _ledger.GetListAsync(new GetMemberLedgerEntryListInput { SchemeCode = "DELTA" })).TotalCount.ShouldBe(8);

            // A member of one scheme cannot be put on a schedule of the other.
            var schedule = await _contributions.CreateAsync(new CreateUpdatePensionContributionHeaderDto
            {
                SponsorNo = gamma.Sponsor.No,
                PostingDate = new DateTime(2026, 4, 28),
                ContributionPeriod = new DateTime(2026, 4, 1),
            });
            var stranger = await Should.ThrowAsync<BusinessException>(() => _lines.CreateAsync(new CreateUpdatePensionContributionLineDto
            {
                DocumentNo = schedule.No,
                MemberNo = delta.First.No,
                EmployeeTaxExempt = 100m,
            }));
            stranger.Code.ShouldBe(ErpErrorCodes.Pensions.MemberNotInScheme);

            // One report across the company, or cut down to a scheme.
            var reports = GetRequiredService<IStandardReportAppService>();
            var all = await reports.RunAsync(new RunStandardReportInput { Code = "MemberBalances", ToDate = new DateTime(2026, 12, 31) });
            var one = await reports.RunAsync(new RunStandardReportInput { Code = "MemberBalances", ToDate = new DateTime(2026, 12, 31), SchemeCode = "gamma" });

            Convert.ToDecimal(all.Rows.Last().Values["total"]).ShouldBe(72000m);
            Convert.ToDecimal(one.Rows.Last().Values["total"]).ShouldBe(24000m);
            one.Rows.Count.ShouldBe(3);
        });
    }

    [Fact]
    public async Task Declared_Interest_Is_Allocated_Once_And_Unregistered_Interest_Is_Taxed()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var (sponsor, first, _) = await SetUpSchemeAsync("EPSILON");

            // An opening fund posted in December, then interest declared for the following year.
            var schedule = await _contributions.CreateAsync(new CreateUpdatePensionContributionHeaderDto
            {
                SponsorNo = sponsor.No,
                PostingDate = new DateTime(2025, 12, 31),
                ContributionPeriod = new DateTime(2025, 12, 1),
            });
            await _lines.CreateAsync(new CreateUpdatePensionContributionLineDto
            {
                DocumentNo = schedule.No,
                MemberNo = first.No,
                EmployeeTaxExempt = 100000m,
                EmployeeNonTaxExempt = 50000m,
            });
            await _contributions.ReleaseAsync(schedule.Id);
            await _contributions.RunPostingAsync(schedule.Id);

            var rate = await _interest.CreateAsync(new CreateUpdatePensionInterestRateDto
            {
                SchemeCode = "EPSILON",
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                RegisteredRatePct = 10m,
                UnregisteredRatePct = 8m,
                TaxRatePct = 15m,
            });

            var preview = await _interest.GetPreviewAsync(rate.Id);
            preview.NoOfMembers.ShouldBe(1);
            // A full year compounds to exactly the annual rate: 10% of 100,000 and 8% of 50,000.
            preview.TotalInterest.ShouldBe(14000m);
            // 15% of the 4,000 earned on unregistered money.
            preview.TotalTax.ShouldBe(600m);

            var allocated = await _interest.AllocateAsync(rate.Id, new AllocateInterestInput());
            allocated.Posted.ShouldBeTrue();
            allocated.PostedDocumentNo.ShouldBe("PI-00001");

            var balance = await _members.GetBalanceAsync(first.Id, new DateTime(2026, 12, 31));
            balance.Registered.ShouldBe(110000m);
            balance.Unregistered.ShouldBe(53400m);

            var glEntries = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == "PI-00001");
            glEntries.Sum(e => (double)e.Amount).ShouldBe(0d);
            glEntries.Single(e => e.GLAccountNo == "8610").Amount.ShouldBe(14000m);
            glEntries.Single(e => e.GLAccountNo == "2610").Amount.ShouldBe(-13400m);
            glEntries.Single(e => e.GLAccountNo == "2630").Amount.ShouldBe(-600m);

            var twice = await Should.ThrowAsync<BusinessException>(() => _interest.AllocateAsync(rate.Id, new AllocateInterestInput()));
            twice.Code.ShouldBe(ErpErrorCodes.Pensions.InterestAlreadyAllocated);

            var overlap = await Should.ThrowAsync<BusinessException>(() => _interest.CreateAsync(new CreateUpdatePensionInterestRateDto
            {
                SchemeCode = "EPSILON",
                StartDate = new DateTime(2026, 7, 1),
                EndDate = new DateTime(2027, 6, 30),
                RegisteredRatePct = 9m,
            }));
            overlap.Code.ShouldBe(ErpErrorCodes.Pensions.InterestPeriodOverlaps);
        });
    }

    [Fact]
    public async Task A_Withdrawal_Pays_The_Vested_Part_Taxes_It_And_Defers_The_Rest()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var (sponsor, first, _) = await SetUpSchemeAsync("ZETA");

            // 1,000,000 of the member's own money and 2,000,000 from the employer, all registered.
            var schedule = await _contributions.CreateAsync(new CreateUpdatePensionContributionHeaderDto
            {
                SponsorNo = sponsor.No,
                PostingDate = new DateTime(2026, 1, 31),
                ContributionPeriod = new DateTime(2026, 1, 1),
            });
            await _lines.CreateAsync(new CreateUpdatePensionContributionLineDto
            {
                DocumentNo = schedule.No,
                MemberNo = first.No,
                EmployeeTaxExempt = 1000000m,
                EmployerTaxExempt = 2000000m,
            });
            await _contributions.ReleaseAsync(schedule.Id);
            await _contributions.RunPostingAsync(schedule.Id);

            // The seeded WITHDRAWAL reason vests half the employer's money and taxes the lump sum.
            var exit = await _exits.CreateAsync(new CreateUpdateMemberExitDto
            {
                MemberNo = first.No,
                ReasonCode = "withdrawal",
                ExitDate = new DateTime(2026, 6, 30),
            });

            exit.No.ShouldBe("PX-00001");
            exit.EmployeeBalance.ShouldBe(1000000m);
            exit.EmployerBalance.ShouldBe(2000000m);
            exit.EmployeePayable.ShouldBe(1000000m);
            exit.EmployerPayable.ShouldBe(1000000m);
            exit.DeferredAmount.ShouldBe(1000000m);
            exit.GrossLumpsum.ShouldBe(2000000m);
            // Joined 1 January 2016: ten and a half years at 60,000 a year hits the 600,000 cap.
            exit.TaxFreeAmount.ShouldBe(600000m);
            exit.TaxableAmount.ShouldBe(1400000m);
            // 400,000 at 10%, 400,000 at 15%, 400,000 at 20% and 200,000 at 25%.
            exit.TaxOnLumpsum.ShouldBe(230000m);
            exit.NetPayable.ShouldBe(1770000m);

            var unapproved = await Should.ThrowAsync<BusinessException>(() => _exits.RunPostingAsync(exit.Id, new PostMemberExitInput()));
            unapproved.Code.ShouldBe(ErpErrorCodes.Pensions.DocumentNotReleased);

            await _exits.ApproveAsync(exit.Id);
            var posted = await _exits.RunPostingAsync(exit.Id, new PostMemberExitInput());
            posted.Status.ShouldBe(MemberExitStatus.Posted);

            var balance = await _members.GetBalanceAsync(first.Id, new DateTime(2026, 12, 31));
            balance.Employee.ShouldBe(0m);
            balance.Employer.ShouldBe(1000000m);

            var member = await _members.GetAsync(first.Id);
            member.Status.ShouldBe(MemberStatus.Deferred);
            member.ExitDate.ShouldBe(new DateTime(2026, 6, 30));

            var glEntries = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == exit.No);
            glEntries.Sum(e => (double)e.Amount).ShouldBe(0d);
            glEntries.Single(e => e.GLAccountNo == "2610").Amount.ShouldBe(2000000m);
            glEntries.Single(e => e.GLAccountNo == "2620").Amount.ShouldBe(-1770000m);
            glEntries.Single(e => e.GLAccountNo == "2630").Amount.ShouldBe(-230000m);

            // The net benefit waits in benefits payable for a payment voucher, raised once.
            posted = await _exits.RaisePaymentVoucherAsync(exit.Id);
            posted.PaymentVoucherNo.ShouldBe("PV-00001");

            var voucherLine = (await GetRequiredService<IRepository<PaymentVoucherLine, Guid>>().GetListAsync(l => l.DocumentNo == posted.PaymentVoucherNo)).Single();
            voucherLine.AccountNo.ShouldBe("2620");
            voucherLine.Amount.ShouldBe(1770000m);

            var twice = await Should.ThrowAsync<BusinessException>(() => _exits.RaisePaymentVoucherAsync(exit.Id));
            twice.Code.ShouldBe(ErpErrorCodes.Pensions.VoucherAlreadyRaised);

            // A member who has left is not contributed for again.
            var next = await _contributions.CreateAsync(new CreateUpdatePensionContributionHeaderDto
            {
                SponsorNo = sponsor.No,
                PostingDate = new DateTime(2026, 7, 31),
                ContributionPeriod = new DateTime(2026, 7, 1),
            });
            (await _contributions.SuggestLinesAsync(next.Id)).NoOfMembers.ShouldBe(1);
        });
    }

    [Fact]
    public void Interest_Grows_By_The_Scheme_Calculation_Mode()
    {
        var january = new DateTime(2026, 1, 1);

        PensionInterestEngine.GrowthFactor(12m, january, new DateTime(2026, 12, 31), InterestCalculationMode.CompoundMonthly, 365).ShouldBe(0.12m, 0.0000001m);
        PensionInterestEngine.GrowthFactor(12m, january, new DateTime(2026, 12, 31), InterestCalculationMode.Simple, 365).ShouldBe(0.12m, 0.0000001m);
        // Half a year compounds to less than half the annual rate.
        PensionInterestEngine.GrowthFactor(12m, january, new DateTime(2026, 6, 30), InterestCalculationMode.CompoundMonthly, 365).ShouldBe(0.0583005m, 0.000001m);
        PensionInterestEngine.GrowthFactor(12m, new DateTime(2026, 7, 1), new DateTime(2026, 6, 30), InterestCalculationMode.CompoundMonthly, 365).ShouldBe(0m);
    }

    [Fact]
    public void Lump_Sum_Tax_Runs_Through_Progressive_Bands()
    {
        LumpsumTaxBand Band(decimal lower, decimal upper, decimal rate)
        {
            var band = new LumpsumTaxBand(Guid.NewGuid(), "T", lower);
            band.Set(upper, rate);
            return band;
        }

        var bands = new[] { Band(0m, 400000m, 10m), Band(400000m, 800000m, 15m), Band(800000m, 0m, 30m) };

        LumpsumTaxBand.TaxOn(0m, bands).ShouldBe(0m);
        LumpsumTaxBand.TaxOn(300000m, bands).ShouldBe(30000m);
        LumpsumTaxBand.TaxOn(500000m, bands).ShouldBe(55000m);
        LumpsumTaxBand.TaxOn(1000000m, bands).ShouldBe(160000m);
    }
}
