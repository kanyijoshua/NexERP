using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>Number series codes per kind of purchase record. Blank means numbers are typed by hand.</summary>
public class PurchasesPayablesSetupDto
{
    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string VendorNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string QuoteNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string OrderNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string InvoiceNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string CreditMemoNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string PostedInvoiceNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string PostedCreditMemoNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string BlanketOrderNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string PostedReceiptNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string PostedReturnShptNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ReturnOrderNos { get; set; }

    public PurchasingDiscountPosting DiscountPosting { get; set; }

    public bool ReceiptOnInvoice { get; set; }

    public bool InvoiceRounding { get; set; }

    public bool ExtDocNoMandatory { get; set; }

    public bool CalcInvDiscount { get; set; }

    public bool AllowVATDifference { get; set; }

    public bool CalcInvDiscPerVATID { get; set; }

    public bool ExactCostReversingMandatory { get; set; }

    public bool PostWithJobQueue { get; set; }

    [StringLength(ErpDomainConsts.MaxJobCategoryCodeLength)]
    public string JobQueueCategoryCode { get; set; }

    public bool NotifyOnSuccess { get; set; }

    public bool CopyCommentsBlanketToOrder { get; set; }

    public bool CopyCommentsOrderToInvoice { get; set; }

    public bool CopyCommentsOrderToReceipt { get; set; }

    public bool CopyCommentsRetOrderToCrMemo { get; set; }

    public bool CopyCommentsRetOrderToRetShpt { get; set; }

    public bool ReturnShipmentOnCreditMemo { get; set; }

    public bool CopyVendorNameToEntries { get; set; }

    public bool CopyLineDescrToGLEntry { get; set; }

    public bool AllowMultiplePostingGroups { get; set; }
}

public interface IPurchaseSetupAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/purchase-setup.</summary>
    Task<PurchasesPayablesSetupDto> GetAsync();

    /// <summary>Routed as PUT /api/erp/purchase-setup.</summary>
    Task<PurchasesPayablesSetupDto> UpdateAsync(PurchasesPayablesSetupDto input);
}
