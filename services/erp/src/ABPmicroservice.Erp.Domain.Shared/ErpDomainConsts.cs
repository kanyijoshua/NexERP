namespace ABPmicroservice.Erp;

/// <summary>
/// Field length constraints shared across the ERP domain.
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
    public const int MaxPaymentMethodCodeLength = 10;
    public const int MaxYourReferenceLength = 35;
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
    public const int MaxLocationCodeLength = 10;
    public const int MaxVatIdentifierLength = 20;
    public const int MaxBankAccountNoLength = 30;
    public const int MaxIbanLength = 50;
    public const int MaxSwiftCodeLength = 20;
    public const int MaxCurrencySymbolLength = 10;
    public const int MaxAccountingPeriodNameLength = 30;
    public const int MaxJobTitleLength = 50;
    public const int MaxJobCategoryCodeLength = 20;
    public const int MaxJobTypeLength = 100;
    public const int MaxJobParameterLength = 4000;
    public const int MaxJobErrorLength = 4000;
    public const int MaxRecordIdLength = 256;
    public const int MaxTaxAreaCodeLength = 20;
    public const int MaxTaxGroupCodeLength = 20;
    public const int MaxCostTypeLength = 20;
    public const int MaxDeferralTemplateCodeLength = 10;
    public const int MaxCurrencyDescriptionLength = 60;
    public const int MaxContactLength = 100;
    public const int MaxVatRegistrationNoLength = 20;
    public const int MaxShippingAgentCodeLength = 10;
    public const int MaxShipmentMethodCodeLength = 10;
    public const int MaxOnHoldLength = 3;
    public const int MaxFileExtensionLength = 30;
    public const int MaxContentTypeLength = 100;

    /// <summary>
    /// Largest file that may be attached to a record. It arrives whole in one request and is kept
    /// in the database, so the cap bounds both the request and the row.
    /// </summary>
    public const int MaxAttachmentBytes = 10 * 1024 * 1024;
    public const int MaxHomePageLength = 80;
    public const int MaxCustomLayoutCodeLength = 20;

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
    /// Largest file a configuration package or data import may upload. The file arrives whole in
    /// one request and is parsed in memory, so the cap is what keeps one upload from exhausting it.
    /// </summary>
    public const int MaxImportFileBytes = 10 * 1024 * 1024;

    /// <summary>
    /// Largest an xlsx part may grow to once unzipped. A zip can hide a gigabyte in a kilobyte;
    /// reading stops here instead of trusting the sizes the archive claims.
    /// </summary>
    public const long MaxUnzippedPartBytes = 100L * 1024 * 1024;

    public const int MaxConfigValueLength = 2000;
    public const int MaxConfigErrorLength = 1000;

    /// <summary>Stored filter lines and field mappings of a package table or field, as JSON.</summary>
    public const int MaxConfigSettingsLength = 8000;

    /// <summary>
    /// Days added to the posting date for the due date of a journal-posted receivable or payable.
    /// A stand-in until payment terms carry their own date formula.
    /// </summary>
    public const int DefaultPaymentDueDays = 30;
}
