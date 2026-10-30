using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.MaintenanceManagement;

public class MaintenanceTeamDto : CodeTableDto { }

public class CreateUpdateMaintenanceTeamDto : CreateUpdateCodeTableDto { }

public interface IMaintenanceTeamAppService : ICrudAppService<MaintenanceTeamDto, Guid, GetCodeTableListInput, CreateUpdateMaintenanceTeamDto> { }

public class MaintenanceEquipmentDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string SerialNo { get; set; }
    public string FixedAssetNo { get; set; }
    public string LocationCode { get; set; }
    public string TeamCode { get; set; }
    public string TechnicianEmployeeNo { get; set; }
    public int PreventiveIntervalDays { get; set; }
    public DateTime? NextPreventiveDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public EquipmentStatus Status { get; set; }
}

public class CreateUpdateMaintenanceEquipmentDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Category { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SerialNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string FixedAssetNo { get; set; }

    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string TeamCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string TechnicianEmployeeNo { get; set; }

    [Range(0, 3650)]
    public int PreventiveIntervalDays { get; set; }

    public DateTime? NextPreventiveDate { get; set; }
}

public class GetMaintenanceListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string EquipmentNo { get; set; }
}

public interface IMaintenanceEquipmentAppService
    : ICrudAppService<MaintenanceEquipmentDto, Guid, GetMaintenanceListInput, CreateUpdateMaintenanceEquipmentDto> { }

public class MaintenanceRequestDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string EquipmentNo { get; set; }
    public string Subject { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenancePriority Priority { get; set; }
    public MaintenanceStage Stage { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string TeamCode { get; set; }
    public string TechnicianEmployeeNo { get; set; }
    public decimal DurationHours { get; set; }
    public decimal Cost { get; set; }
    public string Notes { get; set; }
}

public class CreateUpdateMaintenanceRequestDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string EquipmentNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Subject { get; set; }

    public MaintenanceType Type { get; set; }
    public MaintenancePriority Priority { get; set; } = MaintenancePriority.Normal;
    public DateTime? RequestDate { get; set; }
    public DateTime? ScheduledDate { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string TeamCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string TechnicianEmployeeNo { get; set; }

    public decimal DurationHours { get; set; }
    public decimal Cost { get; set; }

    [StringLength(2000)]
    public string Notes { get; set; }
}

public class GeneratePreventiveInput
{
    public DateTime? AsOfDate { get; set; }
}

public class GeneratePreventiveResultDto
{
    public int RequestsCreated { get; set; }
}

public interface IMaintenanceRequestAppService
    : ICrudAppService<MaintenanceRequestDto, Guid, GetMaintenanceListInput, CreateUpdateMaintenanceRequestDto>
{
    Task<MaintenanceRequestDto> StartAsync(Guid id);

    Task<MaintenanceRequestDto> CompleteAsync(Guid id);

    Task<MaintenanceRequestDto> ScrapAsync(Guid id);

    Task<GeneratePreventiveResultDto> GeneratePreventiveAsync(GeneratePreventiveInput input);
}
