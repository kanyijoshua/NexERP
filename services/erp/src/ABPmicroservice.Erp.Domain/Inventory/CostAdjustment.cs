using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>What one run of the cost adjustment or of "Post Inventory Cost to G/L" did.</summary>
public sealed class CostAdjustmentResult
{
    /// <summary>Outbound entries (and the transfers they fed) that were re-costed.</summary>
    public int AdjustedEntries { get; init; }

    /// <summary>The value entries whose cost went to the G/L.</summary>
    public int PostedValueEntries { get; init; }

    public decimal Amount { get; init; }

    /// <summary>Average cost items are left alone: they leave at the running average, not at what they took.</summary>
    public int SkippedAverageCostItems { get; init; }
}

/// <summary>
/// Inventory cost adjustment.
/// <para>
/// Stock that left before the cost of what it took was final (a purchase invoiced at another price
/// than the receipt, an item charge assigned later, a revaluation of stock it left before) carries
/// the old cost. The adjustment works out what each completely invoiced outbound entry of a FIFO or
/// specific cost item cost from the inbound entries it was applied to, as they stand on the day it
/// left, and posts the difference as an adjustment value entry. A transfer passes the new cost on
/// to the stock it moved, so the run repeats until nothing changes.
/// </para>
/// <para>
/// With automatic cost posting the difference goes to the G/L at once; otherwise
/// <see cref="PostInventoryCostToGLAsync"/> posts every value entry cost not in the G/L yet.
/// </para>
/// </summary>
public class CostAdjustmentManager : DomainService
{
    private const string AdjustSourceCode = "INVTADJMT";
    private const string PostSourceCode = "INVTPCOST";
    private const int MaxPasses = 10;

    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<ItemLedgerEntry, Guid> _entryRepository;
    private readonly IRepository<ValueEntry, Guid> _valueRepository;
    private readonly IRepository<ItemApplicationEntry, Guid> _applicationRepository;

    private InventorySetupManager SetupManager => LazyServiceProvider.LazyGetRequiredService<InventorySetupManager>();
    private PostingSetupManager PostingSetup => LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>();
    private GenJnlPostLine GenJnlPostLine => LazyServiceProvider.LazyGetRequiredService<GenJnlPostLine>();
    private GLRegisterManager RegisterManager => LazyServiceProvider.LazyGetRequiredService<GLRegisterManager>();
    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public CostAdjustmentManager(
        IRepository<Item, Guid> itemRepository,
        IRepository<ItemLedgerEntry, Guid> entryRepository,
        IRepository<ValueEntry, Guid> valueRepository,
        IRepository<ItemApplicationEntry, Guid> applicationRepository
    )
    {
        _itemRepository = itemRepository;
        _entryRepository = entryRepository;
        _valueRepository = valueRepository;
        _applicationRepository = applicationRepository;
    }

    /// <summary>Adjusts the cost of the items whose number is in <paramref name="itemNos"/>, or of every item.</summary>
    public async Task<CostAdjustmentResult> AdjustCostAsync(IReadOnlyCollection<string> itemNos, DateTime postingDate)
    {
        var filter = itemNos?.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        var items = (await _itemRepository.GetListAsync(i => i.Type == ItemType.Inventory)).Where(i => filter.Count == 0 || filter.Contains(i.No)).ToList();
        var automatic = (await SetupManager.GetAsync()).AutomaticCostPosting;

        var register = automatic ? await RegisterManager.OpenAsync(postingDate, AdjustSourceCode, null) : null;
        var context = register == null ? null : new GLPostingContext(register, AdjustSourceCode);
        var adjusted = 0;
        var total = 0m;
        var skipped = 0;

        foreach (var item in items)
        {
            if (item.CostingMethod == CostingMethod.Average)
            {
                skipped++;
                continue;
            }

            for (var pass = 0; pass < MaxPasses; pass++)
            {
                var (count, amount) = await AdjustItemAsync(item, postingDate, automatic, context);
                adjusted += count;
                total += amount;
                if (count == 0)
                {
                    break;
                }
            }
        }

        if (register != null)
        {
            await RegisterManager.CloseAsync(register);
        }

        return new CostAdjustmentResult { AdjustedEntries = adjusted, Amount = total, SkippedAverageCostItems = skipped };
    }

    /// <summary>
    /// Posts to the G/L the cost of every value entry not posted yet, inventory against cost of goods
    /// sold for a sale and against the inventory adjustment account for anything else.
    /// </summary>
    public async Task<CostAdjustmentResult> PostInventoryCostToGLAsync(DateTime postingDate, string documentNo)
    {
        var pending = (await _valueRepository.GetListAsync(v => v.CostAmountActual != v.CostPostedToGL)).OrderBy(v => v.EntryNo).ToList();
        if (pending.Count == 0)
        {
            return new CostAdjustmentResult();
        }

        var register = await RegisterManager.OpenAsync(postingDate, PostSourceCode, documentNo);
        var context = new GLPostingContext(register, PostSourceCode);
        var items = new Dictionary<Guid, Item>();
        var entries = new Dictionary<Guid, ItemLedgerEntry>();
        var total = 0m;
        var posted = 0;

        foreach (var value in pending)
        {
            if (!items.TryGetValue(value.ItemId, out var item))
            {
                items[value.ItemId] = item = await _itemRepository.GetAsync(value.ItemId);
            }

            if (!entries.TryGetValue(value.ItemLedgerEntryId, out var entry))
            {
                entries[value.ItemLedgerEntryId] = entry = await _entryRepository.GetAsync(value.ItemLedgerEntryId);
            }

            var amount = value.CostAmountActual - value.CostPostedToGL;
            if (item.Type == ItemType.Inventory)
            {
                await PostAsync(item, entry.EntryType, value.LocationCode, amount, postingDate, documentNo ?? value.DocumentNo, context);
                total += amount;
            }

            value.MarkCostPostedToGL();
            await _valueRepository.UpdateAsync(value, autoSave: true);
            posted++;
        }

        await RegisterManager.CloseAsync(register);
        return new CostAdjustmentResult { PostedValueEntries = posted, Amount = total };
    }

    private async Task<(int Count, decimal Amount)> AdjustItemAsync(Item item, DateTime postingDate, bool automatic, GLPostingContext context)
    {
        var entries = (await _entryRepository.GetListAsync(e => e.ItemId == item.Id)).ToDictionary(e => e.Id);
        var values = (await _valueRepository.GetListAsync(v => v.ItemId == item.Id)).GroupBy(v => v.ItemLedgerEntryId).ToDictionary(g => g.Key, g => g.ToList());
        var applications = (await _applicationRepository.GetListAsync(a => a.ItemNo == item.No)).GroupBy(a => a.OutboundEntryId).ToDictionary(g => g.Key, g => g.ToList());

        var count = 0;
        var total = 0m;
        foreach (var outbound in entries.Values.Where(e => !e.Positive && e.CompletelyInvoiced).OrderBy(e => e.PostingDate).ThenBy(e => e.EntryNo))
        {
            if (!applications.TryGetValue(outbound.Id, out var applied))
            {
                continue;
            }

            // What the stock it took cost on the day it left; what it took without stock keeps its cost.
            var appliedQuantity = applied.Sum(a => a.Quantity);
            var expected = applied.Sum(a => a.Quantity * InboundUnitCost(entries[a.InboundEntryId], values, outbound.PostingDate));
            var unapplied = Math.Abs(outbound.Quantity) - appliedQuantity;
            expected += unapplied * outbound.UnitCost;
            expected = -Math.Round(expected, 2, MidpointRounding.AwayFromZero);

            var current = values.TryGetValue(outbound.Id, out var own) ? own.Sum(v => v.CostAmountActual) : 0m;
            var difference = expected - current;
            if (Math.Abs(difference) < 0.01m)
            {
                continue;
            }

            var value = await InsertAdjustmentAsync(outbound, difference, postingDate, automatic);
            values[outbound.Id] = [.. own ?? [], value];
            outbound.Recost(Math.Round(-expected / Math.Abs(outbound.Quantity), 5, MidpointRounding.AwayFromZero));
            await _entryRepository.UpdateAsync(outbound, autoSave: true);
            if (automatic)
            {
                await PostAsync(item, outbound.EntryType, outbound.LocationCode, difference, postingDate, outbound.DocumentNo, context);
            }

            count++;
            total += difference;

            // A transfer moved what it took: the stock it brought in costs the same.
            if (outbound.EntryType == ItemLedgerEntryType.Transfer)
            {
                var inbound = entries.Values
                    .Where(e => e.Positive && e.EntryType == ItemLedgerEntryType.Transfer && e.DocumentNo == outbound.DocumentNo
                        && e.EntryNo > outbound.EntryNo && e.Quantity == -outbound.Quantity && e.LotNo == outbound.LotNo && e.SerialNo == outbound.SerialNo)
                    .OrderBy(e => e.EntryNo)
                    .FirstOrDefault();
                if (inbound != null)
                {
                    var inboundValue = await InsertAdjustmentAsync(inbound, -difference, postingDate, automatic);
                    values[inbound.Id] = [.. values.GetValueOrDefault(inbound.Id) ?? [], inboundValue];
                    inbound.Recost(Math.Round(-expected / Math.Abs(inbound.Quantity), 5, MidpointRounding.AwayFromZero));
                    await _entryRepository.UpdateAsync(inbound, autoSave: true);
                    if (automatic)
                    {
                        await PostAsync(item, ItemLedgerEntryType.Transfer, inbound.LocationCode, -difference, postingDate, inbound.DocumentNo, context);
                    }
                }
            }
        }

        return (count, total);
    }

    /// <summary>
    /// The cost of one unit of an inbound entry on a date: everything it cost (its receipt or
    /// invoice, item charges) over its quantity, plus the revaluations of what was left of it up to that date.
    /// </summary>
    private static decimal InboundUnitCost(ItemLedgerEntry inbound, Dictionary<Guid, List<ValueEntry>> values, DateTime date)
    {
        if (!values.TryGetValue(inbound.Id, out var own) || inbound.Quantity == 0m)
        {
            return inbound.UnitCost;
        }

        var acquisition = own.Where(v => v.EntryType != ValueEntryType.Revaluation).Sum(v => v.CostAmountActual + v.CostAmountExpected) / inbound.Quantity;
        var revaluations = own
            .Where(v => v.EntryType == ValueEntryType.Revaluation && v.PostingDate.Date <= date.Date && v.ValuationQuantity != 0m)
            .Sum(v => v.CostAmountActual / v.ValuationQuantity);
        return acquisition + revaluations;
    }

    private async Task<ValueEntry> InsertAdjustmentAsync(ItemLedgerEntry entry, decimal amount, DateTime postingDate, bool posted)
    {
        var value = new ValueEntry(
            GuidGenerator.Create(),
            entry.Id,
            entry.ItemId,
            entry.ItemNo,
            postingDate,
            entry.DocumentNo,
            entry.Quantity,
            amount,
            invoicedQuantity: 0m
        ) { EntryNo = await EntryNos.NextAsync(ErpSequenceNames.ValueEntry) }
            .WithDetails(ValueEntryType.DirectCost, entry.LocationCode, entry.VariantCode)
            .AsAdjustment()
            .PostedToGL(posted);
        return await _valueRepository.InsertAsync(value, autoSave: true);
    }

    /// <summary>
    /// The entry no longer knows the customer's business group: the setup with a blank business
    /// group, else the first one of the product group that has a COGS account.
    /// </summary>
    private async Task<string> CogsAccountAsync(string genProdPostingGroup)
    {
        var prod = PostingGroupBase.NormalizeCode(genProdPostingGroup);
        var setups = (await LazyServiceProvider.LazyGetRequiredService<IRepository<GeneralPostingSetup, Guid>>()
                .GetListAsync(s => s.GenProdPostingGroup == prod && s.COGSAccountNo != null))
            .OrderBy(s => s.GenBusPostingGroup == null ? 0 : 1)
            .ThenBy(s => s.GenBusPostingGroup)
            .ToList();
        return setups.FirstOrDefault()?.COGSAccountNo ?? await PostingSetup.GetCogsAccountAsync(null, genProdPostingGroup);
    }

    /// <summary>Inventory against cost of goods sold (a sale) or the inventory adjustment account (anything else).</summary>
    private async Task PostAsync(Item item, ItemLedgerEntryType entryType, string locationCode, decimal amount, DateTime postingDate, string documentNo, GLPostingContext context)
    {
        if (amount == 0m)
        {
            return;
        }

        var inventoryAccountNo = await PostingSetup.GetInventoryAccountAsync(item.InventoryPostingGroup, locationCode);
        var counterAccountNo = entryType == ItemLedgerEntryType.Sale
            ? await CogsAccountAsync(item.GenProdPostingGroup)
            : await PostingSetup.GetInventoryAdjmtAccountAsync(item.GenProdPostingGroup);
        var description = item.Description ?? item.No;
        await GenJnlPostLine.PostGLDirectAsync(inventoryAccountNo, postingDate, GLEntryDocumentType.None, documentNo, description, amount, item.No, context: context);
        await GenJnlPostLine.PostGLDirectAsync(counterAccountNo, postingDate, GLEntryDocumentType.None, documentNo, description, -amount, item.No, context: context);
    }
}
