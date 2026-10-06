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

    /// <summary>A pensioner whose pension is not taxed, e.g. on a disability exemption.</summary>
    public bool TaxExempt { get; private set; }

    /// <summary>Pension owed for months already past, paid with the next payroll: after a reinstatement or a backdated increment.</summary>
    public decimal ArrearsAmount { get; private set; }

    /// <summary>How many months the arrears are for, so that they are taxed month by month and not as one large payment.</summary>
    public int ArrearsMonths { get; private set; }

    /// <summary>Why the pension is suspended; blank while it is paid.</summary>
    public string SuspensionReason { get; private set; }

    /// <summary>The last proof that the pensioner is alive, and when the next one is due.</summary>
    public DateTime? LastLifeCertificateDate { get; private set; }

    public DateTime? LifeCertificateDueDate { get; private set; }

    /// <summary>How the pension is paid; the bank and branch when it is paid through a bank.</summary>
    public string PayModeCode { get; private set; }

    public string BankCode { get; private set; }
    public string BankBranchCode { get; private set; }

    /// <summary>The suspension reason the pension is stopped for; blank while it is paid.</summary>
    public string SuspensionReasonCode { get; private set; }

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

    public void SetTaxExempt(bool taxExempt) => TaxExempt = taxExempt;

    public void SetPayment(string payModeCode, string bankCode, string bankBranchCode)
    {
        PayModeCode = CodeTableEntity.NormalizeCode(Check.Length(payModeCode, nameof(payModeCode), ErpDomainConsts.MaxCodeLength));
        BankCode = CodeTableEntity.NormalizeCode(Check.Length(bankCode, nameof(bankCode), ErpDomainConsts.MaxCodeLength));
        BankBranchCode = BankCode == null ? null : CodeTableEntity.NormalizeCode(Check.Length(bankBranchCode, nameof(bankBranchCode), ErpDomainConsts.MaxCodeLength));
    }

    /// <summary>Stops the pension until the pensioner is reinstated.</summary>
    internal void Suspend(string reasonCode, string reason)
    {
        if (Status != PensionerStatus.Active)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.PensionerNotActive).WithData("pensionerNo", No).WithData("status", Status);
        }

        Status = PensionerStatus.Suspended;
        SuspensionReasonCode = CodeTableEntity.NormalizeCode(Check.Length(reasonCode, nameof(reasonCode), ErpDomainConsts.MaxCodeLength));
        SuspensionReason = Check.Length(reason, nameof(reason), ErpDomainConsts.MaxDescriptionLength);
    }

    internal void Reinstate()
    {
        if (Status != PensionerStatus.Suspended)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.PensionerNotSuspended).WithData("pensionerNo", No);
        }

        Status = PensionerStatus.Active;
        SuspensionReasonCode = null;
        SuspensionReason = null;
    }

    internal void ChangePension(decimal monthlyPension)
    {
        MonthlyPension = Math.Round(Math.Max(0m, monthlyPension), 2, MidpointRounding.AwayFromZero);
    }

    internal void AddArrears(decimal amount, int months)
    {
        ArrearsAmount += Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        ArrearsMonths += months;
    }

    /// <summary>Takes what a payroll paid off the arrears.</summary>
    internal void SettleArrears(decimal paid, int months)
    {
        ArrearsAmount = Math.Max(0m, ArrearsAmount - paid);
        ArrearsMonths = ArrearsAmount == 0m ? 0 : Math.Max(0, ArrearsMonths - months);
    }

    internal void RecordLifeCertificate(DateTime date, int frequencyMonths)
    {
        LastLifeCertificateDate = date.Date;
        LifeCertificateDueDate = frequencyMonths > 0 ? date.Date.AddMonths(frequencyMonths) : null;
    }

    /// <summary>
    /// The months the pension was due but not paid: from the month after the last one paid (or the
    /// first month of the pension) up to, not including, the month of <paramref name="upTo"/>.
    /// </summary>
    public int UnpaidMonthsBefore(DateTime upTo)
    {
        var first = LastPaidPeriod?.AddMonths(1) ?? new DateTime(StartDate.Year, StartDate.Month, 1);
        var end = new DateTime(upTo.Year, upTo.Month, 1);
        if (EndDate.HasValue && EndDate.Value < end)
        {
            end = new DateTime(EndDate.Value.Year, EndDate.Value.Month, 1).AddMonths(1);
        }

        return first >= end ? 0 : (end.Year - first.Year) * 12 + end.Month - first.Month;
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

    /// <summary>
    /// Graduated tax: the tax table whose bands the taxable pension runs through. Blank taxes at the
    /// flat rate instead.
    /// </summary>
    public string TaxTableCode { get; private set; }

    /// <summary>Taken off the graduated tax of each pensioner every month; it never makes the tax negative.</summary>
    public decimal PersonalRelief { get; private set; }

    public PensionDocumentStatus Status { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public string PostedBy { get; private set; }

    public decimal TotalGross { get; internal set; }
    public decimal TotalTax { get; internal set; }

    /// <summary>What the pensioners' deductions took off the pensions; the net is what is left after tax and deductions.</summary>
    public decimal TotalDeductions { get; internal set; }

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

    public void SetTaxTable(string taxTableCode, decimal personalRelief)
    {
        EnsureOpen();
        if (personalRelief < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Personal Relief");
        }

        TaxTableCode = CodeTableEntity.NormalizeCode(Check.Length(taxTableCode, nameof(taxTableCode), ErpDomainConsts.MaxCodeLength));
        PersonalRelief = personalRelief;
    }

    /// <summary>The tax on one month's pension at the flat rate.</summary>
    public decimal TaxOn(decimal grossPension) =>
        Math.Round(Math.Max(0m, grossPension - TaxFreeAmount) * TaxRatePct / 100m, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// The tax on one month's pension: through the bands of the tax table, less the personal relief,
    /// when the payroll has a tax table; at the flat rate otherwise.
    /// </summary>
    public decimal TaxOn(decimal grossPension, IReadOnlyCollection<LumpsumTaxBand> bands)
    {
        if (TaxTableCode.IsNullOrWhiteSpace())
        {
            return TaxOn(grossPension);
        }

        var tax = LumpsumTaxBand.TaxOn(Math.Max(0m, grossPension - TaxFreeAmount), bands) - PersonalRelief;
        return Math.Max(0m, Math.Round(tax, 2, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// The tax on a pensioner's line: the month's pension, plus the arrears taxed as if each of their
    /// months had been paid on time, so that paying late does not push the pensioner into a higher band.
    /// </summary>
    public decimal TaxOnLine(Pensioner pensioner, decimal monthlyPension, decimal arrears, int arrearsMonths, IReadOnlyCollection<LumpsumTaxBand> bands)
    {
        if (pensioner.TaxExempt)
        {
            return 0m;
        }

        var tax = TaxOn(monthlyPension, bands);
        if (arrears > 0m)
        {
            var months = Math.Max(1, arrearsMonths);
            tax += months * TaxOn(Math.Round(arrears / months, 2, MidpointRounding.AwayFromZero), bands);
        }

        return Math.Min(tax, monthlyPension + arrears);
    }

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

/// <summary>
/// One pensioner's pension on a payroll: the month's pension, any arrears and the pensioner's
/// earnings make the gross; tax and the pensioner's deductions are taken off it to leave the net.
/// </summary>
public class PensionPayrollLine : CompanyEntity
{
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }
    public string PensionerNo { get; private set; }
    public string PensionerName { get; private set; }

    /// <summary>The pension for the month itself, without arrears or earnings.</summary>
    public decimal MonthlyPension { get; private set; }

    /// <summary>The pensioner's earnings for the month, part of the gross.</summary>
    public decimal OtherEarnings { get; private set; }

    public decimal GrossPension { get; private set; }
    public decimal TaxAmount { get; private set; }

    /// <summary>The pensioner's deductions for the month, taken off after tax.</summary>
    public decimal Deductions { get; private set; }

    public decimal NetPension { get; private set; }

    /// <summary>How the net is paid, from the pensioner when the line was worked out.</summary>
    public string PayModeCode { get; private set; }

    /// <summary>The part of the gross that pays arrears of past months, and how many months they are for.</summary>
    public decimal ArrearsAmount { get; private set; }

    public int ArrearsMonths { get; private set; }

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
        PayModeCode = pensioner.PayModeCode;
    }

    /// <summary>Arrears paid on the line. They are part of the gross pension, so set them before the amounts.</summary>
    public void SetArrears(decimal arrearsAmount, int arrearsMonths)
    {
        if (arrearsAmount < 0 || arrearsMonths < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Arrears");
        }

        ArrearsAmount = Math.Round(arrearsAmount, 2, MidpointRounding.AwayFromZero);
        ArrearsMonths = ArrearsAmount == 0m ? 0 : Math.Max(1, arrearsMonths);
    }

    /// <summary>Sets the line's amounts. Set the arrears first: they are part of the gross.</summary>
    public void SetAmounts(decimal monthlyPension, decimal otherEarnings, decimal deductions, decimal taxAmount)
    {
        if (monthlyPension < 0 || otherEarnings < 0 || deductions < 0 || taxAmount < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Pension");
        }

        MonthlyPension = Math.Round(monthlyPension, 2, MidpointRounding.AwayFromZero);
        OtherEarnings = Math.Round(otherEarnings, 2, MidpointRounding.AwayFromZero);
        GrossPension = MonthlyPension + ArrearsAmount + OtherEarnings;
        TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero);
        if (TaxAmount > GrossPension)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Pension");
        }

        Deductions = Math.Round(deductions, 2, MidpointRounding.AwayFromZero);
        if (Deductions > GrossPension - TaxAmount)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DeductionsExceedPension)
                .WithData("pensionerNo", PensionerNo)
                .WithData("documentNo", DocumentNo)
                .WithData("deductions", Deductions)
                .WithData("available", GrossPension - TaxAmount);
        }

        NetPension = GrossPension - TaxAmount - Deductions;
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
    private readonly IRepository<LumpsumTaxBand, Guid> _taxBands;
    private readonly IRepository<PensionerChangeEntry, Guid> _changes;
    private readonly IRepository<PensionPayrollLineItem, Guid> _lineItems;
    private readonly PensionerPayItemCalculator _payItems;
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
        IRepository<LumpsumTaxBand, Guid> taxBands,
        IRepository<PensionerChangeEntry, Guid> changes,
        IRepository<PensionPayrollLineItem, Guid> lineItems,
        PensionerPayItemCalculator payItems,
        PensionSetupManager setupManager,
        PensionSchemeManager schemeManager,
        GenJnlPostLine genJnlPostLine,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager,
        ICurrentUser currentUser
    )
    {
        _lineItems = lineItems;
        _payItems = payItems;
        _headers = headers;
        _lines = lines;
        _pensioners = pensioners;
        _taxBands = taxBands;
        _changes = changes;
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
        header.TotalDeductions = lines.Sum(l => l.Deductions);
        header.TotalNet = lines.Sum(l => l.NetPension);
        header.NoOfPensioners = lines.Count;
        await _headers.UpdateAsync(header, autoSave: true);
    }

    /// <summary>The bands of the payroll's tax table; none when it taxes at a flat rate.</summary>
    public async Task<List<LumpsumTaxBand>> GetTaxBandsAsync(PensionPayrollHeader header)
    {
        var table = header.TaxTableCode;
        return table.IsNullOrWhiteSpace() ? [] : await _taxBands.GetListAsync(b => b.TaxTableCode == table);
    }

    /// <summary>
    /// Works a line out: the month's pension (or <paramref name="monthlyPension"/>), the pensioner's
    /// arrears, and the earnings and deductions the pensioner has for the month, which are stored
    /// with the line. The payroll's tax is on the pension and arrears, plus taxable earnings, less
    /// tax-deductible deductions, unless a tax amount is given.
    /// </summary>
    public async Task CalculateLineAsync(
        PensionPayrollLine line,
        PensionPayrollHeader header,
        Pensioner pensioner,
        IReadOnlyCollection<LumpsumTaxBand> bands,
        decimal? monthlyPension = null,
        decimal? taxAmount = null
    )
    {
        var monthly = monthlyPension ?? pensioner.MonthlyPension;
        line.SetArrears(pensioner.ArrearsAmount, pensioner.ArrearsMonths);

        await _lineItems.DeleteAsync(i => i.DocumentNo == line.DocumentNo && i.LineNo == line.LineNo);
        var items = await _payItems.GetForAsync(pensioner.No, header.PayPeriod, monthly);
        foreach (var item in items)
        {
            await _lineItems.InsertAsync(new PensionPayrollLineItem(GuidGenerator.Create(), line, item.Item, item.Amount));
        }

        var earnings = items.Where(i => i.Item.ItemType == PensionerPayItemType.Earning).ToList();
        var deductions = items.Where(i => i.Item.ItemType == PensionerPayItemType.Deduction).ToList();
        var taxableMonthly = Math.Max(
            0m,
            monthly + earnings.Where(i => i.Item.Taxable).Sum(i => i.Amount) - deductions.Where(i => i.Item.Taxable).Sum(i => i.Amount)
        );

        line.SetAmounts(
            monthly,
            earnings.Sum(i => i.Amount),
            deductions.Sum(i => i.Amount),
            taxAmount ?? header.TaxOnLine(pensioner, taxableMonthly, line.ArrearsAmount, line.ArrearsMonths, bands)
        );
    }

    /// <summary>Removes a line together with the earnings and deductions worked out for it.</summary>
    public async Task DeleteLineAsync(PensionPayrollLine line)
    {
        await _lineItems.DeleteAsync(i => i.DocumentNo == line.DocumentNo && i.LineNo == line.LineNo);
        await _lines.DeleteAsync(line, autoSave: true);
    }

    /// <summary>Removes an open payroll with its lines and their earnings and deductions.</summary>
    public async Task DeletePayrollAsync(PensionPayrollHeader header)
    {
        header.EnsureOpen();
        await _lineItems.DeleteAsync(i => i.DocumentNo == header.No);
        await _lines.DeleteAsync(l => l.DocumentNo == header.No);
        await _headers.DeleteAsync(header, autoSave: true);
    }

    /// <summary>
    /// Fills the payroll with every pensioner of the scheme whose pension is due for the month,
    /// at the monthly pension plus any arrears owed, and the payroll's tax. Pensioners already on
    /// the payroll are left as they are. Returns the number of lines added.
    /// </summary>
    public async Task<int> SuggestLinesAsync(PensionPayrollHeader header)
    {
        header.EnsureOpen();

        var existing = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        var onPayroll = existing.Select(l => l.PensionerNo).ToHashSet(StringComparer.Ordinal);
        var lineNo = existing.Count == 0 ? 0 : existing.Max(l => l.LineNo);
        var bands = await GetTaxBandsAsync(header);

        var due = (await _pensioners.GetListAsync(p => p.SchemeCode == header.SchemeCode))
            .Where(p => p.IsPayableFor(header.PayPeriod) && !onPayroll.Contains(p.No))
            .OrderBy(p => p.No, StringComparer.Ordinal)
            .ToList();

        foreach (var pensioner in due)
        {
            lineNo += 10000;
            var line = new PensionPayrollLine(GuidGenerator.Create(), header.No, lineNo, pensioner);
            await CalculateLineAsync(line, header, pensioner, bands);
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
        var deductions = lines.Sum(l => l.Deductions);
        var net = gross - tax - deductions;
        var taxAccount = tax == 0m ? null : setup.Require(setup.TaxAccountNo, "Tax Account No.");

        // Earnings are debited to their own accounts (the pensions paid account when they have none);
        // deductions are credited to theirs, which they must have.
        var items = await _lineItems.GetListAsync(i => i.DocumentNo == header.No);
        var earningAccounts = items
            .Where(i => i.IsEarning)
            .GroupBy(i => i.AccountNo.IsNullOrWhiteSpace() ? pensionsPaid : i.AccountNo)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Amount));
        var deductionAccounts = items
            .Where(i => !i.IsEarning)
            .GroupBy(i => i.AccountNo.IsNullOrWhiteSpace() ? throw MissingItemAccount(i) : i.AccountNo)
            .ToDictionary(g => g.Key, g => (Amount: g.Sum(i => i.Amount), Description: g.First().Description));
        var earningsTotal = earningAccounts.Values.Sum();

        var dimensionSetId = await _schemeManager.GetDimensionSetIdAsync(header.SchemeCode);
        var register = await _registerManager.OpenAsync(header.PostingDate, SourceCode, header.No);
        var context = new GLPostingContext(register, SourceCode);
        var description = header.Description.IsNullOrWhiteSpace() ? $"Pensions {header.PayPeriod:MMM yyyy} {header.SchemeCode}" : header.Description;

        // The pensions themselves and their arrears go to the pensions paid account.
        var pensionsAndArrears = gross - earningsTotal;
        earningAccounts[pensionsPaid] = earningAccounts.GetValueOrDefault(pensionsPaid) + pensionsAndArrears;
        foreach (var (account, amount) in earningAccounts.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            if (amount != 0m)
            {
                await _genJnlPostLine.PostGLDirectAsync(account, header.PostingDate, GLEntryDocumentType.None, header.No, description, amount, header.SchemeCode, dimensionSetId, context);
            }
        }

        foreach (var (account, deduction) in deductionAccounts.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            await _genJnlPostLine.PostGLDirectAsync(
                account,
                header.PostingDate,
                GLEntryDocumentType.None,
                header.No,
                $"{deduction.Description} {header.PayPeriod:MMM yyyy}",
                -deduction.Amount,
                header.SchemeCode,
                dimensionSetId,
                context
            );
        }

        await _genJnlPostLine.PostGLDirectAsync(benefitsPayable, header.PostingDate, GLEntryDocumentType.None, header.No, description, -net, header.SchemeCode, dimensionSetId, context);

        if (tax != 0m)
        {
            await _genJnlPostLine.PostGLDirectAsync(taxAccount, header.PostingDate, GLEntryDocumentType.None, header.No, "Tax on " + description, -tax, header.SchemeCode, dimensionSetId, context);
        }

        await _registerManager.CloseAsync(register);

        foreach (var line in lines)
        {
            var pensioner = pensioners[line.PensionerNo];
            pensioner.LastPaidPeriod = header.PayPeriod;

            if (line.ArrearsAmount > 0m)
            {
                pensioner.SettleArrears(line.ArrearsAmount, line.ArrearsMonths);
                await _changes.InsertAsync(
                    new PensionerChangeEntry(GuidGenerator.Create(), pensioner, PensionerChangeType.ArrearsPaid, header.PostingDate, header.No, _currentUser.UserName)
                    {
                        Amount = line.ArrearsAmount,
                        Description = $"Arrears for {line.ArrearsMonths} month(s) paid with {header.No}",
                    }
                );
            }

            await _pensioners.UpdateAsync(pensioner);
        }

        header.TotalGross = gross;
        header.TotalTax = tax;
        header.TotalDeductions = deductions;
        header.TotalNet = net;
        header.NoOfPensioners = lines.Count;
        header.MarkPosted(Clock.Now, _currentUser.UserName);
        await _headers.UpdateAsync(header, autoSave: true);
    }

    private static BusinessException MissingItemAccount(PensionPayrollLineItem item) =>
        new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing).WithData("field", "Account No. of " + item.PayItemCode).WithData("setup", "Pensioner Pay Item");

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
