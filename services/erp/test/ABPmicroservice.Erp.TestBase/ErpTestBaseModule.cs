using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Volo.Abp.Threading;

namespace ABPmicroservice.Erp;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(AbpTestBaseModule))]
[DependsOn(typeof(AbpAuthorizationModule))]
[DependsOn(typeof(ErpDomainModule))]
public class ErpTestBaseModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAlwaysAllowAuthorization();

        // Settings live in the Administration database in production, which the tests do not
        // have: values go to memory, and the definitions are not written anywhere.
        Configure<SettingManagementOptions>(options =>
        {
            options.SaveStaticSettingsToDatabase = false;
            options.IsDynamicSettingStoreEnabled = false;
        });
        context.Services.Replace(ServiceDescriptor.Singleton<ISettingManagementStore, InMemorySettingManagementStore>());
        context.Services.Replace(ServiceDescriptor.Singleton<IDynamicSettingDefinitionStore, NullDynamicSettingDefinitionStore>());
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        SeedTestData(context);
    }

    // Runs the production ERP seeder: two CRONUS companies, each with its chart of
    // accounts and posting setup, plus sample master data in the default one.
    private static void SeedTestData(ApplicationInitializationContext context)
    {
        AsyncHelper.RunSync(async () =>
        {
            using (var scope = context.ServiceProvider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IDataSeeder>().SeedAsync();
            }
        });
    }
}
