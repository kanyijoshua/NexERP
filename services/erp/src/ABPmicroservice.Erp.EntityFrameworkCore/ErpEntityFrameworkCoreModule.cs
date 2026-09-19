using System;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.RapidStart;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

[DependsOn(typeof(ErpDomainModule))]
[DependsOn(typeof(AbpEntityFrameworkCorePostgreSqlModule))]
[DependsOn(typeof(ABPmicroserviceSharedModule))]
public class ErpEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.Databases.Configure(
                ABPmicroserviceNames.ErpDb,
                db =>
                {
                    db.MappedConnections.Add(ErpDbProperties.ConnectionStringName);
                }
            );
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql();
        });

        context.Services.AddAbpDbContext<ErpDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);

            // Header/lines aggregates: repository reads with includeDetails (the default
            // for GetAsync) must bring the child rows, or documents load with no lines.
            options.Entity<SalesHeader>(e => e.DefaultWithDetailsFunc = q => q.Include(x => x.Lines));
            options.Entity<PurchaseHeader>(e => e.DefaultWithDetailsFunc = q => q.Include(x => x.Lines));
            options.Entity<PostedSalesHeader>(e => e.DefaultWithDetailsFunc = q => q.Include(x => x.Lines));
            options.Entity<PostedPurchaseHeader>(e => e.DefaultWithDetailsFunc = q => q.Include(x => x.Lines));
            options.Entity<NoSeries>(e => e.DefaultWithDetailsFunc = q => q.Include(x => x.Lines));
            options.Entity<Workflow>(e => e.DefaultWithDetailsFunc = q => q.Include(x => x.Steps));
            options.Entity<AccountSchedule>(e => e.DefaultWithDetailsFunc = q => q.Include(x => x.Lines));
            options.Entity<ConfigPackage>(e =>
                e.DefaultWithDetailsFunc = q => q.Include(x => x.Tables).ThenInclude(t => t.Fields)
            );
        });
    }
}
