using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace ABPmicroservice.Erp.JobQueue;

[Authorize(ErpPermissions.JobQueue.Default)]
public class JobQueueAppService : ErpAppService, IJobQueueAppService
{
    private readonly IRepository<JobQueueCategory, Guid> _categoryRepository;
    private readonly IRepository<JobQueueEntry, Guid> _entryRepository;
    private readonly IRepository<JobQueueLogEntry, Guid> _logRepository;
    private readonly IJobQueueRunner _runner;
    private readonly IClock _clock;

    public JobQueueAppService(
        IRepository<JobQueueCategory, Guid> categoryRepository,
        IRepository<JobQueueEntry, Guid> entryRepository,
        IRepository<JobQueueLogEntry, Guid> logRepository,
        IJobQueueRunner runner,
        IClock clock
    )
    {
        _categoryRepository = categoryRepository;
        _entryRepository = entryRepository;
        _logRepository = logRepository;
        _runner = runner;
        _clock = clock;
    }

    #region Categories

    public async Task<ListResultDto<JobQueueCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _categoryRepository.GetListAsync();
        var items = categories
            .OrderBy(c => c.Code)
            .Select(c => ObjectMapper.Map<JobQueueCategory, JobQueueCategoryDto>(c))
            .ToList();

        return new ListResultDto<JobQueueCategoryDto>(items);
    }

    [Authorize(ErpPermissions.JobQueue.Manage)]
    public async Task<JobQueueCategoryDto> CreateCategoryAsync(CreateUpdateJobQueueCategoryDto input)
    {
        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        var existing = await _categoryRepository.FirstOrDefaultAsync(c => c.Code == normalizedCode);
        if (existing != null)
        {
            throw new BusinessException(ErpErrorCodes.JobQueue.JobQueueCategoryCodeAlreadyExists)
                .WithData("code", normalizedCode);
        }

        var category = new JobQueueCategory(GuidGenerator.Create(), normalizedCode, input.Description);
        await _categoryRepository.InsertAsync(category);

        return ObjectMapper.Map<JobQueueCategory, JobQueueCategoryDto>(category);
    }

    [Authorize(ErpPermissions.JobQueue.Manage)]
    public async Task<JobQueueCategoryDto> UpdateCategoryAsync(Guid id, CreateUpdateJobQueueCategoryDto input)
    {
        var category = await _categoryRepository.GetAsync(id);
        category.Update(input.Description);
        await _categoryRepository.UpdateAsync(category);

        return ObjectMapper.Map<JobQueueCategory, JobQueueCategoryDto>(category);
    }

    [Authorize(ErpPermissions.JobQueue.Manage)]
    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await _categoryRepository.GetAsync(id);
        await _categoryRepository.DeleteAsync(category);
    }

    #endregion

    #region Entries

    public async Task<PagedResultDto<JobQueueEntryDto>> GetListAsync(GetJobQueueEntriesInput input)
    {
        var query = (await _entryRepository.GetQueryableAsync())
            .WhereIf(!input.CategoryCode.IsNullOrWhiteSpace(), e => e.CategoryCode == input.CategoryCode.Trim().ToUpperInvariant())
            .WhereIf(input.Status.HasValue, e => e.Status == input.Status.Value)
            .WhereIf(!input.JobType.IsNullOrWhiteSpace(), e => e.JobType == input.JobType.Trim());

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var term = input.Filter.Trim().ToLower();
            query = query.Where(e =>
                e.Description.ToLower().Contains(term) ||
                e.JobType.ToLower().Contains(term) ||
                (e.CategoryCode != null && e.CategoryCode.ToLower().Contains(term)));
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = input.Sorting.IsNullOrWhiteSpace()
            ? query.OrderBy(e => e.NextRunTime)
            : query.OrderBy(input.Sorting);

        var entries = await AsyncExecuter.ToListAsync(query.PageBy(input));
        var dtos = entries.Select(e => ObjectMapper.Map<JobQueueEntry, JobQueueEntryDto>(e)).ToList();

        return new PagedResultDto<JobQueueEntryDto>(totalCount, dtos);
    }

    public async Task<JobQueueEntryDto> GetAsync(Guid id)
    {
        var entry = await _entryRepository.GetAsync(id);
        return ObjectMapper.Map<JobQueueEntry, JobQueueEntryDto>(entry);
    }

    [Authorize(ErpPermissions.JobQueue.Manage)]
    public async Task<JobQueueEntryDto> CreateAsync(CreateJobQueueEntryDto input)
    {
        var entry = new JobQueueEntry(
            GuidGenerator.Create(),
            input.Description,
            input.JobType,
            input.CategoryCode,
            input.ParameterString,
            input.Priority,
            input.EarliestStartDateTime,
            input.ExpirationDateTime,
            input.RecurringJob,
            input.IntervalType,
            input.IntervalMinutes,
            input.MaxNoOfAttemptsToRun,
            input.RerunDelaySeconds,
            input.TimeoutSeconds,
            input.UserId ?? CurrentUser.UserName,
            input.RecordIdToProcess,
            input.NextRunDateFormula,
            input.ReferenceStartingTime,
            input.NotifyOnSuccess,
            input.ManualRecurrence,
            input.InactivityTimeoutPeriod
        );

        entry.UpdateRecurrence(
            input.RecurringJob,
            input.IntervalType,
            input.IntervalMinutes,
            input.RunOnMondays,
            input.RunOnTuesdays,
            input.RunOnWednesdays,
            input.RunOnThursdays,
            input.RunOnFridays,
            input.RunOnSaturdays,
            input.RunOnSundays,
            input.DailyStartingTime,
            input.DailyEndingTime,
            input.NextRunDateFormula,
            input.ReferenceStartingTime
        );

        await _entryRepository.InsertAsync(entry);
        return ObjectMapper.Map<JobQueueEntry, JobQueueEntryDto>(entry);
    }

    [Authorize(ErpPermissions.JobQueue.Manage)]
    public async Task<JobQueueEntryDto> UpdateAsync(Guid id, UpdateJobQueueEntryDto input)
    {
        var entry = await _entryRepository.GetAsync(id);

        entry.UpdateGeneral(
            input.Description,
            input.CategoryCode,
            input.JobType,
            input.ParameterString,
            input.Priority,
            input.EarliestStartDateTime,
            input.ExpirationDateTime,
            input.UserId ?? entry.UserId,
            input.RecordIdToProcess,
            input.NotifyOnSuccess,
            input.ManualRecurrence,
            input.InactivityTimeoutPeriod
        );

        entry.UpdateRecurrence(
            input.RecurringJob,
            input.IntervalType,
            input.IntervalMinutes,
            input.RunOnMondays,
            input.RunOnTuesdays,
            input.RunOnWednesdays,
            input.RunOnThursdays,
            input.RunOnFridays,
            input.RunOnSaturdays,
            input.RunOnSundays,
            input.DailyStartingTime,
            input.DailyEndingTime,
            input.NextRunDateFormula,
            input.ReferenceStartingTime
        );

        entry.UpdateResiliency(
            input.MaxNoOfAttemptsToRun,
            input.RerunDelaySeconds,
            input.TimeoutSeconds
        );

        await _entryRepository.UpdateAsync(entry);
        return ObjectMapper.Map<JobQueueEntry, JobQueueEntryDto>(entry);
    }

    [Authorize(ErpPermissions.JobQueue.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var entry = await _entryRepository.GetAsync(id);
        if (entry.Status == JobQueueStatus.InProcess)
        {
            throw new BusinessException(ErpErrorCodes.JobQueue.JobQueueCannotDeleteInProcessJob)
                .WithData("id", id);
        }

        // Delete historic logs for this entry
        var logs = await _logRepository.GetListAsync(l => l.JobQueueEntryId == id);
        if (logs.Count > 0)
        {
            await _logRepository.DeleteManyAsync(logs);
        }

        await _entryRepository.DeleteAsync(entry);
    }

    #endregion

    #region Actions

    [Authorize(ErpPermissions.JobQueue.Manage)]
    public async Task<JobQueueEntryDto> SetStatusReadyAsync(Guid id)
    {
        var entry = await _entryRepository.GetAsync(id);
        entry.SetStatusReady();
        await _entryRepository.UpdateAsync(entry);
        return ObjectMapper.Map<JobQueueEntry, JobQueueEntryDto>(entry);
    }

    [Authorize(ErpPermissions.JobQueue.Manage)]
    public async Task<JobQueueEntryDto> SetStatusOnHoldAsync(Guid id)
    {
        var entry = await _entryRepository.GetAsync(id);
        entry.SetStatusOnHold();
        await _entryRepository.UpdateAsync(entry);
        return ObjectMapper.Map<JobQueueEntry, JobQueueEntryDto>(entry);
    }

    [Authorize(ErpPermissions.JobQueue.Manage)]
    public async Task<JobQueueEntryDto> RestartAsync(Guid id)
    {
        var entry = await _entryRepository.GetAsync(id);
        entry.Restart();
        await _entryRepository.UpdateAsync(entry);
        return ObjectMapper.Map<JobQueueEntry, JobQueueEntryDto>(entry);
    }

    [Authorize(ErpPermissions.JobQueue.Execute)]
    public async Task<JobQueueRunResultDto> RunOnceAsync(Guid id)
    {
        var result = await _runner.ExecuteEntryAsync(id);

        return new JobQueueRunResultDto
        {
            JobQueueEntryId = result.JobQueueEntryId,
            LogEntryId = result.LogEntryId,
            Success = result.Success,
            NewStatus = result.NewStatus,
            ProcessedRecords = result.ProcessedRecords,
            OutputDetails = result.OutputDetails,
            ErrorMessage = result.ErrorMessage,
            DurationMs = result.DurationMs,
        };
    }

    #endregion

    #region Metadata & Logs

    public Task<ListResultDto<JobTypeInfoDto>> GetAvailableJobTypesAsync()
    {
        var handlers = _runner.GetAvailableHandlers();
        var dtos = handlers
            .Select(h => new JobTypeInfoDto
            {
                JobType = h.JobType,
                DisplayName = h.DisplayName,
                Description = h.Description,
                DefaultParametersJson = h.DefaultParametersJson,
            })
            .OrderBy(h => h.DisplayName)
            .ToList();

        return Task.FromResult(new ListResultDto<JobTypeInfoDto>(dtos));
    }

    public async Task<PagedResultDto<JobQueueLogEntryDto>> GetLogsAsync(GetJobQueueLogsInput input)
    {
        var query = (await _logRepository.GetQueryableAsync())
            .WhereIf(input.JobQueueEntryId.HasValue, l => l.JobQueueEntryId == input.JobQueueEntryId.Value)
            .WhereIf(input.Status.HasValue, l => l.Status == input.Status.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = input.Sorting.IsNullOrWhiteSpace()
            ? query.OrderByDescending(l => l.StartDateTime)
            : query.OrderBy(input.Sorting);

        var logs = await AsyncExecuter.ToListAsync(query.PageBy(input));
        var dtos = logs.Select(l => ObjectMapper.Map<JobQueueLogEntry, JobQueueLogEntryDto>(l)).ToList();

        return new PagedResultDto<JobQueueLogEntryDto>(totalCount, dtos);
    }

    [Authorize(ErpPermissions.JobQueue.Manage)]
    public async Task ClearLogsAsync(Guid? jobQueueEntryId = null, int? olderThanDays = null)
    {
        var query = await _logRepository.GetQueryableAsync();

        if (jobQueueEntryId.HasValue)
        {
            query = query.Where(l => l.JobQueueEntryId == jobQueueEntryId.Value);
        }

        if (olderThanDays.HasValue && olderThanDays.Value > 0)
        {
            var cutoff = _clock.Now.AddDays(-olderThanDays.Value);
            query = query.Where(l => l.StartDateTime < cutoff);
        }

        var toDelete = await AsyncExecuter.ToListAsync(query);
        if (toDelete.Count > 0)
        {
            await _logRepository.DeleteManyAsync(toDelete);
        }
    }

    #endregion
}
