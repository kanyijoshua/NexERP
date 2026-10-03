namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Mirrors Business Central "Discount Posting" option in Purchases &amp; Payables Setup.
/// </summary>
public enum PurchasingDiscountPosting
{
    AllDiscounts = 0,
    InvoiceDiscounts = 1,
    LineDiscounts = 2,
    NoDiscounts = 3,
}
