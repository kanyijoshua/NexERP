using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>Report Selection.</summary>
public class ReportSelection : CompanyEntity
{
    public ReportSelectionUsage Usage { get; private set; }
    public string Sequence { get; private set; }

    public int ReportId { get; private set; }
    public string CustomReportLayoutCode { get; private set; }
    public bool UseForEmailAttachment { get; private set; }
    public bool UseForEmailBody { get; private set; }
    public string EmailBodyLayoutCode { get; private set; }
    public string ReportLayoutName { get; private set; }

    protected ReportSelection() { }

    public ReportSelection(Guid id, ReportSelectionUsage usage, string sequence)
        : base(id)
    {
        SetKey(usage, sequence);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(ReportSelectionUsage usage, string sequence)
    {
        Usage = usage;
        Sequence = Check.NotNullOrWhiteSpace(sequence, nameof(sequence), 10).Trim().ToUpperInvariant();
    }

    public void Set(
        int reportId,
        string customReportLayoutCode,
        bool useForEmailAttachment,
        bool useForEmailBody,
        string emailBodyLayoutCode,
        string reportLayoutName
    )
    {
        ReportId = reportId;
        CustomReportLayoutCode = CodeTableEntity.NormalizeCode(Check.Length(customReportLayoutCode, nameof(customReportLayoutCode), 20));
        UseForEmailAttachment = useForEmailAttachment;
        UseForEmailBody = useForEmailBody;
        EmailBodyLayoutCode = CodeTableEntity.NormalizeCode(Check.Length(emailBodyLayoutCode, nameof(emailBodyLayoutCode), 20));
        ReportLayoutName = Check.Length(reportLayoutName, nameof(reportLayoutName), 250);
    }
}

/// <summary>Custom Report Selection.</summary>
public class CustomReportSelection : CompanyEntity
{
    public int SourceType { get; private set; }
    public string SourceNo { get; private set; }
    public ReportSelectionUsage Usage { get; private set; }
    public int Sequence { get; private set; }

    public int ReportId { get; private set; }
    public string CustomReportLayoutCode { get; private set; }
    public string SendToEmail { get; private set; }
    public bool UseForEmailAttachment { get; private set; }
    public bool UseForEmailBody { get; private set; }
    public string EmailBodyLayoutCode { get; private set; }
    public bool UseEmailFromContact { get; private set; }

    protected CustomReportSelection() { }

    public CustomReportSelection(Guid id, int sourceType, string sourceNo, ReportSelectionUsage usage, int sequence)
        : base(id)
    {
        SetKey(sourceType, sourceNo, usage, sequence);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(
        int sourceType,
        string sourceNo,
        ReportSelectionUsage usage,
        int sequence
    )
    {
        SourceType = sourceType;
        SourceNo = Check.NotNullOrWhiteSpace(sourceNo, nameof(sourceNo), 20).Trim().ToUpperInvariant();
        Usage = usage;
        Sequence = sequence;
    }

    public void Set(
        int reportId,
        string customReportLayoutCode,
        string sendToEmail,
        bool useForEmailAttachment,
        bool useForEmailBody,
        string emailBodyLayoutCode,
        bool useEmailFromContact
    )
    {
        ReportId = reportId;
        CustomReportLayoutCode = CodeTableEntity.NormalizeCode(Check.Length(customReportLayoutCode, nameof(customReportLayoutCode), 20));
        SendToEmail = Check.Length(sendToEmail, nameof(sendToEmail), 200);
        UseForEmailAttachment = useForEmailAttachment;
        UseForEmailBody = useForEmailBody;
        EmailBodyLayoutCode = CodeTableEntity.NormalizeCode(Check.Length(emailBodyLayoutCode, nameof(emailBodyLayoutCode), 20));
        UseEmailFromContact = useEmailFromContact;
    }
}
