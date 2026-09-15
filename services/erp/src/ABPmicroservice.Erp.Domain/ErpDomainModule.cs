using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace ABPmicroservice.Erp;

[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(ErpDomainSharedModule))]
public class ErpDomainModule : AbpModule { }
