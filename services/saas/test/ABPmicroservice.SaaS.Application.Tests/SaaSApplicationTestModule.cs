using Volo.Abp.Modularity;

namespace ABPmicroservice.SaaS;

[DependsOn(typeof(SaaSApplicationModule))]
[DependsOn(typeof(SaaSDomainTestModule))]
public class SaaSApplicationTestModule : AbpModule { }
