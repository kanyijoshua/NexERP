using Microsoft.Extensions.DependencyInjection;
using ABPmicroservice.Administration;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;

namespace ABPmicroservice.SaaS;

[DependsOn(typeof(AbpAutoMapperModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
[DependsOn(typeof(AbpTenantManagementApplicationModule))]
[DependsOn(typeof(AdministrationApplicationModule))]
[DependsOn(typeof(SaaSApplicationContractsModule))]
[DependsOn(typeof(SaaSDomainModule))]
public class SaaSApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<SaaSApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<SaaSApplicationModule>(true);
        });
    }
}
