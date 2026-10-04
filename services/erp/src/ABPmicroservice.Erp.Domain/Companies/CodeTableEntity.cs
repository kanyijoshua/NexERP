using System;
using Volo.Abp;

namespace ABPmicroservice.Erp.Companies;

/// <summary>
/// A company table keyed by a short code with a description: posting groups, payment terms,
/// causes of absence and the like. Codes are upper case,
/// so a lookup never misses by case.
/// </summary>
public abstract class CodeTableEntity : CompanyEntity
{
    public string Code { get; private set; }
    public string Description { get; private set; }

    protected CodeTableEntity() { }

    protected CodeTableEntity(Guid id, string code, string description)
        : base(id)
    {
        SetCode(code);
        SetDescription(description);
    }

    /// <summary>The length of the Code field, 20 unless the table says otherwise (10 for currencies, locations).</summary>
    protected virtual int MaxCodeLength => ErpDomainConsts.MaxCodeLength;

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code?.Trim(), nameof(code), MaxCodeLength).ToUpperInvariant();
    }

    public void SetDescription(string description)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    /// <summary>A blank code on a record that points at a code table means "none"; anything else is stored upper case.</summary>
    public static string NormalizeCode(string code)
    {
        return code.IsNullOrWhiteSpace() ? null : code.Trim().ToUpperInvariant();
    }
}
