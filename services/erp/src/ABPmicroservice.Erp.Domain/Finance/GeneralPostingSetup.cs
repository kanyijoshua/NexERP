using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// General Posting Setup.
/// Maps (Gen. Bus. Posting Group + Gen. Prod. Posting Group) -> G/L accounts.
/// <para>
/// The business group may be blank (the row used for parties without one), and every
/// account may be blank: posting refuses a blank account only when it actually needs it.
/// </para>
/// </summary>
public class GeneralPostingSetup : CompanyEntity
{
    public string GenBusPostingGroup { get; private set; }
    public string GenProdPostingGroup { get; private set; }

    public string SalesAccountNo { get; private set; }
    public string SalesCreditMemoAccountNo { get; private set; }
    public string SalesDiscountAccountNo { get; private set; }

    public string PurchAccountNo { get; private set; }
    public string PurchCreditMemoAccountNo { get; private set; }
    public string PurchDiscountAccountNo { get; private set; }

    public string COGSAccountNo { get; private set; }
    public string InventoryAdjmtAccountNo { get; private set; }

    protected GeneralPostingSetup() { }

    public GeneralPostingSetup(Guid id, string genBusPostingGroup, string genProdPostingGroup)
        : base(id)
    {
        SetKey(genBusPostingGroup, genProdPostingGroup);
    }

    public GeneralPostingSetup(
        Guid id,
        string genBusPostingGroup,
        string genProdPostingGroup,
        string salesAccountNo,
        string purchAccountNo,
        string cogsAccountNo,
        string inventoryAdjmtAccountNo
    )
        : this(id, genBusPostingGroup, genProdPostingGroup)
    {
        SetSalesAccounts(salesAccountNo, salesAccountNo, null);
        SetPurchaseAccounts(purchAccountNo, purchAccountNo, null);
        SetInventoryAccounts(cogsAccountNo, inventoryAdjmtAccountNo);
    }

    public void SetKey(string genBusPostingGroup, string genProdPostingGroup)
    {
        Check.Length(genBusPostingGroup, nameof(genBusPostingGroup), ErpDomainConsts.MaxGeneralBusPostingGroupLength);
        Check.NotNullOrWhiteSpace(genProdPostingGroup, nameof(genProdPostingGroup), ErpDomainConsts.MaxPostingGroupLength);

        GenBusPostingGroup = PostingGroupBase.NormalizeCode(genBusPostingGroup);
        GenProdPostingGroup = PostingGroupBase.NormalizeCode(genProdPostingGroup);
    }

    public void SetSalesAccounts(string salesAccountNo, string salesCreditMemoAccountNo, string salesDiscountAccountNo)
    {
        SalesAccountNo = PostingAccount.Normalize(salesAccountNo, nameof(salesAccountNo));
        SalesCreditMemoAccountNo = PostingAccount.Normalize(salesCreditMemoAccountNo, nameof(salesCreditMemoAccountNo));
        SalesDiscountAccountNo = PostingAccount.Normalize(salesDiscountAccountNo, nameof(salesDiscountAccountNo));
    }

    public void SetPurchaseAccounts(string purchAccountNo, string purchCreditMemoAccountNo, string purchDiscountAccountNo)
    {
        PurchAccountNo = PostingAccount.Normalize(purchAccountNo, nameof(purchAccountNo));
        PurchCreditMemoAccountNo = PostingAccount.Normalize(purchCreditMemoAccountNo, nameof(purchCreditMemoAccountNo));
        PurchDiscountAccountNo = PostingAccount.Normalize(purchDiscountAccountNo, nameof(purchDiscountAccountNo));
    }

    public void SetInventoryAccounts(string cogsAccountNo, string inventoryAdjmtAccountNo)
    {
        COGSAccountNo = PostingAccount.Normalize(cogsAccountNo, nameof(cogsAccountNo));
        InventoryAdjmtAccountNo = PostingAccount.Normalize(inventoryAdjmtAccountNo, nameof(inventoryAdjmtAccountNo));
    }

    /// <summary>"DOMESTIC / RETAIL", or "/ RETAIL" for the blank business group, as messages show it.</summary>
    public static string DescribeKey(string genBusPostingGroup, string genProdPostingGroup)
    {
        return $"{genBusPostingGroup} / {genProdPostingGroup}".Trim();
    }
}

/// <summary>A G/L account number on a setup table: blank means "not set up".</summary>
internal static class PostingAccount
{
    public static string Normalize(string accountNo, string parameterName)
    {
        return accountNo.IsNullOrWhiteSpace()
            ? null
            : Check.Length(accountNo.Trim(), parameterName, ErpDomainConsts.MaxNoLength);
    }
}
