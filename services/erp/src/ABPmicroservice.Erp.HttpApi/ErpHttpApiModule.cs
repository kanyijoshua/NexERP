using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using ABPmicroservice.Erp.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace ABPmicroservice.Erp;

[DependsOn(typeof(ErpApplicationContractsModule))]
[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class ErpHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(ErpHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources.Get<ErpResource>().AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
