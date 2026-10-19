using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Procurement;

public class ProcurementPlanDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string BudgetName { get; set; }
    public ProcurementPlanStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal TotalEstimatedAmount { get; set; }
    public List<ProcurementPlanLineDto> Lines { get; set; } = new();
}

public class ProcurementPlanLineDto : FullAuditedEntityDto<Guid>
{
    public Guid PlanId { get; set; }
    public string PlanItemNo { get; set; }
    public string Description { get; set; }
    public ProcurementType ProcurementType { get; set; }
    public ProcurementMethod ProcurementMethod { get; set; }
    public string UnitOfMeasure { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal EstimatedCost { get; set; }
    public string SourceOfFunds { get; set; }
    public string DepartmentCode { get; set; }
    public string Quarter { get; set; }
}

public class CreateUpdateProcurementPlanDto
{
    [Required]
    [StringLength(30)]
    public string Code { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; }

    [StringLength(50)]
    public string BudgetName { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<CreateUpdateProcurementPlanLineDto> Lines { get; set; } = new();
}

public class CreateUpdateProcurementPlanLineDto
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(30)]
    public string PlanItemNo { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; }

    public ProcurementType ProcurementType { get; set; }
    public ProcurementMethod ProcurementMethod { get; set; }

    [StringLength(20)]
    public string UnitOfMeasure { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    [StringLength(50)]
    public string SourceOfFunds { get; set; }

    [StringLength(30)]
    public string DepartmentCode { get; set; }

    [StringLength(20)]
    public string Quarter { get; set; }
}

public class GetProcurementPlanListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public ProcurementPlanStatus? Status { get; set; }
}
