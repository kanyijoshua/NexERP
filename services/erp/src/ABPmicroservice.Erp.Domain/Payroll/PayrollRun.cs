using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.HumanResources;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Payroll;

/// <summary>
/// A payroll run: the pay of every employee for one month. It is calculated into payslips
/// (as often as needed while open), posted to the ledger, and the net pay is then paid through a
/// payment voucher.
/// </summary>
public class PayrollRun : CompanyEntity, IHasNo
{
    public string No { get; private set; }

    /// <summary>The first day of the month the run pays.</summary>
    public DateTime PayPeriod { get; private set; }

    public DateTime PostingDate { get; private set; }
    public string Description { get; private set; }

    public PayrollRunStatus Status { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public string PostedBy { get; private set; }

    public int NoOfEmployees { get; internal set; }
    public decimal TotalGross { get; internal set; }
    public decimal TotalDeductions { get; internal set; }
    public decimal TotalNet { get; internal set; }
    public decimal TotalEmployerContributions { get; internal set; }

    public string PaymentVoucherNo { get; internal set; }

    protected PayrollRun() { }

    public PayrollRun(Guid id, string no, DateTime payPeriod, DateTime postingDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(payPeriod, postingDate, null);
    }

    public void Set(DateTime payPeriod, DateTime postingDate, string description)
    {
        EnsureNotPosted();
        PayPeriod = new DateTime(payPeriod.Year, payPeriod.Month, 1);
        PostingDate = postingDate.Date;
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);

        // The payslips were worked out for what the run said before.
        Status = PayrollRunStatus.Open;
    }

    internal void MarkCalculated() => Status = PayrollRunStatus.Calculated;

    internal void MarkPosted(DateTime when, string by)
    {
        Status = PayrollRunStatus.Posted;
        PostedDate = when;
        PostedBy = by;
    }

    public void EnsureNotPosted()
    {
        if (Status == PayrollRunStatus.Posted)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.RunStatusWrong).WithData("documentNo", No ?? string.Empty).WithData("status", Status);
        }
    }
}

/// <summary>One employee's pay on a payroll run: the totals of the employee's payslip lines.</summary>
public class Payslip : CompanyEntity
{
    public string PayrollRunNo { get; private set; }
    public DateTime PayPeriod { get; private set; }
    public string EmployeeNo { get; private set; }
    public string EmployeeName { get; private set; }
    public string JobTitle { get; private set; }
    public string BankAccountNo { get; private set; }

    public decimal BasicPay { get; private set; }
    public decimal GrossPay { get; private set; }
    public decimal TaxablePay { get; private set; }
    public decimal IncomeTax { get; private set; }
    public decimal TotalDeductions { get; private set; }
    public decimal NetPay { get; private set; }
    public decimal EmployerContributions { get; private set; }

    protected Payslip() { }

    public Payslip(Guid id, PayrollRun run, Employee employee)
        : base(id)
    {
        PayrollRunNo = run.No;
        PayPeriod = run.PayPeriod;
        EmployeeNo = employee.No;
        EmployeeName = employee.FullName;
        JobTitle = employee.JobTitle;
        BankAccountNo = employee.BankAccountNo;
    }

    internal void SetTotals(decimal basicPay, decimal grossPay, decimal taxablePay, decimal incomeTax, decimal totalDeductions, decimal employerContributions)
    {
        BasicPay = basicPay;
        GrossPay = grossPay;
        TaxablePay = taxablePay;
        IncomeTax = incomeTax;
        TotalDeductions = totalDeductions;
        NetPay = grossPay - totalDeductions;
        EmployerContributions = employerContributions;
    }
}

/// <summary>One earning, deduction or employer contribution on a payslip.</summary>
public class PayslipLine : CompanyEntity
{
    public string PayrollRunNo { get; private set; }
    public string EmployeeNo { get; private set; }
    public PayslipLineType LineType { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }

    protected PayslipLine() { }

    public PayslipLine(Guid id, string payrollRunNo, string employeeNo, PayslipLineType lineType, string code, string description, decimal amount)
        : base(id)
    {
        PayrollRunNo = payrollRunNo;
        EmployeeNo = employeeNo;
        LineType = lineType;
        Code = code;
        Description = description;
        Amount = amount;
    }
}

/// <summary>
/// Calculates and posts payroll runs.
/// <para>
/// Each earning is debited to its expense account; each deduction, and the employer's share on top
/// of it, is credited to the deduction's liability account (the employer's share debited to its
/// expense account); and each employee's net pay is credited to the employee's account, where the
/// payroll's payment voucher later settles it.
/// </para>
/// </summary>
public class PayrollEngine : DomainService
{
    public const string SourceCode = "PAYROLL";
    public const string VoucherSourceType = "PayrollRun";

    private readonly IRepository<PayrollRun, Guid> _runs;
    private readonly IRepository<Payslip, Guid> _payslips;
    private readonly IRepository<PayslipLine, Guid> _lines;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<EmployeePayItem, Guid> _payItems;
    private readonly IRepository<PayrollEarning, Guid> _earnings;
    private readonly IRepository<PayrollDeduction, Guid> _deductions;
    private readonly IRepository<PayrollTaxBand, Guid> _taxBands;
    private readonly PayrollSetupManager _setupManager;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly PaymentVoucherFactory _voucherFactory;
    private readonly ICurrentUser _currentUser;

    public PayrollEngine(
        IRepository<PayrollRun, Guid> runs,
        IRepository<Payslip, Guid> payslips,
        IRepository<PayslipLine, Guid> lines,
        IRepository<Employee, Guid> employees,
        IRepository<EmployeePayItem, Guid> payItems,
        IRepository<PayrollEarning, Guid> earnings,
        IRepository<PayrollDeduction, Guid> deductions,
        IRepository<PayrollTaxBand, Guid> taxBands,
        PayrollSetupManager setupManager,
        GenJnlPostLine genJnlPostLine,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager,
        PaymentVoucherFactory voucherFactory,
        ICurrentUser currentUser
    )
    {
        _runs = runs;
        _payslips = payslips;
        _lines = lines;
        _employees = employees;
        _payItems = payItems;
        _earnings = earnings;
        _deductions = deductions;
        _taxBands = taxBands;
        _setupManager = setupManager;
        _genJnlPostLine = genJnlPostLine;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
        _voucherFactory = voucherFactory;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Works out every active employee's payslip for the run's month from the employee's pay items
    /// and the statutory deductions, replacing what an earlier calculation left. Employees without
    /// any earning for the month are left out. Returns the number of payslips.
    /// </summary>
    public async Task<int> CalculateAsync(PayrollRun run)
    {
        run.EnsureNotPosted();

        await _lines.DeleteAsync(l => l.PayrollRunNo == run.No, autoSave: true);
        await _payslips.DeleteAsync(p => p.PayrollRunNo == run.No, autoSave: true);

        var setup = await _setupManager.GetAsync();
        var earnings = (await _earnings.GetListAsync(e => !e.Blocked)).ToDictionary(e => e.Code, StringComparer.Ordinal);
        var deductions = (await _deductions.GetListAsync(d => !d.Blocked)).ToDictionary(d => d.Code, StringComparer.Ordinal);
        var bands = await _taxBands.GetListAsync();
        var items = (await _payItems.GetListAsync()).Where(i => i.IsDueFor(run.PayPeriod)).ToLookup(i => i.EmployeeNo, StringComparer.Ordinal);

        var employees = (await _employees.GetListAsync(e => e.Status == EmployeeStatus.Active && !e.Blocked)).OrderBy(e => e.No, StringComparer.Ordinal).ToList();
        var count = 0;
        var (gross, deducted, net, employer) = (0m, 0m, 0m, 0m);

        foreach (var employee in employees)
        {
            var own = items[employee.No].ToList();
            var earningItems = own.Where(i => i.ItemType == PayItemType.Earning && earnings.ContainsKey(i.Code)).ToList();
            if (earningItems.Count == 0)
            {
                continue;
            }

            var payslip = new Payslip(GuidGenerator.Create(), run, employee);
            var lines = new List<PayslipLine>();

            // Earnings: the basic first, as the others may be a percentage of it.
            var basicItem = earningItems.FirstOrDefault(i => earnings[i.Code].BasicPay);
            var basic = basicItem == null ? 0m : Round(basicItem.Amount > 0m ? basicItem.Amount : earnings[basicItem.Code].DefaultValue);
            var (grossPay, taxableGross) = (0m, 0m);

            foreach (var item in earningItems.OrderBy(i => earnings[i.Code].BasicPay ? 0 : 1).ThenBy(i => i.Code, StringComparer.Ordinal))
            {
                var earning = earnings[item.Code];
                var amount = earning.BasicPay
                    ? basic
                    : Round(item.Amount > 0m ? item.Amount : earning.CalculationMethod == PayCalculationMethod.PercentOfBasic ? basic * earning.DefaultValue / 100m : earning.DefaultValue);

                if (amount == 0m)
                {
                    continue;
                }

                lines.Add(NewLine(run, employee, PayslipLineType.Earning, earning.Code, earning.Description, amount));
                grossPay += amount;
                taxableGross += earning.Taxable ? amount : 0m;
            }

            // Deductions: the employee's own and the statutory ones, income tax last as the others may lower taxable pay.
            var deductionAmounts = own.Where(i => i.ItemType == PayItemType.Deduction && deductions.ContainsKey(i.Code)).ToDictionary(i => i.Code, i => i.Amount, StringComparer.Ordinal);
            foreach (var statutory in deductions.Values.Where(d => d.Statutory))
            {
                deductionAmounts.TryAdd(statutory.Code, 0m);
            }

            var (totalDeductions, taxDeductible, employerTotal, incomeTax) = (0m, 0m, 0m, 0m);
            foreach (var (code, own_amount) in deductionAmounts.OrderBy(d => deductions[d.Key].IsIncomeTax ? 1 : 0).ThenBy(d => d.Key, StringComparer.Ordinal))
            {
                var deduction = deductions[code];
                decimal amount;
                if (deduction.IsIncomeTax)
                {
                    var taxablePay = Math.Max(0m, taxableGross - taxDeductible);
                    amount = Math.Max(0m, PayrollTaxBand.TaxOn(taxablePay, bands) - setup.PersonalRelief);
                    incomeTax += amount;
                }
                else
                {
                    amount = deduction.Cap(
                        own_amount > 0m
                            ? own_amount
                            : deduction.CalculationMethod switch
                            {
                                PayCalculationMethod.PercentOfBasic => basic * deduction.DefaultValue / 100m,
                                PayCalculationMethod.PercentOfGross => grossPay * deduction.DefaultValue / 100m,
                                _ => deduction.DefaultValue,
                            }
                    );
                }

                if (amount == 0m)
                {
                    continue;
                }

                lines.Add(NewLine(run, employee, PayslipLineType.Deduction, deduction.Code, deduction.Description, amount));
                totalDeductions += amount;
                taxDeductible += deduction.TaxDeductible ? amount : 0m;

                var share = Round(amount * deduction.EmployerContributionPct / 100m);
                if (share > 0m)
                {
                    lines.Add(NewLine(run, employee, PayslipLineType.EmployerContribution, deduction.Code, $"{deduction.Description} (employer)", share));
                    employerTotal += share;
                }
            }

            if (grossPay - totalDeductions < 0m)
            {
                throw new BusinessException(ErpErrorCodes.Payroll.NetPayNegative)
                    .WithData("employeeNo", employee.No)
                    .WithData("net", (grossPay - totalDeductions).ToString("N2"));
            }

            payslip.SetTotals(basic, grossPay, Math.Max(0m, taxableGross - taxDeductible), incomeTax, totalDeductions, employerTotal);
            await _payslips.InsertAsync(payslip, autoSave: true);
            await _lines.InsertManyAsync(lines, autoSave: true);

            count++;
            gross += grossPay;
            deducted += totalDeductions;
            net += payslip.NetPay;
            employer += employerTotal;
        }

        run.NoOfEmployees = count;
        run.TotalGross = gross;
        run.TotalDeductions = deducted;
        run.TotalNet = net;
        run.TotalEmployerContributions = employer;
        run.MarkCalculated();
        await _runs.UpdateAsync(run, autoSave: true);
        return count;
    }

    public async Task PostAsync(PayrollRun run)
    {
        if (run.Status != PayrollRunStatus.Calculated)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.RunStatusWrong).WithData("documentNo", run.No).WithData("status", run.Status);
        }

        await _glSetupManager.CheckPostingDateAsync(run.PostingDate);

        var payslips = await _payslips.GetListAsync(p => p.PayrollRunNo == run.No);
        var lines = await _lines.GetListAsync(l => l.PayrollRunNo == run.No);
        if (payslips.Count == 0 || payslips.Sum(p => p.GrossPay) == 0m)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.NothingToPost).WithData("documentNo", run.No);
        }

        // A month is paid once: a second run for it would pay everyone twice.
        var (id, period) = (run.Id, run.PayPeriod);
        var other = await _runs.FirstOrDefaultAsync(r => r.Id != id && r.PayPeriod == period && r.Status == PayrollRunStatus.Posted);
        if (other != null)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.PeriodAlreadyPosted).WithData("period", run.PayPeriod.ToString("MMM yyyy")).WithData("documentNo", other.No);
        }

        // Every account is checked before anything is written: a run is posted whole or not at all.
        var earnings = (await _earnings.GetListAsync()).ToDictionary(e => e.Code, StringComparer.Ordinal);
        var deductions = (await _deductions.GetListAsync()).ToDictionary(d => d.Code, StringComparer.Ordinal);
        var postings = new List<(string Account, string Description, decimal Amount)>();

        foreach (var group in lines.GroupBy(l => (l.LineType, l.Code)).OrderBy(g => g.Key.LineType).ThenBy(g => g.Key.Code, StringComparer.Ordinal))
        {
            var total = group.Sum(l => l.Amount);
            switch (group.Key.LineType)
            {
                case PayslipLineType.Earning:
                    var earning = earnings[group.Key.Code];
                    postings.Add((Require(earning.GLAccountNo, "G/L Account No.", $"Payroll Earning {earning.Code}"), earning.Description, total));
                    break;

                case PayslipLineType.Deduction:
                    var deduction = deductions[group.Key.Code];
                    postings.Add((Require(deduction.GLAccountNo, "G/L Account No.", $"Payroll Deduction {deduction.Code}"), deduction.Description, -total));
                    break;

                default:
                    var contribution = deductions[group.Key.Code];
                    postings.Add((Require(contribution.EmployerExpenseAccountNo, "Employer Expense Account No.", $"Payroll Deduction {contribution.Code}"), $"{contribution.Description} (employer)", total));
                    postings.Add((Require(contribution.GLAccountNo, "G/L Account No.", $"Payroll Deduction {contribution.Code}"), $"{contribution.Description} (employer)", -total));
                    break;
            }
        }

        var register = await _registerManager.OpenAsync(run.PostingDate, SourceCode, run.No);
        var context = new GLPostingContext(register, SourceCode);
        var description = run.Description.IsNullOrWhiteSpace() ? $"Payroll {run.PayPeriod:MMM yyyy}" : run.Description;

        foreach (var (account, text, amount) in postings)
        {
            await _genJnlPostLine.PostGLDirectAsync(account, run.PostingDate, GLEntryDocumentType.None, run.No, $"{text} {run.PayPeriod:MMM yyyy}", amount, run.No, context: context);
        }

        var lineNo = 0;
        foreach (var payslip in payslips.Where(p => p.NetPay != 0m).OrderBy(p => p.EmployeeNo, StringComparer.Ordinal))
        {
            await _genJnlPostLine.PostLineAsync(
                new GenJournalLine(
                    GuidGenerator.Create(),
                    Guid.Empty,
                    ++lineNo,
                    run.PostingDate,
                    GLEntryDocumentType.None,
                    run.No,
                    GenJournalAccountType.Employee,
                    payslip.EmployeeNo,
                    $"Net pay {description}".Truncate(ErpDomainConsts.MaxDescriptionLength),
                    -payslip.NetPay
                ),
                context
            );
        }

        await _registerManager.CloseAsync(register);

        run.MarkPosted(Clock.Now, _currentUser.UserName);
        await _runs.UpdateAsync(run, autoSave: true);
    }

    /// <summary>Raises the payment voucher that pays each employee's net pay out of the employee's account.</summary>
    public async Task<PaymentVoucherHeader> RaisePaymentVoucherAsync(PayrollRun run, DateTime date)
    {
        if (run.Status != PayrollRunStatus.Posted)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.RunStatusWrong).WithData("documentNo", run.No).WithData("status", run.Status);
        }

        if (!run.PaymentVoucherNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Payroll.VoucherAlreadyRaised).WithData("documentNo", run.No).WithData("voucherNo", run.PaymentVoucherNo);
        }

        var payslips = (await _payslips.GetListAsync(p => p.PayrollRunNo == run.No && p.NetPay > 0m)).OrderBy(p => p.EmployeeNo, StringComparer.Ordinal).ToList();
        if (payslips.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.NothingToPost).WithData("documentNo", run.No);
        }

        var narration = $"Net pay {run.PayPeriod:MMM yyyy}";
        var voucher = await _voucherFactory.CreateAsync(
            VoucherSourceType,
            run.No,
            date,
            "Employees",
            narration,
            payslips.Select(p => new PaymentVoucherRequestLine(GenJournalAccountType.Employee, p.EmployeeNo, $"{narration} {p.EmployeeName}".Truncate(ErpDomainConsts.MaxDescriptionLength), p.NetPay)).ToList()
        );

        run.PaymentVoucherNo = voucher.No;
        await _runs.UpdateAsync(run, autoSave: true);
        return voucher;
    }

    private PayslipLine NewLine(PayrollRun run, Employee employee, PayslipLineType type, string code, string description, decimal amount) =>
        new(GuidGenerator.Create(), run.No, employee.No, type, code, description, amount);

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static string Require(string accountNo, string field, string setup) =>
        accountNo.IsNullOrWhiteSpace()
            ? throw new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing).WithData("field", field).WithData("setup", setup)
            : accountNo;
}
