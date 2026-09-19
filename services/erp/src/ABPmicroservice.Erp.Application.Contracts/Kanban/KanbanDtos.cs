using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Kanban;

public class KanbanStageDto : EntityDto<Guid>
{
    public string PipelineType { get; set; }
    public int Sequence { get; set; }
    public string Name { get; set; }
    public bool FoldedInKanban { get; set; }
    public bool IsWonStage { get; set; }
}

public class GetKanbanStagesInput
{
    /// <summary>"Sales", "Purchasing" or "Lead".</summary>
    [Required]
    public string PipelineType { get; set; }
}

public interface IKanbanPipelineAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/kanban-pipeline/stages?pipelineType=Sales.</summary>
    Task<ListResultDto<KanbanStageDto>> GetStagesAsync(GetKanbanStagesInput input);
}
