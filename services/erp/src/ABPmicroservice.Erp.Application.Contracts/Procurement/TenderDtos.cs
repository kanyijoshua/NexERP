using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ABPmicroservice.Erp.Documents;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Procurement;

public class TenderHeaderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Title { get; set; }
    public TenderType TenderType { get; set; }
    public string RequisitionNo { get; set; }
    public string ProcurementPlanCode { get; set; }
    public string ProcurementPlanItemNo { get; set; }
    public string Category { get; set; }
    public TenderStatus Status { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ClosingDate { get; set; }
    public string ClosingTime { get; set; }
    public int ValidityDays { get; set; }
    public decimal TenderSecurityAmount { get; set; }
    public string AwardedVendorNo { get; set; }
    public string AwardedVendorName { get; set; }
    public decimal AwardedAmount { get; set; }
    public DateTime? AwardDate { get; set; }
    public string PurchaseOrderNo { get; set; }
    public decimal TotalEstimatedAmount { get; set; }

    public List<TenderLineDto> Lines { get; set; } = new();
    public List<TenderCommitteeDto> Committees { get; set; } = new();
    public List<TenderMandatoryRequirementDto> MandatoryRequirements { get; set; } = new();
    public List<TenderBidDto> Bids { get; set; } = new();
}

public class TenderLineDto : FullAuditedEntityDto<Guid>
{
    public Guid TenderId { get; set; }
    public int LineNo { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public string Specifications { get; set; }
    public decimal Quantity { get; set; }
    public decimal EstimatedUnitCost { get; set; }
    public decimal EstimatedCost { get; set; }
    public string UnitOfMeasure { get; set; }
}

public class TenderCommitteeDto : FullAuditedEntityDto<Guid>
{
    public Guid TenderId { get; set; }
    public TenderCommitteeType CommitteeType { get; set; }
    public string StaffNo { get; set; }
    public string StaffName { get; set; }
    public TenderCommitteeRole Role { get; set; }
    public DateTime AppointmentDate { get; set; }
}

public class TenderMandatoryRequirementDto : FullAuditedEntityDto<Guid>
{
    public Guid TenderId { get; set; }
    public string RequirementDescription { get; set; }
    public bool IsMandatory { get; set; }
}

public class TenderBidDto : FullAuditedEntityDto<Guid>
{
    public Guid TenderId { get; set; }
    public string BidderNo { get; set; }
    public string BidderName { get; set; }
    public string Email { get; set; }
    public decimal BidAmount { get; set; }
    public int DeliveryPeriodDays { get; set; }
    public DateTime SubmissionDate { get; set; }
    public bool PassedMandatory { get; set; }
    public decimal TechnicalScore { get; set; }
    public decimal FinancialScore { get; set; }
    public decimal TotalScore { get; set; }
    public int Rank { get; set; }
    public bool Awarded { get; set; }
    public string Remarks { get; set; }
}

public class CreateUpdateTenderDto
{
    [StringLength(30)]
    public string No { get; set; }

    [Required]
    [StringLength(250)]
    public string Title { get; set; }

    public TenderType TenderType { get; set; }

    [StringLength(30)]
    public string RequisitionNo { get; set; }

    [StringLength(30)]
    public string ProcurementPlanCode { get; set; }

    [StringLength(30)]
    public string ProcurementPlanItemNo { get; set; }

    [StringLength(50)]
    public string Category { get; set; }

    public DateTime? IssueDate { get; set; }
    public DateTime ClosingDate { get; set; }

    [StringLength(20)]
    public string ClosingTime { get; set; } = "10:00 AM";

    public int ValidityDays { get; set; } = 90;
    public decimal TenderSecurityAmount { get; set; }

    public List<CreateUpdateTenderLineDto> Lines { get; set; } = new();
    public List<CreateUpdateTenderCommitteeDto> Committees { get; set; } = new();
    public List<CreateUpdateTenderMandatoryRequirementDto> MandatoryRequirements { get; set; } = new();
}

public class CreateUpdateTenderLineDto
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

    public decimal Quantity { get; set; }
    public decimal EstimatedUnitCost { get; set; }

    [StringLength(20)]
    public string UnitOfMeasure { get; set; }
}

public class CreateUpdateTenderCommitteeDto
{
    public Guid? Id { get; set; }
    public TenderCommitteeType CommitteeType { get; set; }

    [Required]
    [StringLength(30)]
    public string StaffNo { get; set; }

    [Required]
    [StringLength(100)]
    public string StaffName { get; set; }

    public TenderCommitteeRole Role { get; set; }
    public DateTime? AppointmentDate { get; set; }
}

public class CreateUpdateTenderMandatoryRequirementDto
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(250)]
    public string RequirementDescription { get; set; }

    public bool IsMandatory { get; set; } = true;
}

public class SubmitTenderBidInput
{
    [Required]
    [StringLength(30)]
    public string BidderNo { get; set; }

    [Required]
    [StringLength(100)]
    public string BidderName { get; set; }

    [StringLength(100)]
    public string Email { get; set; }

    public decimal BidAmount { get; set; }
    public int DeliveryPeriodDays { get; set; } = 30;
    public bool PassedMandatory { get; set; } = true;
    public string Remarks { get; set; }
}

public class EvaluateTenderBidInput
{
    [Required]
    public Guid BidId { get; set; }

    public bool PassedMandatory { get; set; }
    public decimal TechnicalScore { get; set; }
    public decimal FinancialScore { get; set; }
    public int Rank { get; set; }

    [StringLength(500)]
    public string Remarks { get; set; }
}

public class AwardTenderInput
{
    [Required]
    [StringLength(30)]
    public string VendorNo { get; set; }

    [Required]
    [StringLength(100)]
    public string VendorName { get; set; }

    public decimal AwardAmount { get; set; }
    public DateTime? AwardDate { get; set; }
    public bool GeneratePurchaseOrder { get; set; } = true;
}

public class GetTenderListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public TenderType? TenderType { get; set; }
    public TenderStatus? Status { get; set; }
}
