using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Validation;
using Xunit;

namespace ABPmicroservice.Erp.Theming;

public class ThemeAppService_Tests : ErpApplicationTestBase
{
    private readonly IThemeAppService _theme;

    public ThemeAppService_Tests()
    {
        _theme = GetRequiredService<IThemeAppService>();
    }

    [Fact]
    public async Task Starts_From_The_Default_Colours()
    {
        await _theme.ResetAsync();

        var theme = await _theme.GetAsync();

        theme.PrimaryColor.ShouldBe(ErpThemeSettings.DefaultPrimaryColor.ToUpperInvariant());
        theme.AccentColor.ShouldBe(ErpThemeSettings.DefaultAccentColor.ToUpperInvariant());
        theme.NavbarStyle.ShouldBe(ErpNavbarStyle.Brand);
        theme.CornerStyle.ShouldBe(ErpCornerStyle.Rounded);
    }

    [Fact]
    public async Task Saves_The_Admins_Choice_And_Resets_It()
    {
        var saved = await _theme.SaveAsync(
            new ErpThemeDto
            {
                PrimaryColor = "#1f4e79",
                AccentColor = "#00707a",
                NavbarStyle = ErpNavbarStyle.Light,
                CornerStyle = ErpCornerStyle.Square,
            }
        );

        saved.PrimaryColor.ShouldBe("#1F4E79");
        (await _theme.GetAsync()).NavbarStyle.ShouldBe(ErpNavbarStyle.Light);

        var reset = await _theme.ResetAsync();

        reset.PrimaryColor.ShouldBe(ErpThemeSettings.DefaultPrimaryColor.ToUpperInvariant());
        reset.CornerStyle.ShouldBe(ErpCornerStyle.Rounded);
    }

    /// <summary>The colours are written straight into the page's CSS, so only #RRGGBB gets through.</summary>
    [Theory]
    [InlineData("red")]
    [InlineData("#12345")]
    [InlineData("#123456; background: url(x)")]
    public async Task Refuses_Anything_But_A_Hex_Colour(string color)
    {
        await Should.ThrowAsync<AbpValidationException>(() =>
            _theme.SaveAsync(new ErpThemeDto { PrimaryColor = color, AccentColor = "#017E84" })
        );
    }
}
