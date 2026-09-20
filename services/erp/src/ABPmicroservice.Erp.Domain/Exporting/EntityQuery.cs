using System.Collections.Generic;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>One condition on a field. Several are combined with AND, as in BC's field filters.</summary>
public class EntityFilter
{
    public string Field { get; set; }

    public EntityFilterOperator Operator { get; set; }

    /// <summary>Always carried as text and converted to the field's type when the filter is built.</summary>
    public string Value { get; set; }
}

/// <summary>What to read from one table.</summary>
public class EntityQueryRequest
{
    public string EntityName { get; set; }

    /// <summary>Columns to return. Empty means the table's default columns.</summary>
    public List<string> Fields { get; set; } = new();

    public List<EntityFilter> Filters { get; set; } = new();

    public string OrderBy { get; set; }

    public bool Descending { get; set; }

    public int SkipCount { get; set; }

    public int MaxResultCount { get; set; } = 1000;
}

/// <summary>Rows as plain name/value pairs, so one shape serves JSON, CSV and Excel alike.</summary>
public class EntityQueryResult
{
    public long TotalCount { get; set; }

    public List<ErpEntityField> Fields { get; set; } = new();

    public List<Dictionary<string, object>> Items { get; set; } = new();
}
