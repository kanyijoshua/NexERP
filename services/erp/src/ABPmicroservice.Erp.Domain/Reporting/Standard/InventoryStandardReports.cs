using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Inventory Valuation: per item and location, the quantity on hand at the date and its value
/// from the value entries: the actual cost of what has been invoiced and the expected cost of
/// shipments and receipts not invoiced yet.
/// </summary>
public class InventoryValuationReport : StandardReportBase
{
    private readonly IRepository<Item, Guid> _items;
    private readonly IRepository<ItemLedgerEntry, Guid> _entries;
    private readonly IRepository<ValueEntry, Guid> _values;

    public InventoryValuationReport(IRepository<Item, Guid> items, IRepository<ItemLedgerEntry, Guid> entries, IRepository<ValueEntry, Guid> values)
    {
        _items = items;
        _entries = entries;
        _values = values;
    }

    public override StandardReportDefinition Definition { get; } =
        new(1001, "InventoryValuation", "Inventory Valuation", StandardReportAreas.Inventory, StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "itemNo", "Item No.");
        Text(result, "description", "Description");
        Text(result, "locationCode", "Location Code");
        Number(result, "quantity", "Quantity");
        Number(result, "unitCost", "Unit Cost");
        Number(result, "actualCost", "Cost Amount (Actual)");
        Number(result, "expectedCost", "Cost Amount (Expected)");
        Number(result, "value", "Inventory Value");

        var asOf = request.To;
        var filter = Filter(request);
        var items = (await _items.GetListAsync(i => i.Type == ItemType.Inventory)).Where(i => filter.Matches(i.No)).ToDictionary(i => i.Id);
        var quantities = (await _entries.GetListAsync(e => e.PostingDate <= asOf))
            .Where(e => items.ContainsKey(e.ItemId))
            .GroupBy(e => (e.ItemId, e.LocationCode))
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Quantity));
        var values = (await _values.GetListAsync(v => v.PostingDate <= asOf))
            .Where(v => items.ContainsKey(v.ItemId))
            .GroupBy(v => (v.ItemId, v.LocationCode))
            .ToDictionary(g => g.Key, g => (Actual: g.Sum(v => v.CostAmountActual), Expected: g.Sum(v => v.CostAmountExpected)));

        decimal totalActual = 0m, totalExpected = 0m;
        foreach (var key in quantities.Keys.Union(values.Keys).OrderBy(k => items[k.ItemId].No, StringComparer.Ordinal).ThenBy(k => k.LocationCode))
        {
            var quantity = quantities.GetValueOrDefault(key);
            var (actual, expected) = values.GetValueOrDefault(key);
            if (quantity == 0m && actual == 0m && expected == 0m)
            {
                continue;
            }

            var item = items[key.ItemId];
            Row(
                result,
                ("itemNo", item.No),
                ("description", item.Description),
                ("locationCode", key.LocationCode),
                ("quantity", quantity),
                ("unitCost", quantity == 0m ? 0m : Math.Round((actual + expected) / quantity, 5)),
                ("actualCost", actual),
                ("expectedCost", expected),
                ("value", actual + expected)
            );
            totalActual += actual;
            totalExpected += expected;
        }

        BoldRow(result, ("itemNo", "Total"), ("actualCost", totalActual), ("expectedCost", totalExpected), ("value", totalActual + totalExpected));
        return result;
    }
}

/// <summary>
/// Item Availability by Location: per item, location and variant, what is on hand, what open
/// purchase orders will bring in and open sales orders take out, and what that leaves.
/// </summary>
public class ItemAvailabilityByLocationReport : StandardReportBase
{
    private readonly IRepository<Item, Guid> _items;
    private readonly IRepository<ItemLedgerEntry, Guid> _entries;
    private readonly IRepository<SalesHeader, Guid> _sales;
    private readonly IRepository<PurchaseHeader, Guid> _purchases;

    public ItemAvailabilityByLocationReport(
        IRepository<Item, Guid> items,
        IRepository<ItemLedgerEntry, Guid> entries,
        IRepository<SalesHeader, Guid> sales,
        IRepository<PurchaseHeader, Guid> purchases
    )
    {
        _items = items;
        _entries = entries;
        _sales = sales;
        _purchases = purchases;
    }

    public override StandardReportDefinition Definition { get; } =
        new(705, "ItemAvailabilityByLocation", "Item Availability by Location", StandardReportAreas.Inventory, StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "itemNo", "Item No.");
        Text(result, "description", "Description");
        Text(result, "locationCode", "Location Code");
        Text(result, "variantCode", "Variant Code");
        Number(result, "inventory", "Inventory");
        Number(result, "scheduledReceipts", "Scheduled Receipts");
        Number(result, "grossRequirement", "Gross Requirement");
        Number(result, "projected", "Projected Available Balance");

        var filter = Filter(request);
        var items = (await _items.GetListAsync(i => i.Type == ItemType.Inventory)).Where(i => filter.Matches(i.No)).ToDictionary(i => i.No, StringComparer.Ordinal);

        var rows = (await _entries.GetListAsync())
            .Where(e => items.ContainsKey(e.ItemNo))
            .GroupBy(e => (e.ItemNo, e.LocationCode, e.VariantCode))
            .ToDictionary(g => g.Key, g => (Inventory: g.Sum(e => e.Quantity), Receipts: 0m, Requirement: 0m));

        foreach (var order in await _purchases.GetListAsync(h => h.DocumentType == PurchaseDocumentType.Order && !h.Posted, includeDetails: true))
        {
            foreach (var line in order.Lines.Where(l => l.Type == Documents.DocumentLineType.Item && items.ContainsKey(l.No ?? "")))
            {
                var key = (line.No, line.LocationCode ?? order.LocationCode, line.VariantCode);
                var row = rows.GetValueOrDefault(key);
                rows[key] = (row.Inventory, row.Receipts + line.OutstandingQuantity * line.QtyPerUnitOfMeasure, row.Requirement);
            }
        }

        foreach (var order in await _sales.GetListAsync(h => h.DocumentType == SalesDocumentType.Order && !h.Posted, includeDetails: true))
        {
            foreach (var line in order.Lines.Where(l => l.Type == Documents.DocumentLineType.Item && items.ContainsKey(l.No ?? "")))
            {
                var key = (line.No, line.LocationCode ?? order.LocationCode, line.VariantCode);
                var row = rows.GetValueOrDefault(key);
                rows[key] = (row.Inventory, row.Receipts, row.Requirement + line.OutstandingQuantity * line.QtyPerUnitOfMeasure);
            }
        }

        foreach (var (key, row) in rows.OrderBy(r => r.Key.ItemNo, StringComparer.Ordinal).ThenBy(r => r.Key.LocationCode).ThenBy(r => r.Key.VariantCode))
        {
            if (row.Inventory == 0m && row.Receipts == 0m && row.Requirement == 0m)
            {
                continue;
            }

            Row(
                result,
                ("itemNo", key.ItemNo),
                ("description", items[key.ItemNo].Description),
                ("locationCode", key.LocationCode),
                ("variantCode", key.VariantCode),
                ("inventory", row.Inventory),
                ("scheduledReceipts", row.Receipts),
                ("grossRequirement", row.Requirement),
                ("projected", row.Inventory + row.Receipts - row.Requirement)
            );
        }

        return result;
    }
}

/// <summary>Item Tracing: every movement of the lots and serial numbers of the items, oldest first.</summary>
public class ItemTracingReport : StandardReportBase
{
    private readonly IRepository<ItemLedgerEntry, Guid> _entries;

    public ItemTracingReport(IRepository<ItemLedgerEntry, Guid> entries)
    {
        _entries = entries;
    }

    public override StandardReportDefinition Definition { get; } =
        new(6520, "ItemTracing", "Item Tracing", StandardReportAreas.Inventory, StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "itemNo", "Item No.");
        Text(result, "lotNo", "Lot No.");
        Text(result, "serialNo", "Serial No.");
        Date(result, "postingDate", "Posting Date");
        Text(result, "entryType", "Entry Type");
        Text(result, "documentNo", "Document No.");
        Text(result, "locationCode", "Location Code");
        Number(result, "quantity", "Quantity");
        Number(result, "remainingQuantity", "Remaining Quantity");

        var filter = Filter(request);
        var from = request.From;
        var to = request.To;
        var entries = (await _entries.GetListAsync(e => (e.LotNo != null || e.SerialNo != null) && e.PostingDate >= from && e.PostingDate <= to))
            .Where(e => filter.Matches(e.ItemNo))
            .OrderBy(e => e.ItemNo, StringComparer.Ordinal)
            .ThenBy(e => e.LotNo)
            .ThenBy(e => e.SerialNo)
            .ThenBy(e => e.EntryNo);

        foreach (var entry in entries)
        {
            Row(
                result,
                ("itemNo", entry.ItemNo),
                ("lotNo", entry.LotNo),
                ("serialNo", entry.SerialNo),
                ("postingDate", entry.PostingDate),
                ("entryType", entry.EntryType.ToString()),
                ("documentNo", entry.DocumentNo),
                ("locationCode", entry.LocationCode),
                ("quantity", entry.Quantity),
                ("remainingQuantity", entry.Positive ? entry.RemainingQuantity : 0m)
            );
        }

        return result;
    }
}
