using ABPmicroservice.WebApp.Localization;
using Volo.Abp.AspNetCore.Components;

namespace ABPmicroservice.WebApp.Blazor.Client;

public abstract class WebAppComponentBase : AbpComponentBase
{
    protected WebAppComponentBase()
    {
        LocalizationResource = typeof(WebAppResource);
    }
}
