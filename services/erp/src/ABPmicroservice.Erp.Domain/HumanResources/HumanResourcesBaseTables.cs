using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>Relative.</summary>
public class Relative : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    protected Relative() { }

    public Relative(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>Misc. Article.</summary>
public class MiscArticle : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    protected MiscArticle() { }

    public MiscArticle(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>Confidential.</summary>
public class Confidential : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    protected Confidential() { }

    public Confidential(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>Employee Statistics Group.</summary>
public class EmployeeStatisticsGroup : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    protected EmployeeStatisticsGroup() { }

    public EmployeeStatisticsGroup(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>Employee Relative.</summary>
public class EmployeeRelative : CompanyEntity
{
    public string EmployeeNo { get; private set; }
    public int LineNo { get; private set; }

    public string RelativeCode { get; private set; }
    public string FirstName { get; private set; }
    public string MiddleName { get; private set; }
    public string LastName { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public string PhoneNo { get; private set; }
    public string RelativesEmployeeNo { get; private set; }

    protected EmployeeRelative() { }

    public EmployeeRelative(Guid id, string employeeNo, int lineNo)
        : base(id)
    {
        SetKey(employeeNo, lineNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string employeeNo, int lineNo)
    {
        EmployeeNo = Check.NotNullOrWhiteSpace(employeeNo, nameof(employeeNo), 20).Trim().ToUpperInvariant();
        LineNo = lineNo;
    }

    public void Set(
        string relativeCode,
        string firstName,
        string middleName,
        string lastName,
        DateTime? birthDate,
        string phoneNo,
        string relativesEmployeeNo
    )
    {
        RelativeCode = CodeTableEntity.NormalizeCode(Check.Length(relativeCode, nameof(relativeCode), 10));
        FirstName = Check.Length(firstName, nameof(firstName), 30);
        MiddleName = Check.Length(middleName, nameof(middleName), 30);
        LastName = Check.Length(lastName, nameof(lastName), 30);
        BirthDate = birthDate?.Date;
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), 30);
        RelativesEmployeeNo = CodeTableEntity.NormalizeCode(Check.Length(relativesEmployeeNo, nameof(relativesEmployeeNo), 20));
    }
}

/// <summary>Employee Qualification.</summary>
public class EmployeeQualification : CompanyEntity
{
    public string EmployeeNo { get; private set; }
    public int LineNo { get; private set; }

    public string QualificationCode { get; private set; }
    public DateTime? FromDate { get; private set; }
    public DateTime? ToDate { get; private set; }
    public EmployeeQualificationType Type { get; private set; }
    public string Description { get; private set; }
    public string InstitutionCompany { get; private set; }
    public decimal Cost { get; private set; }
    public string CourseGrade { get; private set; }
    public EmployeeStatus EmployeeStatus { get; private set; }
    public DateTime? ExpirationDate { get; private set; }

    protected EmployeeQualification() { }

    public EmployeeQualification(Guid id, string employeeNo, int lineNo)
        : base(id)
    {
        SetKey(employeeNo, lineNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string employeeNo, int lineNo)
    {
        EmployeeNo = Check.NotNullOrWhiteSpace(employeeNo, nameof(employeeNo), 20).Trim().ToUpperInvariant();
        LineNo = lineNo;
    }

    public void Set(
        string qualificationCode,
        DateTime? fromDate,
        DateTime? toDate,
        EmployeeQualificationType type,
        string description,
        string institutionCompany,
        decimal cost,
        string courseGrade,
        EmployeeStatus employeeStatus,
        DateTime? expirationDate
    )
    {
        QualificationCode = CodeTableEntity.NormalizeCode(Check.Length(qualificationCode, nameof(qualificationCode), 10));
        FromDate = fromDate?.Date;
        ToDate = toDate?.Date;
        Type = type;
        Description = Check.Length(description, nameof(description), 100);
        InstitutionCompany = Check.Length(institutionCompany, nameof(institutionCompany), 100);
        Cost = cost;
        CourseGrade = Check.Length(courseGrade, nameof(courseGrade), 50);
        EmployeeStatus = employeeStatus;
        ExpirationDate = expirationDate?.Date;
    }
}

/// <summary>Misc. Article Information.</summary>
public class MiscArticleInformation : CompanyEntity
{
    public string EmployeeNo { get; private set; }
    public string MiscArticleCode { get; private set; }
    public int LineNo { get; private set; }

    public string Description { get; private set; }
    public DateTime? FromDate { get; private set; }
    public DateTime? ToDate { get; private set; }
    public bool InUse { get; private set; }
    public string SerialNo { get; private set; }

    protected MiscArticleInformation() { }

    public MiscArticleInformation(Guid id, string employeeNo, string miscArticleCode, int lineNo)
        : base(id)
    {
        SetKey(employeeNo, miscArticleCode, lineNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string employeeNo, string miscArticleCode, int lineNo)
    {
        EmployeeNo = Check.NotNullOrWhiteSpace(employeeNo, nameof(employeeNo), 20).Trim().ToUpperInvariant();
        MiscArticleCode = Check.NotNullOrWhiteSpace(miscArticleCode, nameof(miscArticleCode), 10).Trim().ToUpperInvariant();
        LineNo = lineNo;
    }

    public void Set(
        string description,
        DateTime? fromDate,
        DateTime? toDate,
        bool inUse,
        string serialNo
    )
    {
        Description = Check.Length(description, nameof(description), 100);
        FromDate = fromDate?.Date;
        ToDate = toDate?.Date;
        InUse = inUse;
        SerialNo = Check.Length(serialNo, nameof(serialNo), 50);
    }
}

/// <summary>Confidential Information.</summary>
public class ConfidentialInformation : CompanyEntity
{
    public string EmployeeNo { get; private set; }
    public string ConfidentialCode { get; private set; }
    public int LineNo { get; private set; }

    public string Description { get; private set; }

    protected ConfidentialInformation() { }

    public ConfidentialInformation(Guid id, string employeeNo, string confidentialCode, int lineNo)
        : base(id)
    {
        SetKey(employeeNo, confidentialCode, lineNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string employeeNo, string confidentialCode, int lineNo)
    {
        EmployeeNo = Check.NotNullOrWhiteSpace(employeeNo, nameof(employeeNo), 20).Trim().ToUpperInvariant();
        ConfidentialCode = Check.NotNullOrWhiteSpace(confidentialCode, nameof(confidentialCode), 10).Trim().ToUpperInvariant();
        LineNo = lineNo;
    }

    public void Set(string description)
    {
        Description = Check.Length(description, nameof(description), 100);
    }
}

/// <summary>Alternative Address.</summary>
public class AlternativeAddress : CompanyEntity
{
    public string EmployeeNo { get; private set; }
    public string Code { get; private set; }

    public string Name { get; private set; }
    public string Name2 { get; private set; }
    public string Address { get; private set; }
    public string Address2 { get; private set; }
    public string City { get; private set; }
    public string PostCode { get; private set; }
    public string County { get; private set; }
    public string PhoneNo { get; private set; }
    public string FaxNo { get; private set; }
    public string Email { get; private set; }
    public string CountryRegionCode { get; private set; }

    protected AlternativeAddress() { }

    public AlternativeAddress(Guid id, string employeeNo, string code)
        : base(id)
    {
        SetKey(employeeNo, code);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string employeeNo, string code)
    {
        EmployeeNo = Check.NotNullOrWhiteSpace(employeeNo, nameof(employeeNo), 20).Trim().ToUpperInvariant();
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), 10).Trim().ToUpperInvariant();
    }

    public void Set(
        string name,
        string name2,
        string address,
        string address2,
        string city,
        string postCode,
        string county,
        string phoneNo,
        string faxNo,
        string email,
        string countryRegionCode
    )
    {
        Name = Check.Length(name, nameof(name), 100);
        Name2 = Check.Length(name2, nameof(name2), 50);
        Address = Check.Length(address, nameof(address), 100);
        Address2 = Check.Length(address2, nameof(address2), 50);
        City = Check.Length(city, nameof(city), 30);
        PostCode = CodeTableEntity.NormalizeCode(Check.Length(postCode, nameof(postCode), 20));
        County = Check.Length(county, nameof(county), 30);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), 30);
        FaxNo = Check.Length(faxNo, nameof(faxNo), 30);
        Email = Check.Length(email, nameof(email), 80);
        CountryRegionCode = CodeTableEntity.NormalizeCode(Check.Length(countryRegionCode, nameof(countryRegionCode), 10));
    }
}

/// <summary>Human Resource Comment Line.</summary>
public class HumanResourceCommentLine : CompanyEntity
{
    public HumanResourcesCommentTableName TableName { get; private set; }
    public string No { get; private set; }
    public int TableLineNo { get; private set; }
    public int LineNo { get; private set; }

    public string AlternativeAddressCode { get; private set; }
    public DateTime? Date { get; private set; }
    public string Code { get; private set; }
    public string Comment { get; private set; }

    protected HumanResourceCommentLine() { }

    public HumanResourceCommentLine(Guid id, HumanResourcesCommentTableName tableName, string no, int tableLineNo, int lineNo)
        : base(id)
    {
        SetKey(tableName, no, tableLineNo, lineNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(
        HumanResourcesCommentTableName tableName,
        string no,
        int tableLineNo,
        int lineNo
    )
    {
        TableName = tableName;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), 20).Trim().ToUpperInvariant();
        TableLineNo = tableLineNo;
        LineNo = lineNo;
    }

    public void Set(
        string alternativeAddressCode,
        DateTime? date,
        string code,
        string comment
    )
    {
        AlternativeAddressCode = CodeTableEntity.NormalizeCode(Check.Length(alternativeAddressCode, nameof(alternativeAddressCode), 10));
        Date = date?.Date;
        Code = CodeTableEntity.NormalizeCode(Check.Length(code, nameof(code), 10));
        Comment = Check.Length(comment, nameof(comment), 80);
    }
}
