using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Purchasing;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Procurement;

public interface IProcurementPlanAppService
    : ICrudAppService<
        ProcurementPlanDto,
        Guid,
        GetProcurementPlanListInput,
        CreateUpdateProcurementPlanDto,
        CreateUpdateProcurementPlanDto
    >
{
    Task<ProcurementPlanDto> ApproveAsync(Guid id);
    Task<ProcurementPlanDto> ReopenAsync(Guid id);
    Task<ProcurementPlanDto> CloseAsync(Guid id);
}

public interface IPurchaseRequisitionAppService
    : ICrudAppService<
        PurchaseRequisitionDto,
        Guid,
        GetPurchaseRequisitionListInput,
        CreateUpdatePurchaseRequisitionDto,
        CreateUpdatePurchaseRequisitionDto
    >
{
    Task<PurchaseRequisitionDto> ReleaseAsync(Guid id);
    Task<PurchaseRequisitionDto> ReopenAsync(Guid id);
    Task<PurchaseRequisitionDto> RejectAsync(Guid id);
    Task<PurchaseRequisitionDto> CloseAsync(Guid id);

    /// <summary>Convert an approved requisition directly into a Purchase Order (LPO).</summary>
    Task<PurchaseHeaderDto> ConvertToPurchaseOrderAsync(Guid id, ConvertRequisitionToOrderInput input);

    /// <summary>Convert an approved requisition into a Tender / RFQ.</summary>
    Task<TenderHeaderDto> ConvertToTenderAsync(Guid id, ConvertRequisitionToTenderInput input);

    Task<ABPmicroservice.Erp.Workflows.ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id);
    Task CancelApprovalRequestAsync(Guid id);
}

public interface ITenderAppService
    : ICrudAppService<
        TenderHeaderDto,
        Guid,
        GetTenderListInput,
        CreateUpdateTenderDto,
        CreateUpdateTenderDto
    >
{
    Task<TenderHeaderDto> PublishAsync(Guid id);
    Task<TenderHeaderDto> MoveToOpeningAsync(Guid id);
    Task<TenderHeaderDto> MoveToEvaluationAsync(Guid id);
    Task<TenderHeaderDto> CancelTenderAsync(Guid id);

    Task<TenderHeaderDto> SubmitBidAsync(Guid id, SubmitTenderBidInput input);
    Task<TenderHeaderDto> EvaluateBidAsync(Guid id, EvaluateTenderBidInput input);

    /// <summary>Award the tender and optionally auto-generate the Purchase Order (LPO).</summary>
    Task<TenderHeaderDto> AwardAsync(Guid id, AwardTenderInput input);

    Task<ABPmicroservice.Erp.Workflows.ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id);
    Task CancelApprovalRequestAsync(Guid id);
}
