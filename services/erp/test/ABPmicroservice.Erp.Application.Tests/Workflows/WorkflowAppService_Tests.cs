using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Workflows;

public class WorkflowAppService_Tests : ErpApplicationTestBase
{
    private readonly IWorkflowAppService _workflows;

    public WorkflowAppService_Tests()
    {
        _workflows = GetRequiredService<IWorkflowAppService>();
    }

    [Fact]
    public async Task A_Workflow_Is_Created_From_A_Template_And_Copied()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            (await _workflows.GetTemplatesAsync()).Items.ShouldContain(t => t.Code == "VOUCHER-APPROVAL");

            var created = await _workflows.NewFromTemplateAsync(new NewWorkflowFromTemplateDto { TemplateCode = "VOUCHER-APPROVAL", Code = "pv-apw" });
            created.Code.ShouldBe("PV-APW");
            created.Enabled.ShouldBeFalse();
            created.DocumentKind.ShouldBe(ApprovalDocumentKind.PaymentVoucher);
            created.DelegateAfterDays.ShouldBe(2);
            created.Steps.Count.ShouldBe(7);

            var copy = await _workflows.CopyAsync(created.Id, new CopyWorkflowDto { Code = "PV-APW-2", Description = "Second voucher workflow" });
            copy.Description.ShouldBe("Second voucher workflow");
            copy.Steps.Select(s => s.EventName).ShouldBe(created.Steps.Select(s => s.EventName));

            (await Should.ThrowAsync<BusinessException>(
                () => _workflows.CopyAsync(created.Id, new CopyWorkflowDto { Code = "PV-APW" }))).Code
                .ShouldBe(ErpErrorCodes.Approvals.WorkflowCodeAlreadyExists);
        });
    }

    [Fact]
    public async Task Group_And_Specific_Approvers_Must_Exist()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            (await Should.ThrowAsync<BusinessException>(() => _workflows.NewFromTemplateAsync(
                new NewWorkflowFromTemplateDto { TemplateCode = "TENDER-APPROVAL", Code = "T1", WorkflowUserGroupCode = "BOARD" }))).Code
                .ShouldBe(ErpErrorCodes.Approvals.WorkflowUserGroupNotFound);

            await GetRequiredService<IRepository<WorkflowUserGroup, Guid>>().InsertAsync(new WorkflowUserGroup(Guid.NewGuid(), "BOARD"), autoSave: true);
            var tender = await _workflows.NewFromTemplateAsync(
                new NewWorkflowFromTemplateDto { TemplateCode = "TENDER-APPROVAL", Code = "T1", WorkflowUserGroupCode = "board" });
            tender.ApproverType.ShouldBe(WorkflowApproverType.WorkflowUserGroup);
            tender.WorkflowUserGroupCode.ShouldBe("BOARD");

            (await Should.ThrowAsync<BusinessException>(() => _workflows.CreateAsync(new CreateUpdateWorkflowDto
            {
                Code = "SPC",
                DocumentKind = ApprovalDocumentKind.PurchaseDocument,
                ApproverLimitType = ApproverLimitType.SpecificApprover,
                SpecificApproverUserId = Guid.NewGuid(),
            }))).Code.ShouldBe(ErpErrorCodes.Approvals.UserNotInApprovalSetup);
        });
    }
}
