using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.RapidStart;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;

namespace ABPmicroservice.Erp;

[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(ErpDomainSharedModule))]
[DependsOn(typeof(AbpSettingManagementDomainModule))]
public class ErpDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // The entity type of an export or an integration query is only known at run time, so the
        // typed reader is resolved as a closed generic. Open generics are not registered by ABP's
        // conventional registrar, which is why this one is explicit.
        context.Services.AddTransient(typeof(EntitySource<>));

        // The same holds for the writer a configuration package or a data import applies records with.
        context.Services.AddTransient(typeof(ConfigEntityStore<>));

        // Job Queue Handlers
        context.Services.AddTransient<JobQueue.IJobHandler, JobQueue.CleanupJobQueueLogsHandler>();
        context.Services.AddTransient<JobQueue.IJobHandler, JobQueue.SalesPostBatchJobHandler>();
        context.Services.AddTransient<JobQueue.IJobHandler, JobQueue.PurchasingPostBatchJobHandler>();
        context.Services.AddTransient<JobQueue.IJobHandler, JobQueue.WebhookRetrySweepJobHandler>();
    }
}
