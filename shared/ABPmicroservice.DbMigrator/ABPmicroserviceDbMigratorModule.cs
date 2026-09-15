using ABPmicroservice.Administration;
using ABPmicroservice.Administration.EntityFrameworkCore;
using ABPmicroservice.Erp;
using ABPmicroservice.Erp.EntityFrameworkCore;
using ABPmicroservice.IdentityService;
using ABPmicroservice.IdentityService.EntityFrameworkCore;
using ABPmicroservice.Projects;
using ABPmicroservice.Projects.EntityFrameworkCore;
using ABPmicroservice.SaaS;
using ABPmicroservice.SaaS.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.Tokens;

namespace ABPmicroservice.DbMigrator;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(AbpBackgroundJobsAbstractionsModule))]
[DependsOn(typeof(AdministrationEntityFrameworkCoreModule))]
[DependsOn(typeof(AdministrationApplicationContractsModule))]
[DependsOn(typeof(IdentityServiceEntityFrameworkCoreModule))]
[DependsOn(typeof(IdentityServiceApplicationContractsModule))]
[DependsOn(typeof(ProjectsEntityFrameworkCoreModule))]
[DependsOn(typeof(ProjectsApplicationContractsModule))]
[DependsOn(typeof(SaaSEntityFrameworkCoreModule))]
[DependsOn(typeof(SaaSApplicationContractsModule))]
[DependsOn(typeof(ErpEntityFrameworkCoreModule))]
[DependsOn(typeof(ErpApplicationContractsModule))]
//[DependsOn(typeof(WebAppEntityFrameworkCoreModule))]
//[DependsOn(typeof(WebAppApplicationContractsModule))]
public class ABPmicroserviceDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        Configure<TokenCleanupOptions>(options => options.IsCleanupEnabled = false);
    }
}
