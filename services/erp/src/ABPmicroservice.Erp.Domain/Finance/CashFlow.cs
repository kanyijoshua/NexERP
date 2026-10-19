using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Cash Flow Forecast: which sources a forecast draws on and how far ahead it looks. Its worksheet
/// is filled from them and registered as forecast entries, which never touch the ledgers.
/// </summary>
public class CashFlowForecast : CompanyAggregateRoot
{
    public string No { get; private set; }
    public string Description { get; private set; }

    /// <summary>Lines due after this date are left out.</summary>
    public DateTime? ManualPaymentsTo { get; private set; }

    public bool ConsiderReceivables { get; private set; } = true;
    public bool ConsiderPayables { get; private set; } = true;
    public bool ConsiderSalesOrders { get; private set; } = true;
    public bool ConsiderPurchaseOrders { get; private set; } = true;
    public bool ConsiderManual { get; private set; } = true;
    public bool ConsiderLiquidFunds { get; private set; } = true;

    /// <summary>The G/L budget whose expenses and income count, if any.</summary>
    public string GLBudgetName { get; private set; }

    public DateTime? GLBudgetFrom { get; private set; }
    public DateTime? GLBudgetTo { get; private set; }

    protected CashFlowForecast() { }

    public CashFlowForecast(Guid id, string no, string description)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        SetDescription(description);
    }

    public void SetNo(string no) => No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();

    public void SetDescription(string description) => Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);

    public void SetSources(bool receivables, bool payables, bool salesOrders, bool purchaseOrders, bool manual, bool liquidFunds, DateTime? manualPaymentsTo)
    {
        ConsiderReceivables = receivables;
        ConsiderPayables = payables;
        ConsiderSalesOrders = salesOrders;
        ConsiderPurchaseOrders = purchaseOrders;
        ConsiderManual = manual;
        ConsiderLiquidFunds = liquidFunds;
        ManualPaymentsTo = manualPaymentsTo?.Date;
    }

    public void SetBudget(string glBudgetName, DateTime? from, DateTime? to)
    {
        GLBudgetName = CodeTableEntity.NormalizeCode(Check.Length(glBudgetName, nameof(glBudgetName), 10));
        GLBudgetFrom = from?.Date;
        GLBudgetTo = to?.Date;
    }
}

/// <summary>Money expected in or out that no document shows yet: rent, a loan, tax, recurring by a date formula.</summary>
public class CashFlowManualEntry : CodeTableEntity
{
    public CashFlowManualKind Kind { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime StartingDate { get; private set; }

    /// <summary>"1M" repeats monthly; blank happens once.</summary>
    public string RecurringFrequency { get; private set; }

    public DateTime? EndingDate { get; private set; }

    protected CashFlowManualEntry() { }

    public CashFlowManualEntry(Guid id, string code, string description, CashFlowManualKind kind, decimal amount, DateTime startingDate)
        : base(id, code, description)
    {
        Set(kind, amount, startingDate, null, null);
    }

    public void Set(CashFlowManualKind kind, decimal amount, DateTime startingDate, string recurringFrequency, DateTime? endingDate)
    {
        Kind = kind;
        Amount = Math.Abs(amount);
        StartingDate = startingDate.Date;
        RecurringFrequency = recurringFrequency.IsNullOrWhiteSpace() ? null : Sales.ReminderLevel.Formula(recurringFrequency);
        EndingDate = endingDate?.Date;
    }

    /// <summary>The dates it falls on up to <paramref name="until"/>.</summary>
    public IEnumerable<DateTime> Occurrences(DateTime from, DateTime until)
    {
        var date = StartingDate;
        var formula = RecurringFrequency == null ? null : DateFormula.Parse(RecurringFrequency);
        for (var i = 0; i < 1000 && date <= until && (EndingDate == null || date <= EndingDate.Value); i++)
        {
            if (date >= from)
            {
                yield return date;
            }
            if (formula == null)
            {
                yield break;
            }

            var next = formula.Apply(date);
            if (next <= date)
            {
                yield break;
            }
            date = next;
        }
    }
}

/// <summary>A line of the cash flow worksheet: one amount expected on one date, waiting to be registered.</summary>
public class CashFlowWorksheetLine : CompanyEntity
{
    public string CashFlowForecastNo { get; private set; }
    public int LineNo { get; private set; }
    public DateTime CashFlowDate { get; private set; }
    public CashFlowSourceType SourceType { get; private set; }
    public string SourceNo { get; private set; }
    public string DocumentNo { get; private set; }
    public string Description { get; private set; }

    /// <summary>Positive comes in, negative goes out. In LCY.</summary>
    public decimal Amount { get; private set; }

    protected CashFlowWorksheetLine() { }

    public CashFlowWorksheetLine(Guid id, string forecastNo, int lineNo, DateTime date, CashFlowSourceType sourceType, string sourceNo, string documentNo, string description, decimal amount)
        : base(id)
    {
        CashFlowForecastNo = forecastNo;
        LineNo = lineNo;
        Update(date, sourceType, sourceNo, documentNo, description, amount);
    }

    public void Update(DateTime date, CashFlowSourceType sourceType, string sourceNo, string documentNo, string description, decimal amount)
    {
        CashFlowDate = date.Date;
        SourceType = sourceType;
        SourceNo = Check.Length(sourceNo, nameof(sourceNo), ErpDomainConsts.MaxNoLength);
        DocumentNo = Check.Length(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Amount = amount;
    }
}

/// <summary>A registered forecast amount. Read by the forecast reports; posts nothing, and a new registration replaces it.</summary>
public class CashFlowForecastEntry : CompanyBasicEntity
{
    public long EntryNo { get; private set; }
    public string CashFlowForecastNo { get; private set; }
    public DateTime CashFlowDate { get; private set; }
    public CashFlowSourceType SourceType { get; private set; }
    public string SourceNo { get; private set; }
    public string DocumentNo { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }

    protected CashFlowForecastEntry() { }

    internal CashFlowForecastEntry(Guid id, long entryNo, CashFlowWorksheetLine line)
        : base(id)
    {
        EntryNo = entryNo;
        CashFlowForecastNo = line.CashFlowForecastNo;
        CashFlowDate = line.CashFlowDate;
        SourceType = line.SourceType;
        SourceNo = line.SourceNo;
        DocumentNo = line.DocumentNo;
        Description = line.Description;
        Amount = line.Amount;
    }
}

/// <summary>One row of a forecast by period: a source type and its amount per column.</summary>
public sealed record CashFlowPeriodRow(CashFlowSourceType SourceType, decimal[] Amounts, decimal Total);

/// <summary>
/// Fills the cash flow worksheet from what is known, and registers it. Receivables and payables
/// come in or go out on their due dates; open orders on their shipment or receipt dates; manual
/// lines on every date they recur; budget figures on their dates; liquid funds are the bank
/// balances today.
/// </summary>
public class CashFlowManager : DomainService
{
    private readonly IRepository<CashFlowForecast, Guid> _forecastRepository;
    private readonly IRepository<CashFlowWorksheetLine, Guid> _worksheetRepository;
    private readonly IRepository<CashFlowForecastEntry, Guid> _entryRepository;

    private T Repo<T>() where T : class => LazyServiceProvider.LazyGetRequiredService<T>();

    public CashFlowManager(
        IRepository<CashFlowForecast, Guid> forecastRepository,
        IRepository<CashFlowWorksheetLine, Guid> worksheetRepository,
        IRepository<CashFlowForecastEntry, Guid> entryRepository
    )
    {
        _forecastRepository = forecastRepository;
        _worksheetRepository = worksheetRepository;
        _entryRepository = entryRepository;
    }

    /// <summary>Replaces the worksheet lines of the forecast with fresh suggestions. Returns how many.</summary>
    public async Task<int> SuggestWorksheetLinesAsync(Guid forecastId, DateTime fromDate, DateTime toDate)
    {
        var forecast = await _forecastRepository.GetAsync(forecastId);
        if (toDate < fromDate)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.InvalidDateRange);
        }

        await _worksheetRepository.DeleteAsync(l => l.CashFlowForecastNo == forecast.No);
        var lines = new List<(DateTime Date, CashFlowSourceType Type, string SourceNo, string DocumentNo, string Description, decimal Amount)>();
        DateTime Clamp(DateTime date) => date.Date < fromDate.Date ? fromDate.Date : date.Date;

        if (forecast.ConsiderLiquidFunds)
        {
            foreach (var bank in await Repo<IRepository<BankAccount, Guid>>().GetListAsync(b => !b.Blocked))
            {
                if (bank.BalanceLcy != 0m)
                {
                    lines.Add((fromDate.Date, CashFlowSourceType.LiquidFunds, bank.No, null, bank.Name, bank.BalanceLcy));
                }
            }
        }

        if (forecast.ConsiderReceivables)
        {
            foreach (var entry in await Repo<IRepository<CustomerLedgerEntry, Guid>>().GetListAsync(e => e.Open && e.DueDate <= toDate))
            {
                lines.Add((Clamp(entry.DueDate), CashFlowSourceType.Receivables, entry.CustomerNo, entry.DocumentNo, entry.Description, entry.RemainingAmountLcy));
            }
        }

        if (forecast.ConsiderPayables)
        {
            foreach (var entry in await Repo<IRepository<VendorLedgerEntry, Guid>>().GetListAsync(e => e.Open && e.DueDate <= toDate))
            {
                lines.Add((Clamp(entry.DueDate), CashFlowSourceType.Payables, entry.VendorNo, entry.DocumentNo, entry.Description, entry.RemainingAmountLcy));
            }
        }

        if (forecast.ConsiderSalesOrders)
        {
            var orders = await Repo<IRepository<SalesHeader, Guid>>().GetListAsync(h => h.DocumentType == SalesDocumentType.Order, includeDetails: true);
            foreach (var order in orders)
            {
                var outstanding = order.Lines.Sum(l => l.AmountToInvoice(l.Quantity - l.QuantityInvoiced));
                var date = order.DueDate ?? order.PostingDate;
                if (outstanding != 0m && date <= toDate)
                {
                    lines.Add((Clamp(date), CashFlowSourceType.SalesOrders, order.SellToCustomerNo, order.No, order.SellToCustomerName, outstanding));
                }
            }
        }

        if (forecast.ConsiderPurchaseOrders)
        {
            var orders = await Repo<IRepository<PurchaseHeader, Guid>>().GetListAsync(h => h.DocumentType == PurchaseDocumentType.Order, includeDetails: true);
            foreach (var order in orders)
            {
                var outstanding = order.Lines.Sum(l => l.AmountToInvoice(l.Quantity - l.QuantityInvoiced));
                var date = order.DueDate ?? order.PostingDate;
                if (outstanding != 0m && date <= toDate)
                {
                    lines.Add((Clamp(date), CashFlowSourceType.PurchaseOrders, order.BuyFromVendorNo, order.No, order.BuyFromVendorName, -outstanding));
                }
            }
        }

        if (forecast.ConsiderManual)
        {
            var until = forecast.ManualPaymentsTo is { } limit && limit < toDate ? limit : toDate;
            foreach (var manual in await Repo<IRepository<CashFlowManualEntry, Guid>>().GetListAsync())
            {
                foreach (var date in manual.Occurrences(fromDate.Date, until.Date))
                {
                    lines.Add((date, CashFlowSourceType.Manual, manual.Code, null, manual.Description, manual.Kind == CashFlowManualKind.Revenue ? manual.Amount : -manual.Amount));
                }
            }
        }

        if (forecast.GLBudgetName != null)
        {
            var from = forecast.GLBudgetFrom ?? fromDate;
            var to = forecast.GLBudgetTo ?? toDate;
            var budget = await Repo<IRepository<GLBudgetEntry, Guid>>().GetListAsync(e => e.BudgetName == forecast.GLBudgetName && e.Date >= from && e.Date <= to);
            var accounts = (await Repo<IRepository<GLAccount, Guid>>().GetListAsync(a => a.IncomeBalance == IncomeBalanceType.IncomeStatement)).Select(a => a.No).ToHashSet();
            foreach (var entry in budget.Where(e => accounts.Contains(e.GLAccountNo) && e.Amount != 0m))
            {
                // A budgeted expense (debit) is money out, budgeted income (credit) money in.
                lines.Add((Clamp(entry.Date!.Value), CashFlowSourceType.Budget, entry.GLAccountNo, null, entry.Description, -entry.Amount));
            }
        }

        var lineNo = 0;
        foreach (var line in lines.Where(l => l.Amount != 0m).OrderBy(l => l.Date).ThenBy(l => l.Type))
        {
            lineNo += 10000;
            await _worksheetRepository.InsertAsync(new CashFlowWorksheetLine(GuidGenerator.Create(), forecast.No, lineNo, line.Date, line.Type, line.SourceNo, line.DocumentNo, line.Description, line.Amount));
        }

        return lineNo / 10000;
    }

    /// <summary>
    /// Registers the worksheet: its lines become the forecast's entries, replacing those of an
    /// earlier registration, and the worksheet is emptied.
    /// </summary>
    public async Task<int> RegisterAsync(Guid forecastId)
    {
        var forecast = await _forecastRepository.GetAsync(forecastId);
        var lines = await _worksheetRepository.GetListAsync(l => l.CashFlowForecastNo == forecast.No);

        var old = await _entryRepository.GetListAsync(e => e.CashFlowForecastNo == forecast.No);
        foreach (var entry in old)
        {
            await _entryRepository.DeleteAsync(entry);
        }

        var generator = Repo<IEntryNoGenerator>();
        foreach (var line in lines.OrderBy(l => l.LineNo))
        {
            await _entryRepository.InsertAsync(new CashFlowForecastEntry(GuidGenerator.Create(), await generator.NextAsync(ErpSequenceNames.CashFlowForecastEntry), line));
            await _worksheetRepository.DeleteAsync(line);
        }

        return lines.Count;
    }

    /// <summary>The registered forecast summed by source type per period.</summary>
    public async Task<(List<AnalysisPeriod> Periods, List<CashFlowPeriodRow> Rows)> ByPeriodAsync(string forecastNo, DateTime startDate, AnalysisPeriodType periodType, int periodCount)
    {
        var periods = AnalysisPeriods.Build(startDate, periodType, periodCount);
        var entries = await _entryRepository.GetListAsync(e => e.CashFlowForecastNo == forecastNo);
        var rows = entries
            .GroupBy(e => e.SourceType)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var amounts = periods.Select(p => g.Where(e => p.Contains(e.CashFlowDate)).Sum(e => e.Amount)).ToArray();
                return new CashFlowPeriodRow(g.Key, amounts, amounts.Sum());
            })
            .ToList();
        return (periods, rows);
    }
}
