using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ABPmicroservice.Administration.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;

namespace ABPmicroservice.Administration;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        ABPmicroserviceLogging.Initialize();

        try
        {
            Log.Information("Starting web host.");

            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();
            builder.AddSharedEndpoints();

            builder.AddNpgsqlDbContext<AdministrationDbContext>(
                connectionName: ABPmicroserviceNames.AdministrationDb,
                configure => configure.DisableRetry = true
            );
            builder.AddNpgsqlDbContext<IdentityDbContext>(
                connectionName: ABPmicroserviceNames.IdentityServiceDb,
                configure => configure.DisableRetry = true
            );

            builder.Host.AddAppSettingsSecretsJson().UseAutofac().UseSerilog();

            await builder.AddApplicationAsync<AdministrationHttpApiHostModule>();

            var app = builder.Build();

            await app.InitializeApplicationAsync();

            await app.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
