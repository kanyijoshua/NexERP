using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>The rows that belong to one item: listed by item, changed with the item's permissions.</summary>
public abstract class ItemChildAppService<TEntity, TDto, TInput>
    : ErpCrudAppService<TEntity, TDto, Guid, GetItemChildListInput, TInput, TInput>
    where TEntity : CompanyEntity
    where TDto : Volo.Abp.Application.Dtos.IEntityDto<Guid>
{
    protected ItemChildAppService(IRepository<TEntity, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Items.Default;
        GetListPolicyName = ErpPermissions.Items.Default;
        CreatePolicyName = ErpPermissions.Items.Update;
        UpdatePolicyName = ErpPermissions.Items.Update;
        DeletePolicyName = ErpPermissions.Items.Update;
    }

    protected IRepository<Item, Guid> Items => LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>();

    /// <summary>The item a row belongs to has to exist.</summary>
    protected async Task<Item> GetItemAsync(string itemNo)
    {
        var no = itemNo?.Trim();
        return await Items.FirstOrDefaultAsync(i => i.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no ?? "");
    }
}

[Authorize(ErpPermissions.Items.Default)]
public class ItemVariantAppService : ItemChildAppService<ItemVariant, ItemVariantDto, CreateUpdateItemVariantDto>, IItemVariantAppService
{
    public ItemVariantAppService(IRepository<ItemVariant, Guid> repository)
        : base(repository) { }

    public override async Task<ItemVariantDto> CreateAsync(CreateUpdateItemVariantDto input)
    {
        await CheckCreatePolicyAsync();
        var item = await GetItemAsync(input.ItemNo);
        await EnsureUniqueAsync(item.No, input.Code, null);

        var variant = new ItemVariant(GuidGenerator.Create(), item.No, input.Code);
        variant.SetDetails(input.Description, input.Blocked);
        await Repository.InsertAsync(variant, autoSave: true);
        return await MapToGetOutputDtoAsync(variant);
    }

    public override async Task<ItemVariantDto> UpdateAsync(Guid id, CreateUpdateItemVariantDto input)
    {
        await CheckUpdatePolicyAsync();
        var variant = await GetEntityByIdAsync(id);
        await EnsureUniqueAsync(variant.ItemNo, input.Code, id);

        variant.SetCode(input.Code);
        variant.SetDetails(input.Description, input.Blocked);
        await Repository.UpdateAsync(variant, autoSave: true);
        return await MapToGetOutputDtoAsync(variant);
    }

    private async Task EnsureUniqueAsync(string itemNo, string code, Guid? id)
    {
        var normalized = code?.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(v => v.ItemNo == itemNo && v.Code == normalized && v.Id != id))
        {
            throw new BusinessException(ErpErrorCodes.Items.CodeAlreadyExists).WithData("code", normalized);
        }
    }

    protected override async Task<IQueryable<ItemVariant>> CreateFilteredQueryAsync(GetItemChildListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!input.ItemNo.IsNullOrWhiteSpace(), v => v.ItemNo == input.ItemNo)
            .WhereIf(!filter.IsNullOrEmpty(), v => v.Code.ToLower().Contains(filter) || (v.Description != null && v.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<ItemVariant> ApplyDefaultSorting(IQueryable<ItemVariant> query) => query.OrderBy(v => v.ItemNo).ThenBy(v => v.Code);
}

[Authorize(ErpPermissions.Items.Default)]
public class ItemUnitOfMeasureAppService
    : ItemChildAppService<ItemUnitOfMeasure, ItemUnitOfMeasureDto, CreateUpdateItemUnitOfMeasureDto>,
        IItemUnitOfMeasureAppService
{
    public ItemUnitOfMeasureAppService(IRepository<ItemUnitOfMeasure, Guid> repository)
        : base(repository) { }

    public override async Task<ItemUnitOfMeasureDto> CreateAsync(CreateUpdateItemUnitOfMeasureDto input)
    {
        await CheckCreatePolicyAsync();
        var item = await GetItemAsync(input.ItemNo);
        var code = input.Code?.Trim().ToUpperInvariant();
        if (!await LazyServiceProvider.LazyGetRequiredService<IRepository<UnitOfMeasure, Guid>>().AnyAsync(u => u.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.Inventory.UnitOfMeasureNotFound).WithData("itemNo", item.No).WithData("unitOfMeasureCode", code ?? "");
        }
        await EnsureUniqueAsync(item.No, input.Code, null);

        var unit = new ItemUnitOfMeasure(GuidGenerator.Create(), item.No, input.Code, input.QtyPerUnitOfMeasure);
        await Repository.InsertAsync(unit, autoSave: true);
        return await MapToGetOutputDtoAsync(unit);
    }

    public override async Task<ItemUnitOfMeasureDto> UpdateAsync(Guid id, CreateUpdateItemUnitOfMeasureDto input)
    {
        await CheckUpdatePolicyAsync();
        var unit = await GetEntityByIdAsync(id);
        await EnsureUniqueAsync(unit.ItemNo, input.Code, id);

        unit.SetCode(input.Code);
        unit.SetQtyPer(input.QtyPerUnitOfMeasure);
        await Repository.UpdateAsync(unit, autoSave: true);
        return await MapToGetOutputDtoAsync(unit);
    }

    private async Task EnsureUniqueAsync(string itemNo, string code, Guid? id)
    {
        var normalized = code?.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(u => u.ItemNo == itemNo && u.Code == normalized && u.Id != id))
        {
            throw new BusinessException(ErpErrorCodes.Items.CodeAlreadyExists).WithData("code", normalized);
        }
    }

    protected override async Task<IQueryable<ItemUnitOfMeasure>> CreateFilteredQueryAsync(GetItemChildListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(!input.ItemNo.IsNullOrWhiteSpace(), u => u.ItemNo == input.ItemNo);
    }

    protected override IQueryable<ItemUnitOfMeasure> ApplyDefaultSorting(IQueryable<ItemUnitOfMeasure> query) => query.OrderBy(u => u.ItemNo).ThenBy(u => u.QtyPerUnitOfMeasure);
}

/// <summary>Item tracking codes: whether items are tracked by lot, by serial number, or both.</summary>
public class ItemTrackingCodeAppService
    : CodeTableAppServiceBase<ItemTrackingCode, ItemTrackingCodeDto, CreateUpdateItemTrackingCodeDto>,
        IItemTrackingCodeAppService
{
    public ItemTrackingCodeAppService(IRepository<ItemTrackingCode, Guid> repository)
        : base(repository, ErpPermissions.InventorySetup.Default) { }

    protected override ItemTrackingCode NewEntity(Guid id, CreateUpdateItemTrackingCodeDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(ItemTrackingCode entity, CreateUpdateItemTrackingCodeDto input)
    {
        entity.SetRules(input.LotSpecific, input.SerialSpecific, input.ExpirationDateRequired);
        return Task.CompletedTask;
    }
}

[Authorize(ErpPermissions.Items.Default)]
public class LotNoInformationAppService
    : ItemChildAppService<LotNoInformation, LotNoInformationDto, CreateUpdateLotNoInformationDto>,
        ILotNoInformationAppService
{
    public LotNoInformationAppService(IRepository<LotNoInformation, Guid> repository)
        : base(repository) { }

    public override async Task<LotNoInformationDto> CreateAsync(CreateUpdateLotNoInformationDto input)
    {
        await CheckCreatePolicyAsync();
        var item = await GetItemAsync(input.ItemNo);
        var lot = new LotNoInformation(GuidGenerator.Create(), item.No, input.VariantCode, input.LotNo);
        lot.SetDetails(input.Description, input.ExpirationDate, input.Blocked);
        await Repository.InsertAsync(lot, autoSave: true);
        return await MapToGetOutputDtoAsync(lot);
    }

    public override async Task<LotNoInformationDto> UpdateAsync(Guid id, CreateUpdateLotNoInformationDto input)
    {
        await CheckUpdatePolicyAsync();
        var lot = await GetEntityByIdAsync(id);
        lot.SetDetails(input.Description, input.ExpirationDate, input.Blocked);
        await Repository.UpdateAsync(lot, autoSave: true);
        return await MapToGetOutputDtoAsync(lot);
    }

    protected override async Task<IQueryable<LotNoInformation>> CreateFilteredQueryAsync(GetItemChildListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!input.ItemNo.IsNullOrWhiteSpace(), l => l.ItemNo == input.ItemNo)
            .WhereIf(!filter.IsNullOrEmpty(), l => l.LotNo.ToLower().Contains(filter));
    }
}

[Authorize(ErpPermissions.Items.Default)]
public class SerialNoInformationAppService
    : ItemChildAppService<SerialNoInformation, SerialNoInformationDto, CreateUpdateSerialNoInformationDto>,
        ISerialNoInformationAppService
{
    public SerialNoInformationAppService(IRepository<SerialNoInformation, Guid> repository)
        : base(repository) { }

    public override async Task<SerialNoInformationDto> CreateAsync(CreateUpdateSerialNoInformationDto input)
    {
        await CheckCreatePolicyAsync();
        var item = await GetItemAsync(input.ItemNo);
        var serial = new SerialNoInformation(GuidGenerator.Create(), item.No, input.VariantCode, input.SerialNo);
        serial.SetDetails(input.Description, input.Blocked);
        await Repository.InsertAsync(serial, autoSave: true);
        return await MapToGetOutputDtoAsync(serial);
    }

    public override async Task<SerialNoInformationDto> UpdateAsync(Guid id, CreateUpdateSerialNoInformationDto input)
    {
        await CheckUpdatePolicyAsync();
        var serial = await GetEntityByIdAsync(id);
        serial.SetDetails(input.Description, input.Blocked);
        await Repository.UpdateAsync(serial, autoSave: true);
        return await MapToGetOutputDtoAsync(serial);
    }

    protected override async Task<IQueryable<SerialNoInformation>> CreateFilteredQueryAsync(GetItemChildListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!input.ItemNo.IsNullOrWhiteSpace(), s => s.ItemNo == input.ItemNo)
            .WhereIf(!filter.IsNullOrEmpty(), s => s.SerialNo.ToLower().Contains(filter));
    }
}

/// <summary>
/// The lots and serial numbers of a document, journal or transfer line. Anyone who can see items
/// can enter them; the posting of the line checks them against the item's tracking code.
/// </summary>
[Authorize(ErpPermissions.Items.Default)]
public class ItemTrackingLineAppService
    : ErpCrudAppService<ItemTrackingLine, ItemTrackingLineDto, Guid, GetItemTrackingLineListInput, CreateUpdateItemTrackingLineDto, CreateUpdateItemTrackingLineDto>,
        IItemTrackingLineAppService
{
    public ItemTrackingLineAppService(IRepository<ItemTrackingLine, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Items.Default;
        GetListPolicyName = ErpPermissions.Items.Default;
        CreatePolicyName = ErpPermissions.Items.Default;
        UpdatePolicyName = ErpPermissions.Items.Default;
        DeletePolicyName = ErpPermissions.Items.Default;
    }

    public override async Task<ItemTrackingLineDto> CreateAsync(CreateUpdateItemTrackingLineDto input)
    {
        await CheckCreatePolicyAsync();
        var line = new ItemTrackingLine(GuidGenerator.Create(), input.SourceType, input.SourceId, input.ItemNo.Trim());
        await ApplyAsync(line, input);
        await Repository.InsertAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<ItemTrackingLineDto> UpdateAsync(Guid id, CreateUpdateItemTrackingLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        await ApplyAsync(line, input);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    private async Task ApplyAsync(ItemTrackingLine line, CreateUpdateItemTrackingLineDto input)
    {
        line.SetTracking(input.LotNo, input.SerialNo, input.Quantity, input.ExpirationDate);

        var item = await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == line.ItemNo)
            ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", line.ItemNo);
        var code = await LazyServiceProvider.LazyGetRequiredService<ItemTrackingManager>().GetCodeAsync(item);
        if (code != null)
        {
            ItemTrackingManager.Validate(item, code, line);
        }
    }

    protected override async Task<IQueryable<ItemTrackingLine>> CreateFilteredQueryAsync(GetItemTrackingLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query
            .WhereIf(input.SourceType.HasValue, l => l.SourceType == input.SourceType.Value)
            .WhereIf(input.SourceId.HasValue, l => l.SourceId == input.SourceId.Value)
            .WhereIf(!input.ItemNo.IsNullOrWhiteSpace(), l => l.ItemNo == input.ItemNo);
    }

    protected override IQueryable<ItemTrackingLine> ApplyDefaultSorting(IQueryable<ItemTrackingLine> query) =>
        query.OrderBy(l => l.CreationTime);
}

[Authorize(ErpPermissions.ItemJournals.Default)]
public class ItemJournalTemplateAppService
    : ErpCrudAppService<ItemJournalTemplate, ItemJournalTemplateDto, Guid, GetCodeTableListInput, CreateUpdateItemJournalTemplateDto, CreateUpdateItemJournalTemplateDto>,
        IItemJournalTemplateAppService
{
    public ItemJournalTemplateAppService(IRepository<ItemJournalTemplate, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.ItemJournals.Default;
        GetListPolicyName = ErpPermissions.ItemJournals.Default;
        CreatePolicyName = ErpPermissions.InventorySetup.Update;
        UpdatePolicyName = ErpPermissions.InventorySetup.Update;
        DeletePolicyName = ErpPermissions.InventorySetup.Update;
    }

    public override async Task<ItemJournalTemplateDto> CreateAsync(CreateUpdateItemJournalTemplateDto input)
    {
        await CheckCreatePolicyAsync();
        var template = new ItemJournalTemplate(GuidGenerator.Create(), input.Name, input.Type);
        template.SetDescription(input.Description);
        await Repository.InsertAsync(template, autoSave: true);
        return await MapToGetOutputDtoAsync(template);
    }

    public override async Task<ItemJournalTemplateDto> UpdateAsync(Guid id, CreateUpdateItemJournalTemplateDto input)
    {
        await CheckUpdatePolicyAsync();
        var template = await GetEntityByIdAsync(id);
        template.SetName(input.Name);
        template.SetDescription(input.Description);
        template.SetType(input.Type);
        await Repository.UpdateAsync(template, autoSave: true);
        return await MapToGetOutputDtoAsync(template);
    }

    protected override async Task<IQueryable<ItemJournalTemplate>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), t => t.Name.ToLower().Contains(filter) || (t.Description != null && t.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<ItemJournalTemplate> ApplyDefaultSorting(IQueryable<ItemJournalTemplate> query) => query.OrderBy(t => t.Name);
}

/// <summary>Item journal batches: posting them and filling physical inventory and revaluation journals.</summary>
[Authorize(ErpPermissions.ItemJournals.Default)]
public class ItemJournalBatchAppService
    : ErpCrudAppService<ItemJournalBatch, ItemJournalBatchDto, Guid, GetItemJournalBatchListInput, CreateUpdateItemJournalBatchDto, CreateUpdateItemJournalBatchDto>,
        IItemJournalBatchAppService
{
    private readonly ItemJnlPostBatch _postBatch;

    public ItemJournalBatchAppService(IRepository<ItemJournalBatch, Guid> repository, ItemJnlPostBatch postBatch)
        : base(repository)
    {
        _postBatch = postBatch;
        GetPolicyName = ErpPermissions.ItemJournals.Default;
        GetListPolicyName = ErpPermissions.ItemJournals.Default;
        CreatePolicyName = ErpPermissions.ItemJournals.Default;
        UpdatePolicyName = ErpPermissions.ItemJournals.Default;
        DeletePolicyName = ErpPermissions.ItemJournals.Default;
    }

    public override async Task<ItemJournalBatchDto> CreateAsync(CreateUpdateItemJournalBatchDto input)
    {
        await CheckCreatePolicyAsync();
        var batch = new ItemJournalBatch(GuidGenerator.Create(), input.JournalTemplateName.Trim().ToUpperInvariant(), input.Name, input.Description);
        await Repository.InsertAsync(batch, autoSave: true);
        return await MapToGetOutputDtoAsync(batch);
    }

    public override async Task<ItemJournalBatchDto> UpdateAsync(Guid id, CreateUpdateItemJournalBatchDto input)
    {
        await CheckUpdatePolicyAsync();
        var batch = await GetEntityByIdAsync(id);
        batch.SetName(input.Name);
        batch.SetDescription(input.Description);
        await Repository.UpdateAsync(batch, autoSave: true);
        return await MapToGetOutputDtoAsync(batch);
    }

    [Authorize(ErpPermissions.ItemJournals.Post)]
    public async Task<ItemJournalPostingResultDto> RunPostingAsync(Guid id)
    {
        var result = await _postBatch.PostAsync(id);
        return new ItemJournalPostingResultDto { LineCount = result.LineCount, RegisterNo = result.RegisterNo };
    }

    public async Task<int> CalculateInventoryAsync(Guid id, CalculateInventoryInput input)
    {
        await CheckUpdatePolicyAsync();
        return await _postBatch.CalculateInventoryAsync(id, input.PostingDate, input.DocumentNo, input.LocationCode);
    }

    public async Task<int> CalculateInventoryValueAsync(Guid id, CalculateInventoryInput input)
    {
        await CheckUpdatePolicyAsync();
        return await _postBatch.CalculateInventoryValueAsync(id, input.PostingDate, input.DocumentNo);
    }

    protected override async Task<ItemJournalBatchDto> MapToGetOutputDtoAsync(ItemJournalBatch entity)
    {
        var dto = await base.MapToGetOutputDtoAsync(entity);
        dto.TemplateType = await _postBatch.GetTypeAsync(entity);
        return dto;
    }

    protected override async Task<List<ItemJournalBatchDto>> MapToGetListOutputDtosAsync(List<ItemJournalBatch> entities)
    {
        var dtos = new List<ItemJournalBatchDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapToGetOutputDtoAsync(entity));
        }
        return dtos;
    }

    protected override async Task<IQueryable<ItemJournalBatch>> CreateFilteredQueryAsync(GetItemJournalBatchListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        if (input.TemplateType.HasValue)
        {
            var templates = LazyServiceProvider.LazyGetRequiredService<IRepository<ItemJournalTemplate, Guid>>();
            var names = (await templates.GetListAsync(t => t.Type == input.TemplateType.Value)).Select(t => t.Name).ToList();
            query = query.Where(b => names.Contains(b.JournalTemplateName));
        }

        return query.WhereIf(!filter.IsNullOrEmpty(), b => b.Name.ToLower().Contains(filter) || b.JournalTemplateName.ToLower().Contains(filter));
    }

    protected override IQueryable<ItemJournalBatch> ApplyDefaultSorting(IQueryable<ItemJournalBatch> query) =>
        query.OrderBy(b => b.JournalTemplateName).ThenBy(b => b.Name);
}

[Authorize(ErpPermissions.ItemJournals.Default)]
public class ItemJournalLineAppService
    : ErpCrudAppService<ItemJournalLine, ItemJournalLineDto, Guid, GetItemJournalLineListInput, CreateUpdateItemJournalLineDto, CreateUpdateItemJournalLineDto>,
        IItemJournalLineAppService
{
    public ItemJournalLineAppService(IRepository<ItemJournalLine, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.ItemJournals.Default;
        GetListPolicyName = ErpPermissions.ItemJournals.Default;
        CreatePolicyName = ErpPermissions.ItemJournals.Default;
        UpdatePolicyName = ErpPermissions.ItemJournals.Default;
        DeletePolicyName = ErpPermissions.ItemJournals.Default;
    }

    public override async Task<ItemJournalLineDto> CreateAsync(CreateUpdateItemJournalLineDto input)
    {
        await CheckCreatePolicyAsync();
        await LazyServiceProvider.LazyGetRequiredService<IRepository<ItemJournalBatch, Guid>>().GetAsync(input.ItemJournalBatchId, includeDetails: false);

        var lines = await Repository.GetListAsync(l => l.ItemJournalBatchId == input.ItemJournalBatchId);
        var lineNo = (lines.Count == 0 ? 0 : lines.Max(l => l.LineNo)) + 10000;
        var item = await GetItemAsync(input.ItemNo);

        var line = new ItemJournalLine(GuidGenerator.Create(), input.ItemJournalBatchId, lineNo, input.PostingDate, input.EntryType, input.DocumentNo, item.Id, item.No, input.Description ?? item.Description, input.Quantity, input.UnitCost);
        await ApplyAsync(line, item, input);
        await Repository.InsertAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<ItemJournalLineDto> UpdateAsync(Guid id, CreateUpdateItemJournalLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        var item = await GetItemAsync(input.ItemNo);

        line.SetPosting(input.PostingDate, input.EntryType, input.DocumentNo);
        line.SetItem(item.Id, item.No, input.Description ?? item.Description);
        line.SetQuantity(input.Quantity, input.UnitCost);
        await ApplyAsync(line, item, input);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    /// <summary>Locations, variant, unit of measure, tracking, the counted quantity and the new cost.</summary>
    private async Task ApplyAsync(ItemJournalLine line, Item item, CreateUpdateItemJournalLineDto input)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<Location>(input.LocationCode);
        await codes.EnsureExistsAsync<Location>(input.NewLocationCode);

        var units = LazyServiceProvider.LazyGetRequiredService<ItemUnitOfMeasureManager>();
        await units.EnsureVariantAsync(item.No, input.VariantCode);
        var unitCode = input.UnitOfMeasureCode ?? item.BaseUnitOfMeasureCode;

        line.SetLocations(input.LocationCode, input.NewLocationCode);
        if (!input.BinCode.IsNullOrWhiteSpace())
        {
            await LazyServiceProvider.LazyGetRequiredService<WarehouseManager>().GetBinAsync(input.LocationCode, input.BinCode);
        }
        line.SetBin(input.BinCode);
        line.SetVariantAndUnit(input.VariantCode, unitCode, await units.GetQtyPerAsync(item, unitCode));
        line.SetTracking(input.LotNo, input.SerialNo, input.ExpirationDate);

        // Only a counted or revalued line carries these; leave the calculated figures alone.
        if (line.QtyCalculated != 0m || input.QtyPhysInventory != 0m)
        {
            line.SetPhysInventory(line.QtyCalculated, input.QtyPhysInventory);
        }
        if (line.UnitCostCalculated != 0m || input.UnitCostRevalued != 0m)
        {
            line.SetRevaluation(line.UnitCostCalculated, input.UnitCostRevalued);
        }
    }

    private async Task<Item> GetItemAsync(string itemNo)
    {
        var no = itemNo?.Trim();
        return await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no ?? "");
    }

    protected override async Task<IQueryable<ItemJournalLine>> CreateFilteredQueryAsync(GetItemJournalLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(input.ItemJournalBatchId.HasValue, l => l.ItemJournalBatchId == input.ItemJournalBatchId.Value)
            .WhereIf(!filter.IsNullOrEmpty(), l => l.ItemNo.ToLower().Contains(filter) || l.DocumentNo.ToLower().Contains(filter));
    }

    protected override IQueryable<ItemJournalLine> ApplyDefaultSorting(IQueryable<ItemJournalLine> query) => query.OrderBy(l => l.LineNo);
}

/// <summary>Transfer orders: moving stock between locations through transit.</summary>
[Authorize(ErpPermissions.TransferOrders.Default)]
public class TransferOrderAppService
    : ErpCrudAppService<TransferHeader, TransferHeaderDto, Guid, GetTransferListInput, CreateUpdateTransferHeaderDto, CreateUpdateTransferHeaderDto>,
        ITransferOrderAppService
{
    private readonly TransferPostingEngine _engine;

    public TransferOrderAppService(IRepository<TransferHeader, Guid> repository, TransferPostingEngine engine)
        : base(repository)
    {
        _engine = engine;
        GetPolicyName = ErpPermissions.TransferOrders.Default;
        GetListPolicyName = ErpPermissions.TransferOrders.Default;
        CreatePolicyName = ErpPermissions.TransferOrders.Create;
        UpdatePolicyName = ErpPermissions.TransferOrders.Update;
        DeletePolicyName = ErpPermissions.TransferOrders.Delete;
    }

    public override async Task<TransferHeaderDto> CreateAsync(CreateUpdateTransferHeaderDto input)
    {
        await CheckCreatePolicyAsync();
        var setup = await LazyServiceProvider.LazyGetRequiredService<InventorySetupManager>().GetAsync();
        var no = await LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>().ResolveNoAsync(setup.TransferOrderNos, input.No, input.PostingDate);
        if (await Repository.AnyAsync(t => t.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }

        var order = new TransferHeader(GuidGenerator.Create(), no, input.PostingDate);
        await ApplyAsync(order, input);
        await Repository.InsertAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    public override async Task<TransferHeaderDto> UpdateAsync(Guid id, CreateUpdateTransferHeaderDto input)
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
        var shipped = order.Lines.FirstOrDefault(l => l.QuantityShipped != l.QuantityReceived);
        if (shipped != null)
        {
            throw new BusinessException(ErpErrorCodes.Documents.LineAlreadyPosted).WithData("documentNo", order.No).WithData("lineNo", shipped.LineNo);
        }
        await Repository.DeleteAsync(order, autoSave: true);
    }

    [Authorize(ErpPermissions.TransferOrders.Update)]
    public async Task<TransferHeaderDto> ReleaseAsync(Guid id)
    {
        var order = await GetEntityByIdAsync(id);
        if (!order.Lines.Any())
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentHasNoLines).WithData("documentNo", order.No);
        }
        order.Release();
        await Repository.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    [Authorize(ErpPermissions.TransferOrders.Update)]
    public async Task<TransferHeaderDto> ReopenAsync(Guid id)
    {
        var order = await GetEntityByIdAsync(id);
        order.Reopen();
        await Repository.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    [Authorize(ErpPermissions.TransferOrders.Post)]
    public async Task<TransferHeaderDto> ShipAsync(Guid id)
    {
        await _engine.ShipAsync(id);
        return await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id));
    }

    [Authorize(ErpPermissions.TransferOrders.Post)]
    public async Task<TransferHeaderDto> ReceiveAsync(Guid id)
    {
        await _engine.ReceiveAsync(id);
        return await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id));
    }

    private async Task ApplyAsync(TransferHeader order, CreateUpdateTransferHeaderDto input)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<Location>(input.FromLocationCode);
        await codes.EnsureExistsAsync<Location>(input.ToLocationCode);
        await codes.EnsureExistsAsync<Location>(input.InTransitCode);

        if (order.FromLocationCode != CodeTableEntity.NormalizeCode(input.FromLocationCode)
            || order.ToLocationCode != CodeTableEntity.NormalizeCode(input.ToLocationCode)
            || order.InTransitCode != CodeTableEntity.NormalizeCode(input.InTransitCode))
        {
            order.SetLocations(input.FromLocationCode, input.ToLocationCode, input.InTransitCode);
        }
        order.SetDates(input.PostingDate, input.ShipmentDate, input.ReceiptDate);
    }

    protected override async Task<TransferHeader> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    protected override async Task<IQueryable<TransferHeader>> CreateFilteredQueryAsync(GetTransferListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!filter.IsNullOrEmpty(), t => t.No.ToLower().Contains(filter) || (t.FromLocationCode != null && t.FromLocationCode.ToLower().Contains(filter))
                || (t.ToLocationCode != null && t.ToLocationCode.ToLower().Contains(filter)))
            .WhereIf(input.Completed.HasValue, t => t.Completed == input.Completed.Value);
    }

    protected override IQueryable<TransferHeader> ApplyDefaultSorting(IQueryable<TransferHeader> query) =>
        query.OrderByDescending(t => t.PostingDate).ThenByDescending(t => t.No);
}

/// <summary>The lines of a transfer order: changed through the order.</summary>
[Authorize(ErpPermissions.TransferOrders.Default)]
public class TransferLineAppService
    : ErpCrudAppService<TransferLine, TransferLineDto, Guid, GetTransferLineListInput, CreateUpdateTransferLineDto, CreateUpdateTransferLineDto>,
        ITransferLineAppService
{
    private readonly IRepository<TransferHeader, Guid> _orders;

    public TransferLineAppService(IRepository<TransferLine, Guid> repository, IRepository<TransferHeader, Guid> orders)
        : base(repository)
    {
        _orders = orders;
        GetPolicyName = ErpPermissions.TransferOrders.Default;
        GetListPolicyName = ErpPermissions.TransferOrders.Default;
        CreatePolicyName = ErpPermissions.TransferOrders.Update;
        UpdatePolicyName = ErpPermissions.TransferOrders.Update;
        DeletePolicyName = ErpPermissions.TransferOrders.Update;
    }

    public override async Task<TransferLineDto> CreateAsync(CreateUpdateTransferLineDto input)
    {
        await CheckCreatePolicyAsync();
        var order = await _orders.GetAsync(input.TransferHeaderId);
        var item = await GetItemAsync(input.ItemNo);
        var (unitCode, qtyPer) = await UnitOfAsync(item, input);

        var line = order.AddLine(GuidGenerator.Create(), item, input.VariantCode, input.Description, unitCode, qtyPer, input.Quantity);
        ApplyQuantitiesToPost(order, line, input);
        await _orders.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<TransferLineDto> UpdateAsync(Guid id, CreateUpdateTransferLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var existing = await GetEntityByIdAsync(id);
        var order = await _orders.GetAsync(existing.TransferHeaderId);
        var line = order.GetLine(id);

        // A released order only takes new quantities to ship and receive.
        if (order.Status == TransferStatus.Open)
        {
            var item = await GetItemAsync(line.ItemNo);
            var (unitCode, qtyPer) = await UnitOfAsync(item, input);
            line.Update(order.No, input.VariantCode, input.Description ?? line.Description, unitCode, qtyPer, input.Quantity);
        }
        ApplyQuantitiesToPost(order, line, input);
        await _orders.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var existing = await GetEntityByIdAsync(id);
        var order = await _orders.GetAsync(existing.TransferHeaderId);
        order.RemoveLine(id);
        await _orders.UpdateAsync(order, autoSave: true);
    }

    private static void ApplyQuantitiesToPost(TransferHeader order, TransferLine line, CreateUpdateTransferLineDto input)
    {
        if (input.QtyToShip.HasValue || input.QtyToReceive.HasValue)
        {
            line.SetQuantitiesToPost(order.No, input.QtyToShip ?? line.QtyToShip, input.QtyToReceive ?? line.QtyToReceive);
        }
    }

    private async Task<(string UnitCode, decimal QtyPer)> UnitOfAsync(Item item, CreateUpdateTransferLineDto input)
    {
        var units = LazyServiceProvider.LazyGetRequiredService<ItemUnitOfMeasureManager>();
        await units.EnsureVariantAsync(item.No, input.VariantCode);
        var unitCode = input.UnitOfMeasureCode ?? item.BaseUnitOfMeasureCode;
        return (unitCode, await units.GetQtyPerAsync(item, unitCode));
    }

    private async Task<Item> GetItemAsync(string itemNo)
    {
        var no = itemNo?.Trim();
        return await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no ?? "");
    }

    protected override async Task<IQueryable<TransferLine>> CreateFilteredQueryAsync(GetTransferLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.TransferHeaderId.HasValue, l => l.TransferHeaderId == input.TransferHeaderId.Value);
    }

    protected override IQueryable<TransferLine> ApplyDefaultSorting(IQueryable<TransferLine> query) => query.OrderBy(l => l.LineNo);
}

[Authorize(ErpPermissions.TransferOrders.Default)]
public class PostedTransferAppService
    : ErpReadOnlyAppService<PostedTransfer, PostedTransferDto, Guid, GetPostedTransferListInput>,
        IPostedTransferAppService
{
    public PostedTransferAppService(IRepository<PostedTransfer, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.TransferOrders.Default;
        GetListPolicyName = ErpPermissions.TransferOrders.Default;
    }

    protected override async Task<IQueryable<PostedTransfer>> CreateFilteredQueryAsync(GetPostedTransferListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!filter.IsNullOrEmpty(), p => p.No.ToLower().Contains(filter) || (p.TransferOrderNo != null && p.TransferOrderNo.ToLower().Contains(filter)))
            .WhereIf(input.Receipt.HasValue, p => p.Receipt == input.Receipt.Value);
    }

    protected override IQueryable<PostedTransfer> ApplyDefaultSorting(IQueryable<PostedTransfer> query) =>
        query.OrderByDescending(p => p.PostingDate).ThenByDescending(p => p.No);
}
