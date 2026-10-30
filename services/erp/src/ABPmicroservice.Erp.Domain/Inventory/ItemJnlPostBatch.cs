using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>What posting an item journal batch did.</summary>
public sealed class ItemJournalPostingResult
{
    public int LineCount { get; init; }

    /// <summary>The G/L register of the cost it posted; null when no G/L entry was needed.</summary>
    public long? RegisterNo { get; init; }
}

/// <summary>
/// Posts an item journal batch, by the kind of journal its template is:
/// <list type="bullet">
/// <item>Item: positive and negative adjustments, against the Inventory Adjmt. Account.</item>
/// <item>Transfer: moves stock from the line's location to its new location at the cost it has.</item>
/// <item>Physical inventory: posts the difference between the counted and the calculated quantity.</item>
/// <item>Revaluation: sets a new cost on the stock still on hand.</item>
/// </list>
/// The posted lines are deleted, as on a general journal.
/// </summary>
public class ItemJnlPostBatch : DomainService
{
    /// <summary>Stamped on the G/L entries the journal posts.</summary>
    private const string SourceCode = "ITEMJNL";

    private readonly IRepository<ItemJournalBatch, Guid> _batchRepository;
    private readonly IRepository<ItemJournalLine, Guid> _lineRepository;
    private readonly IRepository<ItemJournalTemplate, Guid> _templateRepository;
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<ItemLedgerEntry, Guid> _entryRepository;
    private readonly ItemJnlPostLine _postLine;
    private readonly InventoryCostPoster _costPoster;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;

    private ItemTrackingManager TrackingManager => LazyServiceProvider.LazyGetRequiredService<ItemTrackingManager>();

    public ItemJnlPostBatch(
        IRepository<ItemJournalBatch, Guid> batchRepository,
        IRepository<ItemJournalLine, Guid> lineRepository,
        IRepository<ItemJournalTemplate, Guid> templateRepository,
        IRepository<Item, Guid> itemRepository,
        IRepository<ItemLedgerEntry, Guid> entryRepository,
        ItemJnlPostLine postLine,
        InventoryCostPoster costPoster,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager
    )
    {
        _batchRepository = batchRepository;
        _lineRepository = lineRepository;
        _templateRepository = templateRepository;
        _itemRepository = itemRepository;
        _entryRepository = entryRepository;
        _postLine = postLine;
        _costPoster = costPoster;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
    }

    /// <summary>The kind of journal a batch is; an item journal when its template is gone.</summary>
    public async Task<ItemJournalTemplateType> GetTypeAsync(ItemJournalBatch batch)
    {
        var name = batch.JournalTemplateName.ToUpperInvariant();
        return (await _templateRepository.FirstOrDefaultAsync(t => t.Name == name))?.Type ?? ItemJournalTemplateType.Item;
    }

    public async Task<ItemJournalPostingResult> PostAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        var type = await GetTypeAsync(batch);
        var lines = (await _lineRepository.GetListAsync(l => l.ItemJournalBatchId == batchId)).OrderBy(l => l.LineNo).ToList();

        // A physical inventory line whose count matches the books has nothing to post.
        var toPost = type == ItemJournalTemplateType.PhysicalInventory
            ? lines.Where(l => l.QtyPhysInventory != l.QtyCalculated).ToList()
            : lines;
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.JournalHasNoLines);
        }

        foreach (var line in toPost)
        {
            await _glSetupManager.CheckPostingDateAsync(line.PostingDate);
            Validate(type, line);
        }

        var register = await _registerManager.OpenAsync(lines[0].PostingDate, SourceCode, batch.Name);
        var context = new GLPostingContext(register, SourceCode);

        foreach (var line in toPost)
        {
            var item = await _itemRepository.GetAsync(line.ItemId);
            switch (type)
            {
                case ItemJournalTemplateType.Transfer:
                    await PostTransferAsync(item, line, context);
                    break;
                case ItemJournalTemplateType.Revaluation:
                    var change = await _postLine.RevalueAsync(item, line.LocationCode, line.VariantCode, line.UnitCostRevalued, line.PostingDate, line.DocumentNo);
                    await _costPoster.PostAdjustmentAsync(item, line.LocationCode, change, line.PostingDate, line.DocumentNo, line.Description ?? item.Description, context);
                    break;
                default:
                    await PostAdjustmentAsync(item, line, context);
                    break;
            }
        }

        var posted = await _registerManager.CloseAsync(register);
        await _lineRepository.DeleteManyAsync(lines, autoSave: true);

        return new ItemJournalPostingResult { LineCount = toPost.Count, RegisterNo = posted ? register.No : null };
    }

    /// <summary>
    /// Fills a physical inventory batch with a line per item, location, variant and lot that has
    /// stock, the counted quantity set to what the books say.
    /// </summary>
    public async Task<int> CalculateInventoryAsync(Guid batchId, DateTime postingDate, string documentNo, string locationCode = null)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        await EnsureTypeAsync(batch, ItemJournalTemplateType.PhysicalInventory);

        var location = Companies.CodeTableEntity.NormalizeCode(locationCode);
        var entries = await _entryRepository.GetListAsync(e => e.PostingDate <= postingDate && (location == null || e.LocationCode == location));
        var stock = entries
            .GroupBy(e => new { e.ItemId, e.ItemNo, e.LocationCode, e.VariantCode, e.LotNo, e.SerialNo })
            .Select(g => new { g.Key, Quantity = g.Sum(e => e.Quantity) })
            .Where(x => x.Quantity != 0m)
            .OrderBy(x => x.Key.ItemNo)
            .ThenBy(x => x.Key.LocationCode)
            .ToList();

        var lineNo = await NextLineNoAsync(batchId);
        foreach (var row in stock)
        {
            var item = await _itemRepository.GetAsync(row.Key.ItemId);
            if (item.Type != ItemType.Inventory)
            {
                continue;
            }

            var line = new ItemJournalLine(GuidGenerator.Create(), batchId, lineNo, postingDate, ItemLedgerEntryType.PositiveAdjmt, documentNo, item.Id, item.No, item.Description, 0m, item.UnitCost);
            line.SetLocations(row.Key.LocationCode, null);
            line.SetVariantAndUnit(row.Key.VariantCode, item.BaseUnitOfMeasureCode, 1m);
            line.SetTracking(row.Key.LotNo, row.Key.SerialNo, null);
            line.SetPhysInventory(row.Quantity, row.Quantity);
            await _lineRepository.InsertAsync(line);
            lineNo += 10000;
        }

        return stock.Count;
    }

    /// <summary>
    /// Fills a revaluation batch with a line per item and location with stock: its cost now, and
    /// the same as the new cost for the user to change.
    /// </summary>
    public async Task<int> CalculateInventoryValueAsync(Guid batchId, DateTime postingDate, string documentNo)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        await EnsureTypeAsync(batch, ItemJournalTemplateType.Revaluation);

        var open = await _entryRepository.GetListAsync(e => e.Positive && e.Open && e.RemainingQuantity > 0m);
        var stock = open
            .GroupBy(e => new { e.ItemId, e.ItemNo, e.LocationCode, e.VariantCode })
            .Select(g => new
            {
                g.Key,
                Quantity = g.Sum(e => e.RemainingQuantity),
                Value = g.Sum(e => e.RemainingQuantity * e.UnitCost),
            })
            .OrderBy(x => x.Key.ItemNo)
            .ThenBy(x => x.Key.LocationCode)
            .ToList();

        var lineNo = await NextLineNoAsync(batchId);
        foreach (var row in stock)
        {
            var item = await _itemRepository.GetAsync(row.Key.ItemId);
            var unitCost = Math.Round(row.Value / row.Quantity, 5, MidpointRounding.AwayFromZero);
            var line = new ItemJournalLine(GuidGenerator.Create(), batchId, lineNo, postingDate, ItemLedgerEntryType.PositiveAdjmt, documentNo, item.Id, item.No, item.Description, row.Quantity, unitCost);
            line.SetLocations(row.Key.LocationCode, null);
            line.SetVariantAndUnit(row.Key.VariantCode, item.BaseUnitOfMeasureCode, 1m);
            line.SetRevaluation(unitCost, unitCost);
            await _lineRepository.InsertAsync(line);
            lineNo += 10000;
        }

        return stock.Count;
    }

    private async Task PostAdjustmentAsync(Item item, ItemJournalLine line, GLPostingContext context)
    {
        var baseQty = line.Quantity * line.QtyPerUnitOfMeasure;
        if (baseQty == 0m)
        {
            return;
        }

        var entries = await _postLine.PostAllAsync(new ItemPostingRequest
        {
            ItemId = item.Id,
            ItemNo = item.No,
            PostingDate = line.PostingDate,
            EntryType = line.EntryType,
            DocumentNo = line.DocumentNo,
            Description = line.Description,
            Quantity = baseQty,
            UnitCost = line.QtyPerUnitOfMeasure == 0m ? 0m : line.UnitCost / line.QtyPerUnitOfMeasure,
            UseItemCost = line.UnitCost == 0m,
            LocationCode = line.LocationCode,
            BinCode = line.BinCode,
            VariantCode = line.VariantCode,
            Tracking = await TrackingOfAsync(item, line, baseQty),
            DimensionSetId = line.DimensionSetId,
        });

        var value = entries.Sum(e => Math.Round(e.Quantity * e.UnitCost, 2, MidpointRounding.AwayFromZero));
        await _costPoster.PostAdjustmentAsync(item, line.LocationCode, value, line.PostingDate, line.DocumentNo, line.Description ?? item.Description, context);
    }

    /// <summary>A reclassification: out of the location and into the new one, lot by lot, at the same cost.</summary>
    private async Task PostTransferAsync(Item item, ItemJournalLine line, GLPostingContext context)
    {
        var baseQty = line.Quantity * line.QtyPerUnitOfMeasure;
        var outbound = await _postLine.PostAllAsync(new ItemPostingRequest
        {
            ItemId = item.Id,
            ItemNo = item.No,
            PostingDate = line.PostingDate,
            EntryType = ItemLedgerEntryType.Transfer,
            DocumentNo = line.DocumentNo,
            Description = line.Description,
            Quantity = baseQty,
            Outbound = true,
            LocationCode = line.LocationCode,
            VariantCode = line.VariantCode,
            Tracking = await TrackingOfAsync(item, line, baseQty),
            DimensionSetId = line.DimensionSetId,
        });

        var value = 0m;
        foreach (var from in outbound)
        {
            await _postLine.PostAllAsync(new ItemPostingRequest
            {
                ItemId = item.Id,
                ItemNo = item.No,
                PostingDate = line.PostingDate,
                EntryType = ItemLedgerEntryType.Transfer,
                DocumentNo = line.DocumentNo,
                Description = line.Description,
                Quantity = Math.Abs(from.Quantity),
                UnitCost = from.UnitCost,
                Outbound = false,
                LocationCode = line.NewLocationCode,
                VariantCode = line.VariantCode,
                Tracking = from.LotNo == null && from.SerialNo == null ? null : [new TrackingPart(from.LotNo, from.SerialNo, Math.Abs(from.Quantity), from.ExpirationDate)],
                DimensionSetId = line.DimensionSetId,
            });
            value += Math.Round(Math.Abs(from.Quantity) * from.UnitCost, 2, MidpointRounding.AwayFromZero);
        }

        await _costPoster.PostTransferAsync(item, line.LocationCode, line.NewLocationCode, value, line.PostingDate, line.DocumentNo, line.Description ?? item.Description, context);
    }

    /// <summary>The line's own lot or serial number, else its item tracking lines.</summary>
    private async Task<IReadOnlyList<TrackingPart>> TrackingOfAsync(Item item, ItemJournalLine line, decimal baseQty)
    {
        if (line.LotNo != null || line.SerialNo != null)
        {
            return [new TrackingPart(line.LotNo, line.SerialNo, baseQty, line.ExpirationDate)];
        }

        return await TrackingManager.TakeAsync(item, ItemTrackingSourceType.ItemJournalLine, line.Id, baseQty);
    }

    private static void Validate(ItemJournalTemplateType type, ItemJournalLine line)
    {
        switch (type)
        {
            case ItemJournalTemplateType.Item:
                if (line.EntryType is not (ItemLedgerEntryType.PositiveAdjmt or ItemLedgerEntryType.NegativeAdjmt))
                {
                    throw new BusinessException(ErpErrorCodes.Inventory.JournalEntryTypeNotAllowed).WithData("entryType", line.EntryType.ToString());
                }
                break;
            case ItemJournalTemplateType.Transfer:
                if (line.NewLocationCode == null)
                {
                    throw new BusinessException(ErpErrorCodes.Inventory.NewLocationRequired).WithData("lineNo", line.LineNo);
                }
                if (line.NewLocationCode == line.LocationCode)
                {
                    throw new BusinessException(ErpErrorCodes.Inventory.SameLocation).WithData("lineNo", line.LineNo);
                }
                break;
        }
    }

    private async Task EnsureTypeAsync(ItemJournalBatch batch, ItemJournalTemplateType expected)
    {
        var type = await GetTypeAsync(batch);
        if (type != expected)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.TemplateTypeMismatch)
                .WithData("templateName", batch.JournalTemplateName)
                .WithData("templateType", type.ToString())
                .WithData("expected", expected.ToString());
        }
    }

    private async Task<int> NextLineNoAsync(Guid batchId)
    {
        var lines = await _lineRepository.GetListAsync(l => l.ItemJournalBatchId == batchId);
        return (lines.Count == 0 ? 0 : lines.Max(l => l.LineNo)) + 10000;
    }
}
