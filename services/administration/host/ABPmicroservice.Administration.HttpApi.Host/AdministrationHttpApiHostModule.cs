using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using ABPmicroservice.Administration.EntityFrameworkCore;
using ABPmicroservice.Erp;
using ABPmicroservice.IdentityService;
using ABPmicroservice.IdentityService.EntityFrameworkCore;
using ABPmicroservice.MultiTenancy;
using ABPmicroservice.Projects;
using ABPmicroservice.SaaS;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.VirtualFileSystem;

namespace ABPmicroservice.Administration;

[DependsOn(typeof(AbpAspNetCoreMvcUiMultiTenancyModule))]
[DependsOn(typeof(AbpIdentityDomainModule))]
[DependsOn(typeof(AdministrationApplicationModule))]
[DependsOn(typeof(AdministrationEntityFrameworkCoreModule))]
[DependsOn(typeof(AdministrationHttpApiModule))]
// Every service's contracts and texts (menus, permission names) are referenced here so the
// permission dialog, application configuration, and admin tools show permissions for all modules.
[DependsOn(typeof(ErpApplicationContractsModule))]
[DependsOn(typeof(IdentityServiceApplicationContractsModule))]
[DependsOn(typeof(ProjectsApplicationContractsModule))]
[DependsOn(typeof(IdentityServiceEntityFrameworkCoreModule))]
[DependsOn(typeof(SaaSApplicationContractsModule))]
[DependsOn(typeof(ABPmicroserviceMicroserviceModule))]
[DependsOn(typeof(ABPmicroserviceServiceDefaultsModule))]
public class AdministrationHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.ConfigureMicroservice(ABPmicroserviceNames.AdministrationApi);

        // Dynamic permission store queries the database table (AbpPermissionDefinitionRecords),
        // which omits newly added static permissions or permissions not synced to the DB.
        // Since this host directly references contracts (ErpApplicationContractsModule, ProjectsApplicationContractsModule, etc.),
        // disable dynamic store so all static permissions are reliably available in the permission dialog.
        Configure<PermissionManagementOptions>(options =>
        {
            options.IsDynamicPermissionStoreEnabled = false;
        });

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<AdministrationDomainSharedModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        string.Format(
                            "..{0}..{0}src{0}ABPmicroservice.Administration.Domain.Shared",
                            Path.DirectorySeparatorChar
                        )
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<AdministrationDomainModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        string.Format(
                            "..{0}..{0}src{0}ABPmicroservice.Administration.Domain",
                            Path.DirectorySeparatorChar
                        )
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<AdministrationApplicationContractsModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        string.Format(
                            "..{0}..{0}src{0}ABPmicroservice.Administration.Application.Contracts",
                            Path.DirectorySeparatorChar
                        )
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<AdministrationApplicationModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        string.Format(
                            "..{0}..{0}src{0}ABPmicroservice.Administration.Application",
                            Path.DirectorySeparatorChar
                        )
                    )
                );
            });
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        IdentityModelEventSource.ShowPII = true;
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseAbpRequestLocalization();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Administration API");
            var configuration = context.GetConfiguration();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthClientSecret(configuration["AuthServer:SwaggerClientSecret"]);
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
