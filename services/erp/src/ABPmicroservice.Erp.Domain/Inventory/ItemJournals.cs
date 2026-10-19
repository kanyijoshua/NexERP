using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>An item journal template: what kind of item journal its batches are.</summary>
public class ItemJournalTemplate : CompanyEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ItemJournalTemplateType Type { get; private set; }

    protected ItemJournalTemplate() { }

    public ItemJournalTemplate(Guid id, string name, ItemJournalTemplateType type)
        : base(id)
    {
        SetName(name);
        Type = type;
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name?.Trim(), nameof(name), ErpDomainConsts.MaxJournalTemplateNameLength).ToUpperInvariant();
    }

    public void SetDescription(string description)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    public void SetType(ItemJournalTemplateType type) => Type = type;
}

/// <summary>
/// Item Journal Batch.
/// </summary>
public class ItemJournalBatch : CompanyAggregateRoot
{
    public string JournalTemplateName { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    protected ItemJournalBatch() { }

    public ItemJournalBatch(Guid id, string journalTemplateName, string name, string description = null)
        : base(id)
    {
        JournalTemplateName = Check.NotNullOrWhiteSpace(journalTemplateName, nameof(journalTemplateName), ErpDomainConsts.MaxNameLength);
        SetName(name);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
    }

    public void SetDescription(string description)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }
}

/// <summary>
/// Item Journal Line: one stock adjustment, move between locations, counted quantity or new cost,
/// depending on the kind of journal.
/// </summary>
public class ItemJournalLine : CompanyEntity
{
    public Guid ItemJournalBatchId { get; private set; }
    public int LineNo { get; private set; }
    public DateTime PostingDate { get; private set; }

    /// <summary>Positive or negative adjustment on an item journal; set from the counted difference on a physical inventory journal.</summary>
    public ItemLedgerEntryType EntryType { get; private set; }

    public string DocumentNo { get; private set; }
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public string Description { get; private set; }
    public string LocationCode { get; private set; }

    /// <summary>Where a transfer journal line moves the stock to.</summary>
    public string NewLocationCode { get; private set; }

    public string VariantCode { get; private set; }
    public string UnitOfMeasureCode { get; private set; }
    public decimal QtyPerUnitOfMeasure { get; private set; } = 1m;
    public decimal Quantity { get; private set; }

    /// <summary>Of an adjustment coming in; blank (zero) takes the item's unit cost.</summary>
    public decimal UnitCost { get; private set; }

    public decimal Amount { get; private set; }

    /// <summary>One lot or serial number for the line; more go on its item tracking lines.</summary>
    public string LotNo { get; private set; }
    public string SerialNo { get; private set; }
    public DateTime? ExpirationDate { get; private set; }

    /// <summary>What the stock should be by the books, filled in by Calculate Inventory.</summary>
    public decimal QtyCalculated { get; private set; }

    /// <summary>What was counted.</summary>
    public decimal QtyPhysInventory { get; private set; }

    /// <summary>The cost of the stock now, filled in by Calculate Inventory Value.</summary>
    public decimal UnitCostCalculated { get; private set; }

    /// <summary>The cost the stock is revalued to.</summary>
    public decimal UnitCostRevalued { get; private set; }

    public Guid DimensionSetId { get; private set; }

    protected ItemJournalLine() { }

    public ItemJournalLine(
        Guid id,
        Guid itemJournalBatchId,
        int lineNo,
        DateTime postingDate,
        ItemLedgerEntryType entryType,
        string documentNo,
        Guid itemId,
        string itemNo,
        string description,
        decimal quantity,
        decimal unitCost,
        Guid dimensionSetId = default
    )
        : base(id)
    {
        ItemJournalBatchId = itemJournalBatchId;
        LineNo = lineNo;
        SetPosting(postingDate, entryType, documentNo);
        SetItem(itemId, itemNo, description);
        SetQuantity(quantity, unitCost);
        DimensionSetId = dimensionSetId;
    }

    public void SetPosting(DateTime postingDate, ItemLedgerEntryType entryType, string documentNo)
    {
        PostingDate = postingDate;
        EntryType = entryType;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
    }

    public void SetItem(Guid itemId, string itemNo, string description)
    {
        ItemId = itemId;
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    public void SetQuantity(decimal quantity, decimal unitCost)
    {
        Quantity = quantity;
        UnitCost = unitCost;
        Amount = Math.Round(quantity * unitCost, 2, MidpointRounding.AwayFromZero);
    }

    public void SetLocations(string locationCode, string newLocationCode)
    {
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
        NewLocationCode = CodeTableEntity.NormalizeCode(Check.Length(newLocationCode, nameof(newLocationCode), ErpDomainConsts.MaxLocationCodeLength));
    }

    public void SetVariantAndUnit(string variantCode, string unitOfMeasureCode, decimal qtyPerUnitOfMeasure)
    {
        VariantCode = CodeTableEntity.NormalizeCode(Check.Length(variantCode, nameof(variantCode), ErpDomainConsts.MaxCodeLength));
        UnitOfMeasureCode = CodeTableEntity.NormalizeCode(Check.Length(unitOfMeasureCode, nameof(unitOfMeasureCode), ErpDomainConsts.MaxUnitOfMeasureCodeLength));
        QtyPerUnitOfMeasure = qtyPerUnitOfMeasure > 0m ? qtyPerUnitOfMeasure : 1m;
    }

    public void SetTracking(string lotNo, string serialNo, DateTime? expirationDate)
    {
        LotNo = lotNo.IsNullOrWhiteSpace() ? null : Check.Length(lotNo.Trim(), nameof(lotNo), ErpDomainConsts.MaxLotNoLength);
        SerialNo = serialNo.IsNullOrWhiteSpace() ? null : Check.Length(serialNo.Trim(), nameof(serialNo), ErpDomainConsts.MaxLotNoLength);
        ExpirationDate = expirationDate?.Date;
    }

    /// <summary>The counted quantity; on a calculated line also what the books say.</summary>
    public void SetPhysInventory(decimal qtyCalculated, decimal qtyPhysInventory)
    {
        QtyCalculated = qtyCalculated;
        QtyPhysInventory = qtyPhysInventory;
        var difference = qtyPhysInventory - qtyCalculated;
        EntryType = difference < 0m ? ItemLedgerEntryType.NegativeAdjmt : ItemLedgerEntryType.PositiveAdjmt;
        SetQuantity(Math.Abs(difference), UnitCost);
    }

    public void SetRevaluation(decimal unitCostCalculated, decimal unitCostRevalued)
    {
        UnitCostCalculated = unitCostCalculated;
        UnitCostRevalued = unitCostRevalued;
    }
}
