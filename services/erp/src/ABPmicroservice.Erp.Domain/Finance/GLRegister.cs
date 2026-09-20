using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// G/L Register. Mirrors Business Central table 45 "G/L Register".
/// <para>
/// One row per posting run, holding the range of entry numbers it wrote. It is what the
/// navigate and reverse actions work on: reversing a register cancels every entry in it.
/// </para>
/// </summary>
public class GLRegister : CompanyBasicEntity, IHasCreationTime
{
    /// <summary>Consecutive register number within the company. Mirrors BC "No.".</summary>
    public long No { get; private set; }

    public long FromEntryNo { get; internal set; }

    public long ToEntryNo { get; internal set; }

    /// <summary>First customer ledger entry number of the run; 0 when it wrote none.</summary>
    public long FromCustomerEntryNo { get; internal set; }

    public long ToCustomerEntryNo { get; internal set; }

    public long FromVendorEntryNo { get; internal set; }

    public long ToVendorEntryNo { get; internal set; }

    public DateTime CreationTime { get; private set; }

    /// <summary>Posting date of the run. Mirrors BC "Posting Date" on the register.</summary>
    public DateTime PostingDate { get; private set; }

    public Guid? UserId { get; private set; }

    public string UserName { get; private set; }

    public string SourceCode { get; private set; }

    /// <summary>"TEMPLATE/BATCH" the run came from, or the document number for a document.</summary>
    public string JournalBatchName { get; private set; }

    /// <summary>Transaction number shared by every entry of the run.</summary>
    public long TransactionNo { get; private set; }

    public bool Reversed { get; internal set; }

    /// <summary>Register that reversed this one.</summary>
    public long ReversedByRegisterNo { get; internal set; }

    /// <summary>Set on a reversal register: the register it reversed.</summary>
    public long ReversedRegisterNo { get; internal set; }

    protected GLRegister() { }

    public GLRegister(
        Guid id,
        long no,
        long transactionNo,
        DateTime postingDate,
        Guid? userId,
        string userName,
        string sourceCode,
        string journalBatchName
    )
        : base(id)
    {
        No = no;
        TransactionNo = transactionNo;
        PostingDate = postingDate;
        CreationTime = DateTime.UtcNow;
        UserId = userId;
        UserName = Check.Length(userName, nameof(userName), ErpDomainConsts.MaxUserNameLength);
        SourceCode = Check.Length(sourceCode, nameof(sourceCode), ErpDomainConsts.MaxSourceCodeLength);
        JournalBatchName = Check.Length(journalBatchName, nameof(journalBatchName), ErpDomainConsts.MaxNameLength);
    }

    /// <summary>True when the register wrote no entry at all, which is never worth keeping.</summary>
    public bool IsEmpty => FromEntryNo == 0 && FromCustomerEntryNo == 0 && FromVendorEntryNo == 0;

    /// <summary>
    /// A register can be reversed once, and only if it was not itself a reversal.
    /// Mirrors what BC's "Reverse Transaction" refuses.
    /// </summary>
    public bool IsReversible => !Reversed && ReversedRegisterNo == 0;

    internal void NoteGLEntry(long entryNo)
    {
        if (FromEntryNo == 0)
        {
            FromEntryNo = entryNo;
        }

        ToEntryNo = entryNo;
    }

    internal void NoteCustomerEntry(long entryNo)
    {
        if (FromCustomerEntryNo == 0)
        {
            FromCustomerEntryNo = entryNo;
        }

        ToCustomerEntryNo = entryNo;
    }

    internal void NoteVendorEntry(long entryNo)
    {
        if (FromVendorEntryNo == 0)
        {
            FromVendorEntryNo = entryNo;
        }

        ToVendorEntryNo = entryNo;
    }
}
