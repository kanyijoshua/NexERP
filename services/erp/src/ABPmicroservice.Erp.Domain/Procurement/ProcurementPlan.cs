using System;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Procurement;

/// <summary>
/// Procurement Plan: defines an annual or periodic corporate procurement plan
/// broken into itemized procurement lines with methods, estimated costs, and funding sources.
/// </summary>
public class ProcurementPlan : CompanyAggregateRoot
{
    public string Code { get; private set; }
    public string Description { get; private set; }
    public string BudgetName { get; private set; }
    public ProcurementPlanStatus Status { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public decimal TotalEstimatedAmount { get; private set; }
    public Collection<ProcurementPlanLine> Lines { get; private set; }

    protected ProcurementPlan()
    {
        Lines = new Collection<ProcurementPlanLine>();
    }

    public ProcurementPlan(
        Guid id,
        string code,
        string description,
        string budgetName = null,
        DateTime? startDate = null,
        DateTime? endDate = null
    )
        : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), 30).Trim().ToUpperInvariant();
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), 200).Trim();
        BudgetName = budgetName?.Trim();
        Status = ProcurementPlanStatus.Open;
        StartDate = startDate?.Date;
        EndDate = endDate?.Date;
        Lines = new Collection<ProcurementPlanLine>();
    }

    public void Update(string description, string budgetName, DateTime? startDate, DateTime? endDate)
    {
        EnsureOpen();
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), 200).Trim();
        BudgetName = budgetName?.Trim();
        StartDate = startDate?.Date;
        EndDate = endDate?.Date;
    }

    public void Approve()
    {
        if (Status == ProcurementPlanStatus.Approved)
        {
            return;
        }
        Status = ProcurementPlanStatus.Approved;
    }

    public void Reopen()
    {
        Status = ProcurementPlanStatus.Open;
    }

    public void Close()
    {
        Status = ProcurementPlanStatus.Closed;
    }

    public ProcurementPlanLine AddLine(
        Guid id,
        string planItemNo,
        string description,
        ProcurementType procurementType,
        ProcurementMethod procurementMethod,
        decimal quantity,
        decimal unitPrice,
        string unitOfMeasure = null,
        string sourceOfFunds = null,
        string departmentCode = null,
        string quarter = "All"
    )
    {
        EnsureOpen();
        var line = new ProcurementPlanLine(
            id,
            Id,
            planItemNo,
            description,
            procurementType,
            procurementMethod,
            quantity,
            unitPrice,
            unitOfMeasure,
            sourceOfFunds,
            departmentCode,
            quarter
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
        TotalEstimatedAmount = Lines.Sum(l => l.EstimatedCost);
    }

    private void EnsureOpen()
    {
        if (Status != ProcurementPlanStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Procurement.ProcurementPlanNotOpen)
                .WithData("code", Code);
        }
    }
}

public class ProcurementPlanLine : CompanyEntity
{
    public Guid PlanId { get; private set; }
    public string PlanItemNo { get; private set; }
    public string Description { get; private set; }
    public ProcurementType ProcurementType { get; private set; }
    public ProcurementMethod ProcurementMethod { get; private set; }
    public string UnitOfMeasure { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal EstimatedCost { get; private set; }
    public string SourceOfFunds { get; private set; }
    public string DepartmentCode { get; private set; }
    public string Quarter { get; private set; }

    protected ProcurementPlanLine() { }

    public ProcurementPlanLine(
        Guid id,
        Guid planId,
        string planItemNo,
        string description,
        ProcurementType procurementType,
        ProcurementMethod procurementMethod,
        decimal quantity,
        decimal unitPrice,
        string unitOfMeasure = null,
        string sourceOfFunds = null,
        string departmentCode = null,
        string quarter = "All"
    )
        : base(id)
    {
        PlanId = planId;
        PlanItemNo = Check.NotNullOrWhiteSpace(planItemNo, nameof(planItemNo), 30).Trim().ToUpperInvariant();
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), 200).Trim();
        ProcurementType = procurementType;
        ProcurementMethod = procurementMethod;
        Quantity = quantity;
        UnitPrice = unitPrice;
        EstimatedCost = Math.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero);
        UnitOfMeasure = unitOfMeasure?.Trim();
        SourceOfFunds = sourceOfFunds?.Trim();
        DepartmentCode = departmentCode?.Trim();
        Quarter = string.IsNullOrWhiteSpace(quarter) ? "All" : quarter.Trim();
    }

    public void Update(
        string description,
        ProcurementType procurementType,
        ProcurementMethod procurementMethod,
        decimal quantity,
        decimal unitPrice,
        string unitOfMeasure = null,
        string sourceOfFunds = null,
        string departmentCode = null,
        string quarter = "All"
    )
    {
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), 200).Trim();
        ProcurementType = procurementType;
        ProcurementMethod = procurementMethod;
        Quantity = quantity;
        UnitPrice = unitPrice;
        EstimatedCost = Math.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero);
        UnitOfMeasure = unitOfMeasure?.Trim();
        SourceOfFunds = sourceOfFunds?.Trim();
        DepartmentCode = departmentCode?.Trim();
        Quarter = string.IsNullOrWhiteSpace(quarter) ? "All" : quarter.Trim();
    }
}
