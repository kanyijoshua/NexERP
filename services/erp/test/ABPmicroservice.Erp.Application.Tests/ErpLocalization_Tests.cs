using System.Linq;
using ABPmicroservice.Erp.Localization;
using ABPmicroservice.Erp.Permissions;
using Microsoft.Extensions.Localization;
using Shouldly;
using Volo.Abp.Reflection;
using Xunit;

namespace ABPmicroservice.Erp;

public class ErpLocalization_Tests : ErpApplicationTestBase
{
    private readonly IStringLocalizer<ErpResource> _localizer;

    public ErpLocalization_Tests()
    {
        _localizer = GetRequiredService<IStringLocalizer<ErpResource>>();
    }

    [Fact]
    public void Every_Permission_Has_A_Display_Name()
    {
        var missing = ErpPermissions
            .GetAll()
            .Where(p => p != ErpPermissions.GroupName)
            .Select(p => "Permission:" + p.Replace('.', ':'))
            .Where(key => _localizer[key].ResourceNotFound)
            .ToList();

        missing.ShouldBeEmpty();
    }

    // Otherwise users see raw codes such as "Erp:Journals:00002" instead of a message.
    [Fact]
    public void Every_Error_Code_Has_A_Message()
    {
        var missing = ReflectionHelper
            .GetPublicConstantsRecursively(typeof(ErpErrorCodes))
            .Where(code => code != ErpErrorCodes.Prefix)
            .Where(code => _localizer[code].ResourceNotFound)
            .ToList();

        missing.ShouldBeEmpty();
    }
}
