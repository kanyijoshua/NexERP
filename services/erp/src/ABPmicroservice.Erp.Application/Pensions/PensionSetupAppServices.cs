using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>Pension Setup: one record per company, created on first read.</summary>
[Authorize(ErpPermissions.PensionSetup.Default)]
public class PensionSetupAppService : ErpAppService, IPensionSetupAppService
{
    private readonly PensionSetupManager _setupManager;
    private readonly IRepository<PensionSetup, Guid> _repository;
    private readonly IRepository<Dimension, Guid> _dimensions;
    private readonly NoSeriesCodeValidator _seriesValidator;
    private readonly TableRelationChecker _relations;

    public PensionSetupAppService(
        PensionSetupManager setupManager,
        IRepository<PensionSetup, Guid> repository,
        IRepository<Dimension, Guid> dimensions,
        NoSeriesCodeValidator seriesValidator,
        TableRelationChecker relations
    )
    {
        _setupManager = setupManager;
        _repository = repository;
        _dimensions = dimensions;
        _seriesValidator = seriesValidator;
        _relations = relations;
    }

    public async Task<PensionSetupDto> GetAsync()
    {
        return ObjectMapper.Map<PensionSetup, PensionSetupDto>(await _setupManager.GetAsync());
    }

    [Authorize(ErpPermissions.PensionSetup.Update)]
    public async Task<PensionSetupDto> UpdateAsync(PensionSetupDto input)
    {
        await _seriesValidator.EnsureExistAsync(input.MemberNos, input.SponsorNos, input.ContributionNos, input.InterestBatchNos, input.ExitNos);
        await _relations.EnsureGLAccountsExistAsync(
            input.MemberFundsAccountNo,
            input.ContributionAccrualAccountNo,
            input.InterestAccountNo,
            input.BenefitsPayableAccountNo,
            input.TaxAccountNo
        );

        var dimensionCode = CodeTableEntity.NormalizeCode(input.SchemeDimensionCode);
        if (dimensionCode != null && !await _dimensions.AnyAsync(d => d.Code == dimensionCode))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Dimension").WithData("code", dimensionCode);
        }

        var setup = await _setupManager.GetAsync();
        setup.SetSchemeDimension(input.SchemeDimensionCode);
        setup.SetNumbering(input.MemberNos, input.SponsorNos, input.ContributionNos, input.InterestBatchNos, input.ExitNos);
        setup.SetAccounts(
            input.MemberFundsAccountNo,
            input.ContributionAccrualAccountNo,
            input.InterestAccountNo,
            input.BenefitsPayableAccountNo,
            input.TaxAccountNo
        );
        setup.SetOptions(input.AllowContributionDuplication, input.NoOfDaysInAYear);

        await _seriesValidator.EnsureExistAsync(input.PensionerNos, input.PayrollNos);
        await _relations.EnsureGLAccountsExistAsync(input.PensionsPaidAccountNo);
        setup.SetPensionPayroll(input.PensionerNos, input.PayrollNos, input.PensionsPaidAccountNo);

        await _seriesValidator.EnsureExistAsync(input.BenefitCalculationNos);
        setup.SetBenefitCalculationNumbering(input.BenefitCalculationNos);

        await _seriesValidator.EnsureExistAsync(input.IncrementNos);
        await _relations.EnsureGLAccountsExistAsync(input.TransfersInAccountNo);
        setup.SetMemberAdministration(input.TransfersInAccountNo, input.ExcessContributionAllocation, input.LifeCertificateFrequencyMonths, input.IncrementNos);

        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<PensionerPayMode>(input.DefaultPayModeCode);
        setup.SetPensionerDefaults(input.DefaultPayModeCode, input.TrivialPensionLimit);

        await _repository.UpdateAsync(setup, autoSave: true);
        return ObjectMapper.Map<PensionSetup, PensionSetupDto>(setup);
    }
}

/// <summary>A pension code table under the Pension Setup permissions.</summary>
[Authorize(ErpPermissions.PensionSetup.Default)]
public abstract class PensionCodeTableAppService<TEntity, TDto, TInput> : CodeTableAppServiceBase<TEntity, TDto, TInput>
    where TEntity : CodeTableEntity
    where TDto : CodeTableDto
    where TInput : CreateUpdateCodeTableDto
{
    protected PensionCodeTableAppService(IRepository<TEntity, Guid> repository)
        : base(repository, ErpPermissions.PensionSetup.Default) { }
}

/// <summary>Pension schemes. Saving a scheme keeps the scheme dimension's value of the same code in step.</summary>
public class PensionSchemeAppService
    : PensionCodeTableAppService<PensionScheme, PensionSchemeDto, CreateUpdatePensionSchemeDto>,
        IPensionSchemeAppService
{
    private readonly PensionSchemeManager _schemeManager;
    private readonly IRepository<PensionMember, Guid> _members;
    private readonly IRepository<PensionSponsor, Guid> _sponsors;

    public PensionSchemeAppService(
        IRepository<PensionScheme, Guid> repository,
        PensionSchemeManager schemeManager,
        IRepository<PensionMember, Guid> members,
        IRepository<PensionSponsor, Guid> sponsors
    )
        : base(repository)
    {
        _schemeManager = schemeManager;
        _members = members;
        _sponsors = sponsors;
    }

    protected override PensionScheme NewEntity(Guid id, CreateUpdatePensionSchemeDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(PensionScheme entity, CreateUpdatePensionSchemeDto input)
    {
        entity.Set(
            input.SchemeType,
            input.PlanType,
            input.SchemeMode,
            input.Status,
            input.InterestCalculationMode,
            input.RegulatorReferenceNo,
            input.TaxPinNo,
            input.NormalRetirementAge,
            input.MinimumRetirementAge
        );
        entity.SetDefinedBenefit(
            input.AccrualRatePct,
            input.MaxPensionableServiceYears,
            input.MaxCommutationPct,
            input.CommutationFactor,
            input.EarlyRetirementReductionPct
        );
        entity.SetPensionableSalary(input.PensionableSalaryBasis, input.SalaryAveragingYears);
        return Task.CompletedTask;
    }

    public override async Task<PensionSchemeDto> CreateAsync(CreateUpdatePensionSchemeDto input)
    {
        // Checked first: a scheme that could not be given its dimension value could never post.
        await _schemeManager.GetSchemeDimensionAsync();

        var dto = await base.CreateAsync(input);
        await _schemeManager.EnsureDimensionValueAsync(await Repository.GetAsync(dto.Id));
        return dto;
    }

    public override async Task<PensionSchemeDto> UpdateAsync(Guid id, CreateUpdatePensionSchemeDto input)
    {
        var existing = await Repository.GetAsync(id);
        var newCode = CodeTableEntity.NormalizeCode(input.Code);

        // The code is on every member and every entry of the scheme, so it cannot change once used.
        if (existing.Code != newCode && await IsInUseAsync(existing.Code))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.SchemeInUse).WithData("scheme", existing.Code);
        }

        var dto = await base.UpdateAsync(id, input);
        await _schemeManager.EnsureDimensionValueAsync(await Repository.GetAsync(id));
        return dto;
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var scheme = await Repository.GetAsync(id);
        if (await IsInUseAsync(scheme.Code))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.SchemeInUse).WithData("scheme", scheme.Code);
        }

        await Repository.DeleteAsync(id, autoSave: true);
    }

    private async Task<bool> IsInUseAsync(string schemeCode)
    {
        return await _members.AnyAsync(m => m.SchemeCode == schemeCode) || await _sponsors.AnyAsync(s => s.SchemeCode == schemeCode);
    }
}

/// <summary>Exit reasons: what each way of leaving a scheme pays and how it is taxed.</summary>
public class ExitReasonAppService
    : PensionCodeTableAppService<ExitReason, ExitReasonDto, CreateUpdateExitReasonDto>,
        IExitReasonAppService
{
    public ExitReasonAppService(IRepository<ExitReason, Guid> repository)
        : base(repository) { }

    protected override ExitReason NewEntity(Guid id, CreateUpdateExitReasonDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(ExitReason entity, CreateUpdateExitReasonDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<LumpsumTaxTable>(input.TaxTableCode);
        entity.Set(input.PaymentOption, input.EmployerPortionPct, input.TaxTableCode, input.LumpsumTaxFree, input.StatusAfterExit);
        entity.SetVesting(input.ApplyVestingScale);
    }
}

/// <summary>Lump sum tax tables.</summary>
public class LumpsumTaxTableAppService
    : PensionCodeTableAppService<LumpsumTaxTable, LumpsumTaxTableDto, CreateUpdateLumpsumTaxTableDto>,
        ILumpsumTaxTableAppService
{
    public LumpsumTaxTableAppService(IRepository<LumpsumTaxTable, Guid> repository)
        : base(repository) { }

    protected override LumpsumTaxTable NewEntity(Guid id, CreateUpdateLumpsumTaxTableDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(LumpsumTaxTable entity, CreateUpdateLumpsumTaxTableDto input)
    {
        entity.Set(input.AnnualTaxFreeAmount, input.MaxTaxFreeAmount, input.MaxAgeTaxable);
        return Task.CompletedTask;
    }
}

/// <summary>The bands of the lump sum tax tables.</summary>
public class LumpsumTaxBandAppService
    : ErpTableAppService<LumpsumTaxBand, LumpsumTaxBandDto, GetLumpsumTaxBandListInput, CreateUpdateLumpsumTaxBandDto>,
        ILumpsumTaxBandAppService
{
    public LumpsumTaxBandAppService(IRepository<LumpsumTaxBand, Guid> repository)
        : base(repository, ErpPermissions.PensionSetup.Default) { }

    public override async Task<LumpsumTaxBandDto> CreateAsync(CreateUpdateLumpsumTaxBandDto input)
    {
        await CheckCreatePolicyAsync();
        await CodeTableChecker.EnsureExistsAsync<LumpsumTaxTable>(input.TaxTableCode);
        await EnsureKeyIsUniqueAsync(input, null);

        var band = new LumpsumTaxBand(GuidGenerator.Create(), input.TaxTableCode, input.LowerLimit);
        band.Set(input.UpperLimit, input.RatePct);

        await Repository.InsertAsync(band, autoSave: true);
        return await MapToGetOutputDtoAsync(band);
    }

    public override async Task<LumpsumTaxBandDto> UpdateAsync(Guid id, CreateUpdateLumpsumTaxBandDto input)
    {
        await CheckUpdatePolicyAsync();

        var band = await GetEntityByIdAsync(id);
        await CodeTableChecker.EnsureExistsAsync<LumpsumTaxTable>(input.TaxTableCode);
        await EnsureKeyIsUniqueAsync(input, id);

        band.SetKey(input.TaxTableCode, input.LowerLimit);
        band.Set(input.UpperLimit, input.RatePct);

        await Repository.UpdateAsync(band, autoSave: true);
        return await MapToGetOutputDtoAsync(band);
    }

    protected override async Task<IQueryable<LumpsumTaxBand>> CreateFilteredQueryAsync(GetLumpsumTaxBandListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var table = (input.TaxTableCode ?? input.Filter)?.Trim().ToUpperInvariant();

        return query.WhereIf(!table.IsNullOrEmpty(), x => x.TaxTableCode == table);
    }

    protected override IQueryable<LumpsumTaxBand> ApplyDefaultSorting(IQueryable<LumpsumTaxBand> query) =>
        query.OrderBy(x => x.TaxTableCode).ThenBy(x => x.LowerLimit);

    private async Task EnsureKeyIsUniqueAsync(CreateUpdateLumpsumTaxBandDto input, Guid? exceptId)
    {
        var table = CodeTableEntity.NormalizeCode(input.TaxTableCode);
        if (await Repository.AnyAsync(x => x.TaxTableCode == table && x.LowerLimit == input.LowerLimit && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists)
                .WithData("table", "Lumpsum Tax Band")
                .WithData("key", $"{table} {input.LowerLimit}");
        }
    }
}

/// <summary>Declared interest, and its allocation to members.</summary>
public class PensionInterestRateAppService
    : ErpTableAppService<PensionInterestRate, PensionInterestRateDto, GetPensionInterestRateListInput, CreateUpdatePensionInterestRateDto>,
        IPensionInterestRateAppService
{
    private readonly PensionInterestEngine _engine;
    private readonly PensionSchemeManager _schemeManager;

    public PensionInterestRateAppService(IRepository<PensionInterestRate, Guid> repository, PensionInterestEngine engine, PensionSchemeManager schemeManager)
        : base(repository, ErpPermissions.PensionSetup.Default)
    {
        _engine = engine;
        _schemeManager = schemeManager;
    }

    public override async Task<PensionInterestRateDto> CreateAsync(CreateUpdatePensionInterestRateDto input)
    {
        await CheckCreatePolicyAsync();
        await _schemeManager.GetAsync(input.SchemeCode);

        var rate = new PensionInterestRate(GuidGenerator.Create(), input.SchemeCode, input.StartDate, input.EndDate);
        Apply(rate, input);
        await EnsureNoOverlapAsync(rate);

        await Repository.InsertAsync(rate, autoSave: true);
        return await MapToGetOutputDtoAsync(rate);
    }

    public override async Task<PensionInterestRateDto> UpdateAsync(Guid id, CreateUpdatePensionInterestRateDto input)
    {
        await CheckUpdatePolicyAsync();

        var rate = await GetEntityByIdAsync(id);
        await _schemeManager.GetAsync(input.SchemeCode);
        Apply(rate, input);
        await EnsureNoOverlapAsync(rate);

        await Repository.UpdateAsync(rate, autoSave: true);
        return await MapToGetOutputDtoAsync(rate);
    }

    /// <summary>Interest that has been credited to members stays on record.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var rate = await GetEntityByIdAsync(id);
        if (rate.Posted)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.InterestAlreadyAllocated).WithData("documentNo", rate.PostedDocumentNo);
        }

        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Pensions.Default)]
    public async Task<InterestAllocationDto> GetPreviewAsync(Guid id)
    {
        var lines = await _engine.CalculateAsync(await GetEntityByIdAsync(id));

        return new InterestAllocationDto
        {
            TotalInterest = lines.Sum(l => l.Interest),
            TotalTax = lines.Sum(l => l.Tax),
            NoOfMembers = lines.Select(l => l.MemberNo).Distinct().Count(),
            Lines = lines
                .Select(l => new InterestAllocationLineDto
                {
                    MemberNo = l.MemberNo,
                    MemberName = l.MemberName,
                    ContributionType = l.MoneyType.ContributionType,
                    ExemptionType = l.MoneyType.ExemptionType,
                    Balance = l.Balance,
                    Interest = l.Interest,
                    Tax = l.Tax,
                })
                .ToList(),
        };
    }

    [Authorize(ErpPermissions.Pensions.Post)]
    public async Task<PensionInterestRateDto> AllocateAsync(Guid id, AllocateInterestInput input)
    {
        var rate = await GetEntityByIdAsync(id);
        await _engine.PostAsync(rate, input?.PostingDate ?? rate.EndDate);
        return await MapToGetOutputDtoAsync(rate);
    }

    protected override async Task<IQueryable<PensionInterestRate>> CreateFilteredQueryAsync(GetPensionInterestRateListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var scheme = (input.SchemeCode ?? input.Filter)?.Trim().ToUpperInvariant();

        return query.WhereIf(!scheme.IsNullOrEmpty(), x => x.SchemeCode == scheme);
    }

    protected override IQueryable<PensionInterestRate> ApplyDefaultSorting(IQueryable<PensionInterestRate> query) =>
        query.OrderBy(x => x.SchemeCode).ThenByDescending(x => x.StartDate);

    private static void Apply(PensionInterestRate rate, CreateUpdatePensionInterestRateDto input)
    {
        rate.Set(input.SchemeCode, input.StartDate, input.EndDate, input.DateDeclared, input.RegisteredRatePct, input.UnregisteredRatePct, input.TaxRatePct);
    }

    /// <summary>Two declarations for the same days of a scheme would pay interest on them twice.</summary>
    private async Task EnsureNoOverlapAsync(PensionInterestRate rate)
    {
        if (await Repository.AnyAsync(x => x.Id != rate.Id && x.SchemeCode == rate.SchemeCode && x.StartDate <= rate.EndDate && x.EndDate >= rate.StartDate))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.InterestPeriodOverlaps).WithData("scheme", rate.SchemeCode);
        }
    }
}
