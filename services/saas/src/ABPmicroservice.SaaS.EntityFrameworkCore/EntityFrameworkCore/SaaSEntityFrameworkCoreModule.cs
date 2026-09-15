using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace ABPmicroservice.SaaS.EntityFrameworkCore;

[DependsOn(typeof(AbpEntityFrameworkCorePostgreSqlModule))]
[DependsOn(typeof(AbpTenantManagementEntityFrameworkCoreModule))]
[DependsOn(typeof(SaaSDomainModule))]
[DependsOn(typeof(ABPmicroserviceSharedModule))]
public class SaaSEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.Databases.Configure(
                ABPmicroserviceNames.SaaSDb,
                db =>
                {
                    db.MappedConnections.Add("AbpTenantManagement");
                }
            );
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql();
        });

        context.Services.AddAbpDbContext<SaaSDbContext>(options =>
        {
            options.ReplaceDbContext<ITenantManagementDbContext>();

            options.AddDefaultRepositories(true);
        });
    }
}
