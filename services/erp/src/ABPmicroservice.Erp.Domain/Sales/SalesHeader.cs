using ABPmicroservice.Erp.Companies;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Documents;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Sales document header.
/// Aggregate root that owns its <see cref="SalesLine"/> collection.
/// </summary>
public class SalesHeader : CompanyAggregateRoot, IApprovalDocument
{
    public SalesDocumentType DocumentType { get; private set; }

    /// <summary>Business key.</summary>
    public string No { get; private set; }

    public Guid CustomerId { get; private set; }

    public string SellToCustomerNo { get; private set; }

    public string SellToCustomerName { get; private set; }

    public string BillToCustomerNo { get; private set; }

    public DateTime PostingDate { get; private set; }

    public DateTime OrderDate { get; private set; }

    public DateTime? DueDate { get; private set; }

    public DocumentStatus Status { get; private set; }

    public string CurrencyCode { get; private set; }

    public string PaymentTermsCode { get; private set; }

    /// <summary>Where the stock of the document's item lines is shipped from or received at.</summary>
    public string LocationCode { get; private set; }

    public string ExternalDocumentNo { get; private set; }

    /// <summary>Total amount (denormalized, recalculated from lines).</summary>
    public decimal TotalAmount { get; private set; }

    public decimal TotalAmountIncludingVat { get; private set; }

    public bool Posted { get; private set; }

    public string PostedDocumentNo { get; private set; }

    public string ShipToCode { get; private set; }
    public string ShipToName { get; private set; }
    public string ShipToAddress { get; private set; }
    public string ShipToCity { get; private set; }
    public string ShipToPostCode { get; private set; }
    public string ShipToCountryRegionCode { get; private set; }
    public string ShipToContact { get; private set; }

    /// <summary>When the goods are expected to leave; lines without their own date take this one.</summary>
    public DateTime? ShipmentDate { get; private set; }

    /// <summary>The price group the customer's prices are looked up under.</summary>
    public string CustomerPriceGroup { get; private set; }

    /// <summary>The quote this order was made from.</summary>
    public string QuoteNo { get; private set; }

    /// <summary>The last shipment (or return receipt) posted from this order.</summary>
    public string LastShippingNo { get; private set; }

    /// <summary>The last invoice (or credit memo) posted from this order.</summary>
    public string LastPostingNo { get; private set; }

    public decimal PrepaymentPct { get; private set; }
    public decimal PrepmtInvoicedAmount { get; private set; }
    public decimal PrepmtDeductedAmount { get; private set; }
    public string LastPrepmtInvoiceNo { get; private set; }
    public string LastPrepmtCrMemoNo { get; private set; }

    public Collection<SalesLine> Lines { get; private set; }

    /// <summary>An order or return order: shipped and invoiced in parts rather than in one go.</summary>
    public bool IsOrderType => DocumentType is SalesDocumentType.Order or SalesDocumentType.ReturnOrder;

    /// <summary>A credit memo or return order: posts with every sign turned round.</summary>
    public bool IsReturn => DocumentType is SalesDocumentType.CreditMemo or SalesDocumentType.ReturnOrder;

    public bool IsCompletelyInvoiced => Lines.All(l => l.QuantityInvoiced == l.Quantity);

    protected SalesHeader()
    {
        Lines = new Collection<SalesLine>();
    }

    public SalesHeader(
        Guid id,
        SalesDocumentType documentType,
        string no,
        Guid customerId,
        string sellToCustomerNo,
        string sellToCustomerName,
        DateTime postingDate
    )
        : base(id)
    {
        DocumentType = documentType;
        SetNo(no);
        CustomerId = customerId;
        SellToCustomerNo = Check.NotNullOrWhiteSpace(
            sellToCustomerNo,
            nameof(sellToCustomerNo),
            ErpDomainConsts.MaxNoLength
        );
        SellToCustomerName = Check.Length(
            sellToCustomerName,
            nameof(sellToCustomerName),
            ErpDomainConsts.MaxNameLength
        );
        BillToCustomerNo = sellToCustomerNo;
        PostingDate = postingDate;
        OrderDate = postingDate;
        Status = DocumentStatus.Open;
        Posted = false;
        Lines = new Collection<SalesLine>();
    }

    public void SetNo(string no) =>
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength);

    public void SetDates(DateTime postingDate, DateTime? dueDate)
    {
        EnsureNotPosted();
        PostingDate = postingDate;
        DueDate = dueDate;
    }

    public void SetPaymentTerms(string paymentTermsCode)
    {
        EnsureNotPosted();
        PaymentTermsCode = Check.Length(
            paymentTermsCode,
            nameof(paymentTermsCode),
            ErpDomainConsts.MaxPaymentTermsCodeLength
        );
    }

    public void SetLocation(string locationCode)
    {
        EnsureNotPosted();
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
    }

    public void SetCurrency(string currencyCode)
    {
        EnsureNotPosted();
        CurrencyCode = Check.Length(
            currencyCode,
            nameof(currencyCode),
            ErpDomainConsts.MaxCurrencyCodeLength
        );
    }

    public void SetExternalDocumentNo(string externalDocumentNo)
    {
        EnsureNotPosted();
        ExternalDocumentNo = Check.Length(
            externalDocumentNo,
            nameof(externalDocumentNo),
            ErpDomainConsts.MaxExternalDocumentNoLength
        );
    }

    public void SetShipTo(
        string shipToCode,
        string shipToName = null,
        string shipToAddress = null,
        string shipToCity = null,
        string shipToPostCode = null,
        string shipToCountryRegionCode = null,
        string shipToContact = null
    )
    {
        EnsureNotPosted();
        ShipToCode = CodeTableEntity.NormalizeCode(Check.Length(shipToCode, nameof(shipToCode), ErpDomainConsts.MaxLocationCodeLength));
        ShipToName = Check.Length(shipToName, nameof(shipToName), ErpDomainConsts.MaxNameLength);
        ShipToAddress = Check.Length(shipToAddress, nameof(shipToAddress), ErpDomainConsts.MaxAddressLength);
        ShipToCity = Check.Length(shipToCity, nameof(shipToCity), ErpDomainConsts.MaxCityLength);
        ShipToPostCode = Check.Length(shipToPostCode, nameof(shipToPostCode), ErpDomainConsts.MaxPostCodeLength);
        ShipToCountryRegionCode = CodeTableEntity.NormalizeCode(Check.Length(shipToCountryRegionCode, nameof(shipToCountryRegionCode), ErpDomainConsts.MaxCountryRegionCodeLength));
        ShipToContact = Check.Length(shipToContact, nameof(shipToContact), ErpDomainConsts.MaxContactLength);
    }

    public void SetShipmentDate(DateTime? shipmentDate)
    {
        EnsureNotPosted();
        ShipmentDate = shipmentDate?.Date;
    }

    public void SetCustomerPriceGroup(string customerPriceGroup)
    {
        EnsureNotPosted();
        CustomerPriceGroup = CodeTableEntity.NormalizeCode(Check.Length(customerPriceGroup, nameof(customerPriceGroup), ErpDomainConsts.MaxPostingGroupLength));
    }

    public void SetQuoteNo(string quoteNo)
    {
        QuoteNo = Check.Length(quoteNo, nameof(quoteNo), ErpDomainConsts.MaxDocumentNoLength);
    }

    public void SetPrepayment(decimal prepaymentPct)
    {
        EnsureNotPosted();
        PrepaymentPct = Math.Clamp(prepaymentPct, 0m, 100m);
        foreach (var line in Lines.Where(l => l.PrepmtAmtInv == 0m))
        {
            line.SetPrepaymentPct(PrepaymentPct);
        }
        RecalculateTotals();
    }

    public SalesLine AddLine(
        Guid lineId,
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal lineDiscountPercent = 0m,
        string unitOfMeasureCode = null
    )
    {
        EnsureNotPosted();
        EnsureOpen();

        var line = new SalesLine(
            lineId,
            Id,
            Lines.Count == 0 ? 1 : Lines.Max(l => l.LineNo) + 1,
            type,
            no,
            description,
            quantity,
            unitPrice,
            lineDiscountPercent,
            unitOfMeasureCode
        );

        if (PrepaymentPct > 0m)
        {
            line.SetPrepaymentPct(PrepaymentPct);
        }

        Lines.Add(line);
        RecalculateTotals();
        return line;
    }

    /// <summary>Changes a line in place, so what has been shipped and invoiced on it is kept.</summary>
    public SalesLine UpdateLine(
        Guid lineId,
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal lineDiscountPercent = 0m,
        string unitOfMeasureCode = null
    )
    {
        EnsureNotPosted();
        EnsureOpen();

        var line = GetLine(lineId);
        line.Update(No, type, no, description, quantity, unitPrice, lineDiscountPercent, unitOfMeasureCode);
        RecalculateTotals();
        return line;
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureNotPosted();
        EnsureOpen();

        var line = Lines.FirstOrDefault(l => l.Id == lineId);
        if (line != null)
        {
            EnsureNoPostedQuantity(line);
            Lines.Remove(line);
            RecalculateTotals();
        }
    }

    public void ClearLines()
    {
        EnsureNotPosted();
        EnsureOpen();
        foreach (var line in Lines)
        {
            EnsureNoPostedQuantity(line);
        }
        Lines.Clear();
        RecalculateTotals();
    }

    public SalesLine GetLine(Guid lineId)
    {
        return Lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new BusinessException(ErpErrorCodes.Documents.LineNotFound)
                .WithData("documentNo", No)
                .WithData("lineId", lineId);
    }

    /// <summary>
    /// What the next posting of an order ships and invoices. Unlike the rest of the document this
    /// can be changed while the order is released, which is how a partial shipment is posted.
    /// </summary>
    public void SetQuantitiesToPost(Guid lineId, decimal qtyToShip, decimal qtyToInvoice)
    {
        EnsureNotPosted();
        GetLine(lineId).SetQuantitiesToPost(No, qtyToShip, qtyToInvoice);
    }

    /// <summary>Notes what a posting run produced, and marks the document posted once nothing is left to invoice.</summary>
    internal void RecordPosting(string shippingNo, string postingNo)
    {
        if (shippingNo != null)
        {
            LastShippingNo = shippingNo;
        }
        if (postingNo != null)
        {
            LastPostingNo = postingNo;
        }

        foreach (var line in Lines)
        {
            line.ResetQuantitiesToPost();
        }

        RecalculateTotals();

        if (IsCompletelyInvoiced && postingNo != null)
        {
            MarkPosted(postingNo);
        }
    }

    private void EnsureNoPostedQuantity(SalesLine line)
    {
        if (line.HasPostedQuantity && !line.IsFromShipment)
        {
            throw new BusinessException(ErpErrorCodes.Documents.LineAlreadyPosted)
                .WithData("documentNo", No)
                .WithData("lineNo", line.LineNo);
        }
    }

    public void RecalculateTotals()
    {
        TotalAmount = Lines.Sum(l => l.LineAmount);
        TotalAmountIncludingVat = Lines.Sum(l => l.LineAmountIncludingVat);
        PrepmtInvoicedAmount = Lines.Sum(l => l.PrepmtAmtInv);
        PrepmtDeductedAmount = Lines.Sum(l => l.PrepmtAmtDeducted);
    }

    internal void RecordPrepaymentPosting(string documentNo, bool creditMemo)
    {
        if (creditMemo)
        {
            LastPrepmtCrMemoNo = documentNo;
        }
        else
        {
            LastPrepmtInvoiceNo = documentNo;
        }
        RecalculateTotals();
    }

    public bool HasLines => Lines.Count > 0;

    public decimal ApprovalAmount => TotalAmount;

    /// <summary>Open -> Pending Approval. The document is frozen until the request is settled.</summary>
    public void SendForApproval()
    {
        EnsureNotPosted();
        EnsureOpen();
        Status = DocumentStatus.PendingApproval;
    }

    public void Release()
    {
        EnsureNotPosted();
        Status = DocumentStatus.Released;
    }

    public void Reopen()
    {
        EnsureNotPosted();
        Status = DocumentStatus.Open;
    }

    internal void MarkPosted(string postedDocumentNo)
    {
        Posted = true;
        Status = DocumentStatus.Posted;
        PostedDocumentNo = postedDocumentNo;
    }

    private void EnsureOpen()
    {
        if (Status != DocumentStatus.Open)
        {
            throw new DocumentNotOpenException(No);
        }
    }

    private void EnsureNotPosted()
    {
        if (Posted)
        {
            throw new DocumentAlreadyPostedException(No);
        }
    }
}
