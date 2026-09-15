using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Item Ledger Entry. Mirrors Business Central table 32 "Item Ledger Entry".
/// Immutable once created (posted).
/// </summary>
public class ItemLedgerEntry : Entity<Guid>
{
    public long EntryNo { get; internal set; }

    public Guid ItemId { get; private set; }

    public string ItemNo { get; private set; }

    public DateTime PostingDate { get; private set; }

    public ItemLedgerEntryType EntryType { get; private set; }

    public string DocumentNo { get; private set; }

    /// <summary>Positive for inbound, negative for outbound.</summary>
    public decimal Quantity { get; private set; }

    public decimal UnitCost { get; private set; }

    public decimal SalesAmount { get; private set; }

    public string LocationCode { get; private set; }

    protected ItemLedgerEntry() { }

    public ItemLedgerEntry(
        Guid id,
        Guid itemId,
        string itemNo,
        DateTime postingDate,
        ItemLedgerEntryType entryType,
        string documentNo,
        decimal quantity,
        decimal unitCost = 0m,
        decimal salesAmount = 0m,
        string locationCode = null
    )
        : base(id)
    {
        ItemId = itemId;
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
        PostingDate = postingDate;
        EntryType = entryType;
        DocumentNo = Check.Length(
            documentNo,
            nameof(documentNo),
            ErpDomainConsts.MaxDocumentNoLength
        );
        Quantity = quantity;
        UnitCost = unitCost;
        SalesAmount = salesAmount;
        LocationCode = Check.Length(
            locationCode,
            nameof(locationCode),
            ErpDomainConsts.MaxCodeLength
        );
    }
}
