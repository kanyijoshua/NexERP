using ABPmicroservice.Erp.Companies;
using System;
using System.Collections.ObjectModel;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Account Schedule. Mirrors Business Central Table 84 "Acc. Schedule Name", which BC now shows
/// as a Financial Report. The rows say what to add up; a <see cref="ColumnLayout"/> says over
/// which periods. Together they are how a balance sheet or income statement is defined without
/// writing code.
/// </summary>
public class AccountSchedule : CompanyAggregateRoot
{
    public string Name { get; private set; }

    public string Description { get; private set; }

    /// <summary>Column layout offered first when this schedule is run.</summary>
    public string DefaultColumnLayoutName { get; private set; }

    public Collection<AccountScheduleLine> Lines { get; private set; }

    protected AccountSchedule()
    {
        Lines = new Collection<AccountScheduleLine>();
    }

    public AccountSchedule(Guid id, string name, string description = null, string defaultColumnLayoutName = null)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        Update(description, defaultColumnLayoutName);
        Lines = new Collection<AccountScheduleLine>();
    }

    public void Update(string description, string defaultColumnLayoutName)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        DefaultColumnLayoutName = Check.Length(
            defaultColumnLayoutName,
            nameof(defaultColumnLayoutName),
            ErpDomainConsts.MaxNameLength
        );
    }
}

/// <summary>
/// Account Schedule Line. Mirrors Business Central Table 85 "Acc. Schedule Line".
/// </summary>
public class AccountScheduleLine : FullAuditedEntity<Guid>
{
    public Guid AccountScheduleId { get; private set; }

    public int LineNo { get; private set; }

    /// <summary>Short handle other rows refer to in a formula, e.g. "R10".</summary>
    public string RowNo { get; private set; }

    public string Description { get; private set; }

    public AccountScheduleTotalingType TotalingType { get; private set; }

    /// <summary>
    /// Accounts, e.g. "1000..1999|2100", or a formula over row numbers, e.g. "R10+R20".
    /// Mirrors BC's Totaling field, which carries both depending on the totaling type.
    /// </summary>
    public string Totaling { get; private set; }

    /// <summary>
    /// Flips the sign of the amount. Income and equity accounts carry credit balances, so a
    /// readable income statement shows them positive.
    /// </summary>
    public bool ShowOppositeSign { get; private set; }

    public bool Bold { get; private set; }

    public bool Italic { get; private set; }

    /// <summary>Nesting level of the row in the printed report.</summary>
    public int Indentation { get; private set; }

    /// <summary>Leaves the row out when it computes to zero. Mirrors BC "Hide If Zero".</summary>
    public bool HideIfZero { get; private set; }

    protected AccountScheduleLine() { }

    public AccountScheduleLine(
        Guid id,
        Guid accountScheduleId,
        int lineNo,
        string rowNo,
        string description,
        AccountScheduleTotalingType totalingType,
        string totaling,
        bool showOppositeSign = false,
        bool bold = false,
        bool italic = false,
        int indentation = 0,
        bool hideIfZero = false
    )
        : base(id)
    {
        AccountScheduleId = accountScheduleId;
        LineNo = lineNo;
        RowNo = Check.NotNullOrWhiteSpace(rowNo, nameof(rowNo), ErpDomainConsts.MaxRowNoLength).Trim().ToUpperInvariant();
        Update(description, totalingType, totaling, showOppositeSign, bold, italic, indentation, hideIfZero);
    }

    public void Update(
        string description,
        AccountScheduleTotalingType totalingType,
        string totaling,
        bool showOppositeSign,
        bool bold,
        bool italic,
        int indentation,
        bool hideIfZero
    )
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        TotalingType = totalingType;
        Totaling = Check.Length(totaling, nameof(totaling), ErpDomainConsts.MaxTotalingLength);
        ShowOppositeSign = showOppositeSign;
        Bold = bold;
        Italic = italic;
        Indentation = Math.Max(0, indentation);
        HideIfZero = hideIfZero;

        // A row that adds nothing up needs something to add up.
        if (TotalingType != AccountScheduleTotalingType.Description && Totaling.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Reports.InvalidRowFormula).WithData("rowNo", RowNo);
        }
    }
}
