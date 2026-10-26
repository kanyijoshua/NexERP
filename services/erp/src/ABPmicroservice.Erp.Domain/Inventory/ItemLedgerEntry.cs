using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Item Ledger Entry.
/// Immutable once created (posted), apart from the bookkeeping columns: how much of it has been
/// invoiced and at what cost, and how much of it is still open for application.
/// <para>
/// An inbound entry is open while part of it is still in stock; outbound entries take from it
/// (oldest first, or by serial number), and <see cref="RemainingQuantity"/> says what is left. An
/// outbound entry is open only while it took stock that was not there (negative inventory), until
/// a later receipt covers it.
/// </para>
/// </summary>
public class ItemLedgerEntry : LedgerEntryBase
{

    public Guid ItemId { get; private set; }

    public string ItemNo { get; private set; }

    public DateTime PostingDate { get; private set; }

    public ItemLedgerEntryType EntryType { get; private set; }

    public string DocumentNo { get; private set; }

    /// <summary>The posted shipment or receipt line (or the document line) that posted this entry.</summary>
    public Guid? DocumentLineId { get; private set; }

    /// <summary>Positive for inbound, negative for outbound, in the item's base unit of measure.</summary>
    public decimal Quantity { get; private set; }

    /// <summary>Inbound (the quantity is positive).</summary>
    public bool Positive { get; private set; }

    /// <summary>
    /// How much of <see cref="Quantity"/> has been invoiced, with the same sign. A shipment or
    /// receipt starts at zero and carries expected cost until its invoice turns it into actual cost.
    /// </summary>
    public decimal InvoicedQuantity { get; private set; }

    /// <summary>
    /// What one unit cost: the expected cost until it is invoiced, then the invoiced cost. Outbound
    /// entries take their cost from the entries they apply to.
    /// </summary>
    public decimal UnitCost { get; private set; }

    /// <summary>The cost the posting started from, before any invoice or revaluation changed it.</summary>
    public decimal ExpectedUnitCost { get; private set; }

    /// <summary>The actual cost invoiced so far, with the sign of the quantity.</summary>
    public decimal CostAmountActual { get; private set; }

    public decimal SalesAmount { get; private set; }

    public string LocationCode { get; private set; }

    public string VariantCode { get; private set; }

    public string LotNo { get; private set; }

    public string SerialNo { get; private set; }

    public DateTime? ExpirationDate { get; private set; }

    /// <summary>What is left to apply: still in stock for an inbound entry, still uncovered for an outbound one.</summary>
    public decimal RemainingQuantity { get; private set; }

    public bool Open { get; private set; }

    public bool CompletelyInvoiced => InvoicedQuantity == Quantity;

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
        string locationCode = null,
        bool invoiced = true
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
        Positive = quantity > 0m;
        InvoicedQuantity = invoiced ? quantity : 0m;
        UnitCost = unitCost;
        ExpectedUnitCost = unitCost;
        CostAmountActual = invoiced ? Math.Round(quantity * unitCost, 2, MidpointRounding.AwayFromZero) : 0m;
        SalesAmount = salesAmount;
        LocationCode = Check.Length(
            locationCode,
            nameof(locationCode),
            ErpDomainConsts.MaxCodeLength
        );
        RemainingQuantity = quantity;
        Open = quantity != 0m;
    }

    /// <summary>Variant, lot and serial number; set before the entry is inserted.</summary>
    internal void SetTracking(string variantCode, string lotNo, string serialNo, DateTime? expirationDate)
    {
        VariantCode = CodeTableEntity.NormalizeCode(Check.Length(variantCode, nameof(variantCode), ErpDomainConsts.MaxCodeLength));
        LotNo = lotNo.IsNullOrWhiteSpace() ? null : Check.Length(lotNo.Trim(), nameof(lotNo), ErpDomainConsts.MaxLotNoLength);
        SerialNo = serialNo.IsNullOrWhiteSpace() ? null : Check.Length(serialNo.Trim(), nameof(serialNo), ErpDomainConsts.MaxLotNoLength);
        ExpirationDate = expirationDate?.Date;
    }

    internal void SetDocumentLine(Guid? documentLineId)
    {
        DocumentLineId = documentLineId;
    }

    /// <summary>The cost an outbound entry got from the entries it applied to; set before insert.</summary>
    internal void SetCost(decimal unitCost, bool invoiced)
    {
        UnitCost = unitCost;
        ExpectedUnitCost = unitCost;
        CostAmountActual = invoiced ? Math.Round(Quantity * unitCost, 2, MidpointRounding.AwayFromZero) : 0m;
    }

    /// <summary>Takes <paramref name="quantity"/> (unsigned) off what is left to apply.</summary>
    internal void Apply(decimal quantity)
    {
        RemainingQuantity -= Math.Sign(Quantity) * Math.Abs(quantity);
        Open = RemainingQuantity != 0m;
    }

    /// <summary>
    /// Adds the signed quantity and actual cost an invoice has just covered. The unit cost becomes
    /// the actual cost of what is invoiced and the expected cost of the rest.
    /// </summary>
    internal void AddInvoiced(decimal quantity, decimal actualCost)
    {
        InvoicedQuantity += quantity;
        CostAmountActual += actualCost;
        if (Quantity != 0m)
        {
            var notInvoiced = Math.Abs(Quantity) - Math.Abs(InvoicedQuantity);
            UnitCost = Math.Round((notInvoiced * ExpectedUnitCost + Math.Abs(CostAmountActual)) / Math.Abs(Quantity), 5, MidpointRounding.AwayFromZero);
        }
    }

    /// <summary>A revaluation: what is left in stock is worth <paramref name="unitCost"/> from now on.</summary>
    internal void Revalue(decimal unitCost)
    {
        UnitCost = unitCost;
    }

    /// <summary>Adds an item charge (landed cost: freight, duty, insurance) to the entry's actual cost and unit cost.</summary>
    internal void AddLandedCost(decimal landedCostAmount)
    {
        CostAmountActual += landedCostAmount;
        if (Quantity != 0m)
        {
            var notInvoiced = Math.Abs(Quantity) - Math.Abs(InvoicedQuantity);
            UnitCost = Math.Round((notInvoiced * ExpectedUnitCost + Math.Abs(CostAmountActual)) / Math.Abs(Quantity), 5, MidpointRounding.AwayFromZero);
        }
    }
}
