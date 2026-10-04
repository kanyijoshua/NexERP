namespace ABPmicroservice.Erp.Finance;

public enum ApplicationMethod
{
    Manual = 0,
    ApplyToOldest = 1,
}

public enum GeneralLedgerSetupShowAmounts
{
    AmountOnly = 0,
    DebitCreditOnly = 1,
    AllAmounts = 2,
}

public enum GLSetupVatCalculation
{
    BillToPayToNo = 0,
    SellToBuyFromNo = 1,
}

public enum CurrencyInvoiceRoundingType
{
    Nearest = 0,
    Up = 1,
    Down = 2,
}
