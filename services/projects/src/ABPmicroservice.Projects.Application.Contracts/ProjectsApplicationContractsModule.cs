using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace ABPmicroservice.Projects;

[DependsOn(typeof(ProjectsDomainSharedModule))]
[DependsOn(typeof(AbpDddApplicationContractsModule))]
[DependsOn(typeof(AbpAuthorizationModule))]
public class ProjectsApplicationContractsModule : AbpModule { }
