using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Numbering;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// How purchase documents turn into one another: a quote or a blanket order is made into an order,
/// an invoice takes lines from posted receipts (a credit memo from posted return shipments), and
/// any document can copy the lines of another.
/// </summary>
public class PurchaseDocumentFlow : DomainService
{
    private readonly IRepository<PurchaseHeader, Guid> _headerRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IRepository<PostedPurchaseReceipt, Guid> _receiptRepository;
    private readonly IRepository<PostedPurchaseReceiptLine, Guid> _receiptLineRepository;
    private readonly PurchasesPayablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly DocumentVatCalculator _vatCalculator;

    public PurchaseDocumentFlow(
        IRepository<PurchaseHeader, Guid> headerRepository,
        IRepository<Vendor, Guid> vendorRepository,
        IRepository<PostedPurchaseReceipt, Guid> receiptRepository,
        IRepository<PostedPurchaseReceiptLine, Guid> receiptLineRepository,
        PurchasesPayablesSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        DocumentVatCalculator vatCalculator
    )
    {
        _headerRepository = headerRepository;
        _vendorRepository = vendorRepository;
        _receiptRepository = receiptRepository;
        _receiptLineRepository = receiptLineRepository;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _vatCalculator = vatCalculator;
    }

    /// <summary>
    /// Makes an order from a quote or a blanket order. A quote is used up: its lines move to the
    /// order and the quote is deleted. A blanket order stays: each line gives its quantity to
    /// receive to the order, and its quantity received counts what has been ordered so far.
    /// </summary>
    public async Task<PurchaseHeader> MakeOrderAsync(Guid sourceId, DateTime postingDate)
    {
        var source = await _headerRepository.GetAsync(sourceId);
        if (source.DocumentType is not (PurchaseDocumentType.Quote or PurchaseDocumentType.BlanketOrder))
        {
            throw new BusinessException(ErpErrorCodes.Documents.CannotMakeOrder)
                .WithData("documentNo", source.No)
                .WithData("documentType", source.DocumentType.ToString());
        }

        var blanket = source.DocumentType == PurchaseDocumentType.BlanketOrder;
        var lines = blanket
            ? source.Lines.Where(l => l.QtyToReceive > 0m).ToList()
            : source.Lines.ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Documents.NothingToPost).WithData("documentNo", source.No);
        }

        var vendor = await _vendorRepository.GetAsync(source.VendorId);
        var order = await NewDocumentAsync(PurchaseDocumentType.Order, source, postingDate);

        foreach (var line in lines)
        {
            var quantity = blanket ? line.QtyToReceive : line.Quantity;
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
                line.RecordReceived(quantity);
                line.ResetQuantitiesToPost();
            }
        }

        await _vatCalculator.ApplyAsync(order, vendor);
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
    /// The posted receipt lines of the vendor still waiting for an invoice (return shipment lines
    /// for a credit memo), as candidates for <see cref="GetReceiptLinesAsync"/>.
    /// </summary>
    public async Task<List<(PostedPurchaseReceipt Receipt, PostedPurchaseReceiptLine Line)>> GetOpenReceiptLinesAsync(Guid documentId)
    {
        var document = await _headerRepository.GetAsync(documentId);
        var receipts = await _receiptRepository.GetListAsync(
            r => r.VendorId == document.VendorId && r.Return == document.IsReturn,
            includeDetails: true
        );

        return receipts
            .SelectMany(r => r.Lines.Where(l => l.QtyNotInvoiced > 0m).Select(l => (r, l)))
            .OrderBy(x => x.r.PostingDate)
            .ThenBy(x => x.r.No)
            .ThenBy(x => x.l.LineNo)
            .ToList();
    }

    /// <summary>
    /// Adds posted receipt lines to an invoice (return shipment lines to a credit memo): the goods
    /// are in already, so posting the invoice only invoices them.
    /// </summary>
    public async Task<PurchaseHeader> GetReceiptLinesAsync(Guid documentId, IReadOnlyCollection<Guid> receiptLineIds)
    {
        var document = await _headerRepository.GetAsync(documentId);
        if (document.DocumentType is not (PurchaseDocumentType.Invoice or PurchaseDocumentType.CreditMemo))
        {
            throw new BusinessException(ErpErrorCodes.Documents.WrongDocumentType)
                .WithData("documentNo", document.No)
                .WithData("documentType", document.DocumentType.ToString())
                .WithData("expected", "Invoice or CreditMemo");
        }

        var alreadyOnDocument = document.Lines.Where(l => l.IsFromReceipt).Select(l => l.ReceiptLineId!.Value).ToHashSet();
        foreach (var receiptLineId in receiptLineIds.Distinct().Where(id => !alreadyOnDocument.Contains(id)))
        {
            var receiptLine = await _receiptLineRepository.GetAsync(receiptLineId);
            var receipt = await _receiptRepository.GetAsync(receiptLine.PostedPurchaseReceiptId, includeDetails: false);

            if (receipt.VendorId != document.VendorId || receipt.Return != document.IsReturn)
            {
                throw new BusinessException(ErpErrorCodes.Documents.PartyMismatch)
                    .WithData("sourceNo", receipt.No)
                    .WithData("sourceParty", receipt.BuyFromVendorNo)
                    .WithData("party", document.BuyFromVendorNo)
                    .WithData("documentNo", document.No);
            }
            if (receiptLine.QtyNotInvoiced <= 0m)
            {
                throw new BusinessException(ErpErrorCodes.Documents.ShipmentLineFullyInvoiced)
                    .WithData("documentNo", receipt.No)
                    .WithData("lineNo", receiptLine.LineNo);
            }

            var line = document.AddLine(
                GuidGenerator.Create(),
                receiptLine.Type,
                receiptLine.No,
                receiptLine.Description,
                receiptLine.QtyNotInvoiced,
                receiptLine.DirectUnitCost,
                receiptLine.LineDiscountPercent,
                receiptLine.UnitOfMeasureCode
            );
            line.SetLocation(receiptLine.LocationCode);
            line.SetVariant(receiptLine.VariantCode);
            line.SetQtyPerUnitOfMeasure(receiptLine.QtyPerUnitOfMeasure);
            line.LinkToReceiptLine(receiptLine.Id, receipt.No, receiptLine.LineNo);
        }

        var vendor = await _vendorRepository.GetAsync(document.VendorId);
        await _vatCalculator.ApplyAsync(document, vendor);
        document.RecalculateTotals();
        await _headerRepository.UpdateAsync(document, autoSave: true);
        return document;
    }

    /// <summary>Appends the lines of another unposted purchase document, at its costs.</summary>
    public async Task<PurchaseHeader> CopyLinesAsync(Guid targetId, Guid sourceId)
    {
        var target = await _headerRepository.GetAsync(targetId);
        var source = await _headerRepository.GetAsync(sourceId);

        foreach (var line in source.Lines.Where(l => !l.IsFromReceipt))
        {
            CopyLine(target, line, line.Quantity);
        }

        var vendor = await _vendorRepository.GetAsync(target.VendorId);
        await _vatCalculator.ApplyAsync(target, vendor);
        await _headerRepository.UpdateAsync(target, autoSave: true);
        return target;
    }

    private async Task<PurchaseHeader> NewDocumentAsync(PurchaseDocumentType documentType, PurchaseHeader from, DateTime postingDate)
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

        var document = new PurchaseHeader(GuidGenerator.Create(), documentType, no, from.VendorId, from.BuyFromVendorNo, from.BuyFromVendorName, postingDate);
        document.SetDates(postingDate, from.DueDate, from.ExpectedReceiptDate);
        document.SetCurrency(from.CurrencyCode);
        document.SetPaymentTerms(from.PaymentTermsCode);
        document.SetLocation(from.LocationCode);
        document.SetYourReference(from.YourReference);
        document.SetPayTo(from.PayToVendorNo, from.PayToName, from.PayToAddress, from.PayToCity, from.PayToPostCode, from.PayToCountryRegionCode, from.PayToContact);
        document.SetShipTo(from.ShipToCode, from.ShipToName, from.ShipToAddress, from.ShipToCity, from.ShipToPostCode, from.ShipToCountryRegionCode, from.ShipToContact);
        document.SetShipmentMethod(from.ShipmentMethodCode);
        document.SetPaymentMethod(from.PaymentMethodCode);
        document.SetDimensions(from.ShortcutDimension1Code, from.ShortcutDimension2Code);
        document.SetPostingGroups(from.VendorPostingGroup, from.GenBusPostingGroup, from.VatBusPostingGroup);
        document.SetPricesIncludingVat(from.PricesIncludingVat);
        document.SetTax(from.TaxAreaCode, from.TaxLiable);
        document.SetProcurementDetails(from.RequisitionNo, from.TenderNo, from.ContractCommencementDate, from.ContractCompletionDate);
        if (documentType == PurchaseDocumentType.Order)
        {
            document.SetOrderStage(ABPmicroservice.Erp.Procurement.PurchaseOrderStage.Draft);
        }
        return document;
    }

    private PurchaseLine CopyLine(PurchaseHeader target, PurchaseLine line, decimal quantity)
    {
        var copy = target.AddLine(
            GuidGenerator.Create(),
            line.Type,
            line.No,
            line.Description,
            quantity,
            line.DirectUnitCost,
            line.LineDiscountPercent,
            line.UnitOfMeasureCode
        );
        copy.SetLocation(line.LocationCode);
        copy.SetExpectedReceiptDate(line.ExpectedReceiptDate);
        copy.SetVariant(line.VariantCode);
        copy.SetQtyPerUnitOfMeasure(line.QtyPerUnitOfMeasure);
        if (line.ItemCategoryCode != null)
        {
            copy.SetItemCategory(line.ItemCategoryCode);
        }
        copy.SetDimensions(line.ShortcutDimension1Code, line.ShortcutDimension2Code);
        copy.SetPostingGroups(line.GenBusPostingGroup, line.GenProdPostingGroup);
        copy.SetTax(line.TaxAreaCode, line.TaxLiable, line.TaxGroupCode);
        copy.SetDeferralCode(line.DeferralCode);
        return copy;
    }
}
