using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Workflows;

[Authorize(ErpPermissions.Workflows.Default)]
public class ApprovalEntryAppService : ErpAppService, IApprovalEntryAppService
{
    private readonly IRepository<ApprovalEntry, Guid> _entryRepository;
    private readonly IRepository<ApprovalUserSetup, Guid> _userSetupRepository;
    private readonly ApprovalsManager _approvalsManager;

    public ApprovalEntryAppService(
        IRepository<ApprovalEntry, Guid> entryRepository,
        IRepository<ApprovalUserSetup, Guid> userSetupRepository,
        ApprovalsManager approvalsManager
    )
    {
        _entryRepository = entryRepository;
        _userSetupRepository = userSetupRepository;
        _approvalsManager = approvalsManager;
    }

    public async Task<PagedResultDto<ApprovalEntryDto>> GetListAsync(GetApprovalEntriesInput input)
    {
        var userId = CurrentUser.Id;

        // A document's history shows every status; the work lists default to what is waiting.
        var status = input.Status ?? (input.DocumentId.HasValue || input.AllStatuses ? null : ApprovalStatus.Open);

        var query = (await _entryRepository.GetQueryableAsync())
            .WhereIf(status.HasValue, e => e.Status == status.Value)
            .WhereIf(input.DocumentId.HasValue, e => e.DocumentId == input.DocumentId.Value)
            .WhereIf(input.OnlyMine, e => e.ApproverId == userId)
            .WhereIf(input.SentByMe, e => e.SenderId == userId);

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = input.Sorting.IsNullOrWhiteSpace()
            ? query.OrderByDescending(e => e.CreationTime).ThenBy(e => e.SequenceNo)
            : query.OrderBy(input.Sorting);

        var entries = await AsyncExecuter.ToListAsync(query.PageBy(input));
        var isAdministrator = userId.HasValue
            && await _userSetupRepository.AnyAsync(s => s.UserId == userId.Value && s.IsApprovalAdministrator);

        var dtos = ObjectMapper.Map<List<ApprovalEntry>, List<ApprovalEntryDto>>(entries);
        foreach (var dto in dtos)
        {
            dto.CanAct = dto.Status == ApprovalStatus.Open && (isAdministrator || dto.ApproverId == userId);
        }

        return new PagedResultDto<ApprovalEntryDto>(totalCount, dtos);
    }

    public async Task<int> GetMyOpenCountAsync()
    {
        var userId = CurrentUser.Id;
        return userId.HasValue
            ? await _entryRepository.CountAsync(e => e.ApproverId == userId.Value && e.Status == ApprovalStatus.Open)
            : 0;
    }

    [Authorize(ErpPermissions.Workflows.Approve)]
    public async Task ApproveAsync(Guid id, ApprovalCommentInput input)
    {
        await _approvalsManager.ApproveAsync(id, GetUserId(), input?.Comment);
    }

    [Authorize(ErpPermissions.Workflows.Approve)]
    public async Task RejectAsync(Guid id, ApprovalCommentInput input)
    {
        await _approvalsManager.RejectAsync(id, GetUserId(), input?.Comment);
    }

    [Authorize(ErpPermissions.Workflows.Approve)]
    public async Task DelegateAsync(Guid id)
    {
        await _approvalsManager.DelegateAsync(id, GetUserId());
    }

    private Guid GetUserId()
    {
        return CurrentUser.Id ?? throw new AbpAuthorizationException();
    }
}
