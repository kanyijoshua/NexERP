using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>One report of the standard catalog, and what its request page asks for.</summary>
public class StandardReportDto
{
    /// <summary>The report ID, which is also what Report Selections stores.</summary>
    public int Id { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public string Area { get; set; }

    public bool HasPeriod { get; set; }

    public bool HasAsOfDate { get; set; }

    public bool HasNoFilter { get; set; }

    public bool HasBudgetName { get; set; }

    public bool HasDepreciationBook { get; set; }

    public bool HasScheme { get; set; }
}

public class RunStandardReportInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Code { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    /// <summary>Record numbers in filter syntax, e.g. "V0010..V0020|V0100".</summary>
    [StringLength(ErpDomainConsts.MaxTotalingLength)]
    public string NoFilter { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string BudgetName { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string DepreciationBookCode { get; set; }

    [StringLength(ErpDomainConsts.MaxDimensionValueCodeLength)]
    public string SchemeCode { get; set; }
}

public class StandardReportExportInput : RunStandardReportInput
{
    public ExportFormat Format { get; set; } = ExportFormat.Xlsx;
}

/// <summary>
/// The standard reports: the list, balance and register reports of finance,
/// receivables, payables, cash management, human resources and fixed assets.
/// </summary>
public interface IStandardReportAppService : IApplicationService
{
    /// <summary>The reports the caller may run. Routed as GET /api/erp/standard-report.</summary>
    Task<List<StandardReportDto>> GetListAsync();

    /// <summary>Routed as POST /api/erp/standard-report/run.</summary>
    Task<ReportResultDto> RunAsync(RunStandardReportInput input);

    /// <summary>Routed as POST /api/erp/standard-report/run-export.</summary>
    Task<IRemoteStreamContent> RunExportAsync(StandardReportExportInput input);
}
