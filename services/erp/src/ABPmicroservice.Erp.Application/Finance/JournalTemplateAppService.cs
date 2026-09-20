using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Journal templates. Mirrors Business Central page 100 "Gen. Journal Templates".
/// </summary>
[Authorize(ErpPermissions.Journals.Default)]
public class JournalTemplateAppService : ErpAppService, IJournalTemplateAppService
{
    private readonly IRepository<GenJournalTemplate, Guid> _templateRepository;
    private readonly IRepository<GenJournalBatch, Guid> _batchRepository;

    public JournalTemplateAppService(
        IRepository<GenJournalTemplate, Guid> templateRepository,
        IRepository<GenJournalBatch, Guid> batchRepository
    )
    {
        _templateRepository = templateRepository;
        _batchRepository = batchRepository;
    }

    public async Task<ListResultDto<GenJournalTemplateDto>> GetListAsync()
    {
        var templates = (await _templateRepository.GetListAsync()).OrderBy(t => t.Name).ToList();
        var batches = await _batchRepository.GetListAsync();

        var dtos = ObjectMapper.Map<List<GenJournalTemplate>, List<GenJournalTemplateDto>>(templates);

        foreach (var dto in dtos)
        {
            dto.BatchCount = batches.Count(b => b.JournalTemplateName == dto.Name);
        }

        return new ListResultDto<GenJournalTemplateDto>(dtos);
    }

    [Authorize(ErpPermissions.Journals.Manage)]
    public async Task<GenJournalTemplateDto> CreateAsync(CreateUpdateGenJournalTemplateDto input)
    {
        await EnsureNameIsFreeAsync(input.Name, null);

        var template = new GenJournalTemplate(
            GuidGenerator.Create(),
            input.Name,
            input.Description,
            input.Type,
            input.Recurring,
            input.SourceCode,
            input.NoSeriesCode
        );

        await _templateRepository.InsertAsync(template, autoSave: true);
        return ObjectMapper.Map<GenJournalTemplate, GenJournalTemplateDto>(template);
    }

    [Authorize(ErpPermissions.Journals.Manage)]
    public async Task<GenJournalTemplateDto> UpdateAsync(Guid id, CreateUpdateGenJournalTemplateDto input)
    {
        var template = await _templateRepository.GetAsync(id);

        template.Update(input.Description, input.Type, input.Recurring, input.SourceCode, input.NoSeriesCode);

        await _templateRepository.UpdateAsync(template, autoSave: true);
        return ObjectMapper.Map<GenJournalTemplate, GenJournalTemplateDto>(template);
    }

    [Authorize(ErpPermissions.Journals.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var template = await _templateRepository.GetAsync(id);

        // Deleting a template whose batches still hold lines would strand them.
        if (await _batchRepository.AnyAsync(b => b.JournalTemplateName == template.Name))
        {
            throw new BusinessException(ErpErrorCodes.Journals.BatchNotEmpty).WithData("name", template.Name);
        }

        await _templateRepository.DeleteAsync(template);
    }

    private async Task EnsureNameIsFreeAsync(string name, Guid? exceptId)
    {
        var normalized = name.Trim().ToUpperInvariant();

        var clash = await _templateRepository.FirstOrDefaultAsync(t => t.Name == normalized);
        if (clash != null && clash.Id != exceptId)
        {
            throw new BusinessException(ErpErrorCodes.Journals.TemplateNameAlreadyExists).WithData("name", normalized);
        }
    }
}
