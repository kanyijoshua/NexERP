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
}

public interface IPurchaseSetupAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/purchase-setup.</summary>
    Task<PurchasesPayablesSetupDto> GetAsync();

    /// <summary>Routed as PUT /api/erp/purchase-setup.</summary>
    Task<PurchasesPayablesSetupDto> UpdateAsync(PurchasesPayablesSetupDto input);
}
