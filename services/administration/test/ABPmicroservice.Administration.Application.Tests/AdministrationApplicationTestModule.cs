using Volo.Abp.Modularity;

namespace ABPmicroservice.Administration;

[DependsOn(typeof(AdministrationApplicationModule))]
[DependsOn(typeof(AdministrationDomainTestModule))]
public class AdministrationApplicationTestModule : AbpModule { }
