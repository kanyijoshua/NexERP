using System;
using System.Collections.Generic;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// A saved set of columns for a table, so the same export can be repeated without picking the
/// fields again. Mirrors Odoo's saved export lists, and plays the part Business Central gives to
/// a configuration package's field selection.
/// </summary>
public class ExportTemplate : CompanyAggregateRoot
{
    public string Name { get; private set; }

    /// <summary>Table the template belongs to, as named by the entity registry.</summary>
    public string EntityName { get; private set; }

    /// <summary>Selected field names, in the order they should appear, separated by commas.</summary>
    public string Fields { get; private set; }

    public ExportFormat Format { get; private set; }

    /// <summary>A shared template is offered to everyone in the company, not only its author.</summary>
    public bool IsShared { get; private set; }

    public Guid? OwnerUserId { get; private set; }

    protected ExportTemplate() { }

    public ExportTemplate(
        Guid id,
        string name,
        string entityName,
        IEnumerable<string> fields,
        ExportFormat format = ExportFormat.Xlsx,
        bool isShared = false,
        Guid? ownerUserId = null
    )
        : base(id)
    {
        EntityName = Check.NotNullOrWhiteSpace(entityName, nameof(entityName), ErpDomainConsts.MaxEntityNameLength);
        OwnerUserId = ownerUserId;
        Update(name, fields, format, isShared);
    }

    public void Update(string name, IEnumerable<string> fields, ExportFormat format, bool isShared)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);

        var selected = (fields ?? Enumerable.Empty<string>())
            .Where(f => !f.IsNullOrWhiteSpace())
            .Select(f => f.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (selected.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Exporting.NoFieldsSelected);
        }

        Fields = string.Join(',', selected);
        Format = format;
        IsShared = isShared;
    }

    public IReadOnlyList<string> GetFields()
    {
        return Fields.IsNullOrWhiteSpace()
            ? Array.Empty<string>()
            : Fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>A template is visible to its author, and to everyone once it is shared.</summary>
    public bool IsVisibleTo(Guid? userId) => IsShared || OwnerUserId == null || OwnerUserId == userId;
}
