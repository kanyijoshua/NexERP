using System;
using ABPmicroservice.Erp.Companies;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Item Application Entry: which inbound entry an outbound entry took its stock (and so its cost)
/// from, and how much of it.
/// </summary>
public class ItemApplicationEntry : LedgerEntryBase
{
    public Guid InboundEntryId { get; private set; }
    public Guid OutboundEntryId { get; private set; }
    public string ItemNo { get; private set; }
    public DateTime PostingDate { get; private set; }

    /// <summary>Unsigned, in base units.</summary>
    public decimal Quantity { get; private set; }

    protected ItemApplicationEntry() { }

    public ItemApplicationEntry(Guid id, Guid inboundEntryId, Guid outboundEntryId, string itemNo, DateTime postingDate, decimal quantity)
        : base(id)
    {
        InboundEntryId = inboundEntryId;
        OutboundEntryId = outboundEntryId;
        ItemNo = itemNo;
        PostingDate = postingDate;
        Quantity = Math.Abs(quantity);
    }
}
