using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Sales;

/// <summary>Number series codes per kind of sales record. Blank means numbers are typed by hand.</summary>
public class SalesReceivablesSetupDto
{
    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string CustomerNos { get; set; }

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

public interface ISalesSetupAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/sales-setup.</summary>
    Task<SalesReceivablesSetupDto> GetAsync();

    /// <summary>Routed as PUT /api/erp/sales-setup.</summary>
    Task<SalesReceivablesSetupDto> UpdateAsync(SalesReceivablesSetupDto input);
}
