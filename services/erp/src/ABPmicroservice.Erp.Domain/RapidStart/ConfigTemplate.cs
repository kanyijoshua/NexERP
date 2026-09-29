using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// Configuration template. Mirrors Business Central tables 8618/8619 "Config. Template Header" and
/// "Config. Template Line": default values for a table's fields, filled into every new record a
/// package creates when the record leaves them blank. A customer template, for instance, gives
/// every imported customer the right posting group without each row having to say so.
/// </summary>
public class ConfigTemplate : CompanyAggregateRoot
{
    public string Code { get; private set; }

    public string Description { get; private set; }

    /// <summary>The table the template fills in, as the entity registry names it.</summary>
    public string EntityName { get; private set; }

    /// <summary>A disabled template is kept but not applied.</summary>
    public bool Enabled { get; private set; }

    public Collection<ConfigTemplateLine> Lines { get; private set; }

    protected ConfigTemplate()
    {
        Lines = new Collection<ConfigTemplateLine>();
    }

    public ConfigTemplate(Guid id, string code, string entityName, string description = null)
        : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        EntityName = Check.NotNullOrWhiteSpace(entityName, nameof(entityName), ErpDomainConsts.MaxEntityNameLength);
        Lines = new Collection<ConfigTemplateLine>();
        Update(description, enabled: true);
    }

    public void Update(string description, bool enabled)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Enabled = enabled;
    }

    /// <summary>Replaces the lines. One line per field: a second line for the same field replaces the first.</summary>
    public void SetLines(IEnumerable<(string FieldName, string DefaultValue, bool Mandatory)> lines, Func<Guid> newId)
    {
        Lines.Clear();

        foreach (var line in (lines ?? []).Where(l => !l.FieldName.IsNullOrWhiteSpace()).GroupBy(l => l.FieldName.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            var last = line.Last();
            Lines.Add(new ConfigTemplateLine(newId(), Id, line.Key, last.DefaultValue, last.Mandatory));
        }
    }
}

public class ConfigTemplateLine : Entity<Guid>
{
    public Guid ConfigTemplateId { get; private set; }

    public string FieldName { get; private set; }

    public string DefaultValue { get; private set; }

    /// <summary>A record that still has no value for this field once the default is applied is refused.</summary>
    public bool Mandatory { get; private set; }

    protected ConfigTemplateLine() { }

    internal ConfigTemplateLine(Guid id, Guid configTemplateId, string fieldName, string defaultValue, bool mandatory)
        : base(id)
    {
        ConfigTemplateId = configTemplateId;
        FieldName = Check.NotNullOrWhiteSpace(fieldName, nameof(fieldName), ErpDomainConsts.MaxEntityNameLength);
        DefaultValue = Check.Length(defaultValue, nameof(defaultValue), ErpDomainConsts.MaxConfigValueLength);
        Mandatory = mandatory;
    }
}
