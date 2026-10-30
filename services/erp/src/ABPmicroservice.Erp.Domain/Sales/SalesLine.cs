using System;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Sales document line.
/// Child entity of <see cref="SalesHeader"/>.
/// <para>
/// On an order the quantity is shipped and invoiced in parts: <see cref="QtyToShip"/> and
/// <see cref="QtyToInvoice"/> say what the next posting does, <see cref="QuantityShipped"/> and
/// <see cref="QuantityInvoiced"/> what has been done. On a return order "ship" means receiving the
/// goods back. An invoice or credit memo ships and invoices the whole quantity in one go.
/// </para>
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

    public string VatBusPostingGroup { get; private set; }

    public string VatProdPostingGroup { get; private set; }

    public VatCalculationType VatCalculationType { get; private set; }

    public string VatIdentifier { get; private set; }

    public decimal VatPercent { get; private set; }

    /// <summary>The amount VAT is calculated on; zero for Full VAT.</summary>
    public decimal VatBaseAmount { get; private set; }

    /// <summary>The VAT of the line, also for reverse charge where the party does not pay it.</summary>
    public decimal VatAmount { get; private set; }

    /// <summary>Where the stock of this line ships from; blank takes the header's location.</summary>
    public string LocationCode { get; private set; }

    public DateTime? ShipmentDate { get; private set; }

    public decimal QtyToShip { get; private set; }

    public decimal QuantityShipped { get; private set; }

    public decimal QtyToInvoice { get; private set; }

    public decimal QuantityInvoiced { get; private set; }

    /// <summary>Set on an invoice line made from a posted shipment line: the goods have already left.</summary>
    public Guid? ShipmentLineId { get; private set; }

    public string ShipmentNo { get; private set; }

    public int? ShipmentLineNo { get; private set; }

    /// <summary>The blanket order line an order line draws its quantity from.</summary>
    public string BlanketOrderNo { get; private set; }

    public int? BlanketOrderLineNo { get; private set; }

    /// <summary>The variant of the item; blank is the item itself.</summary>
    public string VariantCode { get; private set; }

    /// <summary>Base units in one unit of the line's unit of measure.</summary>
    public decimal QtyPerUnitOfMeasure { get; private set; } = 1m;

    /// <summary>Spreads the revenue of the line over later periods.</summary>
    public string DeferralCode { get; private set; }

    /// <summary>The project task the line bills; posting writes a project sale.</summary>
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

    /// <summary>The goods go from the vendor straight to the customer; the line ships when its purchase order line is received.</summary>
    public bool DropShipment { get; private set; }

    public string PurchaseOrderNo { get; private set; }

    public Guid? PurchaseOrderLineId { get; private set; }

    public void SetDropShipment(string documentNo, bool dropShipment)
    {
        if (dropShipment == DropShipment)
        {
            return;
        }

        if (PurchaseOrderLineId.HasValue || QuantityShipped != 0m || Type != Documents.DocumentLineType.Item)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.DropShipmentLinked)
                .WithData("documentNo", documentNo)
                .WithData("lineNo", LineNo)
                .WithData("purchaseOrderNo", PurchaseOrderNo ?? "");
        }

        DropShipment = dropShipment;
    }

    public void LinkToPurchaseOrder(string purchaseOrderNo, Guid? purchaseOrderLineId)
    {
        PurchaseOrderNo = purchaseOrderLineId.HasValue ? purchaseOrderNo : null;
        PurchaseOrderLineId = purchaseOrderLineId;
    }

    public void SetDeferralCode(string deferralCode)
    {
        DeferralCode = CodeTableEntity.NormalizeCode(Check.Length(deferralCode, nameof(deferralCode), ErpDomainConsts.MaxDeferralTemplateCodeLength));
    }

    public void SetVariant(string variantCode)
    {
        VariantCode = CodeTableEntity.NormalizeCode(Check.Length(variantCode, nameof(variantCode), ErpDomainConsts.MaxCodeLength));
    }

    public void SetQtyPerUnitOfMeasure(decimal qtyPerUnitOfMeasure)
    {
        QtyPerUnitOfMeasure = qtyPerUnitOfMeasure > 0m ? qtyPerUnitOfMeasure : 1m;
    }

    public decimal OutstandingQuantity => Quantity - QuantityShipped;

    public decimal QtyShippedNotInvoiced => QuantityShipped - QuantityInvoiced;

    public bool HasPostedQuantity => QuantityShipped != 0m || QuantityInvoiced != 0m;

    public bool IsFromShipment => ShipmentLineId.HasValue;

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
        ResetQuantitiesToPost();
        RecalculateAmounts();
    }

    /// <summary>
    /// Changes the line in place. Once part of it has been shipped or invoiced it keeps its item or
    /// account, cannot go below what has been posted, and once invoicing has started its price is fixed.
    /// </summary>
    public void Update(
        string documentNo,
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal lineDiscountPercent,
        string unitOfMeasureCode
    )
    {
        if (HasPostedQuantity)
        {
            // A line taken from a shipment invoices exactly that shipment line.
            if (type != Type || !string.Equals(no, No, StringComparison.OrdinalIgnoreCase)
                || (IsFromShipment && quantity != Quantity)
                || (QuantityInvoiced != 0m && (unitPrice != UnitPrice || lineDiscountPercent != LineDiscountPercent)))
            {
                throw new BusinessException(ErpErrorCodes.Documents.LineAlreadyPosted)
                    .WithData("documentNo", documentNo)
                    .WithData("lineNo", LineNo);
            }

            var posted = Math.Max(Math.Abs(QuantityShipped), Math.Abs(QuantityInvoiced));
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
        SetAmounts(quantity, unitPrice, lineDiscountPercent);
    }

    public void SetLocation(string locationCode)
    {
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
    }

    public void SetShipmentDate(DateTime? shipmentDate)
    {
        ShipmentDate = shipmentDate?.Date;
    }

    /// <summary>
    /// What the next posting ships and invoices. Shipping cannot go beyond the outstanding
    /// quantity, and invoicing not beyond what has been or is being shipped.
    /// </summary>
    public void SetQuantitiesToPost(string documentNo, decimal qtyToShip, decimal qtyToInvoice)
    {
        if (qtyToShip < 0m || qtyToInvoice < 0m)
        {
            throw new BusinessException(ErpErrorCodes.Documents.NegativeQuantityToPost)
                .WithData("documentNo", documentNo)
                .WithData("lineNo", LineNo);
        }

        if (IsFromShipment)
        {
            qtyToShip = 0m;
        }
        else if (qtyToShip > OutstandingQuantity)
        {
            throw new BusinessException(ErpErrorCodes.Documents.QtyToShipTooLarge)
                .WithData("documentNo", documentNo)
                .WithData("lineNo", LineNo)
                .WithData("quantity", qtyToShip)
                .WithData("outstanding", OutstandingQuantity);
        }

        var available = IsFromShipment ? Quantity - QuantityInvoiced : QtyShippedNotInvoiced + qtyToShip;
        if (qtyToInvoice > available)
        {
            throw new BusinessException(ErpErrorCodes.Documents.QtyToInvoiceTooLarge)
                .WithData("documentNo", documentNo)
                .WithData("lineNo", LineNo)
                .WithData("quantity", qtyToInvoice)
                .WithData("available", available);
        }

        QtyToShip = qtyToShip;
        QtyToInvoice = qtyToInvoice;
    }

    /// <summary>Ship whatever is outstanding and invoice everything not invoiced yet.</summary>
    public void ResetQuantitiesToPost()
    {
        QtyToShip = IsFromShipment ? 0m : Quantity - QuantityShipped;
        QtyToInvoice = Quantity - QuantityInvoiced;
    }

    public void LinkToShipmentLine(Guid shipmentLineId, string shipmentNo, int shipmentLineNo)
    {
        ShipmentLineId = shipmentLineId;
        ShipmentNo = Check.Length(shipmentNo, nameof(shipmentNo), ErpDomainConsts.MaxDocumentNoLength);
        ShipmentLineNo = shipmentLineNo;
        // The goods have left already; only the invoice is outstanding.
        QuantityShipped = Quantity;
        ResetQuantitiesToPost();
    }

    public void LinkToBlanketOrder(string blanketOrderNo, int blanketOrderLineNo)
    {
        BlanketOrderNo = Check.Length(blanketOrderNo, nameof(blanketOrderNo), ErpDomainConsts.MaxDocumentNoLength);
        BlanketOrderLineNo = blanketOrderLineNo;
    }

    internal void RecordShipped(decimal quantity)
    {
        QuantityShipped += quantity;
    }

    internal void RecordInvoiced(decimal quantity)
    {
        QuantityInvoiced += quantity;
    }

    /// <summary>Takes back a shipment that was undone.</summary>
    internal void RecordShipmentUndone(decimal quantity)
    {
        QuantityShipped -= quantity;
        ResetQuantitiesToPost();
    }

    /// <summary>The part of the line amount the quantity to invoice stands for.</summary>
    public decimal AmountToInvoice(decimal quantity) =>
        DocumentAmounts.Portion(LineAmount, Quantity, QuantityInvoiced, quantity);

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
        var gross = Quantity * UnitPrice;
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
