using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.SettingManagement;

namespace ABPmicroservice.Erp.Theming;

/// <summary>
/// The tenant's theme. It is saved for the current tenant, or globally when the host is the one
/// signed in, and read back through the usual setting chain (tenant, then global, then default).
/// </summary>
[Authorize]
public class ThemeAppService : ErpAppService, IThemeAppService
{
    private static readonly string[] Names =
    [
        ErpThemeSettings.PrimaryColor,
        ErpThemeSettings.AccentColor,
        ErpThemeSettings.NavbarStyle,
        ErpThemeSettings.CornerStyle,
    ];

    private static readonly Dictionary<string, string> Defaults = new()
    {
        [ErpThemeSettings.PrimaryColor] = ErpThemeSettings.DefaultPrimaryColor,
        [ErpThemeSettings.AccentColor] = ErpThemeSettings.DefaultAccentColor,
        [ErpThemeSettings.NavbarStyle] = ErpThemeSettings.DefaultNavbarStyle,
        [ErpThemeSettings.CornerStyle] = ErpThemeSettings.DefaultCornerStyle,
    };

    private readonly ISettingManager _settingManager;

    public ThemeAppService(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    public Task<ErpThemeDto> GetAsync()
    {
        return ReadAsync(SettingProvider.GetOrNullAsync);
    }

    [Authorize(ErpPermissions.Theme.Default)]
    public async Task<ErpThemeDto> SaveAsync(ErpThemeDto input)
    {
        await SetAsync(ErpThemeSettings.PrimaryColor, Normalize(input.PrimaryColor));
        await SetAsync(ErpThemeSettings.AccentColor, Normalize(input.AccentColor));
        await SetAsync(ErpThemeSettings.NavbarStyle, input.NavbarStyle.ToString());
        await SetAsync(ErpThemeSettings.CornerStyle, input.CornerStyle.ToString());

        // Not read back: the setting cache is only refreshed once this request's unit of work
        // completes, so a read here would still return the old theme.
        return new ErpThemeDto
        {
            PrimaryColor = Normalize(input.PrimaryColor),
            AccentColor = Normalize(input.AccentColor),
            NavbarStyle = input.NavbarStyle,
            CornerStyle = input.CornerStyle,
        };
    }

    [Authorize(ErpPermissions.Theme.Default)]
    public async Task<ErpThemeDto> ResetAsync()
    {
        foreach (var name in Names)
        {
            await SetAsync(name, null);
        }

        // What shows through now: the host's theme for a tenant, the defaults for the host. Read
        // from the level below, which this request did not change (see SaveAsync).
        return CurrentTenant.Id.HasValue
            ? await ReadAsync(name => _settingManager.GetOrNullGlobalAsync(name))
            : await ReadAsync(name => Task.FromResult(Defaults[name]));
    }

    private static async Task<ErpThemeDto> ReadAsync(Func<string, Task<string>> read)
    {
        return new ErpThemeDto
        {
            PrimaryColor = Normalize(await read(ErpThemeSettings.PrimaryColor)),
            AccentColor = Normalize(await read(ErpThemeSettings.AccentColor)),
            NavbarStyle = Parse(await read(ErpThemeSettings.NavbarStyle), ErpNavbarStyle.Brand),
            CornerStyle = Parse(await read(ErpThemeSettings.CornerStyle), ErpCornerStyle.Rounded),
        };
    }

    /// <summary>A null value removes the stored one, so the next level of the chain shows through.</summary>
    private Task SetAsync(string name, string value)
    {
        return CurrentTenant.Id.HasValue
            ? _settingManager.SetForCurrentTenantAsync(name, value)
            : _settingManager.SetGlobalAsync(name, value);
    }

    /// <summary>Colours are kept upper-case, so the same colour always reads the same.</summary>
    private static string Normalize(string color)
    {
        return color?.Trim().ToUpperInvariant();
    }

    private static TEnum Parse<TEnum>(string value, TEnum fallback)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : fallback;
    }
}
