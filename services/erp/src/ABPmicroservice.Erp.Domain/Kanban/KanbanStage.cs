using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Kanban;

/// <summary>
/// Kanban Pipeline Stage Definition.
/// </summary>
public class KanbanStage : CompanyEntity
{
    public string PipelineType { get; private set; } // "Sales", "Purchasing", "Lead"
    public int Sequence { get; private set; }
    public string Name { get; private set; }
    public bool FoldedInKanban { get; private set; }
    public bool IsWonStage { get; private set; }

    /// <summary>A card dropped here is lost; the close code says why.</summary>
    public bool IsLostStage { get; private set; }

    /// <summary>The probability an opportunity takes when it enters the stage, in percent.</summary>
    public decimal DefaultProbability { get; private set; }

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

    public void Update(string pipelineType, int sequence, string name, bool foldedInKanban, bool isWonStage, bool isLostStage, decimal defaultProbability)
    {
        if (isWonStage && isLostStage)
        {
            throw new BusinessException(ErpErrorCodes.Crm.StageWonAndLost).WithData("name", name ?? "");
        }
        if (defaultProbability < 0m || defaultProbability > 100m)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", defaultProbability);
        }

        PipelineType = Check.NotNullOrWhiteSpace(pipelineType, nameof(pipelineType), 50);
        Sequence = sequence;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        FoldedInKanban = foldedInKanban;
        IsWonStage = isWonStage;
        IsLostStage = isLostStage;
        DefaultProbability = defaultProbability;
    }
}
