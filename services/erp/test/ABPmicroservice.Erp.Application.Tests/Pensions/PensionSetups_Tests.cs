using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Pensions;

public class PensionSetups_Tests : ErpApplicationTestBase
{
    private readonly IPensionSchemeAppService _schemes;
    private readonly IPensionSponsorAppService _sponsors;
    private readonly IPensionMemberAppService _members;
    private readonly IPensionContributionAppService _contributions;
    private readonly IPensionContributionLineAppService _lines;
    private readonly IPensionerAppService _pensioners;
    private readonly IPensionPayrollAppService _payrolls;
    private readonly IPensionSetupAppService _setup;

    public PensionSetups_Tests()
    {
        _schemes = GetRequiredService<IPensionSchemeAppService>();
        _sponsors = GetRequiredService<IPensionSponsorAppService>();
        _members = GetRequiredService<IPensionMemberAppService>();
        _contributions = GetRequiredService<IPensionContributionAppService>();
        _lines = GetRequiredService<IPensionContributionLineAppService>();
        _pensioners = GetRequiredService<IPensionerAppService>();
        _payrolls = GetRequiredService<IPensionPayrollAppService>();
        _setup = GetRequiredService<IPensionSetupAppService>();
    }

    /// <summary>Posts one month of a member's contributions on the salary given.</summary>
    private async Task<PensionContributionHeaderDto> PostMonthAsync(PensionSponsorDto sponsor, PensionMemberDto member, DateTime period, decimal salary)
    {
        var schedule = await _contributions.CreateAsync(new CreateUpdatePensionContributionHeaderDto
        {
            SponsorNo = sponsor.No,
            PostingDate = period.AddDays(27),
            ContributionPeriod = period,
        });
        await _lines.CreateAsync(new CreateUpdatePensionContributionLineDto
        {
            DocumentNo = schedule.No,
            MemberNo = member.No,
            BasicSalary = salary,
            EmployeeTaxExempt = salary * 0.05m,
            EmployerTaxExempt = salary * 0.10m,
        });
        await _contributions.ReleaseAsync(schedule.Id);
        return await _contributions.RunPostingAsync(schedule.Id);
    }

    [Fact]
    public async Task A_Payroll_Pays_Earnings_And_Takes_Deductions_And_Posts_Them_To_Their_Accounts()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var items = GetRequiredService<IPensionerPayItemAppService>();
            var assignments = GetRequiredService<IPensionerPayItemAssignmentAppService>();
            var lineItems = GetRequiredService<IPensionPayrollLineItemAppService>();

            await _schemes.CreateAsync(new CreateUpdatePensionSchemeDto { Code = "OMEGA", Description = "Omega Pension Scheme" });

            await GetRequiredService<IPensionBankAppService>().CreateAsync(new CreateUpdatePensionBankDto { Code = "KCB", Description = "Kenya Commercial Bank" });
            await GetRequiredService<IPensionBankBranchAppService>().CreateAsync(new CreateUpdatePensionBankBranchDto { BankCode = "KCB", BranchCode = "001", Name = "Moi Avenue" });

            var wrongBranch = await Should.ThrowAsync<BusinessException>(() => _pensioners.CreateAsync(new CreateUpdatePensionerDto
            {
                SchemeCode = "OMEGA",
                Name = "Achieng Otieno",
                MonthlyPension = 40000m,
                StartDate = new DateTime(2025, 1, 1),
                BankCode = "KCB",
                BankBranchCode = "999",
            }));
            wrongBranch.Code.ShouldBe(ErpErrorCodes.Pensions.BankBranchNotFound);

            var achieng = await _pensioners.CreateAsync(new CreateUpdatePensionerDto
            {
                SchemeCode = "OMEGA",
                Name = "Achieng Otieno",
                MonthlyPension = 40000m,
                StartDate = new DateTime(2025, 1, 1),
                BankCode = "KCB",
                BankBranchCode = "001",
                BankAccountNo = "1100223344",
            });
            achieng.BankName.ShouldBe("Kenya Commercial Bank");
            achieng.BankBranch.ShouldBe("Moi Avenue");
            achieng.PayModeCode.ShouldBe("BANK");

            // A medical allowance that is not taxed, a loan repayment of 10% taken before tax, a union fee after it.
            await items.CreateAsync(new CreateUpdatePensionerPayItemDto { Code = "MEDICAL", Description = "Medical allowance", ItemType = PensionerPayItemType.Earning, Amount = 5000m });
            await items.CreateAsync(new CreateUpdatePensionerPayItemDto
            {
                Code = "LOAN",
                Description = "Loan repayment",
                ItemType = PensionerPayItemType.Deduction,
                Calculation = PensionerPayItemCalculation.PercentOfPension,
                Pct = 10m,
                Taxable = true,
                AccountNo = "2340",
            });
            await items.CreateAsync(new CreateUpdatePensionerPayItemDto { Code = "UNION", Description = "Union fee", ItemType = PensionerPayItemType.Deduction, Amount = 500m, AccountNo = "2340" });
            await items.CreateAsync(new CreateUpdatePensionerPayItemDto { Code = "OLD", Description = "Old allowance", ItemType = PensionerPayItemType.Earning, Amount = 1000m, Blocked = true });

            foreach (var code in new[] { "MEDICAL", "LOAN", "UNION", "OLD" })
            {
                await assignments.CreateAsync(new CreateUpdatePensionerPayItemAssignmentDto { PensionerNo = achieng.No, PayItemCode = code, StartDate = new DateTime(2025, 1, 1) });
            }

            // Ended before the month: not taken.
            await assignments.CreateAsync(new CreateUpdatePensionerPayItemAssignmentDto
            {
                PensionerNo = achieng.No,
                PayItemCode = "UNION",
                Amount = 700m,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 12, 31),
            });

            var payroll = await _payrolls.CreateAsync(new CreateUpdatePensionPayrollHeaderDto
            {
                SchemeCode = "OMEGA",
                PayPeriod = new DateTime(2026, 1, 1),
                PostingDate = new DateTime(2026, 1, 27),
                TaxRatePct = 10m,
            });
            payroll = await _payrolls.SuggestLinesAsync(payroll.Id);

            // Gross 40,000 + 5,000. Taxed on 40,000 less the 4,000 loan: 3,600. Net 45,000 - 3,600 - 4,500.
            var line = (await GetRequiredService<IPensionPayrollLineAppService>().GetListAsync(new GetPensionPayrollLineListInput { DocumentNo = payroll.No })).Items.Single();
            line.MonthlyPension.ShouldBe(40000m);
            line.OtherEarnings.ShouldBe(5000m);
            line.GrossPension.ShouldBe(45000m);
            line.TaxAmount.ShouldBe(3600m);
            line.Deductions.ShouldBe(4500m);
            line.NetPension.ShouldBe(36900m);
            line.PayModeCode.ShouldBe("BANK");

            var worked = await lineItems.GetListAsync(new GetPensionPayrollLineItemListInput { DocumentNo = payroll.No, LineNo = line.LineNo });
            worked.Items.Select(i => i.PayItemCode).ShouldBe(new[] { "MEDICAL", "LOAN", "UNION" });

            await _payrolls.ReleaseAsync(payroll.Id);
            payroll = await _payrolls.RunPostingAsync(payroll.Id);
            payroll.TotalDeductions.ShouldBe(4500m);
            payroll.TotalNet.ShouldBe(36900m);

            var gl = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == payroll.No);
            gl.Sum(e => e.Amount).ShouldBe(0m);
            gl.Where(e => e.GLAccountNo == "8620").Sum(e => e.Amount).ShouldBe(45000m);
            gl.Where(e => e.GLAccountNo == "2340").Sum(e => e.Amount).ShouldBe(-4500m);
            gl.Single(e => e.GLAccountNo == "2630").Amount.ShouldBe(-3600m);
            gl.Single(e => e.GLAccountNo == "2620").Amount.ShouldBe(-36900m);

            var inUse = await Should.ThrowAsync<BusinessException>(async () =>
            {
                var loan = (await items.GetListAsync(new GetCodeTableListInput { Filter = "LOAN" })).Items.Single();
                await items.DeleteAsync(loan.Id);
            });
            inUse.Code.ShouldBe(ErpErrorCodes.Pensions.PayItemInUse);
        });
    }

    [Fact]
    public async Task Suspension_Reasons_Name_Why_A_Pension_Stopped_And_The_Life_Certificate_Reason_Reinstates()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _schemes.CreateAsync(new CreateUpdatePensionSchemeDto { Code = "KAPPA", Description = "Kappa Pension Scheme" });
            var mutua = await _pensioners.CreateAsync(new CreateUpdatePensionerDto { SchemeCode = "KAPPA", Name = "Mutua Kioko", MonthlyPension = 30000m, StartDate = new DateTime(2025, 1, 1) });
            var wairimu = await _pensioners.CreateAsync(new CreateUpdatePensionerDto { SchemeCode = "KAPPA", Name = "Wairimu Njoroge", MonthlyPension = 30000m, StartDate = new DateTime(2026, 1, 1) });

            mutua = await _pensioners.SuspendAsync(mutua.Id, new PensionerActionInput { ReasonCode = "BANK", Reason = "Account dormant", Date = new DateTime(2026, 3, 1) });
            mutua.SuspensionReasonCode.ShouldBe("BANK");
            mutua.SuspensionReason.ShouldBe("Bank account closed or rejected: Account dormant");

            var unknown = await Should.ThrowAsync<BusinessException>(() => _pensioners.SuspendAsync(wairimu.Id, new PensionerActionInput { ReasonCode = "NOPE" }));
            unknown.Code.ShouldBe(ErpErrorCodes.PostingSetup.CodeNotFound);

            // Wairimu's first certificate falls due in January 2027.
            (await _pensioners.SuspendOverdueAsync(new SuspendOverduePensionersInput { SchemeCode = "KAPPA", AsOfDate = new DateTime(2027, 2, 1) })).NoOfPensioners.ShouldBe(1);
            wairimu = await _pensioners.GetAsync(wairimu.Id);
            wairimu.Status.ShouldBe(PensionerStatus.Suspended);
            wairimu.SuspensionReasonCode.ShouldBe("LIFECERT");

            wairimu = await _pensioners.RecordLifeCertificateAsync(wairimu.Id, new PensionerActionInput { Date = new DateTime(2027, 2, 1) });
            wairimu.Status.ShouldBe(PensionerStatus.Active);
            wairimu.SuspensionReasonCode.ShouldBeNull();

            // A certificate does not pay a pension stopped for another reason.
            mutua = await _pensioners.RecordLifeCertificateAsync(mutua.Id, new PensionerActionInput { Date = new DateTime(2027, 2, 1) });
            mutua.Status.ShouldBe(PensionerStatus.Suspended);

            // Increments carry the reason the pensions were revised.
            var increment = await GetRequiredService<IPensionIncrementAppService>().CreateAsync(new CreateUpdatePensionIncrementDto
            {
                SchemeCode = "KAPPA",
                EffectiveDate = new DateTime(2027, 1, 1),
                IncrementPct = 5m,
                ReasonCode = "COLA",
            });
            increment.ReasonCode.ShouldBe("COLA");
        });
    }

    [Fact]
    public async Task An_Exit_Is_Approved_Only_Once_Its_Mandatory_Documents_Are_In()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var exits = GetRequiredService<IMemberExitAppService>();
            var exitDocuments = GetRequiredService<IMemberExitDocumentAppService>();
            var reasonDocuments = GetRequiredService<IExitReasonDocumentAppService>();

            await GetRequiredService<IExitReasonAppService>().CreateAsync(new CreateUpdateExitReasonDto { Code = "RESIGN", Description = "Resignation" });
            await reasonDocuments.CreateAsync(new CreateUpdateExitReasonDocumentDto { ExitReasonCode = "RESIGN", DocumentName = "Claim form", Mandatory = true });
            await reasonDocuments.CreateAsync(new CreateUpdateExitReasonDocumentDto { ExitReasonCode = "RESIGN", DocumentName = "Copy of ID", Mandatory = false });

            await _schemes.CreateAsync(new CreateUpdatePensionSchemeDto { Code = "DELTA", Description = "Delta Pension Scheme" });
            var sponsor = await _sponsors.CreateAsync(new CreateUpdatePensionSponsorDto { Name = "Delta Ltd", SchemeCode = "DELTA", EmployeeRatePct = 5m, EmployerRatePct = 10m });
            var member = await _members.CreateAsync(new CreateUpdatePensionMemberDto
            {
                SponsorNo = sponsor.No,
                FirstName = "Kipchoge",
                LastName = "Keino",
                DateOfBirth = new DateTime(1980, 1, 1),
                JoinSchemeDate = new DateTime(2015, 1, 1),
                CurrentSalary = 80000m,
            });
            await PostMonthAsync(sponsor, member, new DateTime(2026, 1, 1), 80000m);

            var exit = await exits.CreateAsync(new CreateUpdateMemberExitDto { MemberNo = member.No, ReasonCode = "RESIGN", ExitDate = new DateTime(2026, 3, 31) });
            var documents = (await exitDocuments.GetListAsync(new GetMemberExitDocumentListInput { ExitNo = exit.No })).Items;
            documents.Select(d => d.DocumentName).ShouldBe(new[] { "Claim form", "Copy of ID" });

            var outstanding = await Should.ThrowAsync<BusinessException>(() => exits.ApproveAsync(exit.Id));
            outstanding.Code.ShouldBe(ErpErrorCodes.Pensions.ExitDocumentsOutstanding);

            var claimForm = documents.First();
            var received = await exitDocuments.UpdateAsync(claimForm.Id, new CreateUpdateMemberExitDocumentDto
            {
                ExitNo = exit.No,
                DocumentName = claimForm.DocumentName,
                Mandatory = true,
                Received = true,
                ReceivedDate = new DateTime(2026, 4, 2),
            });
            received.ReceivedDate.ShouldBe(new DateTime(2026, 4, 2));

            // Copying again adds nothing that is already listed.
            (await exits.CopyRequiredDocumentsAsync(exit.Id)).NoOfDocuments.ShouldBe(0);

            (await exits.ApproveAsync(exit.Id)).Status.ShouldBe(MemberExitStatus.Approved);

            // An approved exit's documents stay as they are.
            await Should.ThrowAsync<BusinessException>(() => exitDocuments.DeleteAsync(claimForm.Id));
        });
    }

    [Fact]
    public async Task A_Defined_Benefit_Takes_The_Average_Salary_The_Age_Factors_And_Commutes_A_Trivial_Pension()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var calculations = GetRequiredService<IPensionBenefitCalculationAppService>();
            var factors = GetRequiredService<IPensionAgeFactorAppService>();

            await _schemes.CreateAsync(new CreateUpdatePensionSchemeDto
            {
                Code = "SIGMADB",
                Description = "Sigma Defined Benefit Scheme",
                PlanType = PensionPlanType.DefinedBenefit,
                AccrualRatePct = 2m,
                MaxCommutationPct = 25m,
                CommutationFactor = 10m,
                EarlyRetirementReductionPct = 3m,
                PensionableSalaryBasis = PensionableSalaryBasis.AverageOfLastYears,
                SalaryAveragingYears = 3,
            });

            // The table gives a woman retiring at 55 three quarters of the full pension and 12 for each unit commuted.
            await factors.CreateAsync(new CreateUpdatePensionAgeFactorDto { SchemeCode = "SIGMADB", FactorType = PensionFactorType.EarlyRetirement, Age = 55, MaleFactor = 0.8m, FemaleFactor = 0.75m });
            await factors.CreateAsync(new CreateUpdatePensionAgeFactorDto { SchemeCode = "SIGMADB", FactorType = PensionFactorType.Commutation, Age = 50, MaleFactor = 11m, FemaleFactor = 12m });
            var duplicate = await Should.ThrowAsync<BusinessException>(() => factors.CreateAsync(new CreateUpdatePensionAgeFactorDto
            {
                SchemeCode = "SIGMADB",
                FactorType = PensionFactorType.Commutation,
                Age = 50,
            }));
            duplicate.Code.ShouldBe(ErpErrorCodes.BaseTables.RecordAlreadyExists);

            var sponsor = await _sponsors.CreateAsync(new CreateUpdatePensionSponsorDto { Name = "Sigma Ltd", SchemeCode = "SIGMADB", EmployeeRatePct = 5m, EmployerRatePct = 10m });
            var member = await _members.CreateAsync(new CreateUpdatePensionMemberDto
            {
                SponsorNo = sponsor.No,
                FirstName = "Akinyi",
                LastName = "Ouma",
                Gender = MemberGender.Female,
                DateOfBirth = new DateTime(1971, 3, 1),
                JoinSchemeDate = new DateTime(2001, 3, 1),
                CurrentSalary = 150000m,
            });

            // The salary history: 100,000 then 130,000, an average of 1,380,000 a year.
            await PostMonthAsync(sponsor, member, new DateTime(2026, 1, 1), 100000m);
            await PostMonthAsync(sponsor, member, new DateTime(2026, 2, 1), 130000m);

            var calculation = await calculations.CreateAsync(new CreateUpdatePensionBenefitCalculationDto
            {
                MemberNo = member.No,
                CalculationDate = new DateTime(2026, 3, 1),
                RetirementDate = new DateTime(2026, 3, 1),
                CommutationPct = 20m,
            });

            // 1,380,000 x 25 years x 2% = 690,000, times 0.75 = 517,500; a fifth commuted at 12.
            calculation.FinalPensionableSalary.ShouldBe(1380000m);
            calculation.FullAnnualPension.ShouldBe(690000m);
            calculation.AgeFactor.ShouldBe(0.75m);
            calculation.EarlyReductionPct.ShouldBe(25m);
            calculation.ReducedAnnualPension.ShouldBe(517500m);
            calculation.CommutationFactor.ShouldBe(12m);
            calculation.LumpSum.ShouldBe(1242000m);
            calculation.MonthlyPension.ShouldBe(34500m);
            calculation.Trivial.ShouldBeFalse();

            // Below the trivial pension limit, all of it is paid as a lump sum.
            var setup = await _setup.GetAsync();
            setup.TrivialPensionLimit = 50000m;
            await _setup.UpdateAsync(setup);

            await calculations.UpdateAsync(calculation.Id, new CreateUpdatePensionBenefitCalculationDto
            {
                MemberNo = member.No,
                CalculationDate = new DateTime(2026, 3, 1),
                RetirementDate = new DateTime(2026, 3, 1),
                CommutationPct = 20m,
            });
            calculation = await calculations.CalculateAsync(calculation.Id);
            calculation.Trivial.ShouldBeTrue();
            calculation.LumpSum.ShouldBe(6210000m);
            calculation.MonthlyPension.ShouldBe(0m);

            var approved = await calculations.ApproveAsync(calculation.Id);
            approved.PensionerNo.ShouldBeNull();
            approved.PaymentVoucherNo.ShouldNotBeNull();
        });
    }

    [Fact]
    public void The_Highest_Twelve_Months_Count_A_Missing_Month_As_Nothing()
    {
        var start = new DateTime(2024, 1, 1);
        var months = Enumerable.Range(0, 24).ToDictionary(i => start.AddMonths(i), i => i < 12 ? 10000m : 12000m);
        months.Remove(start.AddMonths(20));

        // September 2024 to August 2025: four months at 10,000 and eight at 12,000.
        DefinedBenefitTermsManager.HighestTwelveMonths(months, start, start.AddMonths(24)).ShouldBe(136000m);
    }
}
