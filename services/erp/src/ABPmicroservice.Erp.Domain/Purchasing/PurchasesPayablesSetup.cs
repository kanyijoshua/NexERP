using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Purchases &amp; Payables Setup. Mirrors Business Central table 312: one row per company,
/// naming the number series each kind of purchase record draws from. A blank code means
/// numbers are typed by hand.
/// </summary>
public class PurchasesPayablesSetup : CompanyEntity
{
    public string VendorNos { get; private set; }
    public string QuoteNos { get; private set; }
    public string OrderNos { get; private set; }
    public string InvoiceNos { get; private set; }
    public string CreditMemoNos { get; private set; }
    public string PostedInvoiceNos { get; private set; }
    public string PostedCreditMemoNos { get; private set; }
    public string BlanketOrderNos { get; private set; }
    public string PostedReceiptNos { get; private set; }
    public string PostedReturnShptNos { get; private set; }
    public string ReturnOrderNos { get; private set; }

    /// <summary>An invoice cannot be posted without the vendor's invoice number (BC "Ext. Doc. No. Mandatory").</summary>
    public bool ExtDocNoMandatory { get; private set; }

    public PurchasingDiscountPosting DiscountPosting { get; private set; }
    public bool ReceiptOnInvoice { get; private set; }
    public bool InvoiceRounding { get; private set; }
    public bool CalcInvDiscount { get; private set; }
    public bool AllowVATDifference { get; private set; }
    public bool CalcInvDiscPerVATID { get; private set; }
    public bool ExactCostReversingMandatory { get; private set; }

    public bool PostWithJobQueue { get; private set; }
    public string JobQueueCategoryCode { get; private set; }
    public bool NotifyOnSuccess { get; private set; }

    public bool CopyCommentsBlanketToOrder { get; private set; }
    public bool CopyCommentsOrderToInvoice { get; private set; }
    public bool CopyCommentsOrderToReceipt { get; private set; }
    public bool CopyCommentsRetOrdToRetShpt { get; private set; }
    public bool CopyCommentsRetOrdToCrMemo { get; private set; }
    public bool ReturnShipmentOnCreditMemo { get; private set; }
    public bool CopyVendorNameToEntries { get; private set; }
    public bool CopyLineDescrToGLEntry { get; private set; }
    public bool AllowMultiplePostingGroups { get; private set; }

    protected PurchasesPayablesSetup() { }

    public PurchasesPayablesSetup(Guid id)
        : base(id) { }

    public void SetGeneral(
        bool extDocNoMandatory,
        PurchasingDiscountPosting discountPosting = PurchasingDiscountPosting.AllDiscounts,
        bool receiptOnInvoice = false,
        bool invoiceRounding = false,
        bool calcInvDiscount = false,
        bool allowVATDifference = false,
        bool calcInvDiscPerVATID = false,
        bool exactCostReversingMandatory = false,
        bool allowMultiplePostingGroups = false
    )
    {
        ExtDocNoMandatory = extDocNoMandatory;
        DiscountPosting = discountPosting;
        ReceiptOnInvoice = receiptOnInvoice;
        InvoiceRounding = invoiceRounding;
        CalcInvDiscount = calcInvDiscount;
        AllowVATDifference = allowVATDifference;
        CalcInvDiscPerVATID = calcInvDiscPerVATID;
        ExactCostReversingMandatory = exactCostReversingMandatory;
        AllowMultiplePostingGroups = allowMultiplePostingGroups;
    }

    public void SetJobQueuePosting(bool postWithJobQueue, string categoryCode, bool notifyOnSuccess)
    {
        PostWithJobQueue = postWithJobQueue;
        JobQueueCategoryCode = CodeTableEntity.NormalizeCode(Check.Length(categoryCode, nameof(categoryCode), ErpDomainConsts.MaxJobCategoryCodeLength));
        NotifyOnSuccess = notifyOnSuccess;
    }

    public void SetPostingOptions(
        bool copyCommentsBlanketToOrder,
        bool copyCommentsOrderToInvoice,
        bool copyCommentsOrderToReceipt,
        bool copyCommentsRetOrdToRetShpt,
        bool copyCommentsRetOrdToCrMemo,
        bool returnShipmentOnCreditMemo,
        bool copyVendorNameToEntries,
        bool copyLineDescrToGLEntry
    )
    {
        CopyCommentsBlanketToOrder = copyCommentsBlanketToOrder;
        CopyCommentsOrderToInvoice = copyCommentsOrderToInvoice;
        CopyCommentsOrderToReceipt = copyCommentsOrderToReceipt;
        CopyCommentsRetOrdToRetShpt = copyCommentsRetOrdToRetShpt;
        CopyCommentsRetOrdToCrMemo = copyCommentsRetOrdToCrMemo;
        ReturnShipmentOnCreditMemo = returnShipmentOnCreditMemo;
        CopyVendorNameToEntries = copyVendorNameToEntries;
        CopyLineDescrToGLEntry = copyLineDescrToGLEntry;
    }

    public void SetNumberSeries(
        string vendorNos,
        string quoteNos,
        string orderNos,
        string invoiceNos,
        string creditMemoNos,
        string postedInvoiceNos,
        string postedCreditMemoNos,
        string blanketOrderNos = null,
        string postedReceiptNos = null,
        string postedReturnShptNos = null,
        string returnOrderNos = null
    )
    {
        VendorNos = Code(vendorNos, nameof(vendorNos));
        QuoteNos = Code(quoteNos, nameof(quoteNos));
        OrderNos = Code(orderNos, nameof(orderNos));
        InvoiceNos = Code(invoiceNos, nameof(invoiceNos));
        CreditMemoNos = Code(creditMemoNos, nameof(creditMemoNos));
        PostedInvoiceNos = Code(postedInvoiceNos, nameof(postedInvoiceNos));
        PostedCreditMemoNos = Code(postedCreditMemoNos, nameof(postedCreditMemoNos));
        BlanketOrderNos = Code(blanketOrderNos, nameof(blanketOrderNos));
        PostedReceiptNos = Code(postedReceiptNos, nameof(postedReceiptNos));
        PostedReturnShptNos = Code(postedReturnShptNos, nameof(postedReturnShptNos));
        ReturnOrderNos = Code(returnOrderNos, nameof(returnOrderNos));
    }

    /// <summary>The series that numbers a new, unposted document of this type.</summary>
    public string GetDocumentNos(PurchaseDocumentType documentType)
    {
        return documentType switch
        {
            PurchaseDocumentType.Quote => QuoteNos,
            PurchaseDocumentType.Order => OrderNos,
            PurchaseDocumentType.Invoice => InvoiceNos,
            PurchaseDocumentType.CreditMemo => CreditMemoNos,
            PurchaseDocumentType.BlanketOrder => BlanketOrderNos,
            PurchaseDocumentType.ReturnOrder => ReturnOrderNos,
            _ => null,
        };
    }

    /// <summary>The series that numbers the posted document.</summary>
    public string GetPostedDocumentNos(PurchaseDocumentType documentType)
    {
        return documentType == PurchaseDocumentType.CreditMemo ? PostedCreditMemoNos : PostedInvoiceNos;
    }

    private static string Code(string value, string name)
    {
        return value.IsNullOrWhiteSpace() ? null : Check.Length(value.Trim(), name, ErpDomainConsts.MaxNoSeriesCodeLength);
    }
}

public class PurchasesPayablesSetupManager : DomainService
{
    private readonly IRepository<PurchasesPayablesSetup, Guid> _repository;

    public PurchasesPayablesSetupManager(IRepository<PurchasesPayablesSetup, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>The current company's setup, created blank on first use.</summary>
    public async Task<PurchasesPayablesSetup> GetAsync()
    {
        return await _repository.FirstOrDefaultAsync()
            ?? await _repository.InsertAsync(new PurchasesPayablesSetup(GuidGenerator.Create()), autoSave: true);
    }
}
