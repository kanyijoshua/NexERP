using System;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
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

    public string VatBusPostingGroup { get; private set; }

    public string VatProdPostingGroup { get; private set; }

    public VatCalculationType VatCalculationType { get; private set; }

    public string VatIdentifier { get; private set; }

    public decimal VatPercent { get; private set; }

    /// <summary>The amount VAT is calculated on; zero for Full VAT.</summary>
    public decimal VatBaseAmount { get; private set; }

    /// <summary>The VAT of the line, also for reverse charge where the party does not pay it.</summary>
    public decimal VatAmount { get; private set; }

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

    /// <summary>Takes the rate and groups of a VAT Posting Setup, or none with a null setup.</summary>
    public void SetVat(VatPostingSetup setup)
    {
        VatBusPostingGroup = setup?.VatBusPostingGroup;
        VatProdPostingGroup = setup?.VatProdPostingGroup;
        VatCalculationType = setup?.VatCalculationType ?? VatCalculationType.NormalVat;
        VatIdentifier = setup?.VatIdentifier;
        VatPercent = setup?.VatPercent ?? 0m;
        RecalculateAmounts();
    }

    private void RecalculateAmounts()
    {
        var gross = Quantity * DirectUnitCost;
        var discount = gross * (LineDiscountPercent / 100m);
        LineAmount = Math.Round(gross - discount, 2, MidpointRounding.AwayFromZero);

        // Without a VAT setup the rate is zero, which is the same as no VAT.
        var vat = LineVat.Calculate(LineAmount, VatCalculationType, VatPercent);
        VatBaseAmount = vat.Base;
        VatAmount = vat.Amount;
        LineAmountIncludingVat = vat.AmountIncludingVat;
    }
}
