using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.CostAccounting;

[Authorize(ErpPermissions.CostAccounting.Default)]
public class CostAccountingSetupAppService : ErpAppService, ICostAccountingSetupAppService
{
    private readonly CostAccountingManager _manager;
    private readonly IRepository<CostAccountingSetup, Guid> _repository;

    public CostAccountingSetupAppService(CostAccountingManager manager, IRepository<CostAccountingSetup, Guid> repository)
    {
        _manager = manager;
        _repository = repository;
    }

    public async Task<CostAccountingSetupDto> GetAsync() => Map(await _manager.GetSetupAsync());

    [Authorize(ErpPermissions.CostAccounting.Update)]
    public async Task<CostAccountingSetupDto> UpdateAsync(CostAccountingSetupDto input)
    {
        await EnsureDimensionAsync(input.CostCenterDimension);
        await EnsureDimensionAsync(input.CostObjectDimension);
        var setup = await _manager.GetSetupAsync();
        setup.Set(input.CostCenterDimension, input.CostObjectDimension);
        await _repository.UpdateAsync(setup, autoSave: true);
        return Map(setup);
    }

    private async Task EnsureDimensionAsync(string code)
    {
        var normalized = CodeTableEntity.NormalizeCode(code);
        if (normalized != null && !await LazyServiceProvider.LazyGetRequiredService<IRepository<Dimension, Guid>>().AnyAsync(d => d.Code == normalized))
        {
            throw new BusinessException(ErpErrorCodes.CostAccounting.CostCenterDimensionMissing).WithData("code", normalized);
        }
    }

    private static CostAccountingSetupDto Map(CostAccountingSetup setup) => new()
    {
        CostCenterDimension = setup.CostCenterDimension,
        CostObjectDimension = setup.CostObjectDimension,
        LastGLEntryNoTransferred = setup.LastGLEntryNoTransferred,
    };
}

[Authorize(ErpPermissions.CostAccounting.Default)]
public class CostTypeAppService : ErpTableAppService<CostType, CostTypeDto, GetCodeTableListInput, CreateUpdateCostTypeDto>, ICostTypeAppService
{
    public CostTypeAppService(IRepository<CostType, Guid> repository)
        : base(repository, ErpPermissions.CostAccounting.Default) { }

    public override async Task<CostTypeDto> CreateAsync(CreateUpdateCostTypeDto input)
    {
        await CheckCreatePolicyAsync();
        var no = input.No.Trim();
        if (await Repository.AnyAsync(t => t.No == no))
        {
            throw new BusinessException(ErpErrorCodes.CostAccounting.CostTypeNoExists).WithData("no", no);
        }

        var type = new CostType(GuidGenerator.Create(), no);
        await ApplyAsync(type, input);
        await Repository.InsertAsync(type, autoSave: true);
        return await MapToGetOutputDtoAsync(type);
    }

    public override async Task<CostTypeDto> UpdateAsync(Guid id, CreateUpdateCostTypeDto input)
    {
        await CheckUpdatePolicyAsync();
        var type = await GetEntityByIdAsync(id);
        await ApplyAsync(type, input);
        await Repository.UpdateAsync(type, autoSave: true);
        return await MapToGetOutputDtoAsync(type);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var type = await GetEntityByIdAsync(id);
        var no = type.No;
        if (await LazyServiceProvider.LazyGetRequiredService<IRepository<CostEntry, Guid>>().AnyAsync(e => e.CostTypeNo == no))
        {
            throw new BusinessException(ErpErrorCodes.CostAccounting.CostTypeHasEntries).WithData("no", no);
        }

        await Repository.DeleteAsync(type, autoSave: true);
    }

    private async Task ApplyAsync(CostType type, CreateUpdateCostTypeDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<CostCenter>(input.CostCenterCode);
        await CodeTableChecker.EnsureExistsAsync<CostObject>(input.CostObjectCode);
        type.Set(input.Name, input.Type, input.GLAccountRange, input.Totaling, input.CostCenterCode, input.CostObjectCode, input.Blocked);
    }

    protected override async Task<IQueryable<CostType>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), t => t.No.ToLower().Contains(filter) || (t.Name != null && t.Name.ToLower().Contains(filter)));
    }

    protected override IQueryable<CostType> ApplyDefaultSorting(IQueryable<CostType> query) => query.OrderBy(t => t.No);
}

public class CostCenterAppService : CodeTableAppServiceBase<CostCenter, CostCenterDto, CreateUpdateCostCenterDto>, ICostCenterAppService
{
    public CostCenterAppService(IRepository<CostCenter, Guid> repository)
        : base(repository, ErpPermissions.CostAccounting.Default) { }

    protected override CostCenter NewEntity(Guid id, CreateUpdateCostCenterDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(CostCenter entity, CreateUpdateCostCenterDto input)
    {
        entity.SetBlocked(input.Blocked);
        return Task.CompletedTask;
    }
}

public class CostObjectAppService : CodeTableAppServiceBase<CostObject, CostObjectDto, CreateUpdateCostObjectDto>, ICostObjectAppService
{
    public CostObjectAppService(IRepository<CostObject, Guid> repository)
        : base(repository, ErpPermissions.CostAccounting.Default) { }

    protected override CostObject NewEntity(Guid id, CreateUpdateCostObjectDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(CostObject entity, CreateUpdateCostObjectDto input)
    {
        entity.SetBlocked(input.Blocked);
        return Task.CompletedTask;
    }
}

[Authorize(ErpPermissions.CostAccounting.Default)]
public class CostEntryAppService : ErpReadOnlyAppService<CostEntry, CostEntryDto, Guid, GetCostEntryListInput>, ICostEntryAppService
{
    public CostEntryAppService(IRepository<CostEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.CostAccounting.Default;
        GetListPolicyName = ErpPermissions.CostAccounting.Default;
    }

    protected override async Task<IQueryable<CostEntry>> CreateFilteredQueryAsync(GetCostEntryListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var type = input.CostTypeNo?.Trim();
        var center = CodeTableEntity.NormalizeCode(input.CostCenterCode);
        var costObject = CodeTableEntity.NormalizeCode(input.CostObjectCode);
        return query
            .WhereIf(!type.IsNullOrEmpty(), e => e.CostTypeNo == type)
            .WhereIf(center != null, e => e.CostCenterCode == center)
            .WhereIf(costObject != null, e => e.CostObjectCode == costObject);
    }

    protected override IQueryable<CostEntry> ApplyDefaultSorting(IQueryable<CostEntry> query) => query.OrderByDescending(e => e.EntryNo);
}

[Authorize(ErpPermissions.CostAccounting.Default)]
public class CostRegisterAppService : ErpReadOnlyAppService<CostRegister, CostRegisterDto, Guid, ErpPagedListInput>, ICostRegisterAppService
{
    public CostRegisterAppService(IRepository<CostRegister, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.CostAccounting.Default;
        GetListPolicyName = ErpPermissions.CostAccounting.Default;
    }

    protected override IQueryable<CostRegister> ApplyDefaultSorting(IQueryable<CostRegister> query) => query.OrderByDescending(r => r.No);
}

[Authorize(ErpPermissions.CostAccounting.Default)]
public class CostAllocationSourceAppService
    : ErpTableAppService<CostAllocationSource, CostAllocationSourceDto, GetCodeTableListInput, CreateUpdateCostAllocationSourceDto>,
        ICostAllocationSourceAppService
{
    public CostAllocationSourceAppService(IRepository<CostAllocationSource, Guid> repository)
        : base(repository, ErpPermissions.CostAccounting.Default) { }

    protected override async Task<CostAllocationSource> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    public override async Task<CostAllocationSourceDto> CreateAsync(CreateUpdateCostAllocationSourceDto input)
    {
        await CheckCreatePolicyAsync();
        var code = CodeTableEntity.NormalizeCode(input.Code);
        if (await Repository.AnyAsync(a => a.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeAlreadyExists).WithData("code", code);
        }

        var source = new CostAllocationSource(GuidGenerator.Create(), code);
        await ApplyAsync(source, input);
        await Repository.InsertAsync(source, autoSave: true);
        return await MapToGetOutputDtoAsync(source);
    }

    public override async Task<CostAllocationSourceDto> UpdateAsync(Guid id, CreateUpdateCostAllocationSourceDto input)
    {
        await CheckUpdatePolicyAsync();
        var source = await GetEntityByIdAsync(id);
        await ApplyAsync(source, input);
        await Repository.UpdateAsync(source, autoSave: true);
        return await MapToGetOutputDtoAsync(source);
    }

    private async Task ApplyAsync(CostAllocationSource source, CreateUpdateCostAllocationSourceDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<CostCenter>(input.CostCenterCode);
        await CodeTableChecker.EnsureExistsAsync<CostObject>(input.CostObjectCode);
        await EnsureCostTypeAsync(LazyServiceProvider.LazyGetRequiredService<IRepository<CostType, Guid>>(), input.CreditToCostTypeNo);
        source.Set(input.Description, input.Level, input.CostTypeRange, input.CostCenterCode, input.CostObjectCode, input.CreditToCostTypeNo, input.Blocked);
    }

    internal static async Task EnsureCostTypeAsync(IRepository<CostType, Guid> types, string no)
    {
        var trimmed = no?.Trim();
        if (!await types.AnyAsync(t => t.No == trimmed))
        {
            throw new BusinessException(ErpErrorCodes.CostAccounting.CostTypeNotFound).WithData("no", trimmed ?? "");
        }
    }

    protected override async Task<IQueryable<CostAllocationSource>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.WithDetailsAsync(a => a.Targets);
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), a => a.Code.ToLower().Contains(filter) || (a.Description != null && a.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<CostAllocationSource> ApplyDefaultSorting(IQueryable<CostAllocationSource> query) => query.OrderBy(a => a.Level).ThenBy(a => a.Code);
}

[Authorize(ErpPermissions.CostAccounting.Default)]
public class CostAllocationTargetAppService
    : ErpCrudAppService<CostAllocationTarget, CostAllocationTargetDto, Guid, GetCostAllocationTargetListInput, CreateUpdateCostAllocationTargetDto, CreateUpdateCostAllocationTargetDto>,
        ICostAllocationTargetAppService
{
    private readonly IRepository<CostAllocationSource, Guid> _sources;

    public CostAllocationTargetAppService(IRepository<CostAllocationTarget, Guid> repository, IRepository<CostAllocationSource, Guid> sources)
        : base(repository)
    {
        _sources = sources;
        GetPolicyName = ErpPermissions.CostAccounting.Default;
        GetListPolicyName = ErpPermissions.CostAccounting.Default;
        CreatePolicyName = ErpPermissions.CostAccounting.Update;
        UpdatePolicyName = ErpPermissions.CostAccounting.Update;
        DeletePolicyName = ErpPermissions.CostAccounting.Update;
    }

    public override async Task<CostAllocationTargetDto> CreateAsync(CreateUpdateCostAllocationTargetDto input)
    {
        await CheckCreatePolicyAsync();
        await CheckAsync(input);
        var source = await _sources.GetAsync(input.CostAllocationSourceId, includeDetails: true);
        var target = source.AddTarget(GuidGenerator.Create(), input.CostTypeNo, input.CostCenterCode, input.CostObjectCode, input.Share);
        await _sources.UpdateAsync(source, autoSave: true);
        return await MapToGetOutputDtoAsync(target);
    }

    public override async Task<CostAllocationTargetDto> UpdateAsync(Guid id, CreateUpdateCostAllocationTargetDto input)
    {
        await CheckUpdatePolicyAsync();
        await CheckAsync(input);
        var target = await GetEntityByIdAsync(id);
        target.Set(input.CostTypeNo, input.CostCenterCode, input.CostObjectCode, input.Share);
        await Repository.UpdateAsync(target, autoSave: true);
        return await MapToGetOutputDtoAsync(target);
    }

    private async Task CheckAsync(CreateUpdateCostAllocationTargetDto input)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<CostCenter>(input.CostCenterCode);
        await codes.EnsureExistsAsync<CostObject>(input.CostObjectCode);
        await CostAllocationSourceAppService.EnsureCostTypeAsync(LazyServiceProvider.LazyGetRequiredService<IRepository<CostType, Guid>>(), input.CostTypeNo);
    }

    protected override async Task<IQueryable<CostAllocationTarget>> CreateFilteredQueryAsync(GetCostAllocationTargetListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.CostAllocationSourceId.HasValue, t => t.CostAllocationSourceId == input.CostAllocationSourceId.Value);
    }

    protected override IQueryable<CostAllocationTarget> ApplyDefaultSorting(IQueryable<CostAllocationTarget> query) => query.OrderBy(t => t.LineNo);
}

[Authorize(ErpPermissions.CostAccounting.Default)]
public class CostAccountingAppService : ErpAppService, ICostAccountingAppService
{
    private readonly CostAccountingManager _manager;

    public CostAccountingAppService(CostAccountingManager manager)
    {
        _manager = manager;
    }

    [Authorize(ErpPermissions.CostAccounting.Post)]
    public async Task<CostRegisterDto> TransferFromGeneralLedgerAsync()
    {
        var register = await _manager.TransferGLEntriesAsync(Clock.Now.Date);
        return register == null ? null : ObjectMapper.Map<CostRegister, CostRegisterDto>(register);
    }

    [Authorize(ErpPermissions.CostAccounting.Post)]
    public async Task<CostRegisterDto> AllocateAsync(AllocateCostsInput input)
    {
        var register = await _manager.AllocateAsync(input.FromLevel, input.ToLevel, input.PostingDate ?? Clock.Now.Date, input.DocumentNo.Trim().ToUpperInvariant());
        return register == null ? null : ObjectMapper.Map<CostRegister, CostRegisterDto>(register);
    }

    public async Task<List<CostBalanceDto>> GetBalancesAsync(GetCostBalancesInput input) =>
        (await _manager.GetBalancesAsync(input.FromDate, input.ToDate, input.ByCostObject))
            .Select(b => new CostBalanceDto
            {
                CostTypeNo = b.CostTypeNo,
                CostTypeName = b.CostTypeName,
                CostCenterCode = b.CostCenterCode,
                CostObjectCode = b.CostObjectCode,
                Amount = b.Amount,
            })
            .ToList();
}
