using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Chatter;

/// <summary>
/// Odoo Chatter Internal Note.
/// Attaches internal comments, discussions, and log notes to any ERP entity.
/// </summary>
public class DocumentNote : FullAuditedEntity<Guid>
{
    public string EntityType { get; private set; } // e.g. "SalesHeader", "PurchaseHeader", "Customer", "Vendor", "Item"
    public Guid EntityId { get; private set; }
    public string EntityNo { get; private set; }
    public string NoteText { get; private set; }
    public string AuthorName { get; private set; }

    protected DocumentNote() { }

    public DocumentNote(Guid id, string entityType, Guid entityId, string entityNo, string noteText, string authorName = "System User")
        : base(id)
    {
        EntityType = Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
        EntityId = entityId;
        EntityNo = Check.NotNullOrWhiteSpace(entityNo, nameof(entityNo), ErpDomainConsts.MaxNoLength);
        NoteText = Check.NotNullOrWhiteSpace(noteText, nameof(noteText));
        AuthorName = authorName;
    }
}

/// <summary>
/// Odoo Chatter Activity Stream Entry. Audit log of entity status transitions.
/// </summary>
public class ActivityStreamEntry : FullAuditedEntity<Guid>
{
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public string EntityNo { get; private set; }
    public string FieldName { get; private set; }
    public string OldValue { get; private set; }
    public string NewValue { get; private set; }
    public string ActionDescription { get; private set; }

    protected ActivityStreamEntry() { }

    public ActivityStreamEntry(Guid id, string entityType, Guid entityId, string entityNo, string fieldName, string oldValue, string newValue, string actionDescription)
        : base(id)
    {
        EntityType = Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
        EntityId = entityId;
        EntityNo = Check.NotNullOrWhiteSpace(entityNo, nameof(entityNo), ErpDomainConsts.MaxNoLength);
        FieldName = fieldName;
        OldValue = oldValue;
        NewValue = newValue;
        ActionDescription = actionDescription;
    }
}

/// <summary>
/// Odoo Chatter Follow-Up Activity Task.
/// </summary>
public class DocumentActivityTask : FullAuditedEntity<Guid>
{
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public string EntityNo { get; private set; }
    public string ActivityType { get; private set; } // "To Do", "Call", "Email", "Meeting"
    public string Summary { get; private set; }
    public DateTime DueDate { get; private set; }
    public Guid AssignedUserId { get; private set; }
    public bool Completed { get; private set; }

    protected DocumentActivityTask() { }

    public DocumentActivityTask(Guid id, string entityType, Guid entityId, string entityNo, string activityType, string summary, DateTime dueDate, Guid assignedUserId)
        : base(id)
    {
        EntityType = Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
        EntityId = entityId;
        EntityNo = Check.NotNullOrWhiteSpace(entityNo, nameof(entityNo), ErpDomainConsts.MaxNoLength);
        ActivityType = Check.NotNullOrWhiteSpace(activityType, nameof(activityType));
        Summary = Check.NotNullOrWhiteSpace(summary, nameof(summary), ErpDomainConsts.MaxDescriptionLength);
        DueDate = dueDate;
        AssignedUserId = assignedUserId;
        Completed = false;
    }

    public void MarkCompleted() => Completed = true;
}
