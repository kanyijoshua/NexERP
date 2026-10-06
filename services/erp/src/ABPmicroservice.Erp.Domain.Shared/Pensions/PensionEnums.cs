namespace ABPmicroservice.Erp.Pensions;

/// <summary>What kind of arrangement a scheme is.</summary>
public enum PensionSchemeType
{
    PensionFund = 0,
    ProvidentFund = 1,
    InvestmentPool = 2,
}

/// <summary>How benefits are promised: by formula, by what was contributed, or a mix.</summary>
public enum PensionPlanType
{
    DefinedBenefit = 0,
    DefinedContribution = 1,
    IndividualPensionPlan = 2,
    Hybrid = 3,
}

/// <summary>An umbrella scheme takes many sponsors; a single scheme has one.</summary>
public enum PensionSchemeMode
{
    Umbrella = 0,
    Single = 1,
}

public enum PensionSchemeStatus
{
    Open = 0,

    /// <summary>Closed to new members and new contributions.</summary>
    Closed = 1,
}

/// <summary>How declared interest accrues on a member's balance over the period.</summary>
public enum InterestCalculationMode
{
    CompoundMonthly = 0,
    CompoundDaily = 1,
    Simple = 2,
}

public enum MemberStatus
{
    None = 0,
    Active = 1,
    Inactive = 2,
    Deferred = 3,
    Dormant = 4,
    Pending = 5,
    DeathInService = 6,
    DeathInDeferment = 7,
    Shortfall = 8,
    Deceased = 9,
    Suspended = 10,
    Open = 11,
}

public enum MemberContributionStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2,
    Other = 3,
}

public enum MemberGender
{
    None = 0,
    Female = 1,
    Male = 2,
}

public enum MemberMaritalStatus
{
    Single = 0,
    Married = 1,
    Separated = 2,
    Divorced = 3,
    Widow = 4,
    Widower = 5,
    Minor = 6,
}

/// <summary>Whose money an entry is: the member's, the employer's, or additional voluntary contributions of either.</summary>
public enum PensionContributionType
{
    None = 0,
    EmployeeContribution = 1,
    EmployerContribution = 2,
    EmployerAdditional = 3,
    EmployeeAdditional = 4,
    Pre90Employee = 5,
    Pre90Employer = 6,
}

/// <summary>How the money came in.</summary>
public enum PensionContributionMode
{
    None = 0,
    Normal = 1,
    Arrears = 2,
    TransferIn = 3,
    Gratuity = 4,
    TierII = 5,
    GroupLife = 6,
}

/// <summary>What an entry of the member ledger does to the member's fund.</summary>
public enum PensionTransactionType
{
    None = 0,
    Contribution = 1,
    Withdrawal = 2,
    Interest = 3,
    Reserve = 4,
    Levies = 5,
    TaxOnInterest = 6,
    ReserveWithdrawal = 7,
    InterestWithdrawal = 8,
    GroupLife = 9,
}

/// <summary>Registered (tax exempt) or unregistered (non tax exempt) money: the two are taxed differently on exit.</summary>
public enum PensionExemptionType
{
    None = 0,
    TaxExempt = 1,
    NonTaxExempt = 2,
}

public enum PensionDocumentStatus
{
    Open = 0,
    Released = 1,
    PendingApproval = 2,
    Posted = 3,
    Reversed = 4,
    Rejected = 5,
}

public enum PensionerStatus
{
    Active = 0,

    /// <summary>Not paid until the pensioner is made active again, e.g. while proof of life is awaited.</summary>
    Suspended = 1,

    /// <summary>The pension has ended.</summary>
    Ceased = 2,
}

/// <summary>A defined benefit calculation is worked out (Open) and approved, which retires the member onto it.</summary>
public enum BenefitCalculationStatus
{
    Open = 0,
    Approved = 1,
}

public enum MemberExitStatus
{
    Open = 0,
    PendingApproval = 1,
    Approved = 2,
    Canceled = 3,
    Rejected = 4,
    Posted = 5,
}

/// <summary>An actual exit pays the member out; a projection only shows what an exit would pay.</summary>
public enum MemberWithdrawalType
{
    Actual = 0,
    Projection = 1,
}

/// <summary>Which portions an exit reason pays out.</summary>
public enum ExitPaymentOption
{
    None = 0,
    PayEmployeeAndEmployer = 1,
    PayEmployee = 2,

    /// <summary>The member's own portion is paid; the employer's stays in the scheme until retirement.</summary>
    PayEmployeeEmployerDeferred = 3,
}

/// <summary>How a beneficiary is related to the member who nominated them.</summary>
public enum BeneficiaryRelationship
{
    None = 0,
    Spouse = 1,
    Child = 2,
    Parent = 3,
    Sibling = 4,
    Dependant = 5,
    Other = 6,
}

public enum BeneficiaryStatus
{
    Active = 0,

    /// <summary>Kept on record but given no share, e.g. a child past the age of dependency.</summary>
    Suspended = 1,
}

/// <summary>
/// How the part of a contribution above the monthly tax relief limit is shared out. Under employee
/// priority the employee's money uses the limit first; under contribution rate the limit is shared in
/// proportion to what each side pays.
/// </summary>
public enum ExcessContributionAllocation
{
    EmployeePriority = 0,
    ContributionRate = 1,
}

/// <summary>What changed a pensioner's pension or status, as kept in the pensioner's history.</summary>
public enum PensionerChangeType
{
    None = 0,
    Increment = 1,
    Suspension = 2,
    Reinstatement = 3,
    LifeCertificate = 4,
    ArrearsPaid = 5,
}

public enum PensionIncrementStatus
{
    Open = 0,
    Applied = 1,
}

/// <summary>How a pay mode reaches the pensioner.</summary>
public enum PensionerPaymentType
{
    Bank = 0,
    MobileMoney = 1,
    Cheque = 2,
    Cash = 3,
}

/// <summary>Whether a pensioner pay item adds to the pension or is taken off it.</summary>
public enum PensionerPayItemType
{
    Earning = 0,
    Deduction = 1,
}

/// <summary>How a pensioner pay item's amount is worked out each month.</summary>
public enum PensionerPayItemCalculation
{
    FlatAmount = 0,

    /// <summary>A percentage of the month's pension.</summary>
    PercentOfPension = 1,
}

/// <summary>What an age factor of a defined benefit scheme is used for.</summary>
public enum PensionFactorType
{
    /// <summary>Multiplies the pension of a member retiring before the normal retirement age (below 1).</summary>
    EarlyRetirement = 0,

    /// <summary>Multiplies the pension of a member retiring after the normal retirement age (above 1).</summary>
    LateRetirement = 1,

    /// <summary>What one unit of annual pension given up is worth as a lump sum, at the age of retirement.</summary>
    Commutation = 2,
}

/// <summary>The salary a defined benefit pension is based on when none is keyed in.</summary>
public enum PensionableSalaryBasis
{
    /// <summary>Twelve times the member's current monthly salary.</summary>
    CurrentSalary = 0,

    /// <summary>The average yearly salary over the last years of service, from the member's salary history.</summary>
    AverageOfLastYears = 1,

    /// <summary>The highest salary of any twelve months in a row within the last years of service.</summary>
    HighestAnnualSalary = 2,
}
