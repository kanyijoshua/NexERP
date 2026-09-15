using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ABPmicroservice.Erp.EntityFrameworkCore;
using Volo.Abp;
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

        context.ConfigureMicroservice("Erp");
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
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
