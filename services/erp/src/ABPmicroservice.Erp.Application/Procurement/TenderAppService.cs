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

[Authorize(ErpPermissions.Tenders.Default)]
public class TenderAppService
    : ErpCrudAppService<
        TenderHeader,
        TenderHeaderDto,
        Guid,
        GetTenderListInput,
        CreateUpdateTenderDto,
        CreateUpdateTenderDto
    >,
        ITenderAppService
{
    private readonly PurchasesPayablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly ApprovalsManager _approvalsManager;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IRepository<PurchaseHeader, Guid> _purchaseHeaderRepository;

    public TenderAppService(
        IRepository<TenderHeader, Guid> repository,
        PurchasesPayablesSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        ApprovalsManager approvalsManager,
        IRepository<Vendor, Guid> vendorRepository,
        IRepository<PurchaseHeader, Guid> purchaseHeaderRepository
    )
        : base(repository)
    {
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _approvalsManager = approvalsManager;
        _vendorRepository = vendorRepository;
        _purchaseHeaderRepository = purchaseHeaderRepository;

        GetPolicyName = ErpPermissions.Tenders.Default;
        GetListPolicyName = ErpPermissions.Tenders.Default;
        CreatePolicyName = ErpPermissions.Tenders.Create;
        UpdatePolicyName = ErpPermissions.Tenders.Update;
        DeletePolicyName = ErpPermissions.Tenders.Delete;
    }

    public override async Task<TenderHeaderDto> CreateAsync(CreateUpdateTenderDto input)
    {
        await CheckCreatePolicyAsync();

        var setup = await _setupManager.GetAsync();
        var series = input.TenderType == TenderType.RFQ ? setup.RfqNos : setup.TenderNos;
        var no = await _noSeriesManager.ResolveNoAsync(series, input.No, input.IssueDate ?? DateTime.Today);

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Procurement.TenderNoAlreadyExists).WithData("no", no);
        }

        var tender = new TenderHeader(
            GuidGenerator.Create(),
            no,
            input.Title,
            input.TenderType,
            input.IssueDate ?? DateTime.Today,
            input.ClosingDate,
            input.ClosingTime,
            input.RequisitionNo,
            input.ProcurementPlanCode,
            input.ProcurementPlanItemNo,
            input.Category,
            input.ValidityDays,
            input.TenderSecurityAmount
        );

        int lineNo = 10000;
        foreach (var lineInput in input.Lines)
        {
            tender.AddLine(
                lineInput.Id ?? GuidGenerator.Create(),
                lineNo,
                lineInput.Type,
                lineInput.No,
                lineInput.Description,
                lineInput.Quantity,
                lineInput.EstimatedUnitCost,
                lineInput.Specifications,
                lineInput.UnitOfMeasure
            );
            lineNo += 10000;
        }

        foreach (var comm in input.Committees)
        {
            tender.AddCommitteeMember(
                comm.Id ?? GuidGenerator.Create(),
                comm.CommitteeType,
                comm.StaffNo,
                comm.StaffName,
                comm.Role,
                comm.AppointmentDate
            );
        }

        if (input.MandatoryRequirements.Any())
        {
            foreach (var req in input.MandatoryRequirements)
            {
                tender.AddMandatoryRequirement(req.Id ?? GuidGenerator.Create(), req.RequirementDescription, req.IsMandatory);
            }
        }
        else
        {
            // Add the standard tender mandatory requirements
            tender.AddMandatoryRequirement(GuidGenerator.Create(), "Valid Tax Compliance Certificate", isMandatory: true);
            tender.AddMandatoryRequirement(GuidGenerator.Create(), "Certificate of Incorporation / Registration", isMandatory: true);
            tender.AddMandatoryRequirement(GuidGenerator.Create(), "Valid Business Permit", isMandatory: true);
            tender.AddMandatoryRequirement(GuidGenerator.Create(), "PIN Certificate", isMandatory: true);
        }

        await Repository.InsertAsync(tender, autoSave: true);
        return await MapToGetOutputDtoAsync(tender);
    }

    public override async Task<TenderHeaderDto> UpdateAsync(Guid id, CreateUpdateTenderDto input)
    {
        await CheckUpdatePolicyAsync();

        var tender = await Repository.GetAsync(id, includeDetails: true);
        tender.Update(
            input.Title,
            input.TenderType,
            input.ClosingDate,
            input.ClosingTime,
            input.Category,
            input.ValidityDays,
            input.TenderSecurityAmount
        );

        // Update lines
        var existingLineIds = tender.Lines.Select(l => l.Id).ToList();
        var incomingLineIds = input.Lines.Where(l => l.Id.HasValue).Select(l => l.Id.Value).ToHashSet();

        foreach (var existingId in existingLineIds)
        {
            if (!incomingLineIds.Contains(existingId))
            {
                tender.RemoveLine(existingId);
            }
        }

        int nextLineNo = tender.Lines.Any() ? tender.Lines.Max(l => l.LineNo) + 10000 : 10000;
        foreach (var lineInput in input.Lines.Where(l => !l.Id.HasValue || !tender.Lines.Any(x => x.Id == l.Id.Value)))
        {
            tender.AddLine(
                GuidGenerator.Create(),
                nextLineNo,
                lineInput.Type,
                lineInput.No,
                lineInput.Description,
                lineInput.Quantity,
                lineInput.EstimatedUnitCost,
                lineInput.Specifications,
                lineInput.UnitOfMeasure
            );
            nextLineNo += 10000;
        }

        await Repository.UpdateAsync(tender, autoSave: true);
        return await MapToGetOutputDtoAsync(tender);
    }

    [Authorize(ErpPermissions.Tenders.Release)]
    public async Task<TenderHeaderDto> PublishAsync(Guid id)
    {
        var tender = await Repository.GetAsync(id, includeDetails: true);
        await _approvalsManager.EnsureCanReleaseAsync(ApprovalDocumentKind.Tender, tender);
        tender.Release();
        await Repository.UpdateAsync(tender, autoSave: true);
        return await MapToGetOutputDtoAsync(tender);
    }

    [Authorize(ErpPermissions.Tenders.Update)]
    public async Task<TenderHeaderDto> MoveToOpeningAsync(Guid id)
    {
        var tender = await Repository.GetAsync(id, includeDetails: true);
        tender.MoveToOpening();
        await Repository.UpdateAsync(tender, autoSave: true);
        return await MapToGetOutputDtoAsync(tender);
    }

    [Authorize(ErpPermissions.Tenders.Evaluate)]
    public async Task<TenderHeaderDto> MoveToEvaluationAsync(Guid id)
    {
        var tender = await Repository.GetAsync(id, includeDetails: true);
        tender.MoveToEvaluation();
        await Repository.UpdateAsync(tender, autoSave: true);
        return await MapToGetOutputDtoAsync(tender);
    }

    [Authorize(ErpPermissions.Tenders.Delete)]
    public async Task<TenderHeaderDto> CancelTenderAsync(Guid id)
    {
        var tender = await Repository.GetAsync(id, includeDetails: true);
        tender.Cancel();
        await Repository.UpdateAsync(tender, autoSave: true);
        return await MapToGetOutputDtoAsync(tender);
    }

    [Authorize(ErpPermissions.Tenders.Update)]
    public async Task<TenderHeaderDto> SubmitBidAsync(Guid id, SubmitTenderBidInput input)
    {
        var tender = await Repository.GetAsync(id, includeDetails: true);
        tender.AddBid(
            GuidGenerator.Create(),
            input.BidderNo,
            input.BidderName,
            input.BidAmount,
            input.Email,
            input.DeliveryPeriodDays,
            DateTime.Today,
            input.PassedMandatory,
            0m,
            0m,
            input.Remarks
        );

        await Repository.UpdateAsync(tender, autoSave: true);
        return await MapToGetOutputDtoAsync(tender);
    }

    [Authorize(ErpPermissions.Tenders.Evaluate)]
    public async Task<TenderHeaderDto> EvaluateBidAsync(Guid id, EvaluateTenderBidInput input)
    {
        var tender = await Repository.GetAsync(id, includeDetails: true);
        var bid = tender.Bids.FirstOrDefault(b => b.Id == input.BidId);
        if (bid == null)
        {
            throw new BusinessException(ErpErrorCodes.Procurement.BidNotFound);
        }

        bid.UpdateScores(input.PassedMandatory, input.TechnicalScore, input.FinancialScore, input.Rank, input.Remarks);
        await Repository.UpdateAsync(tender, autoSave: true);
        return await MapToGetOutputDtoAsync(tender);
    }

    [Authorize(ErpPermissions.Tenders.Award)]
    public async Task<TenderHeaderDto> AwardAsync(Guid id, AwardTenderInput input)
    {
        var tender = await Repository.GetAsync(id, includeDetails: true);

        string poNo = null;
        if (input.GeneratePurchaseOrder)
        {
            var vendor = await _vendorRepository.FirstOrDefaultAsync(v => v.No == input.VendorNo);
            if (vendor != null)
            {
                var setup = await _setupManager.GetAsync();
                poNo = await _noSeriesManager.ResolveNoAsync(setup.GetDocumentNos(PurchaseDocumentType.Order), null, DateTime.Today);

                var order = new PurchaseHeader(
                    GuidGenerator.Create(),
                    PurchaseDocumentType.Order,
                    poNo,
                    vendor.Id,
                    vendor.No,
                    vendor.Name,
                    DateTime.Today
                );

                order.SetProcurementDetails(
                    requisitionNo: tender.RequisitionNo,
                    tenderNo: tender.No,
                    commencementDate: DateTime.Today,
                    completionDate: DateTime.Today.AddDays(tender.ValidityDays)
                );
                order.SetOrderStage(PurchaseOrderStage.Released);

                foreach (var tenderLine in tender.Lines)
                {
                    var unitCost = tender.Lines.Count > 0 ? (input.AwardAmount / tender.Lines.Sum(l => l.Quantity > 0 ? l.Quantity : 1)) : input.AwardAmount;
                    var line = order.AddLine(
                        GuidGenerator.Create(),
                        tenderLine.Type,
                        tenderLine.No,
                        tenderLine.Description,
                        tenderLine.Quantity,
                        tenderLine.EstimatedUnitCost > 0 ? tenderLine.EstimatedUnitCost : unitCost,
                        0m,
                        tenderLine.UnitOfMeasure
                    );
                }

                await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(order, vendor);
                await _purchaseHeaderRepository.InsertAsync(order, autoSave: true);
            }
        }

        tender.Award(input.VendorNo, input.VendorName, input.AwardAmount, input.AwardDate ?? DateTime.Today, poNo);
        await Repository.UpdateAsync(tender, autoSave: true);
        return await MapToGetOutputDtoAsync(tender);
    }

    [Authorize(ErpPermissions.Tenders.Update)]
    public async Task<ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id)
    {
        var result = await _approvalsManager.SendApprovalRequestAsync(ApprovalDocumentKind.Tender, id, GetUserId());
        return new ApprovalRequestResultDto
        {
            AutoApproved = result.AutoApproved,
            ApproverCount = result.ApproverCount,
            FirstApproverUserName = result.FirstApproverUserName,
        };
    }

    [Authorize(ErpPermissions.Tenders.Update)]
    public async Task CancelApprovalRequestAsync(Guid id)
    {
        await _approvalsManager.CancelApprovalRequestAsync(ApprovalDocumentKind.Tender, id, GetUserId());
    }

    private Guid GetUserId()
    {
        return CurrentUser.Id ?? throw new AbpAuthorizationException();
    }

    protected override async Task<TenderHeader> GetEntityByIdAsync(Guid id)
    {
        return await Repository.GetAsync(id, includeDetails: true);
    }

    protected override async Task<IQueryable<TenderHeader>> CreateFilteredQueryAsync(GetTenderListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.No.Contains(input.Filter) || x.Title.Contains(input.Filter))
            .WhereIf(input.TenderType.HasValue, x => x.TenderType == input.TenderType.Value)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value);
    }
}
