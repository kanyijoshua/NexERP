using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// A pensioner: someone a scheme pays a monthly pension to, usually a member who has retired or
/// the dependant of one who has died.
/// </summary>
public class Pensioner : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string SchemeCode { get; private set; }

    /// <summary>The member the pension arises from; blank for a pensioner taken on without a member record.</summary>
    public string MemberNo { get; private set; }

    public string Name { get; private set; }
    public string NationalId { get; private set; }
    public string TaxPinNo { get; private set; }
    public DateTime? DateOfBirth { get; private set; }

    public decimal MonthlyPension { get; private set; }

    /// <summary>The first and the last month the pension is paid for; no end date pays it for life.</summary>
    public DateTime StartDate { get; private set; }

    public DateTime? EndDate { get; private set; }

    public PensionerStatus Status { get; private set; }

    public string PhoneNo { get; private set; }
    public string Email { get; private set; }
    public string BankName { get; private set; }
    public string BankBranch { get; private set; }
    public string BankAccountNo { get; private set; }

    public DateTime? LastPaidPeriod { get; internal set; }

    protected Pensioner() { }

    public Pensioner(Guid id, string no, string schemeCode, string name, DateTime startDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        Set(schemeCode, null, name, null, null, null);
        SetPension(0m, startDate, null, PensionerStatus.Active);
    }

    public void Set(string schemeCode, string memberNo, string name, string nationalId, string taxPinNo, DateTime? dateOfBirth)
    {
        SchemeCode = PensionSponsor.SchemeOf(schemeCode);
        MemberNo = CodeTableEntity.NormalizeCode(Check.Length(memberNo, nameof(memberNo), ErpDomainConsts.MaxNoLength));
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength).Trim();
        NationalId = Check.Length(nationalId?.Trim(), nameof(nationalId), ErpDomainConsts.MaxCodeLength * 2);
        TaxPinNo = Check.Length(taxPinNo?.Trim(), nameof(taxPinNo), ErpDomainConsts.MaxVatRegistrationNoLength);
        DateOfBirth = dateOfBirth?.Date;
    }

    public void SetPension(decimal monthlyPension, DateTime startDate, DateTime? endDate, PensionerStatus status)
    {
        if (monthlyPension < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Monthly Pension");
        }

        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
        {
            throw new BusinessException(ErpErrorCodes.Reports.PeriodReversed);
        }

        MonthlyPension = Math.Round(monthlyPension, 2, MidpointRounding.AwayFromZero);
        StartDate = startDate.Date;
        EndDate = endDate?.Date;
        Status = status;
    }

    public void SetContact(string phoneNo, string email, string bankName, string bankBranch, string bankAccountNo)
    {
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
        BankName = Check.Length(bankName, nameof(bankName), ErpDomainConsts.MaxNameLength);
        BankBranch = Check.Length(bankBranch, nameof(bankBranch), ErpDomainConsts.MaxNameLength);
        BankAccountNo = Check.Length(bankAccountNo, nameof(bankAccountNo), ErpDomainConsts.MaxBankAccountNoLength);
    }

    /// <summary>Whether the pension is due for the month starting on <paramref name="period"/>.</summary>
    public bool IsPayableFor(DateTime period)
    {
        var monthEnd = period.AddMonths(1).AddDays(-1);
        return Status == PensionerStatus.Active && MonthlyPension > 0m && StartDate <= monthEnd && (!EndDate.HasValue || EndDate.Value >= period);
    }
}

/// <summary>
/// A pension payroll: the pensions of one scheme for one month. It is prepared (Open), released
/// and posted, and the net is then paid through a payment voucher.
/// </summary>
public class PensionPayrollHeader : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string SchemeCode { get; private set; }

    /// <summary>The first day of the month the pensions are for.</summary>
    public DateTime PayPeriod { get; private set; }

    public DateTime PostingDate { get; private set; }
    public string Description { get; private set; }

    /// <summary>The tax withheld from each pension, as a percentage of what exceeds the tax free amount.</summary>
    public decimal TaxRatePct { get; private set; }

    /// <summary>The part of each monthly pension that is not taxed.</summary>
    public decimal TaxFreeAmount { get; private set; }

    public PensionDocumentStatus Status { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public string PostedBy { get; private set; }

    public decimal TotalGross { get; internal set; }
    public decimal TotalTax { get; internal set; }
    public decimal TotalNet { get; internal set; }
    public int NoOfPensioners { get; internal set; }

    /// <summary>The payment voucher raised to pay the net out of benefits payable.</summary>
    public string PaymentVoucherNo { get; internal set; }

    protected PensionPayrollHeader() { }

    public PensionPayrollHeader(Guid id, string no, string schemeCode, DateTime payPeriod, DateTime postingDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(schemeCode, payPeriod, postingDate, null, 0m, 0m);
    }

    public bool IsOpen => Status == PensionDocumentStatus.Open;

    public void Set(string schemeCode, DateTime payPeriod, DateTime postingDate, string description, decimal taxRatePct, decimal taxFreeAmount)
    {
        EnsureOpen();
        if (taxRatePct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", taxRatePct);
        }

        if (taxFreeAmount < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Tax Free Amount");
        }

        SchemeCode = PensionSponsor.SchemeOf(schemeCode);
        PayPeriod = new DateTime(payPeriod.Year, payPeriod.Month, 1);
        PostingDate = postingDate.Date;
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        TaxRatePct = taxRatePct;
        TaxFreeAmount = taxFreeAmount;
    }

    /// <summary>The tax on one pension of the payroll.</summary>
    public decimal TaxOn(decimal grossPension) =>
        Math.Round(Math.Max(0m, grossPension - TaxFreeAmount) * TaxRatePct / 100m, 2, MidpointRounding.AwayFromZero);

    public void Release()
    {
        EnsureOpen();
        Status = PensionDocumentStatus.Released;
    }

    public void Reopen()
    {
        if (Status != PensionDocumentStatus.Released)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotReleased).WithData("documentNo", No);
        }

        Status = PensionDocumentStatus.Open;
    }

    internal void MarkPosted(DateTime when, string by)
    {
        Status = PensionDocumentStatus.Posted;
        PostedDate = when;
        PostedBy = by;
    }

    public void EnsureOpen()
    {
        if (Status != PensionDocumentStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>One pensioner's pension on a payroll.</summary>
public class PensionPayrollLine : CompanyEntity
{
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }
    public string PensionerNo { get; private set; }
    public string PensionerName { get; private set; }
    public decimal GrossPension { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal NetPension { get; private set; }

    protected PensionPayrollLine() { }

    public PensionPayrollLine(Guid id, string documentNo, int lineNo, Pensioner pensioner)
        : base(id)
    {
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        LineNo = lineNo;
        SetPensioner(pensioner);
    }

    public void SetPensioner(Pensioner pensioner)
    {
        PensionerNo = pensioner.No;
        PensionerName = pensioner.Name;
    }

    public void SetAmounts(decimal grossPension, decimal taxAmount)
    {
        if (grossPension < 0 || taxAmount < 0 || taxAmount > grossPension)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Pension");
        }

        GrossPension = Math.Round(grossPension, 2, MidpointRounding.AwayFromZero);
        TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero);
        NetPension = GrossPension - TaxAmount;
    }
}

/// <summary>
/// Prepares and posts pension payrolls.
/// <para>
/// Posting debits pensions paid with the gross, credits the tax account with what was withheld
/// and benefits payable with the net, all under the scheme's dimension. The net stays in benefits
/// payable until the payroll's payment voucher is paid.
/// </para>
/// </summary>
public class PensionPayrollEngine : DomainService
{
    public const string SourceCode = "PENPAY";

    private readonly IRepository<PensionPayrollHeader, Guid> _headers;
    private readonly IRepository<PensionPayrollLine, Guid> _lines;
    private readonly IRepository<Pensioner, Guid> _pensioners;
    private readonly PensionSetupManager _setupManager;
    private readonly PensionSchemeManager _schemeManager;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly ICurrentUser _currentUser;

    public PensionPayrollEngine(
        IRepository<PensionPayrollHeader, Guid> headers,
        IRepository<PensionPayrollLine, Guid> lines,
        IRepository<Pensioner, Guid> pensioners,
        PensionSetupManager setupManager,
        PensionSchemeManager schemeManager,
        GenJnlPostLine genJnlPostLine,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager,
        ICurrentUser currentUser
    )
    {
        _headers = headers;
        _lines = lines;
        _pensioners = pensioners;
        _setupManager = setupManager;
        _schemeManager = schemeManager;
        _genJnlPostLine = genJnlPostLine;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
        _currentUser = currentUser;
    }

    /// <summary>Stores the totals of the lines on the header.</summary>
    public async Task UpdateTotalsAsync(PensionPayrollHeader header)
    {
        var lines = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        header.TotalGross = lines.Sum(l => l.GrossPension);
        header.TotalTax = lines.Sum(l => l.TaxAmount);
        header.TotalNet = lines.Sum(l => l.NetPension);
        header.NoOfPensioners = lines.Count;
        await _headers.UpdateAsync(header, autoSave: true);
    }

    /// <summary>
    /// Fills the payroll with every pensioner of the scheme whose pension is due for the month,
    /// at the monthly pension and the payroll's tax. Pensioners already on the payroll are left as
    /// they are. Returns the number of lines added.
    /// </summary>
    public async Task<int> SuggestLinesAsync(PensionPayrollHeader header)
    {
        header.EnsureOpen();

        var existing = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        var onPayroll = existing.Select(l => l.PensionerNo).ToHashSet(StringComparer.Ordinal);
        var lineNo = existing.Count == 0 ? 0 : existing.Max(l => l.LineNo);

        var due = (await _pensioners.GetListAsync(p => p.SchemeCode == header.SchemeCode))
            .Where(p => p.IsPayableFor(header.PayPeriod) && !onPayroll.Contains(p.No))
            .OrderBy(p => p.No, StringComparer.Ordinal)
            .ToList();

        foreach (var pensioner in due)
        {
            lineNo += 10000;
            var line = new PensionPayrollLine(GuidGenerator.Create(), header.No, lineNo, pensioner);
            line.SetAmounts(pensioner.MonthlyPension, header.TaxOn(pensioner.MonthlyPension));
            await _lines.InsertAsync(line, autoSave: true);
        }

        await UpdateTotalsAsync(header);
        return due.Count;
    }

    /// <summary>Releases the payroll after checking that it can be posted as it stands.</summary>
    public async Task ReleaseAsync(PensionPayrollHeader header)
    {
        await CheckAsync(header);
        header.Release();
        await _headers.UpdateAsync(header, autoSave: true);
    }

    public async Task PostAsync(PensionPayrollHeader header)
    {
        if (header.Status != PensionDocumentStatus.Released)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotReleased).WithData("documentNo", header.No);
        }

        await _glSetupManager.CheckPostingDateAsync(header.PostingDate);
        var (lines, pensioners) = await CheckAsync(header);

        var setup = await _setupManager.GetAsync();
        var pensionsPaid = setup.Require(setup.PensionsPaidAccountNo, "Pensions Paid Account No.");
        var benefitsPayable = setup.Require(setup.BenefitsPayableAccountNo, "Benefits Payable Account No.");
        var gross = lines.Sum(l => l.GrossPension);
        var tax = lines.Sum(l => l.TaxAmount);
        var taxAccount = tax == 0m ? null : setup.Require(setup.TaxAccountNo, "Tax Account No.");

        var dimensionSetId = await _schemeManager.GetDimensionSetIdAsync(header.SchemeCode);
        var register = await _registerManager.OpenAsync(header.PostingDate, SourceCode, header.No);
        var context = new GLPostingContext(register, SourceCode);
        var description = header.Description.IsNullOrWhiteSpace() ? $"Pensions {header.PayPeriod:MMM yyyy} {header.SchemeCode}" : header.Description;

        await _genJnlPostLine.PostGLDirectAsync(pensionsPaid, header.PostingDate, GLEntryDocumentType.None, header.No, description, gross, header.SchemeCode, dimensionSetId, context);
        await _genJnlPostLine.PostGLDirectAsync(benefitsPayable, header.PostingDate, GLEntryDocumentType.None, header.No, description, -(gross - tax), header.SchemeCode, dimensionSetId, context);

        if (tax != 0m)
        {
            await _genJnlPostLine.PostGLDirectAsync(taxAccount, header.PostingDate, GLEntryDocumentType.None, header.No, "Tax on " + description, -tax, header.SchemeCode, dimensionSetId, context);
        }

        await _registerManager.CloseAsync(register);

        foreach (var pensioner in pensioners.Values)
        {
            pensioner.LastPaidPeriod = header.PayPeriod;
            await _pensioners.UpdateAsync(pensioner);
        }

        header.TotalGross = gross;
        header.TotalTax = tax;
        header.TotalNet = gross - tax;
        header.NoOfPensioners = lines.Count;
        header.MarkPosted(Clock.Now, _currentUser.UserName);
        await _headers.UpdateAsync(header, autoSave: true);
    }

    /// <summary>What must hold before a payroll is released or posted. Returns its lines and their pensioners.</summary>
    private async Task<(List<PensionPayrollLine> Lines, Dictionary<string, Pensioner> Pensioners)> CheckAsync(PensionPayrollHeader header)
    {
        await _schemeManager.GetAsync(header.SchemeCode);

        var lines = (await _lines.GetListAsync(l => l.DocumentNo == header.No)).OrderBy(l => l.LineNo).ToList();
        if (lines.Count == 0 || lines.Sum(l => l.GrossPension) == 0m)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NothingToPost).WithData("documentNo", header.No);
        }

        var duplicate = lines.GroupBy(l => l.PensionerNo).FirstOrDefault(g => g.Count() > 1);
        if (duplicate != null)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.PensionerDuplicated).WithData("pensionerNo", duplicate.Key).WithData("documentNo", header.No);
        }

        var numbers = lines.Select(l => l.PensionerNo).ToList();
        var pensioners = (await _pensioners.GetListAsync(p => numbers.Contains(p.No))).ToDictionary(p => p.No, StringComparer.Ordinal);

        foreach (var line in lines)
        {
            if (!pensioners.TryGetValue(line.PensionerNo, out var pensioner) || pensioner.SchemeCode != header.SchemeCode)
            {
                throw new BusinessException(ErpErrorCodes.Pensions.PensionerNotInScheme).WithData("pensionerNo", line.PensionerNo).WithData("scheme", header.SchemeCode);
            }
        }

        // A month is paid once: a second payroll for it would pay every pensioner twice.
        var (id, scheme, period) = (header.Id, header.SchemeCode, header.PayPeriod);
        var other = await _headers.FirstOrDefaultAsync(h => h.Id != id && h.SchemeCode == scheme && h.PayPeriod == period && h.Status == PensionDocumentStatus.Posted);
        if (other != null)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.PayrollPeriodAlreadyPosted)
                .WithData("scheme", header.SchemeCode)
                .WithData("period", header.PayPeriod.ToString("MMM yyyy"))
                .WithData("documentNo", other.No);
        }

        return (lines, pensioners);
    }
}

/// <summary>
/// Raises the payment vouchers that pay what the pension processes leave in benefits payable: the
/// net benefit of a posted exit and the net of a posted pension payroll.
/// </summary>
public class PensionPaymentManager : DomainService
{
    public const string ExitSourceType = "MemberExit";
    public const string PayrollSourceType = "PensionPayroll";

    private readonly IRepository<MemberExit, Guid> _exits;
    private readonly IRepository<PensionPayrollHeader, Guid> _payrolls;
    private readonly PensionSetupManager _setupManager;
    private readonly PaymentVoucherFactory _voucherFactory;

    public PensionPaymentManager(
        IRepository<MemberExit, Guid> exits,
        IRepository<PensionPayrollHeader, Guid> payrolls,
        PensionSetupManager setupManager,
        PaymentVoucherFactory voucherFactory
    )
    {
        _exits = exits;
        _payrolls = payrolls;
        _setupManager = setupManager;
        _voucherFactory = voucherFactory;
    }

    public async Task<PaymentVoucherHeader> RaiseForExitAsync(MemberExit exit, DateTime date)
    {
        if (exit.Status != MemberExitStatus.Posted)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotPosted).WithData("documentNo", exit.No);
        }

        EnsureNotRaised(exit.No, exit.PaymentVoucherNo);

        var voucher = await CreateAsync(ExitSourceType, exit.No, date, exit.MemberName, $"Exit benefit {exit.No} {exit.MemberNo}", exit.NetPayable);

        exit.PaymentVoucherNo = voucher.No;
        await _exits.UpdateAsync(exit, autoSave: true);
        return voucher;
    }

    public async Task<PaymentVoucherHeader> RaiseForPayrollAsync(PensionPayrollHeader payroll, DateTime date)
    {
        if (payroll.Status != PensionDocumentStatus.Posted)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotPosted).WithData("documentNo", payroll.No);
        }

        EnsureNotRaised(payroll.No, payroll.PaymentVoucherNo);

        var narration = $"Pensions {payroll.PayPeriod:MMM yyyy} {payroll.SchemeCode}";
        var voucher = await CreateAsync(PayrollSourceType, payroll.No, date, $"Pensioners {payroll.SchemeCode}", narration, payroll.TotalNet);

        payroll.PaymentVoucherNo = voucher.No;
        await _payrolls.UpdateAsync(payroll, autoSave: true);
        return voucher;
    }

    private async Task<PaymentVoucherHeader> CreateAsync(string sourceType, string sourceNo, DateTime date, string payee, string narration, decimal amount)
    {
        if (amount <= 0m)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NothingToPost).WithData("documentNo", sourceNo);
        }

        var setup = await _setupManager.GetAsync();
        var benefitsPayable = setup.Require(setup.BenefitsPayableAccountNo, "Benefits Payable Account No.");

        return await _voucherFactory.CreateAsync(
            sourceType,
            sourceNo,
            date,
            payee,
            narration,
            [new PaymentVoucherRequestLine(GenJournalAccountType.GLAccount, benefitsPayable, narration, amount)]
        );
    }

    private static void EnsureNotRaised(string documentNo, string voucherNo)
    {
        if (!voucherNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Pensions.VoucherAlreadyRaised).WithData("documentNo", documentNo).WithData("voucherNo", voucherNo);
        }
    }
}
