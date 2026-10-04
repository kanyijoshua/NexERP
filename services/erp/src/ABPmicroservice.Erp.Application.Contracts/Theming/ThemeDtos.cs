using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Theming;

public enum ErpNavbarStyle
{
    /// <summary>The top bar in the primary colour.</summary>
    Brand = 0,

    /// <summary>A white top bar it.</summary>
    Light = 1,
}

public enum ErpCornerStyle
{
    Rounded = 0,
    Square = 1,
}

/// <summary>The look of the web app for everyone in the tenant.</summary>
public class ErpThemeDto
{
    /// <summary>#RRGGBB. The top bar and headings.</summary>
    [Required]
    [RegularExpression(ErpThemeConsts.HexColorPattern)]
    public string PrimaryColor { get; set; }

    /// <summary>#RRGGBB. Buttons, links and the activity tiles.</summary>
    [Required]
    [RegularExpression(ErpThemeConsts.HexColorPattern)]
    public string AccentColor { get; set; }

    public ErpNavbarStyle NavbarStyle { get; set; }

    public ErpCornerStyle CornerStyle { get; set; }
}

public static class ErpThemeConsts
{
    public const string HexColorPattern = "^#[0-9A-Fa-f]{6}$";
}

/// <summary>
/// Reading the theme is open to anyone signed in (every page is drawn with it); changing it is an
/// administrator's job.
/// </summary>
public interface IThemeAppService : IApplicationService
{
    Task<ErpThemeDto> GetAsync();

    Task<ErpThemeDto> SaveAsync(ErpThemeDto input);

    /// <summary>Back to the defaults: this tenant's own choice is removed.</summary>
    Task<ErpThemeDto> ResetAsync();
}
