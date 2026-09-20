namespace ABPmicroservice.Erp;

/// <summary>
/// Field length constraints shared across the ERP domain. These lengths
/// intentionally mirror Microsoft Dynamics 365 Business Central table fields.
/// </summary>
public static class ErpDomainConsts
{
    public const int MaxNoLength = 20;
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 250;
    public const int MaxAddressLength = 100;
    public const int MaxCityLength = 50;
    public const int MaxPostCodeLength = 20;
    public const int MaxCountryRegionCodeLength = 10;
    public const int MaxPhoneLength = 30;
    public const int MaxEmailLength = 80;
    public const int MaxCodeLength = 20;
    public const int MaxCurrencyCodeLength = 10;
    public const int MaxPaymentTermsCodeLength = 10;
    public const int MaxPostingGroupLength = 20;
    public const int MaxUnitOfMeasureCodeLength = 10;
    public const int MaxDocumentNoLength = 20;
    public const int MaxExternalDocumentNoLength = 35;
    public const int MaxGeneralBusPostingGroupLength = 20;
    public const int MaxVatBusPostingGroupLength = 20;
    public const int MaxDimensionCodeLength = 20;
    public const int MaxDimensionValueCodeLength = 20;
    public const int MaxWorkflowCodeLength = 30;
    public const int MaxNoSeriesCodeLength = 20;
    public const int MaxUserNameLength = 256;
    public const int MaxCommentLength = 500;
    public const int MaxProfileIdLength = 30;
    public const int MaxPackageCodeLength = 20;
    public const int MaxJournalTemplateNameLength = 10;
    public const int MaxSourceCodeLength = 10;
    public const int MaxReasonCodeLength = 10;
    public const int MaxRowNoLength = 10;
    public const int MaxTotalingLength = 250;
    public const int MaxDateFormulaLength = 32;
    public const int MaxColumnHeaderLength = 50;

    /// <summary>
    /// Body of a report layout. Generous enough for a styled page with an inline logo, small
    /// enough that one bad upload cannot fill the table.
    /// </summary>
    public const int MaxLayoutTemplateLength = 200_000;

    /// <summary>Name of a table as the export and integration APIs address it, e.g. "Customer".</summary>
    public const int MaxEntityNameLength = 100;

    public const int MaxUrlLength = 500;
    public const int MaxWebhookSecretLength = 128;

    /// <summary>Number of delivery attempts before a webhook call is abandoned.</summary>
    public const int MaxWebhookAttempts = 5;

    /// <summary>Upper bound on the rows one export or integration query may return.</summary>
    public const int MaxExportRowCount = 50_000;

    /// <summary>
    /// Days added to the posting date for the due date of a journal-posted receivable or payable.
    /// A stand-in until payment terms (BC table 3) carry their own date formula.
    /// </summary>
    public const int DefaultPaymentDueDays = 30;
}
