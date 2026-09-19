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
    /// Routed as POST /api/erp/purchase-document/{id}/run-posting.
    /// </summary>
    Task<PurchaseHeaderDto> RunPostingAsync(Guid id);

    /// <summary>
    /// Asks for approval under the workflow in force: the document becomes Pending Approval and is
    /// released by the last approval. Routed as POST .../{id}/send-approval-request.
    /// </summary>
    Task<ABPmicroservice.Erp.Workflows.ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id);

    /// <summary>Withdraws a pending request and reopens the document. Routed as POST .../{id}/cancel-approval-request.</summary>
    Task CancelApprovalRequestAsync(Guid id);
}
