using ABPmicroservice.Erp.Companies;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Documents;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Purchase document header.
/// Aggregate root that owns its <see cref="PurchaseLine"/> collection.
/// </summary>
public class PurchaseHeader : CompanyAggregateRoot, IApprovalDocument
{
    public PurchaseDocumentType DocumentType { get; private set; }

    /// <summary>Business key.</summary>
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

    /// <summary>Where the stock of the document's item lines is shipped from or received at.</summary>
    public string LocationCode { get; private set; }

    public string VendorInvoiceNo { get; private set; }

    public decimal TotalAmount { get; private set; }

    public decimal TotalAmountIncludingVat { get; private set; }

    public bool Posted { get; private set; }

    public string PostedDocumentNo { get; private set; }

    public string YourReference { get; private set; }

    public string PayToName { get; private set; }
    public string PayToAddress { get; private set; }
    public string PayToCity { get; private set; }
    public string PayToPostCode { get; private set; }
    public string PayToCountryRegionCode { get; private set; }
    public string PayToContact { get; private set; }

    public string ShipToCode { get; private set; }
    public string ShipToName { get; private set; }
    public string ShipToAddress { get; private set; }
    public string ShipToCity { get; private set; }
    public string ShipToPostCode { get; private set; }
    public string ShipToCountryRegionCode { get; private set; }
    public string ShipToContact { get; private set; }

    public string ShipmentMethodCode { get; private set; }
    public string PaymentMethodCode { get; private set; }
    public string ShortcutDimension1Code { get; private set; }
    public string ShortcutDimension2Code { get; private set; }

    public string VendorPostingGroup { get; private set; }
    public string GenBusPostingGroup { get; private set; }
    public string VatBusPostingGroup { get; private set; }
    public bool PricesIncludingVat { get; private set; }

    public string OnHold { get; private set; }
    public PurchaseDocumentType? AppliesToDocType { get; private set; }
    public string AppliesToDocNo { get; private set; }
    public string AppliesToId { get; private set; }

    public string TaxAreaCode { get; private set; }
    public bool TaxLiable { get; private set; }
    public decimal PrepaymentPct { get; private set; }

    public Collection<PurchaseLine> Lines { get; private set; }

    /// <summary>Posting Description.</summary>
    public string PostingDescription { get; private set; }

    /// <summary>Document Date.</summary>
    public DateTime? DocumentDate { get; private set; }

    /// <summary>Purchaser Code.</summary>
    public string PurchaserCode { get; private set; }

    /// <summary>Vendor Order No..</summary>
    public string VendorOrderNo { get; private set; }

    /// <summary>Vendor Shipment No..</summary>
    public string VendorShipmentNo { get; private set; }

    /// <summary>Vendor Cr. Memo No..</summary>
    public string VendorCrMemoNo { get; private set; }

    /// <summary>Buy-from Vendor Name 2.</summary>
    public string BuyFromVendorName2 { get; private set; }

    /// <summary>Buy-from Address.</summary>
    public string BuyFromAddress { get; private set; }

    /// <summary>Buy-from Address 2.</summary>
    public string BuyFromAddress2 { get; private set; }

    /// <summary>Buy-from City.</summary>
    public string BuyFromCity { get; private set; }

    /// <summary>Buy-from Post Code.</summary>
    public string BuyFromPostCode { get; private set; }

    /// <summary>Buy-from County.</summary>
    public string BuyFromCounty { get; private set; }

    /// <summary>Buy-from Country/Region Code.</summary>
    public string BuyFromCountryRegionCode { get; private set; }

    /// <summary>Buy-from Contact.</summary>
    public string BuyFromContact { get; private set; }

    /// <summary>Pay-to Name 2.</summary>
    public string PayToName2 { get; private set; }

    /// <summary>Pay-to Address 2.</summary>
    public string PayToAddress2 { get; private set; }

    /// <summary>Pay-to County.</summary>
    public string PayToCounty { get; private set; }

    /// <summary>Ship-to Name 2.</summary>
    public string ShipToName2 { get; private set; }

    /// <summary>Ship-to Address 2.</summary>
    public string ShipToAddress2 { get; private set; }

    /// <summary>Ship-to County.</summary>
    public string ShipToCounty { get; private set; }

    /// <summary>Order Address Code.</summary>
    public string OrderAddressCode { get; private set; }

    /// <summary>VAT Registration No..</summary>
    public string VatRegistrationNo { get; private set; }

    /// <summary>Payment Discount %.</summary>
    public decimal PaymentDiscountPct { get; private set; }

    /// <summary>Pmt. Discount Date.</summary>
    public DateTime? PmtDiscountDate { get; private set; }

    /// <summary>VAT Base Discount %.</summary>
    public decimal VatBaseDiscountPct { get; private set; }

    /// <summary>Language Code.</summary>
    public string LanguageCode { get; private set; }

    /// <summary>Quote No..</summary>
    public string QuoteNo { get; private set; }

    /// <summary>Responsibility Center.</summary>
    public string ResponsibilityCenter { get; private set; }

    /// <summary>Requested Receipt Date.</summary>
    public DateTime? RequestedReceiptDate { get; private set; }

    /// <summary>Promised Receipt Date.</summary>
    public DateTime? PromisedReceiptDate { get; private set; }

    /// <summary>Assigned User ID.</summary>
    public string AssignedUserId { get; private set; }

    /// <summary>Payment Reference.</summary>
    public string PaymentReference { get; private set; }

    /// <summary>Invoice Received Date.</summary>
    public DateTime? InvoiceReceivedDate { get; private set; }

    /// <summary>Creditor No..</summary>
    public string CreditorNo { get; private set; }

    /// <summary>Reason Code.</summary>
    public string ReasonCode { get; private set; }

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
        PayToName = BuyFromVendorName;
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

    public void SetVendorInvoiceNo(string vendorInvoiceNo)
    {
        EnsureNotPosted();
        VendorInvoiceNo = Check.Length(
            vendorInvoiceNo,
            nameof(vendorInvoiceNo),
            ErpDomainConsts.MaxExternalDocumentNoLength
        );
    }

    public void SetYourReference(string yourReference)
    {
        EnsureNotPosted();
        YourReference = Check.Length(yourReference, nameof(yourReference), ErpDomainConsts.MaxYourReferenceLength);
    }

    public void SetPayTo(
        string payToVendorNo,
        string payToName = null,
        string payToAddress = null,
        string payToCity = null,
        string payToPostCode = null,
        string payToCountryRegionCode = null,
        string payToContact = null
    )
    {
        EnsureNotPosted();
        PayToVendorNo = Check.Length(payToVendorNo, nameof(payToVendorNo), ErpDomainConsts.MaxNoLength) ?? BuyFromVendorNo;
        PayToName = Check.Length(payToName, nameof(payToName), ErpDomainConsts.MaxNameLength);
        PayToAddress = Check.Length(payToAddress, nameof(payToAddress), ErpDomainConsts.MaxAddressLength);
        PayToCity = Check.Length(payToCity, nameof(payToCity), ErpDomainConsts.MaxCityLength);
        PayToPostCode = Check.Length(payToPostCode, nameof(payToPostCode), ErpDomainConsts.MaxPostCodeLength);
        PayToCountryRegionCode = CodeTableEntity.NormalizeCode(Check.Length(payToCountryRegionCode, nameof(payToCountryRegionCode), ErpDomainConsts.MaxCountryRegionCodeLength));
        PayToContact = Check.Length(payToContact, nameof(payToContact), ErpDomainConsts.MaxContactLength);
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

    public void SetShipmentMethod(string shipmentMethodCode)
    {
        EnsureNotPosted();
        ShipmentMethodCode = CodeTableEntity.NormalizeCode(Check.Length(shipmentMethodCode, nameof(shipmentMethodCode), ErpDomainConsts.MaxShipmentMethodCodeLength));
    }

    public void SetPaymentMethod(string paymentMethodCode)
    {
        EnsureNotPosted();
        PaymentMethodCode = CodeTableEntity.NormalizeCode(Check.Length(paymentMethodCode, nameof(paymentMethodCode), ErpDomainConsts.MaxPaymentMethodCodeLength));
    }

    public void SetDimensions(string shortcutDimension1Code, string shortcutDimension2Code)
    {
        EnsureNotPosted();
        ShortcutDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(shortcutDimension1Code, nameof(shortcutDimension1Code), ErpDomainConsts.MaxDimensionCodeLength));
        ShortcutDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(shortcutDimension2Code, nameof(shortcutDimension2Code), ErpDomainConsts.MaxDimensionCodeLength));
    }

    public void SetPostingGroups(string vendorPostingGroup, string genBusPostingGroup, string vatBusPostingGroup)
    {
        EnsureNotPosted();
        VendorPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(vendorPostingGroup, nameof(vendorPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
        GenBusPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(genBusPostingGroup, nameof(genBusPostingGroup), ErpDomainConsts.MaxGeneralBusPostingGroupLength));
        VatBusPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(vatBusPostingGroup, nameof(vatBusPostingGroup), ErpDomainConsts.MaxVatBusPostingGroupLength));
    }

    public void SetPricesIncludingVat(bool pricesIncludingVat)
    {
        EnsureNotPosted();
        PricesIncludingVat = pricesIncludingVat;
    }

    public void SetOnHold(string onHold)
    {
        EnsureNotPosted();
        OnHold = Check.Length(onHold, nameof(onHold), ErpDomainConsts.MaxOnHoldLength);
    }

    public void SetAppliesTo(PurchaseDocumentType? appliesToDocType, string appliesToDocNo, string appliesToId = null)
    {
        EnsureNotPosted();
        AppliesToDocType = appliesToDocType;
        AppliesToDocNo = Check.Length(appliesToDocNo, nameof(appliesToDocNo), ErpDomainConsts.MaxDocumentNoLength);
        AppliesToId = Check.Length(appliesToId, nameof(appliesToId), ErpDomainConsts.MaxNoLength);
    }

    public void SetTax(string taxAreaCode, bool taxLiable)
    {
        EnsureNotPosted();
        TaxAreaCode = CodeTableEntity.NormalizeCode(Check.Length(taxAreaCode, nameof(taxAreaCode), ErpDomainConsts.MaxTaxAreaCodeLength));
        TaxLiable = taxLiable;
    }

    public void SetPrepayment(decimal prepaymentPct)
    {
        EnsureNotPosted();
        PrepaymentPct = prepaymentPct;
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

        if (!string.IsNullOrWhiteSpace(LocationCode))
        {
            line.SetLocation(LocationCode);
        }
        if (ExpectedReceiptDate.HasValue)
        {
            line.SetExpectedReceiptDate(ExpectedReceiptDate);
        }

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

    /// <summary>The card fields beyond those the posting routines read.</summary>
    public void SetAdditionalFields(
        string postingDescription,
        DateTime? documentDate,
        string purchaserCode,
        string vendorOrderNo,
        string vendorShipmentNo,
        string vendorCrMemoNo,
        string buyFromVendorName2,
        string buyFromAddress,
        string buyFromAddress2,
        string buyFromCity,
        string buyFromPostCode,
        string buyFromCounty,
        string buyFromCountryRegionCode,
        string buyFromContact,
        string payToName2,
        string payToAddress2,
        string payToCounty,
        string shipToName2,
        string shipToAddress2,
        string shipToCounty,
        string orderAddressCode,
        string vatRegistrationNo,
        decimal paymentDiscountPct,
        DateTime? pmtDiscountDate,
        decimal vatBaseDiscountPct,
        string languageCode,
        string quoteNo,
        string responsibilityCenter,
        DateTime? requestedReceiptDate,
        DateTime? promisedReceiptDate,
        string assignedUserId,
        string paymentReference,
        DateTime? invoiceReceivedDate,
        string creditorNo,
        string reasonCode
    )
    {
        PostingDescription = Check.Length(postingDescription, nameof(postingDescription), 100);
        DocumentDate = documentDate?.Date;
        PurchaserCode = CodeTableEntity.NormalizeCode(Check.Length(purchaserCode, nameof(purchaserCode), 20));
        VendorOrderNo = CodeTableEntity.NormalizeCode(Check.Length(vendorOrderNo, nameof(vendorOrderNo), 35));
        VendorShipmentNo = CodeTableEntity.NormalizeCode(Check.Length(vendorShipmentNo, nameof(vendorShipmentNo), 35));
        VendorCrMemoNo = CodeTableEntity.NormalizeCode(Check.Length(vendorCrMemoNo, nameof(vendorCrMemoNo), 35));
        BuyFromVendorName2 = Check.Length(buyFromVendorName2, nameof(buyFromVendorName2), 50);
        BuyFromAddress = Check.Length(buyFromAddress, nameof(buyFromAddress), 100);
        BuyFromAddress2 = Check.Length(buyFromAddress2, nameof(buyFromAddress2), 50);
        BuyFromCity = Check.Length(buyFromCity, nameof(buyFromCity), 30);
        BuyFromPostCode = CodeTableEntity.NormalizeCode(Check.Length(buyFromPostCode, nameof(buyFromPostCode), 20));
        BuyFromCounty = Check.Length(buyFromCounty, nameof(buyFromCounty), 30);
        BuyFromCountryRegionCode = CodeTableEntity.NormalizeCode(Check.Length(buyFromCountryRegionCode, nameof(buyFromCountryRegionCode), 10));
        BuyFromContact = Check.Length(buyFromContact, nameof(buyFromContact), 100);
        PayToName2 = Check.Length(payToName2, nameof(payToName2), 50);
        PayToAddress2 = Check.Length(payToAddress2, nameof(payToAddress2), 50);
        PayToCounty = Check.Length(payToCounty, nameof(payToCounty), 30);
        ShipToName2 = Check.Length(shipToName2, nameof(shipToName2), 50);
        ShipToAddress2 = Check.Length(shipToAddress2, nameof(shipToAddress2), 50);
        ShipToCounty = Check.Length(shipToCounty, nameof(shipToCounty), 30);
        OrderAddressCode = CodeTableEntity.NormalizeCode(Check.Length(orderAddressCode, nameof(orderAddressCode), 10));
        VatRegistrationNo = Check.Length(vatRegistrationNo, nameof(vatRegistrationNo), 20);
        PaymentDiscountPct = paymentDiscountPct;
        PmtDiscountDate = pmtDiscountDate?.Date;
        VatBaseDiscountPct = vatBaseDiscountPct;
        LanguageCode = CodeTableEntity.NormalizeCode(Check.Length(languageCode, nameof(languageCode), 10));
        QuoteNo = CodeTableEntity.NormalizeCode(Check.Length(quoteNo, nameof(quoteNo), 20));
        ResponsibilityCenter = CodeTableEntity.NormalizeCode(Check.Length(responsibilityCenter, nameof(responsibilityCenter), 10));
        RequestedReceiptDate = requestedReceiptDate?.Date;
        PromisedReceiptDate = promisedReceiptDate?.Date;
        AssignedUserId = CodeTableEntity.NormalizeCode(Check.Length(assignedUserId, nameof(assignedUserId), 50));
        PaymentReference = CodeTableEntity.NormalizeCode(Check.Length(paymentReference, nameof(paymentReference), 50));
        InvoiceReceivedDate = invoiceReceivedDate?.Date;
        CreditorNo = CodeTableEntity.NormalizeCode(Check.Length(creditorNo, nameof(creditorNo), 20));
        ReasonCode = CodeTableEntity.NormalizeCode(Check.Length(reasonCode, nameof(reasonCode), 10));
    }
}
