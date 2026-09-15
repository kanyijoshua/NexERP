using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

[DependsOn(
    typeof(ErpDomainModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule)
)]
public class ErpEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<ErpDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql();
        });
    }
}
