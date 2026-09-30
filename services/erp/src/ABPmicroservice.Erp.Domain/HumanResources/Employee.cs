using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>Employee. Mirrors Business Central table 5200 "Employee".</summary>
public class Employee : CompanyAggregateRoot
{
    public string No { get; private set; }
    public string FirstName { get; private set; }
    public string MiddleName { get; private set; }
    public string LastName { get; private set; }
    public string JobTitle { get; private set; }

    public string Address { get; private set; }
    public string City { get; private set; }
    public string PostCode { get; private set; }
    public string CountryRegionCode { get; private set; }
    public string PhoneNo { get; private set; }
    public string MobilePhoneNo { get; private set; }
    public string Email { get; private set; }
    public string CompanyEmail { get; private set; }

    public DateTime? BirthDate { get; private set; }
    public string SocialSecurityNo { get; private set; }

    public DateTime? EmploymentDate { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public DateTime? InactiveDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public string GroundsForTermCode { get; private set; }
    public string EmplymtContractCode { get; private set; }
    public string UnionCode { get; private set; }

    public string EmployeePostingGroup { get; private set; }
    public string BankAccountNo { get; private set; }
    public string Iban { get; private set; }

    /// <summary>The Salesperson/Purchaser the employee is, if any.</summary>
    public string SalespersPurchCode { get; private set; }

    public bool Blocked { get; private set; }

    /// <summary>What the company owes the employee (negative) or is owed back, kept by posting.</summary>
    public decimal Balance { get; internal set; }

    protected Employee() { }

    public Employee(Guid id, string no, string firstName, string lastName)
        : base(id)
    {
        SetNo(no);
        SetName(firstName, null, lastName);
    }

    /// <summary>"First Middle Last", as BC's FullName.</summary>
    public string FullName => string.Join(" ", new[] { FirstName, MiddleName, LastName }).Replace("  ", " ").Trim();

    public void SetNo(string no) =>
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim();

    public void SetName(string firstName, string middleName, string lastName)
    {
        FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), ErpDomainConsts.MaxNameLength / 2);
        MiddleName = Check.Length(middleName, nameof(middleName), ErpDomainConsts.MaxNameLength / 2);
        LastName = Check.Length(lastName, nameof(lastName), ErpDomainConsts.MaxNameLength / 2);
    }

    public void SetJobTitle(string jobTitle) =>
        JobTitle = Check.Length(jobTitle, nameof(jobTitle), ErpDomainConsts.MaxJobTitleLength);

    public void SetAddress(string address, string city, string postCode, string countryRegionCode)
    {
        Address = Check.Length(address, nameof(address), ErpDomainConsts.MaxAddressLength);
        City = Check.Length(city, nameof(city), ErpDomainConsts.MaxCityLength);
        PostCode = Check.Length(postCode, nameof(postCode), ErpDomainConsts.MaxPostCodeLength);
        CountryRegionCode = Check.Length(countryRegionCode, nameof(countryRegionCode), ErpDomainConsts.MaxCountryRegionCodeLength);
    }

    public void SetContact(string phoneNo, string mobilePhoneNo, string email, string companyEmail)
    {
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        MobilePhoneNo = Check.Length(mobilePhoneNo, nameof(mobilePhoneNo), ErpDomainConsts.MaxPhoneLength);
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
        CompanyEmail = Check.Length(companyEmail, nameof(companyEmail), ErpDomainConsts.MaxEmailLength);
    }

    public void SetPersonal(DateTime? birthDate, string socialSecurityNo)
    {
        BirthDate = birthDate?.Date;
        SocialSecurityNo = Check.Length(socialSecurityNo, nameof(socialSecurityNo), ErpDomainConsts.MaxCodeLength * 2);
    }

    public void SetEmployment(DateTime? employmentDate, string emplymtContractCode, string unionCode)
    {
        EmploymentDate = employmentDate?.Date;
        EmplymtContractCode = Code(emplymtContractCode, nameof(emplymtContractCode));
        UnionCode = Code(unionCode, nameof(unionCode));
    }

    /// <summary>Active, inactive from a date, or terminated on a date for a reason (BC Status and its dates).</summary>
    public void SetStatus(EmployeeStatus status, DateTime? inactiveDate, DateTime? terminationDate, string groundsForTermCode)
    {
        Status = status;
        InactiveDate = status == EmployeeStatus.Inactive ? inactiveDate?.Date : null;
        TerminationDate = status == EmployeeStatus.Terminated ? terminationDate?.Date : null;
        GroundsForTermCode = status == EmployeeStatus.Terminated ? Code(groundsForTermCode, nameof(groundsForTermCode)) : null;
    }

    public void SetPayment(string employeePostingGroup, string bankAccountNo, string iban, string salespersPurchCode)
    {
        EmployeePostingGroup = Code(employeePostingGroup, nameof(employeePostingGroup));
        BankAccountNo = Check.Length(bankAccountNo, nameof(bankAccountNo), ErpDomainConsts.MaxBankAccountNoLength);
        Iban = Check.Length(iban?.Replace(" ", "").ToUpperInvariant(), nameof(iban), ErpDomainConsts.MaxIbanLength);
        SalespersPurchCode = Code(salespersPurchCode, nameof(salespersPurchCode));
    }

    public void Block() => Blocked = true;

    public void Unblock() => Blocked = false;

    internal void ApplyBalance(decimal amount) => Balance += amount;

    private static string Code(string value, string name) =>
        CodeTableEntity.NormalizeCode(Check.Length(value, name, ErpDomainConsts.MaxCodeLength));
}

/// <summary>
/// Employee Absence. Mirrors Business Central table 5207: a period an employee was away, why, and
/// how much of it in the cause's unit.
/// </summary>
public class EmployeeAbsence : CompanyEntity
{
    public Guid EmployeeId { get; private set; }
    public string EmployeeNo { get; private set; }
    public DateTime FromDate { get; private set; }
    public DateTime? ToDate { get; private set; }
    public string CauseOfAbsenceCode { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public string UnitOfMeasureCode { get; private set; }

    protected EmployeeAbsence() { }

    public EmployeeAbsence(Guid id, Guid employeeId, string employeeNo)
        : base(id)
    {
        SetEmployee(employeeId, employeeNo);
    }

    public void SetEmployee(Guid employeeId, string employeeNo)
    {
        EmployeeId = employeeId;
        EmployeeNo = Check.NotNullOrWhiteSpace(employeeNo, nameof(employeeNo), ErpDomainConsts.MaxNoLength);
    }

    public void Set(DateTime fromDate, DateTime? toDate, string causeOfAbsenceCode, string description, decimal quantity, string unitOfMeasureCode)
    {
        if ((toDate.HasValue && toDate.Value.Date < fromDate.Date) || quantity < 0)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.InvalidAbsencePeriod);
        }

        FromDate = fromDate.Date;
        ToDate = toDate?.Date;
        CauseOfAbsenceCode = CodeTableEntity.NormalizeCode(Check.Length(causeOfAbsenceCode, nameof(causeOfAbsenceCode), ErpDomainConsts.MaxCodeLength));
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        // One day per calendar day when no quantity is given, as BC fills it in.
        Quantity = quantity > 0 ? quantity : ((ToDate ?? FromDate) - FromDate).Days + 1;
        UnitOfMeasureCode = CodeTableEntity.NormalizeCode(Check.Length(unitOfMeasureCode, nameof(unitOfMeasureCode), ErpDomainConsts.MaxUnitOfMeasureCodeLength));
    }
}
