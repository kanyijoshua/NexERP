using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Chatter;

/// <summary>Identifies the record a chatter thread hangs off, e.g. ("SalesHeader", id).</summary>
public class ChatterEntityInput
{
    [Required]
    public string EntityType { get; set; }

    public Guid EntityId { get; set; }
}

public class DocumentNoteDto : EntityDto<Guid>
{
    public string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string EntityNo { get; set; }
    public string NoteText { get; set; }
    public string AuthorName { get; set; }
    public DateTime CreationTime { get; set; }
}

public class CreateDocumentNoteDto
{
    [Required]
    public string EntityType { get; set; }

    public Guid EntityId { get; set; }
    public string EntityNo { get; set; }

    [Required]
    [StringLength(2000)]
    public string NoteText { get; set; }
}

public class ActivityStreamEntryDto : EntityDto<Guid>
{
    public string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string EntityNo { get; set; }
    public string FieldName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string ActionDescription { get; set; }
    public DateTime CreationTime { get; set; }
}

public class DocumentActivityTaskDto : EntityDto<Guid>
{
    public string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string EntityNo { get; set; }
    public string ActivityType { get; set; }
    public string Summary { get; set; }
    public DateTime DueDate { get; set; }
    public Guid AssignedUserId { get; set; }
    public bool Completed { get; set; }
}

public class CreateActivityTaskDto
{
    [Required]
    public string EntityType { get; set; }

    public Guid EntityId { get; set; }
    public string EntityNo { get; set; }

    /// <summary>"To Do", "Call", "Email" or "Meeting".</summary>
    [Required]
    public string ActivityType { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Summary { get; set; }

    public DateTime DueDate { get; set; }

    /// <summary>Defaults to the current user.</summary>
    public Guid? AssignedUserId { get; set; }
}

public interface IChatterAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/chatter/notes?entityType=..&amp;entityId=..</summary>
    Task<ListResultDto<DocumentNoteDto>> GetNotesAsync(ChatterEntityInput input);

    /// <summary>Routed as POST /api/erp/chatter/note.</summary>
    Task<DocumentNoteDto> CreateNoteAsync(CreateDocumentNoteDto input);

    /// <summary>Routed as GET /api/erp/chatter/activity-stream?entityType=..&amp;entityId=..</summary>
    Task<ListResultDto<ActivityStreamEntryDto>> GetActivityStreamAsync(ChatterEntityInput input);

    /// <summary>Routed as GET /api/erp/chatter/tasks?entityType=..&amp;entityId=..</summary>
    Task<ListResultDto<DocumentActivityTaskDto>> GetTasksAsync(ChatterEntityInput input);

    /// <summary>Routed as POST /api/erp/chatter/task.</summary>
    Task<DocumentActivityTaskDto> CreateTaskAsync(CreateActivityTaskDto input);
}
