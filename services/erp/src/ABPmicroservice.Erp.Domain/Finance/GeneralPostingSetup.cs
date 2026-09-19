using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// General Posting Setup. Mirrors Business Central table 252 "General Posting Setup".
/// Maps (Gen. Bus. Posting Group + Gen. Prod. Posting Group) -> G/L accounts.
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

    public GeneralPostingSetup(
        Guid id,
        string genBusPostingGroup,
        string genProdPostingGroup,
        string salesAccountNo,
        string purchAccountNo,
        string cogsAccountNo,
        string inventoryAdjmtAccountNo
    )
        : base(id)
    {
        GenBusPostingGroup = Check.NotNullOrWhiteSpace(genBusPostingGroup, nameof(genBusPostingGroup), ErpDomainConsts.MaxGeneralBusPostingGroupLength);
        GenProdPostingGroup = Check.NotNullOrWhiteSpace(genProdPostingGroup, nameof(genProdPostingGroup), ErpDomainConsts.MaxPostingGroupLength);
        SalesAccountNo = Check.NotNullOrWhiteSpace(salesAccountNo, nameof(salesAccountNo), ErpDomainConsts.MaxNoLength);
        PurchAccountNo = Check.NotNullOrWhiteSpace(purchAccountNo, nameof(purchAccountNo), ErpDomainConsts.MaxNoLength);
        COGSAccountNo = Check.NotNullOrWhiteSpace(cogsAccountNo, nameof(cogsAccountNo), ErpDomainConsts.MaxNoLength);
        InventoryAdjmtAccountNo = Check.NotNullOrWhiteSpace(inventoryAdjmtAccountNo, nameof(inventoryAdjmtAccountNo), ErpDomainConsts.MaxNoLength);
        
        SalesCreditMemoAccountNo = SalesAccountNo;
        PurchCreditMemoAccountNo = PurchAccountNo;
    }
}
