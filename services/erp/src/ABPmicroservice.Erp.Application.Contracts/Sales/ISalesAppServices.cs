using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Sales;

public interface ICustomerAppService
    : ICrudAppService<
        CustomerDto,
        Guid,
        GetCustomerListInput,
        CreateUpdateCustomerDto,
        CreateUpdateCustomerDto
    >
{
    Task<CustomerDto> GetByNoAsync(string no);

    Task BlockAsync(Guid id);

    Task UnblockAsync(Guid id);
}

public interface ISalesDocumentAppService
    : ICrudAppService<
        SalesHeaderDto,
        Guid,
        GetSalesDocumentListInput,
        CreateUpdateSalesHeaderDto,
        CreateUpdateSalesHeaderDto
    >
{
    Task<SalesHeaderDto> ReleaseAsync(Guid id);

    Task<SalesHeaderDto> ReopenAsync(Guid id);

    /// <summary>
    /// Posts the document: creates G/L entries, item ledger entries and
    /// updates the customer balance. Mirrors BC "Post" codeunit behaviour.
    /// Routed as POST /api/erp/sales-document/{id}/run-posting.
    /// </summary>
    Task<SalesHeaderDto> RunPostingAsync(Guid id);
}
