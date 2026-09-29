using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// A line of the configuration worksheet. Mirrors Business Central table 8622 "Config. Line": the
/// implementation checklist of a new company, arranged as areas, groups and tables, each table
/// with who is responsible for it, how far it has got and which package fills it.
/// </summary>
public class ConfigLine : CompanyAggregateRoot
{
    public ConfigLineType LineType { get; private set; }

    public string Name { get; private set; }

    /// <summary>For a table line, the table as the entity registry names it.</summary>
    public string EntityName { get; private set; }

    /// <summary>The package that fills the table, when there is one.</summary>
    public string PackageCode { get; private set; }

    public ConfigLineStatus Status { get; private set; }

    public string ResponsibleUserName { get; private set; }

    public string Comments { get; private set; }

    /// <summary>Position on the worksheet. BC's "Vertical Sorting".</summary>
    public int SortOrder { get; private set; }

    protected ConfigLine() { }

    public ConfigLine(Guid id, ConfigLineType lineType, string name, string entityName, int sortOrder)
        : base(id)
    {
        LineType = lineType;
        SortOrder = sortOrder;
        EntityName = lineType == ConfigLineType.Table
            ? Check.NotNullOrWhiteSpace(entityName, nameof(entityName), ErpDomainConsts.MaxEntityNameLength)
            : null;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        Status = ConfigLineStatus.NotStarted;
    }

    public void Update(string name, string packageCode, ConfigLineStatus status, string responsibleUserName, string comments)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        PackageCode = packageCode.IsNullOrWhiteSpace()
            ? null
            : Check.Length(packageCode.Trim().ToUpperInvariant(), nameof(packageCode), ErpDomainConsts.MaxPackageCodeLength);
        Status = status;
        ResponsibleUserName = Check.Length(responsibleUserName, nameof(responsibleUserName), ErpDomainConsts.MaxUserNameLength);
        Comments = Check.Length(comments, nameof(comments), ErpDomainConsts.MaxCommentLength);
    }

    public void MoveTo(int sortOrder) => SortOrder = sortOrder;
}
