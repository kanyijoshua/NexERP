using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Academics;

/// <summary>Something the laundry washes, its price per piece and the income account it earns.</summary>
public class LaundryItem : CodeTableEntity
{
    public decimal RatePerItem { get; private set; }
    public string GLAccountNo { get; private set; }

    protected LaundryItem() { }

    public LaundryItem(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(decimal ratePerItem, string glAccountNo)
    {
        if (ratePerItem < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Rate Per Item");
        }

        RatePerItem = ratePerItem;
        GLAccountNo = glAccountNo.IsNullOrWhiteSpace() ? null : Check.Length(glAccountNo.Trim(), nameof(glAccountNo), ErpDomainConsts.MaxNoLength);
    }
}

/// <summary>
/// A laundry order: what a customer (a student's account has the student's number) hands in. It is
/// invoiced to the customer when sent for washing, then marked ready and collected.
/// </summary>
public class LaundryOrder : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string CustomerNo { get; private set; }
    public string CustomerName { get; private set; }
    public DateTime ReceivedDate { get; private set; }
    public DateTime? PromisedDate { get; private set; }

    /// <summary>An express order costs the Academic Setup's express charge more per item.</summary>
    public bool Express { get; private set; }

    public decimal DiscountPct { get; private set; }
    public string Remarks { get; private set; }

    public LaundryStatus Status { get; private set; }
    public decimal TotalAmount { get; internal set; }
    public DateTime? InvoicedDate { get; private set; }
    public DateTime? CollectedDate { get; private set; }
    public string ProcessedBy { get; private set; }

    protected LaundryOrder() { }

    public LaundryOrder(Guid id, string no, Customer customer, DateTime receivedDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(customer, receivedDate, null, false, 0m, null);
    }

    public void Set(Customer customer, DateTime receivedDate, DateTime? promisedDate, bool express, decimal discountPct, string remarks)
    {
        EnsureStatus(LaundryStatus.Received);
        if (discountPct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", discountPct);
        }

        CustomerNo = customer.No;
        CustomerName = customer.Name;
        ReceivedDate = receivedDate.Date;
        PromisedDate = promisedDate?.Date;
        Express = express;
        DiscountPct = discountPct;
        Remarks = Check.Length(remarks, nameof(remarks), ErpDomainConsts.MaxDescriptionLength);
    }

    /// <summary>What a line comes to: quantity × rate, plus the express charge, less the order's discount.</summary>
    public decimal AmountOf(decimal quantity, decimal unitPrice, decimal expressChargePct)
    {
        var gross = quantity * unitPrice * (Express ? 1m + expressChargePct / 100m : 1m);
        return Math.Round(gross * (100m - DiscountPct) / 100m, 2, MidpointRounding.AwayFromZero);
    }

    internal void MarkInvoiced(DateTime when, string by)
    {
        Status = LaundryStatus.Invoiced;
        InvoicedDate = when.Date;
        ProcessedBy = by;
    }

    public void MarkReady()
    {
        EnsureStatus(LaundryStatus.Invoiced);
        Status = LaundryStatus.Ready;
    }

    public void MarkCollected(DateTime when, string by)
    {
        EnsureStatus(LaundryStatus.Ready);
        Status = LaundryStatus.Collected;
        CollectedDate = when.Date;
        ProcessedBy = by;
    }

    public void EnsureStatus(LaundryStatus status)
    {
        if (Status != status)
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentStatusWrong).WithData("documentNo", No ?? string.Empty).WithData("status", Status);
        }
    }
}

/// <summary>The pieces of one kind on a laundry order.</summary>
public class LaundryOrderLine : CompanyEntity
{
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }
    public string LaundryItemCode { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Amount { get; internal set; }

    protected LaundryOrderLine() { }

    public LaundryOrderLine(Guid id, string documentNo, int lineNo)
        : base(id)
    {
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        LineNo = lineNo;
    }

    /// <param name="unitPrice">Null takes the item's rate.</param>
    public void Set(LaundryItem item, string description, decimal quantity, decimal? unitPrice)
    {
        if (quantity < 0 || unitPrice < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", quantity < 0 ? "Quantity" : "Unit Price");
        }

        LaundryItemCode = item.Code;
        Description = description.IsNullOrWhiteSpace() ? item.Description : Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Quantity = quantity;
        UnitPrice = unitPrice ?? item.RatePerItem;
    }
}

/// <summary>Prices and invoices laundry orders.</summary>
public class LaundryManager : DomainService
{
    public const string SourceCode = "LAUNDRY";

    private readonly IRepository<LaundryOrder, Guid> _orders;
    private readonly IRepository<LaundryOrderLine, Guid> _lines;
    private readonly IRepository<LaundryItem, Guid> _items;
    private readonly AcademicSetupManager _setupManager;
    private readonly CustomerChargePoster _poster;
    private readonly ICurrentUser _currentUser;

    public LaundryManager(
        IRepository<LaundryOrder, Guid> orders,
        IRepository<LaundryOrderLine, Guid> lines,
        IRepository<LaundryItem, Guid> items,
        AcademicSetupManager setupManager,
        CustomerChargePoster poster,
        ICurrentUser currentUser
    )
    {
        _orders = orders;
        _lines = lines;
        _items = items;
        _setupManager = setupManager;
        _poster = poster;
        _currentUser = currentUser;
    }

    /// <summary>Prices every line at the order's express charge and discount, and stores the total on the order.</summary>
    public async Task<List<LaundryOrderLine>> UpdateTotalsAsync(LaundryOrder order)
    {
        var setup = await _setupManager.GetAsync();
        var lines = (await _lines.GetListAsync(l => l.DocumentNo == order.No)).OrderBy(l => l.LineNo).ToList();

        foreach (var line in lines)
        {
            var amount = order.AmountOf(line.Quantity, line.UnitPrice, setup.LaundryExpressChargePct);
            if (line.Amount != amount)
            {
                line.Amount = amount;
                await _lines.UpdateAsync(line, autoSave: true);
            }
        }

        order.TotalAmount = lines.Sum(l => l.Amount);
        await _orders.UpdateAsync(order, autoSave: true);
        return lines;
    }

    /// <summary>Invoices the order to the customer: the total to the customer, each line to its item's income account.</summary>
    public async Task InvoiceAsync(LaundryOrder order, DateTime postingDate)
    {
        order.EnsureStatus(LaundryStatus.Received);

        var lines = (await UpdateTotalsAsync(order)).Where(l => l.Amount != 0m).ToList();
        var items = (await _items.GetListAsync()).ToDictionary(i => i.Code, StringComparer.Ordinal);

        var charges = lines
            .Select(l => new CustomerChargeLine(items.TryGetValue(l.LaundryItemCode, out var item) ? item.GLAccountNo : null, $"{l.Description} x {l.Quantity:0.##}", l.Amount))
            .ToList();

        await _poster.PostAsync(SourceCode, order.No, postingDate, order.CustomerNo, $"Laundry {order.No}", charges);

        order.MarkInvoiced(postingDate, _currentUser.UserName);
        await _orders.UpdateAsync(order, autoSave: true);
    }
}
