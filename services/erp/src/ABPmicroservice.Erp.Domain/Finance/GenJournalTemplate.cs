using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// General Journal Template. Mirrors Business Central table 80 "Gen. Journal Template".
/// <para>
/// A template is the kind of journal (general, cash receipts, payments, …). Its batches are the
/// individual worksheets people post from. Recurring templates get the extra recurring columns
/// on their lines, exactly as the BC recurring general journal does.
/// </para>
/// </summary>
public class GenJournalTemplate : CompanyAggregateRoot
{
    /// <summary>Business key, e.g. "GENERAL" or "CASHRECPT". Mirrors BC field "Name".</summary>
    public string Name { get; private set; }

    public string Description { get; private set; }

    public GenJournalTemplateType Type { get; private set; }

    /// <summary>Lines of batches under a recurring template carry a method and a frequency.</summary>
    public bool Recurring { get; private set; }

    /// <summary>Stamped on every entry posted from this template. Mirrors BC "Source Code".</summary>
    public string SourceCode { get; private set; }

    /// <summary>Default number series for the document numbers of new lines.</summary>
    public string NoSeriesCode { get; private set; }

    protected GenJournalTemplate() { }

    public GenJournalTemplate(
        Guid id,
        string name,
        string description,
        GenJournalTemplateType type = GenJournalTemplateType.General,
        bool recurring = false,
        string sourceCode = null,
        string noSeriesCode = null
    )
        : base(id)
    {
        Name = NormalizeName(name);
        Update(description, type, recurring, sourceCode, noSeriesCode);
    }

    public void Update(
        string description,
        GenJournalTemplateType type,
        bool recurring,
        string sourceCode,
        string noSeriesCode
    )
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Type = type;
        Recurring = recurring;
        SourceCode = Check.Length(sourceCode, nameof(sourceCode), ErpDomainConsts.MaxSourceCodeLength)?.ToUpperInvariant();
        NoSeriesCode = Check.Length(noSeriesCode, nameof(noSeriesCode), ErpDomainConsts.MaxNoSeriesCodeLength)?.ToUpperInvariant();
    }

    /// <summary>Template and batch names are codes in Business Central, so they are upper-cased.</summary>
    internal static string NormalizeName(string name)
    {
        return Check
            .NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxJournalTemplateNameLength)
            .Trim()
            .ToUpperInvariant();
    }
}
