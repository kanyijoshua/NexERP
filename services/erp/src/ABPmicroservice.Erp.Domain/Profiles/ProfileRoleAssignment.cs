using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace ABPmicroservice.Erp.Profiles;

/// <summary>
/// The profile everyone in a role works as, unless they picked one themselves. Business Central
/// sets a profile per user; this lets an administrator set it once per role instead.
/// </summary>
public class ProfileRoleAssignment : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string RoleName { get; private set; }

    public string ProfileId { get; private set; }

    protected ProfileRoleAssignment() { }

    public ProfileRoleAssignment(Guid id, Guid? tenantId, string roleName, string profileId)
        : base(id)
    {
        TenantId = tenantId;
        RoleName = Check.NotNullOrWhiteSpace(roleName, nameof(roleName), ErpDomainConsts.MaxUserNameLength);
        SetProfileId(profileId);
    }

    public void SetProfileId(string profileId)
    {
        ProfileId = Check.NotNullOrWhiteSpace(profileId, nameof(profileId), ErpDomainConsts.MaxProfileIdLength);
    }
}
