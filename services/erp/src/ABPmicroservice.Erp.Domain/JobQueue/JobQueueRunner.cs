using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.Timing;

namespace ABPmicroservice.Erp.JobQueue;

public interface IJobQueueRunner
{
    IReadOnlyList<IJobHandler> GetAvailableHandlers();

    Task<JobQueueRunResult> ExecuteEntryAsync(Guid entryId, CancellationToken cancellationToken = default);
}

public class JobQueueRunResult
{
    public Guid JobQueueEntryId { get; set; }
    public Guid LogEntryId { get; set; }
    public bool Success { get; set; }
    public JobQueueStatus NewStatus { get; set; }
    public int ProcessedRecords { get; set; }
    public string OutputDetails { get; set; }
    public string ErrorMessage { get; set; }
    public long DurationMs { get; set; }
}

public class JobQueueRunner : DomainService, IJobQueueRunner
{
    private readonly IRepository<JobQueueEntry, Guid> _entryRepository;
    private readonly IRepository<JobQueueLogEntry, Guid> _logRepository;
    private readonly IEnumerable<IJobHandler> _handlers;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public JobQueueRunner(
        IRepository<JobQueueEntry, Guid> entryRepository,
        IRepository<JobQueueLogEntry, Guid> logRepository,
        IEnumerable<IJobHandler> handlers,
        IGuidGenerator guidGenerator,
        IClock clock
    )
    {
        _entryRepository = entryRepository;
        _logRepository = logRepository;
        _handlers = handlers;
        _guidGenerator = guidGenerator;
        _clock = clock;
    }

    public IReadOnlyList<IJobHandler> GetAvailableHandlers() => _handlers.ToList();

    public async Task<JobQueueRunResult> ExecuteEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        var entry = await _entryRepository.GetAsync(entryId);

        if (entry.Status == JobQueueStatus.InProcess)
        {
            // If heartbeat is fresh (less than timeout), reject concurrent execution
            if (entry.LastHeartbeat.HasValue &&
                entry.LastHeartbeat.Value.AddSeconds(entry.TimeoutSeconds) > _clock.Now)
            {
                throw new BusinessException(ErpErrorCodes.JobQueue.JobQueueCannotRunInProcessJob)
                    .WithData("id", entryId);
            }
        }

        var startTime = _clock.Now;
        entry.MarkInProcess(startTime);
        await _entryRepository.UpdateAsync(entry, autoSave: true);

        var systemTaskId = entry.SystemTaskId ?? _guidGenerator.Create();
        var logEntry = new JobQueueLogEntry(
            _guidGenerator.Create(),
            entry.Id,
            entry.Description,
            entry.JobType,
            startTime,
            entry.UserId,
            entry.CategoryCode,
            entry.ParameterString,
            systemTaskId
        );
        await _logRepository.InsertAsync(logEntry, autoSave: true);

        var handler = _handlers.FirstOrDefault(h => string.Equals(h.JobType, entry.JobType, StringComparison.OrdinalIgnoreCase));
        if (handler == null)
        {
            var err = $"No registered job handler found for JobType '{entry.JobType}'.";
            var now = _clock.Now;
            logEntry.MarkError(now, err);
            await _logRepository.UpdateAsync(logEntry, autoSave: true);

            entry.MarkFailed(err, null, now);
            await _entryRepository.UpdateAsync(entry, autoSave: true);

            return new JobQueueRunResult
            {
                JobQueueEntryId = entry.Id,
                LogEntryId = logEntry.Id,
                Success = false,
                NewStatus = entry.Status,
                ErrorMessage = err,
                DurationMs = logEntry.DurationMs,
            };
        }

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(entry.TimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        var context = new JobExecutionContext(
            entry.Id,
            entry.CompanyId,
            entry.JobType,
            entry.ParameterString,
            linkedCts.Token
        );

        JobExecutionResult executionResult;
        try
        {
            executionResult = await handler.ExecuteAsync(context);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            executionResult = JobExecutionResult.Fail($"Job execution timed out after {entry.TimeoutSeconds} seconds.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Job queue entry {Id} ({Description}) failed with unhandled exception.", entry.Id, entry.Description);
            executionResult = JobExecutionResult.Fail(ex.Message, ex.StackTrace);
        }

        var endTime = _clock.Now;
        if (executionResult.Success)
        {
            logEntry.MarkSuccess(endTime, executionResult.ProcessedRecords, executionResult.OutputDetails);
            await _logRepository.UpdateAsync(logEntry, autoSave: true);

            entry.MarkFinished(endTime);
            await _entryRepository.UpdateAsync(entry, autoSave: true);
        }
        else
        {
            logEntry.MarkError(endTime, executionResult.ErrorMessage, executionResult.ErrorStackTrace);
            await _logRepository.UpdateAsync(logEntry, autoSave: true);

            entry.MarkFailed(executionResult.ErrorMessage, executionResult.ErrorStackTrace, endTime);
            await _entryRepository.UpdateAsync(entry, autoSave: true);
        }

        return new JobQueueRunResult
        {
            JobQueueEntryId = entry.Id,
            LogEntryId = logEntry.Id,
            Success = executionResult.Success,
            NewStatus = entry.Status,
            ProcessedRecords = executionResult.ProcessedRecords,
            OutputDetails = executionResult.OutputDetails,
            ErrorMessage = executionResult.ErrorMessage,
            DurationMs = logEntry.DurationMs,
        };
    }
}
