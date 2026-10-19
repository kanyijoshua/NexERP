using System;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Transfer Order: moves stock from one location to another through an in-transit location.
/// Shipping takes it out of the from-location into transit; receiving takes it out of transit into
/// the to-location. Both can be done in parts.
/// </summary>
public class TransferHeader : CompanyAggregateRoot
{
    public string No { get; private set; }
    public string FromLocationCode { get; private set; }
    public string ToLocationCode { get; private set; }
    public string InTransitCode { get; private set; }
    public DateTime PostingDate { get; private set; }
    public DateTime? ShipmentDate { get; private set; }
    public DateTime? ReceiptDate { get; private set; }
    public TransferStatus Status { get; private set; }

    /// <summary>Everything has been received; the order takes no further postings.</summary>
    public bool Completed { get; private set; }

    public string LastShipmentNo { get; private set; }
    public string LastReceiptNo { get; private set; }

    public Collection<TransferLine> Lines { get; private set; }

    protected TransferHeader()
    {
        Lines = new Collection<TransferLine>();
    }

    public TransferHeader(Guid id, string no, DateTime postingDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength);
        PostingDate = postingDate;
        Status = TransferStatus.Open;
        Lines = new Collection<TransferLine>();
    }

    public void SetLocations(string fromLocationCode, string toLocationCode, string inTransitCode)
    {
        EnsureOpen();
        FromLocationCode = Code(fromLocationCode, nameof(fromLocationCode));
        ToLocationCode = Code(toLocationCode, nameof(toLocationCode));
        InTransitCode = Code(inTransitCode, nameof(inTransitCode));
    }

    public void SetDates(DateTime postingDate, DateTime? shipmentDate, DateTime? receiptDate)
    {
        PostingDate = postingDate;
        ShipmentDate = shipmentDate?.Date;
        ReceiptDate = receiptDate?.Date;
    }

    public TransferLine AddLine(Guid lineId, Item item, string variantCode, string description, string unitOfMeasureCode, decimal qtyPerUnitOfMeasure, decimal quantity)
    {
        EnsureOpen();
        var line = new TransferLine(lineId, Id, Lines.Count == 0 ? 10000 : Lines.Max(l => l.LineNo) + 10000, item.Id, item.No);
        line.Update(No, variantCode, description ?? item.Description, unitOfMeasureCode ?? item.BaseUnitOfMeasureCode, qtyPerUnitOfMeasure, quantity);
        Lines.Add(line);
        return line;
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureOpen();
        var line = Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
        {
            return;
        }
        if (line.QuantityShipped != 0m)
        {
            throw new BusinessException(ErpErrorCodes.Documents.LineAlreadyPosted).WithData("documentNo", No).WithData("lineNo", line.LineNo);
        }
        Lines.Remove(line);
    }

    public TransferLine GetLine(Guid lineId) =>
        Lines.FirstOrDefault(l => l.Id == lineId)
        ?? throw new BusinessException(ErpErrorCodes.Documents.LineNotFound).WithData("documentNo", No).WithData("lineId", lineId);

    public void Release()
    {
        Status = TransferStatus.Released;
    }

    public void Reopen()
    {
        Status = TransferStatus.Open;
    }

    internal void RecordShipment(string shipmentNo)
    {
        LastShipmentNo = shipmentNo;
        foreach (var line in Lines)
        {
            line.ResetQuantitiesToPost();
        }
    }

    internal void RecordReceipt(string receiptNo)
    {
        LastReceiptNo = receiptNo;
        foreach (var line in Lines)
        {
            line.ResetQuantitiesToPost();
        }
        Completed = Lines.All(l => l.QuantityReceived == l.Quantity);
    }

    public void EnsureOpen()
    {
        if (Status != TransferStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNotReleased).WithData("documentNo", No);
        }
    }

    private static string Code(string value, string name) =>
        CodeTableEntity.NormalizeCode(Check.Length(value, name, ErpDomainConsts.MaxLocationCodeLength));
}

/// <summary>One item moved by a transfer order, with what has been shipped and received of it.</summary>
public class TransferLine : Entity<Guid>
{
    public Guid TransferHeaderId { get; private set; }
    public int LineNo { get; private set; }
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public string VariantCode { get; private set; }
    public string Description { get; private set; }
    public string UnitOfMeasureCode { get; private set; }
    public decimal QtyPerUnitOfMeasure { get; private set; } = 1m;
    public decimal Quantity { get; private set; }
    public decimal QtyToShip { get; private set; }
    public decimal QuantityShipped { get; private set; }
    public decimal QtyToReceive { get; private set; }
    public decimal QuantityReceived { get; private set; }

    public decimal OutstandingQuantity => Quantity - QuantityShipped;
    public decimal QtyInTransit => QuantityShipped - QuantityReceived;

    protected TransferLine() { }

    public TransferLine(Guid id, Guid transferHeaderId, int lineNo, Guid itemId, string itemNo)
        : base(id)
    {
        TransferHeaderId = transferHeaderId;
        LineNo = lineNo;
        ItemId = itemId;
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
    }

    public void Update(string documentNo, string variantCode, string description, string unitOfMeasureCode, decimal qtyPerUnitOfMeasure, decimal quantity)
    {
        if (quantity < QuantityShipped)
        {
            throw new BusinessException(ErpErrorCodes.Documents.QuantityBelowPosted)
                .WithData("documentNo", documentNo)
                .WithData("lineNo", LineNo)
                .WithData("quantity", QuantityShipped);
        }

        VariantCode = CodeTableEntity.NormalizeCode(Check.Length(variantCode, nameof(variantCode), ErpDomainConsts.MaxCodeLength));
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        UnitOfMeasureCode = CodeTableEntity.NormalizeCode(Check.Length(unitOfMeasureCode, nameof(unitOfMeasureCode), ErpDomainConsts.MaxUnitOfMeasureCodeLength));
        QtyPerUnitOfMeasure = qtyPerUnitOfMeasure > 0m ? qtyPerUnitOfMeasure : 1m;
        Quantity = quantity;
        ResetQuantitiesToPost();
    }

    public void SetQuantitiesToPost(string documentNo, decimal qtyToShip, decimal qtyToReceive)
    {
        if (qtyToShip < 0m || qtyToReceive < 0m)
        {
            throw new BusinessException(ErpErrorCodes.Documents.NegativeQuantityToPost).WithData("documentNo", documentNo).WithData("lineNo", LineNo);
        }
        if (qtyToShip > OutstandingQuantity)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.TransferQuantityTooLarge)
                .WithData("documentNo", documentNo).WithData("lineNo", LineNo).WithData("quantity", qtyToShip).WithData("outstanding", OutstandingQuantity);
        }
        if (qtyToReceive > QtyInTransit)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.TransferQuantityTooLarge)
                .WithData("documentNo", documentNo).WithData("lineNo", LineNo).WithData("quantity", qtyToReceive).WithData("outstanding", QtyInTransit);
        }

        QtyToShip = qtyToShip;
        QtyToReceive = qtyToReceive;
    }

    /// <summary>Ship whatever is outstanding; receive whatever is in transit.</summary>
    public void ResetQuantitiesToPost()
    {
        QtyToShip = OutstandingQuantity;
        QtyToReceive = QtyInTransit;
    }

    internal void RecordShipped(decimal quantity) => QuantityShipped += quantity;

    internal void RecordReceived(decimal quantity) => QuantityReceived += quantity;
}

/// <summary>A posted transfer shipment or, with <see cref="Receipt"/> set, a posted transfer receipt.</summary>
public class PostedTransfer : CompanyBasicEntity
{
    public string No { get; private set; }
    public bool Receipt { get; private set; }
    public string TransferOrderNo { get; private set; }
    public string FromLocationCode { get; private set; }
    public string ToLocationCode { get; private set; }
    public string InTransitCode { get; private set; }
    public DateTime PostingDate { get; private set; }
    public Collection<PostedTransferLine> Lines { get; private set; }

    protected PostedTransfer()
    {
        Lines = new Collection<PostedTransferLine>();
    }

    public PostedTransfer(Guid id, string no, bool receipt, TransferHeader order)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength);
        Receipt = receipt;
        TransferOrderNo = order.No;
        FromLocationCode = order.FromLocationCode;
        ToLocationCode = order.ToLocationCode;
        InTransitCode = order.InTransitCode;
        PostingDate = order.PostingDate;
        Lines = new Collection<PostedTransferLine>();
    }
}

public class PostedTransferLine : Entity<Guid>
{
    public Guid PostedTransferId { get; private set; }
    public int LineNo { get; private set; }
    public Guid TransferLineId { get; private set; }
    public string ItemNo { get; private set; }
    public string VariantCode { get; private set; }
    public string Description { get; private set; }
    public string UnitOfMeasureCode { get; private set; }
    public decimal Quantity { get; private set; }

    protected PostedTransferLine() { }

    public PostedTransferLine(Guid id, Guid postedTransferId, TransferLine line, decimal quantity)
        : base(id)
    {
        PostedTransferId = postedTransferId;
        LineNo = line.LineNo;
        TransferLineId = line.Id;
        ItemNo = line.ItemNo;
        VariantCode = line.VariantCode;
        Description = line.Description;
        UnitOfMeasureCode = line.UnitOfMeasureCode;
        Quantity = quantity;
    }
}
