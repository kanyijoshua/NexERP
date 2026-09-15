using ABPmicroservice.Erp.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace ABPmicroservice.Erp;

[DependsOn(typeof(AbpValidationModule))]
public class ErpDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ErpDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options
                .Resources.Add<ErpResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/Erp");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Erp", typeof(ErpResource));
        });
    }
}
