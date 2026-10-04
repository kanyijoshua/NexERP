using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// G/L registers: what was posted, by whom, and the action that undoes it.
/// </summary>
[Authorize(ErpPermissions.GLRegisters.Default)]
public class GLRegisterAppService : ErpAppService, IGLRegisterAppService
{
    private readonly IRepository<GLRegister, Guid> _registerRepository;
    private readonly RegisterEntryReader _entryReader;
    private readonly GenJnlPostReverse _postReverse;

    public GLRegisterAppService(
        IRepository<GLRegister, Guid> registerRepository,
        RegisterEntryReader entryReader,
        GenJnlPostReverse postReverse
    )
    {
        _registerRepository = registerRepository;
        _entryReader = entryReader;
        _postReverse = postReverse;
    }

    public async Task<PagedResultDto<GLRegisterDto>> GetListAsync(GetGLRegistersInput input)
    {
        var queryable = await _registerRepository.GetQueryableAsync();

        if (input.FromDate.HasValue)
        {
            queryable = queryable.Where(r => r.PostingDate >= input.FromDate.Value);
        }

        if (input.ToDate.HasValue)
        {
            queryable = queryable.Where(r => r.PostingDate <= input.ToDate.Value);
        }

        if (input.OnlyReversible)
        {
            queryable = queryable.Where(r => !r.Reversed && r.ReversedRegisterNo == 0);
        }

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var term = input.Filter.Trim().ToLower();
            queryable = queryable.Where(r =>
                r.JournalBatchName.ToLower().Contains(term)
                || r.SourceCode.ToLower().Contains(term)
                || r.UserName.ToLower().Contains(term)
            );
        }

        queryable = ErpListQuery.Filter(queryable, input, Clock.Now, null);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Newest first: the register someone wants to reverse is almost always the last one.
        queryable = input.Sorting.IsNullOrWhiteSpace()
            ? queryable.OrderByDescending(r => r.No)
            : queryable.OrderBy(input.Sorting);

        var registers = await AsyncExecuter.ToListAsync(queryable.PageBy(input.SkipCount, input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<GLRegister>, List<GLRegisterDto>>(registers);

        for (var i = 0; i < registers.Count; i++)
        {
            dtos[i].IsReversible = registers[i].IsReversible;
        }

        return new PagedResultDto<GLRegisterDto>(totalCount, dtos);
    }

    public async Task<ListResultDto<PostingPreviewLineDto>> GetEntriesAsync(long registerNo)
    {
        var entries = await _entryReader.ReadAsync(registerNo);
        return new ListResultDto<PostingPreviewLineDto>(entries.Lines);
    }

    [Authorize(ErpPermissions.GLRegisters.Reverse)]
    public async Task<ReversalResultDto> RunReversalAsync(ReverseRegisterInput input)
    {
        var result = await _postReverse.ReverseRegisterAsync(input.RegisterNo, input.Description);
        return ObjectMapper.Map<ReversalResult, ReversalResultDto>(result);
    }
}
