using System;
using System.Collections.Generic;
using System.Linq;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>A ready-made approval workflow a new one can start from.</summary>
public class WorkflowTemplate
{
    public string Code { get; }
    public string Description { get; }
    public ApprovalDocumentKind DocumentKind { get; }
    public ApproverLimitType ApproverLimitType { get; }
    public WorkflowApproverType ApproverType { get; }
    public decimal MinimumAmount { get; }
    public int DueDays { get; }
    public int DelegateAfterDays { get; }

    public WorkflowTemplate(
        string code,
        string description,
        ApprovalDocumentKind documentKind,
        ApproverLimitType approverLimitType,
        WorkflowApproverType approverType = WorkflowApproverType.Approver,
        decimal minimumAmount = 0m,
        int dueDays = 3,
        int delegateAfterDays = 0
    )
    {
        Code = code;
        Description = description;
        DocumentKind = documentKind;
        ApproverLimitType = approverLimitType;
        ApproverType = approverType;
        MinimumAmount = minimumAmount;
        DueDays = dueDays;
        DelegateAfterDays = delegateAfterDays;
    }

    /// <summary>
    /// A disabled workflow built from this template. A group template needs the group to send to;
    /// everything else comes from the template.
    /// </summary>
    public Workflow CreateWorkflow(Guid id, string code, string description, string workflowUserGroupCode, Func<Guid> newId)
    {
        var workflow = new Workflow(id, code, description.IsNullOrWhiteSpace() ? Description : description,
            DocumentKind, MinimumAmount, ApproverLimitType, DueDays);
        workflow.SetApprovers(ApproverType, workflowUserGroupCode, null, null);
        workflow.SetDelegateAfterDays(DelegateAfterDays);
        workflow.RebuildSteps(newId);
        return workflow;
    }
}

/// <summary>The built-in workflow templates, one or more per kind of approval document.</summary>
public static class WorkflowTemplates
{
    public static IReadOnlyList<WorkflowTemplate> All { get; } = new List<WorkflowTemplate>
    {
        new("SALES-APPROVAL", "Sales Document Approval Workflow",
            ApprovalDocumentKind.SalesDocument, ApproverLimitType.ApproverChain),
        new("PURCH-APPROVAL", "Purchase Document Approval Workflow",
            ApprovalDocumentKind.PurchaseDocument, ApproverLimitType.ApproverChain),
        new("PURCH-GROUP-APPROVAL", "Purchase Document Approval by Workflow User Group",
            ApprovalDocumentKind.PurchaseDocument, ApproverLimitType.ApproverChain, WorkflowApproverType.WorkflowUserGroup),
        new("PURCH-LARGE-APPROVAL", "Large Purchase Approval (First Qualified Approver)",
            ApprovalDocumentKind.PurchaseDocument, ApproverLimitType.FirstQualifiedApprover, minimumAmount: 10000m, delegateAfterDays: 2),
        new("VOUCHER-APPROVAL", "Payment Voucher Approval Workflow",
            ApprovalDocumentKind.PaymentVoucher, ApproverLimitType.ApproverChain, delegateAfterDays: 2),
        new("REQUISITION-APPROVAL", "Purchase Requisition Approval Workflow",
            ApprovalDocumentKind.PurchaseRequisition, ApproverLimitType.DirectApprover),
        new("TENDER-APPROVAL", "Tender Approval by Workflow User Group",
            ApprovalDocumentKind.Tender, ApproverLimitType.ApproverChain, WorkflowApproverType.WorkflowUserGroup, dueDays: 5),
    };

    public static WorkflowTemplate Find(string code) =>
        All.FirstOrDefault(t => string.Equals(t.Code, code?.Trim(), StringComparison.OrdinalIgnoreCase));
}
