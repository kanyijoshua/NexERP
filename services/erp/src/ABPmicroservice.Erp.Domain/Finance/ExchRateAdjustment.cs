using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Exch. Rate Adjmt. Register. Mirrors Business Central table 86: one row per customer, vendor or
/// bank account an adjustment run revalued, with the amounts it was carried at before and after.
/// </summary>
public class ExchRateAdjmtRegister : CompanyEntity
{
    public DateTime PostingDate { get; private set; }
    public string DocumentNo { get; private set; }
    public ExchRateAdjmtAccountType AccountType { get; private set; }
    public string AccountNo { get; private set; }
    public string CurrencyCode { get; private set; }
    public decimal CurrencyFactor { get; private set; }

    /// <summary>The open amount revalued, in the currency.</summary>
    public decimal AdjustedBase { get; private set; }

    /// <summary>What that amount was carried at in LCY before the run.</summary>
    public decimal AdjustedBaseLcy { get; private set; }

    /// <summary>The gain (positive) or loss (negative) posted.</summary>
    public decimal AdjustedAmount { get; private set; }

    public long GLRegisterNo { get; private set; }

    protected ExchRateAdjmtRegister() { }

    public ExchRateAdjmtRegister(
        Guid id,
        DateTime postingDate,
        string documentNo,
        ExchRateAdjustmentLine line,
        long glRegisterNo
    )
        : base(id)
    {
        PostingDate = postingDate;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        AccountType = line.AccountType;
        AccountNo = line.AccountNo;
        CurrencyCode = line.CurrencyCode;
        CurrencyFactor = line.CurrencyFactor;
        AdjustedBase = line.Base;
        AdjustedBaseLcy = line.OldAmountLcy;
        AdjustedAmount = line.Difference;
        GLRegisterNo = glRegisterNo;
    }
}

public class ExchRateAdjustmentRequest
{
    /// <summary>Entries posted up to this date are revalued, at the rate in force on it.</summary>
    public DateTime EndingDate { get; set; }

    public DateTime PostingDate { get; set; }

    public string DocumentNo { get; set; }

    /// <summary>Blank adjusts every currency.</summary>
    public string CurrencyCode { get; set; }

    public bool AdjustCustomers { get; set; } = true;

    public bool AdjustVendors { get; set; } = true;

    public bool AdjustBankAccounts { get; set; } = true;
}

public class ExchRateAdjustmentLine
{
    public ExchRateAdjmtAccountType AccountType { get; set; }
    public string AccountNo { get; set; }
    public string CurrencyCode { get; set; }
    public decimal CurrencyFactor { get; set; }
    public decimal Base { get; set; }
    public decimal OldAmountLcy { get; set; }
    public decimal NewAmountLcy { get; set; }

    /// <summary>Positive is an unrealized gain, negative a loss.</summary>
    public decimal Difference { get; set; }
}

public class ExchRateAdjustmentResult
{
    public List<ExchRateAdjustmentLine> Lines { get; set; } = new();
    public decimal TotalGains { get; set; }
    public decimal TotalLosses { get; set; }
    public bool Posted { get; set; }
    public long RegisterNo { get; set; }
}

/// <summary>
/// Revalues what is open in foreign currencies. Mirrors Business Central report 596 "Exch. Rate
/// Adjustment": the remaining amount of every open customer and vendor entry, and the balance of
/// every foreign currency bank account, is converted at the rate on the ending date. The change
/// against what it is carried at goes to the control account and, opposite, to the currency's
/// unrealized gain or loss account; the entries then carry the new LCY amount, so a later payment
/// only realizes what moved since.
/// </summary>
public class ExchRateAdjustmentEngine : DomainService
{
    private readonly IRepository<Currency, Guid> _currencyRepository;
    private readonly IRepository<CustomerLedgerEntry, Guid> _customerEntryRepository;
    private readonly IRepository<VendorLedgerEntry, Guid> _vendorEntryRepository;
    private readonly IRepository<BankAccountLedgerEntry, Guid> _bankEntryRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IRepository<BankAccount, Guid> _bankAccountRepository;
    private readonly IRepository<ExchRateAdjmtRegister, Guid> _adjmtRegisterRepository;
    private readonly CurrencyExchangeRateManager _currencyManager;
    private readonly GenJnlPostLine _postLine;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;

    public ExchRateAdjustmentEngine(
        IRepository<Currency, Guid> currencyRepository,
        IRepository<CustomerLedgerEntry, Guid> customerEntryRepository,
        IRepository<VendorLedgerEntry, Guid> vendorEntryRepository,
        IRepository<BankAccountLedgerEntry, Guid> bankEntryRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vendor, Guid> vendorRepository,
        IRepository<BankAccount, Guid> bankAccountRepository,
        IRepository<ExchRateAdjmtRegister, Guid> adjmtRegisterRepository,
        CurrencyExchangeRateManager currencyManager,
        GenJnlPostLine postLine,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager
    )
    {
        _currencyRepository = currencyRepository;
        _customerEntryRepository = customerEntryRepository;
        _vendorEntryRepository = vendorEntryRepository;
        _bankEntryRepository = bankEntryRepository;
        _customerRepository = customerRepository;
        _vendorRepository = vendorRepository;
        _bankAccountRepository = bankAccountRepository;
        _adjmtRegisterRepository = adjmtRegisterRepository;
        _currencyManager = currencyManager;
        _postLine = postLine;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
    }

    /// <summary>What an adjustment would post, without writing anything.</summary>
    public async Task<ExchRateAdjustmentResult> CalculateAsync(ExchRateAdjustmentRequest request)
    {
        var plan = await PlanAsync(request);
        return Result(plan.Select(p => p.Line).ToList());
    }

    public async Task<ExchRateAdjustmentResult> PostAsync(ExchRateAdjustmentRequest request)
    {
        if (request.DocumentNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.NoSeries.NumberRequired);
        }

        await _glSetupManager.CheckPostingDateAsync(request.PostingDate);

        var plan = (await PlanAsync(request)).Where(p => p.Line.Difference != 0m).ToList();
        if (plan.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.NothingToAdjust);
        }

        var documentNo = request.DocumentNo.Trim();
        var register = await _registerManager.OpenAsync(request.PostingDate, GLRegister.ExchRateAdjustmentSourceCode, documentNo);
        var context = new GLPostingContext(register, GLRegister.ExchRateAdjustmentSourceCode);
        var currencies = new Dictionary<string, Currency>();

        foreach (var item in plan)
        {
            var line = item.Line;
            var description = $"Exch. rate adjmt. {line.CurrencyCode} {line.AccountNo}";

            if (!currencies.TryGetValue(line.CurrencyCode, out var currency))
            {
                currency = await _currencyManager.GetCurrencyAsync(line.CurrencyCode);
                currencies[line.CurrencyCode] = currency;
            }

            // Resolved first, so a missing account stops the run before anything is written.
            var gainLossAccountNo = currency.GetGainLossAccount(gain: line.Difference > 0m, realized: false);

            switch (line.AccountType)
            {
                case ExchRateAdjmtAccountType.Customer:
                    var customer = await _customerRepository.FirstAsync(c => c.No == line.AccountNo);
                    var receivables = await _postLine.GetReceivablesAccountNoAsync(customer);
                    // One by one: UpdateMany marks every column changed, which the append-only guard refuses.
                    foreach (var (entry, newLcy) in item.CustomerEntries)
                    {
                        entry.AdjustRemainingLcy(newLcy);
                        await _customerEntryRepository.UpdateAsync(entry);
                    }

                    customer.ApplyBalance(line.Difference);
                    await _customerRepository.UpdateAsync(customer);
                    await PostGLAsync(receivables, line.Difference, request, documentNo, description, line.AccountNo, context);
                    break;

                case ExchRateAdjmtAccountType.Vendor:
                    var vendor = await _vendorRepository.FirstAsync(v => v.No == line.AccountNo);
                    var payables = await _postLine.GetPayablesAccountNoAsync(vendor);
                    foreach (var (entry, newLcy) in item.VendorEntries)
                    {
                        entry.AdjustRemainingLcy(newLcy);
                        await _vendorEntryRepository.UpdateAsync(entry);
                    }

                    vendor.ApplyBalance(line.Difference);
                    await _vendorRepository.UpdateAsync(vendor);
                    await PostGLAsync(payables, line.Difference, request, documentNo, description, line.AccountNo, context);
                    break;

                case ExchRateAdjmtAccountType.BankAccount:
                    var bankAccount = await _bankAccountRepository.FirstAsync(b => b.No == line.AccountNo);
                    await _postLine.PostBankAdjustmentAsync(bankAccount, line.Difference, request.PostingDate, documentNo, description, context);
                    break;
            }

            await PostGLAsync(gainLossAccountNo, -line.Difference, request, documentNo, description, line.AccountNo, context);
            await _adjmtRegisterRepository.InsertAsync(new ExchRateAdjmtRegister(GuidGenerator.Create(), request.PostingDate, documentNo, line, register.No));
        }

        await _registerManager.CloseAsync(register);

        var result = Result(plan.Select(p => p.Line).ToList());
        result.Posted = true;
        result.RegisterNo = register.No;
        return result;
    }

    private Task<GLEntry> PostGLAsync(
        string accountNo,
        decimal amount,
        ExchRateAdjustmentRequest request,
        string documentNo,
        string description,
        string sourceNo,
        GLPostingContext context
    )
    {
        return _postLine.PostGLDirectAsync(accountNo, request.PostingDate, GLEntryDocumentType.None, documentNo, description, amount, sourceNo, context: context);
    }

    private static ExchRateAdjustmentResult Result(List<ExchRateAdjustmentLine> lines)
    {
        return new ExchRateAdjustmentResult
        {
            Lines = lines,
            TotalGains = lines.Where(l => l.Difference > 0m).Sum(l => l.Difference),
            TotalLosses = -lines.Where(l => l.Difference < 0m).Sum(l => l.Difference),
        };
    }

    private sealed class PlanItem
    {
        public ExchRateAdjustmentLine Line { get; init; }
        public List<(CustomerLedgerEntry Entry, decimal NewLcy)> CustomerEntries { get; init; } = new();
        public List<(VendorLedgerEntry Entry, decimal NewLcy)> VendorEntries { get; init; } = new();
    }

    private async Task<List<PlanItem>> PlanAsync(ExchRateAdjustmentRequest request)
    {
        var end = request.EndingDate.Date;
        var filter = CodeTableEntity.NormalizeCode(request.CurrencyCode);

        var codes = (await _currencyRepository.GetListAsync())
            .Select(c => c.Code)
            .Where(c => filter == null || c == filter)
            .OrderBy(c => c)
            .ToList();

        var plan = new List<PlanItem>();
        foreach (var code in codes)
        {
            // The local currency never needs revaluing, even if someone set it up as a row too.
            if (await _currencyManager.NormalizeAsync(code) == null)
            {
                continue;
            }

            decimal? factor = null;
            async Task<decimal> FactorAsync() => factor ??= await _currencyManager.GetCurrencyFactorAsync(code, end);

            if (request.AdjustCustomers)
            {
                var entries = await _customerEntryRepository.GetListAsync(e => e.Open && e.CurrencyCode == code && e.PostingDate <= end);
                foreach (var party in entries.GroupBy(e => e.CustomerNo).OrderBy(g => g.Key))
                {
                    var f = await FactorAsync();
                    var item = new PlanItem { Line = PartyLine(ExchRateAdjmtAccountType.Customer, party.Key, code, f) };
                    foreach (var entry in party)
                    {
                        var newLcy = CurrencyExchangeRateManager.ToLcy(entry.RemainingAmount, f);
                        item.CustomerEntries.Add((entry, newLcy));
                        Accumulate(item.Line, entry.RemainingAmount, entry.RemainingAmountLcy, newLcy);
                    }

                    plan.Add(item);
                }
            }

            if (request.AdjustVendors)
            {
                var entries = await _vendorEntryRepository.GetListAsync(e => e.Open && e.CurrencyCode == code && e.PostingDate <= end);
                foreach (var party in entries.GroupBy(e => e.VendorNo).OrderBy(g => g.Key))
                {
                    var f = await FactorAsync();
                    var item = new PlanItem { Line = PartyLine(ExchRateAdjmtAccountType.Vendor, party.Key, code, f) };
                    foreach (var entry in party)
                    {
                        var newLcy = CurrencyExchangeRateManager.ToLcy(entry.RemainingAmount, f);
                        item.VendorEntries.Add((entry, newLcy));
                        Accumulate(item.Line, entry.RemainingAmount, entry.RemainingAmountLcy, newLcy);
                    }

                    plan.Add(item);
                }
            }

            if (request.AdjustBankAccounts)
            {
                var bankAccounts = await _bankAccountRepository.GetListAsync(b => b.CurrencyCode == code);
                foreach (var bankAccount in bankAccounts.OrderBy(b => b.No))
                {
                    var entries = await _bankEntryRepository.GetListAsync(e => e.BankAccountId == bankAccount.Id && e.PostingDate <= end);
                    if (entries.Count == 0)
                    {
                        continue;
                    }

                    var f = await FactorAsync();
                    var balance = entries.Sum(e => e.Amount);
                    var line = PartyLine(ExchRateAdjmtAccountType.BankAccount, bankAccount.No, code, f);
                    Accumulate(line, balance, entries.Sum(e => e.AmountLcy), CurrencyExchangeRateManager.ToLcy(balance, f));
                    plan.Add(new PlanItem { Line = line });
                }
            }
        }

        return plan;
    }

    private static ExchRateAdjustmentLine PartyLine(ExchRateAdjmtAccountType type, string no, string currencyCode, decimal factor) =>
        new() { AccountType = type, AccountNo = no, CurrencyCode = currencyCode, CurrencyFactor = factor };

    private static void Accumulate(ExchRateAdjustmentLine line, decimal @base, decimal oldLcy, decimal newLcy)
    {
        line.Base += @base;
        line.OldAmountLcy += oldLcy;
        line.NewAmountLcy += newLcy;
        line.Difference = line.NewAmountLcy - line.OldAmountLcy;
    }
}
