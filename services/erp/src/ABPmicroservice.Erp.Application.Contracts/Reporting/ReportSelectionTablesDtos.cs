using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Reporting;

public class ReportSelectionDto : FullAuditedEntityDto<Guid>
{
    public ReportSelectionUsage Usage { get; set; }
    public string Sequence { get; set; }
    public int ReportId { get; set; }
    public string CustomReportLayoutCode { get; set; }
    public bool UseForEmailAttachment { get; set; }
    public bool UseForEmailBody { get; set; }
    public string EmailBodyLayoutCode { get; set; }
    public string ReportLayoutName { get; set; }
}

public class CreateUpdateReportSelectionDto
{
    public ReportSelectionUsage Usage { get; set; }

    [Required]
    [StringLength(10)]
    public string Sequence { get; set; }

    public int ReportId { get; set; }

    [StringLength(20)]
    public string CustomReportLayoutCode { get; set; }

    public bool UseForEmailAttachment { get; set; }

    public bool UseForEmailBody { get; set; }

    [StringLength(20)]
    public string EmailBodyLayoutCode { get; set; }

    [StringLength(250)]
    public string ReportLayoutName { get; set; }
}

public class GetReportSelectionListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
}

/// <summary>Report Selections.</summary>
public interface IReportSelectionAppService : ICrudAppService<ReportSelectionDto, Guid, GetReportSelectionListInput, CreateUpdateReportSelectionDto, CreateUpdateReportSelectionDto> { }

public class CustomReportSelectionDto : FullAuditedEntityDto<Guid>
{
    public int SourceType { get; set; }
    public string SourceNo { get; set; }
    public ReportSelectionUsage Usage { get; set; }
    public int Sequence { get; set; }
    public int ReportId { get; set; }
    public string CustomReportLayoutCode { get; set; }
    public string SendToEmail { get; set; }
    public bool UseForEmailAttachment { get; set; }
    public bool UseForEmailBody { get; set; }
    public string EmailBodyLayoutCode { get; set; }
    public bool UseEmailFromContact { get; set; }
}

public class CreateUpdateCustomReportSelectionDto
{
    public int SourceType { get; set; }

    [Required]
    [StringLength(20)]
    public string SourceNo { get; set; }

    public ReportSelectionUsage Usage { get; set; }

    public int Sequence { get; set; }

    public int ReportId { get; set; }

    [StringLength(20)]
    public string CustomReportLayoutCode { get; set; }

    [StringLength(200)]
    public string SendToEmail { get; set; }

    public bool UseForEmailAttachment { get; set; }

    public bool UseForEmailBody { get; set; }

    [StringLength(20)]
    public string EmailBodyLayoutCode { get; set; }

    public bool UseEmailFromContact { get; set; }
}

public class GetCustomReportSelectionListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string SourceNo { get; set; }
}

/// <summary>Custom Report Selections.</summary>
public interface ICustomReportSelectionAppService : ICrudAppService<CustomReportSelectionDto, Guid, GetCustomReportSelectionListInput, CreateUpdateCustomReportSelectionDto, CreateUpdateCustomReportSelectionDto> { }
