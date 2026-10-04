using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Finance;

/// <summary>Source Code.</summary>
public class SourceCode : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    protected SourceCode() { }

    public SourceCode(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>Reason Code.</summary>
public class ReasonCode : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    protected ReasonCode() { }

    public ReasonCode(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>Country/Region.</summary>
public class CountryRegion : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    public string IsoCode { get; private set; }
    public string IsoNumericCode { get; private set; }
    public string EuCountryRegionCode { get; private set; }
    public string IntrastatCode { get; private set; }
    public CountryRegionAddressFormat AddressFormat { get; private set; }
    public CountryRegionContactAddressFormat ContactAddressFormat { get; private set; }
    public string VatScheme { get; private set; }
    public string CountyName { get; private set; }

    protected CountryRegion() { }

    public CountryRegion(Guid id, string code, string description = null)
        : base(id, code, description) { }

    public void Set(
        string isoCode,
        string isoNumericCode,
        string euCountryRegionCode,
        string intrastatCode,
        CountryRegionAddressFormat addressFormat,
        CountryRegionContactAddressFormat contactAddressFormat,
        string vatScheme,
        string countyName
    )
    {
        IsoCode = CodeTableEntity.NormalizeCode(Check.Length(isoCode, nameof(isoCode), 2));
        IsoNumericCode = CodeTableEntity.NormalizeCode(Check.Length(isoNumericCode, nameof(isoNumericCode), 3));
        EuCountryRegionCode = CodeTableEntity.NormalizeCode(Check.Length(euCountryRegionCode, nameof(euCountryRegionCode), 10));
        IntrastatCode = CodeTableEntity.NormalizeCode(Check.Length(intrastatCode, nameof(intrastatCode), 10));
        AddressFormat = addressFormat;
        ContactAddressFormat = contactAddressFormat;
        VatScheme = CodeTableEntity.NormalizeCode(Check.Length(vatScheme, nameof(vatScheme), 10));
        CountyName = Check.Length(countyName, nameof(countyName), 30);
    }
}

/// <summary>Post Code.</summary>
public class PostCode : CompanyEntity
{
    public string Code { get; private set; }
    public string City { get; private set; }

    public string SearchCity { get; private set; }
    public string CountryRegionCode { get; private set; }
    public string County { get; private set; }

    protected PostCode() { }

    public PostCode(Guid id, string code, string city)
        : base(id)
    {
        SetKey(code, city);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string code, string city)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), 20).Trim().ToUpperInvariant();
        City = Check.NotNullOrWhiteSpace(city, nameof(city), 30);
    }

    public void Set(string searchCity, string countryRegionCode, string county)
    {
        SearchCity = CodeTableEntity.NormalizeCode(Check.Length(searchCity, nameof(searchCity), 30));
        CountryRegionCode = CodeTableEntity.NormalizeCode(Check.Length(countryRegionCode, nameof(countryRegionCode), 10));
        County = Check.Length(county, nameof(county), 30);
    }
}

/// <summary>Shipment Method.</summary>
public class ShipmentMethod : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    protected ShipmentMethod() { }

    public ShipmentMethod(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>Responsibility Center.</summary>
public class ResponsibilityCenter : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    public string Address { get; private set; }
    public string Address2 { get; private set; }
    public string City { get; private set; }
    public string PostCode { get; private set; }
    public string CountryRegionCode { get; private set; }
    public string PhoneNo { get; private set; }
    public string FaxNo { get; private set; }
    public string Name2 { get; private set; }
    public string Contact { get; private set; }
    public string GlobalDimension1Code { get; private set; }
    public string GlobalDimension2Code { get; private set; }
    public string LocationCode { get; private set; }
    public string County { get; private set; }
    public string Email { get; private set; }

    protected ResponsibilityCenter() { }

    public ResponsibilityCenter(Guid id, string code, string description = null)
        : base(id, code, description) { }

    public void Set(
        string address,
        string address2,
        string city,
        string postCode,
        string countryRegionCode,
        string phoneNo,
        string faxNo,
        string name2,
        string contact,
        string globalDimension1Code,
        string globalDimension2Code,
        string locationCode,
        string county,
        string email
    )
    {
        Address = Check.Length(address, nameof(address), 100);
        Address2 = Check.Length(address2, nameof(address2), 50);
        City = Check.Length(city, nameof(city), 30);
        PostCode = CodeTableEntity.NormalizeCode(Check.Length(postCode, nameof(postCode), 20));
        CountryRegionCode = CodeTableEntity.NormalizeCode(Check.Length(countryRegionCode, nameof(countryRegionCode), 10));
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), 30);
        FaxNo = Check.Length(faxNo, nameof(faxNo), 30);
        Name2 = Check.Length(name2, nameof(name2), 50);
        Contact = Check.Length(contact, nameof(contact), 100);
        GlobalDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension1Code, nameof(globalDimension1Code), 20));
        GlobalDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension2Code, nameof(globalDimension2Code), 20));
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), 10));
        County = Check.Length(county, nameof(county), 30);
        Email = Check.Length(email, nameof(email), 80);
    }
}

/// <summary>User Setup.</summary>
public class UserSetup : CompanyEntity
{
    public string UserId { get; private set; }

    public DateTime? AllowPostingFrom { get; private set; }
    public DateTime? AllowPostingTo { get; private set; }
    public bool RegisterTime { get; private set; }
    public DateTime? AllowDeferralPostingFrom { get; private set; }
    public DateTime? AllowDeferralPostingTo { get; private set; }
    public string SalespersPurchCode { get; private set; }
    public string ApproverId { get; private set; }
    public int SalesAmountApprovalLimit { get; private set; }
    public int PurchaseAmountApprovalLimit { get; private set; }
    public bool UnlimitedSalesApproval { get; private set; }
    public bool UnlimitedPurchaseApproval { get; private set; }
    public string Substitute { get; private set; }
    public string Email { get; private set; }
    public string PhoneNo { get; private set; }
    public int RequestAmountApprovalLimit { get; private set; }
    public bool UnlimitedRequestApproval { get; private set; }
    public bool ApprovalAdministrator { get; private set; }
    public DateTime? AllowVatDateFrom { get; private set; }
    public DateTime? AllowVatDateTo { get; private set; }
    public InvoicePostingPolicy SalesInvoicePostingPolicy { get; private set; }
    public InvoicePostingPolicy PurchInvoicePostingPolicy { get; private set; }
    public DateTime? AllowFAPostingFrom { get; private set; }
    public DateTime? AllowFAPostingTo { get; private set; }
    public string SalesRespCtrFilter { get; private set; }
    public string PurchaseRespCtrFilter { get; private set; }

    protected UserSetup() { }

    public UserSetup(Guid id, string userId)
        : base(id)
    {
        SetKey(userId);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string userId)
    {
        UserId = Check.NotNullOrWhiteSpace(userId, nameof(userId), 50).Trim().ToUpperInvariant();
    }

    public void Set(
        DateTime? allowPostingFrom,
        DateTime? allowPostingTo,
        bool registerTime,
        DateTime? allowDeferralPostingFrom,
        DateTime? allowDeferralPostingTo,
        string salespersPurchCode,
        string approverId,
        int salesAmountApprovalLimit,
        int purchaseAmountApprovalLimit,
        bool unlimitedSalesApproval,
        bool unlimitedPurchaseApproval,
        string substitute,
        string email,
        string phoneNo,
        int requestAmountApprovalLimit,
        bool unlimitedRequestApproval,
        bool approvalAdministrator,
        DateTime? allowVatDateFrom,
        DateTime? allowVatDateTo,
        InvoicePostingPolicy salesInvoicePostingPolicy,
        InvoicePostingPolicy purchInvoicePostingPolicy,
        DateTime? allowFAPostingFrom,
        DateTime? allowFAPostingTo,
        string salesRespCtrFilter,
        string purchaseRespCtrFilter
    )
    {
        AllowPostingFrom = allowPostingFrom?.Date;
        AllowPostingTo = allowPostingTo?.Date;
        RegisterTime = registerTime;
        AllowDeferralPostingFrom = allowDeferralPostingFrom?.Date;
        AllowDeferralPostingTo = allowDeferralPostingTo?.Date;
        SalespersPurchCode = CodeTableEntity.NormalizeCode(Check.Length(salespersPurchCode, nameof(salespersPurchCode), 20));
        ApproverId = CodeTableEntity.NormalizeCode(Check.Length(approverId, nameof(approverId), 50));
        SalesAmountApprovalLimit = salesAmountApprovalLimit;
        PurchaseAmountApprovalLimit = purchaseAmountApprovalLimit;
        UnlimitedSalesApproval = unlimitedSalesApproval;
        UnlimitedPurchaseApproval = unlimitedPurchaseApproval;
        Substitute = CodeTableEntity.NormalizeCode(Check.Length(substitute, nameof(substitute), 50));
        Email = Check.Length(email, nameof(email), 100);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), 30);
        RequestAmountApprovalLimit = requestAmountApprovalLimit;
        UnlimitedRequestApproval = unlimitedRequestApproval;
        ApprovalAdministrator = approvalAdministrator;
        AllowVatDateFrom = allowVatDateFrom?.Date;
        AllowVatDateTo = allowVatDateTo?.Date;
        SalesInvoicePostingPolicy = salesInvoicePostingPolicy;
        PurchInvoicePostingPolicy = purchInvoicePostingPolicy;
        AllowFAPostingFrom = allowFAPostingFrom?.Date;
        AllowFAPostingTo = allowFAPostingTo?.Date;
        SalesRespCtrFilter = CodeTableEntity.NormalizeCode(Check.Length(salesRespCtrFilter, nameof(salesRespCtrFilter), 10));
        PurchaseRespCtrFilter = CodeTableEntity.NormalizeCode(Check.Length(purchaseRespCtrFilter, nameof(purchaseRespCtrFilter), 10));
    }
}

/// <summary>G/L Budget Name.</summary>
public class GLBudgetName : CompanyEntity
{
    public string Name { get; private set; }

    public string Description { get; private set; }
    public bool Blocked { get; private set; }
    public string BudgetDimension1Code { get; private set; }
    public string BudgetDimension2Code { get; private set; }
    public string BudgetDimension3Code { get; private set; }
    public string BudgetDimension4Code { get; private set; }

    protected GLBudgetName() { }

    public GLBudgetName(Guid id, string name)
        : base(id)
    {
        SetKey(name);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 10).Trim().ToUpperInvariant();
    }

    public void Set(
        string description,
        bool blocked,
        string budgetDimension1Code,
        string budgetDimension2Code,
        string budgetDimension3Code,
        string budgetDimension4Code
    )
    {
        Description = Check.Length(description, nameof(description), 80);
        Blocked = blocked;
        BudgetDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(budgetDimension1Code, nameof(budgetDimension1Code), 20));
        BudgetDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(budgetDimension2Code, nameof(budgetDimension2Code), 20));
        BudgetDimension3Code = CodeTableEntity.NormalizeCode(Check.Length(budgetDimension3Code, nameof(budgetDimension3Code), 20));
        BudgetDimension4Code = CodeTableEntity.NormalizeCode(Check.Length(budgetDimension4Code, nameof(budgetDimension4Code), 20));
    }
}

/// <summary>G/L Budget Entry.</summary>
public class GLBudgetEntry : CompanyEntity
{
    public long EntryNo { get; private set; }

    public string BudgetName { get; private set; }
    public string GLAccountNo { get; private set; }
    public DateTime? Date { get; private set; }
    public string GlobalDimension1Code { get; private set; }
    public string GlobalDimension2Code { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public string BusinessUnitCode { get; private set; }
    public string BudgetDimension1Code { get; private set; }
    public string BudgetDimension2Code { get; private set; }
    public string BudgetDimension3Code { get; private set; }
    public string BudgetDimension4Code { get; private set; }

    protected GLBudgetEntry() { }

    public GLBudgetEntry(Guid id, long entryNo)
        : base(id)
    {
        SetKey(entryNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(long entryNo)
    {
        EntryNo = entryNo;
    }

    public void Set(
        string budgetName,
        string glAccountNo,
        DateTime? date,
        string globalDimension1Code,
        string globalDimension2Code,
        decimal amount,
        string description,
        string businessUnitCode,
        string budgetDimension1Code,
        string budgetDimension2Code,
        string budgetDimension3Code,
        string budgetDimension4Code
    )
    {
        BudgetName = Check.NotNullOrWhiteSpace(budgetName, nameof(budgetName), 10).Trim().ToUpperInvariant();
        GLAccountNo = Check.NotNullOrWhiteSpace(glAccountNo, nameof(glAccountNo), 20).Trim().ToUpperInvariant();
        Date = date?.Date;
        GlobalDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension1Code, nameof(globalDimension1Code), 20));
        GlobalDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension2Code, nameof(globalDimension2Code), 20));
        Amount = amount;
        Description = Check.Length(description, nameof(description), 100);
        BusinessUnitCode = CodeTableEntity.NormalizeCode(Check.Length(businessUnitCode, nameof(businessUnitCode), 20));
        BudgetDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(budgetDimension1Code, nameof(budgetDimension1Code), 20));
        BudgetDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(budgetDimension2Code, nameof(budgetDimension2Code), 20));
        BudgetDimension3Code = CodeTableEntity.NormalizeCode(Check.Length(budgetDimension3Code, nameof(budgetDimension3Code), 20));
        BudgetDimension4Code = CodeTableEntity.NormalizeCode(Check.Length(budgetDimension4Code, nameof(budgetDimension4Code), 20));
    }
}

/// <summary>Comment Line.</summary>
public class CommentLine : CompanyEntity
{
    public CommentLineTableName TableName { get; private set; }
    public string No { get; private set; }
    public int LineNo { get; private set; }

    public DateTime? Date { get; private set; }
    public string Code { get; private set; }
    public string Comment { get; private set; }

    protected CommentLine() { }

    public CommentLine(Guid id, CommentLineTableName tableName, string no, int lineNo)
        : base(id)
    {
        SetKey(tableName, no, lineNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(CommentLineTableName tableName, string no, int lineNo)
    {
        TableName = tableName;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), 20).Trim().ToUpperInvariant();
        LineNo = lineNo;
    }

    public void Set(DateTime? date, string code, string comment)
    {
        Date = date?.Date;
        Code = CodeTableEntity.NormalizeCode(Check.Length(code, nameof(code), 10));
        Comment = Check.Length(comment, nameof(comment), 80);
    }
}
