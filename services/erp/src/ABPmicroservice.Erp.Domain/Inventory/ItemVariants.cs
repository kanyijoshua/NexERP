using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>A variant of an item, e.g. a colour or size, whose stock is kept apart.</summary>
public class ItemVariant : CompanyEntity
{
    public string ItemNo { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public bool Blocked { get; private set; }

    protected ItemVariant() { }

    public ItemVariant(Guid id, string itemNo, string code)
        : base(id)
    {
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
        SetCode(code);
    }

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code?.Trim(), nameof(code), ErpDomainConsts.MaxCodeLength).ToUpperInvariant();
    }

    public void SetDetails(string description, bool blocked)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Blocked = blocked;
    }
}

/// <summary>A unit of measure an item is bought or sold in, and how many base units it holds.</summary>
public class ItemUnitOfMeasure : CompanyEntity
{
    public string ItemNo { get; private set; }
    public string Code { get; private set; }

    /// <summary>Base units in one of this unit, e.g. 12 for a box of twelve.</summary>
    public decimal QtyPerUnitOfMeasure { get; private set; }

    protected ItemUnitOfMeasure() { }

    public ItemUnitOfMeasure(Guid id, string itemNo, string code, decimal qtyPerUnitOfMeasure)
        : base(id)
    {
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
        SetCode(code);
        SetQtyPer(qtyPerUnitOfMeasure);
    }

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code?.Trim(), nameof(code), ErpDomainConsts.MaxUnitOfMeasureCodeLength).ToUpperInvariant();
    }

    public void SetQtyPer(decimal qtyPerUnitOfMeasure)
    {
        QtyPerUnitOfMeasure = qtyPerUnitOfMeasure > 0m ? qtyPerUnitOfMeasure : 1m;
    }
}

/// <summary>Turns document units into base units and checks variants.</summary>
public class ItemUnitOfMeasureManager : DomainService
{
    private readonly IRepository<ItemUnitOfMeasure, Guid> _unitRepository;
    private readonly IRepository<ItemVariant, Guid> _variantRepository;

    public ItemUnitOfMeasureManager(IRepository<ItemUnitOfMeasure, Guid> unitRepository, IRepository<ItemVariant, Guid> variantRepository)
    {
        _unitRepository = unitRepository;
        _variantRepository = variantRepository;
    }

    /// <summary>Base units in one <paramref name="unitOfMeasureCode"/>; 1 for the base unit or a blank code.</summary>
    public async Task<decimal> GetQtyPerAsync(Item item, string unitOfMeasureCode)
    {
        var code = CodeTableEntity.NormalizeCode(unitOfMeasureCode);
        if (code == null || string.Equals(code, item.BaseUnitOfMeasureCode, StringComparison.OrdinalIgnoreCase))
        {
            return 1m;
        }

        var unit = await _unitRepository.FirstOrDefaultAsync(u => u.ItemNo == item.No && u.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.Inventory.UnitOfMeasureNotFound)
                .WithData("itemNo", item.No)
                .WithData("unitOfMeasureCode", code);
        return unit.QtyPerUnitOfMeasure;
    }

    /// <summary>A variant on a line must be one of the item's; blank means none.</summary>
    public async Task EnsureVariantAsync(string itemNo, string variantCode)
    {
        var code = CodeTableEntity.NormalizeCode(variantCode);
        if (code != null && !await _variantRepository.AnyAsync(v => v.ItemNo == itemNo && v.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.Inventory.VariantNotFound)
                .WithData("itemNo", itemNo)
                .WithData("variantCode", code);
        }
    }
}
