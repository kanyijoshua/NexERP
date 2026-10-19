using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Numbering;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// How sales documents turn into one another: a quote or a blanket order is made into an order,
/// an invoice takes lines from posted shipments (a credit memo from posted return receipts), and
/// any document can copy the lines of another.
/// </summary>
public class SalesDocumentFlow : DomainService
{
    private readonly IRepository<SalesHeader, Guid> _headerRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<PostedSalesShipment, Guid> _shipmentRepository;
    private readonly IRepository<PostedSalesShipmentLine, Guid> _shipmentLineRepository;
    private readonly SalesReceivablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly DocumentVatCalculator _vatCalculator;

    public SalesDocumentFlow(
        IRepository<SalesHeader, Guid> headerRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<PostedSalesShipment, Guid> shipmentRepository,
        IRepository<PostedSalesShipmentLine, Guid> shipmentLineRepository,
        SalesReceivablesSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        DocumentVatCalculator vatCalculator
    )
    {
        _headerRepository = headerRepository;
        _customerRepository = customerRepository;
        _shipmentRepository = shipmentRepository;
        _shipmentLineRepository = shipmentLineRepository;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _vatCalculator = vatCalculator;
    }

    /// <summary>
    /// Makes an order from a quote or a blanket order. A quote is used up: its lines move to the
    /// order and the quote is deleted. A blanket order stays: each line gives its quantity to ship
    /// to the order, and its quantity shipped counts what has been ordered so far.
    /// </summary>
    public async Task<SalesHeader> MakeOrderAsync(Guid sourceId, DateTime postingDate)
    {
        var source = await _headerRepository.GetAsync(sourceId);
        if (source.DocumentType is not (SalesDocumentType.Quote or SalesDocumentType.BlanketOrder))
        {
            throw new BusinessException(ErpErrorCodes.Documents.CannotMakeOrder)
                .WithData("documentNo", source.No)
                .WithData("documentType", source.DocumentType.ToString());
        }

        var blanket = source.DocumentType == SalesDocumentType.BlanketOrder;
        var lines = blanket
            ? source.Lines.Where(l => l.QtyToShip > 0m).ToList()
            : source.Lines.ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Documents.NothingToPost).WithData("documentNo", source.No);
        }

        var customer = await _customerRepository.GetAsync(source.CustomerId);
        var order = await NewDocumentAsync(SalesDocumentType.Order, source, postingDate);
        if (!blanket)
        {
            order.SetQuoteNo(source.No);
        }

        foreach (var line in lines)
        {
            var quantity = blanket ? line.QtyToShip : line.Quantity;
            if (blanket && quantity > line.OutstandingQuantity)
            {
                throw new BusinessException(ErpErrorCodes.Documents.BlanketQuantityExceeded)
                    .WithData("documentNo", source.No)
                    .WithData("lineNo", line.LineNo)
                    .WithData("quantity", quantity)
                    .WithData("outstanding", line.OutstandingQuantity);
            }

            var orderLine = CopyLine(order, line, quantity);
            if (blanket)
            {
                orderLine.LinkToBlanketOrder(source.No, line.LineNo);
                line.RecordShipped(quantity);
                line.ResetQuantitiesToPost();
            }
        }

        await _vatCalculator.ApplyAsync(order, customer);
        await _headerRepository.InsertAsync(order, autoSave: true);

        if (blanket)
        {
            source.RecalculateTotals();
            await _headerRepository.UpdateAsync(source, autoSave: true);
        }
        else
        {
            await _headerRepository.DeleteAsync(source, autoSave: true);
        }

        return order;
    }

    /// <summary>
    /// The posted shipment lines of the customer still waiting for an invoice (return receipt lines
    /// for a credit memo), as candidates for <see cref="GetShipmentLinesAsync"/>.
    /// </summary>
    public async Task<List<(PostedSalesShipment Shipment, PostedSalesShipmentLine Line)>> GetOpenShipmentLinesAsync(Guid documentId)
    {
        var document = await _headerRepository.GetAsync(documentId);
        var shipments = await _shipmentRepository.GetListAsync(
            s => s.CustomerId == document.CustomerId && s.Return == document.IsReturn,
            includeDetails: true
        );

        return shipments
            .SelectMany(s => s.Lines.Where(l => l.QtyNotInvoiced > 0m).Select(l => (s, l)))
            .OrderBy(x => x.s.PostingDate)
            .ThenBy(x => x.s.No)
            .ThenBy(x => x.l.LineNo)
            .ToList();
    }

    /// <summary>
    /// Adds posted shipment lines to an invoice (return receipt lines to a credit memo): the goods
    /// have already left, so posting the invoice only invoices them.
    /// </summary>
    public async Task<SalesHeader> GetShipmentLinesAsync(Guid documentId, IReadOnlyCollection<Guid> shipmentLineIds)
    {
        var document = await _headerRepository.GetAsync(documentId);
        if (document.DocumentType is not (SalesDocumentType.Invoice or SalesDocumentType.CreditMemo))
        {
            throw new BusinessException(ErpErrorCodes.Documents.WrongDocumentType)
                .WithData("documentNo", document.No)
                .WithData("documentType", document.DocumentType.ToString())
                .WithData("expected", "Invoice or CreditMemo");
        }

        var alreadyOnDocument = document.Lines.Where(l => l.IsFromShipment).Select(l => l.ShipmentLineId!.Value).ToHashSet();
        foreach (var shipmentLineId in shipmentLineIds.Distinct().Where(id => !alreadyOnDocument.Contains(id)))
        {
            var shipmentLine = await _shipmentLineRepository.GetAsync(shipmentLineId);
            var shipment = await _shipmentRepository.GetAsync(shipmentLine.PostedSalesShipmentId, includeDetails: false);

            if (shipment.CustomerId != document.CustomerId || shipment.Return != document.IsReturn)
            {
                throw new BusinessException(ErpErrorCodes.Documents.PartyMismatch)
                    .WithData("sourceNo", shipment.No)
                    .WithData("sourceParty", shipment.SellToCustomerNo)
                    .WithData("party", document.SellToCustomerNo)
                    .WithData("documentNo", document.No);
            }
            if (shipmentLine.QtyNotInvoiced <= 0m)
            {
                throw new BusinessException(ErpErrorCodes.Documents.ShipmentLineFullyInvoiced)
                    .WithData("documentNo", shipment.No)
                    .WithData("lineNo", shipmentLine.LineNo);
            }

            var line = document.AddLine(
                GuidGenerator.Create(),
                shipmentLine.Type,
                shipmentLine.No,
                shipmentLine.Description,
                shipmentLine.QtyNotInvoiced,
                shipmentLine.UnitPrice,
                shipmentLine.LineDiscountPercent,
                shipmentLine.UnitOfMeasureCode
            );
            line.SetLocation(shipmentLine.LocationCode);
            line.SetVariant(shipmentLine.VariantCode);
            line.SetQtyPerUnitOfMeasure(shipmentLine.QtyPerUnitOfMeasure);
            line.LinkToShipmentLine(shipmentLine.Id, shipment.No, shipmentLine.LineNo);
        }

        var customer = await _customerRepository.GetAsync(document.CustomerId);
        await _vatCalculator.ApplyAsync(document, customer);
        document.RecalculateTotals();
        await _headerRepository.UpdateAsync(document, autoSave: true);
        return document;
    }

    /// <summary>Appends the lines of another unposted sales document, at its prices.</summary>
    public async Task<SalesHeader> CopyLinesAsync(Guid targetId, Guid sourceId)
    {
        var target = await _headerRepository.GetAsync(targetId);
        var source = await _headerRepository.GetAsync(sourceId);

        foreach (var line in source.Lines.Where(l => !l.IsFromShipment))
        {
            CopyLine(target, line, line.Quantity);
        }

        var customer = await _customerRepository.GetAsync(target.CustomerId);
        await _vatCalculator.ApplyAsync(target, customer);
        await _headerRepository.UpdateAsync(target, autoSave: true);
        return target;
    }

    private async Task<SalesHeader> NewDocumentAsync(SalesDocumentType documentType, SalesHeader from, DateTime postingDate)
    {
        var setup = await _setupManager.GetAsync();
        var series = setup.GetDocumentNos(documentType);
        string no;
        if (series.IsNullOrWhiteSpace())
        {
            var count = await _headerRepository.CountAsync(h => h.DocumentType == documentType && h.No.StartsWith(from.No));
            no = count == 0 ? $"{from.No}-O" : $"{from.No}-O{count + 1}";
        }
        else
        {
            no = await _noSeriesManager.GetNextNoAsync(series, postingDate);
        }

        var document = new SalesHeader(GuidGenerator.Create(), documentType, no, from.CustomerId, from.SellToCustomerNo, from.SellToCustomerName, postingDate);
        document.SetDates(postingDate, from.DueDate);
        document.SetCurrency(from.CurrencyCode);
        document.SetPaymentTerms(from.PaymentTermsCode);
        document.SetLocation(from.LocationCode);
        document.SetExternalDocumentNo(from.ExternalDocumentNo);
        document.SetShipTo(from.ShipToCode, from.ShipToName, from.ShipToAddress, from.ShipToCity, from.ShipToPostCode, from.ShipToCountryRegionCode, from.ShipToContact);
        document.SetShipmentDate(from.ShipmentDate);
        document.SetCustomerPriceGroup(from.CustomerPriceGroup);
        return document;
    }

    private SalesLine CopyLine(SalesHeader target, SalesLine line, decimal quantity)
    {
        var copy = target.AddLine(
            GuidGenerator.Create(),
            line.Type,
            line.No,
            line.Description,
            quantity,
            line.UnitPrice,
            line.LineDiscountPercent,
            line.UnitOfMeasureCode
        );
        copy.SetLocation(line.LocationCode);
        copy.SetShipmentDate(line.ShipmentDate);
        copy.SetVariant(line.VariantCode);
        copy.SetQtyPerUnitOfMeasure(line.QtyPerUnitOfMeasure);
        return copy;
    }
}
