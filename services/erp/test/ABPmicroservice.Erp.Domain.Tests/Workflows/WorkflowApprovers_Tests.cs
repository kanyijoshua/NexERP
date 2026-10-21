using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>
/// Approver types beyond the approver chain: workflow user groups (sequential and parallel),
/// a specific approver, delegation after a timeout, templates and copies.
/// Users: clerk (sends, limit 1,000) -> supervisor (substitute: deputy) -> manager; plus reviewer.
/// </summary>
public class WorkflowApprovers_Tests : ErpDomainTestBase
{
    private static readonly Guid Clerk = Guid.NewGuid();
    private static readonly Guid Supervisor = Guid.NewGuid();
    private static readonly Guid Manager = Guid.NewGuid();
    private static readonly Guid Deputy = Guid.NewGuid();
    private static readonly Guid Reviewer = Guid.NewGuid();

    private readonly ApprovalsManager _approvals;
    private readonly IRepository<ApprovalEntry, Guid> _entries;
    private readonly IRepository<SalesHeader, Guid> _salesHeaders;
    private readonly IRepository<Workflow, Guid> _workflows;

    public WorkflowApprovers_Tests()
    {
        _approvals = GetRequiredService<ApprovalsManager>();
        _entries = GetRequiredService<IRepository<ApprovalEntry, Guid>>();
        _salesHeaders = GetRequiredService<IRepository<SalesHeader, Guid>>();
        _workflows = GetRequiredService<IRepository<Workflow, Guid>>();
    }

    [Fact]
    public async Task A_User_Group_Asks_Each_Sequence_In_Turn_And_Members_Of_One_Sequence_Together()
    {
        await SetUpUsersAsync();
        await AddGroupAsync("FINANCE", ("SUPERVISOR", 1), ("REVIEWER", 1), ("MANAGER", 2));
        await AddWorkflowAsync(w => w.SetApprovers(WorkflowApproverType.WorkflowUserGroup, "finance", null, null));
        var documentId = await NewInvoiceAsync("GRP-1", 500m);

        // Within the clerk's own limit, yet a group workflow still asks the group.
        var result = await SendAsync(documentId, Clerk);
        result.AutoApproved.ShouldBeFalse();
        result.ApproverCount.ShouldBe(3);

        var entries = await EntriesAsync(documentId);
        entries.Where(e => e.Status == ApprovalStatus.Open).Select(e => e.ApproverUserName)
            .ShouldBe(new[] { "reviewer", "supervisor" }, ignoreOrder: true);
        entries.Single(e => e.ApproverUserName == "manager").Status.ShouldBe(ApprovalStatus.Created);

        // One of two parallel approvals is not enough to move on.
        await ApproveAsync(entries.Single(e => e.ApproverUserName == "supervisor").Id, Supervisor);
        (await EntriesAsync(documentId)).Single(e => e.ApproverUserName == "manager").Status.ShouldBe(ApprovalStatus.Created);

        await ApproveAsync(entries.Single(e => e.ApproverUserName == "reviewer").Id, Reviewer);
        (await EntriesAsync(documentId)).Single(e => e.ApproverUserName == "manager").Status.ShouldBe(ApprovalStatus.Open);
        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.PendingApproval);

        await ApproveAsync(entries.Single(e => e.ApproverUserName == "manager").Id, Manager);
        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.Released);
    }

    [Fact]
    public async Task A_Group_Without_Members_Or_With_Unknown_Users_Is_Refused()
    {
        await SetUpUsersAsync();
        await AddGroupAsync("EMPTY");
        await AddWorkflowAsync(w => w.SetApprovers(WorkflowApproverType.WorkflowUserGroup, "EMPTY", null, null));
        var documentId = await NewInvoiceAsync("GRP-2", 500m);

        (await Should.ThrowAsync<BusinessException>(() => SendAsync(documentId, Clerk))).Code
            .ShouldBe(ErpErrorCodes.Approvals.WorkflowUserGroupEmpty);

        await AddGroupAsync("GHOSTS", ("NOBODY", 1));
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var workflow = await _workflows.SingleAsync(w => w.Enabled);
            workflow.SetApprovers(WorkflowApproverType.WorkflowUserGroup, "GHOSTS", null, null);
            await _workflows.UpdateAsync(workflow, autoSave: true);
        });

        (await Should.ThrowAsync<BusinessException>(() => SendAsync(documentId, Clerk))).Code
            .ShouldBe(ErpErrorCodes.Approvals.UserNotInApprovalSetup);
        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.Open);
    }

    [Fact]
    public async Task A_Specific_Approver_Gets_Every_Request_Whatever_The_Chain()
    {
        await SetUpUsersAsync();
        await AddWorkflowAsync(w => w.SetApprovers(WorkflowApproverType.Approver, null, Reviewer, "reviewer"),
            ApproverLimitType.SpecificApprover);
        var documentId = await NewInvoiceAsync("SPC-1", 20000m);

        var result = await SendAsync(documentId, Clerk);
        result.ApproverCount.ShouldBe(1);
        result.FirstApproverUserName.ShouldBe("reviewer");

        await ApproveAsync((await EntriesAsync(documentId)).Single().Id, Reviewer);
        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.Released);
    }

    [Fact]
    public void Approver_Settings_Are_Checked()
    {
        var workflow = new Workflow(Guid.NewGuid(), "CHK", "Check", ApprovalDocumentKind.PurchaseDocument,
            approverLimitType: ApproverLimitType.SpecificApprover);

        Should.Throw<BusinessException>(() => workflow.SetApprovers(WorkflowApproverType.Approver, null, null, null))
            .Code.ShouldBe(ErpErrorCodes.Approvals.SpecificApproverRequired);
        Should.Throw<BusinessException>(() => workflow.SetApprovers(WorkflowApproverType.WorkflowUserGroup, " ", null, null))
            .Code.ShouldBe(ErpErrorCodes.Approvals.WorkflowUserGroupRequired);

        // A group workflow keeps no specific approver.
        workflow.SetApprovers(WorkflowApproverType.WorkflowUserGroup, "fin", Reviewer, "reviewer");
        workflow.WorkflowUserGroupCode.ShouldBe("FIN");
        workflow.SpecificApproverUserId.ShouldBeNull();
    }

    [Fact]
    public async Task Requests_Left_Longer_Than_The_Delegate_After_Period_Go_To_The_Substitute()
    {
        await SetUpUsersAsync();
        await AddWorkflowAsync(w => w.SetDelegateAfterDays(2));
        var stale = await NewInvoiceAsync("DLG-1", 3000m);
        var fresh = await NewInvoiceAsync("DLG-2", 3000m);
        await SendAsync(stale, Clerk);
        await SendAsync(fresh, Clerk);

        // Pretend the first request has waited three days.
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var entry = await _entries.SingleAsync(e => e.DocumentId == stale);
            typeof(ApprovalEntry).GetProperty(nameof(ApprovalEntry.LastStatusChangeTime))!.SetValue(entry, DateTime.Now.AddDays(-3));
            await _entries.UpdateAsync(entry, autoSave: true);
        });

        (await InCompanyAsync(DefaultCompanyName, () => _approvals.DelegateOverdueRequestsAsync())).ShouldBe(1);

        (await EntriesAsync(stale)).Single().ApproverUserName.ShouldBe("deputy");
        (await EntriesAsync(fresh)).Single().ApproverUserName.ShouldBe("supervisor");

        // Delegating restarts the clock, so running again does nothing.
        (await InCompanyAsync(DefaultCompanyName, () => _approvals.DelegateOverdueRequestsAsync())).ShouldBe(0);
    }

    [Fact]
    public void Templates_Build_Disabled_Workflows_With_Their_Steps()
    {
        WorkflowTemplates.All.Select(t => t.Code).ShouldBeUnique();
        WorkflowTemplates.Find("voucher-approval").ShouldNotBeNull();

        var workflow = WorkflowTemplates.Find("TENDER-APPROVAL")!
            .CreateWorkflow(Guid.NewGuid(), "TENDER", null, "board", Guid.NewGuid);

        workflow.Enabled.ShouldBeFalse();
        workflow.Description.ShouldBe("Tender Approval by Workflow User Group");
        workflow.Category.ShouldBe("PROCUREMENT");
        workflow.WorkflowUserGroupCode.ShouldBe("BOARD");
        workflow.Steps.First().EventName.ShouldBe("Approval of a tender is requested.");

        var voucher = WorkflowTemplates.Find("VOUCHER-APPROVAL")!.CreateWorkflow(Guid.NewGuid(), "PV", null, null, Guid.NewGuid);
        voucher.Steps.ShouldContain(s => s.EventName == "An approval request is not acted on.");

        var copy = voucher.CopyAs(Guid.NewGuid(), "PV-2", null, Guid.NewGuid);
        copy.Code.ShouldBe("PV-2");
        copy.DelegateAfterDays.ShouldBe(voucher.DelegateAfterDays);
        copy.Steps.Select(s => s.ResponseAction).ShouldBe(voucher.Steps.OrderBy(s => s.SequenceNo).Select(s => s.ResponseAction));
    }

    private Task SetUpUsersAsync()
    {
        var users = new List<(Guid Id, string Name, Guid? Approver, Guid? Substitute, decimal Limit, bool Unlimited)>
        {
            (Clerk, "clerk", Supervisor, null, 1000m, false),
            (Supervisor, "supervisor", Manager, Deputy, 5000m, false),
            (Manager, "manager", null, null, 0m, true),
            (Deputy, "deputy", null, null, 5000m, false),
            (Reviewer, "reviewer", null, null, 0m, false),
        };

        return InCompanyAsync(DefaultCompanyName, async () =>
        {
            var setups = GetRequiredService<IRepository<ApprovalUserSetup, Guid>>();
            foreach (var u in users)
            {
                var setup = new ApprovalUserSetup(Guid.NewGuid(), u.Id, u.Name);
                setup.Update(u.Name, u.Approver, u.Substitute, u.Limit, u.Unlimited, 0m, false, false);
                await setups.InsertAsync(setup);
            }
        });
    }

    private Task AddGroupAsync(string code, params (string UserName, int SequenceNo)[] members)
    {
        return InCompanyAsync(DefaultCompanyName, async () =>
        {
            await GetRequiredService<IRepository<WorkflowUserGroup, Guid>>().InsertAsync(new WorkflowUserGroup(Guid.NewGuid(), code));
            var memberRepository = GetRequiredService<IRepository<WorkflowUserGroupMember, Guid>>();
            foreach (var m in members)
            {
                var member = new WorkflowUserGroupMember(Guid.NewGuid(), code, m.UserName);
                member.Set(m.SequenceNo);
                await memberRepository.InsertAsync(member);
            }
        });
    }

    private Task AddWorkflowAsync(Action<Workflow> configure, ApproverLimitType limitType = ApproverLimitType.ApproverChain)
    {
        return InCompanyAsync(DefaultCompanyName, async () =>
        {
            var workflow = new Workflow(Guid.NewGuid(), "WF-" + Guid.NewGuid().ToString("N")[..6],
                "Sales approval", ApprovalDocumentKind.SalesDocument, 0m, limitType, dueDays: 3);
            configure(workflow);
            workflow.RebuildSteps(Guid.NewGuid);
            workflow.Enable();
            await _workflows.InsertAsync(workflow);
        });
    }

    private Task<Guid> NewInvoiceAsync(string no, decimal amount)
    {
        return InCompanyAsync(DefaultCompanyName, async () =>
        {
            var customer = await GetRequiredService<IRepository<Customer, Guid>>().SingleAsync(c => c.No == "C00010");
            var header = new SalesHeader(Guid.NewGuid(), SalesDocumentType.Invoice, no, customer.Id, customer.No, customer.Name, new DateTime(2026, 6, 15));
            header.AddLine(Guid.NewGuid(), DocumentLineType.GLAccount, "4000", "Consulting", 1m, amount);
            await _salesHeaders.InsertAsync(header, autoSave: true);
            return header.Id;
        });
    }

    private Task<ApprovalRequestResult> SendAsync(Guid documentId, Guid sender) =>
        InCompanyAsync(DefaultCompanyName, () => _approvals.SendApprovalRequestAsync(ApprovalDocumentKind.SalesDocument, documentId, sender));

    private Task ApproveAsync(Guid entryId, Guid user) =>
        InCompanyAsync(DefaultCompanyName, () => _approvals.ApproveAsync(entryId, user));

    private async Task<DocumentStatus> StatusAsync(Guid documentId) =>
        (await InCompanyAsync(DefaultCompanyName, () => _salesHeaders.GetAsync(documentId))).Status;

    private async Task<List<ApprovalEntry>> EntriesAsync(Guid documentId) =>
        (await InCompanyAsync(DefaultCompanyName, () => _entries.GetListAsync(e => e.DocumentId == documentId)))
            .OrderBy(e => e.SequenceNo).ThenBy(e => e.ApproverUserName).ToList();
}
