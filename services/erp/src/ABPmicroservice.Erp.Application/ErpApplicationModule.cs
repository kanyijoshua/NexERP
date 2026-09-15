using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace ABPmicroservice.Erp;

[DependsOn(typeof(ErpDomainModule))]
[DependsOn(typeof(ErpApplicationContractsModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
[DependsOn(typeof(AbpAutoMapperModule))]
public class ErpApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<ErpApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ErpApplicationModule>(true);
        });
    }
}
