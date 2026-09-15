using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Kanban;

public class KanbanPipelineAppService : ApplicationService
{
    private readonly IRepository<KanbanStage, Guid> _stageRepository;

    public KanbanPipelineAppService(IRepository<KanbanStage, Guid> stageRepository)
    {
        _stageRepository = stageRepository;
    }

    public async Task<List<KanbanStage>> GetStagesAsync(string pipelineType)
    {
        return await _stageRepository.GetListAsync(s => s.PipelineType == pipelineType);
    }
}
