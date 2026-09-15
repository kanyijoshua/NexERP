using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Reporting;

public class FinancialReportAppService : ApplicationService
{
    private readonly FinancialReportEngine _reportEngine;

    public FinancialReportAppService(FinancialReportEngine reportEngine)
    {
        _reportEngine = reportEngine;
    }

    [Authorize(ErpPermissions.Reports.Default)]
    public async Task<FinancialReportResultDto> GetTrialBalanceAsync(DateTime fromDate, DateTime toDate)
    {
        return await _reportEngine.GenerateTrialBalanceAsync(fromDate, toDate);
    }
}
