using Volo.Abp.Modularity;

namespace ABPmicroservice.Erp;

[DependsOn(typeof(ErpApplicationModule))]
[DependsOn(typeof(ErpDomainTestModule))]
public class ErpApplicationTestModule : AbpModule { }
