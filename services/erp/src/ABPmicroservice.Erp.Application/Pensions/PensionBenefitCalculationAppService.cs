using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>Defined benefit calculations: worked out from the scheme's formula and approved onto the pension payroll.</summary>
public class PensionBenefitCalculationAppService
    : ErpTableAppService<PensionBenefitCalculation, PensionBenefitCalculationDto, GetPensionBenefitCalculationListInput, CreateUpdatePensionBenefitCalculationDto>,
        IPensionBenefitCalculationAppService
{
    private readonly DefinedBenefitCalculator _calculator;
    private readonly PensionSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly IRepository<PensionMember, Guid> _members;

    public PensionBenefitCalculationAppService(
        IRepository<PensionBenefitCalculation, Guid> repository,
        DefinedBenefitCalculator calculator,
        PensionSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        IRepository<PensionMember, Guid> members
    )
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _calculator = calculator;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _members = members;
    }

    public override async Task<PensionBenefitCalculationDto> CreateAsync(CreateUpdatePensionBenefitCalculationDto input)
    {
        await CheckCreatePolicyAsync();

        var member = await GetMemberAsync(input.MemberNo);
        var date = input.CalculationDate == default ? Clock.Now.Date : input.CalculationDate;
        var no = (await _noSeriesManager.ResolveNoAsync((await _setupManager.GetAsync()).BenefitCalculationNos, input.No, date)).ToUpperInvariant();

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Pension Benefit Calculation").WithData("key", no);
        }

        var retirement = input.RetirementDate ?? member.ExpectedRetirementDate ?? date;
        var calculation = new PensionBenefitCalculation(GuidGenerator.Create(), no, member, retirement);
        calculation.Set(member, date, retirement, input.FinalPensionableSalary, input.CommutationPct, input.Comment);

        await Repository.InsertAsync(calculation, autoSave: true);
        await _calculator.CalculateAsync(calculation);
        return await MapToGetOutputDtoAsync(calculation);
    }

    public override async Task<PensionBenefitCalculationDto> UpdateAsync(Guid id, CreateUpdatePensionBenefitCalculationDto input)
    {
        await CheckUpdatePolicyAsync();

        var calculation = await GetEntityByIdAsync(id);
        var member = await GetMemberAsync(input.MemberNo);
        calculation.Set(
            member,
            input.CalculationDate == default ? calculation.CalculationDate : input.CalculationDate,
            input.RetirementDate ?? calculation.RetirementDate,
            input.FinalPensionableSalary,
            input.CommutationPct,
            input.Comment
        );

        await _calculator.CalculateAsync(calculation);
        return await MapToGetOutputDtoAsync(calculation);
    }

    /// <summary>An approved calculation made a pensioner and stays.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        (await GetEntityByIdAsync(id)).EnsureOpen();
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Pensions.Update)]
    public async Task<PensionBenefitCalculationDto> CalculateAsync(Guid id)
    {
        var calculation = await GetEntityByIdAsync(id);
        await _calculator.CalculateAsync(calculation);
        return await MapToGetOutputDtoAsync(calculation);
    }

    [Authorize(ErpPermissions.Pensions.Post)]
    public async Task<PensionBenefitCalculationDto> ApproveAsync(Guid id)
    {
        var calculation = await GetEntityByIdAsync(id);
        await _calculator.ApproveAsync(calculation);
        return await MapToGetOutputDtoAsync(calculation);
    }

    protected override async Task<IQueryable<PensionBenefitCalculation>> CreateFilteredQueryAsync(GetPensionBenefitCalculationListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var scheme = input.SchemeCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!scheme.IsNullOrEmpty(), x => x.SchemeCode == scheme)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter) || x.MemberNo.ToLower().Contains(filter) || (x.MemberName != null && x.MemberName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<PensionBenefitCalculation> ApplyDefaultSorting(IQueryable<PensionBenefitCalculation> query) => query.OrderByDescending(x => x.No);

    private async Task<PensionMember> GetMemberAsync(string memberNo)
    {
        var no = CodeTableEntity.NormalizeCode(memberNo);
        return await _members.FirstOrDefaultAsync(m => m.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Member").WithData("code", no ?? string.Empty);
    }
}
