using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Inventory;

[Authorize(ErpPermissions.Warehouse.Default)]
public class BinAppService : ErpTableAppService<Bin, BinDto, GetBinListInput, CreateUpdateBinDto>, IBinAppService
{
    private WarehouseManager Warehouse => LazyServiceProvider.LazyGetRequiredService<WarehouseManager>();

    public BinAppService(IRepository<Bin, Guid> repository)
        : base(repository, ErpPermissions.Warehouse.Default) { }

    public override async Task<BinDto> CreateAsync(CreateUpdateBinDto input)
    {
        await CheckCreatePolicyAsync();
        var location = await Warehouse.FindLocationAsync(input.LocationCode);
        if (location == null || !location.BinMandatory)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.BinsNotUsed).WithData("locationCode", CodeTableEntity.NormalizeCode(input.LocationCode) ?? "");
        }

        var code = Bin.NormalizeCode(input.Code);
        if (await Repository.AnyAsync(b => b.LocationCode == location.Code && b.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeAlreadyExists).WithData("code", code);
        }

        var bin = new Bin(GuidGenerator.Create(), location.Code, code, input.Description);
        bin.Set(input.Description, input.Blocked);
        await Repository.InsertAsync(bin, autoSave: true);
        return await MapToGetOutputDtoAsync(bin);
    }

    /// <summary>Only the description and the block change; a bin is moved by making a new one.</summary>
    public override async Task<BinDto> UpdateAsync(Guid id, CreateUpdateBinDto input)
    {
        await CheckUpdatePolicyAsync();
        var bin = await GetEntityByIdAsync(id);
        bin.Set(input.Description, input.Blocked);
        await Repository.UpdateAsync(bin, autoSave: true);
        return await MapToGetOutputDtoAsync(bin);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var bin = await GetEntityByIdAsync(id);
        if ((await Warehouse.GetBinContentsAsync(bin.LocationCode, bin.Code)).Count > 0)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.BinHasContent).WithData("binCode", bin.Code);
        }

        await Repository.DeleteAsync(bin, autoSave: true);
    }

    public async Task<PagedResultDto<BinContentDto>> GetContentsAsync(GetBinContentListInput input)
    {
        Guid? itemId = null;
        if (!input.ItemNo.IsNullOrWhiteSpace())
        {
            var no = input.ItemNo.Trim();
            itemId = (await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == no))?.Id ?? Guid.Empty;
        }

        var contents = await Warehouse.GetBinContentsAsync(input.LocationCode, input.BinCode, itemId);
        var page = contents
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount > 0 ? input.MaxResultCount : 100)
            .Select(c => new BinContentDto { LocationCode = c.LocationCode, BinCode = c.BinCode, ItemNo = c.ItemNo, VariantCode = c.VariantCode, Quantity = c.Quantity })
            .ToList();
        return new PagedResultDto<BinContentDto>(contents.Count, page);
    }

    protected override async Task<IQueryable<Bin>> CreateFilteredQueryAsync(GetBinListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var location = CodeTableEntity.NormalizeCode(input.LocationCode);
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(location != null, b => b.LocationCode == location)
            .WhereIf(!filter.IsNullOrEmpty(), b => b.Code.ToLower().Contains(filter) || (b.Description != null && b.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<Bin> ApplyDefaultSorting(IQueryable<Bin> query) => query.OrderBy(b => b.LocationCode).ThenBy(b => b.Code);
}

[Authorize(ErpPermissions.Warehouse.Default)]
public class WarehouseEntryAppService : ErpReadOnlyAppService<WarehouseEntry, WarehouseEntryDto, Guid, GetWarehouseEntryListInput>, IWarehouseEntryAppService
{
    public WarehouseEntryAppService(IRepository<WarehouseEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Warehouse.Default;
        GetListPolicyName = ErpPermissions.Warehouse.Default;
    }

    protected override async Task<IQueryable<WarehouseEntry>> CreateFilteredQueryAsync(GetWarehouseEntryListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var location = CodeTableEntity.NormalizeCode(input.LocationCode);
        var bin = Bin.NormalizeCode(input.BinCode);
        var item = input.ItemNo?.Trim();
        return query
            .WhereIf(location != null, e => e.LocationCode == location)
            .WhereIf(bin != null, e => e.BinCode == bin)
            .WhereIf(!item.IsNullOrEmpty(), e => e.ItemNo == item);
    }

    protected override IQueryable<WarehouseEntry> ApplyDefaultSorting(IQueryable<WarehouseEntry> query) => query.OrderByDescending(e => e.EntryNo);
}

[Authorize(ErpPermissions.Warehouse.Default)]
public class WarehouseActivityAppService
    : ErpCrudAppService<WarehouseActivityHeader, WarehouseActivityDto, Guid, GetWarehouseActivityListInput, CreateUpdateWarehouseActivityDto, CreateUpdateWarehouseActivityDto>,
        IWarehouseActivityAppService
{
    private WarehouseManager Warehouse => LazyServiceProvider.LazyGetRequiredService<WarehouseManager>();

    public WarehouseActivityAppService(IRepository<WarehouseActivityHeader, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Warehouse.Default;
        GetListPolicyName = ErpPermissions.Warehouse.Default;
        CreatePolicyName = ErpPermissions.Warehouse.Create;
        UpdatePolicyName = ErpPermissions.Warehouse.Update;
        DeletePolicyName = ErpPermissions.Warehouse.Delete;
    }

    protected override async Task<WarehouseActivityHeader> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    public override async Task<WarehouseActivityDto> CreateAsync(CreateUpdateWarehouseActivityDto input)
    {
        await CheckCreatePolicyAsync();
        return await MapToGetOutputDtoAsync(await Warehouse.CreateMovementAsync(input.LocationCode, input.Description));
    }

    public override async Task<WarehouseActivityDto> UpdateAsync(Guid id, CreateUpdateWarehouseActivityDto input)
    {
        await CheckUpdatePolicyAsync();
        var header = await GetEntityByIdAsync(id);
        header.SetDescription(input.Description);
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    /// <summary>A registered document is the record of what moved; only an open one can be deleted.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var header = await GetEntityByIdAsync(id);
        header.EnsureOpen();
        await Repository.DeleteAsync(header, autoSave: true);
    }

    [Authorize(ErpPermissions.Warehouse.Create)]
    public async Task<WarehouseActivityDto> MakePutAwayAsync(MakeWarehouseDocumentInput input)
    {
        var no = input.SourceNo?.Trim();
        var receipt = await LazyServiceProvider.LazyGetRequiredService<IRepository<PostedPurchaseReceipt, Guid>>()
            .FirstOrDefaultAsync(r => r.No == no && !r.Return)
            ?? throw new BusinessException(ErpErrorCodes.Inventory.NothingToPutAway).WithData("documentNo", no ?? "");
        receipt = await LazyServiceProvider.LazyGetRequiredService<IRepository<PostedPurchaseReceipt, Guid>>().GetAsync(receipt.Id, includeDetails: true);
        return await MapToGetOutputDtoAsync(await Warehouse.CreatePutAwayAsync(receipt));
    }

    [Authorize(ErpPermissions.Warehouse.Create)]
    public async Task<WarehouseActivityDto> MakePickAsync(MakeWarehouseDocumentInput input)
    {
        var no = input.SourceNo?.Trim();
        var order = await LazyServiceProvider.LazyGetRequiredService<IRepository<SalesHeader, Guid>>()
            .FirstOrDefaultAsync(h => h.No == no && h.DocumentType == SalesDocumentType.Order)
            ?? throw new BusinessException(ErpErrorCodes.Inventory.NothingToPick).WithData("documentNo", no ?? "");
        return await MapToGetOutputDtoAsync(await Warehouse.CreatePickAsync(order));
    }

    [Authorize(ErpPermissions.Warehouse.Register)]
    public async Task<WarehouseActivityDto> RegisterAsync(Guid id, RegisterWarehouseActivityInput input) =>
        await MapToGetOutputDtoAsync(await Warehouse.RegisterAsync(id, input?.RegisteringDate ?? Clock.Now.Date));

    protected override async Task<IQueryable<WarehouseActivityHeader>> CreateFilteredQueryAsync(GetWarehouseActivityListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(input.Type.HasValue, h => h.Type == input.Type.Value)
            .WhereIf(input.Status.HasValue, h => h.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                h => h.No.ToLower().Contains(filter) || (h.SourceNo != null && h.SourceNo.ToLower().Contains(filter)) || (h.Description != null && h.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<WarehouseActivityHeader> ApplyDefaultSorting(IQueryable<WarehouseActivityHeader> query) => query.OrderByDescending(h => h.No);
}

[Authorize(ErpPermissions.Warehouse.Default)]
public class WarehouseActivityLineAppService
    : ErpCrudAppService<WarehouseActivityLine, WarehouseActivityLineDto, Guid, GetWarehouseActivityLineListInput, CreateUpdateWarehouseActivityLineDto, CreateUpdateWarehouseActivityLineDto>,
        IWarehouseActivityLineAppService
{
    private readonly IRepository<WarehouseActivityHeader, Guid> _headers;

    public WarehouseActivityLineAppService(IRepository<WarehouseActivityLine, Guid> repository, IRepository<WarehouseActivityHeader, Guid> headers)
        : base(repository)
    {
        _headers = headers;
        GetPolicyName = ErpPermissions.Warehouse.Default;
        GetListPolicyName = ErpPermissions.Warehouse.Default;
        CreatePolicyName = ErpPermissions.Warehouse.Update;
        UpdatePolicyName = ErpPermissions.Warehouse.Update;
        DeletePolicyName = ErpPermissions.Warehouse.Update;
    }

    /// <summary>Lines are added by hand to movements only; put-aways and picks get theirs from the source document.</summary>
    public override async Task<WarehouseActivityLineDto> CreateAsync(CreateUpdateWarehouseActivityLineDto input)
    {
        await CheckCreatePolicyAsync();
        var header = await _headers.GetAsync(input.WarehouseActivityHeaderId, includeDetails: true);
        header.EnsureOpen();
        var no = input.ItemNo?.Trim();
        var item = await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no ?? "");
        await LazyServiceProvider.LazyGetRequiredService<ItemUnitOfMeasureManager>().EnsureVariantAsync(item.No, input.VariantCode);
        var line = header.AddLine(GuidGenerator.Create(), item, input.VariantCode, input.Description, input.FromBinCode, input.ToBinCode, input.Quantity, null);
        if (input.QtyToHandle.HasValue)
        {
            line.SetQtyToHandle(header.No, input.QtyToHandle.Value);
        }

        await _headers.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<WarehouseActivityLineDto> UpdateAsync(Guid id, CreateUpdateWarehouseActivityLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        var header = await _headers.GetAsync(line.WarehouseActivityHeaderId);
        header.EnsureOpen();
        line.SetBins(input.FromBinCode, input.ToBinCode);
        if (header.Type == WarehouseActivityType.Movement && input.Quantity > 0m && input.Quantity != line.Quantity)
        {
            line.SetQuantity(header.No, input.Quantity);
        }

        line.SetQtyToHandle(header.No, input.QtyToHandle ?? line.QtyToHandle);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        var header = await _headers.GetAsync(line.WarehouseActivityHeaderId);
        header.EnsureOpen();
        if (line.QtyHandled != 0m)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.WarehouseDocumentRegistered).WithData("no", header.No);
        }

        await Repository.DeleteAsync(line, autoSave: true);
    }

    protected override async Task<IQueryable<WarehouseActivityLine>> CreateFilteredQueryAsync(GetWarehouseActivityLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.WarehouseActivityHeaderId.HasValue, l => l.WarehouseActivityHeaderId == input.WarehouseActivityHeaderId.Value);
    }

    protected override IQueryable<WarehouseActivityLine> ApplyDefaultSorting(IQueryable<WarehouseActivityLine> query) => query.OrderBy(l => l.LineNo);
}

[Authorize(ErpPermissions.Reservations.Default)]
public class ReservationEntryAppService : ErpReadOnlyAppService<ReservationEntry, ReservationEntryDto, Guid, GetReservationEntryListInput>, IReservationEntryAppService
{
    private ReservationManager Reservations => LazyServiceProvider.LazyGetRequiredService<ReservationManager>();

    public ReservationEntryAppService(IRepository<ReservationEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Reservations.Default;
        GetListPolicyName = ErpPermissions.Reservations.Default;
    }

    [Authorize(ErpPermissions.Reservations.Reserve)]
    public async Task<ReservationResultDto> ReserveSalesOrderAsync(ReserveDocumentInput input)
    {
        var no = input.DocumentNo?.Trim();
        var order = await LazyServiceProvider.LazyGetRequiredService<IRepository<SalesHeader, Guid>>()
            .FirstOrDefaultAsync(h => h.No == no && h.DocumentType == SalesDocumentType.Order)
            ?? throw new BusinessException(ErpErrorCodes.Documents.DocumentNotFound).WithData("documentNo", no ?? "");
        return new ReservationResultDto { LineCount = await Reservations.ReserveSalesOrderAsync(order) };
    }

    [Authorize(ErpPermissions.Reservations.Reserve)]
    public async Task<ReservationResultDto> ReserveAssemblyOrderAsync(ReserveDocumentInput input)
    {
        var no = input.DocumentNo?.Trim().ToUpperInvariant();
        var order = await LazyServiceProvider.LazyGetRequiredService<IRepository<AssemblyHeader, Guid>>().FirstOrDefaultAsync(h => h.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Documents.DocumentNotFound).WithData("documentNo", no ?? "");
        return new ReservationResultDto { LineCount = await LazyServiceProvider.LazyGetRequiredService<AssemblyManager>().ReserveComponentsAsync(order.Id) };
    }

    [Authorize(ErpPermissions.Reservations.Reserve)]
    public async Task CancelDocumentAsync(ReserveDocumentInput input)
    {
        var no = input.DocumentNo?.Trim();
        var ids = (await ReadOnlyRepository.GetListAsync(r => r.SourceDocumentNo == no)).Select(r => r.SourceId);
        await Reservations.CancelAsync(ids);
    }

    [Authorize(ErpPermissions.Reservations.Reserve)]
    public async Task CancelAsync(Guid id)
    {
        var entry = await GetEntityByIdAsync(id);
        await Reservations.CancelAsync([entry.SourceId]);
    }

    public async Task<ItemAvailabilityDto> GetAvailabilityAsync(GetItemAvailabilityInput input)
    {
        var no = input.ItemNo?.Trim();
        var item = await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no ?? "");
        var reserved = await Reservations.GetReservedAsync(item.Id, input.LocationCode, input.VariantCode);
        var available = await Reservations.GetAvailableToReserveAsync(item.Id, input.LocationCode, input.VariantCode);
        return new ItemAvailabilityDto
        {
            ItemNo = item.No,
            LocationCode = CodeTableEntity.NormalizeCode(input.LocationCode),
            VariantCode = CodeTableEntity.NormalizeCode(input.VariantCode),
            Inventory = available + reserved,
            Reserved = reserved,
            Available = available,
        };
    }

    protected override async Task<IQueryable<ReservationEntry>> CreateFilteredQueryAsync(GetReservationEntryListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var item = input.ItemNo?.Trim();
        var document = input.SourceDocumentNo?.Trim();
        return query
            .WhereIf(!item.IsNullOrEmpty(), r => r.ItemNo == item)
            .WhereIf(!document.IsNullOrEmpty(), r => r.SourceDocumentNo == document)
            .WhereIf(input.SourceId.HasValue, r => r.SourceId == input.SourceId.Value);
    }

    protected override IQueryable<ReservationEntry> ApplyDefaultSorting(IQueryable<ReservationEntry> query) =>
        query.OrderBy(r => r.ShipmentDate).ThenBy(r => r.SourceDocumentNo).ThenBy(r => r.SourceLineNo);
}

public class PhysInvtCountingPeriodAppService
    : CodeTableAppServiceBase<PhysInvtCountingPeriod, PhysInvtCountingPeriodDto, CreateUpdatePhysInvtCountingPeriodDto>,
        IPhysInvtCountingPeriodAppService
{
    public PhysInvtCountingPeriodAppService(IRepository<PhysInvtCountingPeriod, Guid> repository)
        : base(repository, ErpPermissions.PhysInventory.Default) { }

    protected override PhysInvtCountingPeriod NewEntity(Guid id, CreateUpdatePhysInvtCountingPeriodDto input) =>
        new(id, input.Code, input.Description, input.CountFrequencyPerYear);

    protected override Task ApplyAsync(PhysInvtCountingPeriod entity, CreateUpdatePhysInvtCountingPeriodDto input)
    {
        entity.SetFrequency(input.CountFrequencyPerYear);
        return Task.CompletedTask;
    }
}

[Authorize(ErpPermissions.PhysInventory.Default)]
public class PhysInvtOrderAppService
    : ErpCrudAppService<PhysInvtOrderHeader, PhysInvtOrderDto, Guid, GetPhysInvtOrderListInput, CreateUpdatePhysInvtOrderDto, CreateUpdatePhysInvtOrderDto>,
        IPhysInvtOrderAppService
{
    private PhysInvtOrderManager Manager => LazyServiceProvider.LazyGetRequiredService<PhysInvtOrderManager>();

    public PhysInvtOrderAppService(IRepository<PhysInvtOrderHeader, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.PhysInventory.Default;
        GetListPolicyName = ErpPermissions.PhysInventory.Default;
        CreatePolicyName = ErpPermissions.PhysInventory.Create;
        UpdatePolicyName = ErpPermissions.PhysInventory.Update;
        DeletePolicyName = ErpPermissions.PhysInventory.Delete;
    }

    protected override async Task<PhysInvtOrderHeader> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    public override async Task<PhysInvtOrderDto> CreateAsync(CreateUpdatePhysInvtOrderDto input)
    {
        await CheckCreatePolicyAsync();
        var no = input.No.IsNullOrWhiteSpace() ? await Manager.NextNoAsync() : input.No.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(o => o.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }

        var order = new PhysInvtOrderHeader(GuidGenerator.Create(), no);
        await ApplyAsync(order, input);
        await Repository.InsertAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    public override async Task<PhysInvtOrderDto> UpdateAsync(Guid id, CreateUpdatePhysInvtOrderDto input)
    {
        await CheckUpdatePolicyAsync();
        var order = await GetEntityByIdAsync(id);
        await ApplyAsync(order, input);
        await Repository.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var order = await GetEntityByIdAsync(id);
        order.EnsureOpen();
        await Repository.DeleteAsync(order, autoSave: true);
    }

    [Authorize(ErpPermissions.PhysInventory.Update)]
    public async Task<CalculatePhysInvtLinesResultDto> CalculateLinesAsync(Guid id, CalculatePhysInvtLinesInput input) =>
        new() { LinesAdded = await Manager.CalculateLinesAsync(id, input.OnlyItemsDueForCounting, input.ItemCategoryCode, input.IncludeItemsNotOnInventory) };

    [Authorize(ErpPermissions.PhysInventory.Post)]
    public async Task<PhysInvtOrderDto> FinishAsync(Guid id) => await MapToGetOutputDtoAsync(await Manager.FinishAsync(id));

    private async Task ApplyAsync(PhysInvtOrderHeader order, CreateUpdatePhysInvtOrderDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<Location>(input.LocationCode);
        var date = input.OrderDate ?? Clock.Now.Date;
        order.Set(input.Description, input.LocationCode, date, input.PostingDate ?? date);
    }

    protected override async Task<IQueryable<PhysInvtOrderHeader>> CreateFilteredQueryAsync(GetPhysInvtOrderListInput input)
    {
        var query = await Repository.WithDetailsAsync(o => o.Lines);
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(input.Status.HasValue, o => o.Status == input.Status.Value)
            .WhereIf(!filter.IsNullOrEmpty(), o => o.No.ToLower().Contains(filter) || (o.Description != null && o.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<PhysInvtOrderHeader> ApplyDefaultSorting(IQueryable<PhysInvtOrderHeader> query) => query.OrderByDescending(o => o.No);
}

[Authorize(ErpPermissions.PhysInventory.Default)]
public class PhysInvtOrderLineAppService
    : ErpCrudAppService<PhysInvtOrderLine, PhysInvtOrderLineDto, Guid, GetPhysInvtOrderLineListInput, CreateUpdatePhysInvtOrderLineDto, CreateUpdatePhysInvtOrderLineDto>,
        IPhysInvtOrderLineAppService
{
    private readonly IRepository<PhysInvtOrderHeader, Guid> _orders;

    public PhysInvtOrderLineAppService(IRepository<PhysInvtOrderLine, Guid> repository, IRepository<PhysInvtOrderHeader, Guid> orders)
        : base(repository)
    {
        _orders = orders;
        GetPolicyName = ErpPermissions.PhysInventory.Default;
        GetListPolicyName = ErpPermissions.PhysInventory.Default;
        CreatePolicyName = ErpPermissions.PhysInventory.Update;
        UpdatePolicyName = ErpPermissions.PhysInventory.Update;
        DeletePolicyName = ErpPermissions.PhysInventory.Update;
    }

    /// <summary>A line added by hand expects what the books hold of the item there, so the count shows the difference.</summary>
    public override async Task<PhysInvtOrderLineDto> CreateAsync(CreateUpdatePhysInvtOrderLineDto input)
    {
        await CheckCreatePolicyAsync();
        var order = await _orders.GetAsync(input.PhysInvtOrderHeaderId, includeDetails: true);
        order.EnsureOpen();
        var no = input.ItemNo?.Trim();
        var item = await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no ?? "");

        decimal expected;
        if (!input.BinCode.IsNullOrWhiteSpace())
        {
            var warehouse = LazyServiceProvider.LazyGetRequiredService<WarehouseManager>();
            var bin = await warehouse.GetBinAsync(order.LocationCode, input.BinCode);
            expected = await warehouse.GetQuantityInBinAsync(order.LocationCode, bin.Code, item.Id, input.VariantCode);
        }
        else
        {
            expected = await LazyServiceProvider.LazyGetRequiredService<ItemJnlPostLine>()
                .GetAvailableAsync(item.Id, order.LocationCode, input.VariantCode, input.LotNo.IsNullOrWhiteSpace() ? null : input.LotNo.Trim(), input.SerialNo.IsNullOrWhiteSpace() ? null : input.SerialNo.Trim());
        }

        var line = order.AddLine(GuidGenerator.Create(), item, input.VariantCode, input.BinCode, input.LotNo, input.SerialNo, expected);
        line.Record(input.QtyRecorded, input.Recorded);
        await _orders.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    /// <summary>Only the count changes; what the books expected stays as it was calculated.</summary>
    public override async Task<PhysInvtOrderLineDto> UpdateAsync(Guid id, CreateUpdatePhysInvtOrderLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        (await _orders.GetAsync(line.PhysInvtOrderHeaderId)).EnsureOpen();
        line.Record(input.QtyRecorded, input.Recorded);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        (await _orders.GetAsync(line.PhysInvtOrderHeaderId)).EnsureOpen();
        await Repository.DeleteAsync(line, autoSave: true);
    }

    protected override async Task<IQueryable<PhysInvtOrderLine>> CreateFilteredQueryAsync(GetPhysInvtOrderLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.PhysInvtOrderHeaderId.HasValue, l => l.PhysInvtOrderHeaderId == input.PhysInvtOrderHeaderId.Value);
    }

    protected override IQueryable<PhysInvtOrderLine> ApplyDefaultSorting(IQueryable<PhysInvtOrderLine> query) => query.OrderBy(l => l.LineNo);
}

[Authorize(ErpPermissions.Assembly.Default)]
public class AssemblyBomComponentAppService
    : ErpTableAppService<AssemblyBomComponent, AssemblyBomComponentDto, GetAssemblyBomComponentListInput, CreateUpdateAssemblyBomComponentDto>,
        IAssemblyBomComponentAppService
{
    private AssemblyManager Manager => LazyServiceProvider.LazyGetRequiredService<AssemblyManager>();

    public AssemblyBomComponentAppService(IRepository<AssemblyBomComponent, Guid> repository)
        : base(repository, ErpPermissions.Assembly.Default) { }

    public override async Task<AssemblyBomComponentDto> CreateAsync(CreateUpdateAssemblyBomComponentDto input)
    {
        await CheckCreatePolicyAsync();
        var parent = await Manager.GetItemAsync(input.ParentItemNo);
        var lines = await Repository.GetListAsync(c => c.ParentItemNo == parent.No);
        var component = new AssemblyBomComponent(GuidGenerator.Create(), parent.No, (lines.Count == 0 ? 0 : lines.Max(c => c.LineNo)) + 10000);
        await ApplyAsync(component, input);
        await Repository.InsertAsync(component, autoSave: true);
        return await MapToGetOutputDtoAsync(component);
    }

    public override async Task<AssemblyBomComponentDto> UpdateAsync(Guid id, CreateUpdateAssemblyBomComponentDto input)
    {
        await CheckUpdatePolicyAsync();
        var component = await GetEntityByIdAsync(id);
        await ApplyAsync(component, input);
        await Repository.UpdateAsync(component, autoSave: true);
        return await MapToGetOutputDtoAsync(component);
    }

    /// <summary>A component cannot itself be assembled from the parent, or the BOM would never end.</summary>
    private async Task ApplyAsync(AssemblyBomComponent component, CreateUpdateAssemblyBomComponentDto input)
    {
        var item = await Manager.GetItemAsync(input.ComponentItemNo);
        if (await Repository.AnyAsync(c => c.ParentItemNo == item.No && c.ComponentItemNo == component.ParentItemNo))
        {
            throw new BusinessException(ErpErrorCodes.Inventory.AssemblyBomRecursive).WithData("itemNo", item.No);
        }

        component.Set(item.No, input.Description ?? item.Description, input.QuantityPer);
    }

    protected override async Task<IQueryable<AssemblyBomComponent>> CreateFilteredQueryAsync(GetAssemblyBomComponentListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var parent = input.ParentItemNo?.Trim();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!parent.IsNullOrEmpty(), c => c.ParentItemNo == parent)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                c => c.ParentItemNo.ToLower().Contains(filter) || c.ComponentItemNo.ToLower().Contains(filter) || (c.Description != null && c.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<AssemblyBomComponent> ApplyDefaultSorting(IQueryable<AssemblyBomComponent> query) => query.OrderBy(c => c.ParentItemNo).ThenBy(c => c.LineNo);
}

[Authorize(ErpPermissions.Assembly.Default)]
public class AssemblyOrderAppService
    : ErpCrudAppService<AssemblyHeader, AssemblyOrderDto, Guid, GetAssemblyOrderListInput, CreateUpdateAssemblyOrderDto, CreateUpdateAssemblyOrderDto>,
        IAssemblyOrderAppService
{
    private AssemblyManager Manager => LazyServiceProvider.LazyGetRequiredService<AssemblyManager>();

    public AssemblyOrderAppService(IRepository<AssemblyHeader, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Assembly.Default;
        GetListPolicyName = ErpPermissions.Assembly.Default;
        CreatePolicyName = ErpPermissions.Assembly.Create;
        UpdatePolicyName = ErpPermissions.Assembly.Update;
        DeletePolicyName = ErpPermissions.Assembly.Delete;
    }

    protected override async Task<AssemblyHeader> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    public override async Task<AssemblyOrderDto> CreateAsync(CreateUpdateAssemblyOrderDto input)
    {
        await CheckCreatePolicyAsync();
        var item = await Manager.GetItemAsync(input.ItemNo);
        var no = input.No.IsNullOrWhiteSpace() ? await Manager.NextNoAsync() : input.No.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(o => o.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }

        var order = new AssemblyHeader(GuidGenerator.Create(), no, item);
        await ApplyAsync(order, input);
        order.SetQuantity(input.Quantity, input.QuantityToAssemble);
        await Manager.ExplodeBomAsync(order);
        await Repository.InsertAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    public override async Task<AssemblyOrderDto> UpdateAsync(Guid id, CreateUpdateAssemblyOrderDto input)
    {
        await CheckUpdatePolicyAsync();
        var order = await GetEntityByIdAsync(id);
        await ApplyAsync(order, input);
        order.SetQuantity(input.Quantity, input.QuantityToAssemble);
        await Repository.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var order = await GetEntityByIdAsync(id);
        if (order.AssembledQuantity != 0m)
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentAlreadyPosted).WithData("documentNo", order.No);
        }

        await LazyServiceProvider.LazyGetRequiredService<ReservationManager>().CancelAsync(order.Lines.Select(l => l.Id));
        await Repository.DeleteAsync(order, autoSave: true);
    }

    [Authorize(ErpPermissions.Assembly.Update)]
    public async Task<AssemblyOrderDto> ReleaseAsync(Guid id)
    {
        var order = await GetEntityByIdAsync(id);
        order.Release();
        await Repository.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    [Authorize(ErpPermissions.Assembly.Update)]
    public async Task<AssemblyOrderDto> ReopenAsync(Guid id)
    {
        var order = await GetEntityByIdAsync(id);
        order.Reopen();
        await Repository.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    /// <summary>Reads the BOM again, for when it changed after the order was made; only before anything is consumed.</summary>
    [Authorize(ErpPermissions.Assembly.Update)]
    public async Task<AssemblyOrderDto> RefreshLinesAsync(Guid id)
    {
        var order = await GetEntityByIdAsync(id);
        await LazyServiceProvider.LazyGetRequiredService<ReservationManager>().CancelAsync(order.Lines.Where(l => l.ConsumedQuantity == 0m).Select(l => l.Id));
        await Manager.ExplodeBomAsync(order);
        await Repository.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    [Authorize(ErpPermissions.Assembly.Post)]
    public async Task<AssemblyOrderDto> RunPostingAsync(Guid id) => await MapToGetOutputDtoAsync(await Manager.PostAsync(id));

    private async Task ApplyAsync(AssemblyHeader order, CreateUpdateAssemblyOrderDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<Location>(input.LocationCode);
        if (!input.BinCode.IsNullOrWhiteSpace())
        {
            await LazyServiceProvider.LazyGetRequiredService<WarehouseManager>().GetBinAsync(input.LocationCode, input.BinCode);
        }

        await LazyServiceProvider.LazyGetRequiredService<ItemUnitOfMeasureManager>().EnsureVariantAsync(order.ItemNo, input.VariantCode);
        var due = input.DueDate ?? Clock.Now.Date;
        order.Set(input.Description, input.VariantCode, input.LocationCode, input.BinCode, due, input.PostingDate ?? due);
    }

    protected override async Task<IQueryable<AssemblyHeader>> CreateFilteredQueryAsync(GetAssemblyOrderListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(input.Status.HasValue, o => o.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                o => o.No.ToLower().Contains(filter) || o.ItemNo.ToLower().Contains(filter) || (o.Description != null && o.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<AssemblyHeader> ApplyDefaultSorting(IQueryable<AssemblyHeader> query) => query.OrderByDescending(o => o.No);
}

[Authorize(ErpPermissions.Assembly.Default)]
public class AssemblyLineAppService : ErpReadOnlyAppService<AssemblyLine, AssemblyLineDto, Guid, GetAssemblyLineListInput>, IAssemblyLineAppService
{
    public AssemblyLineAppService(IRepository<AssemblyLine, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Assembly.Default;
        GetListPolicyName = ErpPermissions.Assembly.Default;
    }

    protected override async Task<IQueryable<AssemblyLine>> CreateFilteredQueryAsync(GetAssemblyLineListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        return query.WhereIf(input.AssemblyHeaderId.HasValue, l => l.AssemblyHeaderId == input.AssemblyHeaderId.Value);
    }

    protected override IQueryable<AssemblyLine> ApplyDefaultSorting(IQueryable<AssemblyLine> query) => query.OrderBy(l => l.LineNo);
}
