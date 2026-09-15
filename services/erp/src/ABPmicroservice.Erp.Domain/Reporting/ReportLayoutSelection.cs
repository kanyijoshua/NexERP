using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Report Layout Selection. Mirrors Business Central Table 9651 "Report Layout Selection".
/// Maps a Report Name to its active default layout.
/// </summary>
public class ReportLayoutSelection : FullAuditedEntity<Guid>
{
    public string ReportName { get; private set; }
    public Guid SelectedLayoutId { get; private set; }
    public string LayoutType { get; private set; } // "RDLC", "Word", "Excel", "Html", "Custom"

    protected ReportLayoutSelection() { }

    public ReportLayoutSelection(Guid id, string reportName, Guid selectedLayoutId, string layoutType = "Html")
        : base(id)
    {
        ReportName = Check.NotNullOrWhiteSpace(reportName, nameof(reportName), ErpDomainConsts.MaxNameLength);
        SelectedLayoutId = selectedLayoutId;
        LayoutType = layoutType;
    }

    public void SetSelectedLayout(Guid layoutId, string layoutType)
    {
        SelectedLayoutId = layoutId;
        LayoutType = layoutType;
    }
}

/// <summary>
/// Custom Report Layout. Mirrors Business Central Table 9650 "Custom Report Layouts".
/// </summary>
public class CustomReportLayout : FullAuditedEntity<Guid>
{
    public string ReportName { get; private set; }
    public string LayoutName { get; private set; }
    public string LayoutType { get; private set; } // "RDLC", "Word", "Excel", "Html"
    public string Description { get; private set; }
    public bool IsDefault { get; private set; }
    public string TemplateContent { get; private set; }

    protected CustomReportLayout() { }

    public CustomReportLayout(
        Guid id,
        string reportName,
        string layoutName,
        string layoutType,
        string description = null,
        bool isDefault = false,
        string templateContent = null
    )
        : base(id)
    {
        ReportName = Check.NotNullOrWhiteSpace(reportName, nameof(reportName), ErpDomainConsts.MaxNameLength);
        LayoutName = Check.NotNullOrWhiteSpace(layoutName, nameof(layoutName), ErpDomainConsts.MaxNameLength);
        LayoutType = Check.NotNullOrWhiteSpace(layoutType, nameof(layoutType));
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        IsDefault = isDefault;
        TemplateContent = templateContent;
    }

    public void SetDefault(bool isDefault) => IsDefault = isDefault;
}
