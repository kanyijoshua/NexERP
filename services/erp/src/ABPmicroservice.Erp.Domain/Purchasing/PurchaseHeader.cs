using System;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Documents;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Purchase document header. Mirrors Business Central table 38 "Purchase Header".
/// Aggregate root that owns its <see cref="PurchaseLine"/> collection.
/// </summary>
public class PurchaseHeader : FullAuditedAggregateRoot<Guid>
{
    public PurchaseDocumentType DocumentType { get; private set; }

    /// <summary>Business key. Mirrors BC field "No.".</summary>
    public string No { get; private set; }

    public Guid VendorId { get; private set; }

    public string BuyFromVendorNo { get; private set; }

    public string BuyFromVendorName { get; private set; }

    public string PayToVendorNo { get; private set; }

    public DateTime PostingDate { get; private set; }

    public DateTime OrderDate { get; private set; }

    public DateTime? DueDate { get; private set; }

    public DateTime? ExpectedReceiptDate { get; private set; }

    public DocumentStatus Status { get; private set; }

    public string CurrencyCode { get; private set; }

    public string PaymentTermsCode { get; private set; }

    public string VendorInvoiceNo { get; private set; }

    public decimal TotalAmount { get; private set; }

    public decimal TotalAmountIncludingVat { get; private set; }

    public bool Posted { get; private set; }

    public string PostedDocumentNo { get; private set; }

    public Collection<PurchaseLine> Lines { get; private set; }

    protected PurchaseHeader()
    {
        Lines = new Collection<PurchaseLine>();
    }

    public PurchaseHeader(
        Guid id,
        PurchaseDocumentType documentType,
        string no,
        Guid vendorId,
        string buyFromVendorNo,
        string buyFromVendorName,
        DateTime postingDate
    )
        : base(id)
    {
        DocumentType = documentType;
        SetNo(no);
        VendorId = vendorId;
        BuyFromVendorNo = Check.NotNullOrWhiteSpace(
            buyFromVendorNo,
            nameof(buyFromVendorNo),
            ErpDomainConsts.MaxNoLength
        );
        BuyFromVendorName = Check.Length(
            buyFromVendorName,
            nameof(buyFromVendorName),
            ErpDomainConsts.MaxNameLength
        );
        PayToVendorNo = buyFromVendorNo;
        PostingDate = postingDate;
        OrderDate = postingDate;
        Status = DocumentStatus.Open;
        Posted = false;
        Lines = new Collection<PurchaseLine>();
    }

    public void SetNo(string no) =>
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength);

    public void SetDates(DateTime postingDate, DateTime? dueDate, DateTime? expectedReceiptDate)
    {
        EnsureNotPosted();
        PostingDate = postingDate;
        DueDate = dueDate;
        ExpectedReceiptDate = expectedReceiptDate;
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

    public void SetCurrency(string currencyCode)
    {
        EnsureNotPosted();
        CurrencyCode = Check.Length(
            currencyCode,
            nameof(currencyCode),
            ErpDomainConsts.MaxCurrencyCodeLength
        );
    }

    public void SetVendorInvoiceNo(string vendorInvoiceNo)
    {
        EnsureNotPosted();
        VendorInvoiceNo = Check.Length(
            vendorInvoiceNo,
            nameof(vendorInvoiceNo),
            ErpDomainConsts.MaxExternalDocumentNoLength
        );
    }

    public PurchaseLine AddLine(
        Guid lineId,
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal directUnitCost,
        decimal lineDiscountPercent = 0m,
        string unitOfMeasureCode = null
    )
    {
        EnsureNotPosted();
        EnsureOpen();

        var line = new PurchaseLine(
            lineId,
            Id,
            Lines.Count + 1,
            type,
            no,
            description,
            quantity,
            directUnitCost,
            lineDiscountPercent,
            unitOfMeasureCode
        );

        Lines.Add(line);
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
            Lines.Remove(line);
            RecalculateTotals();
        }
    }

    public void ClearLines()
    {
        EnsureNotPosted();
        EnsureOpen();
        Lines.Clear();
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        TotalAmount = Lines.Sum(l => l.LineAmount);
        TotalAmountIncludingVat = Lines.Sum(l => l.LineAmountIncludingVat);
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
