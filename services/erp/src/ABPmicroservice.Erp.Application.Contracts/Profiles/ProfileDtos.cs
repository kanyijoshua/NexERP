using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Profiles;

/// <summary>Where the current user's profile came from.</summary>
public enum ProfileSource
{
    /// <summary>Nothing chose one: the default profile.</summary>
    Default = 0,

    /// <summary>One of the user's roles is assigned a profile.</summary>
    Role = 1,

    /// <summary>The user picked it.</summary>
    User = 2,
}

/// <summary>A profile a user can work as.</summary>
public class ProfileDto
{
    public string Id { get; set; }

    /// <summary>Already localized.</summary>
    public string DisplayName { get; set; }

    /// <summary>Already localized.</summary>
    public string Description { get; set; }
}

/// <summary>
/// One entry of a role center's navigation bar: a link, or a menu of links when it has children.
/// </summary>
public class RoleCenterNavItemDto
{
    public string Key { get; set; }

    /// <summary>Already localized.</summary>
    public string DisplayName { get; set; }

    /// <summary>Set on links; empty on menus.</summary>
    public string Route { get; set; }

    public List<RoleCenterNavItemDto> Children { get; set; } = [];
}

/// <summary>The current user's profile and the navigation it gives them.</summary>
public class MyProfileDto
{
    public string ProfileId { get; set; }

    public string DisplayName { get; set; }

    public ProfileSource Source { get; set; }

    /// <summary>The role the profile came from, when <see cref="Source"/> is <see cref="ProfileSource.Role"/>.</summary>
    public string RoleName { get; set; }

    /// <summary>
    /// Only the links this user may open in this company: each is checked against its permission
    /// and its module, and a menu left with no links is dropped.
    /// </summary>
    public List<RoleCenterNavItemDto> Navigation { get; set; } = [];

    /// <summary>Every profile, for the switcher.</summary>
    public List<ProfileDto> Profiles { get; set; } = [];
}

public class SetMyProfileInput
{
    /// <summary>Blank goes back to the profile the user's roles give them.</summary>
    [StringLength(ErpDomainConsts.MaxProfileIdLength)]
    public string ProfileId { get; set; }
}

public class ProfileRoleAssignmentDto
{
    public string RoleName { get; set; }

    public string ProfileId { get; set; }
}

public class SetProfileRoleAssignmentInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxUserNameLength)]
    public string RoleName { get; set; }

    /// <summary>Blank removes the role's assignment.</summary>
    [StringLength(ErpDomainConsts.MaxProfileIdLength)]
    public string ProfileId { get; set; }
}

/// <summary>
/// Profiles (roles) and the role center navigation they give. Each user has a
/// profile that decides their role center; it can also come from the user's roles.
/// </summary>
public interface IProfileAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/profile/my.</summary>
    Task<MyProfileDto> GetMyAsync();

    /// <summary>Routed as POST /api/erp/profile/set-my.</summary>
    Task<MyProfileDto> SetMyAsync(SetMyProfileInput input);

    Task<ListResultDto<ProfileDto>> GetProfilesAsync();

    Task<ListResultDto<ProfileRoleAssignmentDto>> GetRoleAssignmentsAsync();

    /// <summary>Routed as POST /api/erp/profile/set-role-assignment.</summary>
    Task SetRoleAssignmentAsync(SetProfileRoleAssignmentInput input);
}
