using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>
/// Dynamic Workflow Evaluation Engine.
/// Mirrors Business Central Workflow Event & Response Management.
/// </summary>
public class WorkflowEngine : DomainService
{
    private readonly IRepository<Workflow, Guid> _workflowRepository;
    private readonly IRepository<ApprovalEntry, Guid> _approvalEntryRepository;

    public WorkflowEngine(
        IRepository<Workflow, Guid> workflowRepository,
        IRepository<ApprovalEntry, Guid> approvalEntryRepository
    )
    {
        _workflowRepository = workflowRepository;
        _approvalEntryRepository = approvalEntryRepository;
    }

    public async Task<bool> ProcessEventAsync(string eventName, string tableName, Guid documentId, string documentNo, decimal amount, Guid senderId, Guid approverId)
    {
        var activeWorkflow = await _workflowRepository.FirstOrDefaultAsync(w => w.Enabled);
        if (activeWorkflow == null)
        {
            return false; // No workflow active, auto-approve
        }

        var approvalEntry = new ApprovalEntry(
            GuidGenerator.Create(),
            tableName,
            documentId,
            documentNo,
            senderId,
            approverId,
            amount
        );

        await _approvalEntryRepository.InsertAsync(approvalEntry);
        return true; // Sent for approval
    }
}
