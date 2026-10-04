namespace ABPmicroservice.Erp.Payroll;

public enum PayItemType
{
    Earning = 0,
    Deduction = 1,
}

/// <summary>How an earning or a deduction is worked out each month.</summary>
public enum PayCalculationMethod
{
    /// <summary>The employee's amount, or the item's default.</summary>
    FlatAmount = 0,

    /// <summary>A percentage of the employee's basic pay.</summary>
    PercentOfBasic = 1,

    /// <summary>A percentage of the employee's gross pay.</summary>
    PercentOfGross = 2,

    /// <summary>Income tax on taxable pay, by the payroll tax bands, less the personal relief.</summary>
    TaxBands = 3,
}

/// <summary>A payroll run is calculated (and recalculated) while open, then posted.</summary>
public enum PayrollRunStatus
{
    Open = 0,
    Calculated = 1,
    Posted = 2,
}

/// <summary>What a payslip line is: pay, a deduction from it, or what the employer pays on top.</summary>
public enum PayslipLineType
{
    Earning = 0,
    Deduction = 1,
    EmployerContribution = 2,
}
