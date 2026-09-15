using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Kanban;

/// <summary>
/// Odoo Kanban Pipeline Stage Definition.
/// </summary>
public class KanbanStage : FullAuditedEntity<Guid>
{
    public string PipelineType { get; private set; } // "Sales", "Purchasing", "Lead"
    public int Sequence { get; private set; }
    public string Name { get; private set; }
    public bool FoldedInKanban { get; private set; }
    public bool IsWonStage { get; private set; }

    protected KanbanStage() { }

    public KanbanStage(Guid id, string pipelineType, int sequence, string name, bool foldedInKanban = false, bool isWonStage = false)
        : base(id)
    {
        PipelineType = Check.NotNullOrWhiteSpace(pipelineType, nameof(pipelineType));
        Sequence = sequence;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        FoldedInKanban = foldedInKanban;
        IsWonStage = isWonStage;
    }
}
