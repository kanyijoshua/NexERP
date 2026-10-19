using System;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using Volo.Abp;

namespace ABPmicroservice.Erp.Procurement;

/// <summary>
/// Purchase Requisition / Internal Request: allows staff to request goods, services, or capex.
/// Sourced from procurement plan items and can be converted to Tenders or Purchase Orders.
/// </summary>
public class PurchaseRequisition : CompanyAggregateRoot, IApprovalDocument
{
    public string No { get; private set; }
    public RequisitionType RequestType { get; private set; }
    public string RequestedBy { get; private set; }
    public string DepartmentCode { get; private set; }
    public string ProcurementPlanCode { get; private set; }
    public DateTime RequestDate { get; private set; }
    public DateTime? RequiredDate { get; private set; }
    public RequisitionStatus Status { get; private set; }
    public string Description { get; private set; }
    public string TenderNo { get; private set; }
    public string PurchaseOrderNo { get; private set; }
    public decimal TotalEstimatedCost { get; private set; }
    public Collection<PurchaseRequisitionLine> Lines { get; private set; }

    // IApprovalDocument implementation
    DocumentStatus IApprovalDocument.Status => Status switch
    {
        RequisitionStatus.Open => DocumentStatus.Open,
        RequisitionStatus.PendingApproval => DocumentStatus.PendingApproval,
        RequisitionStatus.Released => DocumentStatus.Released,
        _ => DocumentStatus.Open,
    };
    bool IApprovalDocument.HasLines => Lines.Any();
    decimal IApprovalDocument.ApprovalAmount => TotalEstimatedCost;
    void IApprovalDocument.SendForApproval() => Status = RequisitionStatus.PendingApproval;
    void IApprovalDocument.Release() => Release();
    void IApprovalDocument.Reopen() => Reopen();

    protected PurchaseRequisition()
    {
        Lines = new Collection<PurchaseRequisitionLine>();
    }

    public PurchaseRequisition(
        Guid id,
        string no,
        RequisitionType requestType,
        string requestedBy,
        string departmentCode = null,
        string procurementPlanCode = null,
        DateTime? requestDate = null,
        DateTime? requiredDate = null,
        string description = null
    )
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), 30).Trim().ToUpperInvariant();
        RequestType = requestType;
        RequestedBy = Check.NotNullOrWhiteSpace(requestedBy, nameof(requestedBy), 50).Trim();
        DepartmentCode = departmentCode?.Trim();
        ProcurementPlanCode = procurementPlanCode?.Trim();
        RequestDate = (requestDate ?? DateTime.Today).Date;
        RequiredDate = requiredDate?.Date;
        Description = description?.Trim();
        Status = RequisitionStatus.Open;
        Lines = new Collection<PurchaseRequisitionLine>();
    }

    public void Update(
        RequisitionType requestType,
        string departmentCode,
        string procurementPlanCode,
        DateTime? requiredDate,
        string description
    )
    {
        EnsureOpen();
        RequestType = requestType;
        DepartmentCode = departmentCode?.Trim();
        ProcurementPlanCode = procurementPlanCode?.Trim();
        RequiredDate = requiredDate?.Date;
        Description = description?.Trim();
    }

    public void Release()
    {
        if (!Lines.Any())
        {
            throw new BusinessException(ErpErrorCodes.Procurement.RequisitionHasNoLines)
                .WithData("no", No);
        }
        Status = RequisitionStatus.Released;
    }

    public void Reopen()
    {
        if (Status == RequisitionStatus.Closed)
        {
            throw new BusinessException(ErpErrorCodes.Procurement.RequisitionAlreadyClosed)
                .WithData("no", No);
        }
        Status = RequisitionStatus.Open;
    }

    public void Reject()
    {
        Status = RequisitionStatus.Rejected;
    }

    public void Close()
    {
        Status = RequisitionStatus.Closed;
    }

    public void MarkSourcedToTender(string tenderNo)
    {
        TenderNo = Check.NotNullOrWhiteSpace(tenderNo, nameof(tenderNo), 30).Trim().ToUpperInvariant();
    }

    public void MarkConvertedToPurchaseOrder(string poNo)
    {
        PurchaseOrderNo = Check.NotNullOrWhiteSpace(poNo, nameof(poNo), 30).Trim().ToUpperInvariant();
        Status = RequisitionStatus.Closed;
    }

    public PurchaseRequisitionLine AddLine(
        Guid id,
        int lineNo,
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal estimatedUnitCost,
        string specifications = null,
        string procurementPlanItemNo = null,
        string unitOfMeasure = null
    )
    {
        EnsureOpen();
        var line = new PurchaseRequisitionLine(
            id,
            Id,
            lineNo,
            type,
            no,
            description,
            quantity,
            estimatedUnitCost,
            specifications,
            procurementPlanItemNo,
            unitOfMeasure
        );
        Lines.Add(line);
        RecalculateTotal();
        return line;
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureOpen();
        var line = Lines.FirstOrDefault(l => l.Id == lineId);
        if (line != null)
        {
            Lines.Remove(line);
            RecalculateTotal();
        }
    }

    public void RecalculateTotal()
    {
        TotalEstimatedCost = Lines.Sum(l => l.EstimatedTotalCost);
    }

    private void EnsureOpen()
    {
        if (Status != RequisitionStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Procurement.RequisitionNotOpen)
                .WithData("no", No);
        }
    }
}

public class PurchaseRequisitionLine : CompanyEntity
{
    public Guid RequisitionId { get; private set; }
    public int LineNo { get; private set; }
    public DocumentLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public string Specifications { get; private set; }
    public string ProcurementPlanItemNo { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal EstimatedUnitCost { get; private set; }
    public decimal EstimatedTotalCost { get; private set; }
    public string UnitOfMeasure { get; private set; }
    public decimal OrderedQuantity { get; private set; }

    protected PurchaseRequisitionLine() { }

    public PurchaseRequisitionLine(
        Guid id,
        Guid requisitionId,
        int lineNo,
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal estimatedUnitCost,
        string specifications = null,
        string procurementPlanItemNo = null,
        string unitOfMeasure = null
    )
        : base(id)
    {
        RequisitionId = requisitionId;
        LineNo = lineNo;
        Type = type;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), 30).Trim().ToUpperInvariant();
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), 200).Trim();
        Quantity = quantity;
        EstimatedUnitCost = estimatedUnitCost;
        EstimatedTotalCost = Math.Round(quantity * estimatedUnitCost, 2, MidpointRounding.AwayFromZero);
        Specifications = specifications?.Trim();
        ProcurementPlanItemNo = procurementPlanItemNo?.Trim();
        UnitOfMeasure = unitOfMeasure?.Trim();
    }

    public void Update(
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal estimatedUnitCost,
        string specifications = null,
        string procurementPlanItemNo = null,
        string unitOfMeasure = null
    )
    {
        Type = type;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), 30).Trim().ToUpperInvariant();
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), 200).Trim();
        Quantity = quantity;
        EstimatedUnitCost = estimatedUnitCost;
        EstimatedTotalCost = Math.Round(quantity * estimatedUnitCost, 2, MidpointRounding.AwayFromZero);
        Specifications = specifications?.Trim();
        ProcurementPlanItemNo = procurementPlanItemNo?.Trim();
        UnitOfMeasure = unitOfMeasure?.Trim();
    }

    public void SetOrderedQuantity(decimal orderedQuantity)
    {
        OrderedQuantity = orderedQuantity;
    }
}
