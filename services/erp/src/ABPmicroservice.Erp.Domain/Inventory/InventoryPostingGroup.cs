using System;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Inventory Posting Group: the kind of stock an item is
/// (resale, raw materials, finished goods), resolved to accounts by the Inventory Posting Setup.
/// </summary>
public class InventoryPostingGroup : PostingGroupBase
{
    protected InventoryPostingGroup() { }

    public InventoryPostingGroup(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>
/// Inventory Posting Setup, keyed by location and inventory posting group: the balance-sheet account the value of the stock is carried on. The row with a
/// blank location applies wherever a location has no row of its own.
/// </summary>
public class InventoryPostingSetup : CompanyEntity
{
    public string LocationCode { get; private set; }
    public string InventoryPostingGroup { get; private set; }
    public string InventoryAccountNo { get; private set; }

    protected InventoryPostingSetup() { }

    public InventoryPostingSetup(Guid id, string inventoryPostingGroup, string inventoryAccountNo, string locationCode = null)
        : base(id)
    {
        SetLocation(locationCode);
        SetInventoryPostingGroup(inventoryPostingGroup);
        SetInventoryAccount(inventoryAccountNo);
    }

    public void SetInventoryPostingGroup(string inventoryPostingGroup)
    {
        Check.NotNullOrWhiteSpace(inventoryPostingGroup, nameof(inventoryPostingGroup), ErpDomainConsts.MaxPostingGroupLength);
        InventoryPostingGroup = PostingGroupBase.NormalizeCode(inventoryPostingGroup);
    }

    public void SetLocation(string locationCode)
    {
        Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength);
        LocationCode = CodeTableEntity.NormalizeCode(locationCode);
    }

    public void SetInventoryAccount(string inventoryAccountNo)
    {
        InventoryAccountNo = PostingAccount.Normalize(inventoryAccountNo, nameof(inventoryAccountNo));
    }
}
