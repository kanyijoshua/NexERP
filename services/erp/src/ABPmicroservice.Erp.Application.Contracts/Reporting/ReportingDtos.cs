using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace ABPmicroservice.Erp.Reporting;

public class ReportColumnDto
{
    public string Key { get; set; }

    public string Header { get; set; }

    public ReportColumnKind Kind { get; set; }
}

public class ReportRowDto
{
    public Dictionary<string, object> Values { get; set; } = new();

    public bool Bold { get; set; }

    public bool Italic { get; set; }

    public int Indentation { get; set; }

    /// <summary>Accounts behind the row, to hand to the detail report when the user drills in.</summary>
    public string DrillDownFilter { get; set; }
}

/// <summary>Every report comes back in this shape, whichever report it is.</summary>
public class ReportResultDto
{
    public string Title { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public List<ReportColumnDto> Columns { get; set; } = new();

    public List<ReportRowDto> Rows { get; set; } = new();
}

public class FinancialReportPeriodInput
{
    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    /// <summary>Account filter in Totaling syntax, e.g. "1000..1999|2100".</summary>
    [StringLength(ErpDomainConsts.MaxTotalingLength)]
    public string AccountFilter { get; set; }

    public bool ExcludeZeroBalances { get; set; } = true;
}

public class AgedAccountsInput
{
    public AgedLedgerKind Kind { get; set; }

    public DateTime AsOfDate { get; set; }

    public AgingMethod AgingMethod { get; set; } = AgingMethod.DueDate;

    [Range(1, 365)]
    public int PeriodLengthDays { get; set; } = 30;

    public bool ExcludeZeroBalances { get; set; } = true;
}

public class RunAccountScheduleInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string ScheduleName { get; set; }

    /// <summary>Blank runs a single net-change column over the period.</summary>
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string ColumnLayoutName { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }
}

/// <summary>Everything a report export needs: which report, its parameters and the file format.</summary>
public class ReportExportInput
{
    public ReportKind Report { get; set; }

    public ExportFormat Format { get; set; } = ExportFormat.Xlsx;

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    [StringLength(ErpDomainConsts.MaxTotalingLength)]
    public string AccountFilter { get; set; }

    public bool ExcludeZeroBalances { get; set; } = true;

    /// <summary>Aged reports only.</summary>
    public AgingMethod AgingMethod { get; set; } = AgingMethod.DueDate;

    /// <summary>Aged reports only.</summary>
    [Range(1, 365)]
    public int PeriodLengthDays { get; set; } = 30;

    /// <summary>Account schedule only.</summary>
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string ScheduleName { get; set; }

    /// <summary>Account schedule only.</summary>
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string ColumnLayoutName { get; set; }
}

public class AccountScheduleDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string DefaultColumnLayoutName { get; set; }

    public int LineCount { get; set; }
}

public class CreateUpdateAccountScheduleDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string DefaultColumnLayoutName { get; set; }
}

public class AccountScheduleLineDto : EntityDto<Guid>
{
    public Guid AccountScheduleId { get; set; }

    public int LineNo { get; set; }

    public string RowNo { get; set; }

    public string Description { get; set; }

    public AccountScheduleTotalingType TotalingType { get; set; }

    public string Totaling { get; set; }

    public bool ShowOppositeSign { get; set; }

    public bool Bold { get; set; }

    public bool Italic { get; set; }

    public int Indentation { get; set; }

    public bool HideIfZero { get; set; }
}

public class CreateUpdateAccountScheduleLineDto
{
    public Guid AccountScheduleId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxRowNoLength)]
    public string RowNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public AccountScheduleTotalingType TotalingType { get; set; }

    [StringLength(ErpDomainConsts.MaxTotalingLength)]
    public string Totaling { get; set; }

    public bool ShowOppositeSign { get; set; }

    public bool Bold { get; set; }

    public bool Italic { get; set; }

    [Range(0, 5)]
    public int Indentation { get; set; }

    public bool HideIfZero { get; set; }
}

public class ColumnLayoutDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public string Description { get; set; }

    public int LineCount { get; set; }
}

public class CreateUpdateColumnLayoutDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class ColumnLayoutLineDto : EntityDto<Guid>
{
    public Guid ColumnLayoutId { get; set; }

    public int LineNo { get; set; }

    public string ColumnNo { get; set; }

    public string ColumnHeader { get; set; }

    public ColumnLayoutType ColumnType { get; set; }

    public string ComparisonDateFormula { get; set; }

    public bool ShowOppositeSign { get; set; }
}

public class CreateUpdateColumnLayoutLineDto
{
    public Guid ColumnLayoutId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxRowNoLength)]
    public string ColumnNo { get; set; }

    [StringLength(ErpDomainConsts.MaxColumnHeaderLength)]
    public string ColumnHeader { get; set; }

    public ColumnLayoutType ColumnType { get; set; }

    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string ComparisonDateFormula { get; set; }

    public bool ShowOppositeSign { get; set; }
}

public class ReportLayoutDto : EntityDto<Guid>
{
    public string ReportName { get; set; }

    public string LayoutName { get; set; }

    public ReportLayoutType LayoutType { get; set; }

    public string Description { get; set; }

    public bool IsDefault { get; set; }
}

/// <summary>A layout with its body, for editing and downloading.</summary>
public class ReportLayoutDetailDto : ReportLayoutDto
{
    public string TemplateContent { get; set; }
}

/// <summary>A report a layout can be attached to.</summary>
public class ReportNameDto
{
    /// <summary>The value stored on a layout, e.g. "TrialBalance" or "AccountSchedule:BALANCE".</summary>
    public string Name { get; set; }

    public string DisplayName { get; set; }
}

public class GetReportLayoutsInput
{
    /// <summary>Blank lists the layouts of every report.</summary>
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string ReportName { get; set; }
}

public class CreateUpdateReportLayoutDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string ReportName { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string LayoutName { get; set; }

    /// <summary>Only <see cref="ReportLayoutType.Html"/> can be rendered today.</summary>
    public ReportLayoutType LayoutType { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxLayoutTemplateLength)]
    public string TemplateContent { get; set; }
}

public class SetDefaultReportLayoutInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string ReportName { get; set; }

    public Guid LayoutId { get; set; }
}

/// <summary>Renders a layout against sample figures, so it can be checked before it is saved.</summary>
public class PreviewReportLayoutInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxLayoutTemplateLength)]
    public string TemplateContent { get; set; }
}

/// <summary>
/// The financial statements, calculated from the G/L entries.
/// </summary>
public interface IFinancialReportAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/financial-report/trial-balance.</summary>
    Task<ReportResultDto> GetTrialBalanceAsync(FinancialReportPeriodInput input);

    Task<ReportResultDto> GetIncomeStatementAsync(FinancialReportPeriodInput input);

    Task<ReportResultDto> GetBalanceSheetAsync(FinancialReportPeriodInput input);

    /// <summary>Entry-level detail; where a drill-down from any other report lands.</summary>
    Task<ReportResultDto> GetGLDetailAsync(FinancialReportPeriodInput input);

    Task<ReportResultDto> GetAgedAccountsAsync(AgedAccountsInput input);

    /// <summary>Runs a user-defined account schedule against a column layout.</summary>
    Task<ReportResultDto> RunScheduleAsync(RunAccountScheduleInput input);

    /// <summary>Any of the reports above as a file. Routed as POST /api/erp/financial-report/run-export.</summary>
    Task<IRemoteStreamContent> RunExportAsync(ReportExportInput input);
}

/// <summary>
/// Account schedules and their lines. Mirrors Business Central tables 84 and 85.
/// </summary>
public interface IAccountScheduleAppService : IApplicationService
{
    Task<ListResultDto<AccountScheduleDto>> GetListAsync();

    Task<AccountScheduleDto> CreateAsync(CreateUpdateAccountScheduleDto input);

    Task<AccountScheduleDto> UpdateAsync(Guid id, CreateUpdateAccountScheduleDto input);

    Task DeleteAsync(Guid id);

    Task<ListResultDto<AccountScheduleLineDto>> GetLinesAsync(Guid scheduleId);

    Task<AccountScheduleLineDto> CreateLineAsync(CreateUpdateAccountScheduleLineDto input);

    Task<AccountScheduleLineDto> UpdateLineAsync(Guid id, CreateUpdateAccountScheduleLineDto input);

    Task DeleteLineAsync(Guid id);
}

/// <summary>
/// Column layouts and their lines. Mirrors Business Central tables 333 and 334.
/// </summary>
public interface IColumnLayoutAppService : IApplicationService
{
    Task<ListResultDto<ColumnLayoutDto>> GetListAsync();

    Task<ColumnLayoutDto> CreateAsync(CreateUpdateColumnLayoutDto input);

    Task<ColumnLayoutDto> UpdateAsync(Guid id, CreateUpdateColumnLayoutDto input);

    Task DeleteAsync(Guid id);

    Task<ListResultDto<ColumnLayoutLineDto>> GetLinesAsync(Guid columnLayoutId);

    Task<ColumnLayoutLineDto> CreateLineAsync(CreateUpdateColumnLayoutLineDto input);

    Task<ColumnLayoutLineDto> UpdateLineAsync(Guid id, CreateUpdateColumnLayoutLineDto input);

    Task DeleteLineAsync(Guid id);
}

/// <summary>
/// The layouts a report can be printed through. Mirrors Business Central's Report Layouts page
/// and table 9651 "Report Layout Selection".
/// </summary>
public interface IReportLayoutAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/report-layout?reportName=..</summary>
    Task<ListResultDto<ReportLayoutDto>> GetListAsync(GetReportLayoutsInput input);

    Task<ReportLayoutDetailDto> GetAsync(Guid id);

    /// <summary>The reports a layout can be attached to. Routed as GET /api/erp/report-layout/report-names.</summary>
    Task<ListResultDto<ReportNameDto>> GetReportNamesAsync();

    /// <summary>
    /// The built-in layout, which is the starting point for a custom one.
    /// Routed as GET /api/erp/report-layout/built-in-template.
    /// </summary>
    Task<string> GetBuiltInTemplateAsync();

    Task<ReportLayoutDto> CreateAsync(CreateUpdateReportLayoutDto input);

    Task<ReportLayoutDto> UpdateAsync(Guid id, CreateUpdateReportLayoutDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Routed as POST /api/erp/report-layout/set-default.</summary>
    Task SetDefaultAsync(SetDefaultReportLayoutInput input);

    /// <summary>Routed as POST /api/erp/report-layout/run-preview.</summary>
    Task<string> RunPreviewAsync(PreviewReportLayoutInput input);
}
