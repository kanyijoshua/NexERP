using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.JobQueue;

public interface IJobQueueAppService : IApplicationService
{
    // Categories
    Task<ListResultDto<JobQueueCategoryDto>> GetCategoriesAsync();
    Task<JobQueueCategoryDto> CreateCategoryAsync(CreateUpdateJobQueueCategoryDto input);
    Task<JobQueueCategoryDto> UpdateCategoryAsync(Guid id, CreateUpdateJobQueueCategoryDto input);
    Task DeleteCategoryAsync(Guid id);

    // Entries
    Task<PagedResultDto<JobQueueEntryDto>> GetListAsync(GetJobQueueEntriesInput input);
    Task<JobQueueEntryDto> GetAsync(Guid id);
    Task<JobQueueEntryDto> CreateAsync(CreateJobQueueEntryDto input);
    Task<JobQueueEntryDto> UpdateAsync(Guid id, UpdateJobQueueEntryDto input);
    Task DeleteAsync(Guid id);

    // Entry Actions (Business Central / Odoo lifecycle operations)
    Task<JobQueueEntryDto> SetStatusReadyAsync(Guid id);
    Task<JobQueueEntryDto> SetStatusOnHoldAsync(Guid id);
    Task<JobQueueEntryDto> RestartAsync(Guid id);
    Task<JobQueueRunResultDto> RunOnceAsync(Guid id);

    // Handlers metadata
    Task<ListResultDto<JobTypeInfoDto>> GetAvailableJobTypesAsync();

    // Logs
    Task<PagedResultDto<JobQueueLogEntryDto>> GetLogsAsync(GetJobQueueLogsInput input);
    Task ClearLogsAsync(Guid? jobQueueEntryId = null, int? olderThanDays = null);
}
