using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Value Entry. Mirrors Business Central table 5802 "Value Entry".
/// Tracks monetary valuation changes and unit cost calculations for item transactions.
/// </summary>
public class ValueEntry : LedgerEntryBase
{
    public Guid ItemLedgerEntryId { get; private set; }
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string DocumentNo { get; private set; }
    public decimal ValuationQuantity { get; private set; }
    public decimal CostAmountExpected { get; private set; }
    public decimal CostAmountActual { get; private set; }
    public Guid DimensionSetId { get; private set; }

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
        Guid dimensionSetId = default
    )
        : base(id)
    {
        ItemLedgerEntryId = itemLedgerEntryId;
        ItemId = itemId;
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
        PostingDate = postingDate;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        ValuationQuantity = valuationQuantity;
        CostAmountActual = costAmountActual;
        CostAmountExpected = 0m;
        DimensionSetId = dimensionSetId;
    }
}
