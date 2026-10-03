using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.JobQueue;

public class CleanupJobQueueLogsParameters
{
    public int RetentionDays { get; set; } = 30;
}

/// <summary>
/// Background job that prunes historic Job Queue Log Entries older than a configured retention period.
/// Mirrors Business Central standard maintenance task in Codeunit 448 and Odoo cron log cleanup.
/// </summary>
public class CleanupJobQueueLogsHandler : IJobHandler, ITransientDependency
{
    public string JobType => "CleanupJobQueueLogs";
    public string DisplayName => "Job Queue Log Cleanup";
    public string Description => "Deletes historic execution logs older than the specified retention days (default: 30 days).";
    public string DefaultParametersJson => "{\"RetentionDays\": 30}";

    private readonly IRepository<JobQueueLogEntry, Guid> _logRepository;
    private readonly ILogger<CleanupJobQueueLogsHandler> _logger;

    public CleanupJobQueueLogsHandler(
        IRepository<JobQueueLogEntry, Guid> logRepository,
        ILogger<CleanupJobQueueLogsHandler> logger
    )
    {
        _logRepository = logRepository;
        _logger = logger;
    }

    public async Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context)
    {
        var retentionDays = 30;
        if (!string.IsNullOrWhiteSpace(context.ParameterString))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<CleanupJobQueueLogsParameters>(context.ParameterString);
                if (parsed != null && parsed.RetentionDays > 0)
                {
                    retentionDays = parsed.RetentionDays;
                }
            }
            catch
            {
                // Fall back to default
            }
        }

        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
        var oldLogs = await _logRepository.GetListAsync(l => l.StartDateTime < cutoff);
        var count = oldLogs.Count;

        if (count > 0)
        {
            await _logRepository.DeleteManyAsync(oldLogs, autoSave: true);
            _logger.LogInformation("Cleaned up {Count} job queue log entries older than {RetentionDays} days.", count, retentionDays);
        }

        return JobExecutionResult.Ok(count, $"Pruned {count} log entries older than {retentionDays} days.");
    }
}
