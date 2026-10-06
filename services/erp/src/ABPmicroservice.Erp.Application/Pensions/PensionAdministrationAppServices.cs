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

/// <summary>Members' beneficiaries (nominees). A member's active shares never add up to more than 100%.</summary>
public class PensionBeneficiaryAppService
    : ErpTableAppService<PensionBeneficiary, PensionBeneficiaryDto, GetPensionBeneficiaryListInput, CreateUpdatePensionBeneficiaryDto>,
        IPensionBeneficiaryAppService
{
    private readonly PensionBeneficiaryManager _manager;
    private readonly IRepository<PensionMember, Guid> _members;

    public PensionBeneficiaryAppService(IRepository<PensionBeneficiary, Guid> repository, PensionBeneficiaryManager manager, IRepository<PensionMember, Guid> members)
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _manager = manager;
        _members = members;
    }

    public override async Task<PensionBeneficiaryDto> CreateAsync(CreateUpdatePensionBeneficiaryDto input)
    {
        await CheckCreatePolicyAsync();

        var memberNo = await GetMemberNoAsync(input.MemberNo);
        var lineNo = input.LineNo;
        if (lineNo <= 0)
        {
            var query = await Repository.GetQueryableAsync();
            var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.MemberNo == memberNo).OrderByDescending(x => x.LineNo));
            lineNo = (last?.LineNo ?? 0) + 10000;
        }

        if (await Repository.AnyAsync(x => x.MemberNo == memberNo && x.LineNo == lineNo))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Pension Beneficiary").WithData("key", $"{memberNo} {lineNo}");
        }

        var beneficiary = new PensionBeneficiary(GuidGenerator.Create(), memberNo, lineNo, input.Name);
        Apply(beneficiary, input);
        await _manager.EnsureSharesWithinLimitAsync(beneficiary);

        await Repository.InsertAsync(beneficiary, autoSave: true);
        return await MapToGetOutputDtoAsync(beneficiary);
    }

    public override async Task<PensionBeneficiaryDto> UpdateAsync(Guid id, CreateUpdatePensionBeneficiaryDto input)
    {
        await CheckUpdatePolicyAsync();

        var beneficiary = await GetEntityByIdAsync(id);
        Apply(beneficiary, input);
        await _manager.EnsureSharesWithinLimitAsync(beneficiary);

        await Repository.UpdateAsync(beneficiary, autoSave: true);
        return await MapToGetOutputDtoAsync(beneficiary);
    }

    protected override async Task<IQueryable<PensionBeneficiary>> CreateFilteredQueryAsync(GetPensionBeneficiaryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var member = input.MemberNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!member.IsNullOrEmpty(), x => x.MemberNo == member)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.MemberNo.ToLower().Contains(filter) || x.Name.ToLower().Contains(filter));
    }

    protected override IQueryable<PensionBeneficiary> ApplyDefaultSorting(IQueryable<PensionBeneficiary> query) =>
        query.OrderBy(x => x.MemberNo).ThenBy(x => x.LineNo);

    private async Task<string> GetMemberNoAsync(string memberNo)
    {
        var no = CodeTableEntity.NormalizeCode(memberNo);
        return await _members.AnyAsync(m => m.No == no)
            ? no
            : throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Member").WithData("code", no ?? string.Empty);
    }

    private static void Apply(PensionBeneficiary beneficiary, CreateUpdatePensionBeneficiaryDto input)
    {
        beneficiary.Set(input.Name, input.Relationship, input.DateOfBirth, input.NationalId, input.BenefitPct, input.Status, input.GuardianName);
        beneficiary.SetContact(input.PhoneNo, input.Email, input.BankName, input.BankAccountNo);
    }
}

/// <summary>A sponsor's contribution rates by date. The periods of one sponsor may not overlap.</summary>
public class PensionContributionRateAppService
    : ErpTableAppService<PensionContributionRate, PensionContributionRateDto, GetPensionContributionRateListInput, CreateUpdatePensionContributionRateDto>,
        IPensionContributionRateAppService
{
    private readonly IRepository<PensionSponsor, Guid> _sponsors;

    public PensionContributionRateAppService(IRepository<PensionContributionRate, Guid> repository, IRepository<PensionSponsor, Guid> sponsors)
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _sponsors = sponsors;
    }

    public override async Task<PensionContributionRateDto> CreateAsync(CreateUpdatePensionContributionRateDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureSponsorExistsAsync(input.SponsorNo);

        var rate = new PensionContributionRate(GuidGenerator.Create(), input.SponsorNo, input.StartDate);
        rate.Set(input.SponsorNo, input.StartDate, input.EndDate, input.EmployeeRatePct, input.EmployerRatePct);
        await EnsureNoOverlapAsync(rate);

        await Repository.InsertAsync(rate, autoSave: true);
        return await MapToGetOutputDtoAsync(rate);
    }

    public override async Task<PensionContributionRateDto> UpdateAsync(Guid id, CreateUpdatePensionContributionRateDto input)
    {
        await CheckUpdatePolicyAsync();
        await EnsureSponsorExistsAsync(input.SponsorNo);

        var rate = await GetEntityByIdAsync(id);
        rate.Set(input.SponsorNo, input.StartDate, input.EndDate, input.EmployeeRatePct, input.EmployerRatePct);
        await EnsureNoOverlapAsync(rate);

        await Repository.UpdateAsync(rate, autoSave: true);
        return await MapToGetOutputDtoAsync(rate);
    }

    protected override async Task<IQueryable<PensionContributionRate>> CreateFilteredQueryAsync(GetPensionContributionRateListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var sponsor = (input.SponsorNo ?? input.Filter)?.Trim().ToUpperInvariant();
        return query.WhereIf(!sponsor.IsNullOrEmpty(), x => x.SponsorNo == sponsor);
    }

    protected override IQueryable<PensionContributionRate> ApplyDefaultSorting(IQueryable<PensionContributionRate> query) =>
        query.OrderBy(x => x.SponsorNo).ThenByDescending(x => x.StartDate);

    private async Task EnsureSponsorExistsAsync(string sponsorNo)
    {
        var no = CodeTableEntity.NormalizeCode(sponsorNo);
        if (!await _sponsors.AnyAsync(s => s.No == no))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Sponsor").WithData("code", no ?? string.Empty);
        }
    }

    /// <summary>Two rates for the same months would leave a schedule not knowing which one it is under.</summary>
    private async Task EnsureNoOverlapAsync(PensionContributionRate rate)
    {
        var others = await Repository.GetListAsync(x => x.SponsorNo == rate.SponsorNo && x.Id != rate.Id);
        if (others.Any(rate.Overlaps))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.ContributionRatePeriodOverlaps).WithData("sponsorNo", rate.SponsorNo);
        }
    }
}

/// <summary>Sponsors' vesting scales: one band per length of service.</summary>
public class PensionVestingScaleAppService
    : ErpTableAppService<PensionVestingScale, PensionVestingScaleDto, GetPensionVestingScaleListInput, CreateUpdatePensionVestingScaleDto>,
        IPensionVestingScaleAppService
{
    private readonly IRepository<PensionSponsor, Guid> _sponsors;

    public PensionVestingScaleAppService(IRepository<PensionVestingScale, Guid> repository, IRepository<PensionSponsor, Guid> sponsors)
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _sponsors = sponsors;
    }

    public override async Task<PensionVestingScaleDto> CreateAsync(CreateUpdatePensionVestingScaleDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureValidAsync(input, null);

        var band = new PensionVestingScale(GuidGenerator.Create(), input.SponsorNo, input.FromServiceYears);
        band.Set(input.EmployerVestedPct);

        await Repository.InsertAsync(band, autoSave: true);
        return await MapToGetOutputDtoAsync(band);
    }

    public override async Task<PensionVestingScaleDto> UpdateAsync(Guid id, CreateUpdatePensionVestingScaleDto input)
    {
        await CheckUpdatePolicyAsync();

        var band = await GetEntityByIdAsync(id);
        await EnsureValidAsync(input, id);
        band.SetKey(input.SponsorNo, input.FromServiceYears);
        band.Set(input.EmployerVestedPct);

        await Repository.UpdateAsync(band, autoSave: true);
        return await MapToGetOutputDtoAsync(band);
    }

    protected override async Task<IQueryable<PensionVestingScale>> CreateFilteredQueryAsync(GetPensionVestingScaleListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var sponsor = (input.SponsorNo ?? input.Filter)?.Trim().ToUpperInvariant();
        return query.WhereIf(!sponsor.IsNullOrEmpty(), x => x.SponsorNo == sponsor);
    }

    protected override IQueryable<PensionVestingScale> ApplyDefaultSorting(IQueryable<PensionVestingScale> query) =>
        query.OrderBy(x => x.SponsorNo).ThenBy(x => x.FromServiceYears);

    private async Task EnsureValidAsync(CreateUpdatePensionVestingScaleDto input, Guid? exceptId)
    {
        var no = CodeTableEntity.NormalizeCode(input.SponsorNo);
        if (!await _sponsors.AnyAsync(s => s.No == no))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Sponsor").WithData("code", no ?? string.Empty);
        }

        if (await Repository.AnyAsync(x => x.SponsorNo == no && x.FromServiceYears == input.FromServiceYears && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Vesting Scale").WithData("key", $"{no} {input.FromServiceYears}");
        }
    }
}

/// <summary>Monthly tax relief limits on contributions, by the date each takes effect.</summary>
public class PensionTaxReliefLimitAppService
    : ErpTableAppService<PensionTaxReliefLimit, PensionTaxReliefLimitDto, GetPensionTaxReliefLimitListInput, CreateUpdatePensionTaxReliefLimitDto>,
        IPensionTaxReliefLimitAppService
{
    public PensionTaxReliefLimitAppService(IRepository<PensionTaxReliefLimit, Guid> repository)
        : base(repository, ErpPermissions.PensionSetup.Default) { }

    public override async Task<PensionTaxReliefLimitDto> CreateAsync(CreateUpdatePensionTaxReliefLimitDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureUniqueAsync(input.EffectiveDate, null);

        var limit = new PensionTaxReliefLimit(GuidGenerator.Create(), input.EffectiveDate, input.MonthlyLimit);
        await Repository.InsertAsync(limit, autoSave: true);
        return await MapToGetOutputDtoAsync(limit);
    }

    public override async Task<PensionTaxReliefLimitDto> UpdateAsync(Guid id, CreateUpdatePensionTaxReliefLimitDto input)
    {
        await CheckUpdatePolicyAsync();

        var limit = await GetEntityByIdAsync(id);
        await EnsureUniqueAsync(input.EffectiveDate, id);
        limit.Set(input.EffectiveDate, input.MonthlyLimit);

        await Repository.UpdateAsync(limit, autoSave: true);
        return await MapToGetOutputDtoAsync(limit);
    }

    protected override IQueryable<PensionTaxReliefLimit> ApplyDefaultSorting(IQueryable<PensionTaxReliefLimit> query) => query.OrderByDescending(x => x.EffectiveDate);

    private async Task EnsureUniqueAsync(DateTime effectiveDate, Guid? exceptId)
    {
        var date = effectiveDate.Date;
        if (await Repository.AnyAsync(x => x.EffectiveDate == date && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Tax Relief Limit").WithData("key", date.ToString("yyyy-MM-dd"));
        }
    }
}

/// <summary>Members' status history.</summary>
public class MemberStatusEntryAppService
    : ErpReadOnlyAppService<MemberStatusEntry, MemberStatusEntryDto, Guid, GetMemberStatusEntryListInput>,
        IMemberStatusEntryAppService
{
    public MemberStatusEntryAppService(IRepository<MemberStatusEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Pensions.Default;
        GetListPolicyName = ErpPermissions.Pensions.Default;
    }

    protected override async Task<IQueryable<MemberStatusEntry>> CreateFilteredQueryAsync(GetMemberStatusEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var member = input.MemberNo?.Trim().ToUpperInvariant();
        var scheme = input.SchemeCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!member.IsNullOrEmpty(), x => x.MemberNo == member)
            .WhereIf(!scheme.IsNullOrEmpty(), x => x.SchemeCode == scheme)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.MemberNo.ToLower().Contains(filter) || (x.DocumentNo != null && x.DocumentNo.ToLower().Contains(filter)));
    }

    protected override IQueryable<MemberStatusEntry> ApplyDefaultSorting(IQueryable<MemberStatusEntry> query) =>
        query.OrderByDescending(x => x.EffectiveDate).ThenBy(x => x.MemberNo);
}

/// <summary>Members' salary history, one salary per member and month.</summary>
public class MemberSalaryEntryAppService
    : ErpReadOnlyAppService<MemberSalaryEntry, MemberSalaryEntryDto, Guid, GetMemberSalaryEntryListInput>,
        IMemberSalaryEntryAppService
{
    public MemberSalaryEntryAppService(IRepository<MemberSalaryEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Pensions.Default;
        GetListPolicyName = ErpPermissions.Pensions.Default;
    }

    protected override async Task<IQueryable<MemberSalaryEntry>> CreateFilteredQueryAsync(GetMemberSalaryEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var member = input.MemberNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!member.IsNullOrEmpty(), x => x.MemberNo == member)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.MemberNo.ToLower().Contains(filter) || (x.DocumentNo != null && x.DocumentNo.ToLower().Contains(filter)));
    }

    protected override IQueryable<MemberSalaryEntry> ApplyDefaultSorting(IQueryable<MemberSalaryEntry> query) =>
        query.OrderBy(x => x.MemberNo).ThenByDescending(x => x.Period);
}

/// <summary>Pension increments: prepared, then applied to every pension of the scheme at once.</summary>
public class PensionIncrementAppService
    : ErpTableAppService<PensionIncrement, PensionIncrementDto, GetPensionIncrementListInput, CreateUpdatePensionIncrementDto>,
        IPensionIncrementAppService
{
    private readonly PensionerAdministrator _administrator;
    private readonly PensionSetupManager _setupManager;
    private readonly PensionSchemeManager _schemeManager;
    private readonly NoSeriesManager _noSeriesManager;

    public PensionIncrementAppService(
        IRepository<PensionIncrement, Guid> repository,
        PensionerAdministrator administrator,
        PensionSetupManager setupManager,
        PensionSchemeManager schemeManager,
        NoSeriesManager noSeriesManager
    )
        : base(repository, ErpPermissions.Pensions.Default)
    {
        _administrator = administrator;
        _setupManager = setupManager;
        _schemeManager = schemeManager;
        _noSeriesManager = noSeriesManager;
    }

    public override async Task<PensionIncrementDto> CreateAsync(CreateUpdatePensionIncrementDto input)
    {
        await CheckCreatePolicyAsync();

        var scheme = await _schemeManager.GetAsync(input.SchemeCode);
        var setup = await _setupManager.GetAsync();
        var effective = input.EffectiveDate == default ? Clock.Now.Date : input.EffectiveDate;
        var no = (await _noSeriesManager.ResolveNoAsync(setup.IncrementNos, input.No, effective)).ToUpperInvariant();

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Pension Increment").WithData("key", no);
        }

        var increment = new PensionIncrement(GuidGenerator.Create(), no, scheme.Code, effective);
        increment.Set(scheme.Code, effective, input.IncrementPct, input.MinimumMonthlyPension, input.Description);
        await CodeTableChecker.EnsureExistsAsync<PensionRevisionReason>(input.ReasonCode);
        increment.SetReason(input.ReasonCode);

        await Repository.InsertAsync(increment, autoSave: true);
        return await MapToGetOutputDtoAsync(increment);
    }

    public override async Task<PensionIncrementDto> UpdateAsync(Guid id, CreateUpdatePensionIncrementDto input)
    {
        await CheckUpdatePolicyAsync();

        var increment = await GetEntityByIdAsync(id);
        var scheme = await _schemeManager.GetAsync(input.SchemeCode);
        increment.Set(scheme.Code, input.EffectiveDate == default ? increment.EffectiveDate : input.EffectiveDate, input.IncrementPct, input.MinimumMonthlyPension, input.Description);
        await CodeTableChecker.EnsureExistsAsync<PensionRevisionReason>(input.ReasonCode);
        increment.SetReason(input.ReasonCode);

        await Repository.UpdateAsync(increment, autoSave: true);
        return await MapToGetOutputDtoAsync(increment);
    }

    /// <summary>An applied increment changed pensions and stays on record.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var increment = await GetEntityByIdAsync(id);
        increment.EnsureOpen();
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Pensions.Post)]
    public async Task<PensionIncrementDto> ApplyAsync(Guid id)
    {
        var increment = await GetEntityByIdAsync(id);
        await _administrator.ApplyIncrementAsync(increment);
        return await MapToGetOutputDtoAsync(increment);
    }

    protected override async Task<IQueryable<PensionIncrement>> CreateFilteredQueryAsync(GetPensionIncrementListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var scheme = input.SchemeCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!scheme.IsNullOrEmpty(), x => x.SchemeCode == scheme)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter) || x.SchemeCode.ToLower().Contains(filter) || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<PensionIncrement> ApplyDefaultSorting(IQueryable<PensionIncrement> query) =>
        query.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.No);
}

/// <summary>Pensioners' history: increments, suspensions, reinstatements, life certificates and arrears paid.</summary>
public class PensionerChangeEntryAppService
    : ErpReadOnlyAppService<PensionerChangeEntry, PensionerChangeEntryDto, Guid, GetPensionerChangeEntryListInput>,
        IPensionerChangeEntryAppService
{
    public PensionerChangeEntryAppService(IRepository<PensionerChangeEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Pensions.Default;
        GetListPolicyName = ErpPermissions.Pensions.Default;
    }

    protected override async Task<IQueryable<PensionerChangeEntry>> CreateFilteredQueryAsync(GetPensionerChangeEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var pensioner = input.PensionerNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!pensioner.IsNullOrEmpty(), x => x.PensionerNo == pensioner)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.PensionerNo.ToLower().Contains(filter) || (x.DocumentNo != null && x.DocumentNo.ToLower().Contains(filter)));
    }

    protected override IQueryable<PensionerChangeEntry> ApplyDefaultSorting(IQueryable<PensionerChangeEntry> query) =>
        query.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.CreationTime);
}
