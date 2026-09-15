using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace ABPmicroservice.Erp;

[DependsOn(typeof(ErpDomainSharedModule))]
[DependsOn(typeof(AbpDddApplicationContractsModule))]
[DependsOn(typeof(AbpAuthorizationModule))]
public class ErpApplicationContractsModule : AbpModule { }
