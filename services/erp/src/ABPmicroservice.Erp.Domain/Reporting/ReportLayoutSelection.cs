using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Report Layout Selection. Mirrors Business Central Table 9651 "Report Layout Selection".
/// Says which layout a report is printed through in this company. With no row, the report falls
/// back to the built-in layout, which is how a company that has never customised anything still
/// prints.
/// </summary>
public class ReportLayoutSelection : CompanyEntity
{
    public string ReportName { get; private set; }

    public Guid SelectedLayoutId { get; private set; }

    public ReportLayoutType LayoutType { get; private set; }

    protected ReportLayoutSelection() { }

    public ReportLayoutSelection(Guid id, string reportName, Guid selectedLayoutId, ReportLayoutType layoutType)
        : base(id)
    {
        ReportName = Check.NotNullOrWhiteSpace(reportName, nameof(reportName), ErpDomainConsts.MaxNameLength);
        SetSelectedLayout(selectedLayoutId, layoutType);
    }

    public void SetSelectedLayout(Guid layoutId, ReportLayoutType layoutType)
    {
        SelectedLayoutId = layoutId;
        LayoutType = layoutType;
    }
}

/// <summary>
/// Custom Report Layout. Mirrors Business Central Table 9650 "Custom Report Layouts".
/// <para>
/// A layout is the markup a report is rendered through, held as text so it can be downloaded,
/// edited and uploaded again the way BC's Word and Excel layouts are.
/// </para>
/// </summary>
public class CustomReportLayout : CompanyEntity
{
    public string ReportName { get; private set; }

    public string LayoutName { get; private set; }

    public ReportLayoutType LayoutType { get; private set; }

    public string Description { get; private set; }

    public bool IsDefault { get; private set; }

    /// <summary>The layout itself. See <see cref="ReportTemplate"/> for what it may contain.</summary>
    public string TemplateContent { get; private set; }

    protected CustomReportLayout() { }

    public CustomReportLayout(
        Guid id,
        string reportName,
        string layoutName,
        ReportLayoutType layoutType,
        string templateContent,
        string description = null
    )
        : base(id)
    {
        ReportName = Check.NotNullOrWhiteSpace(reportName, nameof(reportName), ErpDomainConsts.MaxNameLength);
        LayoutName = Check.NotNullOrWhiteSpace(layoutName, nameof(layoutName), ErpDomainConsts.MaxNameLength);
        LayoutType = layoutType;
        Update(templateContent, description);
    }

    public void Update(string templateContent, string description)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        SetTemplate(templateContent);
    }

    /// <summary>
    /// Replaces the layout body. The template is parsed here rather than at print time: a layout
    /// that cannot render is refused when it is uploaded, not discovered by whoever prints next.
    /// </summary>
    public void SetTemplate(string templateContent)
    {
        EnsureRenderable();

        if (templateContent.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Reports.LayoutTemplateEmpty)
                .WithData("layoutName", LayoutName);
        }

        if (templateContent.Length > ErpDomainConsts.MaxLayoutTemplateLength)
        {
            throw new BusinessException(ErpErrorCodes.Reports.LayoutTemplateNotValid)
                .WithData("reason", "TooLong")
                .WithData("token", templateContent.Length.ToString());
        }

        ReportTemplate.Validate(templateContent);
        TemplateContent = templateContent;
    }

    public void SetDefault(bool isDefault)
    {
        if (isDefault)
        {
            EnsureRenderable();
        }

        IsDefault = isDefault;
    }

    /// <summary>
    /// Word and Excel layouts are in the model for parity with BC but nothing can render them
    /// yet, so they are refused here rather than accepted and quietly skipped when printing.
    /// </summary>
    private void EnsureRenderable()
    {
        if (LayoutType != ReportLayoutType.Html)
        {
            throw new BusinessException(ErpErrorCodes.Reports.LayoutTypeNotRenderable)
                .WithData("layoutType", LayoutType.ToString());
        }
    }
}
