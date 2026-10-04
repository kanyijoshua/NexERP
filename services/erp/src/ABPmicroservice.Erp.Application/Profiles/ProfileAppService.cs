using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Modules;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Profiles;

/// <summary>
/// Which profile the current user works as, and the role center navigation it gives them.
/// <para>
/// The profile is the one the user picked; failing that, the one assigned to the first of their
/// roles that has one (in the order the profiles are listed); failing that, the default. Every
/// link is then checked against its permission and its module, so the bar never offers a page
/// the user cannot open — choosing a profile changes what is shown, never what is allowed.
/// </para>
/// </summary>
[Authorize]
public class ProfileAppService : ErpAppService, IProfileAppService
{
    private readonly IRepository<UserProfile, Guid> _userProfileRepository;
    private readonly IRepository<ProfileRoleAssignment, Guid> _roleAssignmentRepository;
    private readonly ErpModuleManager _moduleManager;

    public ProfileAppService(
        IRepository<UserProfile, Guid> userProfileRepository,
        IRepository<ProfileRoleAssignment, Guid> roleAssignmentRepository,
        ErpModuleManager moduleManager
    )
    {
        _userProfileRepository = userProfileRepository;
        _roleAssignmentRepository = roleAssignmentRepository;
        _moduleManager = moduleManager;
    }

    public async Task<MyProfileDto> GetMyAsync()
    {
        var (profileId, source, roleName) = await ResolveAsync();
        var definition = RoleCenterCatalog.Find(profileId) ?? RoleCenterCatalog.Find(ErpProfileIds.Default);

        return new MyProfileDto
        {
            ProfileId = definition.ProfileId,
            DisplayName = L[definition.NameKey],
            Source = source,
            RoleName = roleName,
            Navigation = await BuildNavigationAsync(definition),
            Profiles = ToDtos(),
        };
    }

    public async Task<MyProfileDto> SetMyAsync(SetMyProfileInput input)
    {
        var userId = CurrentUser.GetId();
        var profileId = input.ProfileId?.Trim();
        var existing = await _userProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profileId.IsNullOrEmpty())
        {
            // Back to what the user's roles give them.
            if (existing != null)
            {
                await _userProfileRepository.DeleteAsync(existing, autoSave: true);
            }
        }
        else
        {
            EnsureExists(profileId);

            if (existing == null)
            {
                await _userProfileRepository.InsertAsync(
                    new UserProfile(GuidGenerator.Create(), CurrentTenant.Id, userId, profileId),
                    autoSave: true
                );
            }
            else
            {
                existing.SetProfileId(profileId);
                await _userProfileRepository.UpdateAsync(existing, autoSave: true);
            }
        }

        return await GetMyAsync();
    }

    public Task<ListResultDto<ProfileDto>> GetProfilesAsync()
    {
        return Task.FromResult(new ListResultDto<ProfileDto>(ToDtos()));
    }

    [Authorize(ErpPermissions.Profiles.Default)]
    public async Task<ListResultDto<ProfileRoleAssignmentDto>> GetRoleAssignmentsAsync()
    {
        var assignments = (await _roleAssignmentRepository.GetListAsync())
            .OrderBy(a => a.RoleName)
            .Select(a => new ProfileRoleAssignmentDto { RoleName = a.RoleName, ProfileId = a.ProfileId })
            .ToList();

        return new ListResultDto<ProfileRoleAssignmentDto>(assignments);
    }

    [Authorize(ErpPermissions.Profiles.Default)]
    public async Task SetRoleAssignmentAsync(SetProfileRoleAssignmentInput input)
    {
        var roleName = input.RoleName.Trim();
        var profileId = input.ProfileId?.Trim();
        var existing = await _roleAssignmentRepository.FirstOrDefaultAsync(a => a.RoleName == roleName);

        if (profileId.IsNullOrEmpty())
        {
            if (existing != null)
            {
                await _roleAssignmentRepository.DeleteAsync(existing, autoSave: true);
            }

            return;
        }

        EnsureExists(profileId);

        if (existing == null)
        {
            await _roleAssignmentRepository.InsertAsync(
                new ProfileRoleAssignment(GuidGenerator.Create(), CurrentTenant.Id, roleName, profileId),
                autoSave: true
            );
        }
        else
        {
            existing.SetProfileId(profileId);
            await _roleAssignmentRepository.UpdateAsync(existing, autoSave: true);
        }
    }

    private async Task<(string ProfileId, ProfileSource Source, string RoleName)> ResolveAsync()
    {
        var userId = CurrentUser.Id;

        if (userId.HasValue)
        {
            var own = await _userProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId.Value);

            // A profile that has since been dropped from the catalog falls through to the roles.
            if (own != null && RoleCenterCatalog.Exists(own.ProfileId))
            {
                return (own.ProfileId, ProfileSource.User, null);
            }
        }

        var roles = CurrentUser.Roles ?? [];
        if (roles.Length > 0)
        {
            var assignments = await _roleAssignmentRepository.GetListAsync(a => roles.Contains(a.RoleName));

            // Several roles with a profile each: the one listed first wins, so the outcome does
            // not depend on the order the roles were granted in.
            var match = RoleCenterCatalog.All
                .Select(d => assignments.Where(a => a.ProfileId == d.ProfileId).OrderBy(a => a.RoleName).FirstOrDefault())
                .FirstOrDefault(a => a != null);

            if (match != null)
            {
                return (match.ProfileId, ProfileSource.Role, match.RoleName);
            }
        }

        return (ErpProfileIds.Default, ProfileSource.Default, null);
    }

    private async Task<List<RoleCenterNavItemDto>> BuildNavigationAsync(RoleCenterDefinition definition)
    {
        // Each link is checked once even when several menus list it.
        var allowed = new Dictionary<RoleCenterLink, bool>();

        // The module states are read once for the whole menu rather than once per link.
        var states = await _moduleManager.GetStatesAsync();

        async Task<bool> CanOpenAsync(RoleCenterLink link)
        {
            if (!allowed.TryGetValue(link, out var can))
            {
                can = ErpModuleManager.IsEnabledIn(link.Module, states);
                foreach (var permission in link.Permissions)
                {
                    can = can && await AuthorizationService.IsGrantedAsync(permission);
                }

                allowed[link] = can;
            }

            return can;
        }

        var items = new List<RoleCenterNavItemDto>();

        foreach (var menu in definition.Navigation)
        {
            if (menu.Link != null)
            {
                if (await CanOpenAsync(menu.Link))
                {
                    items.Add(ToItem(menu.Key, menu.Link));
                }

                continue;
            }

            var children = new List<RoleCenterNavItemDto>();
            foreach (var link in menu.Links)
            {
                if (await CanOpenAsync(link))
                {
                    children.Add(ToItem(link.Route, link));
                }
            }

            // A menu with nothing the user may open is not shown at all.
            if (children.Count > 0)
            {
                items.Add(new RoleCenterNavItemDto { Key = menu.Key, DisplayName = L[menu.NameKey], Children = children });
            }
        }

        return items;
    }

    private RoleCenterNavItemDto ToItem(string key, RoleCenterLink link) =>
        new() { Key = key, DisplayName = L[link.NameKey], Route = link.Route };

    private List<ProfileDto> ToDtos() =>
        RoleCenterCatalog.All
            .Select(d => new ProfileDto { Id = d.ProfileId, DisplayName = L[d.NameKey], Description = L[d.DescriptionKey] })
            .ToList();

    private static void EnsureExists(string profileId)
    {
        if (!RoleCenterCatalog.Exists(profileId))
        {
            throw new BusinessException(ErpErrorCodes.Profiles.UnknownProfile).WithData("profileId", profileId);
        }
    }
}
