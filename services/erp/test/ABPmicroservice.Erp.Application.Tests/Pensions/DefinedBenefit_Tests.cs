using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Finance;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Pensions;

public class DefinedBenefit_Tests : ErpApplicationTestBase
{
    private readonly IPensionSchemeAppService _schemes;
    private readonly IPensionSponsorAppService _sponsors;
    private readonly IPensionMemberAppService _members;
    private readonly IPensionBenefitCalculationAppService _calculations;

    public DefinedBenefit_Tests()
    {
        _schemes = GetRequiredService<IPensionSchemeAppService>();
        _sponsors = GetRequiredService<IPensionSponsorAppService>();
        _members = GetRequiredService<IPensionMemberAppService>();
        _calculations = GetRequiredService<IPensionBenefitCalculationAppService>();
    }

    /// <summary>A scheme paying 2% of final salary a year, a quarter commutable at 20, cut 3% a year for retiring early.</summary>
    private async Task<PensionSponsorDto> SetUpSchemeAsync(string code, PensionPlanType planType = PensionPlanType.DefinedBenefit)
    {
        await _schemes.CreateAsync(new CreateUpdatePensionSchemeDto
        {
            Code = code,
            Description = code + " Staff Pension Scheme",
            PlanType = planType,
            NormalRetirementAge = 60,
            MinimumRetirementAge = 50,
            AccrualRatePct = 2m,
            MaxCommutationPct = 25m,
            CommutationFactor = 20m,
            EarlyRetirementReductionPct = 3m,
        });

        return await _sponsors.CreateAsync(new CreateUpdatePensionSponsorDto { Name = code + " Employer Ltd", SchemeCode = code });
    }

    private Task<PensionMemberDto> JoinAsync(PensionSponsorDto sponsor, DateTime dateOfBirth) =>
        _members.CreateAsync(new CreateUpdatePensionMemberDto
        {
            SponsorNo = sponsor.No,
            FirstName = "Lucy",
            LastName = "Achieng",
            DateOfBirth = dateOfBirth,
            JoinSchemeDate = new DateTime(2001, 1, 1),
            CurrentSalary = 100000m,
        });

    [Fact]
    public async Task A_Member_Retires_On_The_Formula_Pension_With_A_Commuted_Lump_Sum()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var sponsor = await SetUpSchemeAsync("DELTA");
            var member = await JoinAsync(sponsor, new DateTime(1966, 1, 1));

            var calculation = await _calculations.CreateAsync(new CreateUpdatePensionBenefitCalculationDto
            {
                MemberNo = member.No,
                CalculationDate = new DateTime(2026, 1, 1),
                RetirementDate = new DateTime(2026, 1, 1),
                CommutationPct = 25m,
            });

            calculation.No.ShouldBe("DBC-00001");
            calculation.AgeAtRetirement.ShouldBe(60m);
            calculation.PensionableServiceYears.ShouldBe(25m);
            calculation.FinalPensionableSalary.ShouldBe(1200000m);

            // 1,200,000 x 25 years x 2%; a quarter of it given up at 20 times.
            calculation.FullAnnualPension.ShouldBe(600000m);
            calculation.EarlyReductionPct.ShouldBe(0m);
            calculation.CommutedAnnualPension.ShouldBe(150000m);
            calculation.LumpSum.ShouldBe(3000000m);
            calculation.AnnualPension.ShouldBe(450000m);
            calculation.MonthlyPension.ShouldBe(37500m);

            calculation = await _calculations.ApproveAsync(calculation.Id);
            calculation.Status.ShouldBe(BenefitCalculationStatus.Approved);
            calculation.PensionerNo.ShouldBe("PN0010");

            var pensioner = (await GetRequiredService<IRepository<Pensioner, Guid>>().GetListAsync(p => p.No == calculation.PensionerNo)).Single();
            pensioner.MonthlyPension.ShouldBe(37500m);
            pensioner.MemberNo.ShouldBe(member.No);
            pensioner.StartDate.ShouldBe(new DateTime(2026, 2, 1));

            var voucherLine = (await GetRequiredService<IRepository<PaymentVoucherLine, Guid>>().GetListAsync(l => l.DocumentNo == calculation.PaymentVoucherNo)).Single();
            voucherLine.AccountType.ShouldBe(GenJournalAccountType.GLAccount);
            voucherLine.AccountNo.ShouldBe("8620");
            voucherLine.Amount.ShouldBe(3000000m);

            (await _members.GetAsync(member.Id)).Status.ShouldBe(MemberStatus.Inactive);
            (await Should.ThrowAsync<BusinessException>(() => _calculations.ApproveAsync(calculation.Id))).Code.ShouldBe(ErpErrorCodes.Pensions.DocumentNotOpen);
        });
    }

    [Fact]
    public async Task Retiring_Early_Cuts_The_Pension_And_The_Scheme_Limits_Apply()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var sponsor = await SetUpSchemeAsync("EPSILON");
            var member = await JoinAsync(sponsor, new DateTime(1971, 1, 1));

            // Five years early at 3% a year.
            var early = await _calculations.CreateAsync(new CreateUpdatePensionBenefitCalculationDto { MemberNo = member.No, RetirementDate = new DateTime(2026, 1, 1) });
            early.AgeAtRetirement.ShouldBe(55m);
            early.EarlyReductionPct.ShouldBe(15m);
            early.ReducedAnnualPension.ShouldBe(510000m);
            early.MonthlyPension.ShouldBe(42500m);

            var tooMuch = await Should.ThrowAsync<BusinessException>(() => _calculations.UpdateAsync(early.Id, new CreateUpdatePensionBenefitCalculationDto
            {
                MemberNo = member.No, RetirementDate = new DateTime(2026, 1, 1), CommutationPct = 50m,
            }));
            tooMuch.Code.ShouldBe(ErpErrorCodes.Pensions.CommutationAboveMaximum);

            var tooYoung = await Should.ThrowAsync<BusinessException>(() => _calculations.CreateAsync(new CreateUpdatePensionBenefitCalculationDto
            {
                MemberNo = member.No, RetirementDate = new DateTime(2016, 1, 1),
            }));
            tooYoung.Code.ShouldBe(ErpErrorCodes.Pensions.RetirementTooEarly);

            var contribution = await SetUpSchemeAsync("ZETA", PensionPlanType.DefinedContribution);
            var saver = await JoinAsync(contribution, new DateTime(1966, 1, 1));
            var notDefinedBenefit = await Should.ThrowAsync<BusinessException>(() => _calculations.CreateAsync(new CreateUpdatePensionBenefitCalculationDto { MemberNo = saver.No }));
            notDefinedBenefit.Code.ShouldBe(ErpErrorCodes.Pensions.NotDefinedBenefitScheme);
        });
    }
}
