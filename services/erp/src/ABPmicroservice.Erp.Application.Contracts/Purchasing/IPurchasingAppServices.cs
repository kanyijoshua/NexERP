using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Purchasing;

public interface IVendorAppService
    : ICrudAppService<
        VendorDto,
        Guid,
        GetVendorListInput,
        CreateUpdateVendorDto,
        CreateUpdateVendorDto
    >
{
    Task<VendorDto> GetByNoAsync(string no);

    Task BlockAsync(Guid id);

    Task UnblockAsync(Guid id);
}

public interface IPurchaseDocumentAppService
    : ICrudAppService<
        PurchaseHeaderDto,
        Guid,
        GetPurchaseDocumentListInput,
        CreateUpdatePurchaseHeaderDto,
        CreateUpdatePurchaseHeaderDto
    >
{
    Task<PurchaseHeaderDto> ReleaseAsync(Guid id);

    Task<PurchaseHeaderDto> ReopenAsync(Guid id);

    /// <summary>
    /// Posts the document: creates G/L entries, item ledger entries and
    /// updates the vendor balance. Mirrors BC "Post" codeunit behaviour.
    /// </summary>
    Task<PurchaseHeaderDto> PostAsync(Guid id);
}
