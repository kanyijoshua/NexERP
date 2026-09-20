using ABPmicroservice.Erp.Exporting;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace ABPmicroservice.Erp;

[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(ErpDomainSharedModule))]
public class ErpDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // The entity type of an export or an integration query is only known at run time, so the
        // typed reader is resolved as a closed generic. Open generics are not registered by ABP's
        // conventional registrar, which is why this one is explicit.
        context.Services.AddTransient(typeof(EntitySource<>));
    }
}
