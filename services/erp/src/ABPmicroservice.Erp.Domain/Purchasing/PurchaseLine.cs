using System;
using ABPmicroservice.Erp.Documents;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Purchase document line. Mirrors Business Central table 39 "Purchase Line".
/// Child entity of <see cref="PurchaseHeader"/>.
/// </summary>
public class PurchaseLine : Entity<Guid>
{
    public Guid PurchaseHeaderId { get; private set; }

    public int LineNo { get; private set; }

    public DocumentLineType Type { get; private set; }

    /// <summary>Item No. / G/L Account No. depending on <see cref="Type"/>.</summary>
    public string No { get; private set; }

    public string Description { get; private set; }

    public decimal Quantity { get; private set; }

    /// <summary>Mirrors BC field "Direct Unit Cost".</summary>
    public decimal DirectUnitCost { get; private set; }

    public decimal LineDiscountPercent { get; private set; }

    public decimal LineAmount { get; private set; }

    public decimal LineAmountIncludingVat { get; private set; }

    public string UnitOfMeasureCode { get; private set; }

    protected PurchaseLine() { }

    public PurchaseLine(
        Guid id,
        Guid purchaseHeaderId,
        int lineNo,
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal directUnitCost,
        decimal lineDiscountPercent = 0m,
        string unitOfMeasureCode = null
    )
        : base(id)
    {
        PurchaseHeaderId = purchaseHeaderId;
        LineNo = lineNo;
        Type = type;
        No = Check.Length(no, nameof(no), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(
            description,
            nameof(description),
            ErpDomainConsts.MaxDescriptionLength
        );
        UnitOfMeasureCode = Check.Length(
            unitOfMeasureCode,
            nameof(unitOfMeasureCode),
            ErpDomainConsts.MaxUnitOfMeasureCodeLength
        );
        SetAmounts(quantity, directUnitCost, lineDiscountPercent);
    }

    public void SetAmounts(decimal quantity, decimal directUnitCost, decimal lineDiscountPercent)
    {
        Quantity = quantity;
        DirectUnitCost = directUnitCost;
        LineDiscountPercent = lineDiscountPercent;
        RecalculateAmounts();
    }

    private void RecalculateAmounts()
    {
        var gross = Quantity * DirectUnitCost;
        var discount = gross * (LineDiscountPercent / 100m);
        LineAmount = Math.Round(gross - discount, 2, MidpointRounding.AwayFromZero);
        LineAmountIncludingVat = LineAmount;
    }
}
