using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Inventory Setup: one row per company with the item number
/// series and the rules stock moves by.
/// </summary>
public class InventorySetup : CompanyEntity
{
    /// <summary>The series a new item's number is drawn from when it is left blank.</summary>
    public string ItemNos { get; private set; }

    /// <summary>Every item entry must name a location.</summary>
    public bool LocationMandatory { get; private set; }

    /// <summary>A sale may not take an inventory item below zero.</summary>
    public bool PreventNegativeInventory { get; private set; }

    /// <summary>
    /// Sales post their cost of goods sold to the G/L as they post, instead of waiting for a
    /// "Post Inventory Cost to G/L" run.
    /// </summary>
    public bool AutomaticCostPosting { get; private set; }

    protected InventorySetup() { }

    public InventorySetup(Guid id)
        : base(id)
    {
        AutomaticCostPosting = true;
    }

    public void SetNumbering(string itemNos)
    {
        ItemNos = itemNos.IsNullOrWhiteSpace()
            ? null
            : Check.Length(itemNos.Trim(), nameof(itemNos), ErpDomainConsts.MaxNoSeriesCodeLength);
    }

    public void SetRules(bool locationMandatory, bool preventNegativeInventory, bool automaticCostPosting)
    {
        LocationMandatory = locationMandatory;
        PreventNegativeInventory = preventNegativeInventory;
        AutomaticCostPosting = automaticCostPosting;
    }
}

public class InventorySetupManager : DomainService
{
    private readonly IRepository<InventorySetup, Guid> _repository;

    public InventorySetupManager(IRepository<InventorySetup, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>The current company's setup, created on first use.</summary>
    public async Task<InventorySetup> GetAsync()
    {
        return await _repository.FirstOrDefaultAsync()
            ?? await _repository.InsertAsync(new InventorySetup(GuidGenerator.Create()), autoSave: true);
    }
}
