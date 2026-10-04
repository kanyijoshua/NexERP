using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Payroll;

/// <summary>Payroll Setup: one row per company with the number series of payroll runs and the tax relief every employee gets.</summary>
public class PayrollSetup : CompanyEntity
{
    public string PayrollRunNos { get; private set; }

    /// <summary>Taken off each employee's monthly income tax; tax never goes below zero.</summary>
    public decimal PersonalRelief { get; private set; }

    protected PayrollSetup() { }

    public PayrollSetup(Guid id)
        : base(id) { }

    public void Set(string payrollRunNos, decimal personalRelief)
    {
        if (personalRelief < 0)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.NegativeAmount).WithData("field", "Personal Relief");
        }

        PayrollRunNos = payrollRunNos.IsNullOrWhiteSpace() ? null : Check.Length(payrollRunNos.Trim(), nameof(payrollRunNos), ErpDomainConsts.MaxNoSeriesCodeLength);
        PersonalRelief = personalRelief;
    }
}

public class PayrollSetupManager : DomainService
{
    private readonly IRepository<PayrollSetup, Guid> _repository;

    public PayrollSetupManager(IRepository<PayrollSetup, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<PayrollSetup> GetAsync()
    {
        return await _repository.FirstOrDefaultAsync()
            ?? await _repository.InsertAsync(new PayrollSetup(GuidGenerator.Create()), autoSave: true);
    }
}

/// <summary>Something employees are paid: basic salary, an allowance, overtime. Its cost is debited to its account.</summary>
public class PayrollEarning : CodeTableEntity
{
    public PayCalculationMethod CalculationMethod { get; private set; }

    /// <summary>The amount of a flat earning nobody has an amount of their own for, or the percentage of the others.</summary>
    public decimal DefaultValue { get; private set; }

    /// <summary>The earning other earnings and deductions take percentages of; one earning should be it.</summary>
    public bool BasicPay { get; private set; }

    public bool Taxable { get; private set; } = true;

    /// <summary>The expense account the earning is debited to.</summary>
    public string GLAccountNo { get; private set; }

    public bool Blocked { get; private set; }

    protected PayrollEarning() { }

    public PayrollEarning(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(PayCalculationMethod calculationMethod, decimal defaultValue, bool basicPay, bool taxable, string glAccountNo, bool blocked)
    {
        if (defaultValue < 0)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.NegativeAmount).WithData("field", "Default Value");
        }

        // An earning cannot be a percentage of the gross it is part of, nor taxed by the bands.
        CalculationMethod = calculationMethod is PayCalculationMethod.PercentOfGross or PayCalculationMethod.TaxBands ? PayCalculationMethod.FlatAmount : calculationMethod;
        CalculationMethod = basicPay ? PayCalculationMethod.FlatAmount : CalculationMethod;
        DefaultValue = defaultValue;
        BasicPay = basicPay;
        Taxable = taxable;
        GLAccountNo = glAccountNo.IsNullOrWhiteSpace() ? null : Check.Length(glAccountNo.Trim(), nameof(glAccountNo), ErpDomainConsts.MaxNoLength);
        Blocked = blocked;
    }
}

/// <summary>
/// Something taken off employees' pay: income tax, a pension or social security contribution, a
/// loan repayment. What is deducted is owed to someone else, so it is credited to a liability
/// account, together with what the employer adds on top.
/// </summary>
public class PayrollDeduction : CodeTableEntity
{
    public PayCalculationMethod CalculationMethod { get; private set; }

    /// <summary>The flat amount, or the percentage of basic or gross pay.</summary>
    public decimal DefaultValue { get; private set; }

    /// <summary>The most deducted in a month; 0 is no cap.</summary>
    public decimal MaximumAmount { get; private set; }

    /// <summary>Taken off pay before income tax is worked out, e.g. a registered pension contribution.</summary>
    public bool TaxDeductible { get; private set; }

    /// <summary>What the employer pays on top, as a percentage of what is deducted, e.g. 100 for a matched pension.</summary>
    public decimal EmployerContributionPct { get; private set; }

    /// <summary>The liability account what is deducted (and the employer's share) is credited to.</summary>
    public string GLAccountNo { get; private set; }

    /// <summary>The expense account the employer's share is debited to.</summary>
    public string EmployerExpenseAccountNo { get; private set; }

    public bool Blocked { get; private set; }

    /// <summary>Deducted from every employee on the payroll (income tax, social security), without a pay item each.</summary>
    public bool Statutory { get; private set; }

    public void SetStatutory(bool statutory) => Statutory = statutory;

    protected PayrollDeduction() { }

    public PayrollDeduction(Guid id, string code, string description)
        : base(id, code, description) { }

    public bool IsIncomeTax => CalculationMethod == PayCalculationMethod.TaxBands;

    public void Set(
        PayCalculationMethod calculationMethod,
        decimal defaultValue,
        decimal maximumAmount,
        bool taxDeductible,
        decimal employerContributionPct,
        string glAccountNo,
        string employerExpenseAccountNo,
        bool blocked
    )
    {
        if (defaultValue < 0 || maximumAmount < 0 || employerContributionPct < 0)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.NegativeAmount).WithData("field", defaultValue < 0 ? "Default Value" : maximumAmount < 0 ? "Maximum Amount" : "Employer Contribution %");
        }

        CalculationMethod = calculationMethod;
        DefaultValue = defaultValue;
        MaximumAmount = maximumAmount;
        TaxDeductible = taxDeductible && calculationMethod != PayCalculationMethod.TaxBands;
        EmployerContributionPct = employerContributionPct;
        GLAccountNo = Account(glAccountNo, nameof(glAccountNo));
        EmployerExpenseAccountNo = Account(employerExpenseAccountNo, nameof(employerExpenseAccountNo));
        Blocked = blocked;
    }

    /// <summary>The amount capped at the maximum, rounded to cents.</summary>
    public decimal Cap(decimal amount)
    {
        var rounded = Math.Round(Math.Max(0m, amount), 2, MidpointRounding.AwayFromZero);
        return MaximumAmount > 0m ? Math.Min(rounded, MaximumAmount) : rounded;
    }

    private static string Account(string value, string name) =>
        value.IsNullOrWhiteSpace() ? null : Check.Length(value.Trim(), name, ErpDomainConsts.MaxNoLength);
}

/// <summary>One band of the monthly income tax table: the rate on the part of taxable pay between its limits.</summary>
public class PayrollTaxBand : CompanyEntity
{
    public decimal LowerLimit { get; private set; }

    /// <summary>0 is no upper limit: the top band.</summary>
    public decimal UpperLimit { get; private set; }

    public decimal RatePct { get; private set; }

    protected PayrollTaxBand() { }

    public PayrollTaxBand(Guid id, decimal lowerLimit)
        : base(id)
    {
        SetLowerLimit(lowerLimit);
    }

    public void SetLowerLimit(decimal lowerLimit)
    {
        if (lowerLimit < 0)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.InvalidTaxBand).WithData("lower", lowerLimit);
        }

        LowerLimit = lowerLimit;
    }

    public void Set(decimal upperLimit, decimal ratePct)
    {
        if ((upperLimit != 0m && upperLimit <= LowerLimit) || ratePct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.InvalidTaxBand).WithData("lower", LowerLimit);
        }

        UpperLimit = upperLimit;
        RatePct = ratePct;
    }

    /// <summary>The tax on taxable pay by a set of bands.</summary>
    public static decimal TaxOn(decimal taxablePay, IEnumerable<PayrollTaxBand> bands)
    {
        var tax = 0m;
        foreach (var band in bands.OrderBy(b => b.LowerLimit))
        {
            if (taxablePay <= band.LowerLimit)
            {
                break;
            }

            var top = band.UpperLimit == 0m ? taxablePay : Math.Min(taxablePay, band.UpperLimit);
            tax += (top - band.LowerLimit) * band.RatePct / 100m;
        }

        return Math.Round(tax, 2, MidpointRounding.AwayFromZero);
    }
}

/// <summary>
/// A recurring earning or deduction of one employee: the employee's basic salary, an allowance, a
/// loan repayment. It is paid every month from its start to its end; an amount of zero takes the
/// earning's or deduction's own calculation.
/// </summary>
public class EmployeePayItem : CompanyEntity
{
    public string EmployeeNo { get; private set; }
    public PayItemType ItemType { get; private set; }
    public string Code { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    protected EmployeePayItem() { }

    public EmployeePayItem(Guid id, string employeeNo, PayItemType itemType, string code)
        : base(id)
    {
        SetKey(employeeNo, itemType, code);
    }

    public void SetKey(string employeeNo, PayItemType itemType, string code)
    {
        EmployeeNo = Check.NotNullOrWhiteSpace(employeeNo, nameof(employeeNo), ErpDomainConsts.MaxNoLength).Trim();
        ItemType = itemType;
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public void Set(decimal amount, DateTime? startDate, DateTime? endDate)
    {
        if (amount < 0)
        {
            throw new BusinessException(ErpErrorCodes.Payroll.NegativeAmount).WithData("field", "Amount");
        }

        if (startDate.HasValue && endDate.HasValue && endDate.Value.Date < startDate.Value.Date)
        {
            throw new BusinessException(ErpErrorCodes.Reports.PeriodReversed);
        }

        Amount = amount;
        StartDate = startDate?.Date;
        EndDate = endDate?.Date;
    }

    /// <summary>Whether the item is paid in the month starting on <paramref name="period"/>.</summary>
    public bool IsDueFor(DateTime period)
    {
        var monthEnd = period.AddMonths(1).AddDays(-1);
        return (!StartDate.HasValue || StartDate.Value <= monthEnd) && (!EndDate.HasValue || EndDate.Value >= period);
    }
}
