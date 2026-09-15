using System;
using ABPmicroservice.Erp.Documents;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Sales document line. Mirrors Business Central table 37 "Sales Line".
/// Child entity of <see cref="SalesHeader"/>.
/// </summary>
public class SalesLine : Entity<Guid>
{
    public Guid SalesHeaderId { get; private set; }

    public int LineNo { get; private set; }

    public DocumentLineType Type { get; private set; }

    /// <summary>Item No. / G/L Account No. depending on <see cref="Type"/>.</summary>
    public string No { get; private set; }

    public string Description { get; private set; }

    public decimal Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineDiscountPercent { get; private set; }

    public decimal LineAmount { get; private set; }

    public decimal LineAmountIncludingVat { get; private set; }

    public string UnitOfMeasureCode { get; private set; }

    protected SalesLine() { }

    public SalesLine(
        Guid id,
        Guid salesHeaderId,
        int lineNo,
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal lineDiscountPercent = 0m,
        string unitOfMeasureCode = null
    )
        : base(id)
    {
        SalesHeaderId = salesHeaderId;
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
        SetAmounts(quantity, unitPrice, lineDiscountPercent);
    }

    public void SetAmounts(decimal quantity, decimal unitPrice, decimal lineDiscountPercent)
    {
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineDiscountPercent = lineDiscountPercent;
        RecalculateAmounts();
    }

    private void RecalculateAmounts()
    {
        var gross = Quantity * UnitPrice;
        var discount = gross * (LineDiscountPercent / 100m);
        LineAmount = Math.Round(gross - discount, 2, MidpointRounding.AwayFromZero);
        // VAT calculation is intentionally out of scope; equals line amount.
        LineAmountIncludingVat = LineAmount;
    }
}
