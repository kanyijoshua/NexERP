using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Shouldly;
using Volo.Abp.Authorization.Permissions;
using Xunit;

namespace ABPmicroservice.Erp.Permissions;

public class PermissionDefinition_Tests : ErpApplicationTestBase
{
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;

    public PermissionDefinition_Tests()
    {
        _permissionDefinitionManager = GetRequiredService<IPermissionDefinitionManager>();
    }

    [Fact]
    public async Task All_Permissions_Should_Be_Defined()
    {
        var allGroups = await _permissionDefinitionManager.GetGroupsAsync();
        var erpGroup = allGroups.FirstOrDefault(g => g.Name == ErpPermissions.GroupName);
        erpGroup.ShouldNotBeNull();

        var definedPermissions = (await _permissionDefinitionManager.GetPermissionsAsync())
            .Select(p => p.Name)
            .ToHashSet();

        var allConstants = ErpPermissions.GetAll();
        var missing = allConstants.Where(c => !definedPermissions.Contains(c) && c != ErpPermissions.GroupName).ToList();

        missing.ShouldBeEmpty($"Missing permissions: {string.Join(", ", missing)}");
    }
}
