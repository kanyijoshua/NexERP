using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Sequences;

/// <summary>
/// Counter behind the "Entry No." of a ledger and behind register numbers.
/// One row per company and sequence name.
/// <para>
/// Business Central gets these numbers from the platform's AutoIncrement fields. There is no
/// portable equivalent here, so the counter is a row and is advanced by a single atomic
/// UPDATE ... RETURNING statement (see <c>EntryNoGenerator</c>).
/// </para>
/// </summary>
public class ErpNumberSequence : CompanyEntity
{
    public string Name { get; private set; }

    /// <summary>Highest number handed out so far; the next one is this plus one.</summary>
    public long LastValue { get; private set; }

    protected ErpNumberSequence() { }

    public ErpNumberSequence(Guid id, string name, long lastValue = 0)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxCodeLength);
        LastValue = lastValue;
    }
}

/// <summary>Names of the counters. One per ledger, as each has its own numbering in BC.</summary>
public static class ErpSequenceNames
{
    public const string GLEntry = "GLENTRY";
    public const string CustomerLedgerEntry = "CUSTLEDG";
    public const string VendorLedgerEntry = "VENDLEDG";
    public const string ItemLedgerEntry = "ITEMLEDG";
    public const string ValueEntry = "VALUEENTRY";
    public const string GLRegister = "GLREGISTER";

    /// <summary>
    /// Groups the entries written by one posting run. Mirrors BC's "Transaction No.",
    /// which is what a reversal reverses.
    /// </summary>
    public const string TransactionNo = "TRANSACTION";
}

/// <summary>
/// Hands out consecutive entry numbers. Implemented in the EF Core layer because it has to be
/// atomic across concurrent postings, which needs one database statement rather than a read
/// followed by a write.
/// </summary>
public interface IEntryNoGenerator
{
    /// <summary>
    /// Reserves <paramref name="count"/> consecutive numbers and returns the first of them.
    /// </summary>
    Task<long> NextAsync(string sequenceName, int count = 1);
}
