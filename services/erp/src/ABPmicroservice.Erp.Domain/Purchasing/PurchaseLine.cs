using System;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Purchase document line.
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

    public string LocationCode { get; private set; }

    public DateTime? ExpectedReceiptDate { get; private set; }

    public string ItemCategoryCode { get; private set; }

    public string ShortcutDimension1Code { get; private set; }

    public string ShortcutDimension2Code { get; private set; }

    public decimal QtyToReceive { get; private set; }

    public decimal QuantityReceived { get; private set; }

    public decimal QtyToInvoice { get; private set; }

    public decimal QuantityInvoiced { get; private set; }

    public string DeferralCode { get; private set; }

    public string TaxAreaCode { get; private set; }

    public bool TaxLiable { get; private set; }

    public string TaxGroupCode { get; private set; }

    public string GenBusPostingGroup { get; private set; }

    public string GenProdPostingGroup { get; private set; }

    /// <summary>Set on an invoice line made from a posted receipt line: the goods are in already.</summary>
    public Guid? ReceiptLineId { get; private set; }

    public string ReceiptNo { get; private set; }

    public int? ReceiptLineNo { get; private set; }

    /// <summary>The blanket order line an order line draws its quantity from.</summary>
    public string BlanketOrderNo { get; private set; }

    public int? BlanketOrderLineNo { get; private set; }

    /// <summary>The variant of the item; blank is the item itself.</summary>
    public string VariantCode { get; private set; }

    /// <summary>Base units in one unit of the line's unit of measure.</summary>
    public decimal QtyPerUnitOfMeasure { get; private set; } = 1m;

    public void SetVariant(string variantCode)
    {
        VariantCode = CodeTableEntity.NormalizeCode(Check.Length(variantCode, nameof(variantCode), ErpDomainConsts.MaxCodeLength));
    }

    public void SetQtyPerUnitOfMeasure(decimal qtyPerUnitOfMeasure)
    {
        QtyPerUnitOfMeasure = qtyPerUnitOfMeasure > 0m ? qtyPerUnitOfMeasure : 1m;
    }

    public decimal OutstandingQuantity => Quantity - QuantityReceived;

    public decimal QtyReceivedNotInvoiced => QuantityReceived - QuantityInvoiced;

    public bool HasPostedQuantity => QuantityReceived != 0m || QuantityInvoiced != 0m;

    public bool IsFromReceipt => ReceiptLineId.HasValue;

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
        ResetQuantitiesToPost();
        RecalculateAmounts();
    }

    /// <summary>
    /// Changes the line in place. Once part of it has been received or invoiced it keeps its item or
    /// account, cannot go below what has been posted, and once invoicing has started its cost is fixed.
    /// </summary>
    public void Update(
        string documentNo,
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal directUnitCost,
        decimal lineDiscountPercent,
        string unitOfMeasureCode
    )
    {
        if (HasPostedQuantity)
        {
            // A line taken from a receipt invoices exactly that receipt line.
            if (type != Type || !string.Equals(no, No, StringComparison.OrdinalIgnoreCase)
                || (IsFromReceipt && quantity != Quantity)
                || (QuantityInvoiced != 0m && (directUnitCost != DirectUnitCost || lineDiscountPercent != LineDiscountPercent)))
            {
                throw new BusinessException(ErpErrorCodes.Documents.LineAlreadyPosted)
                    .WithData("documentNo", documentNo)
                    .WithData("lineNo", LineNo);
            }

            var posted = Math.Max(Math.Abs(QuantityReceived), Math.Abs(QuantityInvoiced));
            if (Math.Abs(quantity) < posted)
            {
                throw new BusinessException(ErpErrorCodes.Documents.QuantityBelowPosted)
                    .WithData("documentNo", documentNo)
                    .WithData("lineNo", LineNo)
                    .WithData("quantity", posted);
            }
        }

        Type = type;
        No = Check.Length(no, nameof(no), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        UnitOfMeasureCode = Check.Length(unitOfMeasureCode, nameof(unitOfMeasureCode), ErpDomainConsts.MaxUnitOfMeasureCodeLength);
        SetAmounts(quantity, directUnitCost, lineDiscountPercent);
    }

    /// <summary>
    /// What the next posting receives and invoices. Receiving cannot go beyond the outstanding
    /// quantity, and invoicing not beyond what has been or is being received.
    /// </summary>
    public void SetQuantitiesToPost(string documentNo, decimal qtyToReceive, decimal qtyToInvoice)
    {
        if (qtyToReceive < 0m || qtyToInvoice < 0m)
        {
            throw new BusinessException(ErpErrorCodes.Documents.NegativeQuantityToPost)
                .WithData("documentNo", documentNo)
                .WithData("lineNo", LineNo);
        }

        if (IsFromReceipt)
        {
            qtyToReceive = 0m;
        }
        else if (qtyToReceive > OutstandingQuantity)
        {
            throw new BusinessException(ErpErrorCodes.Documents.QtyToShipTooLarge)
                .WithData("documentNo", documentNo)
                .WithData("lineNo", LineNo)
                .WithData("quantity", qtyToReceive)
                .WithData("outstanding", OutstandingQuantity);
        }

        var available = IsFromReceipt ? Quantity - QuantityInvoiced : QtyReceivedNotInvoiced + qtyToReceive;
        if (qtyToInvoice > available)
        {
            throw new BusinessException(ErpErrorCodes.Documents.QtyToInvoiceTooLarge)
                .WithData("documentNo", documentNo)
                .WithData("lineNo", LineNo)
                .WithData("quantity", qtyToInvoice)
                .WithData("available", available);
        }

        QtyToReceive = qtyToReceive;
        QtyToInvoice = qtyToInvoice;
    }

    /// <summary>Receive whatever is outstanding and invoice everything not invoiced yet.</summary>
    public void ResetQuantitiesToPost()
    {
        QtyToReceive = IsFromReceipt ? 0m : Quantity - QuantityReceived;
        QtyToInvoice = Quantity - QuantityInvoiced;
    }

    public void LinkToReceiptLine(Guid receiptLineId, string receiptNo, int receiptLineNo)
    {
        ReceiptLineId = receiptLineId;
        ReceiptNo = Check.Length(receiptNo, nameof(receiptNo), ErpDomainConsts.MaxDocumentNoLength);
        ReceiptLineNo = receiptLineNo;
        // The goods are in already; only the invoice is outstanding.
        QuantityReceived = Quantity;
        ResetQuantitiesToPost();
    }

    public void LinkToBlanketOrder(string blanketOrderNo, int blanketOrderLineNo)
    {
        BlanketOrderNo = Check.Length(blanketOrderNo, nameof(blanketOrderNo), ErpDomainConsts.MaxDocumentNoLength);
        BlanketOrderLineNo = blanketOrderLineNo;
    }

    internal void RecordReceived(decimal quantity)
    {
        QuantityReceived += quantity;
    }

    internal void RecordInvoiced(decimal quantity)
    {
        QuantityInvoiced += quantity;
    }

    /// <summary>Takes back a receipt that was undone.</summary>
    internal void RecordReceiptUndone(decimal quantity)
    {
        QuantityReceived -= quantity;
        ResetQuantitiesToPost();
    }

    /// <summary>The part of the line amount the quantity to invoice stands for.</summary>
    public decimal AmountToInvoice(decimal quantity) =>
        DocumentAmounts.Portion(LineAmount, Quantity, QuantityInvoiced, quantity);

    public void SetLocation(string locationCode)
    {
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
    }

    public void SetExpectedReceiptDate(DateTime? expectedReceiptDate)
    {
        ExpectedReceiptDate = expectedReceiptDate;
    }

    public void SetItemCategory(string itemCategoryCode)
    {
        ItemCategoryCode = CodeTableEntity.NormalizeCode(Check.Length(itemCategoryCode, nameof(itemCategoryCode), ErpDomainConsts.MaxPostingGroupLength));
    }

    public void SetDimensions(string shortcutDimension1Code, string shortcutDimension2Code)
    {
        ShortcutDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(shortcutDimension1Code, nameof(shortcutDimension1Code), ErpDomainConsts.MaxDimensionCodeLength));
        ShortcutDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(shortcutDimension2Code, nameof(shortcutDimension2Code), ErpDomainConsts.MaxDimensionCodeLength));
    }

    public void SetPostingGroups(string genBusPostingGroup, string genProdPostingGroup)
    {
        GenBusPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(genBusPostingGroup, nameof(genBusPostingGroup), ErpDomainConsts.MaxGeneralBusPostingGroupLength));
        GenProdPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(genProdPostingGroup, nameof(genProdPostingGroup), ErpDomainConsts.MaxGeneralBusPostingGroupLength));
    }

    public void SetTax(string taxAreaCode, bool taxLiable, string taxGroupCode)
    {
        TaxAreaCode = CodeTableEntity.NormalizeCode(Check.Length(taxAreaCode, nameof(taxAreaCode), ErpDomainConsts.MaxTaxAreaCodeLength));
        TaxLiable = taxLiable;
        TaxGroupCode = CodeTableEntity.NormalizeCode(Check.Length(taxGroupCode, nameof(taxGroupCode), ErpDomainConsts.MaxTaxGroupCodeLength));
    }

    /// <summary>The project task the purchase is used on; posting writes a project usage.</summary>
    public string ProjectNo { get; private set; }

    public string ProjectTaskNo { get; private set; }

    public decimal PrepaymentPct { get; private set; }

    public decimal PrepmtLineAmount { get; private set; }

    public decimal PrepmtAmtInv { get; private set; }

    public decimal PrepmtAmtDeducted { get; private set; }

    public void SetPrepaymentPct(decimal prepaymentPct)
    {
        PrepaymentPct = Math.Clamp(prepaymentPct, 0m, 100m);
        RecalculateAmounts();
    }

    internal void RecordPrepaymentInvoiced(decimal amount)
    {
        PrepmtAmtInv += amount;
    }

    internal void RecordPrepaymentDeducted(decimal amount)
    {
        PrepmtAmtDeducted += amount;
    }

    public decimal PrepmtAmtToDeduct(decimal quantityToInvoice)
    {
        var remaining = PrepmtAmtInv - PrepmtAmtDeducted;
        if (remaining <= 0m || quantityToInvoice <= 0m)
        {
            return 0m;
        }

        if (quantityToInvoice >= Quantity - QuantityInvoiced)
        {
            return remaining;
        }

        var portion = DocumentAmounts.Portion(PrepmtAmtInv, Quantity, QuantityInvoiced, quantityToInvoice);
        return Math.Min(remaining, Math.Max(0m, portion));
    }

    public void SetProject(string projectNo, string projectTaskNo)
    {
        ProjectNo = CodeTableEntity.NormalizeCode(Check.Length(projectNo, nameof(projectNo), ErpDomainConsts.MaxNoLength));
        ProjectTaskNo = ProjectNo == null ? null : CodeTableEntity.NormalizeCode(Check.Length(projectTaskNo, nameof(projectTaskNo), ErpDomainConsts.MaxNoLength));
    }

    public void SetDeferralCode(string deferralCode)
    {
        DeferralCode = CodeTableEntity.NormalizeCode(Check.Length(deferralCode, nameof(deferralCode), ErpDomainConsts.MaxDeferralTemplateCodeLength));
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
        PrepmtLineAmount = Math.Round(LineAmount * (PrepaymentPct / 100m), 2, MidpointRounding.AwayFromZero);

        // Without a VAT setup the rate is zero, which is the same as no VAT.
        var vat = LineVat.Calculate(LineAmount, VatCalculationType, VatPercent);
        VatBaseAmount = vat.Base;
        VatAmount = vat.Amount;
        LineAmountIncludingVat = vat.AmountIncludingVat;
    }
}
