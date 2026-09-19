using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Chatter;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>
/// Chain used throughout: clerk (limit 1,000) -> supervisor (limit 5,000, substitute: deputy)
/// -> manager (unlimited). The deputy has no approver and a limit of 5,000.
/// </summary>
public class ApprovalsManager_Tests : ErpDomainTestBase
{
    private static readonly Guid Clerk = Guid.NewGuid();
    private static readonly Guid Supervisor = Guid.NewGuid();
    private static readonly Guid Manager = Guid.NewGuid();
    private static readonly Guid Deputy = Guid.NewGuid();
    private static readonly Guid Outsider = Guid.NewGuid();

    private readonly ApprovalsManager _approvals;
    private readonly IRepository<ApprovalEntry, Guid> _entries;
    private readonly IRepository<SalesHeader, Guid> _salesHeaders;

    public ApprovalsManager_Tests()
    {
        _approvals = GetRequiredService<ApprovalsManager>();
        _entries = GetRequiredService<IRepository<ApprovalEntry, Guid>>();
        _salesHeaders = GetRequiredService<IRepository<SalesHeader, Guid>>();
    }

    [Fact]
    public async Task Approver_Chain_Runs_Up_To_The_First_Sufficient_Limit_One_Approver_At_A_Time()
    {
        await SetUpAsync(ApproverLimitType.ApproverChain);
        var documentId = await NewInvoiceAsync("APR-CHAIN", 20000m);

        var result = await SendAsync(documentId, Clerk);
        result.AutoApproved.ShouldBeFalse();
        result.ApproverCount.ShouldBe(2);
        result.FirstApproverUserName.ShouldBe("supervisor");
        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.PendingApproval);

        var entries = await EntriesAsync(documentId);
        entries.Select(e => (e.ApproverUserName, e.Status)).ShouldBe(new[]
        {
            ("supervisor", ApprovalStatus.Open),
            ("manager", ApprovalStatus.Created),
        });

        // The manager cannot jump the queue: their entry is not open yet.
        (await Should.ThrowAsync<BusinessException>(() => ApproveAsync(entries[1].Id, Manager))).Code
            .ShouldBe(ErpErrorCodes.Approvals.EntryNotOpen);

        await ApproveAsync(entries[0].Id, Supervisor);
        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.PendingApproval);
        (await EntriesAsync(documentId))[1].Status.ShouldBe(ApprovalStatus.Open);

        await ApproveAsync(entries[1].Id, Manager);
        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.Released);
        (await EntriesAsync(documentId)).ShouldAllBe(e => e.Status == ApprovalStatus.Approved);
    }

    [Fact]
    public async Task The_Chain_Stops_At_The_Supervisor_When_Their_Limit_Is_Enough()
    {
        await SetUpAsync(ApproverLimitType.ApproverChain);
        var documentId = await NewInvoiceAsync("APR-MID", 3000m);

        (await SendAsync(documentId, Clerk)).ApproverCount.ShouldBe(1);

        await ApproveAsync((await EntriesAsync(documentId)).Single().Id, Supervisor);
        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.Released);
    }

    [Fact]
    public async Task A_Sender_Within_Their_Own_Limit_Is_Approved_Automatically()
    {
        await SetUpAsync(ApproverLimitType.ApproverChain);
        var documentId = await NewInvoiceAsync("APR-AUTO", 500m);

        (await SendAsync(documentId, Clerk)).AutoApproved.ShouldBeTrue();

        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.Released);
        var entry = (await EntriesAsync(documentId)).Single();
        entry.Status.ShouldBe(ApprovalStatus.Approved);
        entry.ApproverUserName.ShouldBe("clerk");
    }

    [Fact]
    public async Task First_Qualified_Approver_Skips_Those_Whose_Limit_Is_Too_Low()
    {
        await SetUpAsync(ApproverLimitType.FirstQualifiedApprover);
        var documentId = await NewInvoiceAsync("APR-FQA", 20000m);

        (await SendAsync(documentId, Clerk)).ApproverCount.ShouldBe(1);
        (await EntriesAsync(documentId)).Single().ApproverUserName.ShouldBe("manager");
    }

    [Fact]
    public async Task Direct_Approver_Ignores_Limits()
    {
        await SetUpAsync(ApproverLimitType.DirectApprover);
        var small = await NewInvoiceAsync("APR-DIR-1", 500m);
        var large = await NewInvoiceAsync("APR-DIR-2", 20000m);

        // Even an amount within the clerk's own limit goes to the supervisor, and only to them.
        (await SendAsync(small, Clerk)).AutoApproved.ShouldBeFalse();
        await SendAsync(large, Clerk);

        (await EntriesAsync(small)).Single().ApproverUserName.ShouldBe("supervisor");
        (await EntriesAsync(large)).Single().ApproverUserName.ShouldBe("supervisor");
    }

    [Fact]
    public async Task A_Rejection_Ends_The_Whole_Request_And_Reopens_The_Document()
    {
        await SetUpAsync(ApproverLimitType.ApproverChain);
        var documentId = await NewInvoiceAsync("APR-REJ", 20000m);
        await SendAsync(documentId, Clerk);

        var first = (await EntriesAsync(documentId))[0];
        await InCompanyAsync(DefaultCompanyName, () => _approvals.RejectAsync(first.Id, Supervisor, "Price is below cost."));

        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.Open);
        var entries = await EntriesAsync(documentId);
        entries.ShouldAllBe(e => e.Status == ApprovalStatus.Rejected);
        entries[0].Comment.ShouldBe("Price is below cost.");

        // The sender can fix the document and ask again.
        await SendAsync(documentId, Clerk);
        (await EntriesAsync(documentId)).Count(e => e.IsPending).ShouldBe(2);
    }

    [Fact]
    public async Task Only_The_Approver_Or_An_Administrator_May_Act()
    {
        await SetUpAsync(ApproverLimitType.ApproverChain);
        var documentId = await NewInvoiceAsync("APR-WHO", 3000m);
        await SendAsync(documentId, Clerk);
        var entry = (await EntriesAsync(documentId)).Single();

        (await Should.ThrowAsync<BusinessException>(() => ApproveAsync(entry.Id, Clerk))).Code
            .ShouldBe(ErpErrorCodes.Approvals.NotTheApprover);
        (await Should.ThrowAsync<BusinessException>(() => ApproveAsync(entry.Id, Outsider))).Code
            .ShouldBe(ErpErrorCodes.Approvals.NotTheApprover);

        // The manager is set up as approval administrator.
        await ApproveAsync(entry.Id, Manager);
        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.Released);
    }

    [Fact]
    public async Task Delegation_Goes_To_The_Substitute_Then_Falls_Back_To_The_Approver()
    {
        await SetUpAsync(ApproverLimitType.ApproverChain);
        var documentId = await NewInvoiceAsync("APR-DEL", 3000m);
        await SendAsync(documentId, Clerk);
        var entry = (await EntriesAsync(documentId)).Single();

        await InCompanyAsync(DefaultCompanyName, () => _approvals.DelegateAsync(entry.Id, Supervisor));
        (await EntriesAsync(documentId)).Single().ApproverUserName.ShouldBe("deputy");

        // The supervisor no longer holds it; the deputy does.
        (await Should.ThrowAsync<BusinessException>(() => ApproveAsync(entry.Id, Supervisor))).Code
            .ShouldBe(ErpErrorCodes.Approvals.NotTheApprover);

        // The deputy has neither substitute nor approver, so cannot pass it on.
        (await Should.ThrowAsync<BusinessException>(
            () => InCompanyAsync(DefaultCompanyName, () => _approvals.DelegateAsync(entry.Id, Deputy)))).Code
            .ShouldBe(ErpErrorCodes.Approvals.NoSubstituteOrApprover);

        await ApproveAsync(entry.Id, Deputy);
        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.Released);
    }

    [Fact]
    public async Task Only_The_Sender_Or_An_Administrator_May_Cancel()
    {
        await SetUpAsync(ApproverLimitType.ApproverChain);
        var documentId = await NewInvoiceAsync("APR-CAN", 20000m);
        await SendAsync(documentId, Clerk);

        (await Should.ThrowAsync<BusinessException>(() => CancelAsync(documentId, Supervisor))).Code
            .ShouldBe(ErpErrorCodes.Approvals.OnlySenderCanCancel);

        await CancelAsync(documentId, Clerk);
        (await StatusAsync(documentId)).ShouldBe(DocumentStatus.Open);
        (await EntriesAsync(documentId)).ShouldAllBe(e => e.Status == ApprovalStatus.Canceled);

        (await Should.ThrowAsync<BusinessException>(() => CancelAsync(documentId, Clerk))).Code
            .ShouldBe(ErpErrorCodes.Approvals.NothingToCancel);
    }

    [Fact]
    public async Task Sending_Is_Refused_When_It_Cannot_Work()
    {
        await SetUpAsync(ApproverLimitType.ApproverChain);
        var documentId = await NewInvoiceAsync("APR-BAD", 20000m);

        // A user the approval setup does not know.
        (await Should.ThrowAsync<BusinessException>(() => SendAsync(documentId, Outsider))).Code
            .ShouldBe(ErpErrorCodes.Approvals.UserNotInApprovalSetup);

        // The deputy's chain ends before anyone can approve 20,000.
        (await Should.ThrowAsync<BusinessException>(() => SendAsync(documentId, Deputy))).Code
            .ShouldBe(ErpErrorCodes.Approvals.NoQualifiedApprover);

        await SendAsync(documentId, Clerk);
        (await Should.ThrowAsync<BusinessException>(() => SendAsync(documentId, Clerk))).Code
            .ShouldBe(ErpErrorCodes.Approvals.ApprovalAlreadyRequested);

        var empty = await NewInvoiceAsync("APR-EMPTY", 0m);
        (await Should.ThrowAsync<BusinessException>(() => SendAsync(empty, Clerk))).Code
            .ShouldBe(ErpErrorCodes.Documents.DocumentHasNoLines);
    }

    [Fact]
    public async Task The_Workflow_Threshold_Decides_Whether_Release_Needs_Approval()
    {
        await SetUpAsync(ApproverLimitType.ApproverChain, minimumAmount: 1000m);
        var small = await NewInvoiceAsync("APR-THR-1", 999m);
        var large = await NewInvoiceAsync("APR-THR-2", 1000m);

        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            // Below the threshold no workflow applies: release freely, and there is nothing to send.
            await _approvals.EnsureCanReleaseAsync(ApprovalDocumentKind.SalesDocument, await _salesHeaders.GetAsync(small));

            var ex = await Should.ThrowAsync<BusinessException>(
                async () => await _approvals.EnsureCanReleaseAsync(ApprovalDocumentKind.SalesDocument, await _salesHeaders.GetAsync(large)));
            ex.Code.ShouldBe(ErpErrorCodes.Approvals.ApprovalRequired);
        });

        (await Should.ThrowAsync<BusinessException>(() => SendAsync(small, Clerk))).Code
            .ShouldBe(ErpErrorCodes.Approvals.NoWorkflowApplies);

        // Purchase documents have no workflow at all here.
        (await InCompanyAsync(DefaultCompanyName, () => _approvals.FindWorkflowAsync(ApprovalDocumentKind.PurchaseDocument, 1_000_000m)))
            .ShouldBeNull();
    }

    [Fact]
    public async Task A_Loop_In_The_Approver_Setup_Is_Detected()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        await SetUpAsync(ApproverLimitType.ApproverChain, extraUsers: new[]
        {
            (a, "loop-a", (Guid?)b, (Guid?)null, 10m, false, false),
            (b, "loop-b", (Guid?)a, (Guid?)null, 10m, false, false),
        });
        var documentId = await NewInvoiceAsync("APR-LOOP", 500m);

        (await Should.ThrowAsync<BusinessException>(() => SendAsync(documentId, a))).Code
            .ShouldBe(ErpErrorCodes.Approvals.ApprovalChainLoop);
    }

    [Fact]
    public async Task Every_Step_Is_Written_To_The_Documents_Activity_Stream()
    {
        await SetUpAsync(ApproverLimitType.ApproverChain);
        var documentId = await NewInvoiceAsync("APR-LOG", 3000m);
        await SendAsync(documentId, Clerk);
        await ApproveAsync((await EntriesAsync(documentId)).Single().Id, Supervisor);

        var stream = await InCompanyAsync(DefaultCompanyName,
            () => GetRequiredService<IRepository<ActivityStreamEntry, Guid>>()
                .GetListAsync(a => a.EntityType == nameof(SalesHeader) && a.EntityId == documentId));

        stream.Count.ShouldBe(2);
        stream.ShouldContain(a => a.ActionDescription.Contains("sent the document for approval to supervisor"));
        stream.ShouldContain(a => a.NewValue == nameof(DocumentStatus.Released));
    }

    private Task SetUpAsync(
        ApproverLimitType limitType,
        decimal minimumAmount = 0m,
        IEnumerable<(Guid Id, string Name, Guid? Approver, Guid? Substitute, decimal Limit, bool Unlimited, bool Admin)> extraUsers = null
    )
    {
        var users = new List<(Guid Id, string Name, Guid? Approver, Guid? Substitute, decimal Limit, bool Unlimited, bool Admin)>
        {
            (Clerk, "clerk", Supervisor, null, 1000m, false, false),
            (Supervisor, "supervisor", Manager, Deputy, 5000m, false, false),
            (Manager, "manager", null, null, 0m, true, true),
            (Deputy, "deputy", null, null, 5000m, false, false),
        };
        users.AddRange(extraUsers ?? Array.Empty<(Guid, string, Guid?, Guid?, decimal, bool, bool)>());

        return InCompanyAsync(DefaultCompanyName, async () =>
        {
            var setups = GetRequiredService<IRepository<ApprovalUserSetup, Guid>>();
            foreach (var u in users)
            {
                var setup = new ApprovalUserSetup(Guid.NewGuid(), u.Id, u.Name);
                setup.Update(u.Name, u.Approver, u.Substitute, u.Limit, u.Unlimited, 0m, false, u.Admin);
                await setups.InsertAsync(setup);
            }

            var workflow = new Workflow(Guid.NewGuid(), "SIAPW-" + Guid.NewGuid().ToString("N")[..6],
                "Sales Invoice Approval Workflow", ApprovalDocumentKind.SalesDocument, minimumAmount, limitType, dueDays: 3);
            workflow.RebuildSteps(Guid.NewGuid);
            workflow.Enable();
            await GetRequiredService<IRepository<Workflow, Guid>>().InsertAsync(workflow);
        });
    }

    private Task<Guid> NewInvoiceAsync(string no, decimal amount)
    {
        return InCompanyAsync(DefaultCompanyName, async () =>
        {
            var customer = await GetRequiredService<IRepository<Customer, Guid>>().SingleAsync(c => c.No == "C00010");
            var header = new SalesHeader(Guid.NewGuid(), SalesDocumentType.Invoice, no, customer.Id, customer.No, customer.Name, new DateTime(2026, 6, 15));
            if (amount > 0m)
            {
                header.AddLine(Guid.NewGuid(), DocumentLineType.GLAccount, "4000", "Consulting", 1m, amount);
            }

            await _salesHeaders.InsertAsync(header, autoSave: true);
            return header.Id;
        });
    }

    private Task<ApprovalRequestResult> SendAsync(Guid documentId, Guid sender) =>
        InCompanyAsync(DefaultCompanyName, () => _approvals.SendApprovalRequestAsync(ApprovalDocumentKind.SalesDocument, documentId, sender));

    private Task ApproveAsync(Guid entryId, Guid user) =>
        InCompanyAsync(DefaultCompanyName, () => _approvals.ApproveAsync(entryId, user));

    private Task CancelAsync(Guid documentId, Guid user) =>
        InCompanyAsync(DefaultCompanyName, () => _approvals.CancelApprovalRequestAsync(ApprovalDocumentKind.SalesDocument, documentId, user));

    private async Task<DocumentStatus> StatusAsync(Guid documentId) =>
        (await InCompanyAsync(DefaultCompanyName, () => _salesHeaders.GetAsync(documentId))).Status;

    // Latest request first would mix old and new; tests that resend only look at pending ones.
    private async Task<List<ApprovalEntry>> EntriesAsync(Guid documentId) =>
        (await InCompanyAsync(DefaultCompanyName, () => _entries.GetListAsync(e => e.DocumentId == documentId)))
            .OrderBy(e => e.CreationTime).ThenBy(e => e.SequenceNo).ToList();
}
