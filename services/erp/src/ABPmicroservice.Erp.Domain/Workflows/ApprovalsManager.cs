using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Chatter;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Workflows;

public class ApprovalRequestResult
{
    /// <summary>The sender's own limit covered the amount, so the document was released at once.</summary>
    public bool AutoApproved { get; set; }

    public int ApproverCount { get; set; }
    public string FirstApproverUserName { get; set; }
}

/// <summary>
/// Runs approval requests, together with the approval responses of the workflow engine: create the approver chain, send, approve,
/// reject, delegate and cancel, moving the document between Open, Pending Approval and Released.
/// </summary>
public class ApprovalsManager : DomainService
{
    private readonly IRepository<Workflow, Guid> _workflowRepository;
    private readonly IRepository<ApprovalEntry, Guid> _entryRepository;
    private readonly IRepository<ApprovalUserSetup, Guid> _userSetupRepository;
    private readonly IRepository<SalesHeader, Guid> _salesHeaderRepository;
    private readonly IRepository<PurchaseHeader, Guid> _purchaseHeaderRepository;
    private readonly IRepository<PaymentVoucherHeader, Guid> _paymentVoucherRepository;
    private readonly IRepository<ActivityStreamEntry, Guid> _activityRepository;

    public ApprovalsManager(
        IRepository<Workflow, Guid> workflowRepository,
        IRepository<ApprovalEntry, Guid> entryRepository,
        IRepository<ApprovalUserSetup, Guid> userSetupRepository,
        IRepository<SalesHeader, Guid> salesHeaderRepository,
        IRepository<PurchaseHeader, Guid> purchaseHeaderRepository,
        IRepository<PaymentVoucherHeader, Guid> paymentVoucherRepository,
        IRepository<ActivityStreamEntry, Guid> activityRepository
    )
    {
        _workflowRepository = workflowRepository;
        _entryRepository = entryRepository;
        _userSetupRepository = userSetupRepository;
        _salesHeaderRepository = salesHeaderRepository;
        _purchaseHeaderRepository = purchaseHeaderRepository;
        _paymentVoucherRepository = paymentVoucherRepository;
        _activityRepository = activityRepository;
    }

    /// <summary>
    /// The enabled workflow that applies to a document of this kind and amount, or null.
    /// With several, the one with the highest threshold still met wins.
    /// </summary>
    public async Task<Workflow> FindWorkflowAsync(ApprovalDocumentKind kind, decimal amount, IApprovalDocument document = null)
    {
        var workflows = await _workflowRepository.GetListAsync(w => w.Enabled && w.DocumentKind == kind, includeDetails: true);

        var matching = workflows.Where(w => w.MinimumAmount <= amount);

        if (document != null)
        {
            matching = matching.Where(w => MatchesConditions(w, document));
        }

        return matching.OrderByDescending(w => w.MinimumAmount).FirstOrDefault();
    }

    /// <summary>
    /// Guards a manual release: "This document can only be released when the approval process is complete."
    /// </summary>
    public async Task EnsureCanReleaseAsync(ApprovalDocumentKind kind, IApprovalDocument document)
    {
        if (document.Status == DocumentStatus.PendingApproval)
        {
            throw new BusinessException(ErpErrorCodes.Approvals.PendingApproval).WithData("documentNo", document.No);
        }

        if (document.Status == DocumentStatus.Open && await FindWorkflowAsync(kind, document.ApprovalAmount, document) != null)
        {
            throw new BusinessException(ErpErrorCodes.Approvals.ApprovalRequired).WithData("documentNo", document.No);
        }
    }

    public async Task<ApprovalRequestResult> SendApprovalRequestAsync(ApprovalDocumentKind kind, Guid documentId, Guid senderUserId)
    {
        var document = await GetDocumentAsync(kind, documentId);

        if (document.Status == DocumentStatus.PendingApproval)
        {
            throw new BusinessException(ErpErrorCodes.Approvals.ApprovalAlreadyRequested).WithData("documentNo", document.No);
        }

        if (document.Status != DocumentStatus.Open)
        {
            throw new DocumentNotOpenException(document.No);
        }

        if (!document.HasLines)
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentHasNoLines).WithData("documentNo", document.No);
        }

        var amount = document.ApprovalAmount;
        var workflow = await FindWorkflowAsync(kind, amount, document);
        if (workflow == null)
        {
            throw new BusinessException(ErpErrorCodes.Approvals.NoWorkflowApplies).WithData("documentNo", document.No);
        }

        var setups = (await _userSetupRepository.GetListAsync()).ToDictionary(s => s.UserId);
        if (!setups.TryGetValue(senderUserId, out var sender))
        {
            throw new BusinessException(ErpErrorCodes.Approvals.UserNotInApprovalSetup).WithData("user", senderUserId);
        }

        var approvers = BuildApproverChain(workflow.ApproverLimitType, kind, amount, sender, setups);
        var now = Clock.Now;
        var dueDate = workflow.DueDays > 0 ? now.Date.AddDays(workflow.DueDays) : (DateTime?)null;

        var entries = approvers
            .Select((approver, index) => new ApprovalEntry(
                GuidGenerator.Create(), kind, document.Id, document.No, workflow.Code, index + 1,
                sender.UserId, sender.UserName, approver.UserId, approver.UserName, amount, dueDate))
            .ToList();

        document.SendForApproval();

        // The sender heads the chain only when their own limit covers the amount: nothing to wait for.
        var autoApproved = approvers.Count == 1 && approvers[0].UserId == sender.UserId;
        if (autoApproved)
        {
            entries[0].Approve(now, "Approved automatically: within the sender's own approval limit.");
            document.Release();
        }
        else
        {
            entries[0].Open(now);
        }

        await _entryRepository.InsertManyAsync(entries);
        await UpdateDocumentAsync(kind, document);
        await LogAsync(kind, document, autoApproved
            ? $"{sender.UserName} sent the document for approval; it was within their own limit and was released."
            : $"{sender.UserName} sent the document for approval to {approvers[0].UserName}.");

        return new ApprovalRequestResult
        {
            AutoApproved = autoApproved,
            ApproverCount = approvers.Count,
            FirstApproverUserName = approvers[0].UserName,
        };
    }

    public async Task ApproveAsync(Guid entryId, Guid userId, string comment = null)
    {
        var entry = await GetOpenEntryForAsync(entryId, userId);
        var now = Clock.Now;
        entry.Approve(now, comment);
        await _entryRepository.UpdateAsync(entry);

        var waiting = (await GetPendingEntriesAsync(entry.DocumentKind, entry.DocumentId))
            .Where(e => e.Id != entry.Id)
            .OrderBy(e => e.SequenceNo)
            .ToList();

        var document = await GetDocumentAsync(entry.DocumentKind, entry.DocumentId);

        if (waiting.Count > 0)
        {
            waiting[0].Open(now);
            await _entryRepository.UpdateAsync(waiting[0]);
            await LogAsync(entry.DocumentKind, document, $"{entry.ApproverUserName} approved. The request moved on to {waiting[0].ApproverUserName}.");
            return;
        }

        document.Release();
        await UpdateDocumentAsync(entry.DocumentKind, document);
        await LogAsync(entry.DocumentKind, document, $"{entry.ApproverUserName} approved. All approvals are complete and the document was released.");
    }

    public async Task RejectAsync(Guid entryId, Guid userId, string comment = null)
    {
        var entry = await GetOpenEntryForAsync(entryId, userId);
        var now = Clock.Now;

        // One rejection ends the whole request.
        foreach (var pending in await GetPendingEntriesAsync(entry.DocumentKind, entry.DocumentId))
        {
            pending.Reject(now, pending.Id == entry.Id ? comment : null);
            await _entryRepository.UpdateAsync(pending);
        }

        var document = await GetDocumentAsync(entry.DocumentKind, entry.DocumentId);
        document.Reopen();
        await UpdateDocumentAsync(entry.DocumentKind, document);
        await LogAsync(entry.DocumentKind, document,
            $"{entry.ApproverUserName} rejected the approval request{(comment.IsNullOrWhiteSpace() ? "." : $": {comment.Trim()}")} The document was reopened.");
    }

    /// <summary>Passes an open request to the approver's substitute or, failing that, to their own approver.</summary>
    public async Task DelegateAsync(Guid entryId, Guid userId)
    {
        var entry = await GetOpenEntryForAsync(entryId, userId);

        var approverSetup = await _userSetupRepository.FirstOrDefaultAsync(s => s.UserId == entry.ApproverId);
        var targetId = approverSetup?.SubstituteUserId ?? approverSetup?.ApproverUserId;
        var target = targetId.HasValue ? await _userSetupRepository.FirstOrDefaultAsync(s => s.UserId == targetId.Value) : null;
        if (target == null)
        {
            throw new BusinessException(ErpErrorCodes.Approvals.NoSubstituteOrApprover).WithData("user", entry.ApproverUserName);
        }

        var from = entry.ApproverUserName;
        entry.DelegateTo(target.UserId, target.UserName, Clock.Now);
        await _entryRepository.UpdateAsync(entry);

        var document = await GetDocumentAsync(entry.DocumentKind, entry.DocumentId);
        await LogAsync(entry.DocumentKind, document, $"The approval request was delegated from {from} to {target.UserName}.");
    }

    /// <summary>Withdraws a request. Only its sender or an approval administrator may do so.</summary>
    public async Task CancelApprovalRequestAsync(ApprovalDocumentKind kind, Guid documentId, Guid userId)
    {
        var pending = await GetPendingEntriesAsync(kind, documentId);
        var document = await GetDocumentAsync(kind, documentId);

        if (pending.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Approvals.NothingToCancel).WithData("documentNo", document.No);
        }

        if (pending.All(e => e.SenderId != userId) && !await IsAdministratorAsync(userId))
        {
            throw new BusinessException(ErpErrorCodes.Approvals.OnlySenderCanCancel).WithData("documentNo", document.No);
        }

        var now = Clock.Now;
        foreach (var entry in pending)
        {
            entry.Cancel(now);
            await _entryRepository.UpdateAsync(entry);
        }

        document.Reopen();
        await UpdateDocumentAsync(kind, document);
        await LogAsync(kind, document, "The approval request was canceled and the document was reopened.");
    }

    /// <summary>
    /// Builds the list of approvers, in order: the approver chain, the direct approver, or the
    /// first qualified approver, depending on the approver limit type.
    /// </summary>
    private static List<ApprovalUserSetup> BuildApproverChain(
        ApproverLimitType limitType,
        ApprovalDocumentKind kind,
        decimal amount,
        ApprovalUserSetup sender,
        IReadOnlyDictionary<Guid, ApprovalUserSetup> setups
    )
    {
        if (limitType == ApproverLimitType.DirectApprover)
        {
            return new List<ApprovalUserSetup> { NextApprover(sender, setups, kind, amount, qualifiedNeeded: false) };
        }

        // Within their own limit the sender needs nobody else.
        if (sender.CanApprove(kind, amount))
        {
            return new List<ApprovalUserSetup> { sender };
        }

        var chain = new List<ApprovalUserSetup>();
        var visited = new HashSet<Guid> { sender.UserId };
        var current = sender;

        while (true)
        {
            current = NextApprover(current, setups, kind, amount, qualifiedNeeded: true);
            if (!visited.Add(current.UserId))
            {
                throw new BusinessException(ErpErrorCodes.Approvals.ApprovalChainLoop).WithData("user", current.UserName);
            }

            chain.Add(current);
            if (current.CanApprove(kind, amount))
            {
                return limitType == ApproverLimitType.FirstQualifiedApprover
                    ? new List<ApprovalUserSetup> { current }
                    : chain;
            }
        }
    }

    private static ApprovalUserSetup NextApprover(
        ApprovalUserSetup user,
        IReadOnlyDictionary<Guid, ApprovalUserSetup> setups,
        ApprovalDocumentKind kind,
        decimal amount,
        bool qualifiedNeeded
    )
    {
        if (!user.ApproverUserId.HasValue)
        {
            // The top of the chain was reached without anyone able to approve this amount.
            throw new BusinessException(qualifiedNeeded
                    ? ErpErrorCodes.Approvals.NoQualifiedApprover
                    : ErpErrorCodes.Approvals.NoApproverDefined)
                .WithData("user", user.UserName)
                .WithData("amount", amount);
        }

        if (!setups.TryGetValue(user.ApproverUserId.Value, out var approver))
        {
            throw new BusinessException(ErpErrorCodes.Approvals.UserNotInApprovalSetup).WithData("user", user.ApproverUserId.Value);
        }

        return approver;
    }

    private async Task<ApprovalEntry> GetOpenEntryForAsync(Guid entryId, Guid userId)
    {
        var entry = await _entryRepository.GetAsync(entryId);

        if (entry.Status != ApprovalStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Approvals.EntryNotOpen).WithData("status", entry.Status.ToString());
        }

        if (entry.ApproverId != userId && !await IsAdministratorAsync(userId))
        {
            throw new BusinessException(ErpErrorCodes.Approvals.NotTheApprover).WithData("approver", entry.ApproverUserName);
        }

        return entry;
    }

    private async Task<bool> IsAdministratorAsync(Guid userId)
    {
        return await _userSetupRepository.AnyAsync(s => s.UserId == userId && s.IsApprovalAdministrator);
    }

    private async Task<List<ApprovalEntry>> GetPendingEntriesAsync(ApprovalDocumentKind kind, Guid documentId)
    {
        return await _entryRepository.GetListAsync(e =>
            e.DocumentKind == kind
            && e.DocumentId == documentId
            && (e.Status == ApprovalStatus.Created || e.Status == ApprovalStatus.Open)
        );
    }

    private async Task<IApprovalDocument> GetDocumentAsync(ApprovalDocumentKind kind, Guid documentId)
    {
        return kind switch
        {
            ApprovalDocumentKind.SalesDocument => await _salesHeaderRepository.GetAsync(documentId),
            ApprovalDocumentKind.PaymentVoucher => await _paymentVoucherRepository.GetAsync(documentId),
            _ => await _purchaseHeaderRepository.GetAsync(documentId),
        };
    }

    private async Task UpdateDocumentAsync(ApprovalDocumentKind kind, IApprovalDocument document)
    {
        switch (kind)
        {
            case ApprovalDocumentKind.SalesDocument:
                await _salesHeaderRepository.UpdateAsync((SalesHeader)document);
                break;

            case ApprovalDocumentKind.PaymentVoucher:
                await _paymentVoucherRepository.UpdateAsync((PaymentVoucherHeader)document);
                break;

            default:
                await _purchaseHeaderRepository.UpdateAsync((PurchaseHeader)document);
                break;
        }
    }

    // Shown in the document's chatter, so the approval history reads alongside the notes.
    private async Task LogAsync(ApprovalDocumentKind kind, IApprovalDocument document, string description)
    {
        await _activityRepository.InsertAsync(new ActivityStreamEntry(
            GuidGenerator.Create(),
            kind switch
            {
                ApprovalDocumentKind.SalesDocument => nameof(SalesHeader),
                ApprovalDocumentKind.PaymentVoucher => nameof(PaymentVoucherHeader),
                _ => nameof(PurchaseHeader),
            },
            document.Id,
            document.No,
            nameof(IApprovalDocument.Status),
            null,
            document.Status.ToString(),
            description
        ));
    }

    private static bool MatchesConditions(Workflow workflow, IApprovalDocument document)
    {
        if (workflow.Steps == null || workflow.Steps.Count == 0)
        {
            return true;
        }

        var triggerStep = workflow.Steps.FirstOrDefault(s =>
            s.EventName != null && s.EventName.Contains("requested", StringComparison.OrdinalIgnoreCase))
            ?? workflow.Steps.FirstOrDefault();

        if (triggerStep == null || string.IsNullOrWhiteSpace(triggerStep.ConditionRule) || triggerStep.ConditionRule.Equals("Always", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var rules = triggerStep.ConditionRule.Split(new[] { ';', '\n', '|' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var ruleRaw in rules)
        {
            var rule = ruleRaw.Trim();
            if (string.IsNullOrWhiteSpace(rule) || rule.Equals("Always", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!EvaluateConditionRule(rule, document))
            {
                return false;
            }
        }

        return true;
    }

    private static bool EvaluateConditionRule(string rule, IApprovalDocument document)
    {
        string op = null;
        string left = null;
        string right = null;

        var operators = new[] { ">=", "<=", "<>", "!=", ">", "<", "=", " contains " };
        foreach (var candidate in operators)
        {
            var idx = rule.IndexOf(candidate, StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                op = candidate.Trim();
                left = rule.Substring(0, idx).Trim();
                right = rule.Substring(idx + candidate.Length).Trim();
                break;
            }
        }

        if (op == null || left == null || right == null)
        {
            return true;
        }

        left = left.Replace(" ", "").Replace("-", "").ToLowerInvariant();
        right = right.Trim('\'', '"', ' ');

        var purch = document as PurchaseHeader;
        var sales = document as SalesHeader;

        if (left.Contains("amount") && !left.Contains("vat"))
        {
            if (decimal.TryParse(right, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var targetAmount))
            {
                return CompareDecimals(document.ApprovalAmount, op, targetAmount);
            }
            return true;
        }

        if (left.Contains("amountincludingvat") || left.Contains("totalamountincludingvat") || (left.Contains("amount") && left.Contains("vat")))
        {
            var docAmountVat = purch?.TotalAmountIncludingVat ?? sales?.TotalAmountIncludingVat ?? document.ApprovalAmount;
            if (decimal.TryParse(right, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var targetAmount))
            {
                return CompareDecimals(docAmountVat, op, targetAmount);
            }
            return true;
        }

        string docVal = null;
        if (left.Contains("documenttype"))
        {
            docVal = purch != null ? purch.DocumentType.ToString() : sales?.DocumentType.ToString();
        }
        else if (left.Contains("vendor") || left.Contains("buyfromvendorno"))
        {
            docVal = purch?.BuyFromVendorNo ?? purch?.BuyFromVendorName;
        }
        else if (left.Contains("customer") || left.Contains("selltocustomerno"))
        {
            docVal = sales?.SellToCustomerNo ?? sales?.SellToCustomerName;
        }
        else if (left.Contains("currency"))
        {
            docVal = purch?.CurrencyCode ?? sales?.CurrencyCode ?? "";
        }
        else if (left.Contains("paymentterms"))
        {
            docVal = purch?.PaymentTermsCode ?? sales?.PaymentTermsCode ?? "";
        }

        if (docVal != null)
        {
            return CompareStrings(docVal, op, right);
        }

        return true;
    }

    private static bool CompareDecimals(decimal actual, string op, decimal target)
    {
        return op switch
        {
            ">=" => actual >= target,
            "<=" => actual <= target,
            ">" => actual > target,
            "<" => actual < target,
            "=" => actual == target,
            "!=" or "<>" => actual != target,
            _ => true
        };
    }

    private static bool CompareStrings(string actual, string op, string target)
    {
        var eq = string.Equals(actual, target, StringComparison.OrdinalIgnoreCase);
        return op switch
        {
            "=" => eq,
            "!=" or "<>" => !eq,
            "contains" => (actual ?? "").Contains(target, StringComparison.OrdinalIgnoreCase),
            _ => eq
        };
    }
}
