using ABPmicroservice.Erp.Finance;
using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>Employee.</summary>
public class Employee : CompanyAggregateRoot, IHasNo
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

    /// <summary>Initials.</summary>
    public string Initials { get; private set; }

    /// <summary>Search Name.</summary>
    public string SearchName { get; private set; }

    /// <summary>Address 2.</summary>
    public string Address2 { get; private set; }

    /// <summary>County.</summary>
    public string County { get; private set; }

    /// <summary>Gender.</summary>
    public EmployeeGender Gender { get; private set; }

    /// <summary>Extension.</summary>
    public string Extension { get; private set; }

    /// <summary>Fax No..</summary>
    public string FaxNo { get; private set; }

    /// <summary>Pager.</summary>
    public string Pager { get; private set; }

    /// <summary>Manager No..</summary>
    public string ManagerNo { get; private set; }

    /// <summary>Statistics Group Code.</summary>
    public string StatisticsGroupCode { get; private set; }

    /// <summary>Cause of Inactivity Code.</summary>
    public string CauseOfInactivityCode { get; private set; }

    /// <summary>Global Dimension 1 Code.</summary>
    public string GlobalDimension1Code { get; private set; }

    /// <summary>Global Dimension 2 Code.</summary>
    public string GlobalDimension2Code { get; private set; }

    /// <summary>Alt. Address Code.</summary>
    public string AltAddressCode { get; private set; }

    /// <summary>Alt. Address Start Date.</summary>
    public DateTime? AltAddressStartDate { get; private set; }

    /// <summary>Alt. Address End Date.</summary>
    public DateTime? AltAddressEndDate { get; private set; }

    /// <summary>Bank Branch No..</summary>
    public string BankBranchNo { get; private set; }

    /// <summary>SWIFT Code.</summary>
    public string SwiftCode { get; private set; }

    /// <summary>Currency Code.</summary>
    public string CurrencyCode { get; private set; }

    /// <summary>Application Method.</summary>
    public ApplicationMethod ApplicationMethod { get; private set; }

    /// <summary>Union Membership No..</summary>
    public string UnionMembershipNo { get; private set; }

    /// <summary>Privacy Blocked.</summary>
    public bool PrivacyBlocked { get; private set; }

    protected Employee() { }

    public Employee(Guid id, string no, string firstName, string lastName)
        : base(id)
    {
        SetNo(no);
        SetName(firstName, null, lastName);
    }

    /// <summary>"First Middle Last".</summary>
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

    /// <summary>Active, inactive from a date, or terminated on a date for a reason.</summary>
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

    /// <summary>The card fields beyond those the posting routines read.</summary>
    public void SetAdditionalFields(
        string initials,
        string searchName,
        string address2,
        string county,
        EmployeeGender gender,
        string extension,
        string faxNo,
        string pager,
        string managerNo,
        string statisticsGroupCode,
        string causeOfInactivityCode,
        string globalDimension1Code,
        string globalDimension2Code,
        string altAddressCode,
        DateTime? altAddressStartDate,
        DateTime? altAddressEndDate,
        string bankBranchNo,
        string swiftCode,
        string currencyCode,
        ApplicationMethod applicationMethod,
        string unionMembershipNo,
        bool privacyBlocked
    )
    {
        Initials = Check.Length(initials, nameof(initials), 30);
        SearchName = CodeTableEntity.NormalizeCode(Check.Length(searchName, nameof(searchName), 250));
        Address2 = Check.Length(address2, nameof(address2), 50);
        County = Check.Length(county, nameof(county), 30);
        Gender = gender;
        Extension = Check.Length(extension, nameof(extension), 30);
        FaxNo = Check.Length(faxNo, nameof(faxNo), 30);
        Pager = Check.Length(pager, nameof(pager), 30);
        ManagerNo = CodeTableEntity.NormalizeCode(Check.Length(managerNo, nameof(managerNo), 20));
        StatisticsGroupCode = CodeTableEntity.NormalizeCode(Check.Length(statisticsGroupCode, nameof(statisticsGroupCode), 10));
        CauseOfInactivityCode = CodeTableEntity.NormalizeCode(Check.Length(causeOfInactivityCode, nameof(causeOfInactivityCode), 10));
        GlobalDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension1Code, nameof(globalDimension1Code), 20));
        GlobalDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension2Code, nameof(globalDimension2Code), 20));
        AltAddressCode = CodeTableEntity.NormalizeCode(Check.Length(altAddressCode, nameof(altAddressCode), 10));
        AltAddressStartDate = altAddressStartDate?.Date;
        AltAddressEndDate = altAddressEndDate?.Date;
        BankBranchNo = Check.Length(bankBranchNo, nameof(bankBranchNo), 20);
        SwiftCode = CodeTableEntity.NormalizeCode(Check.Length(swiftCode, nameof(swiftCode), 20));
        CurrencyCode = CodeTableEntity.NormalizeCode(Check.Length(currencyCode, nameof(currencyCode), 10));
        ApplicationMethod = applicationMethod;
        UnionMembershipNo = Check.Length(unionMembershipNo, nameof(unionMembershipNo), 30);
        PrivacyBlocked = privacyBlocked;
    }
}

/// <summary>
/// Employee Absence: a period an employee was away, why, and
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
        // One day per calendar day when no quantity is given.
        Quantity = quantity > 0 ? quantity : ((ToDate ?? FromDate) - FromDate).Days + 1;
        UnitOfMeasureCode = CodeTableEntity.NormalizeCode(Check.Length(unitOfMeasureCode, nameof(unitOfMeasureCode), ErpDomainConsts.MaxUnitOfMeasureCodeLength));
    }
}
