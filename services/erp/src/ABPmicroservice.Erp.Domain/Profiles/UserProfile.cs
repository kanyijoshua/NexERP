using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Profiles;

/// <summary>
/// User Personalization Profile. Mirrors Business Central Table 2000000073 "User Personalization".
/// </summary>
public class UserProfile : FullAuditedEntity<Guid>
{
    public Guid UserId { get; private set; }
    public string ProfileId { get; private set; } // e.g., "BUSINESS_MANAGER", "ACCOUNTANT", "SALES_ORDER_PROCESSOR"
    public string LanguageCode { get; private set; }
    public string CompanyName { get; private set; }

    protected UserProfile() { }

    public UserProfile(Guid id, Guid userId, string profileId, string languageCode = "en-US", string companyName = "CRONUS International Ltd.")
        : base(id)
    {
        UserId = userId;
        SetProfileId(profileId);
        LanguageCode = languageCode;
        CompanyName = companyName;
    }

    public void SetProfileId(string profileId)
    {
        ProfileId = Check.NotNullOrWhiteSpace(profileId, nameof(profileId), ErpDomainConsts.MaxProfileIdLength);
    }
}

/// <summary>
/// Role Center Dashboard Metadata.
/// </summary>
public class UserRoleCenter : FullAuditedEntity<Guid>
{
    public string ProfileId { get; private set; }
    public string RoleCenterName { get; private set; }
    public string DefaultDashboardLayoutJson { get; private set; }

    protected UserRoleCenter() { }

    public UserRoleCenter(Guid id, string profileId, string roleCenterName, string defaultDashboardLayoutJson = "{}")
        : base(id)
    {
        ProfileId = Check.NotNullOrWhiteSpace(profileId, nameof(profileId), ErpDomainConsts.MaxProfileIdLength);
        RoleCenterName = Check.NotNullOrWhiteSpace(roleCenterName, nameof(roleCenterName), ErpDomainConsts.MaxNameLength);
        DefaultDashboardLayoutJson = defaultDashboardLayoutJson;
    }
}
