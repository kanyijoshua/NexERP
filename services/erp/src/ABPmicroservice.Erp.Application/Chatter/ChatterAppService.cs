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
        var notes = await _noteRepository.GetListAsync(n =>
            n.EntityType == input.EntityType && n.EntityId == input.EntityId
        );

        return new ListResultDto<DocumentNoteDto>(
            ObjectMapper.Map<List<DocumentNote>, List<DocumentNoteDto>>(
                notes.OrderByDescending(n => n.CreationTime).ToList()
            )
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
        var entries = await _activityStreamRepository.GetListAsync(a =>
            a.EntityType == input.EntityType && a.EntityId == input.EntityId
        );

        return new ListResultDto<ActivityStreamEntryDto>(
            ObjectMapper.Map<List<ActivityStreamEntry>, List<ActivityStreamEntryDto>>(
                entries.OrderByDescending(a => a.CreationTime).ToList()
            )
        );
    }

    public async Task<ListResultDto<DocumentActivityTaskDto>> GetTasksAsync(ChatterEntityInput input)
    {
        var tasks = await _taskRepository.GetListAsync(t =>
            t.EntityType == input.EntityType && t.EntityId == input.EntityId
        );

        return new ListResultDto<DocumentActivityTaskDto>(
            ObjectMapper.Map<List<DocumentActivityTask>, List<DocumentActivityTaskDto>>(
                tasks.OrderBy(t => t.Completed).ThenBy(t => t.DueDate).ToList()
            )
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
}
