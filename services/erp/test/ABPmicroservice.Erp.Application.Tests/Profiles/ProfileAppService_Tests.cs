using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Localization;
using ABPmicroservice.Erp.Modules;
using Microsoft.Extensions.Localization;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Security.Claims;
using Xunit;

namespace ABPmicroservice.Erp.Profiles;

public class ProfileAppService_Tests : ErpApplicationTestBase
{
    private readonly IProfileAppService _profiles;
    private readonly IModuleAppService _modules;

    public ProfileAppService_Tests()
    {
        _profiles = GetRequiredService<IProfileAppService>();
        _modules = GetRequiredService<IModuleAppService>();
    }

    private Task<MyProfileDto> MyAsync() => InCompanyAsync(DefaultCompanyName, () => _profiles.GetMyAsync());

    private Task<MyProfileDto> PickAsync(string profileId) =>
        InCompanyAsync(DefaultCompanyName, () => _profiles.SetMyAsync(new SetMyProfileInput { ProfileId = profileId }));

    [Fact]
    public async Task A_User_Who_Chose_Nothing_Works_As_The_Default_Profile()
    {
        var mine = await MyAsync();

        mine.ProfileId.ShouldBe(ErpProfileIds.Default);
        mine.Source.ShouldBe(ProfileSource.Default);
        mine.DisplayName.ShouldBe("Business Manager");
        mine.Profiles.Select(p => p.Id).ShouldContain(ErpProfileIds.Accountant);
    }

    /// <summary>The bar shows names, and every entry either opens a page or holds links that do.</summary>
    [Fact]
    public async Task The_Navigation_Is_Localized_And_Every_Entry_Leads_Somewhere()
    {
        var mine = await MyAsync();

        mine.Navigation.ShouldNotBeEmpty();
        var links = mine.Navigation.SelectMany(n => n.Children.Count > 0 ? n.Children : [n]).ToList();

        mine.Navigation.ShouldAllBe(n => !n.DisplayName.Contains(':'));
        mine.Navigation.Where(n => n.Children.Count > 0).ShouldAllBe(n => string.IsNullOrEmpty(n.Route));
        links.ShouldAllBe(l => l.Route.StartsWith("/erp/") && !l.DisplayName.Contains(':'));
        mine.Navigation.Select(n => n.Key).ShouldContain("finance");
    }

    [Fact]
    public async Task Picking_A_Profile_Changes_The_Navigation_And_Blank_Goes_Back()
    {
        var accountant = await PickAsync(ErpProfileIds.Accountant);

        accountant.ProfileId.ShouldBe(ErpProfileIds.Accountant);
        accountant.Source.ShouldBe(ProfileSource.User);
        accountant.Navigation.Select(n => n.Key).ShouldContain("journals");
        (await MyAsync()).ProfileId.ShouldBe(ErpProfileIds.Accountant);

        var back = await PickAsync(null);

        back.ProfileId.ShouldBe(ErpProfileIds.Default);
        back.Source.ShouldBe(ProfileSource.Default);
    }

    [Fact]
    public async Task A_Role_Gives_Its_Members_A_Profile_But_The_Users_Own_Choice_Wins()
    {
        await _profiles.SetRoleAssignmentAsync(
            new SetProfileRoleAssignmentInput { RoleName = "sales", ProfileId = ErpProfileIds.SalesOrderProcessor }
        );
        (await _profiles.GetRoleAssignmentsAsync()).Items.ShouldContain(a =>
            a.RoleName == "sales" && a.ProfileId == ErpProfileIds.SalesOrderProcessor
        );

        using (SignInAs("clerk", "sales", "reporting"))
        {
            var mine = await MyAsync();

            mine.ProfileId.ShouldBe(ErpProfileIds.SalesOrderProcessor);
            mine.Source.ShouldBe(ProfileSource.Role);
            mine.RoleName.ShouldBe("sales");

            var picked = await PickAsync(ErpProfileIds.PurchasingAgent);
            picked.Source.ShouldBe(ProfileSource.User);
        }

        // Removing the assignment takes the profile away from the role.
        await _profiles.SetRoleAssignmentAsync(new SetProfileRoleAssignmentInput { RoleName = "sales" });
        (await _profiles.GetRoleAssignmentsAsync()).Items.ShouldNotContain(a => a.RoleName == "sales");
    }

    /// <summary>Two roles with a profile each: the profile listed first wins, whatever the role order.</summary>
    [Fact]
    public async Task Between_Two_Roles_The_Profile_Listed_First_Wins()
    {
        await _profiles.SetRoleAssignmentAsync(
            new SetProfileRoleAssignmentInput { RoleName = "buyers", ProfileId = ErpProfileIds.PurchasingAgent }
        );
        await _profiles.SetRoleAssignmentAsync(
            new SetProfileRoleAssignmentInput { RoleName = "accounts", ProfileId = ErpProfileIds.Accountant }
        );

        using (SignInAs("both", "buyers", "accounts"))
        {
            (await MyAsync()).ProfileId.ShouldBe(ErpProfileIds.Accountant);
        }
    }

    [Fact]
    public async Task An_Unknown_Profile_Is_Refused()
    {
        var picked = await Should.ThrowAsync<BusinessException>(() => PickAsync("ASTRONAUT"));
        picked.Code.ShouldBe(ErpErrorCodes.Profiles.UnknownProfile);

        var assigned = await Should.ThrowAsync<BusinessException>(
            () => _profiles.SetRoleAssignmentAsync(new SetProfileRoleAssignmentInput { RoleName = "x", ProfileId = "ASTRONAUT" })
        );
        assigned.Code.ShouldBe(ErpErrorCodes.Profiles.UnknownProfile);
    }

    [Fact]
    public async Task A_Switched_Off_Module_Takes_Its_Links_With_It()
    {
        await PickAsync(ErpProfileIds.HumanResourcesManager);
        (await MyAsync()).Navigation.Select(n => n.Key).ShouldContain("human-resources");

        // Payroll runs on employees, so it goes first.
        await InCompanyAsync(
            DefaultCompanyName,
            async () =>
            {
                await _modules.SetEnabledAsync(new SetModuleEnabledInput { Code = ErpModuleRegistry.Payroll, Enabled = false });
                await _modules.SetEnabledAsync(new SetModuleEnabledInput { Code = ErpModuleRegistry.HumanResources, Enabled = false });
            }
        );

        try
        {
            var mine = await MyAsync();
            mine.Navigation.Select(n => n.Key).ShouldNotContain("human-resources");
            mine.Navigation.SelectMany(n => n.Children).ShouldNotContain(l => l.Route == "/erp/employees");
        }
        finally
        {
            await InCompanyAsync(
                DefaultCompanyName,
                async () =>
                {
                    await _modules.SetEnabledAsync(new SetModuleEnabledInput { Code = ErpModuleRegistry.HumanResources, Enabled = true });
                    await _modules.SetEnabledAsync(new SetModuleEnabledInput { Code = ErpModuleRegistry.Payroll, Enabled = true });
                }
            );
            await PickAsync(null);
        }
    }

    /// <summary>A link to a permission or text that does not exist would hide itself or show a key.</summary>
    [Fact]
    public async Task Every_Link_Names_A_Real_Permission_And_A_Localized_Name()
    {
        var permissions = GetRequiredService<IPermissionDefinitionManager>();
        var l = GetRequiredService<IStringLocalizer<ErpResource>>();

        foreach (var definition in RoleCenterCatalog.All)
        {
            l[definition.NameKey].ResourceNotFound.ShouldBeFalse(definition.NameKey);
            l[definition.DescriptionKey].ResourceNotFound.ShouldBeFalse(definition.DescriptionKey);

            foreach (var menu in definition.Navigation)
            {
                l[menu.NameKey].ResourceNotFound.ShouldBeFalse(menu.NameKey);

                foreach (var link in menu.Link != null ? [menu.Link] : menu.Links)
                {
                    l[link.NameKey].ResourceNotFound.ShouldBeFalse(link.NameKey);
                    link.Route.ShouldStartWith("/erp/");
                    link.Permissions.ShouldNotBeEmpty();

                    foreach (var permission in link.Permissions)
                    {
                        (await permissions.GetOrNullAsync(permission)).ShouldNotBeNull(permission);
                    }
                }
            }
        }
    }

    private IDisposable SignInAs(string userName, params string[] roles)
    {
        var claims = new[]
            {
                new Claim(AbpClaimTypes.UserId, Guid.NewGuid().ToString()),
                new Claim(AbpClaimTypes.UserName, userName),
            }
            .Concat(roles.Select(r => new Claim(AbpClaimTypes.Role, r)));

        return GetRequiredService<ICurrentPrincipalAccessor>()
            .Change(new ClaimsPrincipal(new ClaimsIdentity(claims, "test")));
    }
}
