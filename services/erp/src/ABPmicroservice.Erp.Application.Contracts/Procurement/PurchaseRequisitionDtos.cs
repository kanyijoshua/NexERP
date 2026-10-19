using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ABPmicroservice.Erp.Documents;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Procurement;

public class PurchaseRequisitionDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public RequisitionType RequestType { get; set; }
    public string RequestedBy { get; set; }
    public string DepartmentCode { get; set; }
    public string ProcurementPlanCode { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public RequisitionStatus Status { get; set; }
    public string Description { get; set; }
    public string TenderNo { get; set; }
    public string PurchaseOrderNo { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public List<PurchaseRequisitionLineDto> Lines { get; set; } = new();
}

public class PurchaseRequisitionLineDto : FullAuditedEntityDto<Guid>
{
    public Guid RequisitionId { get; set; }
    public int LineNo { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public string Specifications { get; set; }
    public string ProcurementPlanItemNo { get; set; }
    public decimal Quantity { get; set; }
    public decimal EstimatedUnitCost { get; set; }
    public decimal EstimatedTotalCost { get; set; }
    public string UnitOfMeasure { get; set; }
    public decimal OrderedQuantity { get; set; }
}

public class CreateUpdatePurchaseRequisitionDto
{
    [StringLength(30)]
    public string No { get; set; }

    public RequisitionType RequestType { get; set; }

    [StringLength(50)]
    public string RequestedBy { get; set; }

    [StringLength(30)]
    public string DepartmentCode { get; set; }

    [StringLength(30)]
    public string ProcurementPlanCode { get; set; }

    public DateTime? RequestDate { get; set; }
    public DateTime? RequiredDate { get; set; }

    [StringLength(250)]
    public string Description { get; set; }

    public List<CreateUpdatePurchaseRequisitionLineDto> Lines { get; set; } = new();
}

public class CreateUpdatePurchaseRequisitionLineDto
{
    public Guid? Id { get; set; }
    public DocumentLineType Type { get; set; }

    [Required]
    [StringLength(30)]
    public string No { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; }

    [StringLength(500)]
    public string Specifications { get; set; }

    [StringLength(30)]
    public string ProcurementPlanItemNo { get; set; }

    public decimal Quantity { get; set; }
    public decimal EstimatedUnitCost { get; set; }

    [StringLength(20)]
    public string UnitOfMeasure { get; set; }
}

public class ConvertRequisitionToOrderInput
{
    [Required]
    public Guid VendorId { get; set; }
}

public class ConvertRequisitionToTenderInput
{
    public TenderType TenderType { get; set; } = TenderType.Open;

    [Required]
    [StringLength(250)]
    public string Title { get; set; }

    public DateTime ClosingDate { get; set; }
    public string ClosingTime { get; set; } = "10:00 AM";
    public int ValidityDays { get; set; } = 90;
    public decimal TenderSecurityAmount { get; set; }
}

public class GetPurchaseRequisitionListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public RequisitionType? RequestType { get; set; }
    public RequisitionStatus? Status { get; set; }
    public string DepartmentCode { get; set; }
}
