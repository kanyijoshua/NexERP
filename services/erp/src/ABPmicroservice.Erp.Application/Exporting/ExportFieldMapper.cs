using System;
using System.Collections.Generic;
using System.Linq;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// Shared translation between the domain's entity registry and what a client is shown, used by
/// both the export dialog and the integration API so the two describe a table identically.
/// </summary>
internal static class ExportFieldMapper
{
    public static ExportableFieldDto ToDto(ErpEntityField field, ErpEntityDefinition definition)
    {
        var defaults = definition.DefaultFields;

        return new ExportableFieldDto
        {
            Name = field.Name,
            DisplayName = field.DisplayName,
            DataType = DataTypeOf(field.ClrType),
            EnumValues = field.ClrType.IsEnum ? Enum.GetNames(field.ClrType).ToList() : null,
            IncludedByDefault = defaults.Any(f => f.Name == field.Name),
        };
    }

    public static List<ExportableFieldDto> ToDtos(ErpEntityDefinition definition)
    {
        return definition.Fields.Select(f => ToDto(f, definition)).ToList();
    }

    /// <summary>
    /// A coarse type is enough for the client: it decides which filter operators and which input
    /// control to offer, and nothing else.
    /// </summary>
    private static string DataTypeOf(Type clrType)
    {
        if (clrType.IsEnum)
        {
            return "enum";
        }

        if (clrType == typeof(string))
        {
            return "string";
        }

        if (clrType == typeof(bool))
        {
            return "boolean";
        }

        if (clrType == typeof(Guid))
        {
            return "guid";
        }

        if (clrType == typeof(DateTime) || clrType == typeof(DateTimeOffset))
        {
            return "date";
        }

        return "number";
    }

    /// <summary>
    /// The registry lists more tables than a given caller may see. Anything without a permission
    /// entry is not exportable at all, which keeps a new table private until it is given one.
    /// </summary>
    public static string PermissionOf(ErpEntityDefinition definition)
    {
        return ErpEntityPermissions.Find(definition.Name);
    }

    public static ExportableEntityDto ToDto(ErpEntityDefinition definition)
    {
        return new ExportableEntityDto
        {
            Name = definition.Name,
            DisplayName = ErpEntityField.Humanize(definition.Name),
            FieldCount = definition.Fields.Count,
            IsCompanyScoped = definition.IsCompanyScoped,
        };
    }

    public static List<EntityFilter> ToFilters(IEnumerable<EntityFilterDto> filters)
    {
        return (filters ?? Enumerable.Empty<EntityFilterDto>())
            .Where(f => !f.Field.IsNullOrWhiteSpace())
            .Select(f => new EntityFilter
            {
                Field = f.Field,
                Operator = f.Operator,
                Value = f.Value,
            })
            .ToList();
    }
}
