using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Pricing;
using ABPmicroservice.Erp.Purchasing;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Orders shipped, received and invoiced in parts, quotes and blanket orders made into orders,
/// return orders, undoing a shipment, invoicing from posted shipments, and price lists. The seed
/// gives item 1000 the STANDARD VAT group (16% for DOMESTIC parties), a unit cost of 150 and
/// automatic cost posting.
/// </summary>
public class Orders_Tests : ErpApplicationTestBase
{
    private static readonly DateTime PostingDate = new(2026, 3, 1);

    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly IPurchaseDocumentAppService _purchaseDocuments;
    private readonly IPostedSalesShipmentAppService _shipments;
    private readonly ICustomerAppService _customers;
    private readonly IVendorAppService _vendors;
    private readonly IItemAppService _items;
    private readonly IPriceListAppService _priceLists;
    private readonly IPriceListLineAppService _priceListLines;
    private readonly IRepository<GLEntry, Guid> _glEntries;
    private readonly IRepository<ItemLedgerEntry, Guid> _itemEntries;
    private readonly IRepository<ValueEntry, Guid> _valueEntries;

    public Orders_Tests()
    {
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _purchaseDocuments = GetRequiredService<IPurchaseDocumentAppService>();
        _shipments = GetRequiredService<IPostedSalesShipmentAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
        _vendors = GetRequiredService<IVendorAppService>();
        _items = GetRequiredService<IItemAppService>();
        _priceLists = GetRequiredService<IPriceListAppService>();
        _priceListLines = GetRequiredService<IPriceListLineAppService>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
        _itemEntries = GetRequiredService<IRepository<ItemLedgerEntry, Guid>>();
        _valueEntries = GetRequiredService<IRepository<ValueEntry, Guid>>();
    }

    [Fact]
    public async Task An_Order_Is_Shipped_And_Invoiced_In_Parts()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var order = await _salesDocuments.CreateAsync(await SalesDocumentAsync("ORD-PART-1", SalesDocumentType.Order, quantity: 10));
            var lineId = order.Lines.Single().Id;
            var before = (await _items.GetByNoAsync("1000")).Inventory;

            // Ship 4: the stock leaves at expected cost and nothing reaches the G/L.
            var shipped = await _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput
            {
                Ship = true,
                Invoice = false,
                Lines = [new SalesLineQuantitiesInput { LineId = lineId, QtyToShip = 4, QtyToInvoice = 0 }],
            });
            shipped.ShipmentNo.ShouldNotBeNull();
            shipped.InvoiceNo.ShouldBeNull();
            shipped.Document.Posted.ShouldBeFalse();
            shipped.Document.Lines.Single().QuantityShipped.ShouldBe(4m);
            (await _glEntries.GetListAsync(e => e.DocumentNo == shipped.ShipmentNo)).ShouldBeEmpty();
            (await _items.GetByNoAsync("1000")).Inventory.ShouldBe(before - 4);
            var entry = (await _itemEntries.GetListAsync(e => e.DocumentNo == shipped.ShipmentNo)).ShouldHaveSingleItem();
            entry.Quantity.ShouldBe(-4m);
            entry.InvoicedQuantity.ShouldBe(0m);

            // Invoicing more than has been shipped is refused.
            var tooMuch = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput
            {
                Ship = false,
                Invoice = true,
                Lines = [new SalesLineQuantitiesInput { LineId = lineId, QtyToShip = 0, QtyToInvoice = 5 }],
            }));
            tooMuch.Code.ShouldBe(ErpErrorCodes.Documents.QtyToInvoiceTooLarge);

            // Invoice 3 of the 4 shipped.
            var invoiced = await _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput
            {
                Ship = false,
                Invoice = true,
                Lines = [new SalesLineQuantitiesInput { LineId = lineId, QtyToShip = 0, QtyToInvoice = 3 }],
            });
            invoiced.InvoiceNo.ShouldNotBeNull();
            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == invoiced.InvoiceNo);
            Sum(entries, "1200").ShouldBe(1044m);  // 3 x 300 + 16% VAT
            Sum(entries, "4000").ShouldBe(-900m);
            Sum(entries, "5000").ShouldBe(450m);   // 3 x 150
            Sum(entries, "1400").ShouldBe(-450m);
            entries.Sum(e => e.Amount).ShouldBe(0m);
            (await _itemEntries.GetAsync(entry.Id)).InvoicedQuantity.ShouldBe(-3m);

            // Ship and invoice the rest: the order is done.
            var rest = await _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput { Ship = true, Invoice = true });
            rest.Document.Posted.ShouldBeTrue();
            rest.Document.Lines.Single().QuantityInvoiced.ShouldBe(10m);
            var restEntries = await _glEntries.GetListAsync(e => e.DocumentNo == rest.InvoiceNo);
            Sum(restEntries, "1200").ShouldBe(2436m);  // 7 x 300 + 16% VAT
            Sum(restEntries, "5000").ShouldBe(1050m);
            restEntries.Sum(e => e.Amount).ShouldBe(0m);

            // Every expected cost has turned into actual cost.
            var values = await _valueEntries.GetListAsync(v => v.ItemNo == "1000" && (v.DocumentNo == shipped.ShipmentNo || v.DocumentNo == invoiced.InvoiceNo
                || v.DocumentNo == rest.ShipmentNo || v.DocumentNo == rest.InvoiceNo));
            values.Sum(v => v.CostAmountExpected).ShouldBe(0m);
            values.Sum(v => v.CostAmountActual).ShouldBe(-1500m);
        });
    }

    [Fact]
    public async Task A_Quote_Is_Not_Posted_But_Made_Into_An_Order()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var quote = await _salesDocuments.CreateAsync(await SalesDocumentAsync("QUO-1", SalesDocumentType.Quote, quantity: 2));

            var refused = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.RunPostingAsync(quote.Id));
            refused.Code.ShouldBe(ErpErrorCodes.Documents.DocumentTypeCannotBePosted);

            var order = await _salesDocuments.MakeOrderAsync(quote.Id, new MakeOrderInput { PostingDate = PostingDate });
            order.DocumentType.ShouldBe(SalesDocumentType.Order);
            order.QuoteNo.ShouldBe("QUO-1");
            order.Lines.Single().Quantity.ShouldBe(2m);
            order.Lines.Single().UnitPrice.ShouldBe(300m);

            // The quote is used up.
            (await _salesDocuments.GetListAsync(new GetSalesDocumentListInput { Filter = "QUO-1", DocumentType = SalesDocumentType.Quote, MaxResultCount = 10 }))
                .Items.ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task A_Blanket_Order_Gives_Its_Quantity_To_Orders_In_Parts()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var input = await SalesDocumentAsync("BLK-1", SalesDocumentType.BlanketOrder, quantity: 10);
            input.Lines[0].QtyToShip = 4;
            var blanket = await _salesDocuments.CreateAsync(input);
            blanket.Lines.Single().QtyToShip.ShouldBe(4m);

            var first = await _salesDocuments.MakeOrderAsync(blanket.Id, new MakeOrderInput { PostingDate = PostingDate });
            first.Lines.Single().Quantity.ShouldBe(4m);
            first.Lines.Single().BlanketOrderNo.ShouldBe("BLK-1");

            var after = await _salesDocuments.GetAsync(blanket.Id);
            after.Lines.Single().QuantityShipped.ShouldBe(4m);
            after.Lines.Single().QtyToShip.ShouldBe(6m);

            var second = await _salesDocuments.MakeOrderAsync(blanket.Id, new MakeOrderInput { PostingDate = PostingDate });
            second.Lines.Single().Quantity.ShouldBe(6m);

            // Nothing is left to order.
            var empty = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.MakeOrderAsync(blanket.Id, new MakeOrderInput { PostingDate = PostingDate }));
            empty.Code.ShouldBe(ErpErrorCodes.Documents.NothingToPost);
        });
    }

    [Fact]
    public async Task A_Return_Order_Takes_The_Goods_Back_And_Credits_The_Customer()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var before = (await _items.GetByNoAsync("1000")).Inventory;
            var returnOrder = await _salesDocuments.CreateAsync(await SalesDocumentAsync("RET-1", SalesDocumentType.ReturnOrder, quantity: 2));

            var result = await _salesDocuments.RunOrderPostingAsync(returnOrder.Id, new SalesOrderPostingInput { Ship = true, Invoice = true });

            result.Document.Posted.ShouldBeTrue();
            var receipt = await _shipments.GetAsync((await _shipments.GetListAsync(new GetPostedSalesDocumentListInput { OrderNo = "RET-1", MaxResultCount = 10 })).Items.Single().Id);
            receipt.Return.ShouldBeTrue();
            receipt.Lines.Single().QuantityInvoiced.ShouldBe(2m);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == result.InvoiceNo);
            Sum(entries, "1200").ShouldBe(-696m);
            Sum(entries, "1400").ShouldBe(300m);
            entries.Sum(e => e.Amount).ShouldBe(0m);
            entries.ShouldAllBe(e => e.DocumentType == GLEntryDocumentType.CreditMemo);
            (await _items.GetByNoAsync("1000")).Inventory.ShouldBe(before + 2);
        });
    }

    [Fact]
    public async Task A_Shipment_Line_Not_Invoiced_Can_Be_Undone()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var before = (await _items.GetByNoAsync("1000")).Inventory;
            var order = await _salesDocuments.CreateAsync(await SalesDocumentAsync("ORD-UNDO-1", SalesDocumentType.Order, quantity: 5));
            var shipped = await _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput { Ship = true, Invoice = false });

            var shipment = await _shipments.GetAsync((await _shipments.GetListAsync(new GetPostedSalesDocumentListInput { OrderNo = "ORD-UNDO-1", MaxResultCount = 10 })).Items.Single().Id);
            var shipmentLine = shipment.Lines.Single();
            await _shipments.UndoLineAsync(shipmentLine.Id);

            (await _items.GetByNoAsync("1000")).Inventory.ShouldBe(before);
            (await _salesDocuments.GetAsync(order.Id)).Lines.Single().QuantityShipped.ShouldBe(0m);
            (await _shipments.GetAsync(shipment.Id)).Lines.Single().Correction.ShouldBeTrue();

            var again = await Should.ThrowAsync<BusinessException>(() => _shipments.UndoLineAsync(shipmentLine.Id));
            again.Code.ShouldBe(ErpErrorCodes.Documents.CannotUndoInvoicedLine);
            shipped.ShipmentNo.ShouldBe(shipment.No);
        });
    }

    [Fact]
    public async Task An_Invoice_Can_Take_Its_Lines_From_Posted_Shipments()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var order = await _salesDocuments.CreateAsync(await SalesDocumentAsync("ORD-GSL-1", SalesDocumentType.Order, quantity: 5));
            await _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput { Ship = true, Invoice = false });

            var invoiceInput = await SalesDocumentAsync("INV-GSL-1", SalesDocumentType.Invoice, quantity: 1);
            invoiceInput.Lines.Clear();
            var invoice = await _salesDocuments.CreateAsync(invoiceInput);

            var open = await _salesDocuments.GetOpenShipmentLinesAsync(invoice.Id);
            var candidate = open.Single(l => l.OrderNo == "ORD-GSL-1");
            candidate.QtyNotInvoiced.ShouldBe(5m);

            invoice = await _salesDocuments.CollectShipmentLinesAsync(invoice.Id, new GetShipmentLinesInput { ShipmentLineIds = [candidate.ShipmentLineId] });
            invoice.Lines.Single().ShipmentNo.ShouldBe(candidate.ShipmentNo);

            var stockBefore = (await _items.GetByNoAsync("1000")).Inventory;
            var posted = await _salesDocuments.RunPostingAsync(invoice.Id);

            // The goods left with the shipment: the invoice moves no stock but posts the cost.
            (await _items.GetByNoAsync("1000")).Inventory.ShouldBe(stockBefore);
            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == posted.PostedDocumentNo);
            Sum(entries, "5000").ShouldBe(750m);
            entries.Sum(e => e.Amount).ShouldBe(0m);

            var orderAfter = await _salesDocuments.GetAsync(order.Id);
            orderAfter.Lines.Single().QuantityInvoiced.ShouldBe(5m);
            orderAfter.Posted.ShouldBeTrue();
            (await _salesDocuments.GetOpenShipmentLinesAsync(invoice.Id)).ShouldNotContain(l => l.OrderNo == "ORD-GSL-1");
        });
    }

    [Fact]
    public async Task A_Purchase_Order_Receives_At_Expected_Cost_And_Invoices_The_Actual_Cost()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var vendor = await _vendors.GetByNoAsync("V00010");
            var order = await _purchaseDocuments.CreateAsync(new CreateUpdatePurchaseHeaderDto
            {
                DocumentType = PurchaseDocumentType.Order,
                No = "PO-PART-1",
                VendorId = vendor.Id,
                PostingDate = PostingDate,
                VendorInvoiceNo = "VI-1",
                Lines = [new PurchaseLineInputDto { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle", Quantity = 10, DirectUnitCost = 100m }],
            });
            var lineId = order.Lines.Single().Id;

            var received = await _purchaseDocuments.RunOrderPostingAsync(order.Id, new PurchaseOrderPostingInput
            {
                Receive = true,
                Invoice = false,
                Lines = [new PurchaseLineQuantitiesInput { LineId = lineId, QtyToReceive = 10, QtyToInvoice = 0 }],
            });
            received.ReceiptNo.ShouldNotBeNull();
            received.Document.OrderStage.ShouldBe(ABPmicroservice.Erp.Procurement.PurchaseOrderStage.Received);
            (await _glEntries.GetListAsync(e => e.DocumentNo == received.ReceiptNo)).ShouldBeEmpty();

            var invoiced = await _purchaseDocuments.RunOrderPostingAsync(order.Id, new PurchaseOrderPostingInput { Receive = false, Invoice = true });
            invoiced.Document.Posted.ShouldBeTrue();
            invoiced.Document.OrderStage.ShouldBe(ABPmicroservice.Erp.Procurement.PurchaseOrderStage.Invoiced);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == invoiced.InvoiceNo);
            Sum(entries, "1400").ShouldBe(1000m);
            entries.Sum(e => e.Amount).ShouldBe(0m);

            var values = await _valueEntries.GetListAsync(v => v.DocumentNo == received.ReceiptNo || v.DocumentNo == invoiced.InvoiceNo);
            values.Sum(v => v.CostAmountExpected).ShouldBe(0m);
            values.Sum(v => v.CostAmountActual).ShouldBe(1000m);
        });
    }

    [Fact]
    public async Task A_Closed_Purchase_Order_Is_Not_Posted()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var vendor = await _vendors.GetByNoAsync("V00010");
            var order = await _purchaseDocuments.CreateAsync(new CreateUpdatePurchaseHeaderDto
            {
                DocumentType = PurchaseDocumentType.Order,
                No = "PO-CLOSED-1",
                VendorId = vendor.Id,
                PostingDate = PostingDate,
                Lines = [new PurchaseLineInputDto { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle", Quantity = 1, DirectUnitCost = 100m }],
            });

            await _purchaseDocuments.CloseOrderAsync(order.Id, new CloseCancelPurchaseOrderInput { Comments = "No longer needed" });

            var refused = await Should.ThrowAsync<BusinessException>(() => _purchaseDocuments.RunPostingAsync(order.Id));
            refused.Code.ShouldBe(ErpErrorCodes.Documents.OrderClosed);
        });
    }

    [Fact]
    public async Task A_Price_List_Prices_Item_Lines_Sent_Without_A_Price()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var list = await _priceLists.CreateAsync(new CreateUpdatePriceListDto
            {
                Code = "VOLUME",
                Description = "Volume prices",
                PriceType = PriceType.Sale,
                SourceType = PriceSourceType.AllCustomers,
                StartingDate = new DateTime(2026, 1, 1),
            });
            await _priceListLines.CreateAsync(new CreateUpdatePriceListLineDto
            {
                PriceListId = list.Id,
                AssetType = PriceAssetType.Item,
                AssetNo = "1000",
                MinimumQuantity = 5,
                UnitPrice = 250m,
                LineDiscountPercent = 5m,
            });

            // A draft list prices nothing.
            var draft = await _priceLists.GetPriceQuoteAsync(new GetPriceInput { PriceType = PriceType.Sale, PartyNo = "C00010", ItemNo = "1000", Quantity = 6, Date = PostingDate });
            draft.PriceListCode.ShouldBeNull();

            await _priceLists.UpdateAsync(list.Id, new CreateUpdatePriceListDto
            {
                Code = "VOLUME",
                Description = "Volume prices",
                PriceType = PriceType.Sale,
                SourceType = PriceSourceType.AllCustomers,
                StartingDate = new DateTime(2026, 1, 1),
                Status = PriceListStatus.Active,
            });

            var small = await _priceLists.GetPriceQuoteAsync(new GetPriceInput { PriceType = PriceType.Sale, PartyNo = "C00010", ItemNo = "1000", Quantity = 2, Date = PostingDate });
            small.PriceListCode.ShouldBeNull();

            var input = await SalesDocumentAsync("ORD-PRICE-1", SalesDocumentType.Order, quantity: 6);
            input.Lines[0].UnitPrice = 0m;
            var order = await _salesDocuments.CreateAsync(input);
            order.Lines.Single().UnitPrice.ShouldBe(250m);
            order.Lines.Single().LineDiscountPercent.ShouldBe(5m);
            order.Lines.Single().LineAmount.ShouldBe(1425m);

            // An active list cannot be changed under the documents that use it.
            var locked = await Should.ThrowAsync<BusinessException>(() => _priceListLines.CreateAsync(new CreateUpdatePriceListLineDto
            {
                PriceListId = list.Id,
                AssetType = PriceAssetType.Item,
                AssetNo = "1000",
                UnitPrice = 1m,
            }));
            locked.Code.ShouldBe(ErpErrorCodes.Pricing.PriceListNotEditable);
        });
    }

    [Fact]
    public async Task A_Shipped_Line_Keeps_Its_Item_And_Cannot_Go_Below_What_Was_Shipped()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var input = await SalesDocumentAsync("ORD-EDIT-1", SalesDocumentType.Order, quantity: 5);
            var order = await _salesDocuments.CreateAsync(input);
            var lineId = order.Lines.Single().Id;
            await _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput
            {
                Ship = true,
                Invoice = false,
                Lines = [new SalesLineQuantitiesInput { LineId = lineId, QtyToShip = 3, QtyToInvoice = 0 }],
            });
            await _salesDocuments.ReopenAsync(order.Id);

            input.Lines[0].Id = lineId;
            input.Lines[0].Quantity = 2;
            var below = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.UpdateAsync(order.Id, input));
            below.Code.ShouldBe(ErpErrorCodes.Documents.QuantityBelowPosted);

            input.Lines.Clear();
            var removed = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.UpdateAsync(order.Id, input));
            removed.Code.ShouldBe(ErpErrorCodes.Documents.LineAlreadyPosted);
        });
    }

    private static decimal Sum(IEnumerable<GLEntry> entries, string accountNo) =>
        entries.Where(e => e.GLAccountNo == accountNo).Sum(e => e.Amount);

    private async Task<CreateUpdateSalesHeaderDto> SalesDocumentAsync(string no, SalesDocumentType type, decimal quantity)
    {
        var customer = await _customers.GetByNoAsync("C00010");

        return new CreateUpdateSalesHeaderDto
        {
            DocumentType = type,
            No = no,
            CustomerId = customer.Id,
            PostingDate = PostingDate,
            Lines =
            [
                new SalesLineInputDto { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle Assembly", Quantity = quantity, UnitPrice = 300m },
            ],
        };
    }
}
