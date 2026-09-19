using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Item Journal Batch. Mirrors Business Central table 233 "Item Journal Batch".
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
}

/// <summary>
/// Item Journal Line. Mirrors Business Central table 83 "Item Journal Line".
/// </summary>
public class ItemJournalLine : CompanyEntity
{
    public Guid ItemJournalBatchId { get; private set; }
    public int LineNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public ItemLedgerEntryType EntryType { get; private set; } // Purchase, Sale, Positive Adjmt, Negative Adjmt
    public string DocumentNo { get; private set; }
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal Amount { get; private set; }
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
        PostingDate = postingDate;
        EntryType = entryType;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        ItemId = itemId;
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Quantity = quantity;
        UnitCost = unitCost;
        Amount = quantity * unitCost;
        DimensionSetId = dimensionSetId;
    }
}
