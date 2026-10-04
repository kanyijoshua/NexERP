using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// VAT Business Posting Group: how a customer or vendor is
/// taxed (domestic, EU, export), the row key of the VAT Posting Setup.
/// </summary>
public class VatBusinessPostingGroup : PostingGroupBase
{
    protected VatBusinessPostingGroup() { }

    public VatBusinessPostingGroup(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>
/// VAT Product Posting Group: the VAT rate class of an item or
/// G/L account (standard, reduced, zero), the column key of the VAT Posting Setup.
/// </summary>
public class VatProductPostingGroup : PostingGroupBase
{
    protected VatProductPostingGroup() { }

    public VatProductPostingGroup(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>
/// VAT Posting Setup: per business and product VAT group, the
/// rate, how it is calculated and the accounts the VAT posts to. Either key may be blank.
/// </summary>
public class VatPostingSetup : CompanyEntity
{
    public string VatBusPostingGroup { get; private set; }
    public string VatProdPostingGroup { get; private set; }
    public string Description { get; private set; }

    /// <summary>Groups setups with the same rate on VAT statements, e.g. "VAT16".</summary>
    public string VatIdentifier { get; private set; }

    public decimal VatPercent { get; private set; }
    public VatCalculationType VatCalculationType { get; private set; }

    public string SalesVatAccountNo { get; private set; }
    public string PurchaseVatAccountNo { get; private set; }

    /// <summary>Reverse charge only: the account the self-assessed output VAT is credited to.</summary>
    public string ReverseChrgVatAccountNo { get; private set; }

    public bool Blocked { get; private set; }

    protected VatPostingSetup() { }

    public VatPostingSetup(Guid id, string vatBusPostingGroup, string vatProdPostingGroup)
        : base(id)
    {
        SetKey(vatBusPostingGroup, vatProdPostingGroup);
    }

    public void SetKey(string vatBusPostingGroup, string vatProdPostingGroup)
    {
        Check.Length(vatBusPostingGroup, nameof(vatBusPostingGroup), ErpDomainConsts.MaxVatBusPostingGroupLength);
        Check.Length(vatProdPostingGroup, nameof(vatProdPostingGroup), ErpDomainConsts.MaxPostingGroupLength);

        VatBusPostingGroup = CodeTableEntity.NormalizeCode(vatBusPostingGroup);
        VatProdPostingGroup = CodeTableEntity.NormalizeCode(vatProdPostingGroup);
    }

    public void SetRate(VatCalculationType calculationType, decimal vatPercent, string vatIdentifier, string description)
    {
        if (vatPercent < 0 || vatPercent > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", vatPercent);
        }

        VatCalculationType = calculationType;
        // Full VAT posts the whole line as VAT, so a rate means nothing there.
        VatPercent = calculationType == VatCalculationType.FullVat ? 100m : vatPercent;
        VatIdentifier = CodeTableEntity.NormalizeCode(Check.Length(vatIdentifier, nameof(vatIdentifier), ErpDomainConsts.MaxVatIdentifierLength));
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    public void SetAccounts(string salesVatAccountNo, string purchaseVatAccountNo, string reverseChrgVatAccountNo)
    {
        SalesVatAccountNo = PostingAccount.Normalize(salesVatAccountNo, nameof(salesVatAccountNo));
        PurchaseVatAccountNo = PostingAccount.Normalize(purchaseVatAccountNo, nameof(purchaseVatAccountNo));
        ReverseChrgVatAccountNo = PostingAccount.Normalize(reverseChrgVatAccountNo, nameof(reverseChrgVatAccountNo));
    }

    public void SetBlocked(bool blocked) => Blocked = blocked;

    public static string DescribeKey(string vatBusPostingGroup, string vatProdPostingGroup)
    {
        return $"{vatBusPostingGroup} / {vatProdPostingGroup}".Trim();
    }
}
