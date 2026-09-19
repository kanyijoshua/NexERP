using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace ABPmicroservice.Erp.Reporting;

[Authorize(ErpPermissions.Reports.Default)]
public class FinancialReportAppService : ErpAppService, IFinancialReportAppService
{
    private readonly FinancialReportEngine _reportEngine;

    public FinancialReportAppService(FinancialReportEngine reportEngine)
    {
        _reportEngine = reportEngine;
    }

    public async Task<FinancialReportDto> GetTrialBalanceAsync(FinancialReportPeriodInput input)
    {
        var result = await _reportEngine.GenerateTrialBalanceAsync(input.FromDate, input.ToDate);
        return ObjectMapper.Map<FinancialReportResultDto, FinancialReportDto>(result);
    }
}
