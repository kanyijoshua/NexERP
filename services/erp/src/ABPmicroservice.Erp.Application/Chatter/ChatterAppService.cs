using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Chatter;

[Authorize(ErpPermissions.Chatter.Default)]
public class ChatterAppService : ErpAppService, IChatterAppService
{
    /// <summary>The newest activity entries a record's chatter shows.</summary>
    public const int MaxActivityEntries = 200;

    private readonly IRepository<DocumentNote, Guid> _noteRepository;
    private readonly IRepository<ActivityStreamEntry, Guid> _activityStreamRepository;
    private readonly IRepository<DocumentActivityTask, Guid> _taskRepository;

    public ChatterAppService(
        IRepository<DocumentNote, Guid> noteRepository,
        IRepository<ActivityStreamEntry, Guid> activityStreamRepository,
        IRepository<DocumentActivityTask, Guid> taskRepository
    )
    {
        _noteRepository = noteRepository;
        _activityStreamRepository = activityStreamRepository;
        _taskRepository = taskRepository;
    }

    public async Task<ListResultDto<DocumentNoteDto>> GetNotesAsync(ChatterEntityInput input)
    {
        // Every card asks for this when it opens: filtered and sorted by the database, on its index.
        var query = (await _noteRepository.GetQueryableAsync())
            .Where(n => n.EntityType == input.EntityType && n.EntityId == input.EntityId)
            .OrderByDescending(n => n.CreationTime);

        return new ListResultDto<DocumentNoteDto>(
            ObjectMapper.Map<List<DocumentNote>, List<DocumentNoteDto>>(await AsyncExecuter.ToListAsync(query))
        );
    }

    [Authorize(ErpPermissions.Chatter.Create)]
    public async Task<DocumentNoteDto> CreateNoteAsync(CreateDocumentNoteDto input)
    {
        var note = new DocumentNote(
            GuidGenerator.Create(),
            input.EntityType,
            input.EntityId,
            input.EntityNo,
            input.NoteText,
            CurrentUser.UserName ?? "System User"
        );

        await _noteRepository.InsertAsync(note, autoSave: true);
        return ObjectMapper.Map<DocumentNote, DocumentNoteDto>(note);
    }

    public async Task<ListResultDto<ActivityStreamEntryDto>> GetActivityStreamAsync(ChatterEntityInput input)
    {
        // The stream only grows; a card shows its most recent part.
        var query = (await _activityStreamRepository.GetQueryableAsync())
            .Where(a => a.EntityType == input.EntityType && a.EntityId == input.EntityId)
            .OrderByDescending(a => a.CreationTime)
            .Take(MaxActivityEntries);

        return new ListResultDto<ActivityStreamEntryDto>(
            ObjectMapper.Map<List<ActivityStreamEntry>, List<ActivityStreamEntryDto>>(await AsyncExecuter.ToListAsync(query))
        );
    }

    public async Task<ListResultDto<DocumentActivityTaskDto>> GetTasksAsync(ChatterEntityInput input)
    {
        var query = (await _taskRepository.GetQueryableAsync())
            .Where(t => t.EntityType == input.EntityType && t.EntityId == input.EntityId)
            .OrderBy(t => t.Completed)
            .ThenBy(t => t.DueDate);

        return new ListResultDto<DocumentActivityTaskDto>(
            ObjectMapper.Map<List<DocumentActivityTask>, List<DocumentActivityTaskDto>>(await AsyncExecuter.ToListAsync(query))
        );
    }

    [Authorize(ErpPermissions.Chatter.Create)]
    public async Task<DocumentActivityTaskDto> CreateTaskAsync(CreateActivityTaskDto input)
    {
        var task = new DocumentActivityTask(
            GuidGenerator.Create(),
            input.EntityType,
            input.EntityId,
            input.EntityNo,
            input.ActivityType,
            input.Summary,
            input.DueDate,
            input.AssignedUserId ?? CurrentUser.Id ?? Guid.Empty
        );

        await _taskRepository.InsertAsync(task, autoSave: true);
        return ObjectMapper.Map<DocumentActivityTask, DocumentActivityTaskDto>(task);
    }

    [Authorize(ErpPermissions.Chatter.Create)]
    public async Task<DocumentActivityTaskDto> CompleteTaskAsync(Guid id)
    {
        var task = await _taskRepository.GetAsync(id);
        task.MarkCompleted();
        await _taskRepository.UpdateAsync(task, autoSave: true);
        return ObjectMapper.Map<DocumentActivityTask, DocumentActivityTaskDto>(task);
    }

    public async Task<ListResultDto<DocumentActivityTaskDto>> GetMyTasksAsync()
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var tasks = await _taskRepository.GetListAsync(t => t.AssignedUserId == userId && !t.Completed);
        return new ListResultDto<DocumentActivityTaskDto>(
            ObjectMapper.Map<List<DocumentActivityTask>, List<DocumentActivityTaskDto>>(tasks.OrderBy(t => t.DueDate).ToList())
        );
    }
}
