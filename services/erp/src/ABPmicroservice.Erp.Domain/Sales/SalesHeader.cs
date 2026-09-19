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
/// Sales document header. Mirrors Business Central table 36 "Sales Header".
/// Aggregate root that owns its <see cref="SalesLine"/> collection.
/// </summary>
public class SalesHeader : CompanyAggregateRoot
{
    public SalesDocumentType DocumentType { get; private set; }

    /// <summary>Business key. Mirrors BC field "No.".</summary>
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

    public string ExternalDocumentNo { get; private set; }

    /// <summary>Total amount (denormalized, recalculated from lines).</summary>
    public decimal TotalAmount { get; private set; }

    public decimal TotalAmountIncludingVat { get; private set; }

    public bool Posted { get; private set; }

    public string PostedDocumentNo { get; private set; }

    public Collection<SalesLine> Lines { get; private set; }

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
            Lines.Count + 1,
            type,
            no,
            description,
            quantity,
            unitPrice,
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
