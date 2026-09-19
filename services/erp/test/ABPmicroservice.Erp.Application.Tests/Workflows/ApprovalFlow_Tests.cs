using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Security.Claims;
using Xunit;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>
/// The whole approval round trip through the application services, with two signed-in users:
/// the test principal ("admin", who creates and sends documents) and their approver, "boss".
/// </summary>
public class ApprovalFlow_Tests : ErpApplicationTestBase
{
    // The id FakeCurrentPrincipalAccessor signs the tests in with.
    private static readonly Guid Sender = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    private static readonly Guid Boss = Guid.NewGuid();

    private readonly ISalesDocumentAppService _documents;
    private readonly IWorkflowAppService _workflows;
    private readonly IApprovalEntryAppService _approvals;
    private readonly IApprovalUserSetupAppService _userSetups;
    private readonly ICustomerAppService _customers;

    public ApprovalFlow_Tests()
    {
        _documents = GetRequiredService<ISalesDocumentAppService>();
        _workflows = GetRequiredService<IWorkflowAppService>();
        _approvals = GetRequiredService<IApprovalEntryAppService>();
        _userSetups = GetRequiredService<IApprovalUserSetupAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
    }

    [Fact]
    public async Task Without_An_Enabled_Workflow_Documents_Release_Directly()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            // The seeded templates exist but are disabled.
            (await _workflows.GetListAsync()).Items.ShouldAllBe(w => !w.Enabled);

            var document = await NewInvoiceAsync(5000m);
            (await _documents.ReleaseAsync(document.Id)).Status.ShouldBe(DocumentStatus.Released);
        });
    }

    [Fact]
    public async Task With_A_Workflow_A_Document_Is_Released_Only_By_Its_Approver()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpApprovalsAsync(senderLimit: 1000m);
            var document = await NewInvoiceAsync(5000m);

            // Neither releasing nor posting gets round the workflow.
            (await Should.ThrowAsync<BusinessException>(() => _documents.ReleaseAsync(document.Id))).Code
                .ShouldBe(ErpErrorCodes.Approvals.ApprovalRequired);
            (await Should.ThrowAsync<BusinessException>(() => _documents.RunPostingAsync(document.Id))).Code
                .ShouldBe(ErpErrorCodes.Approvals.ApprovalRequired);

            var sent = await _documents.SendApprovalRequestAsync(document.Id);
            sent.AutoApproved.ShouldBeFalse();
            sent.FirstApproverUserName.ShouldBe("boss");
            (await _documents.GetAsync(document.Id)).Status.ShouldBe(DocumentStatus.PendingApproval);

            // While pending, the document is frozen and cannot be reopened around the request.
            await Should.ThrowAsync<DocumentNotOpenException>(() => _documents.UpdateAsync(document.Id, ToInput(document)));
            (await Should.ThrowAsync<BusinessException>(() => _documents.ReopenAsync(document.Id))).Code
                .ShouldBe(ErpErrorCodes.Approvals.PendingApproval);

            // The sender sees the request but cannot act on it.
            var mine = await _approvals.GetListAsync(new GetApprovalEntriesInput { SentByMe = true });
            mine.Items.Single().CanAct.ShouldBeFalse();
            (await _approvals.GetMyOpenCountAsync()).ShouldBe(0);
            (await Should.ThrowAsync<BusinessException>(() => _approvals.ApproveAsync(mine.Items[0].Id, new ApprovalCommentInput()))).Code
                .ShouldBe(ErpErrorCodes.Approvals.NotTheApprover);

            using (SignInAs(Boss, "boss"))
            {
                (await _approvals.GetMyOpenCountAsync()).ShouldBe(1);
                var toApprove = (await _approvals.GetListAsync(new GetApprovalEntriesInput { OnlyMine = true })).Items.Single();
                toApprove.CanAct.ShouldBeTrue();
                toApprove.DocumentNo.ShouldBe(document.No);
                toApprove.DueDate.ShouldNotBeNull();

                await _approvals.ApproveAsync(toApprove.Id, new ApprovalCommentInput { Comment = "Fine by me." });
                (await _approvals.GetMyOpenCountAsync()).ShouldBe(0);
            }

            (await _documents.GetAsync(document.Id)).Status.ShouldBe(DocumentStatus.Released);

            var history = await _approvals.GetListAsync(new GetApprovalEntriesInput { DocumentId = document.Id });
            history.Items.Single().Status.ShouldBe(ApprovalStatus.Approved);
            history.Items.Single().Comment.ShouldBe("Fine by me.");
        });
    }

    [Fact]
    public async Task A_Rejection_Returns_The_Document_To_The_Sender_With_The_Reason()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpApprovalsAsync(senderLimit: 1000m);
            var document = await NewInvoiceAsync(5000m);
            await _documents.SendApprovalRequestAsync(document.Id);

            using (SignInAs(Boss, "boss"))
            {
                var entry = (await _approvals.GetListAsync(new GetApprovalEntriesInput { OnlyMine = true })).Items.Single();
                await _approvals.RejectAsync(entry.Id, new ApprovalCommentInput { Comment = "Discount too high." });
            }

            (await _documents.GetAsync(document.Id)).Status.ShouldBe(DocumentStatus.Open);
            var history = await _approvals.GetListAsync(new GetApprovalEntriesInput { DocumentId = document.Id });
            history.Items.Single().Comment.ShouldBe("Discount too high.");

            // Open again, so it can be corrected and sent back.
            await _documents.UpdateAsync(document.Id, ToInput(document));
            await _documents.SendApprovalRequestAsync(document.Id);
        });
    }

    [Fact]
    public async Task The_Sender_Can_Withdraw_A_Request()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpApprovalsAsync(senderLimit: 1000m);
            var document = await NewInvoiceAsync(5000m);
            await _documents.SendApprovalRequestAsync(document.Id);

            await _documents.CancelApprovalRequestAsync(document.Id);

            (await _documents.GetAsync(document.Id)).Status.ShouldBe(DocumentStatus.Open);
            using (SignInAs(Boss, "boss"))
            {
                (await _approvals.GetMyOpenCountAsync()).ShouldBe(0);
            }
        });
    }

    [Fact]
    public async Task An_Amount_Within_The_Senders_Own_Limit_Is_Released_At_Once()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpApprovalsAsync(senderLimit: 10000m);
            var document = await NewInvoiceAsync(5000m);

            (await _documents.SendApprovalRequestAsync(document.Id)).AutoApproved.ShouldBeTrue();
            (await _documents.GetAsync(document.Id)).Status.ShouldBe(DocumentStatus.Released);
        });
    }

    [Fact]
    public async Task An_Enabled_Workflow_Is_Read_Only_And_An_Approver_Must_Be_Set_Up_First()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpApprovalsAsync(senderLimit: 1000m);
            var workflow = (await _workflows.GetListAsync()).Items.Single(w => w.Code == "SIAPW");
            workflow.Steps.Count.ShouldBe(6);

            var change = new CreateUpdateWorkflowDto
            {
                Code = workflow.Code,
                Description = workflow.Description,
                DocumentKind = workflow.DocumentKind,
                MinimumAmount = 2500m,
                ApproverLimitType = ApproverLimitType.FirstQualifiedApprover,
            };
            await Should.ThrowAsync<UserFriendlyException>(() => _workflows.UpdateAsync(workflow.Id, change));

            await _workflows.DisableAsync(workflow.Id);
            var updated = await _workflows.UpdateAsync(workflow.Id, change);
            updated.MinimumAmount.ShouldBe(2500m);
            updated.Steps.First().ConditionRule.ShouldBe("Amount >= 2500");

            // Naming an approver who has no setup row of their own would break the chain later.
            var orphan = new CreateUpdateApprovalUserSetupDto { UserId = Guid.NewGuid(), UserName = "newhire", ApproverUserId = Guid.NewGuid() };
            (await Should.ThrowAsync<BusinessException>(() => _userSetups.CreateAsync(orphan))).Code
                .ShouldBe(ErpErrorCodes.Approvals.UserNotInApprovalSetup);
        });
    }

    private async Task SetUpApprovalsAsync(decimal senderLimit)
    {
        await _userSetups.CreateAsync(new CreateUpdateApprovalUserSetupDto { UserId = Boss, UserName = "boss", UnlimitedSalesApproval = true });
        var sender = await _userSetups.CreateAsync(new CreateUpdateApprovalUserSetupDto
        {
            UserId = Sender,
            UserName = "admin",
            ApproverUserId = Boss,
            SalesAmountApprovalLimit = senderLimit,
        });
        sender.ApproverUserName.ShouldBe("boss");

        var workflow = (await _workflows.GetListAsync()).Items.Single(w => w.Code == "SIAPW");
        await _workflows.EnableAsync(workflow.Id);
    }

    private async Task<SalesHeaderDto> NewInvoiceAsync(decimal amount)
    {
        var customer = await _customers.GetByNoAsync("C00010");

        return await _documents.CreateAsync(new CreateUpdateSalesHeaderDto
        {
            DocumentType = SalesDocumentType.Invoice,
            CustomerId = customer.Id,
            PostingDate = new DateTime(2026, 3, 1),
            Lines = new List<SalesLineInputDto>
            {
                new() { Type = DocumentLineType.GLAccount, No = "4000", Description = "Consulting", Quantity = 1, UnitPrice = amount },
            },
        });
    }

    private static CreateUpdateSalesHeaderDto ToInput(SalesHeaderDto document)
    {
        return new CreateUpdateSalesHeaderDto
        {
            DocumentType = document.DocumentType,
            No = document.No,
            CustomerId = document.CustomerId,
            PostingDate = document.PostingDate,
            Lines = document.Lines
                .Select(l => new SalesLineInputDto { Type = l.Type, No = l.No, Description = l.Description, Quantity = l.Quantity, UnitPrice = l.UnitPrice })
                .ToList(),
        };
    }

    private IDisposable SignInAs(Guid userId, string userName)
    {
        return GetRequiredService<ICurrentPrincipalAccessor>().Change(new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(AbpClaimTypes.UserId, userId.ToString()), new Claim(AbpClaimTypes.UserName, userName) },
            "test"
        )));
    }
}
