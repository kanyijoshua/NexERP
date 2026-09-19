using System;

namespace ABPmicroservice.Erp.Documents;

/// <summary>
/// What the approval engine needs from a document. Business Central reaches documents through
/// RecordRef in codeunit 1535; here the sales and purchase headers implement this instead.
/// </summary>
public interface IApprovalDocument
{
    Guid Id { get; }
    string No { get; }
    DocumentStatus Status { get; }
    bool HasLines { get; }

    /// <summary>The amount compared with approval limits and workflow thresholds (excluding VAT).</summary>
    decimal ApprovalAmount { get; }

    /// <summary>Open -> Pending Approval.</summary>
    void SendForApproval();

    void Release();

    void Reopen();
}
