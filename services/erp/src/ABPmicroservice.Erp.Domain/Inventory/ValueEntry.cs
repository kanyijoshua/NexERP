using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Value Entry.
/// Tracks monetary valuation changes and unit cost calculations for item transactions.
/// Cost amounts carry the sign of the stock movement: inbound adds value, outbound removes it.
/// </summary>
public class ValueEntry : LedgerEntryBase
{
    public Guid ItemLedgerEntryId { get; private set; }
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string DocumentNo { get; private set; }
    public decimal ValuationQuantity { get; private set; }

    /// <summary>The part of the movement this entry invoices; zero on a shipment or receipt.</summary>
    public decimal InvoicedQuantity { get; private set; }

    public decimal CostAmountExpected { get; private set; }
    public decimal CostAmountActual { get; private set; }
    public Guid DimensionSetId { get; private set; }

    public ValueEntryType EntryType { get; private set; }
    public string LocationCode { get; private set; }
    public string VariantCode { get; private set; }

    /// <summary>How much of the actual cost is in the G/L; "Post Inventory Cost to G/L" posts the rest.</summary>
    public decimal CostPostedToGL { get; private set; }

    /// <summary>Made by the cost adjustment, which re-costs stock that left before its cost changed.</summary>
    public bool Adjustment { get; private set; }

    protected ValueEntry() { }

    public ValueEntry(
        Guid id,
        Guid itemLedgerEntryId,
        Guid itemId,
        string itemNo,
        DateTime postingDate,
        string documentNo,
        decimal valuationQuantity,
        decimal costAmountActual,
        Guid dimensionSetId = default,
        decimal costAmountExpected = 0m,
        decimal? invoicedQuantity = null
    )
        : base(id)
    {
        ItemLedgerEntryId = itemLedgerEntryId;
        ItemId = itemId;
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
        PostingDate = postingDate;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        ValuationQuantity = valuationQuantity;
        InvoicedQuantity = invoicedQuantity ?? valuationQuantity;
        CostAmountActual = costAmountActual;
        CostAmountExpected = costAmountExpected;
        DimensionSetId = dimensionSetId;
    }

    internal ValueEntry WithDetails(ValueEntryType entryType, string locationCode, string variantCode)
    {
        EntryType = entryType;
        LocationCode = locationCode;
        VariantCode = variantCode;
        return this;
    }

    /// <summary>Whether whoever made the entry posts its cost to the G/L at the same time.</summary>
    internal ValueEntry PostedToGL(bool posted)
    {
        CostPostedToGL = posted ? CostAmountActual : 0m;
        return this;
    }

    internal ValueEntry AsAdjustment()
    {
        Adjustment = true;
        return this;
    }

    internal void MarkCostPostedToGL() => CostPostedToGL = CostAmountActual;
}
