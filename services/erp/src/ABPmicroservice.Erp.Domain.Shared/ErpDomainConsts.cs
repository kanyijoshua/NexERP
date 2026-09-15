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
    public const int MaxProfileIdLength = 30;
    public const int MaxPackageCodeLength = 20;
}
