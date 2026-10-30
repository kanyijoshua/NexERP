using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Drop shipment: the vendor delivers a sales order line to the customer directly. The sales line
/// is ordered on a purchase order line linked to it; receiving the purchase line ships the sales
/// line at the same time, at the cost it was received at, so the goods pass through the books
/// without ever being in stock.
/// </summary>
public class DropShipmentManager : DomainService
{
    private readonly IRepository<SalesHeader, Guid> _salesRepository;
    private readonly IRepository<PurchaseHeader, Guid> _purchaseRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IRepository<Item, Guid> _itemRepository;

    private PurchasesPayablesSetupManager SetupManager => LazyServiceProvider.LazyGetRequiredService<PurchasesPayablesSetupManager>();
    private NoSeriesManager NoSeriesManager => LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>();
    private DocumentVatCalculator VatCalculator => LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>();
    private SalesPostingEngine SalesPostingEngine => LazyServiceProvider.LazyGetRequiredService<SalesPostingEngine>();

    public DropShipmentManager(
        IRepository<SalesHeader, Guid> salesRepository,
        IRepository<PurchaseHeader, Guid> purchaseRepository,
        IRepository<Vendor, Guid> vendorRepository,
        IRepository<Item, Guid> itemRepository
    )
    {
        _salesRepository = salesRepository;
        _purchaseRepository = purchaseRepository;
        _vendorRepository = vendorRepository;
        _itemRepository = itemRepository;
    }

    /// <summary>
    /// Orders the drop shipment lines of a sales order that are not on a purchase order yet from a
    /// vendor, at the item's last direct cost, for delivery to the order's ship-to address.
    /// </summary>
    public async Task<PurchaseHeader> CreatePurchaseOrderAsync(Guid salesHeaderId, string vendorNo, DateTime orderDate)
    {
        var sales = await _salesRepository.GetAsync(salesHeaderId);
        var lines = sales.Lines
            .Where(l => l.DropShipment && !l.PurchaseOrderLineId.HasValue && l.OutstandingQuantity > 0m)
            .OrderBy(l => l.LineNo)
            .ToList();
        if (sales.DocumentType != SalesDocumentType.Order || lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.NothingToDropShip).WithData("documentNo", sales.No);
        }

        var no = vendorNo?.Trim();
        var vendor = await _vendorRepository.FirstOrDefaultAsync(v => v.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Vendors.VendorNotFound).WithData("accountNo", no ?? "");

        var series = (await SetupManager.GetAsync()).GetDocumentNos(PurchaseDocumentType.Order);
        var purchaseNo = series.IsNullOrWhiteSpace()
            ? await FallbackNoAsync(sales.No)
            : await NoSeriesManager.GetNextNoAsync(series, orderDate);

        var order = new PurchaseHeader(GuidGenerator.Create(), PurchaseDocumentType.Order, purchaseNo, vendor.Id, vendor.No, vendor.Name, orderDate);
        order.SetDates(orderDate, null, sales.ShipmentDate);
        order.SetCurrency(vendor.CurrencyCode);
        order.SetPaymentTerms(vendor.PaymentTermsCode);
        order.SetLocation(sales.LocationCode);
        order.SetPostingGroups(vendor.VendorPostingGroup, vendor.GenBusPostingGroup, vendor.VatBusPostingGroup);
        order.SetShipTo(sales.ShipToCode, sales.ShipToName ?? sales.SellToCustomerName, sales.ShipToAddress, sales.ShipToCity, sales.ShipToPostCode, sales.ShipToCountryRegionCode, sales.ShipToContact);
        order.SetYourReference(sales.No);
        order.SetOrderStage(Procurement.PurchaseOrderStage.Draft);

        foreach (var line in lines)
        {
            var item = await _itemRepository.FirstOrDefaultAsync(i => i.No == line.No);
            var cost = item == null ? 0m : (item.LastDirectCost != 0m ? item.LastDirectCost : item.UnitCost) * line.QtyPerUnitOfMeasure;
            var purchaseLine = order.AddLine(GuidGenerator.Create(), DocumentLineType.Item, line.No, line.Description, line.OutstandingQuantity, cost, 0m, line.UnitOfMeasureCode);
            purchaseLine.SetLocation(line.LocationCode);
            purchaseLine.SetVariant(line.VariantCode);
            purchaseLine.SetQtyPerUnitOfMeasure(line.QtyPerUnitOfMeasure);
            purchaseLine.LinkToSalesOrder(sales.No, line.Id);
            line.LinkToPurchaseOrder(order.No, purchaseLine.Id);
        }

        await VatCalculator.ApplyAsync(order, vendor);
        await _purchaseRepository.InsertAsync(order, autoSave: true);
        await _salesRepository.UpdateAsync(sales, autoSave: true);
        return order;
    }

    /// <summary>A purchase line was removed or its order deleted: its sales line can be ordered again.</summary>
    public async Task UnlinkAsync(IEnumerable<PurchaseLine> purchaseLines)
    {
        foreach (var group in purchaseLines.Where(l => l.SalesOrderLineId.HasValue).GroupBy(l => l.SalesOrderNo))
        {
            var no = group.Key;
            var sales = await _salesRepository.FirstOrDefaultAsync(h => h.DocumentType == SalesDocumentType.Order && h.No == no);
            if (sales == null)
            {
                continue;
            }

            var ids = group.Select(l => l.SalesOrderLineId!.Value).ToHashSet();
            foreach (var line in sales.Lines.Where(l => ids.Contains(l.Id)))
            {
                line.LinkToPurchaseOrder(null, null);
            }

            await _salesRepository.UpdateAsync(sales, autoSave: true);
        }
    }

    /// <summary>
    /// Ships the sales lines of the drop shipment lines a receipt just received, each from the very
    /// item ledger entries the receipt posted.
    /// </summary>
    public async Task ShipReceivedAsync(IReadOnlyCollection<(PurchaseLine Line, List<ItemLedgerEntry> Entries)> received)
    {
        foreach (var group in received.Where(r => r.Line.SalesOrderLineId.HasValue && r.Entries.Count > 0).GroupBy(r => r.Line.SalesOrderNo))
        {
            var no = group.Key;
            var sales = await _salesRepository.FirstOrDefaultAsync(h => h.DocumentType == SalesDocumentType.Order && h.No == no)
                ?? throw new BusinessException(ErpErrorCodes.Inventory.NothingToDropShip).WithData("documentNo", no ?? "");
            await SalesPostingEngine.ShipDropShipmentAsync(sales.Id, group.ToDictionary(r => r.Line.SalesOrderLineId!.Value, r => r.Entries));
        }
    }

    private async Task<string> FallbackNoAsync(string salesNo)
    {
        var count = await _purchaseRepository.CountAsync(h => h.DocumentType == PurchaseDocumentType.Order && h.No.StartsWith(salesNo + "-DS"));
        return count == 0 ? $"{salesNo}-DS" : $"{salesNo}-DS{count + 1}";
    }
}
