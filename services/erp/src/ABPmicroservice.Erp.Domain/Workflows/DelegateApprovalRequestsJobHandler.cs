using System.Threading.Tasks;
using ABPmicroservice.Erp.JobQueue;
using Volo.Abp.DependencyInjection;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>
/// Delegates approval requests nobody has acted on within their workflow's delegate-after period,
/// to the approver's substitute or their own approver. Meant to run daily.
/// </summary>
public class DelegateApprovalRequestsJobHandler : IJobHandler, ITransientDependency
{
    public string JobType => "DelegateApprovalRequests";
    public string DisplayName => "Delegate Approval Requests";
    public string Description => "Delegates open approval requests that have waited longer than their workflow's delegate-after period.";
    public string DefaultParametersJson => "{}";

    private readonly ApprovalsManager _approvalsManager;

    public DelegateApprovalRequestsJobHandler(ApprovalsManager approvalsManager)
    {
        _approvalsManager = approvalsManager;
    }

    public async Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context)
    {
        var delegated = await _approvalsManager.DelegateOverdueRequestsAsync();
        return JobExecutionResult.Ok(delegated, $"Delegated {delegated} approval request(s).");
    }
}
