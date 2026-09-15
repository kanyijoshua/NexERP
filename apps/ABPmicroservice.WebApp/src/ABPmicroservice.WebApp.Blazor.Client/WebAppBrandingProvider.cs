using Microsoft.Extensions.Localization;
using ABPmicroservice.WebApp.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace ABPmicroservice.WebApp.Blazor.Client;

[Dependency(ReplaceServices = true)]
public class WebAppBrandingProvider(IStringLocalizer<WebAppResource> localizer)
    : DefaultBrandingProvider
{
    private readonly IStringLocalizer<WebAppResource> _localizer = localizer;

    public override string AppName => _localizer["AppName"];
}
