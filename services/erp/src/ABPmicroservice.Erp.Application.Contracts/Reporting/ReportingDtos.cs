using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Reporting;

public class FinancialReportDto
{
    public string ReportTitle { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<FinancialReportLineDto> Rows { get; set; } = new();
}

public class FinancialReportLineDto
{
    public string RowNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
}

public class FinancialReportPeriodInput
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public interface IFinancialReportAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/financial-report/trial-balance?fromDate=..&amp;toDate=..</summary>
    Task<FinancialReportDto> GetTrialBalanceAsync(FinancialReportPeriodInput input);
}

public class ReportLayoutDto : EntityDto<Guid>
{
    public string ReportName { get; set; }
    public string LayoutName { get; set; }
    public string LayoutType { get; set; }
    public string Description { get; set; }
    public bool IsDefault { get; set; }
}

public class GetReportLayoutsInput
{
    [Required]
    public string ReportName { get; set; }
}

public class CreateReportLayoutDto
{
    [Required]
    public string ReportName { get; set; }

    [Required]
    public string LayoutName { get; set; }

    /// <summary>"RDLC", "Word", "Excel" or "Html".</summary>
    [Required]
    public string LayoutType { get; set; }

    public string Description { get; set; }
}

public class SetDefaultReportLayoutInput
{
    [Required]
    public string ReportName { get; set; }

    public Guid LayoutId { get; set; }
}

public interface IReportLayoutAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/report-layout?reportName=..</summary>
    Task<ListResultDto<ReportLayoutDto>> GetListAsync(GetReportLayoutsInput input);

    Task<ReportLayoutDto> CreateAsync(CreateReportLayoutDto input);

    /// <summary>Routed as POST /api/erp/report-layout/set-default.</summary>
    Task SetDefaultAsync(SetDefaultReportLayoutInput input);
}
