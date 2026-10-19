using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Purchasing;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Manufacturing;

[Authorize(ErpPermissions.ManufacturingSetup.Default)]
public class ManufacturingSetupAppService : ErpAppService, IManufacturingSetupAppService
{
    private readonly IRepository<ManufacturingSetup, Guid> _repository;

    public ManufacturingSetupAppService(IRepository<ManufacturingSetup, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ManufacturingSetupDto> GetAsync() => Map(await GetOrCreateAsync());

    [Authorize(ErpPermissions.ManufacturingSetup.Update)]
    public async Task<ManufacturingSetupDto> UpdateAsync(ManufacturingSetupDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<NoSeriesCodeValidator>().EnsureExistAsync(input.ProductionOrderNos);
        await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().EnsureGLAccountsExistAsync(input.WipAccountNo, input.CapacityAppliedAccountNo, input.VarianceAccountNo);
        var setup = await GetOrCreateAsync();
        setup.Set(input.ProductionOrderNos, input.WipAccountNo, input.CapacityAppliedAccountNo, input.VarianceAccountNo);
        await _repository.UpdateAsync(setup, autoSave: true);
        return Map(setup);
    }

    private async Task<ManufacturingSetup> GetOrCreateAsync() =>
        await _repository.FirstOrDefaultAsync() ?? await _repository.InsertAsync(new ManufacturingSetup(GuidGenerator.Create()), autoSave: true);

    private static ManufacturingSetupDto Map(ManufacturingSetup s) => new()
    {
        ProductionOrderNos = s.ProductionOrderNos,
        WipAccountNo = s.WipAccountNo,
        CapacityAppliedAccountNo = s.CapacityAppliedAccountNo,
        VarianceAccountNo = s.VarianceAccountNo,
    };
}

public class WorkCenterAppService : CodeTableAppServiceBase<WorkCenter, WorkCenterDto, CreateUpdateWorkCenterDto>, IWorkCenterAppService
{
    public WorkCenterAppService(IRepository<WorkCenter, Guid> repository)
        : base(repository, ErpPermissions.ManufacturingSetup.Default) { }

    protected override WorkCenter NewEntity(Guid id, CreateUpdateWorkCenterDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(WorkCenter entity, CreateUpdateWorkCenterDto input)
    {
        entity.Set(input.UnitCostPerHour, input.CapacityHoursPerDay);
        return Task.CompletedTask;
    }
}

public class MachineCenterAppService : CodeTableAppServiceBase<MachineCenter, MachineCenterDto, CreateUpdateMachineCenterDto>, IMachineCenterAppService
{
    public MachineCenterAppService(IRepository<MachineCenter, Guid> repository)
        : base(repository, ErpPermissions.ManufacturingSetup.Default) { }

    protected override MachineCenter NewEntity(Guid id, CreateUpdateMachineCenterDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(MachineCenter entity, CreateUpdateMachineCenterDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<WorkCenter>(input.WorkCenterCode);
        entity.Set(input.WorkCenterCode, input.UnitCostPerHour);
    }
}

[Authorize(ErpPermissions.Manufacturing.Default)]
public class ProductionBomAppService
    : ErpCrudAppService<ProductionBom, ProductionBomDto, Guid, GetCodeTableListInput, CreateUpdateProductionBomDto, CreateUpdateProductionBomDto>,
        IProductionBomAppService
{
    public ProductionBomAppService(IRepository<ProductionBom, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Manufacturing.Default;
        GetListPolicyName = ErpPermissions.Manufacturing.Default;
        CreatePolicyName = ErpPermissions.Manufacturing.Create;
        UpdatePolicyName = ErpPermissions.Manufacturing.Update;
        DeletePolicyName = ErpPermissions.Manufacturing.Delete;
    }

    protected override async Task<ProductionBom> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    public override async Task<ProductionBomDto> CreateAsync(CreateUpdateProductionBomDto input)
    {
        await CheckCreatePolicyAsync();
        var no = input.No.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(b => b.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.AlreadyExists).WithData("no", no);
        }

        var bom = new ProductionBom(GuidGenerator.Create(), no, input.Description);
        await Repository.InsertAsync(bom, autoSave: true);
        return await MapToGetOutputDtoAsync(bom);
    }

    public override async Task<ProductionBomDto> UpdateAsync(Guid id, CreateUpdateProductionBomDto input)
    {
        await CheckUpdatePolicyAsync();
        var bom = await GetEntityByIdAsync(id);
        bom.SetDescription(input.Description);
        bom.SetStatus(input.Status);
        await Repository.UpdateAsync(bom, autoSave: true);
        return await MapToGetOutputDtoAsync(bom);
    }

    protected override async Task<IQueryable<ProductionBom>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), b => b.No.ToLower().Contains(filter) || (b.Description != null && b.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<ProductionBom> ApplyDefaultSorting(IQueryable<ProductionBom> query) => query.OrderBy(b => b.No);
}

[Authorize(ErpPermissions.Manufacturing.Default)]
public class ProductionBomLineAppService
    : ErpCrudAppService<ProductionBomLine, ProductionBomLineDto, Guid, GetProductionBomLineListInput, CreateUpdateProductionBomLineDto, CreateUpdateProductionBomLineDto>,
        IProductionBomLineAppService
{
    private readonly IRepository<ProductionBom, Guid> _boms;

    public ProductionBomLineAppService(IRepository<ProductionBomLine, Guid> repository, IRepository<ProductionBom, Guid> boms)
        : base(repository)
    {
        _boms = boms;
        GetPolicyName = ErpPermissions.Manufacturing.Default;
        GetListPolicyName = ErpPermissions.Manufacturing.Default;
        CreatePolicyName = ErpPermissions.Manufacturing.Update;
        UpdatePolicyName = ErpPermissions.Manufacturing.Update;
        DeletePolicyName = ErpPermissions.Manufacturing.Update;
    }

    public override async Task<ProductionBomLineDto> CreateAsync(CreateUpdateProductionBomLineDto input)
    {
        await CheckCreatePolicyAsync();
        var bom = await _boms.GetAsync(input.ProductionBomId, includeDetails: true);
        bom.EnsureEditable();
        var line = new ProductionBomLine(GuidGenerator.Create(), bom.Id, (bom.Lines.Count == 0 ? 0 : bom.Lines.Max(l => l.LineNo)) + 10000);
        await ApplyAsync(line, input);
        await Repository.InsertAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<ProductionBomLineDto> UpdateAsync(Guid id, CreateUpdateProductionBomLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        (await _boms.GetAsync(line.ProductionBomId)).EnsureEditable();
        await ApplyAsync(line, input);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        (await _boms.GetAsync(line.ProductionBomId)).EnsureEditable();
        await Repository.DeleteAsync(line);
    }

    private async Task ApplyAsync(ProductionBomLine line, CreateUpdateProductionBomLineDto input)
    {
        var no = input.No?.Trim();
        var description = input.Description;
        if (input.Type == ProductionBomLineType.Item)
        {
            var item = await LazyServiceProvider.LazyGetRequiredService<ProductionManager>().GetItemAsync(no);
            description ??= item.Description;
        }
        else if (!await _boms.AnyAsync(b => b.No == no.ToUpper()))
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.BomNotFound).WithData("no", no ?? "");
        }

        line.Set(input.Type, no, description, input.QuantityPer, input.ScrapPct);
    }

    protected override async Task<IQueryable<ProductionBomLine>> CreateFilteredQueryAsync(GetProductionBomLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.ProductionBomId.HasValue, l => l.ProductionBomId == input.ProductionBomId.Value);
    }

    protected override IQueryable<ProductionBomLine> ApplyDefaultSorting(IQueryable<ProductionBomLine> query) => query.OrderBy(l => l.LineNo);
}

[Authorize(ErpPermissions.Manufacturing.Default)]
public class RoutingAppService
    : ErpCrudAppService<Routing, RoutingDto, Guid, GetCodeTableListInput, CreateUpdateRoutingDto, CreateUpdateRoutingDto>,
        IRoutingAppService
{
    public RoutingAppService(IRepository<Routing, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Manufacturing.Default;
        GetListPolicyName = ErpPermissions.Manufacturing.Default;
        CreatePolicyName = ErpPermissions.Manufacturing.Create;
        UpdatePolicyName = ErpPermissions.Manufacturing.Update;
        DeletePolicyName = ErpPermissions.Manufacturing.Delete;
    }

    public override async Task<RoutingDto> CreateAsync(CreateUpdateRoutingDto input)
    {
        await CheckCreatePolicyAsync();
        var no = input.No.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(r => r.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.AlreadyExists).WithData("no", no);
        }

        var routing = new Routing(GuidGenerator.Create(), no, input.Description);
        routing.SetStatus(input.Status);
        await Repository.InsertAsync(routing, autoSave: true);
        return await MapToGetOutputDtoAsync(routing);
    }

    public override async Task<RoutingDto> UpdateAsync(Guid id, CreateUpdateRoutingDto input)
    {
        await CheckUpdatePolicyAsync();
        var routing = await GetEntityByIdAsync(id);
        routing.SetDescription(input.Description);
        routing.SetStatus(input.Status);
        await Repository.UpdateAsync(routing, autoSave: true);
        return await MapToGetOutputDtoAsync(routing);
    }

    protected override async Task<IQueryable<Routing>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), r => r.No.ToLower().Contains(filter) || (r.Description != null && r.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<Routing> ApplyDefaultSorting(IQueryable<Routing> query) => query.OrderBy(r => r.No);
}

[Authorize(ErpPermissions.Manufacturing.Default)]
public class RoutingLineAppService
    : ErpCrudAppService<RoutingLine, RoutingLineDto, Guid, GetRoutingLineListInput, CreateUpdateRoutingLineDto, CreateUpdateRoutingLineDto>,
        IRoutingLineAppService
{
    public RoutingLineAppService(IRepository<RoutingLine, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Manufacturing.Default;
        GetListPolicyName = ErpPermissions.Manufacturing.Default;
        CreatePolicyName = ErpPermissions.Manufacturing.Update;
        UpdatePolicyName = ErpPermissions.Manufacturing.Update;
        DeletePolicyName = ErpPermissions.Manufacturing.Update;
    }

    public override async Task<RoutingLineDto> CreateAsync(CreateUpdateRoutingLineDto input)
    {
        await CheckCreatePolicyAsync();
        await LazyServiceProvider.LazyGetRequiredService<IRepository<Routing, Guid>>().GetAsync(input.RoutingId);
        var line = new RoutingLine(GuidGenerator.Create(), input.RoutingId);
        await ApplyAsync(line, input);
        await Repository.InsertAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<RoutingLineDto> UpdateAsync(Guid id, CreateUpdateRoutingLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        await ApplyAsync(line, input);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    private async Task ApplyAsync(RoutingLine line, CreateUpdateRoutingLineDto input)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        if (input.Type == RoutingLineType.MachineCenter)
        {
            await codes.EnsureExistsAsync<MachineCenter>(input.No);
        }
        else
        {
            await codes.EnsureExistsAsync<WorkCenter>(input.No);
        }

        line.Set(input.OperationNo, input.Type, input.No, input.Description, input.SetupTime, input.RunTime);
    }

    protected override async Task<IQueryable<RoutingLine>> CreateFilteredQueryAsync(GetRoutingLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.RoutingId.HasValue, l => l.RoutingId == input.RoutingId.Value);
    }

    protected override IQueryable<RoutingLine> ApplyDefaultSorting(IQueryable<RoutingLine> query) => query.OrderBy(l => l.OperationNo);
}

[Authorize(ErpPermissions.Manufacturing.Default)]
public class ProductionOrderAppService
    : ErpCrudAppService<ProductionOrder, ProductionOrderDto, Guid, GetProductionOrderListInput, CreateUpdateProductionOrderDto, CreateUpdateProductionOrderDto>,
        IProductionOrderAppService
{
    private ProductionManager Manager => LazyServiceProvider.LazyGetRequiredService<ProductionManager>();

    public ProductionOrderAppService(IRepository<ProductionOrder, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Manufacturing.Default;
        GetListPolicyName = ErpPermissions.Manufacturing.Default;
        CreatePolicyName = ErpPermissions.Manufacturing.Create;
        UpdatePolicyName = ErpPermissions.Manufacturing.Update;
        DeletePolicyName = ErpPermissions.Manufacturing.Delete;
    }

    protected override async Task<ProductionOrder> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    public override async Task<ProductionOrderDto> CreateAsync(CreateUpdateProductionOrderDto input)
    {
        await CheckCreatePolicyAsync();
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<Inventory.Location>(input.LocationCode);
        var order = await Manager.CreateAsync(input.No, input.Status, input.SourceItemNo, input.Quantity, input.DueDate == default ? Clock.Now.Date : input.DueDate, input.LocationCode);
        return await MapToGetOutputDtoAsync(order);
    }

    public override async Task<ProductionOrderDto> UpdateAsync(Guid id, CreateUpdateProductionOrderDto input)
    {
        await CheckUpdatePolicyAsync();
        var order = await GetEntityByIdAsync(id);
        var quantityChanged = order.Quantity != input.Quantity;
        order.Set(input.Description ?? order.Description, input.Quantity, input.DueDate == default ? order.DueDate : input.DueDate, input.LocationCode);
        await Repository.UpdateAsync(order, autoSave: true);
        if (quantityChanged)
        {
            order = await Manager.RefreshAsync(id);
        }

        return await MapToGetOutputDtoAsync(order);
    }

    [Authorize(ErpPermissions.Manufacturing.Update)]
    public async Task<ProductionOrderDto> RefreshAsync(Guid id) => await MapToGetOutputDtoAsync(await Manager.RefreshAsync(id));

    [Authorize(ErpPermissions.Manufacturing.Update)]
    public async Task<ProductionOrderDto> ChangeStatusAsync(Guid id, ChangeProductionOrderStatusInput input) =>
        await MapToGetOutputDtoAsync(await Manager.ChangeStatusAsync(id, input.Status));

    [Authorize(ErpPermissions.Manufacturing.Post)]
    public async Task<ProductionOrderDto> ConsumeAsync(Guid id, PostConsumptionInput input) =>
        await MapToGetOutputDtoAsync(await Manager.PostConsumptionAsync(id, input.Lines.Select(l => new ConsumptionRequest(l.ComponentId, l.Quantity)).ToList(), Date(input.PostingDate)));

    [Authorize(ErpPermissions.Manufacturing.Post)]
    public async Task<ProductionOrderDto> ReportOutputAsync(Guid id, PostOutputInput input) =>
        await MapToGetOutputDtoAsync(await Manager.PostOutputAsync(id, input.Quantity, input.Capacity.Select(c => new CapacityRequest(c.OperationNo, c.Minutes)).ToList(), Date(input.PostingDate)));

    [Authorize(ErpPermissions.Manufacturing.Post)]
    public async Task<ProductionOrderDto> FinishAsync(Guid id, FinishProductionOrderInput input) =>
        await MapToGetOutputDtoAsync(await Manager.FinishAsync(id, Date(input.PostingDate)));

    private DateTime Date(DateTime date) => date == default ? Clock.Now.Date : date;

    protected override async Task<IQueryable<ProductionOrder>> CreateFilteredQueryAsync(GetProductionOrderListInput input)
    {
        var query = await Repository.WithDetailsAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(input.Status.HasValue, o => o.Status == input.Status.Value)
            .WhereIf(!filter.IsNullOrEmpty(), o => o.No.ToLower().Contains(filter) || o.SourceItemNo.ToLower().Contains(filter));
    }

    protected override IQueryable<ProductionOrder> ApplyDefaultSorting(IQueryable<ProductionOrder> query) => query.OrderByDescending(o => o.CreationTime);
}

[Authorize(ErpPermissions.Manufacturing.Default)]
public class CapacityLedgerEntryAppService : ErpReadOnlyAppService<CapacityLedgerEntry, CapacityLedgerEntryDto, Guid, GetCapacityLedgerListInput>, ICapacityLedgerEntryAppService
{
    public CapacityLedgerEntryAppService(IRepository<CapacityLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Manufacturing.Default;
        GetListPolicyName = ErpPermissions.Manufacturing.Default;
    }

    protected override async Task<IQueryable<CapacityLedgerEntry>> CreateFilteredQueryAsync(GetCapacityLedgerListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        return query.WhereIf(!input.ProductionOrderNo.IsNullOrWhiteSpace(), e => e.ProductionOrderNo == input.ProductionOrderNo);
    }

    protected override IQueryable<CapacityLedgerEntry> ApplyDefaultSorting(IQueryable<CapacityLedgerEntry> query) => query.OrderByDescending(e => e.EntryNo);
}

/// <summary>The planning worksheet: calculating it, changing its suggestions and carrying them out.</summary>
[Authorize(ErpPermissions.Manufacturing.Default)]
public class PlanningAppService : ErpAppService, IPlanningAppService
{
    private readonly IRepository<PlanningLine, Guid> _repository;
    private readonly PlanningManager _planning;

    public PlanningAppService(IRepository<PlanningLine, Guid> repository, PlanningManager planning)
    {
        _repository = repository;
        _planning = planning;
    }

    public async Task<PagedResultDto<PlanningLineDto>> GetListAsync(ErpPagedListInput input)
    {
        var query = ErpListQuery.Filter(await _repository.GetQueryableAsync(), input, Clock.Now, null);
        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(l => l.LowLevelCode).ThenBy(l => l.ItemNo).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<PlanningLineDto>(total, ObjectMapper.Map<List<PlanningLine>, List<PlanningLineDto>>(items));
    }

    public async Task<PlanningLineDto> GetAsync(Guid id) => ObjectMapper.Map<PlanningLine, PlanningLineDto>(await _repository.GetAsync(id));

    [Authorize(ErpPermissions.Manufacturing.Update)]
    public async Task<PlanningLineDto> UpdateAsync(Guid id, UpdatePlanningLineDto input)
    {
        var line = await _repository.GetAsync(id);
        line.Set(input.Quantity, input.DueDate == default ? line.DueDate : input.DueDate, input.AcceptActionMessage);
        line.SetVendor(input.VendorNo);
        await _repository.UpdateAsync(line, autoSave: true);
        return ObjectMapper.Map<PlanningLine, PlanningLineDto>(line);
    }

    [Authorize(ErpPermissions.Manufacturing.Update)]
    public Task DeleteAsync(Guid id) => _repository.DeleteAsync(id);

    [Authorize(ErpPermissions.Manufacturing.Update)]
    public Task<int> CalculateAsync(CalculatePlanInput input) => _planning.CalculateAsync(input.ToDate == default ? Clock.Now.Date.AddDays(30) : input.ToDate);

    [Authorize(ErpPermissions.Manufacturing.Post)]
    public async Task<CarryOutResultDto> CarryOutAsync()
    {
        var lines = (await _repository.GetListAsync(l => l.AcceptActionMessage && l.CarriedOutNo == null && l.Quantity > 0m)).ToList();
        var result = new CarryOutResultDto();

        foreach (var line in lines.Where(l => l.ReplenishmentSystem == ReplenishmentSystem.ProdOrder))
        {
            var order = await LazyServiceProvider.LazyGetRequiredService<ProductionManager>()
                .CreateAsync(null, ProductionOrderStatus.FirmPlanned, line.ItemNo, line.Quantity, line.DueDate, line.LocationCode);
            await _planning.MarkCarriedOutAsync(line, order.No);
            result.ProductionOrderNos.Add(order.No);
        }

        var purchases = LazyServiceProvider.LazyGetRequiredService<IPurchaseDocumentAppService>();
        var vendors = LazyServiceProvider.LazyGetRequiredService<IRepository<Vendor, Guid>>();
        foreach (var group in lines.Where(l => l.ReplenishmentSystem == ReplenishmentSystem.Purchase).GroupBy(l => l.VendorNo))
        {
            if (group.Key == null)
            {
                throw new BusinessException(ErpErrorCodes.Manufacturing.NoVendor).WithData("no", group.First().ItemNo);
            }

            var vendor = await vendors.FirstOrDefaultAsync(v => v.No == group.Key)
                ?? throw new BusinessException(ErpErrorCodes.Vendors.VendorNotFound).WithData("accountNo", group.Key);
            var order = await purchases.CreateAsync(new CreateUpdatePurchaseHeaderDto
            {
                DocumentType = PurchaseDocumentType.Order,
                VendorId = vendor.Id,
                PostingDate = group.Min(l => l.DueDate),
                Lines = group.Select(l => new PurchaseLineInputDto { Type = DocumentLineType.Item, No = l.ItemNo, Description = l.Description, Quantity = l.Quantity }).ToList(),
            });
            foreach (var line in group)
            {
                await _planning.MarkCarriedOutAsync(line, order.No);
            }

            result.PurchaseOrderNos.Add(order.No);
        }

        return result;
    }
}
