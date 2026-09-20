using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// G/L registers: what was posted, by whom, and the action that undoes it.
/// Mirrors Business Central page 116 "G/L Registers".
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

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Newest first: the register someone wants to reverse is almost always the last one.
        var registers = await AsyncExecuter.ToListAsync(
            queryable.OrderByDescending(r => r.No).PageBy(input.SkipCount, input.MaxResultCount)
        );

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
