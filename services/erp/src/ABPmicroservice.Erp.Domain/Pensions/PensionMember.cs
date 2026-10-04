using System;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// A sponsor: an employer that pays contributions into a scheme for its employees. A sponsor that
/// is also set up as a customer has its contributions posted to that customer's account, so what
/// it owes is followed in the receivables ledger and settled by an ordinary receipt.
/// </summary>
public class PensionSponsor : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Name { get; private set; }

    /// <summary>The scheme the sponsor participates in.</summary>
    public string SchemeCode { get; private set; }

    /// <summary>The customer the sponsor's contributions are charged to; blank posts them to the accrual account.</summary>
    public string CustomerNo { get; private set; }

    public string Address { get; private set; }
    public string City { get; private set; }
    public string PhoneNo { get; private set; }
    public string Email { get; private set; }
    public string Contact { get; private set; }
    public string TaxPinNo { get; private set; }

    /// <summary>Percentage of pensionable salary the employee contributes.</summary>
    public decimal EmployeeRatePct { get; private set; }

    /// <summary>Percentage of pensionable salary the employer contributes.</summary>
    public decimal EmployerRatePct { get; private set; }

    public DateTime? LastScheduleDate { get; internal set; }

    public bool Blocked { get; private set; }

    protected PensionSponsor() { }

    public PensionSponsor(Guid id, string no, string name, string schemeCode)
        : base(id)
    {
        SetNo(no);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        SchemeCode = SchemeOf(schemeCode);
    }

    public void SetNo(string no) => No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();

    public void Set(
        string name,
        string schemeCode,
        string customerNo,
        string address,
        string city,
        string phoneNo,
        string email,
        string contact,
        string taxPinNo,
        decimal employeeRatePct,
        decimal employerRatePct,
        bool blocked
    )
    {
        if (employeeRatePct is < 0 or > 100 || employerRatePct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", employeeRatePct is < 0 or > 100 ? employeeRatePct : employerRatePct);
        }

        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        SchemeCode = SchemeOf(schemeCode);
        CustomerNo = customerNo.IsNullOrWhiteSpace() ? null : Check.Length(customerNo.Trim(), nameof(customerNo), ErpDomainConsts.MaxNoLength);
        Address = Check.Length(address, nameof(address), ErpDomainConsts.MaxAddressLength);
        City = Check.Length(city, nameof(city), ErpDomainConsts.MaxCityLength);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
        Contact = Check.Length(contact, nameof(contact), ErpDomainConsts.MaxContactLength);
        TaxPinNo = Check.Length(taxPinNo, nameof(taxPinNo), ErpDomainConsts.MaxVatRegistrationNoLength);
        EmployeeRatePct = employeeRatePct;
        EmployerRatePct = employerRatePct;
        Blocked = blocked;
    }

    internal static string SchemeOf(string schemeCode) =>
        Check.NotNullOrWhiteSpace(schemeCode, nameof(schemeCode), ErpDomainConsts.MaxDimensionValueCodeLength).Trim().ToUpperInvariant();
}

/// <summary>
/// A member of a scheme. What the member owns in the scheme is not stored here: it is the sum of
/// the member's ledger entries, which is what makes a statement and a balance always agree.
/// </summary>
public class PensionMember : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string SchemeCode { get; private set; }
    public string SponsorNo { get; private set; }

    public string FirstName { get; private set; }
    public string OtherName { get; private set; }
    public string LastName { get; private set; }

    /// <summary>"First Other Last", kept so that lists can search and sort on one column.</summary>
    public string FullName { get; private set; }

    public string NationalId { get; private set; }
    public string TaxPinNo { get; private set; }
    public MemberGender Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public MemberMaritalStatus MaritalStatus { get; private set; }

    /// <summary>The member's number in the sponsor's payroll, which is how schedules identify members.</summary>
    public string PayrollNo { get; private set; }

    public string Designation { get; private set; }
    public DateTime? DateOfEmployment { get; private set; }
    public DateTime? JoinSchemeDate { get; private set; }

    /// <summary>Monthly pensionable salary; contributions are suggested from it.</summary>
    public decimal CurrentSalary { get; private set; }

    public DateTime? ExpectedRetirementDate { get; private set; }

    public MemberStatus Status { get; private set; } = MemberStatus.Active;
    public MemberContributionStatus ContributionStatus { get; private set; }
    public DateTime? ExitDate { get; internal set; }

    public string Address { get; private set; }
    public string City { get; private set; }
    public string PhoneNo { get; private set; }
    public string Email { get; private set; }

    public string BankName { get; private set; }
    public string BankBranch { get; private set; }
    public string BankAccountNo { get; private set; }

    protected PensionMember() { }

    public PensionMember(Guid id, string no, string schemeCode, string sponsorNo, string firstName, string lastName)
        : base(id)
    {
        SetNo(no);
        SetScheme(schemeCode, sponsorNo);
        SetName(firstName, null, lastName);
    }

    public void SetNo(string no) => No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();

    public void SetScheme(string schemeCode, string sponsorNo)
    {
        SchemeCode = PensionSponsor.SchemeOf(schemeCode);
        SponsorNo = Check.NotNullOrWhiteSpace(sponsorNo, nameof(sponsorNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
    }

    public void SetName(string firstName, string otherName, string lastName)
    {
        FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), ErpDomainConsts.MaxNameLength / 2).Trim();
        OtherName = Check.Length(otherName?.Trim(), nameof(otherName), ErpDomainConsts.MaxNameLength / 2);
        LastName = Check.Length(lastName?.Trim(), nameof(lastName), ErpDomainConsts.MaxNameLength / 2);
        FullName = string.Join(" ", new[] { FirstName, OtherName, LastName }.Where(part => !part.IsNullOrWhiteSpace()));
    }

    /// <param name="normalRetirementAge">The scheme's; the expected retirement date is the birthday on which it is reached.</param>
    public void SetPersonal(
        string nationalId,
        string taxPinNo,
        MemberGender gender,
        DateTime? dateOfBirth,
        MemberMaritalStatus maritalStatus,
        int normalRetirementAge
    )
    {
        NationalId = Check.Length(nationalId?.Trim(), nameof(nationalId), ErpDomainConsts.MaxCodeLength * 2);
        TaxPinNo = Check.Length(taxPinNo?.Trim(), nameof(taxPinNo), ErpDomainConsts.MaxVatRegistrationNoLength);
        Gender = gender;
        DateOfBirth = dateOfBirth?.Date;
        MaritalStatus = maritalStatus;
        ExpectedRetirementDate = DateOfBirth?.AddYears(normalRetirementAge);
    }

    public void SetEmployment(string payrollNo, string designation, DateTime? dateOfEmployment, DateTime? joinSchemeDate, decimal currentSalary)
    {
        if (currentSalary < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Current Salary");
        }

        PayrollNo = Check.Length(payrollNo?.Trim(), nameof(payrollNo), ErpDomainConsts.MaxNoLength);
        Designation = Check.Length(designation, nameof(designation), ErpDomainConsts.MaxJobTitleLength);
        DateOfEmployment = dateOfEmployment?.Date;
        JoinSchemeDate = joinSchemeDate?.Date;
        CurrentSalary = currentSalary;
    }

    public void SetStatus(MemberStatus status, MemberContributionStatus contributionStatus)
    {
        Status = status == MemberStatus.None ? MemberStatus.Active : status;
        ContributionStatus = contributionStatus;
    }

    public void SetContact(string address, string city, string phoneNo, string email)
    {
        Address = Check.Length(address, nameof(address), ErpDomainConsts.MaxAddressLength);
        City = Check.Length(city, nameof(city), ErpDomainConsts.MaxCityLength);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
    }

    public void SetBank(string bankName, string bankBranch, string bankAccountNo)
    {
        BankName = Check.Length(bankName, nameof(bankName), ErpDomainConsts.MaxNameLength);
        BankBranch = Check.Length(bankBranch, nameof(bankBranch), ErpDomainConsts.MaxNameLength);
        BankAccountNo = Check.Length(bankAccountNo, nameof(bankAccountNo), ErpDomainConsts.MaxBankAccountNoLength);
    }

    /// <summary>A member who can still be contributed for.</summary>
    public bool IsContributing =>
        Status is MemberStatus.Active or MemberStatus.Dormant or MemberStatus.Open
        && ContributionStatus is MemberContributionStatus.Active or MemberContributionStatus.Other;

    /// <summary>What an exit does to the member: the status its reason leaves the member in, as at the exit date.</summary>
    internal void Exit(MemberStatus statusAfterExit, DateTime exitDate)
    {
        Status = statusAfterExit;
        ContributionStatus = MemberContributionStatus.Inactive;
        ExitDate = exitDate.Date;
    }

    /// <summary>A contribution for a dormant member brings the member back to active.</summary>
    internal void Reactivate()
    {
        if (Status == MemberStatus.Dormant)
        {
            Status = MemberStatus.Active;
            ContributionStatus = MemberContributionStatus.Active;
        }
    }
}
