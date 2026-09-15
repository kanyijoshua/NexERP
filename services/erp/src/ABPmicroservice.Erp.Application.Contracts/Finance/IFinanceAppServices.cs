using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Finance;

public interface IGLAccountAppService
    : ICrudAppService<
        GLAccountDto,
        Guid,
        GetGLAccountListInput,
        CreateUpdateGLAccountDto,
        CreateUpdateGLAccountDto
    >
{
    Task<GLAccountDto> GetByNoAsync(string no);

    Task BlockAsync(Guid id);

    Task UnblockAsync(Guid id);
}

public interface IGLEntryAppService : IReadOnlyAppService<GLEntryDto, Guid, GetGLEntryListInput>
{
    Task<ListResultDto<GLEntryDto>> GetByDocumentAsync(string documentNo);
}
