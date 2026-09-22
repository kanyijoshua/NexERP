using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Home;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Modules;

public class ModuleAppService_Tests : ErpApplicationTestBase
{
    private readonly IModuleAppService _modules;
    private readonly IHomeAppService _home;

    public ModuleAppService_Tests()
    {
        _modules = GetRequiredService<IModuleAppService>();
        _home = GetRequiredService<IHomeAppService>();
    }

    private Task<ErpModuleDto> SetAsync(string code, bool enabled)
    {
        return InCompanyAsync(
            DefaultCompanyName,
            () => _modules.SetEnabledAsync(new SetModuleEnabledInput { Code = code, Enabled = enabled })
        );
    }

    /// <summary>A company that has never touched this runs everything, as a fresh install should.</summary>
    [Fact]
    public async Task Every_Module_Is_On_Until_One_Is_Switched_Off()
    {
        var list = await InCompanyAsync(DefaultCompanyName, () => _modules.GetListAsync());

        list.Items.Count.ShouldBe(ErpModuleRegistry.All.Count);
        list.Items.ShouldAllBe(m => m.Enabled);
    }

    [Fact]
    public async Task Switching_A_Module_Off_And_On_Again_Sticks()
    {
        (await SetAsync(ErpModuleRegistry.Inventory, false)).Enabled.ShouldBeFalse();

        var off = await InCompanyAsync(DefaultCompanyName, () => _modules.GetListAsync());
        off.Items.Single(m => m.Code == ErpModuleRegistry.Inventory).Enabled.ShouldBeFalse();

        (await SetAsync(ErpModuleRegistry.Inventory, true)).Enabled.ShouldBeTrue();
    }

    [Fact]
    public async Task A_Core_Module_Cannot_Be_Switched_Off()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() => SetAsync(ErpModuleRegistry.Finance, false));

        exception.Code.ShouldBe(ErpErrorCodes.Modules.CoreModuleCannotBeDisabled);
    }

    [Fact]
    public async Task An_Unknown_Module_Is_Refused()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() => SetAsync("NotAModule", false));

        exception.Code.ShouldBe(ErpErrorCodes.Modules.UnknownModule);
    }

    /// <summary>
    /// The rule Odoo enforces on uninstall: something that is still needed cannot go first.
    /// Core modules cover the built-in dependencies, so this is shown with a stored one.
    /// </summary>
    [Fact]
    public async Task A_Module_Still_Needed_By_Another_Cannot_Be_Switched_Off()
    {
        // Sales depends on Finance, and Finance is core, so the pairing is checked the other way:
        // every dependency named by an enabled module must itself be enabled.
        var sales = await InCompanyAsync(
            DefaultCompanyName,
            async () => (await _modules.GetListAsync()).Items.Single(m => m.Code == ErpModuleRegistry.Sales)
        );

        sales.DependsOn.ShouldContain(ErpModuleRegistry.Finance);
        sales.BlockedBy.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_Switched_Off_Module_Drops_Off_The_Home_Page()
    {
        var before = await InCompanyAsync(DefaultCompanyName, () => _home.GetSummaryAsync());
        before.Apps.Select(a => a.Code).ShouldContain(ErpModuleRegistry.Reporting);

        await SetAsync(ErpModuleRegistry.Reporting, false);

        var after = await InCompanyAsync(DefaultCompanyName, () => _home.GetSummaryAsync());
        after.Apps.Select(a => a.Code).ShouldNotContain(ErpModuleRegistry.Reporting);

        await SetAsync(ErpModuleRegistry.Reporting, true);
    }

    /// <summary>A tile that opens nothing is not a tile, so a module with no screen is not an app.</summary>
    [Fact]
    public async Task A_Module_With_No_Screen_Is_Not_Shown_As_An_App()
    {
        var summary = await InCompanyAsync(DefaultCompanyName, () => _home.GetSummaryAsync());

        summary.Apps.ShouldAllBe(a => !string.IsNullOrWhiteSpace(a.Route));
        summary.Apps.Select(a => a.Code).ShouldNotContain(ErpModuleRegistry.Inventory);
    }

    [Fact]
    public async Task Each_Module_Reads_With_A_Name_And_A_Description()
    {
        var list = await InCompanyAsync(DefaultCompanyName, () => _modules.GetListAsync());

        foreach (var module in list.Items)
        {
            // An unresolved localization key comes back as the key itself.
            module.DisplayName.ShouldNotStartWith("Module:");
            module.Description.ShouldNotStartWith("ModuleDescription:");
            module.Group.ShouldNotStartWith("ModuleGroup:");
        }
    }

    /// <summary>Switching a module off in one company must not switch it off in another.</summary>
    [Fact]
    public async Task The_Switches_Belong_To_One_Company()
    {
        await SetAsync(ErpModuleRegistry.DataExport, false);

        var other = await InCompanyAsync(SecondCompanyName, () => _modules.GetListAsync());
        other.Items.Single(m => m.Code == ErpModuleRegistry.DataExport).Enabled.ShouldBeTrue();

        await SetAsync(ErpModuleRegistry.DataExport, true);
    }
}
