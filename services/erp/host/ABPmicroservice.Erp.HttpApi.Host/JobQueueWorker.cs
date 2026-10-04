using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.JobQueue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace ABPmicroservice.Erp;

/// <summary>
/// Background worker daemon executing scheduled and recurring Job Queue entries across companies.
/// </summary>
public class JobQueueWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);
    private const int BatchSize = 10;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobQueueWorker> _logger;

    public JobQueueWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<JobQueueWorker> logger
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueJobsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The Job Queue background worker encountered an unexpected failure.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task ProcessDueJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        var unitOfWorkManager = provider.GetRequiredService<IUnitOfWorkManager>();
        var dataFilter = provider.GetRequiredService<IDataFilter>();
        var currentTenant = provider.GetRequiredService<ICurrentTenant>();
        var clock = provider.GetRequiredService<IClock>();

        using var uow = unitOfWorkManager.Begin(requiresNew: true);

        // Bypass ambient multi-tenant and company filters to scan due jobs across the entire instance
        using (currentTenant.Change(null))
        using (dataFilter.Disable<IMultiTenant>())
        using (dataFilter.Disable<ICompanyScoped>())
        {
            var entryRepository = provider.GetRequiredService<IRepository<JobQueueEntry, Guid>>();
            var runner = provider.GetRequiredService<IJobQueueRunner>();

            var now = clock.Now;

            var dueJobs = (await entryRepository.GetListAsync(e =>
                e.Status == JobQueueStatus.Ready &&
                e.NextRunTime.HasValue &&
                e.NextRunTime.Value <= now
            ))
            .OrderBy(e => e.Priority)
            .ThenBy(e => e.NextRunTime)
            .Take(BatchSize)
            .ToList();

            if (dueJobs.Count == 0)
            {
                await uow.CompleteAsync(cancellationToken);
                return;
            }

            _logger.LogInformation("Job Queue Worker found {Count} due job(s) to process.", dueJobs.Count);

            foreach (var job in dueJobs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    using (currentTenant.Change(job.TenantId))
                    {
                        var result = await runner.ExecuteEntryAsync(job.Id, cancellationToken);
                        _logger.LogInformation(
                            "Executed Job Queue Entry {Id} ({Description}): {Status} ({Result})",
                            job.Id,
                            job.Description,
                            result.NewStatus,
                            result.Success ? "Success" : result.ErrorMessage
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispatch Job Queue Entry {Id} ({Description})", job.Id, job.Description);
                }
            }

            await uow.CompleteAsync(cancellationToken);
        }
    }
}

public static class JobQueueWorkerExtensions
{
    public static IServiceCollection AddErpJobQueueWorker(this IServiceCollection services)
    {
        services.AddHostedService<JobQueueWorker>();
        return services;
    }
}
