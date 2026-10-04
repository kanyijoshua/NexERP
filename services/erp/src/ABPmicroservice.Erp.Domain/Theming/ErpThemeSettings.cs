using Volo.Abp.Settings;

namespace ABPmicroservice.Erp.Theming;

/// <summary>
/// The look of the web app, one set per tenant (falling back to the host's, then to these
/// defaults). Stored as ABP settings, so there is no table of its own.
/// </summary>
public static class ErpThemeSettings
{
    private const string Prefix = "Erp.Theme.";

    /// <summary>The brand colour: the top bar and headings. Plum by default.</summary>
    public const string PrimaryColor = Prefix + "PrimaryColor";

    /// <summary>The action colour: buttons, links and the activity tiles. Teal by default.</summary>
    public const string AccentColor = Prefix + "AccentColor";

    /// <summary>"Brand" paints the top bar in the primary colour; "Light" keeps it white.</summary>
    public const string NavbarStyle = Prefix + "NavbarStyle";

    /// <summary>"Rounded" or "Square".</summary>
    public const string CornerStyle = Prefix + "CornerStyle";

    public const string DefaultPrimaryColor = "#714B67";
    public const string DefaultAccentColor = "#017E84";
    public const string DefaultNavbarStyle = "Brand";
    public const string DefaultCornerStyle = "Rounded";
}

public class ErpThemeSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(ErpThemeSettings.PrimaryColor, ErpThemeSettings.DefaultPrimaryColor),
            new SettingDefinition(ErpThemeSettings.AccentColor, ErpThemeSettings.DefaultAccentColor),
            new SettingDefinition(ErpThemeSettings.NavbarStyle, ErpThemeSettings.DefaultNavbarStyle),
            new SettingDefinition(ErpThemeSettings.CornerStyle, ErpThemeSettings.DefaultCornerStyle)
        );
    }
}
