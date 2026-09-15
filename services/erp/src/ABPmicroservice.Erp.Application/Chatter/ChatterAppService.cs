using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Chatter;

public class CreateDocumentNoteDto
{
    public string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string EntityNo { get; set; }
    public string NoteText { get; set; }
}

public class CreateActivityTaskDto
{
    public string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string EntityNo { get; set; }
    public string ActivityType { get; set; }
    public string Summary { get; set; }
    public DateTime DueDate { get; set; }
    public Guid AssignedUserId { get; set; }
}

public class ChatterAppService : ApplicationService
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

    public async Task<List<DocumentNote>> GetNotesAsync(string entityType, Guid entityId)
    {
        return await _noteRepository.GetListAsync(n => n.EntityType == entityType && n.EntityId == entityId);
    }

    public async Task<DocumentNote> AddNoteAsync(CreateDocumentNoteDto input)
    {
        var note = new DocumentNote(
            GuidGenerator.Create(),
            input.EntityType,
            input.EntityId,
            input.EntityNo,
            input.NoteText,
            CurrentUser.UserName ?? "System User"
        );
        return await _noteRepository.InsertAsync(note, autoSave: true);
    }

    public async Task<List<ActivityStreamEntry>> GetActivityStreamAsync(string entityType, Guid entityId)
    {
        return await _activityStreamRepository.GetListAsync(a => a.EntityType == entityType && a.EntityId == entityId);
    }

    public async Task<List<DocumentActivityTask>> GetTasksAsync(string entityType, Guid entityId)
    {
        return await _taskRepository.GetListAsync(t => t.EntityType == entityType && t.EntityId == entityId);
    }

    public async Task<DocumentActivityTask> CreateTaskAsync(CreateActivityTaskDto input)
    {
        var task = new DocumentActivityTask(
            GuidGenerator.Create(),
            input.EntityType,
            input.EntityId,
            input.EntityNo,
            input.ActivityType,
            input.Summary,
            input.DueDate,
            input.AssignedUserId
        );
        return await _taskRepository.InsertAsync(task, autoSave: true);
    }
}
