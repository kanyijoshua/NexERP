using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Workflows;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Procurement;

[Authorize(ErpPermissions.PurchaseRequisitions.Default)]
public class PurchaseRequisitionAppService
    : ErpCrudAppService<
        PurchaseRequisition,
        PurchaseRequisitionDto,
        Guid,
        GetPurchaseRequisitionListInput,
        CreateUpdatePurchaseRequisitionDto,
        CreateUpdatePurchaseRequisitionDto
    >,
        IPurchaseRequisitionAppService
{
    private readonly PurchasesPayablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly ApprovalsManager _approvalsManager;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IRepository<PurchaseHeader, Guid> _purchaseHeaderRepository;
    private readonly IRepository<TenderHeader, Guid> _tenderHeaderRepository;

    public PurchaseRequisitionAppService(
        IRepository<PurchaseRequisition, Guid> repository,
        PurchasesPayablesSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        ApprovalsManager approvalsManager,
        IRepository<Vendor, Guid> vendorRepository,
        IRepository<PurchaseHeader, Guid> purchaseHeaderRepository,
        IRepository<TenderHeader, Guid> tenderHeaderRepository
    )
        : base(repository)
    {
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _approvalsManager = approvalsManager;
        _vendorRepository = vendorRepository;
        _purchaseHeaderRepository = purchaseHeaderRepository;
        _tenderHeaderRepository = tenderHeaderRepository;

        GetPolicyName = ErpPermissions.PurchaseRequisitions.Default;
        GetListPolicyName = ErpPermissions.PurchaseRequisitions.Default;
        CreatePolicyName = ErpPermissions.PurchaseRequisitions.Create;
        UpdatePolicyName = ErpPermissions.PurchaseRequisitions.Update;
        DeletePolicyName = ErpPermissions.PurchaseRequisitions.Delete;
    }

    public override async Task<PurchaseRequisitionDto> CreateAsync(CreateUpdatePurchaseRequisitionDto input)
    {
        await CheckCreatePolicyAsync();

        var setup = await _setupManager.GetAsync();
        var no = await _noSeriesManager.ResolveNoAsync(setup.RequisitionNos, input.No, input.RequestDate ?? DateTime.Today);

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Procurement.RequisitionNoAlreadyExists).WithData("no", no);
        }

        var req = new PurchaseRequisition(
            GuidGenerator.Create(),
            no,
            input.RequestType,
            input.RequestedBy ?? CurrentUser.UserName ?? "User",
            input.DepartmentCode,
            input.ProcurementPlanCode,
            input.RequestDate,
            input.RequiredDate,
            input.Description
        );

        int lineNo = 10000;
        foreach (var lineInput in input.Lines)
        {
            req.AddLine(
                lineInput.Id ?? GuidGenerator.Create(),
                lineNo,
                lineInput.Type,
                lineInput.No,
                lineInput.Description,
                lineInput.Quantity,
                lineInput.EstimatedUnitCost,
                lineInput.Specifications,
                lineInput.ProcurementPlanItemNo,
                lineInput.UnitOfMeasure
            );
            lineNo += 10000;
        }

        await Repository.InsertAsync(req, autoSave: true);
        return await MapToGetOutputDtoAsync(req);
    }

    public override async Task<PurchaseRequisitionDto> UpdateAsync(Guid id, CreateUpdatePurchaseRequisitionDto input)
    {
        await CheckUpdatePolicyAsync();

        var req = await Repository.GetAsync(id, includeDetails: true);
        req.Update(input.RequestType, input.DepartmentCode, input.ProcurementPlanCode, input.RequiredDate, input.Description);

        // Replace lines
        var existingLineIds = req.Lines.Select(l => l.Id).ToList();
        var incomingLineIds = input.Lines.Where(l => l.Id.HasValue).Select(l => l.Id.Value).ToHashSet();

        foreach (var existingId in existingLineIds)
        {
            if (!incomingLineIds.Contains(existingId))
            {
                req.RemoveLine(existingId);
            }
        }

        int nextLineNo = req.Lines.Any() ? req.Lines.Max(l => l.LineNo) + 10000 : 10000;
        foreach (var lineInput in input.Lines)
        {
            if (lineInput.Id.HasValue && req.Lines.Any(l => l.Id == lineInput.Id.Value))
            {
                var line = req.Lines.First(l => l.Id == lineInput.Id.Value);
                line.Update(
                    lineInput.Type,
                    lineInput.No,
                    lineInput.Description,
                    lineInput.Quantity,
                    lineInput.EstimatedUnitCost,
                    lineInput.Specifications,
                    lineInput.ProcurementPlanItemNo,
                    lineInput.UnitOfMeasure
                );
            }
            else
            {
                req.AddLine(
                    GuidGenerator.Create(),
                    nextLineNo,
                    lineInput.Type,
                    lineInput.No,
                    lineInput.Description,
                    lineInput.Quantity,
                    lineInput.EstimatedUnitCost,
                    lineInput.Specifications,
                    lineInput.ProcurementPlanItemNo,
                    lineInput.UnitOfMeasure
                );
                nextLineNo += 10000;
            }
        }
        req.RecalculateTotal();

        await Repository.UpdateAsync(req, autoSave: true);
        return await MapToGetOutputDtoAsync(req);
    }

    [Authorize(ErpPermissions.PurchaseRequisitions.Release)]
    public async Task<PurchaseRequisitionDto> ReleaseAsync(Guid id)
    {
        var req = await Repository.GetAsync(id, includeDetails: true);
        await _approvalsManager.EnsureCanReleaseAsync(ApprovalDocumentKind.PurchaseRequisition, req);
        req.Release();
        await Repository.UpdateAsync(req, autoSave: true);
        return await MapToGetOutputDtoAsync(req);
    }

    [Authorize(ErpPermissions.PurchaseRequisitions.Update)]
    public async Task<PurchaseRequisitionDto> ReopenAsync(Guid id)
    {
        var req = await Repository.GetAsync(id, includeDetails: true);
        req.Reopen();
        await Repository.UpdateAsync(req, autoSave: true);
        return await MapToGetOutputDtoAsync(req);
    }

    [Authorize(ErpPermissions.PurchaseRequisitions.Update)]
    public async Task<PurchaseRequisitionDto> RejectAsync(Guid id)
    {
        var req = await Repository.GetAsync(id, includeDetails: true);
        req.Reject();
        await Repository.UpdateAsync(req, autoSave: true);
        return await MapToGetOutputDtoAsync(req);
    }

    [Authorize(ErpPermissions.PurchaseRequisitions.Update)]
    public async Task<PurchaseRequisitionDto> CloseAsync(Guid id)
    {
        var req = await Repository.GetAsync(id, includeDetails: true);
        req.Close();
        await Repository.UpdateAsync(req, autoSave: true);
        return await MapToGetOutputDtoAsync(req);
    }

    [Authorize(ErpPermissions.PurchaseRequisitions.Convert)]
    public async Task<PurchaseHeaderDto> ConvertToPurchaseOrderAsync(Guid id, ConvertRequisitionToOrderInput input)
    {
        var req = await Repository.GetAsync(id, includeDetails: true);
        if (req.Status != RequisitionStatus.Released)
        {
            throw new BusinessException(ErpErrorCodes.Procurement.RequisitionMustBeApprovedToConvert).WithData("no", req.No);
        }

        var vendor = await _vendorRepository.GetAsync(input.VendorId);
        var setup = await _setupManager.GetAsync();
        var orderNo = await _noSeriesManager.ResolveNoAsync(setup.GetDocumentNos(PurchaseDocumentType.Order), null, DateTime.Today);

        var order = new PurchaseHeader(
            GuidGenerator.Create(),
            PurchaseDocumentType.Order,
            orderNo,
            vendor.Id,
            vendor.No,
            vendor.Name,
            DateTime.Today
        );

        order.SetProcurementDetails(
            requisitionNo: req.No,
            tenderNo: null,
            commencementDate: DateTime.Today,
            completionDate: req.RequiredDate
        );
        order.SetOrderStage(PurchaseOrderStage.Released);

        foreach (var reqLine in req.Lines)
        {
            var line = order.AddLine(
                GuidGenerator.Create(),
                reqLine.Type,
                reqLine.No,
                reqLine.Description,
                reqLine.Quantity,
                reqLine.EstimatedUnitCost,
                0m,
                reqLine.UnitOfMeasure
            );
            reqLine.SetOrderedQuantity(reqLine.Quantity);
        }

        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(order, vendor);
        await _purchaseHeaderRepository.InsertAsync(order, autoSave: true);

        req.MarkConvertedToPurchaseOrder(order.No);
        await Repository.UpdateAsync(req, autoSave: true);

        return ObjectMapper.Map<PurchaseHeader, PurchaseHeaderDto>(order);
    }

    [Authorize(ErpPermissions.PurchaseRequisitions.Convert)]
    public async Task<TenderHeaderDto> ConvertToTenderAsync(Guid id, ConvertRequisitionToTenderInput input)
    {
        var req = await Repository.GetAsync(id, includeDetails: true);
        if (req.Status != RequisitionStatus.Released)
        {
            throw new BusinessException(ErpErrorCodes.Procurement.RequisitionMustBeApprovedToConvert).WithData("no", req.No);
        }

        var setup = await _setupManager.GetAsync();
        var tenderSeries = input.TenderType == TenderType.RFQ ? setup.RfqNos : setup.TenderNos;
        var tenderNo = await _noSeriesManager.ResolveNoAsync(tenderSeries, null, DateTime.Today);

        var tender = new TenderHeader(
            GuidGenerator.Create(),
            tenderNo,
            input.Title,
            input.TenderType,
            DateTime.Today,
            input.ClosingDate,
            input.ClosingTime,
            requisitionNo: req.No,
            procurementPlanCode: req.ProcurementPlanCode,
            validityDays: input.ValidityDays,
            tenderSecurityAmount: input.TenderSecurityAmount
        );

        int lineNo = 10000;
        foreach (var reqLine in req.Lines)
        {
            tender.AddLine(
                GuidGenerator.Create(),
                lineNo,
                reqLine.Type,
                reqLine.No,
                reqLine.Description,
                reqLine.Quantity,
                reqLine.EstimatedUnitCost,
                reqLine.Specifications,
                reqLine.UnitOfMeasure
            );
            lineNo += 10000;
        }

        // Add standard mandatory requirements
        tender.AddMandatoryRequirement(GuidGenerator.Create(), "Valid Tax Compliance Certificate", isMandatory: true);
        tender.AddMandatoryRequirement(GuidGenerator.Create(), "Certificate of Incorporation / Registration", isMandatory: true);
        tender.AddMandatoryRequirement(GuidGenerator.Create(), "Valid Business Permit", isMandatory: true);
        tender.AddMandatoryRequirement(GuidGenerator.Create(), "KRA PIN Certificate", isMandatory: true);

        await _tenderHeaderRepository.InsertAsync(tender, autoSave: true);

        req.MarkSourcedToTender(tender.No);
        await Repository.UpdateAsync(req, autoSave: true);

        return ObjectMapper.Map<TenderHeader, TenderHeaderDto>(tender);
    }

    [Authorize(ErpPermissions.PurchaseRequisitions.Update)]
    public async Task<ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id)
    {
        var result = await _approvalsManager.SendApprovalRequestAsync(ApprovalDocumentKind.PurchaseRequisition, id, GetUserId());
        return new ApprovalRequestResultDto
        {
            AutoApproved = result.AutoApproved,
            ApproverCount = result.ApproverCount,
            FirstApproverUserName = result.FirstApproverUserName,
        };
    }

    [Authorize(ErpPermissions.PurchaseRequisitions.Update)]
    public async Task CancelApprovalRequestAsync(Guid id)
    {
        await _approvalsManager.CancelApprovalRequestAsync(ApprovalDocumentKind.PurchaseRequisition, id, GetUserId());
    }

    private Guid GetUserId()
    {
        return CurrentUser.Id ?? throw new AbpAuthorizationException();
    }

    protected override async Task<PurchaseRequisition> GetEntityByIdAsync(Guid id)
    {
        return await Repository.GetAsync(id, includeDetails: true);
    }

    protected override async Task<IQueryable<PurchaseRequisition>> CreateFilteredQueryAsync(GetPurchaseRequisitionListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.No.Contains(input.Filter) || x.Description.Contains(input.Filter) || x.RequestedBy.Contains(input.Filter))
            .WhereIf(input.RequestType.HasValue, x => x.RequestType == input.RequestType.Value)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(!input.DepartmentCode.IsNullOrWhiteSpace(), x => x.DepartmentCode == input.DepartmentCode);
    }
}
