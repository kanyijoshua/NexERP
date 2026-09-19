using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace ABPmicroservice.Erp.Companies;

/// <summary>
/// Legal Entity Company. Mirrors Business Central Table 2000000006 "Company".
/// Scoped by Multi-Tenancy.
/// </summary>
public class Company : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public bool EvaluationCompany { get; private set; }

    /// <summary>Company used when a request names none (one per tenant).</summary>
    public bool IsDefault { get; private set; }

    protected Company() { }

    public Company(
        Guid id,
        string name,
        string displayName,
        Guid? tenantId = null,
        bool evaluationCompany = false,
        bool isDefault = false
    )
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName), ErpDomainConsts.MaxNameLength);
        TenantId = tenantId;
        EvaluationCompany = evaluationCompany;
        IsDefault = isDefault;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }
}

/// <summary>
/// Company Information. Mirrors Business Central Table 79 "Company Information".
/// Stores official registration numbers, bank details, and addresses.
/// </summary>
public class CompanyInformation : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }
    public string PhoneNo { get; private set; }
    public string VatRegistrationNo { get; private set; }
    public string BankName { get; private set; }
    public string BankBranchNo { get; private set; }
    public string BankAccountNo { get; private set; }
    public string Iban { get; private set; }

    protected CompanyInformation() { }

    public CompanyInformation(
        Guid id,
        Guid companyId,
        string name,
        string address = null,
        string city = null,
        string vatRegistrationNo = null,
        Guid? tenantId = null
    )
        : base(id)
    {
        CompanyId = companyId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        Address = Check.Length(address, nameof(address), ErpDomainConsts.MaxAddressLength);
        City = Check.Length(city, nameof(city), ErpDomainConsts.MaxCityLength);
        VatRegistrationNo = vatRegistrationNo;
        TenantId = tenantId;
    }
}
