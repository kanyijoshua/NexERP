using Serilog;
using ABPmicroservice.Administration.EntityFrameworkCore;
using ABPmicroservice.Erp.EntityFrameworkCore;
using ABPmicroservice.Projects.EntityFrameworkCore;
using ABPmicroservice.SaaS.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;

namespace ABPmicroservice.DbMigrator;

internal class Program
{
    private static async Task Main(string[] args)
    {
        ABPmicroserviceLogging.Initialize();

        var builder = Host.CreateApplicationBuilder(args);

        builder.AddServiceDefaults();

        builder.AddNpgsqlDbContext<AdministrationDbContext>(
            connectionName: ABPmicroserviceNames.AdministrationDb
        );
        builder.AddNpgsqlDbContext<IdentityDbContext>(connectionName: ABPmicroserviceNames.IdentityServiceDb);
        builder.AddNpgsqlDbContext<SaaSDbContext>(connectionName: ABPmicroserviceNames.SaaSDb);
        builder.AddNpgsqlDbContext<ProjectsDbContext>(connectionName: ABPmicroserviceNames.ProjectsDb);
        builder.AddNpgsqlDbContext<ErpDbContext>(connectionName: ABPmicroserviceNames.ErpDb);

        builder.Configuration.AddAppSettingsSecretsJson();

        builder.Logging.AddSerilog();

        builder.Services.AddHostedService<DbMigratorHostedService>();

        var host = builder.Build();

        await host.RunAsync();
    }
}
