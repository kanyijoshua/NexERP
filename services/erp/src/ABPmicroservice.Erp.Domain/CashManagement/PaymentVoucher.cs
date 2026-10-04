using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.CashManagement;

/// <summary>Cash Management Setup: one row per company with the number series of its documents.</summary>
public class CashManagementSetup : CompanyEntity
{
    public string PaymentVoucherNos { get; private set; }

    protected CashManagementSetup() { }

    public CashManagementSetup(Guid id)
        : base(id) { }

    public void SetNumbering(string paymentVoucherNos)
    {
        PaymentVoucherNos = paymentVoucherNos.IsNullOrWhiteSpace()
            ? null
            : Check.Length(paymentVoucherNos.Trim(), nameof(paymentVoucherNos), ErpDomainConsts.MaxNoSeriesCodeLength);
    }
}

public class CashManagementSetupManager : DomainService
{
    private readonly IRepository<CashManagementSetup, Guid> _repository;

    public CashManagementSetupManager(IRepository<CashManagementSetup, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>The current company's setup, created blank on first use.</summary>
    public async Task<CashManagementSetup> GetAsync()
    {
        return await _repository.FirstOrDefaultAsync()
            ?? await _repository.InsertAsync(new CashManagementSetup(GuidGenerator.Create()), autoSave: true);
    }
}

/// <summary>
/// Something withheld from a payment: withholding tax, withholding VAT or retention. The rate is
/// applied to the amount excluding VAT, and what is withheld is owed on the payable account
/// instead of being paid out of the bank.
/// </summary>
public class PaymentDeductionCode : CodeTableEntity
{
    public PaymentDeductionType DeductionType { get; private set; }
    public decimal RatePct { get; private set; }

    /// <summary>The liability account the withheld amount is credited to.</summary>
    public string PayableAccountNo { get; private set; }

    protected PaymentDeductionCode() { }

    public PaymentDeductionCode(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(PaymentDeductionType deductionType, decimal ratePct, string payableAccountNo)
    {
        if (ratePct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", ratePct);
        }

        DeductionType = deductionType;
        RatePct = ratePct;
        PayableAccountNo = payableAccountNo.IsNullOrWhiteSpace()
            ? null
            : Check.Length(payableAccountNo.Trim(), nameof(payableAccountNo), ErpDomainConsts.MaxNoLength);
    }
}

/// <summary>
/// A kind of payment (supplier payment, professional fees, rent, ...). Picking one on a voucher
/// line fills in the account and the deductions that payments of that kind normally carry.
/// </summary>
public class PaymentType : CodeTableEntity
{
    public GenJournalAccountType AccountType { get; private set; } = GenJournalAccountType.GLAccount;

    /// <summary>The account the line defaults to; blank leaves it to be chosen on the line.</summary>
    public string AccountNo { get; private set; }

    /// <summary>The VAT rate contained in amounts of this kind, used to find the amount excluding VAT.</summary>
    public decimal VatRatePct { get; private set; }

    public string WithholdingTaxCode { get; private set; }
    public string WithholdingVatCode { get; private set; }
    public string RetentionCode { get; private set; }
    public bool Blocked { get; private set; }

    protected PaymentType() { }

    public PaymentType(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetAccount(GenJournalAccountType accountType, string accountNo)
    {
        PaymentVoucherLine.EnsureLineAccountType(accountType);
        AccountType = accountType;
        AccountNo = accountNo.IsNullOrWhiteSpace() ? null : Check.Length(accountNo.Trim(), nameof(accountNo), ErpDomainConsts.MaxNoLength);
    }

    public void SetDeductions(decimal vatRatePct, string withholdingTaxCode, string withholdingVatCode, string retentionCode)
    {
        if (vatRatePct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", vatRatePct);
        }

        VatRatePct = vatRatePct;
        WithholdingTaxCode = NormalizeCode(Check.Length(withholdingTaxCode, nameof(withholdingTaxCode), ErpDomainConsts.MaxCodeLength));
        WithholdingVatCode = NormalizeCode(Check.Length(withholdingVatCode, nameof(withholdingVatCode), ErpDomainConsts.MaxCodeLength));
        RetentionCode = NormalizeCode(Check.Length(retentionCode, nameof(retentionCode), ErpDomainConsts.MaxCodeLength));
    }

    public void SetBlocked(bool blocked) => Blocked = blocked;
}

/// <summary>
/// A payment voucher: the request to pay one payee out of one bank account. It is prepared
/// (Open), approved or released, and then posted: the accounts on its lines are debited, what is
/// withheld is credited to the deduction accounts and the net amount leaves the bank.
/// </summary>
public class PaymentVoucherHeader : CompanyEntity, IHasNo, IApprovalDocument
{
    public const int MaxPayeeLength = 100;
    public const int MaxNarrationLength = 100;

    public string No { get; private set; }
    public DateTime DocumentDate { get; private set; }

    /// <summary>The date the payment is made on, and the date its entries carry.</summary>
    public DateTime PostingDate { get; private set; }

    /// <summary>How the payee is paid: a Payment Method code (cheque, transfer, cash).</summary>
    public string PayMode { get; private set; }

    public string PayingBankAccountNo { get; private set; }

    /// <summary>The paying bank account's currency; null for LCY. Every amount on the voucher is in it.</summary>
    public string CurrencyCode { get; private set; }

    public string Payee { get; private set; }
    public string OnBehalfOf { get; private set; }
    public string PaymentNarration { get; private set; }

    /// <summary>The cheque or transfer reference; stamped on the entries as their external document number.</summary>
    public string ChequeNo { get; private set; }

    public DateTime? ChequeDate { get; private set; }
    public DocumentStatus Status { get; private set; }

    public decimal TotalAmount { get; internal set; }
    public decimal TotalWithholdingTaxAmount { get; internal set; }
    public decimal TotalWithholdingVatAmount { get; internal set; }
    public decimal TotalRetentionAmount { get; internal set; }

    /// <summary>What leaves the bank: the total less everything withheld.</summary>
    public decimal TotalNetAmount { get; internal set; }

    public int NoOfLines { get; internal set; }

    public DateTime? PostedDate { get; private set; }
    public string PostedBy { get; private set; }

    /// <summary>The kind of document that raised the voucher, e.g. "StudentRefund"; blank on a voucher entered by hand.</summary>
    public string SourceType { get; private set; }

    public string SourceNo { get; private set; }

    protected PaymentVoucherHeader() { }

    /// <summary>Records the document the voucher pays, so that either can be found from the other.</summary>
    public void SetSource(string sourceType, string sourceNo)
    {
        SourceType = Text(sourceType, nameof(sourceType), ErpDomainConsts.MaxEntityNameLength);
        SourceNo = Text(sourceNo, nameof(sourceNo), ErpDomainConsts.MaxDocumentNoLength);
    }

    public PaymentVoucherHeader(Guid id, string no, DateTime documentDate, DateTime postingDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        SetDates(documentDate, postingDate);
    }

    public bool HasLines => NoOfLines > 0;

    /// <summary>Approval limits and workflow thresholds are compared with the gross amount.</summary>
    public decimal ApprovalAmount => TotalAmount;

    public void SetDates(DateTime documentDate, DateTime postingDate)
    {
        EnsureOpen();
        DocumentDate = documentDate.Date;
        PostingDate = postingDate.Date;
    }

    public void SetPayment(string payMode, BankAccount payingBankAccount, string currencyCode)
    {
        EnsureOpen();
        PayMode = CodeTableEntity.NormalizeCode(Check.Length(payMode, nameof(payMode), ErpDomainConsts.MaxCodeLength));
        PayingBankAccountNo = payingBankAccount?.No;
        CurrencyCode = payingBankAccount == null ? null : currencyCode;
    }

    public void SetPayee(string payee, string onBehalfOf, string paymentNarration)
    {
        EnsureOpen();
        Payee = Text(payee, nameof(payee), MaxPayeeLength);
        OnBehalfOf = Text(onBehalfOf, nameof(onBehalfOf), MaxPayeeLength);
        PaymentNarration = Text(paymentNarration, nameof(paymentNarration), MaxNarrationLength);
    }

    /// <summary>The cheque is usually written after approval, so it may be filled in until the voucher is posted.</summary>
    public void SetCheque(string chequeNo, DateTime? chequeDate)
    {
        if (Status == DocumentStatus.Posted)
        {
            throw new BusinessException(ErpErrorCodes.Documents.CannotModifyPostedDocument).WithData("documentNo", No);
        }

        ChequeNo = Text(chequeNo, nameof(chequeNo), ErpDomainConsts.MaxExternalDocumentNoLength);
        ChequeDate = chequeDate?.Date;
    }

    public void SendForApproval()
    {
        EnsureOpen();
        Status = DocumentStatus.PendingApproval;
    }

    public void Release()
    {
        if (Status is not (DocumentStatus.Open or DocumentStatus.PendingApproval))
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.VoucherNotOpen).WithData("documentNo", No);
        }

        Status = DocumentStatus.Released;
    }

    public void Reopen()
    {
        if (Status == DocumentStatus.Posted)
        {
            throw new BusinessException(ErpErrorCodes.Documents.CannotModifyPostedDocument).WithData("documentNo", No);
        }

        Status = DocumentStatus.Open;
    }

    internal void MarkPosted(DateTime when, string by)
    {
        Status = DocumentStatus.Posted;
        PostedDate = when;
        PostedBy = by;
    }

    /// <summary>Only an open voucher may be changed; once sent for approval it is paid as it was approved.</summary>
    public void EnsureOpen()
    {
        if (Status != DocumentStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.VoucherNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }

    private static string Text(string value, string name, int maxLength) =>
        value.IsNullOrWhiteSpace() ? null : Check.Length(value.Trim(), name, maxLength);
}

/// <summary>One amount on a voucher: the account it is paid on behalf of and what is withheld from it.</summary>
public class PaymentVoucherLine : CompanyEntity
{
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }
    public string PaymentTypeCode { get; private set; }
    public GenJournalAccountType AccountType { get; private set; }
    public string AccountNo { get; private set; }
    public string AccountName { get; private set; }
    public string Description { get; private set; }

    /// <summary>The invoice of the vendor, customer or employee this amount settles.</summary>
    public string AppliesToDocNo { get; private set; }

    /// <summary>The gross amount, VAT included.</summary>
    public decimal Amount { get; private set; }

    /// <summary>The VAT rate contained in the amount; deductions are worked out on the amount without it.</summary>
    public decimal VatRatePct { get; private set; }

    public string WithholdingTaxCode { get; private set; }
    public decimal WithholdingTaxAmount { get; private set; }
    public string WithholdingVatCode { get; private set; }
    public decimal WithholdingVatAmount { get; private set; }
    public string RetentionCode { get; private set; }
    public decimal RetentionAmount { get; private set; }

    /// <summary>What is paid out for this line: the amount less everything withheld.</summary>
    public decimal NetAmount { get; private set; }

    protected PaymentVoucherLine() { }

    public PaymentVoucherLine(Guid id, string documentNo, int lineNo)
        : base(id)
    {
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        LineNo = lineNo;
    }

    public decimal TotalDeductions => WithholdingTaxAmount + WithholdingVatAmount + RetentionAmount;

    /// <summary>The amount without the VAT it contains: what the deduction rates are applied to.</summary>
    public decimal AmountExcludingVat => Round(Amount / (1m + VatRatePct / 100m));

    public void SetAccount(string paymentTypeCode, GenJournalAccountType accountType, string accountNo, string accountName)
    {
        EnsureLineAccountType(accountType);
        PaymentTypeCode = CodeTableEntity.NormalizeCode(Check.Length(paymentTypeCode, nameof(paymentTypeCode), ErpDomainConsts.MaxCodeLength));
        AccountType = accountType;
        AccountNo = Check.NotNullOrWhiteSpace(accountNo, nameof(accountNo), ErpDomainConsts.MaxNoLength).Trim();
        AccountName = accountName.IsNullOrWhiteSpace() ? null : accountName.Trim().Truncate(ErpDomainConsts.MaxNameLength);
    }

    public void SetDetails(string description, string appliesToDocNo)
    {
        Description = description.IsNullOrWhiteSpace() ? null : Check.Length(description.Trim(), nameof(description), ErpDomainConsts.MaxDescriptionLength);

        appliesToDocNo = appliesToDocNo.IsNullOrWhiteSpace() ? null : Check.Length(appliesToDocNo.Trim(), nameof(appliesToDocNo), ErpDomainConsts.MaxDocumentNoLength);
        // Only a customer, vendor or employee has open entries to apply to.
        AppliesToDocNo = AccountType == GenJournalAccountType.GLAccount ? null : appliesToDocNo;
    }

    /// <summary>
    /// Sets the amount and works out what is withheld from it. Each deduction is its rate on the
    /// amount excluding VAT; withholding VAT applies only to an amount that contains VAT.
    /// </summary>
    public void SetAmounts(
        decimal amount,
        decimal vatRatePct,
        PaymentDeductionCode withholdingTax,
        PaymentDeductionCode withholdingVat,
        PaymentDeductionCode retention
    )
    {
        if (amount <= 0m)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.VoucherAmountNotPositive);
        }

        if (vatRatePct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", vatRatePct);
        }

        EnsureType(withholdingTax, PaymentDeductionType.WithholdingTax);
        EnsureType(withholdingVat, PaymentDeductionType.WithholdingVat);
        EnsureType(retention, PaymentDeductionType.Retention);

        Amount = Round(amount);
        VatRatePct = vatRatePct;

        var @base = AmountExcludingVat;
        WithholdingTaxCode = withholdingTax?.Code;
        WithholdingTaxAmount = withholdingTax == null ? 0m : Round(@base * withholdingTax.RatePct / 100m);
        WithholdingVatCode = withholdingVat?.Code;
        WithholdingVatAmount = withholdingVat == null || vatRatePct == 0m ? 0m : Round(@base * withholdingVat.RatePct / 100m);
        RetentionCode = retention?.Code;
        RetentionAmount = retention == null ? 0m : Round(@base * retention.RatePct / 100m);

        NetAmount = Amount - TotalDeductions;
        if (NetAmount < 0m)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.DeductionExceedsAmount).WithData("amount", Amount);
        }
    }

    /// <summary>A bank account is what a voucher pays from, never what it pays.</summary>
    internal static void EnsureLineAccountType(GenJournalAccountType accountType)
    {
        if (accountType == GenJournalAccountType.BankAccount)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.InvalidVoucherAccountType);
        }
    }

    private static void EnsureType(PaymentDeductionCode code, PaymentDeductionType expected)
    {
        if (code != null && code.DeductionType != expected)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.WrongDeductionType)
                .WithData("code", code.Code)
                .WithData("type", expected.ToString());
        }
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

/// <summary>
/// Prepares, releases and posts payment vouchers.
/// <para>
/// Posting debits each line's account with the gross amount (applied to the invoice the line
/// names), credits each deduction's payable account with what was withheld, and credits the
/// paying bank account with the net amount, all under the voucher's number in one register.
/// </para>
/// </summary>
public class PaymentVoucherEngine : DomainService
{
    public const string SourceCode = "PAYVOUCH";

    private readonly IRepository<PaymentVoucherHeader, Guid> _headers;
    private readonly IRepository<PaymentVoucherLine, Guid> _lines;
    private readonly IRepository<PaymentDeductionCode, Guid> _deductionCodes;
    private readonly IRepository<PaymentMethod, Guid> _paymentMethods;
    private readonly IRepository<BankAccount, Guid> _bankAccounts;
    private readonly IRepository<GLAccount, Guid> _glAccounts;
    private readonly IRepository<Vendor, Guid> _vendors;
    private readonly IRepository<Customer, Guid> _customers;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly CurrencyExchangeRateManager _currencyManager;
    private readonly ICurrentUser _currentUser;

    public PaymentVoucherEngine(
        IRepository<PaymentVoucherHeader, Guid> headers,
        IRepository<PaymentVoucherLine, Guid> lines,
        IRepository<PaymentDeductionCode, Guid> deductionCodes,
        IRepository<PaymentMethod, Guid> paymentMethods,
        IRepository<BankAccount, Guid> bankAccounts,
        IRepository<GLAccount, Guid> glAccounts,
        IRepository<Vendor, Guid> vendors,
        IRepository<Customer, Guid> customers,
        IRepository<Employee, Guid> employees,
        GenJnlPostLine genJnlPostLine,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager,
        CurrencyExchangeRateManager currencyManager,
        ICurrentUser currentUser
    )
    {
        _headers = headers;
        _lines = lines;
        _deductionCodes = deductionCodes;
        _paymentMethods = paymentMethods;
        _bankAccounts = bankAccounts;
        _glAccounts = glAccounts;
        _vendors = vendors;
        _customers = customers;
        _employees = employees;
        _genJnlPostLine = genJnlPostLine;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
        _currencyManager = currencyManager;
        _currentUser = currentUser;
    }

    /// <summary>The paying bank account, which must exist and not be blocked.</summary>
    public async Task<BankAccount> GetPayingBankAccountAsync(string bankAccountNo)
    {
        var no = bankAccountNo?.Trim();
        var bankAccount = await _bankAccounts.FirstOrDefaultAsync(b => b.No == no)
            ?? throw new BusinessException(ErpErrorCodes.CashManagement.BankAccountNotFound).WithData("accountNo", no ?? string.Empty);

        return bankAccount.Blocked
            ? throw new BusinessException(ErpErrorCodes.CashManagement.BankAccountBlocked).WithData("accountNo", bankAccount.No)
            : bankAccount;
    }

    /// <summary>Sets how the voucher is paid; its currency follows the paying bank account.</summary>
    public async Task SetPaymentAsync(PaymentVoucherHeader header, string payMode, string payingBankAccountNo)
    {
        var bankAccount = payingBankAccountNo.IsNullOrWhiteSpace() ? null : await GetPayingBankAccountAsync(payingBankAccountNo);
        var currencyCode = bankAccount == null ? null : await _currencyManager.NormalizeAsync(bankAccount.CurrencyCode);
        header.SetPayment(payMode, bankAccount, currencyCode);
    }

    /// <summary>The name of the account a line pays, which must exist and not be blocked.</summary>
    public async Task<string> GetAccountNameAsync(GenJournalAccountType accountType, string accountNo)
    {
        var no = accountNo?.Trim();
        (string Name, bool Blocked)? account = accountType switch
        {
            GenJournalAccountType.GLAccount => (await _glAccounts.FirstOrDefaultAsync(a => a.No == no)) is { } gl ? (gl.Name, gl.Blocked) : null,
            GenJournalAccountType.Vendor => (await _vendors.FirstOrDefaultAsync(v => v.No == no)) is { } vendor ? (vendor.Name, vendor.Blocked) : null,
            GenJournalAccountType.Customer => (await _customers.FirstOrDefaultAsync(c => c.No == no)) is { } customer ? (customer.Name, customer.Blocked) : null,
            GenJournalAccountType.Employee => (await _employees.FirstOrDefaultAsync(e => e.No == no)) is { } employee ? (employee.FullName, employee.Blocked) : null,
            _ => throw new BusinessException(ErpErrorCodes.CashManagement.InvalidVoucherAccountType),
        };

        if (account == null || account.Value.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.VoucherAccountNotFound)
                .WithData("accountType", accountType.ToString())
                .WithData("accountNo", no ?? string.Empty);
        }

        return account.Value.Name;
    }

    /// <summary>The deduction code of that name, or null for a blank code.</summary>
    public async Task<PaymentDeductionCode> GetDeductionCodeAsync(string code)
    {
        var normalized = CodeTableEntity.NormalizeCode(code);
        if (normalized == null)
        {
            return null;
        }

        return await _deductionCodes.FirstOrDefaultAsync(c => c.Code == normalized)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Payment Deduction Code").WithData("code", normalized);
    }

    /// <summary>Stores the totals of the lines on the header.</summary>
    public async Task UpdateTotalsAsync(PaymentVoucherHeader header)
    {
        var lines = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        header.TotalAmount = lines.Sum(l => l.Amount);
        header.TotalWithholdingTaxAmount = lines.Sum(l => l.WithholdingTaxAmount);
        header.TotalWithholdingVatAmount = lines.Sum(l => l.WithholdingVatAmount);
        header.TotalRetentionAmount = lines.Sum(l => l.RetentionAmount);
        header.TotalNetAmount = lines.Sum(l => l.NetAmount);
        header.NoOfLines = lines.Count;
        await _headers.UpdateAsync(header, autoSave: true);
    }

    /// <summary>What must hold before a voucher is sent for approval, released or posted. Returns its lines.</summary>
    public async Task<List<PaymentVoucherLine>> CheckAsync(PaymentVoucherHeader header)
    {
        Require(header, header.Payee, "Payee");
        Require(header, header.PayMode, "Pay Mode");
        Require(header, header.PayingBankAccountNo, "Paying Bank Account");

        if (!await _paymentMethods.AnyAsync(m => m.Code == header.PayMode))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Payment Method").WithData("code", header.PayMode);
        }

        await GetPayingBankAccountAsync(header.PayingBankAccountNo);

        var lines = (await _lines.GetListAsync(l => l.DocumentNo == header.No)).OrderBy(l => l.LineNo).ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.VoucherHasNoLines).WithData("documentNo", header.No);
        }

        return lines;
    }

    /// <summary>Releases the voucher after checking that it can be posted as it stands.</summary>
    public async Task ReleaseAsync(PaymentVoucherHeader header)
    {
        await CheckAsync(header);
        header.Release();
        await _headers.UpdateAsync(header, autoSave: true);
    }

    public async Task PostAsync(PaymentVoucherHeader header)
    {
        if (header.Status != DocumentStatus.Released)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.VoucherNotReleased).WithData("documentNo", header.No);
        }

        await _glSetupManager.CheckPostingDateAsync(header.PostingDate);
        var lines = await CheckAsync(header);

        var bankAccount = await GetPayingBankAccountAsync(header.PayingBankAccountNo);
        var currencyCode = await _currencyManager.NormalizeAsync(bankAccount.CurrencyCode);
        var currencyFactor = currencyCode == null ? 1m : await _currencyManager.GetCurrencyFactorAsync(currencyCode, header.PostingDate);

        var codes = (await _deductionCodes.GetListAsync()).ToDictionary(c => c.Code, StringComparer.Ordinal);

        var register = await _registerManager.OpenAsync(header.PostingDate, SourceCode, header.No);
        var context = new GLPostingContext(register, SourceCode);
        var lineNo = 0;
        var bankAmountLcy = 0m;

        foreach (var line in lines)
        {
            var description = line.Description ?? header.PaymentNarration ?? $"Payment to {header.Payee}";

            lineNo += 10000;
            var journalLine = NewJournalLine(header, lineNo, line.AccountType, line.AccountNo, description, line.Amount, line.AppliesToDocNo);
            journalLine.SetCurrency(currencyCode, currencyFactor);
            await _genJnlPostLine.PostLineAsync(journalLine, context);
            bankAmountLcy += journalLine.AmountLcy;

            // What is withheld stays owed to whoever it is withheld for, instead of leaving the bank.
            (string Code, decimal Amount)[] deductions =
            [
                (line.WithholdingTaxCode, line.WithholdingTaxAmount),
                (line.WithholdingVatCode, line.WithholdingVatAmount),
                (line.RetentionCode, line.RetentionAmount),
            ];

            foreach (var (code, amount) in deductions)
            {
                if (amount == 0m)
                {
                    continue;
                }

                var payableAccountNo = codes.TryGetValue(code, out var deduction) && !deduction.PayableAccountNo.IsNullOrWhiteSpace()
                    ? deduction.PayableAccountNo
                    : throw new BusinessException(ErpErrorCodes.CashManagement.DeductionAccountMissing).WithData("code", code);

                var amountLcy = CurrencyExchangeRateManager.ToLcy(amount, currencyFactor);
                await _genJnlPostLine.PostGLDirectAsync(
                    payableAccountNo,
                    header.PostingDate,
                    GLEntryDocumentType.Payment,
                    header.No,
                    $"{deduction.Description ?? deduction.Code} - {header.Payee}".Truncate(ErpDomainConsts.MaxDescriptionLength),
                    -amountLcy,
                    line.AccountNo,
                    context: context,
                    documentDate: header.DocumentDate
                );
                bankAmountLcy -= amountLcy;
            }
        }

        var netAmount = lines.Sum(l => l.NetAmount);
        if (netAmount != 0m || bankAmountLcy != 0m)
        {
            lineNo += 10000;
            var bankLine = NewJournalLine(
                header,
                lineNo,
                GenJournalAccountType.BankAccount,
                bankAccount.No,
                header.PaymentNarration ?? $"Payment to {header.Payee}",
                -netAmount,
                appliesToDocNo: null
            );
            bankLine.SetCurrency(currencyCode, currencyFactor);
            // The bank takes whatever the other entries leave, so the voucher balances in LCY to the cent.
            bankLine.SetAmountLcy(-bankAmountLcy);
            await _genJnlPostLine.PostLineAsync(bankLine, context);
        }

        await _registerManager.CloseAsync(register);

        header.MarkPosted(Clock.Now, _currentUser.UserName);
        await _headers.UpdateAsync(header, autoSave: true);
    }

    private GenJournalLine NewJournalLine(
        PaymentVoucherHeader header,
        int lineNo,
        GenJournalAccountType accountType,
        string accountNo,
        string description,
        decimal amount,
        string appliesToDocNo
    )
    {
        var line = new GenJournalLine(
            GuidGenerator.Create(),
            Guid.Empty,
            lineNo,
            header.PostingDate,
            GLEntryDocumentType.Payment,
            header.No,
            accountType,
            accountNo,
            description.Truncate(ErpDomainConsts.MaxDescriptionLength),
            amount
        );

        line.Update(
            header.PostingDate,
            GLEntryDocumentType.Payment,
            header.No,
            accountType,
            accountNo,
            description.Truncate(ErpDomainConsts.MaxDescriptionLength),
            amount,
            documentDate: header.DocumentDate,
            externalDocumentNo: header.ChequeNo,
            appliesToDocNo: appliesToDocNo
        );

        return line;
    }

    private static void Require(PaymentVoucherHeader header, string value, string field)
    {
        if (value.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.VoucherFieldMissing).WithData("field", field).WithData("documentNo", header.No);
        }
    }
}
