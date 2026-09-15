using Microsoft.Extensions.Hosting;
using Projects;

namespace ABPmicroservice.AppHost;

internal class Program
{
    private static void Main(string[] args)
    {
        const string LaunchProfileName = "Aspire";
        var builder = DistributedApplication.CreateBuilder(args);

        var postgres = builder.AddPostgres(ABPmicroserviceNames.Postgres).WithPgWeb();
        var rabbitMq = builder.AddRabbitMQ(ABPmicroserviceNames.RabbitMq).WithManagementPlugin();
        var redis = builder.AddRedis(ABPmicroserviceNames.Redis).WithRedisCommander();
        var seq = builder.AddSeq(ABPmicroserviceNames.Seq);

        var adminDb = postgres.AddDatabase(ABPmicroserviceNames.AdministrationDb);
        var identityDb = postgres.AddDatabase(ABPmicroserviceNames.IdentityServiceDb);
        var projectsDb = postgres.AddDatabase(ABPmicroserviceNames.ProjectsDb);
        var saasDb = postgres.AddDatabase(ABPmicroserviceNames.SaaSDb);
        var erpDb = postgres.AddDatabase("ErpDb");

        var migrator = builder
            .AddProject<ABPmicroservice_DbMigrator>(
                ABPmicroserviceNames.DbMigrator,
                launchProfileName: LaunchProfileName
            )
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(projectsDb)
            .WithReference(saasDb)
            .WithReference(erpDb)
            .WithReference(seq)
            .WaitFor(postgres);

        var admin = builder
            .AddProject<ABPmicroservice_Administration_HttpApi_Host>(
                ABPmicroserviceNames.AdministrationApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var identity = builder
            .AddProject<ABPmicroservice_IdentityService_HttpApi_Host>(
                ABPmicroserviceNames.IdentityServiceApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var saas = builder
            .AddProject<ABPmicroservice_SaaS_HttpApi_Host>(
                ABPmicroserviceNames.SaaSApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        builder
            .AddProject<ABPmicroservice_Projects_HttpApi_Host>(
                ABPmicroserviceNames.ProjectsApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(projectsDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        builder
            .AddProject<ABPmicroservice_Erp_HttpApi_Host>(
                "erp-api",
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(erpDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var gateway = builder
            .AddProject<ABPmicroservice_Gateway>(ABPmicroserviceNames.Gateway, launchProfileName: LaunchProfileName)
            .WithExternalHttpEndpoints()
            .WithReference(seq)
            .WaitFor(admin)
            .WaitFor(identity)
            .WaitFor(saas);

        var authserver = builder
            .AddProject<ABPmicroservice_AuthServer>(
                ABPmicroserviceNames.AuthServer,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        builder
            .AddProject<ABPmicroservice_WebApp_Blazor>(
                ABPmicroserviceNames.WebAppClient,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(seq)
            .WaitFor(authserver)
            .WaitFor(gateway);

        builder.Build().Run();
    }
}
