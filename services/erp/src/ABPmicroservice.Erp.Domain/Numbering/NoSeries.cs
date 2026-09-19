using System;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Numbering;

/// <summary>
/// Number Series. Mirrors Business Central table 308 "No. Series".
/// </summary>
public class NoSeries : CompanyAggregateRoot
{
    public string Code { get; private set; }
    public string Description { get; private set; }

    /// <summary>Numbers are assigned automatically when the user leaves the number blank.</summary>
    public bool DefaultNos { get; private set; }

    /// <summary>The user may type a number instead of taking the next one.</summary>
    public bool ManualNos { get; private set; }

    /// <summary>Numbers must be handed out in date order (required for posted invoices in many countries).</summary>
    public bool DateOrder { get; private set; }

    public Collection<NoSeriesLine> Lines { get; private set; }

    protected NoSeries()
    {
        Lines = new Collection<NoSeriesLine>();
    }

    public NoSeries(
        Guid id,
        string code,
        string description,
        bool defaultNos = true,
        bool manualNos = false,
        bool dateOrder = false
    )
        : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxNoSeriesCodeLength);
        Lines = new Collection<NoSeriesLine>();
        Update(description, defaultNos, manualNos, dateOrder);
    }

    public void Update(string description, bool defaultNos, bool manualNos, bool dateOrder)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        DefaultNos = defaultNos;
        ManualNos = manualNos;
        DateOrder = dateOrder;
    }

    public NoSeriesLine AddLine(
        Guid lineId,
        DateTime? startingDate,
        string startingNo,
        string endingNo = null,
        string warningNo = null,
        int incrementByNo = 1
    )
    {
        var lineNo = Lines.Count == 0 ? 10000 : Lines.Max(l => l.LineNo) + 10000;
        var line = new NoSeriesLine(lineId, Id, lineNo, startingDate, startingNo, endingNo, warningNo, incrementByNo);
        Lines.Add(line);
        return line;
    }

    public void RemoveLine(Guid lineId)
    {
        var line = Lines.FirstOrDefault(l => l.Id == lineId);
        if (line != null)
        {
            Lines.Remove(line);
        }
    }

    /// <summary>
    /// The line in force on a date: the open line with the latest starting date not after it.
    /// Mirrors NoSeriesManagement.SetNoSeriesLineFilter.
    /// </summary>
    public NoSeriesLine FindLine(DateTime usageDate)
    {
        return Lines
            .Where(l => l.Open && (l.StartingDate == null || l.StartingDate.Value.Date <= usageDate.Date))
            .OrderByDescending(l => l.StartingDate ?? DateTime.MinValue)
            .ThenByDescending(l => l.LineNo)
            .FirstOrDefault();
    }
}

/// <summary>
/// Number Series Line. Mirrors Business Central table 309 "No. Series Line".
/// </summary>
public class NoSeriesLine : Entity<Guid>
{
    public Guid NoSeriesId { get; private set; }
    public int LineNo { get; private set; }
    public DateTime? StartingDate { get; private set; }
    public string StartingNo { get; private set; }
    public string EndingNo { get; private set; }
    public string WarningNo { get; private set; }
    public int IncrementByNo { get; private set; }

    /// <summary>
    /// Also the optimistic concurrency token: two transactions that read the same value
    /// cannot both hand out the next number; the second one fails and must retry.
    /// </summary>
    public string LastNoUsed { get; private set; }

    public DateTime? LastDateUsed { get; private set; }

    /// <summary>False once the ending number has been used.</summary>
    public bool Open { get; private set; }

    protected NoSeriesLine() { }

    internal NoSeriesLine(
        Guid id,
        Guid noSeriesId,
        int lineNo,
        DateTime? startingDate,
        string startingNo,
        string endingNo,
        string warningNo,
        int incrementByNo
    )
        : base(id)
    {
        NoSeriesId = noSeriesId;
        LineNo = lineNo;
        Open = true;
        Update(startingDate, startingNo, endingNo, warningNo, incrementByNo);
    }

    public void Update(DateTime? startingDate, string startingNo, string endingNo, string warningNo, int incrementByNo)
    {
        Check.NotNullOrWhiteSpace(startingNo, nameof(startingNo), ErpDomainConsts.MaxDocumentNoLength);
        Check.Length(endingNo, nameof(endingNo), ErpDomainConsts.MaxDocumentNoLength);
        Check.Length(warningNo, nameof(warningNo), ErpDomainConsts.MaxDocumentNoLength);

        if (incrementByNo < 1)
        {
            throw InvalidLine("Increment-by No. must be 1 or more.");
        }

        if (NoSeriesIncrement.Increment(startingNo) == null)
        {
            throw InvalidLine($"Starting No. '{startingNo}' contains no digits, so it cannot be incremented.");
        }

        if (!endingNo.IsNullOrWhiteSpace() && NoSeriesIncrement.Compare(startingNo, endingNo) > 0)
        {
            throw InvalidLine($"Ending No. '{endingNo}' is before Starting No. '{startingNo}'.");
        }

        // A series in use cannot be restarted below what it already handed out.
        if (LastNoUsed != null && NoSeriesIncrement.Compare(startingNo, LastNoUsed) > 0)
        {
            throw InvalidLine($"Starting No. '{startingNo}' is after the last number used, '{LastNoUsed}'.");
        }

        StartingDate = startingDate?.Date;
        StartingNo = startingNo;
        EndingNo = endingNo.IsNullOrWhiteSpace() ? null : endingNo;
        WarningNo = warningNo.IsNullOrWhiteSpace() ? null : warningNo;
        IncrementByNo = incrementByNo;

        Open = EndingNo == null || LastNoUsed == null || NoSeriesIncrement.Compare(LastNoUsed, EndingNo) < 0;
    }

    /// <summary>The number the next call would hand out, or null when the line is used up.</summary>
    public string PeekNextNo()
    {
        var next = LastNoUsed == null ? StartingNo : NoSeriesIncrement.Increment(LastNoUsed, IncrementByNo);

        return next == null || (EndingNo != null && NoSeriesIncrement.Compare(next, EndingNo) > 0) ? null : next;
    }

    /// <summary>True when the warning number has been reached (BC shows a notification).</summary>
    public bool IsPastWarningNo()
    {
        return WarningNo != null && LastNoUsed != null && NoSeriesIncrement.Compare(LastNoUsed, WarningNo) >= 0;
    }

    internal void Use(string no, DateTime usageDate)
    {
        LastNoUsed = no;
        LastDateUsed = usageDate.Date;
        Open = EndingNo == null || NoSeriesIncrement.Compare(no, EndingNo) < 0;
    }

    private static BusinessException InvalidLine(string reason)
    {
        return new BusinessException(ErpErrorCodes.NoSeries.InvalidLine).WithData("reason", reason);
    }
}
