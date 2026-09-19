using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ABPmicroservice.Erp.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.VirtualFileSystem;

namespace ABPmicroservice.Erp;

[DependsOn(typeof(AbpAspNetCoreMvcUiMultiTenancyModule))]
[DependsOn(typeof(AbpAuditLoggingEntityFrameworkCoreModule))]
[DependsOn(typeof(AbpPermissionManagementEntityFrameworkCoreModule))]
[DependsOn(typeof(AbpSettingManagementEntityFrameworkCoreModule))]
[DependsOn(typeof(ErpApplicationModule))]
[DependsOn(typeof(ErpEntityFrameworkCoreModule))]
[DependsOn(typeof(ErpHttpApiModule))]
[DependsOn(typeof(ABPmicroserviceMicroserviceModule))]
[DependsOn(typeof(ABPmicroserviceServiceDefaultsModule))]
public class ErpHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        context.ConfigureMicroservice(ABPmicroserviceNames.ErpApi);

        // Expose every ERP application service under /api/erp/<kebab-name>.
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(
                typeof(ErpApplicationModule).Assembly,
                settings =>
                {
                    settings.RootPath = ErpRemoteServiceConsts.ModuleName;
                    settings.RemoteServiceName = ErpRemoteServiceConsts.RemoteServiceName;
                    settings.UrlControllerNameNormalizer = c => ErpUrlNames.NormalizeAcronyms(c.ControllerName);
                }
            );
        });

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<ErpDomainSharedModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        string.Format(
                            "..{0}..{0}src{0}ABPmicroservice.Erp.Domain.Shared",
                            Path.DirectorySeparatorChar
                        )
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<ErpDomainModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        string.Format(
                            "..{0}..{0}src{0}ABPmicroservice.Erp.Domain",
                            Path.DirectorySeparatorChar
                        )
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<ErpApplicationContractsModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        string.Format(
                            "..{0}..{0}src{0}ABPmicroservice.Erp.Application.Contracts",
                            Path.DirectorySeparatorChar
                        )
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<ErpApplicationModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        string.Format(
                            "..{0}..{0}src{0}ABPmicroservice.Erp.Application",
                            Path.DirectorySeparatorChar
                        )
                    )
                );
            });
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
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
        app.UseMultiTenancy();
        app.UseAbpRequestLocalization();
        app.UseAuthorization();
        app.UseMiddleware<CompanyResolutionMiddleware>();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP API");

            var configuration = context.GetConfiguration();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthClientSecret(configuration["AuthServer:SwaggerClientSecret"]);
            options.OAuthScopes(ABPmicroserviceNames.ErpApi);
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
