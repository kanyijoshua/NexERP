namespace ABPmicroservice.Erp.CashManagement;

/// <summary>What a deduction code withholds from a payment.</summary>
public enum PaymentDeductionType
{
    /// <summary>Income tax withheld from the payee and owed to the revenue authority.</summary>
    WithholdingTax = 0,

    /// <summary>The part of the VAT on the invoice that the payer remits directly.</summary>
    WithholdingVat = 1,

    /// <summary>Money held back until a contract's defects period has passed.</summary>
    Retention = 2,
}
