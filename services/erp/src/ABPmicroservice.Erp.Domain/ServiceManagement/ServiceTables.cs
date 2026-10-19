using System;
using System.Collections.Generic;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.ServiceManagement;

/// <summary>Service Management Setup: the number series and the accounts service and contract sales go to.</summary>
public class ServiceMgtSetup : CompanyEntity
{
    public string ServiceOrderNos { get; private set; }
    public string ServiceItemNos { get; private set; }
    public string ServiceContractNos { get; private set; }

    /// <summary>Labour (resource lines) is sold on this account.</summary>
    public string ServiceRevenueAccountNo { get; private set; }

    /// <summary>Contract periods are invoiced on this account.</summary>
    public string ContractRevenueAccountNo { get; private set; }

    /// <summary>How long a new service item is under warranty when no end date is given.</summary>
    public int DefaultWarrantyMonths { get; private set; } = 12;

    protected ServiceMgtSetup() { }

    public ServiceMgtSetup(Guid id)
        : base(id) { }

    public void Set(string serviceOrderNos, string serviceItemNos, string serviceContractNos, string serviceRevenueAccountNo, string contractRevenueAccountNo, int defaultWarrantyMonths)
    {
        ServiceOrderNos = Series(serviceOrderNos, nameof(serviceOrderNos));
        ServiceItemNos = Series(serviceItemNos, nameof(serviceItemNos));
        ServiceContractNos = Series(serviceContractNos, nameof(serviceContractNos));
        ServiceRevenueAccountNo = Account(serviceRevenueAccountNo, nameof(serviceRevenueAccountNo));
        ContractRevenueAccountNo = Account(contractRevenueAccountNo, nameof(contractRevenueAccountNo));
        DefaultWarrantyMonths = Math.Max(0, defaultWarrantyMonths);
    }

    private static string Series(string code, string name) =>
        code.IsNullOrWhiteSpace() ? null : Check.Length(code.Trim(), name, ErpDomainConsts.MaxNoSeriesCodeLength);

    private static string Account(string no, string name) =>
        no.IsNullOrWhiteSpace() ? null : Check.Length(no.Trim(), name, ErpDomainConsts.MaxNoLength);
}

/// <summary>Service Order Type: maintenance, repair, installation and the like.</summary>
public class ServiceOrderType : CodeTableEntity
{
    protected ServiceOrderType() { }

    public ServiceOrderType(Guid id, string code, string description)
        : base(id, code, description) { }
}

/// <summary>Fault Code: what was wrong with a service item.</summary>
public class FaultCode : CodeTableEntity
{
    protected FaultCode() { }

    public FaultCode(Guid id, string code, string description)
        : base(id, code, description) { }
}

/// <summary>Resolution Code: what was done about it.</summary>
public class ResolutionCode : CodeTableEntity
{
    protected ResolutionCode() { }

    public ResolutionCode(Guid id, string code, string description)
        : base(id, code, description) { }
}

/// <summary>Service Item: a customer's piece of equipment that is serviced, with its serial number and warranty.</summary>
public class ServiceItem : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Description { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerNo { get; private set; }
    public string CustomerName { get; private set; }

    /// <summary>The item it is, when it was sold from stock.</summary>
    public string ItemNo { get; private set; }

    public string SerialNo { get; private set; }
    public DateTime? InstallationDate { get; private set; }
    public DateTime? WarrantyStartingDate { get; private set; }
    public DateTime? WarrantyEndingDate { get; private set; }
    public bool Blocked { get; private set; }

    protected ServiceItem() { }

    public ServiceItem(Guid id, string no)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
    }

    public void Set(string description, Guid customerId, string customerNo, string customerName, string itemNo, string serialNo, DateTime? installationDate, bool blocked)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        CustomerId = customerId;
        CustomerNo = customerNo;
        CustomerName = customerName;
        ItemNo = itemNo.IsNullOrWhiteSpace() ? null : itemNo.Trim();
        SerialNo = serialNo.IsNullOrWhiteSpace() ? null : Check.Length(serialNo.Trim(), nameof(serialNo), ErpDomainConsts.MaxNoLength);
        InstallationDate = installationDate?.Date;
        Blocked = blocked;
    }

    public void SetWarranty(DateTime? startingDate, DateTime? endingDate)
    {
        if (startingDate.HasValue && endingDate.HasValue && endingDate.Value.Date < startingDate.Value.Date)
        {
            throw new BusinessException(ErpErrorCodes.Service.WarrantyDates).WithData("no", No);
        }

        WarrantyStartingDate = startingDate?.Date;
        WarrantyEndingDate = endingDate?.Date;
    }

    /// <summary>Whether work on the item on a date is covered by its warranty.</summary>
    public bool IsUnderWarranty(DateTime date) =>
        WarrantyStartingDate.HasValue && date.Date >= WarrantyStartingDate.Value && (!WarrantyEndingDate.HasValue || date.Date <= WarrantyEndingDate.Value);
}

/// <summary>Service Contract: a customer pays a yearly amount, invoiced every period, for servicing its items.</summary>
public class ServiceContract : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Description { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerNo { get; private set; }
    public string CustomerName { get; private set; }
    public ServiceContractStatus Status { get; private set; }
    public DateTime StartingDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public ServiceInvoicePeriod InvoicePeriod { get; private set; } = ServiceInvoicePeriod.Month;

    /// <summary>The first day of the next period to invoice; set when the contract is signed.</summary>
    public DateTime? NextInvoiceDate { get; private set; }

    public DateTime? LastInvoiceDate { get; private set; }
    public string LastInvoiceNo { get; private set; }

    public List<ServiceContractLine> Lines { get; private set; } = new();

    /// <summary>The yearly amount: the sum of the line values.</summary>
    public decimal AnnualAmount => Lines.Sum(l => l.LineValue);

    protected ServiceContract() { }

    public ServiceContract(Guid id, string no)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
    }

    public void Set(string description, Guid customerId, string customerNo, string customerName, DateTime startingDate, DateTime? expirationDate, ServiceInvoicePeriod invoicePeriod)
    {
        EnsureDraft();
        if (expirationDate.HasValue && expirationDate.Value.Date < startingDate.Date)
        {
            throw new BusinessException(ErpErrorCodes.Service.ContractDates).WithData("no", No);
        }

        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        CustomerId = customerId;
        CustomerNo = customerNo;
        CustomerName = customerName;
        StartingDate = startingDate.Date;
        ExpirationDate = expirationDate?.Date;
        InvoicePeriod = Enum.IsDefined(invoicePeriod) ? invoicePeriod : ServiceInvoicePeriod.Month;
    }

    public ServiceContractLine AddLine(Guid id, string serviceItemNo, string description, decimal lineValue)
    {
        EnsureDraft();
        var line = new ServiceContractLine(id, Id, Lines.Count == 0 ? 10000 : Lines.Max(l => l.LineNo) + 10000);
        line.Set(serviceItemNo, description, lineValue);
        Lines.Add(line);
        return line;
    }

    public void EnsureDraft()
    {
        if (Status != ServiceContractStatus.Draft)
        {
            throw new BusinessException(ErpErrorCodes.Service.ContractNotDraft).WithData("no", No).WithData("status", Status.ToString());
        }
    }

    /// <summary>Signing starts the invoicing: the first period begins on the starting date.</summary>
    public void Sign()
    {
        EnsureDraft();
        if (Lines.Count == 0 || AnnualAmount <= 0m)
        {
            throw new BusinessException(ErpErrorCodes.Service.ContractEmpty).WithData("no", No);
        }

        Status = ServiceContractStatus.Signed;
        NextInvoiceDate = StartingDate;
    }

    public void Cancel()
    {
        if (Status == ServiceContractStatus.Cancelled)
        {
            return;
        }

        Status = ServiceContractStatus.Cancelled;
        NextInvoiceDate = null;
    }

    /// <summary>Whether the period starting at <see cref="NextInvoiceDate"/> is due on a date.</summary>
    public bool IsDue(DateTime date) =>
        Status == ServiceContractStatus.Signed
        && NextInvoiceDate.HasValue
        && NextInvoiceDate.Value <= date.Date
        && (!ExpirationDate.HasValue || NextInvoiceDate.Value <= ExpirationDate.Value);

    /// <summary>The amount of one period: the yearly amount spread over the year's periods.</summary>
    public decimal PeriodAmount => Math.Round(AnnualAmount * (int)InvoicePeriod / 12m, 2, MidpointRounding.AwayFromZero);

    internal void RecordInvoiced(string invoiceNo)
    {
        LastInvoiceDate = NextInvoiceDate;
        LastInvoiceNo = invoiceNo;
        NextInvoiceDate = NextInvoiceDate!.Value.AddMonths((int)InvoicePeriod);
    }
}

public class ServiceContractLine : Entity<Guid>
{
    public Guid ServiceContractId { get; private set; }
    public int LineNo { get; private set; }
    public string ServiceItemNo { get; private set; }
    public string Description { get; private set; }

    /// <summary>What the line adds to the contract's yearly amount.</summary>
    public decimal LineValue { get; private set; }

    protected ServiceContractLine() { }

    public ServiceContractLine(Guid id, Guid contractId, int lineNo)
        : base(id)
    {
        ServiceContractId = contractId;
        LineNo = lineNo;
    }

    public void Set(string serviceItemNo, string description, decimal lineValue)
    {
        ServiceItemNo = serviceItemNo.IsNullOrWhiteSpace() ? null : serviceItemNo.Trim().ToUpperInvariant();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        LineValue = Math.Max(0m, lineValue);
    }
}

/// <summary>Service Order: work on one or more of a customer's service items, and the parts, labour and costs it took.</summary>
public class ServiceOrder : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Description { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerNo { get; private set; }
    public string CustomerName { get; private set; }
    public string ServiceOrderType { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    public ServicePriority Priority { get; private set; } = ServicePriority.Medium;
    public DateTime OrderDate { get; private set; }

    /// <summary>The technician: a resource that does the work.</summary>
    public string ResourceNo { get; private set; }

    public string ContractNo { get; private set; }
    public DateTime? FinishingDate { get; private set; }
    public string LastInvoiceNo { get; private set; }

    public List<ServiceItemLine> ItemLines { get; private set; } = new();
    public List<ServiceLine> Lines { get; private set; } = new();

    protected ServiceOrder() { }

    public ServiceOrder(Guid id, string no)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
    }

    public void Set(string description, Guid customerId, string customerNo, string customerName, string orderType, ServicePriority priority, DateTime orderDate, string resourceNo, string contractNo)
    {
        EnsureNotFinished();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        CustomerId = customerId;
        CustomerNo = customerNo;
        CustomerName = customerName;
        ServiceOrderType = CodeTableEntity.NormalizeCode(orderType);
        Priority = priority;
        OrderDate = orderDate.Date;
        ResourceNo = resourceNo.IsNullOrWhiteSpace() ? null : resourceNo.Trim().ToUpperInvariant();
        ContractNo = contractNo.IsNullOrWhiteSpace() ? null : contractNo.Trim().ToUpperInvariant();
    }

    public void SetStatus(ServiceOrderStatus status)
    {
        EnsureNotFinished();
        if (status == ServiceOrderStatus.Finished)
        {
            // Finishing is what posting does once every line is posted.
            throw new BusinessException(ErpErrorCodes.Service.FinishByPosting).WithData("no", No);
        }

        Status = status;
    }

    public void EnsureNotFinished()
    {
        if (Status == ServiceOrderStatus.Finished)
        {
            throw new BusinessException(ErpErrorCodes.Service.OrderFinished).WithData("no", No);
        }
    }

    public ServiceItemLine AddItemLine(Guid id, ServiceItem serviceItem)
    {
        EnsureNotFinished();
        var line = new ServiceItemLine(id, Id, ItemLines.Count == 0 ? 10000 : ItemLines.Max(l => l.LineNo) + 10000, serviceItem, OrderDate);
        ItemLines.Add(line);
        return line;
    }

    public ServiceLine AddLine(Guid id)
    {
        EnsureNotFinished();
        var line = new ServiceLine(id, Id, Lines.Count == 0 ? 10000 : Lines.Max(l => l.LineNo) + 10000);
        Lines.Add(line);
        return line;
    }

    /// <summary>The lines with something left to post.</summary>
    public List<ServiceLine> GetOpenLines() => Lines.Where(l => l.RemainingQuantity != 0m).ToList();

    internal void RecordPosting(string invoiceNo, DateTime postingDate)
    {
        LastInvoiceNo = invoiceNo ?? LastInvoiceNo;
        if (GetOpenLines().Count == 0)
        {
            Status = ServiceOrderStatus.Finished;
            FinishingDate = postingDate.Date;
        }
        else if (Status == ServiceOrderStatus.Pending)
        {
            Status = ServiceOrderStatus.InProcess;
        }
    }
}

/// <summary>A service item on a service order: what was wrong, what was done, and whether the warranty covers it.</summary>
public class ServiceItemLine : Entity<Guid>
{
    public Guid ServiceOrderId { get; private set; }
    public int LineNo { get; private set; }
    public string ServiceItemNo { get; private set; }
    public string Description { get; private set; }
    public string ItemNo { get; private set; }
    public string SerialNo { get; private set; }
    public string FaultCode { get; private set; }
    public string ResolutionCode { get; private set; }

    /// <summary>Set from the service item's warranty on the order date; its lines are then free to the customer.</summary>
    public bool Warranty { get; private set; }

    public string FaultComment { get; private set; }

    protected ServiceItemLine() { }

    internal ServiceItemLine(Guid id, Guid orderId, int lineNo, ServiceItem serviceItem, DateTime orderDate)
        : base(id)
    {
        ServiceOrderId = orderId;
        LineNo = lineNo;
        ServiceItemNo = serviceItem.No;
        Description = serviceItem.Description;
        ItemNo = serviceItem.ItemNo;
        SerialNo = serviceItem.SerialNo;
        Warranty = serviceItem.IsUnderWarranty(orderDate);
    }

    public void Set(string faultCode, string resolutionCode, string faultComment, bool warranty)
    {
        FaultCode = CodeTableEntity.NormalizeCode(faultCode);
        ResolutionCode = CodeTableEntity.NormalizeCode(resolutionCode);
        FaultComment = Check.Length(faultComment, nameof(faultComment), ErpDomainConsts.MaxDescriptionLength);
        Warranty = warranty;
    }
}

/// <summary>A part, labour or cost on a service order, posted to the customer's invoice.</summary>
public class ServiceLine : Entity<Guid>
{
    public Guid ServiceOrderId { get; private set; }
    public int LineNo { get; private set; }

    /// <summary>The service item the line was spent on.</summary>
    public string ServiceItemNo { get; private set; }

    public ServiceLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public string LocationCode { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal LineDiscountPct { get; private set; }

    /// <summary>Covered by the warranty: the customer is not charged, and the value goes to the warranty ledger.</summary>
    public bool Warranty { get; private set; }

    public decimal QuantityPosted { get; private set; }

    public decimal RemainingQuantity => Quantity - QuantityPosted;

    /// <summary>What the line is worth before the warranty: quantity times price, less the discount.</summary>
    public decimal LineAmount => Math.Round(Quantity * UnitPrice * (1m - LineDiscountPct / 100m), 2, MidpointRounding.AwayFromZero);

    protected ServiceLine() { }

    internal ServiceLine(Guid id, Guid orderId, int lineNo)
        : base(id)
    {
        ServiceOrderId = orderId;
        LineNo = lineNo;
    }

    public void Set(string serviceItemNo, ServiceLineType type, string no, string description, string locationCode, decimal quantity, decimal unitPrice, decimal unitCost, decimal lineDiscountPct, bool warranty)
    {
        if (QuantityPosted != 0m && (type != Type || no?.Trim() != No))
        {
            throw new BusinessException(ErpErrorCodes.Service.LinePosted).WithData("no", No ?? "");
        }

        if (quantity < QuantityPosted || unitPrice < 0m || lineDiscountPct < 0m || lineDiscountPct > 100m)
        {
            throw new BusinessException(ErpErrorCodes.Service.InvalidLine).WithData("no", no ?? "");
        }

        ServiceItemNo = serviceItemNo.IsNullOrWhiteSpace() ? null : serviceItemNo.Trim().ToUpperInvariant();
        Type = type;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        LocationCode = CodeTableEntity.NormalizeCode(locationCode);
        Quantity = quantity;
        UnitPrice = unitPrice;
        UnitCost = Math.Max(0m, unitCost);
        LineDiscountPct = lineDiscountPct;
        Warranty = warranty;
    }

    internal void RecordPosted(decimal quantity) => QuantityPosted += quantity;
}

/// <summary>Service Ledger Entry: a posted service line or an invoiced contract period, with its cost and amount.</summary>
public class ServiceLedgerEntry : LedgerEntryBase
{
    public ServiceLedgerEntryType EntryType { get; internal set; }
    public DateTime PostingDate { get; internal set; }
    public string DocumentNo { get; internal set; }
    public string ServiceOrderNo { get; internal set; }
    public string ServiceContractNo { get; internal set; }
    public string ServiceItemNo { get; internal set; }
    public string CustomerNo { get; internal set; }
    public ServiceLineType Type { get; internal set; }
    public string No { get; internal set; }
    public string Description { get; internal set; }
    public decimal Quantity { get; internal set; }
    public decimal CostAmount { get; internal set; }

    /// <summary>What the customer was charged, excluding VAT.</summary>
    public decimal Amount { get; internal set; }

    public bool Warranty { get; internal set; }

    protected ServiceLedgerEntry() { }

    public ServiceLedgerEntry(Guid id)
        : base(id) { }
}

/// <summary>Warranty Ledger Entry: work done free under a warranty, at the value the customer would have paid.</summary>
public class WarrantyLedgerEntry : LedgerEntryBase
{
    public DateTime PostingDate { get; internal set; }
    public string DocumentNo { get; internal set; }
    public string ServiceOrderNo { get; internal set; }
    public string ServiceItemNo { get; internal set; }
    public string ItemNo { get; internal set; }
    public string SerialNo { get; internal set; }
    public string CustomerNo { get; internal set; }
    public ServiceLineType Type { get; internal set; }
    public string No { get; internal set; }
    public string Description { get; internal set; }
    public decimal Quantity { get; internal set; }
    public decimal Amount { get; internal set; }
    public decimal CostAmount { get; internal set; }

    protected WarrantyLedgerEntry() { }

    public WarrantyLedgerEntry(Guid id)
        : base(id) { }
}
