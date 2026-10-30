using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.PointOfSale;

/// <summary>
/// POS Terminal: a till. Its sales go to the walk-in customer unless another is chosen, take stock
/// from its location, and are paid into its cash or card bank account.
/// </summary>
public class PosTerminal : CodeTableEntity
{
    public string LocationCode { get; private set; }
    public string WalkInCustomerNo { get; private set; }
    public string CashBankAccountNo { get; private set; }
    public string CardBankAccountNo { get; private set; }
    public bool Blocked { get; private set; }

    protected PosTerminal() { }

    public PosTerminal(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(string locationCode, string walkInCustomerNo, string cashBankAccountNo, string cardBankAccountNo, bool blocked)
    {
        LocationCode = NormalizeCode(Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
        WalkInCustomerNo = Clean(walkInCustomerNo, nameof(walkInCustomerNo));
        CashBankAccountNo = Clean(cashBankAccountNo, nameof(cashBankAccountNo));
        CardBankAccountNo = Clean(cardBankAccountNo, nameof(cardBankAccountNo));
        Blocked = blocked;
    }

    private static string Clean(string no, string name) =>
        no.IsNullOrWhiteSpace() ? null : Check.Length(no.Trim(), name, ErpDomainConsts.MaxNoLength);
}

/// <summary>POS Session: a till's shift, from the opening float to the counted cash at closing.</summary>
public class PosSession : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string TerminalCode { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public string OpenedBy { get; private set; }
    public decimal OpeningFloat { get; private set; }
    public PosSessionStatus Status { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public decimal CashSales { get; private set; }
    public decimal CardSales { get; private set; }
    public decimal CountedCash { get; private set; }

    /// <summary>What should be in the drawer: the float plus the cash taken.</summary>
    public decimal ExpectedCash => OpeningFloat + CashSales;

    public decimal Difference => Status == PosSessionStatus.Closed ? CountedCash - ExpectedCash : 0m;

    protected PosSession() { }

    public PosSession(Guid id, string no, string terminalCode, DateTime openedAt, string openedBy, decimal openingFloat)
        : base(id)
    {
        No = no;
        TerminalCode = terminalCode;
        OpenedAt = openedAt;
        OpenedBy = openedBy;
        OpeningFloat = Math.Max(0m, openingFloat);
        Status = PosSessionStatus.Open;
    }

    public void EnsureOpen()
    {
        if (Status != PosSessionStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.PointOfSale.SessionNotOpen).WithData("no", No);
        }
    }

    internal void RecordPayment(PosPaymentMethod method, decimal amount)
    {
        if (method == PosPaymentMethod.Cash)
        {
            CashSales += amount;
        }
        else
        {
            CardSales += amount;
        }
    }

    internal void Close(decimal countedCash, DateTime closedAt)
    {
        EnsureOpen();
        CountedCash = countedCash;
        ClosedAt = closedAt;
        Status = PosSessionStatus.Closed;
    }
}

/// <summary>POS Order: one sale at a till, paid in full, which posts as a sales invoice and its payment.</summary>
public class PosOrder : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public Guid SessionId { get; private set; }
    public string TerminalCode { get; private set; }
    public string CustomerNo { get; private set; }
    public DateTime OrderDate { get; private set; }
    public PosOrderStatus Status { get; private set; }
    public PosPaymentMethod PaymentMethod { get; private set; }
    public decimal AmountTendered { get; private set; }
    public decimal Change { get; private set; }

    /// <summary>Including VAT, as the invoice posted it.</summary>
    public decimal TotalPaid { get; private set; }

    public string PostedInvoiceNo { get; private set; }

    public Collection<PosOrderLine> Lines { get; private set; } = new();

    public decimal Subtotal => Lines.Sum(l => l.LineAmount);

    protected PosOrder() { }

    public PosOrder(Guid id, string no, PosSession session, string customerNo, DateTime orderDate)
        : base(id)
    {
        No = no;
        SessionId = session.Id;
        TerminalCode = session.TerminalCode;
        CustomerNo = customerNo;
        OrderDate = orderDate.Date;
        Status = PosOrderStatus.Open;
    }

    public PosOrderLine AddLine(Guid id, Item item, decimal quantity, decimal unitPrice, decimal discountPct)
    {
        EnsureOpen();
        var line = new PosOrderLine(id, Id, (Lines.Count == 0 ? 0 : Lines.Max(l => l.LineNo)) + 10000, item);
        line.Set(quantity, unitPrice, discountPct);
        Lines.Add(line);
        return line;
    }

    public void SetCustomer(string customerNo)
    {
        EnsureOpen();
        CustomerNo = customerNo;
    }

    public void Cancel()
    {
        EnsureOpen();
        Status = PosOrderStatus.Cancelled;
    }

    public void EnsureOpen()
    {
        if (Status != PosOrderStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.PointOfSale.OrderNotOpen).WithData("no", No);
        }
    }

    internal void RecordPaid(PosPaymentMethod method, decimal tendered, decimal total, string invoiceNo)
    {
        PaymentMethod = method;
        AmountTendered = tendered;
        TotalPaid = total;
        Change = method == PosPaymentMethod.Cash ? tendered - total : 0m;
        PostedInvoiceNo = invoiceNo;
        Status = PosOrderStatus.Paid;
    }
}

public class PosOrderLine : Entity<Guid>
{
    public Guid PosOrderId { get; private set; }
    public int LineNo { get; private set; }
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPct { get; private set; }
    public decimal LineAmount { get; private set; }

    protected PosOrderLine() { }

    public PosOrderLine(Guid id, Guid orderId, int lineNo, Item item)
        : base(id)
    {
        PosOrderId = orderId;
        LineNo = lineNo;
        ItemId = item.Id;
        ItemNo = item.No;
        Description = item.Description;
    }

    public void Set(decimal quantity, decimal unitPrice, decimal discountPct)
    {
        Quantity = quantity > 0m ? quantity : 1m;
        UnitPrice = Math.Max(0m, unitPrice);
        DiscountPct = Math.Clamp(discountPct, 0m, 100m);
        LineAmount = Math.Round(Quantity * UnitPrice * (1m - DiscountPct / 100m), 2, MidpointRounding.AwayFromZero);
    }
}

/// <summary>
/// Point of sale: opens and closes sessions and takes payment for orders. Paying an order posts a
/// sales invoice to its customer (which takes the stock out of the till's location) and the
/// payment into the till's cash or card bank account, applied to that invoice.
/// </summary>
public class PosManager : DomainService
{
    private const string SourceCode = "POS";

    private readonly IRepository<PosTerminal, Guid> _terminals;
    private readonly IRepository<PosSession, Guid> _sessions;
    private readonly IRepository<PosOrder, Guid> _orders;

    private IRepository<T, Guid> Repo<T>() where T : class, IEntity<Guid> => LazyServiceProvider.LazyGetRequiredService<IRepository<T, Guid>>();
    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public PosManager(IRepository<PosTerminal, Guid> terminals, IRepository<PosSession, Guid> sessions, IRepository<PosOrder, Guid> orders)
    {
        _terminals = terminals;
        _sessions = sessions;
        _orders = orders;
    }

    public async Task<PosTerminal> GetTerminalAsync(string code)
    {
        var normalized = CodeTableEntity.NormalizeCode(code);
        var terminal = await _terminals.FirstOrDefaultAsync(t => t.Code == normalized && !t.Blocked)
            ?? throw new BusinessException(ErpErrorCodes.PointOfSale.TerminalNotFound).WithData("code", normalized ?? "");
        if (terminal.WalkInCustomerNo == null || terminal.CashBankAccountNo == null)
        {
            throw new BusinessException(ErpErrorCodes.PointOfSale.TerminalSetupMissing).WithData("code", terminal.Code).WithData("field", terminal.WalkInCustomerNo == null ? "WalkInCustomerNo" : "CashBankAccountNo");
        }

        return terminal;
    }

    public async Task<PosSession> OpenSessionAsync(string terminalCode, decimal openingFloat, string userName)
    {
        var terminal = await GetTerminalAsync(terminalCode);
        var open = await _sessions.FirstOrDefaultAsync(s => s.TerminalCode == terminal.Code && s.Status == PosSessionStatus.Open);
        if (open != null)
        {
            throw new BusinessException(ErpErrorCodes.PointOfSale.SessionAlreadyOpen).WithData("code", terminal.Code).WithData("no", open.No);
        }

        var no = $"POSS{await EntryNos.NextAsync(ErpSequenceNames.PosSessionNo):D5}";
        return await _sessions.InsertAsync(new PosSession(GuidGenerator.Create(), no, terminal.Code, Clock.Now, userName, openingFloat), autoSave: true);
    }

    public async Task<PosOrder> NewOrderAsync(Guid sessionId, string customerNo)
    {
        var session = await _sessions.GetAsync(sessionId);
        session.EnsureOpen();
        var terminal = await GetTerminalAsync(session.TerminalCode);
        var no = $"POS{await EntryNos.NextAsync(ErpSequenceNames.PosOrderNo):D6}";
        var order = new PosOrder(GuidGenerator.Create(), no, session, customerNo.IsNullOrWhiteSpace() ? terminal.WalkInCustomerNo : customerNo.Trim(), Clock.Now);
        return await _orders.InsertAsync(order, autoSave: true);
    }

    /// <summary>Posts the order: the invoice, then the payment applied to it. Returns the order, paid.</summary>
    public async Task<PosOrder> PayAsync(Guid orderId, PosPaymentMethod method, decimal amountTendered)
    {
        var order = await _orders.GetAsync(orderId, includeDetails: true);
        order.EnsureOpen();
        if (order.Lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.PointOfSale.OrderHasNoLines).WithData("no", order.No);
        }

        var session = await _sessions.GetAsync(order.SessionId);
        session.EnsureOpen();
        var terminal = await GetTerminalAsync(order.TerminalCode);
        var bankNo = method == PosPaymentMethod.Card ? terminal.CardBankAccountNo ?? terminal.CashBankAccountNo : terminal.CashBankAccountNo;

        var customerNo = order.CustomerNo;
        var customer = await Repo<Customer>().FirstOrDefaultAsync(c => c.No == customerNo)
            ?? throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", customerNo ?? "");

        // The invoice decides the total, VAT included, before any money is taken.
        var invoice = new SalesHeader(GuidGenerator.Create(), SalesDocumentType.Invoice, order.No, customer.Id, customer.No, customer.Name, order.OrderDate);
        invoice.SetDates(order.OrderDate, order.OrderDate);
        invoice.SetLocation(terminal.LocationCode);
        invoice.SetExternalDocumentNo(session.No);
        foreach (var line in order.Lines.OrderBy(l => l.LineNo))
        {
            var salesLine = invoice.AddLine(GuidGenerator.Create(), DocumentLineType.Item, line.ItemNo, line.Description, line.Quantity, line.UnitPrice, line.DiscountPct);
            salesLine.SetLocation(terminal.LocationCode);
        }

        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(invoice, customer);
        invoice.RecalculateTotals();
        var total = invoice.TotalAmountIncludingVat;
        if (method == PosPaymentMethod.Cash && amountTendered < total)
        {
            throw new BusinessException(ErpErrorCodes.PointOfSale.PaymentTooSmall).WithData("tendered", amountTendered).WithData("total", total);
        }

        await Repo<SalesHeader>().InsertAsync(invoice, autoSave: true);
        var posted = await LazyServiceProvider.LazyGetRequiredService<SalesPostingEngine>().PostAsync(invoice.Id);
        var invoiceNo = posted.Invoice.No;

        var registers = LazyServiceProvider.LazyGetRequiredService<GLRegisterManager>();
        var register = await registers.OpenAsync(order.OrderDate, SourceCode, order.No);
        var context = new GLPostingContext(register, SourceCode);
        var postLine = LazyServiceProvider.LazyGetRequiredService<GenJnlPostLine>();
        var description = $"POS {order.No}";
        await postLine.PostLineAsync(PaymentLine(order, 10000, GenJournalAccountType.Customer, customer.No, description, -total, invoiceNo), context);
        await postLine.PostLineAsync(PaymentLine(order, 20000, GenJournalAccountType.BankAccount, bankNo, description, total, null), context);
        await registers.CloseAsync(register);

        order.RecordPaid(method, method == PosPaymentMethod.Cash ? amountTendered : total, total, invoiceNo);
        session.RecordPayment(method, total);
        await _sessions.UpdateAsync(session, autoSave: true);
        return await _orders.UpdateAsync(order, autoSave: true);
    }

    public async Task<PosSession> CloseSessionAsync(Guid sessionId, decimal countedCash)
    {
        var session = await _sessions.GetAsync(sessionId);
        var open = await _orders.GetListAsync(o => o.SessionId == sessionId && o.Status == PosOrderStatus.Open);
        foreach (var order in open)
        {
            order.Cancel();
            await _orders.UpdateAsync(order, autoSave: true);
        }

        session.Close(countedCash, Clock.Now);
        return await _sessions.UpdateAsync(session, autoSave: true);
    }

    private GenJournalLine PaymentLine(PosOrder order, int lineNo, GenJournalAccountType accountType, string accountNo, string description, decimal amount, string appliesToDocNo)
    {
        var line = new GenJournalLine(GuidGenerator.Create(), Guid.Empty, lineNo, order.OrderDate, GLEntryDocumentType.Payment, order.No, accountType, accountNo, description, amount);
        line.Update(order.OrderDate, GLEntryDocumentType.Payment, order.No, accountType, accountNo, description, amount, appliesToDocNo: appliesToDocNo);
        return line;
    }
}
