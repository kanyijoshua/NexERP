using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Kanban;

[Authorize(ErpPermissions.Kanban.Default)]
public class KanbanPipelineAppService : ErpAppService, IKanbanPipelineAppService
{
    private readonly IRepository<KanbanStage, Guid> _stageRepository;

    public KanbanPipelineAppService(IRepository<KanbanStage, Guid> stageRepository)
    {
        _stageRepository = stageRepository;
    }

    public async Task<ListResultDto<KanbanStageDto>> GetStagesAsync(GetKanbanStagesInput input)
    {
        var stages = await _stageRepository.GetListAsync(s => s.PipelineType == input.PipelineType);

        return new ListResultDto<KanbanStageDto>(
            ObjectMapper.Map<List<KanbanStage>, List<KanbanStageDto>>(stages.OrderBy(s => s.Sequence).ToList())
        );
    }
}
