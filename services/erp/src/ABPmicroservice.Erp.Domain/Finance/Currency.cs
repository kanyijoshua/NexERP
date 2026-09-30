using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Currency. Mirrors Business Central table 4: a foreign currency customers, vendors and bank
/// accounts may deal in. The local currency (LCY) is not a row here; it is the General Ledger
/// Setup's LCY Code.
/// </summary>
public class Currency : CodeTableEntity
{
    protected override int MaxCodeLength => ErpDomainConsts.MaxCurrencyCodeLength;

    public string Symbol { get; private set; }
    public decimal AmountRoundingPrecision { get; private set; } = GeneralLedgerSetup.DefaultAmountRoundingPrecision;
    public string RealizedGainsAccountNo { get; private set; }
    public string RealizedLossesAccountNo { get; private set; }

    /// <summary>Credited when an exchange rate adjustment revalues open entries upwards.</summary>
    public string UnrealizedGainsAccountNo { get; private set; }

    public string UnrealizedLossesAccountNo { get; private set; }

    protected Currency() { }

    public Currency(Guid id, string code, string description, string symbol = null)
        : base(id, code, description)
    {
        SetSymbol(symbol);
    }

    public void SetSymbol(string symbol)
    {
        Symbol = Check.Length(symbol?.Trim(), nameof(symbol), ErpDomainConsts.MaxCurrencySymbolLength);
    }

    public void SetAmountRoundingPrecision(decimal precision)
    {
        if (precision <= 0)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.InvalidRoundingPrecision);
        }

        AmountRoundingPrecision = precision;
    }

    public void SetGainLossAccounts(string realizedGainsAccountNo, string realizedLossesAccountNo)
    {
        RealizedGainsAccountNo = PostingAccount.Normalize(realizedGainsAccountNo, nameof(realizedGainsAccountNo));
        RealizedLossesAccountNo = PostingAccount.Normalize(realizedLossesAccountNo, nameof(realizedLossesAccountNo));
    }

    public void SetUnrealizedAccounts(string unrealizedGainsAccountNo, string unrealizedLossesAccountNo)
    {
        UnrealizedGainsAccountNo = PostingAccount.Normalize(unrealizedGainsAccountNo, nameof(unrealizedGainsAccountNo));
        UnrealizedLossesAccountNo = PostingAccount.Normalize(unrealizedLossesAccountNo, nameof(unrealizedLossesAccountNo));
    }

    /// <summary>
    /// The account a gain or loss in this currency goes to: realized when a payment settles an
    /// entry at another rate, unrealized when an adjustment revalues an open one.
    /// </summary>
    public string GetGainLossAccount(bool gain, bool realized)
    {
        var accountNo = realized
            ? gain ? RealizedGainsAccountNo : RealizedLossesAccountNo
            : gain ? UnrealizedGainsAccountNo : UnrealizedLossesAccountNo;

        return accountNo
            ?? throw new BusinessException(ErpErrorCodes.GeneralLedger.CurrencyAccountMissing)
                .WithData("currencyCode", Code)
                .WithData("account", (realized ? "Realized" : "Unrealized") + (gain ? "Gains" : "Losses") + "AccountNo");
    }
}

/// <summary>
/// Currency Exchange Rate. Mirrors Business Central table 330: from its starting date on,
/// <see cref="ExchangeRateAmount"/> units of the currency are worth
/// <see cref="RelationalExchangeRateAmount"/> units of LCY.
/// </summary>
public class CurrencyExchangeRate : CompanyEntity
{
    public string CurrencyCode { get; private set; }
    public DateTime StartingDate { get; private set; }
    public decimal ExchangeRateAmount { get; private set; }
    public decimal RelationalExchangeRateAmount { get; private set; }

    protected CurrencyExchangeRate() { }

    public CurrencyExchangeRate(Guid id, string currencyCode, DateTime startingDate, decimal exchangeRateAmount, decimal relationalExchangeRateAmount)
        : base(id)
    {
        Set(currencyCode, startingDate, exchangeRateAmount, relationalExchangeRateAmount);
    }

    public void Set(string currencyCode, DateTime startingDate, decimal exchangeRateAmount, decimal relationalExchangeRateAmount)
    {
        Check.NotNullOrWhiteSpace(currencyCode, nameof(currencyCode), ErpDomainConsts.MaxCodeLength);
        if (exchangeRateAmount <= 0 || relationalExchangeRateAmount <= 0)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidExchangeRate);
        }

        CurrencyCode = CodeTableEntity.NormalizeCode(currencyCode);
        StartingDate = startingDate.Date;
        ExchangeRateAmount = exchangeRateAmount;
        RelationalExchangeRateAmount = relationalExchangeRateAmount;
    }

    /// <summary>An amount in the currency, in LCY at this rate.</summary>
    public decimal ToLcy(decimal amount)
    {
        return amount * RelationalExchangeRateAmount / ExchangeRateAmount;
    }
}

/// <summary>
/// Turns foreign currency amounts into LCY. Mirrors the parts of Business Central table 330's
/// functions the posting routines use: the rate in force on a date and the "Currency Factor"
/// (units of currency per unit of LCY) a document or journal line carries.
/// </summary>
public class CurrencyExchangeRateManager : DomainService
{
    private readonly IRepository<CurrencyExchangeRate, Guid> _rateRepository;
    private readonly IRepository<Currency, Guid> _currencyRepository;
    private readonly GeneralLedgerSetupManager _glSetupManager;

    public CurrencyExchangeRateManager(
        IRepository<CurrencyExchangeRate, Guid> rateRepository,
        IRepository<Currency, Guid> currencyRepository,
        GeneralLedgerSetupManager glSetupManager
    )
    {
        _rateRepository = rateRepository;
        _currencyRepository = currencyRepository;
        _glSetupManager = glSetupManager;
    }

    /// <summary>
    /// The currency a posting is in, or null for LCY. The LCY code itself counts as LCY, so a
    /// customer set up with the local currency's code posts without a rate.
    /// </summary>
    public async Task<string> NormalizeAsync(string currencyCode)
    {
        var code = CodeTableEntity.NormalizeCode(currencyCode);
        if (code == null)
        {
            return null;
        }

        var lcyCode = (await _glSetupManager.GetAsync()).LcyCode;
        return string.Equals(code, lcyCode, StringComparison.OrdinalIgnoreCase) ? null : code;
    }

    /// <summary>
    /// Units of the currency one unit of LCY buys on <paramref name="date"/>, from the latest rate
    /// starting on or before it. 1 for LCY. BC "ExchangeRate" / "Currency Factor".
    /// </summary>
    public async Task<decimal> GetCurrencyFactorAsync(string currencyCode, DateTime date)
    {
        var code = await NormalizeAsync(currencyCode);
        if (code == null)
        {
            return 1m;
        }

        var day = date.Date;
        var rate = (await _rateRepository.GetListAsync(r => r.CurrencyCode == code && r.StartingDate <= day))
            .OrderByDescending(r => r.StartingDate)
            .FirstOrDefault();

        if (rate == null)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.ExchangeRateNotFound)
                .WithData("currencyCode", code)
                .WithData("date", day.ToString("yyyy-MM-dd"));
        }

        return rate.ExchangeRateAmount / rate.RelationalExchangeRateAmount;
    }

    public async Task<Currency> GetCurrencyAsync(string currencyCode)
    {
        var code = CodeTableEntity.NormalizeCode(currencyCode);
        return await _currencyRepository.FirstOrDefaultAsync(c => c.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound)
                .WithData("table", nameof(Currency))
                .WithData("code", code ?? "");
    }

    /// <summary>An amount in a currency, in LCY at <paramref name="currencyFactor"/>, rounded to cents.</summary>
    public static decimal ToLcy(decimal amount, decimal currencyFactor)
    {
        return currencyFactor == 1m || currencyFactor <= 0m
            ? amount
            : RoundLcy(amount / currencyFactor);
    }

    public static decimal RoundLcy(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
