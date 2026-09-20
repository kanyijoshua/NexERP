using System;
using System.Collections.ObjectModel;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Column Layout. Mirrors Business Central Table 333 "Column Layout Name".
/// It is the "across" of a financial report: this period, last period, year to date, and so on.
/// </summary>
public class ColumnLayout : CompanyAggregateRoot
{
    public string Name { get; private set; }

    public string Description { get; private set; }

    public Collection<ColumnLayoutLine> Lines { get; private set; }

    protected ColumnLayout()
    {
        Lines = new Collection<ColumnLayoutLine>();
    }

    public ColumnLayout(Guid id, string name, string description = null)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        SetDescription(description);
        Lines = new Collection<ColumnLayoutLine>();
    }

    public void SetDescription(string description)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }
}

/// <summary>
/// Column Layout line. Mirrors Business Central Table 334 "Column Layout".
/// </summary>
public class ColumnLayoutLine : FullAuditedEntity<Guid>
{
    public Guid ColumnLayoutId { get; private set; }

    public int LineNo { get; private set; }

    /// <summary>Short handle of the column, e.g. "C10".</summary>
    public string ColumnNo { get; private set; }

    public string ColumnHeader { get; private set; }

    public ColumnLayoutType ColumnType { get; private set; }

    /// <summary>
    /// Date formula that shifts this column's period, e.g. "-1Y" for the same period last year.
    /// Mirrors BC "Comparison Date Formula".
    /// </summary>
    public string ComparisonDateFormula { get; private set; }

    public bool ShowOppositeSign { get; private set; }

    protected ColumnLayoutLine() { }

    public ColumnLayoutLine(
        Guid id,
        Guid columnLayoutId,
        int lineNo,
        string columnNo,
        string columnHeader,
        ColumnLayoutType columnType = ColumnLayoutType.NetChange,
        string comparisonDateFormula = null,
        bool showOppositeSign = false
    )
        : base(id)
    {
        ColumnLayoutId = columnLayoutId;
        LineNo = lineNo;
        ColumnNo = Check.NotNullOrWhiteSpace(columnNo, nameof(columnNo), ErpDomainConsts.MaxRowNoLength)
            .Trim()
            .ToUpperInvariant();
        Update(columnHeader, columnType, comparisonDateFormula, showOppositeSign);
    }

    public void Update(
        string columnHeader,
        ColumnLayoutType columnType,
        string comparisonDateFormula,
        bool showOppositeSign
    )
    {
        ColumnHeader = Check.Length(columnHeader, nameof(columnHeader), ErpDomainConsts.MaxColumnHeaderLength);
        ColumnType = columnType;
        ShowOppositeSign = showOppositeSign;

        if (comparisonDateFormula.IsNullOrWhiteSpace())
        {
            ComparisonDateFormula = null;
            return;
        }

        if (!DateFormula.TryParse(comparisonDateFormula, out var parsed))
        {
            throw new BusinessException(ErpErrorCodes.Journals.InvalidDateFormula)
                .WithData("formula", comparisonDateFormula);
        }

        ComparisonDateFormula = parsed.Text;
    }
}
