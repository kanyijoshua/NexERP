using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Report Layout Selection.
/// Says which layout a report is printed through in this company. With no row, the report falls
/// back to the built-in layout, which is how a company that has never customised anything still
/// prints.
/// </summary>
public class ReportLayoutSelection : CompanyEntity
{
    public string ReportName { get; private set; }

    public Guid SelectedLayoutId { get; private set; }

    public ReportLayoutType LayoutType { get; private set; }

    /// <summary>Report ID.</summary>
    public int ReportID { get; private set; }

    /// <summary>Custom Report Layout Code.</summary>
    public string CustomReportLayoutCode { get; private set; }

    /// <summary>Report Layout Description.</summary>
    public string ReportLayoutDescription { get; private set; }

    /// <summary>Report Caption.</summary>
    public string ReportCaption { get; private set; }

    protected ReportLayoutSelection() { }

    public ReportLayoutSelection(
        Guid id,
        string reportName,
        Guid selectedLayoutId,
        ReportLayoutType layoutType,
        int reportId = 0,
        string customReportLayoutCode = null,
        string reportLayoutDescription = null,
        string reportCaption = null
    )
        : base(id)
    {
        ReportName = Check.NotNullOrWhiteSpace(reportName, nameof(reportName), ErpDomainConsts.MaxNameLength);
        SetSelectedLayout(selectedLayoutId, layoutType);
        SetReportMetadata(reportId, customReportLayoutCode, reportLayoutDescription, reportCaption);
    }

    public void SetSelectedLayout(Guid layoutId, ReportLayoutType layoutType)
    {
        SelectedLayoutId = layoutId;
        LayoutType = layoutType;
    }

    public void SetReportMetadata(int reportId, string customLayoutCode, string layoutDescription, string reportCaption)
    {
        ReportID = reportId;
        CustomReportLayoutCode = CodeTableEntity.NormalizeCode(Check.Length(customLayoutCode, nameof(customLayoutCode), ErpDomainConsts.MaxCustomLayoutCodeLength));
        ReportLayoutDescription = Check.Length(layoutDescription, nameof(layoutDescription), ErpDomainConsts.MaxDescriptionLength);
        ReportCaption = Check.Length(reportCaption, nameof(reportCaption), ErpDomainConsts.MaxNameLength);
    }
}

/// <summary>
/// Custom Report Layout.
/// <para>
/// A layout is the markup a report is rendered through, held as text so it can be downloaded,
/// edited and uploaded again.
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

    /// <summary>Layout code.</summary>
    public string Code { get; private set; }

    /// <summary>Report ID.</summary>
    public int ReportID { get; private set; }

    /// <summary>File extension.</summary>
    public string FileExtension { get; private set; }

    /// <summary>Built-in layout flag.</summary>
    public bool BuiltIn { get; private set; }

    /// <summary>User who last modified the layout.</summary>
    public string LastModifiedByUser { get; private set; }

    /// <summary>Timestamp of last layout update.</summary>
    public DateTime? LayoutLastUpdated { get; private set; }

    protected CustomReportLayout() { }

    public CustomReportLayout(
        Guid id,
        string reportName,
        string layoutName,
        ReportLayoutType layoutType,
        string templateContent,
        string description = null,
        string code = null,
        int reportId = 0,
        string fileExtension = "html",
        bool builtIn = false,
        string lastModifiedByUser = null,
        DateTime? layoutLastUpdated = null
    )
        : base(id)
    {
        ReportName = Check.NotNullOrWhiteSpace(reportName, nameof(reportName), ErpDomainConsts.MaxNameLength);
        LayoutName = Check.NotNullOrWhiteSpace(layoutName, nameof(layoutName), ErpDomainConsts.MaxNameLength);
        LayoutType = layoutType;
        SetCode(code ?? layoutName);
        ReportID = reportId;
        FileExtension = Check.Length(fileExtension, nameof(fileExtension), ErpDomainConsts.MaxFileExtensionLength);
        BuiltIn = builtIn;
        LastModifiedByUser = Check.Length(lastModifiedByUser, nameof(lastModifiedByUser), ErpDomainConsts.MaxUserNameLength);
        LayoutLastUpdated = layoutLastUpdated ?? DateTime.UtcNow;
        Update(templateContent, description);
    }

    public void SetCode(string code) =>
        Code = CodeTableEntity.NormalizeCode(Check.Length(code, nameof(code), ErpDomainConsts.MaxCustomLayoutCodeLength));

    public void SetReportID(int reportId) => ReportID = reportId;

    public void SetFileExtension(string ext) =>
        FileExtension = Check.Length(ext, nameof(ext), ErpDomainConsts.MaxFileExtensionLength);

    public void SetAuditDetails(string user, DateTime? updated, bool builtIn)
    {
        LastModifiedByUser = Check.Length(user, nameof(user), ErpDomainConsts.MaxUserNameLength);
        LayoutLastUpdated = updated ?? DateTime.UtcNow;
        BuiltIn = builtIn;
    }

    public void Update(string templateContent, string description)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        LayoutLastUpdated = DateTime.UtcNow;
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
        LayoutLastUpdated = DateTime.UtcNow;
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
    /// Word and Excel layouts are in the model but nothing can render them
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
