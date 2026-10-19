using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Projects;

public class ProjectsSetupDto
{
    public string ProjectNos { get; set; }
    public string ResourceNos { get; set; }
    public string TimeSheetNos { get; set; }
    public string DefaultPostingGroup { get; set; }
}

public interface IProjectsSetupAppService : IApplicationService
{
    Task<ProjectsSetupDto> GetAsync();

    Task<ProjectsSetupDto> UpdateAsync(ProjectsSetupDto input);
}

public class ProjectPostingGroupDto : CodeTableDto
{
    public string ProjectCostsAccount { get; set; }
    public string ProjectSalesAccount { get; set; }
    public string WipCostsAccount { get; set; }
    public string ProjectCostsAppliedAccount { get; set; }
    public string WipSalesAccount { get; set; }
    public string RecognizedSalesAccount { get; set; }
}

public class CreateUpdateProjectPostingGroupDto : CreateUpdateCodeTableDto
{
    public string ProjectCostsAccount { get; set; }
    public string ProjectSalesAccount { get; set; }
    public string WipCostsAccount { get; set; }
    public string ProjectCostsAppliedAccount { get; set; }
    public string WipSalesAccount { get; set; }
    public string RecognizedSalesAccount { get; set; }
}

public interface IProjectPostingGroupAppService : ICrudAppService<ProjectPostingGroupDto, Guid, GetCodeTableListInput, CreateUpdateProjectPostingGroupDto> { }

public class ResourceDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Name { get; set; }
    public ResourceType Type { get; set; }
    public string EmployeeNo { get; set; }
    public string UnitOfMeasureCode { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CapacityPerDay { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateResourceDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    public ResourceType Type { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string EmployeeNo { get; set; }

    [StringLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength)]
    public string UnitOfMeasureCode { get; set; }

    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CapacityPerDay { get; set; } = 8m;
    public bool Blocked { get; set; }
}

public interface IResourceAppService : ICrudAppService<ResourceDto, Guid, GetCodeTableListInput, CreateUpdateResourceDto> { }

public class ProjectDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public Guid? CustomerId { get; set; }
    public string CustomerNo { get; set; }
    public string CustomerName { get; set; }
    public ProjectStatus Status { get; set; }
    public string PostingGroup { get; set; }
    public ProjectWipMethod WipMethod { get; set; }
    public DateTime? StartingDate { get; set; }
    public DateTime? EndingDate { get; set; }
    public string PersonResponsible { get; set; }
}

public class CreateUpdateProjectDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }

    public ProjectStatus Status { get; set; }

    [StringLength(ErpDomainConsts.MaxPostingGroupLength)]
    public string PostingGroup { get; set; }

    public ProjectWipMethod WipMethod { get; set; }
    public DateTime? StartingDate { get; set; }
    public DateTime? EndingDate { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string PersonResponsible { get; set; }
}

public class ProjectTaskDto : EntityDto<Guid>
{
    public Guid ProjectId { get; set; }
    public string TaskNo { get; set; }
    public string Description { get; set; }
    public ProjectTaskType TaskType { get; set; }
    public int Indentation { get; set; }
}

public class CreateUpdateProjectTaskDto
{
    public Guid ProjectId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string TaskNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public ProjectTaskType TaskType { get; set; }
    public int Indentation { get; set; }
}

public class GetProjectTaskListInput : ErpPagedListInput
{
    public Guid? ProjectId { get; set; }
}

public class ProjectPlanningLineDto : EntityDto<Guid>
{
    public string ProjectNo { get; set; }
    public string TaskNo { get; set; }
    public int LineNo { get; set; }
    public ProjectPlanningLineType LineType { get; set; }
    public ProjectLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public DateTime PlanningDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal QtyTransferredToInvoice { get; set; }
    public string InvoiceNo { get; set; }
}

public class CreateUpdateProjectPlanningLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ProjectNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string TaskNo { get; set; }

    public ProjectPlanningLineType LineType { get; set; }
    public ProjectLineType Type { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public DateTime PlanningDate { get; set; }
    public decimal Quantity { get; set; }

    /// <summary>Blank takes the resource's or item's cost and price.</summary>
    public decimal? UnitCost { get; set; }

    public decimal? UnitPrice { get; set; }
}

public class GetProjectPlanningLineListInput : ErpPagedListInput
{
    public string ProjectNo { get; set; }
}

public class ProjectTaskStatisticsDto
{
    public string TaskNo { get; set; }
    public string Description { get; set; }
    public ProjectTaskType TaskType { get; set; }
    public int Indentation { get; set; }
    public decimal BudgetCost { get; set; }
    public decimal BudgetPrice { get; set; }
    public decimal UsageCost { get; set; }
    public decimal UsagePrice { get; set; }
    public decimal InvoicedPrice { get; set; }
}

public class ProjectPostingDateInput
{
    public DateTime PostingDate { get; set; }
}

public class ProjectWipResultDto
{
    public decimal Wip { get; set; }
}

public class ProjectInvoiceResultDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNo { get; set; }
    public int LineCount { get; set; }
}

public interface IProjectAppService : ICrudAppService<ProjectDto, Guid, GetCodeTableListInput, CreateUpdateProjectDto>
{
    Task<List<ProjectTaskStatisticsDto>> GetStatisticsAsync(Guid id);

    /// <summary>Values and posts the project's work in progress, reversing the last WIP run.</summary>
    Task<ProjectWipResultDto> CalculateWipAsync(Guid id, ProjectPostingDateInput input);

    /// <summary>Makes a sales invoice of what the billable planning lines have not invoiced yet.</summary>
    Task<ProjectInvoiceResultDto> MakeSalesInvoiceAsync(Guid id, ProjectPostingDateInput input);
}

public interface IProjectTaskAppService : ICrudAppService<ProjectTaskDto, Guid, GetProjectTaskListInput, CreateUpdateProjectTaskDto> { }

public interface IProjectPlanningLineAppService : ICrudAppService<ProjectPlanningLineDto, Guid, GetProjectPlanningLineListInput, CreateUpdateProjectPlanningLineDto> { }

public class ProjectJournalBatchDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class CreateUpdateProjectJournalBatchDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxJournalTemplateNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class ProjectJournalLineDto : EntityDto<Guid>
{
    public Guid ProjectJournalBatchId { get; set; }
    public int LineNo { get; set; }
    public DateTime PostingDate { get; set; }
    public string DocumentNo { get; set; }
    public string ProjectNo { get; set; }
    public string TaskNo { get; set; }
    public ProjectLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? UnitPrice { get; set; }
    public bool Chargeable { get; set; }
    public string LocationCode { get; set; }
    public string BalAccountNo { get; set; }
}

public class CreateUpdateProjectJournalLineDto
{
    public Guid ProjectJournalBatchId { get; set; }
    public DateTime PostingDate { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ProjectNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string TaskNo { get; set; }

    public ProjectLineType Type { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? UnitPrice { get; set; }
    public bool Chargeable { get; set; } = true;

    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string BalAccountNo { get; set; }
}

public class GetProjectJournalLineListInput : ErpPagedListInput
{
    public Guid? ProjectJournalBatchId { get; set; }
}

public interface IProjectJournalBatchAppService : ICrudAppService<ProjectJournalBatchDto, Guid, GetCodeTableListInput, CreateUpdateProjectJournalBatchDto>
{
    Task<int> RunPostingAsync(Guid id);
}

public interface IProjectJournalLineAppService : ICrudAppService<ProjectJournalLineDto, Guid, GetProjectJournalLineListInput, CreateUpdateProjectJournalLineDto> { }

public class ProjectLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public string ProjectNo { get; set; }
    public string TaskNo { get; set; }
    public DateTime PostingDate { get; set; }
    public string DocumentNo { get; set; }
    public ProjectLedgerEntryType EntryType { get; set; }
    public ProjectLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public bool Chargeable { get; set; }
}

public class ResourceLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public string ResourceNo { get; set; }
    public DateTime PostingDate { get; set; }
    public string DocumentNo { get; set; }
    public string ProjectNo { get; set; }
    public string TaskNo { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }
    public bool Chargeable { get; set; }
}

public class GetProjectLedgerListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string ProjectNo { get; set; }
    public string ResourceNo { get; set; }
}

public interface IProjectLedgerEntryAppService : IReadOnlyAppService<ProjectLedgerEntryDto, Guid, GetProjectLedgerListInput> { }

public interface IResourceLedgerEntryAppService : IReadOnlyAppService<ResourceLedgerEntryDto, Guid, GetProjectLedgerListInput> { }

public class TimeSheetLineDto : EntityDto<Guid>
{
    public int LineNo { get; set; }
    public string ProjectNo { get; set; }
    public string TaskNo { get; set; }
    public string Description { get; set; }
    public bool Chargeable { get; set; }
    public List<decimal> Days { get; set; } = new();
    public decimal TotalHours { get; set; }
    public bool Posted { get; set; }
}

public class TimeSheetDto : EntityDto<Guid>
{
    public string No { get; set; }
    public string ResourceNo { get; set; }
    public string ResourceName { get; set; }
    public DateTime StartingDate { get; set; }
    public DateTime EndingDate { get; set; }
    public TimeSheetStatus Status { get; set; }
    public string ApproverComment { get; set; }
    public decimal TotalHours { get; set; }
    public List<TimeSheetLineDto> Lines { get; set; } = new();
}

public class TimeSheetLineInputDto
{
    public Guid? Id { get; set; }

    [Required]
    public string ProjectNo { get; set; }

    [Required]
    public string TaskNo { get; set; }

    public string Description { get; set; }
    public bool Chargeable { get; set; } = true;
    public List<decimal> Days { get; set; } = new();
}

public class CreateUpdateTimeSheetDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    public string ResourceNo { get; set; }

    public DateTime StartingDate { get; set; }
    public List<TimeSheetLineInputDto> Lines { get; set; } = new();
}

public class GetTimeSheetListInput : ErpPagedListInput
{
    public string ResourceNo { get; set; }
    public TimeSheetStatus? Status { get; set; }
}

public class RejectTimeSheetInput
{
    public string Comment { get; set; }
}

public class TransferTimeSheetInput
{
    public Guid ProjectJournalBatchId { get; set; }
}

public interface ITimeSheetAppService : ICrudAppService<TimeSheetDto, Guid, GetTimeSheetListInput, CreateUpdateTimeSheetDto>
{
    Task<TimeSheetDto> SubmitAsync(Guid id);

    Task<TimeSheetDto> ApproveAsync(Guid id);

    Task<TimeSheetDto> RejectAsync(Guid id, RejectTimeSheetInput input);

    Task<TimeSheetDto> ReopenAsync(Guid id);

    /// <summary>Moves the approved hours into a project journal batch. Returns the journal lines made.</summary>
    Task<int> TransferToJournalAsync(Guid id, TransferTimeSheetInput input);
}
