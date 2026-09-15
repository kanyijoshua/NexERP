using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Account Schedule Header. Mirrors Business Central Table 84 "Acc. Schedule Name".
/// </summary>
public class AccountSchedule : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Collection<AccountScheduleLine> Lines { get; private set; }

    protected AccountSchedule()
    {
        Lines = new Collection<AccountScheduleLine>();
    }

    public AccountSchedule(Guid id, string name, string description = null)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Lines = new Collection<AccountScheduleLine>();
    }
}

/// <summary>
/// Account Schedule Line. Mirrors Business Central Table 85 "Acc. Schedule Line".
/// </summary>
public class AccountScheduleLine : FullAuditedEntity<Guid>
{
    public Guid AccountScheduleId { get; private set; }
    public int LineNo { get; private set; }
    public string RowNo { get; private set; }
    public string Description { get; private set; }
    public string TotalingType { get; private set; } // "Posting Accounts", "Total Accounts", "Formula"
    public string Totaling { get; private set; } // e.g. "1000..1999" or "ROW10+ROW20"

    protected AccountScheduleLine() { }

    public AccountScheduleLine(
        Guid id,
        Guid accountScheduleId,
        int lineNo,
        string rowNo,
        string description,
        string totalingType,
        string totaling
    )
        : base(id)
    {
        AccountScheduleId = accountScheduleId;
        LineNo = lineNo;
        RowNo = Check.NotNullOrWhiteSpace(rowNo, nameof(rowNo), 10);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        TotalingType = Check.NotNullOrWhiteSpace(totalingType, nameof(totalingType));
        Totaling = Check.NotNullOrWhiteSpace(totaling, nameof(totaling));
    }
}
