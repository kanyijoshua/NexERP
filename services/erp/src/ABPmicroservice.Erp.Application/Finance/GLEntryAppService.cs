using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

[Authorize(ErpPermissions.GLEntries.Default)]
public class GLEntryAppService
    : ReadOnlyAppService<GLEntry, GLEntryDto, Guid, GetGLEntryListInput>,
        IGLEntryAppService
{
    public GLEntryAppService(IRepository<GLEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.GLEntries.Default;
        GetListPolicyName = ErpPermissions.GLEntries.Default;
    }

    public async Task<ListResultDto<GLEntryDto>> GetByDocumentAsync(string documentNo)
    {
        var entries = await Repository.GetListAsync(x => x.DocumentNo == documentNo);
        return new ListResultDto<GLEntryDto>(
            ObjectMapper.Map<System.Collections.Generic.List<GLEntry>, System.Collections.Generic.List<GLEntryDto>>(
                entries
            )
        );
    }

    protected override async Task<IQueryable<GLEntry>> CreateFilteredQueryAsync(
        GetGLEntryListInput input
    )
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(input.GLAccountId.HasValue, x => x.GLAccountId == input.GLAccountId.Value)
            .WhereIf(
                !input.DocumentNo.IsNullOrWhiteSpace(),
                x => x.DocumentNo == input.DocumentNo
            )
            .WhereIf(input.FromDate.HasValue, x => x.PostingDate >= input.FromDate.Value)
            .WhereIf(input.ToDate.HasValue, x => x.PostingDate <= input.ToDate.Value);
    }
}
