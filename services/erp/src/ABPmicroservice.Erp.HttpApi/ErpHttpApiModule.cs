using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace ABPmicroservice.Erp;

[DependsOn(
    typeof(ErpApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule)
)]
public class ErpHttpApiModule : AbpModule
{
}
