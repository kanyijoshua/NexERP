using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>Configuration templates: the default values a package or an import gives a new record.</summary>
[Authorize(ErpPermissions.RapidStart.Default)]
public class ConfigTemplateAppService : ErpAppService, IConfigTemplateAppService
{
    private readonly IRepository<ConfigTemplate, Guid> _templateRepository;
    private readonly RapidStartAccess _access;

    public ConfigTemplateAppService(IRepository<ConfigTemplate, Guid> templateRepository, RapidStartAccess access)
    {
        _templateRepository = templateRepository;
        _access = access;
    }

    public async Task<ListResultDto<ConfigTemplateDto>> GetListAsync(GetConfigTemplatesInput input)
    {
        var templates = (await _templateRepository.GetListAsync(includeDetails: true))
            .Where(t => input?.EntityName.IsNullOrWhiteSpace() != false || string.Equals(t.EntityName, input.EntityName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.EntityName)
            .ThenBy(t => t.Code)
            .Select(ToDto)
            .ToList();

        return new ListResultDto<ConfigTemplateDto>(templates);
    }

    public async Task<ConfigTemplateDto> GetAsync(Guid id)
    {
        return ToDto(await _templateRepository.GetAsync(id, includeDetails: true));
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task<ConfigTemplateDto> CreateAsync(CreateUpdateConfigTemplateDto input)
    {
        var profile = await _access.CheckReadAsync(input.EntityName);
        var code = input.Code.Trim().ToUpperInvariant();

        if (await _templateRepository.AnyAsync(t => t.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.TemplateCodeAlreadyExists).WithData("code", code);
        }

        var template = new ConfigTemplate(GuidGenerator.Create(), code, profile.Name, input.Description);
        Apply(template, profile, input);

        await _templateRepository.InsertAsync(template, autoSave: true);
        return ToDto(template);
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task<ConfigTemplateDto> UpdateAsync(Guid id, CreateUpdateConfigTemplateDto input)
    {
        var template = await _templateRepository.GetAsync(id, includeDetails: true);

        // The code and the table are what packages refer to the template by, so they stay fixed.
        var profile = await _access.CheckReadAsync(template.EntityName);
        Apply(template, profile, input);

        await _templateRepository.UpdateAsync(template, autoSave: true);
        return ToDto(template);
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _templateRepository.DeleteAsync(id);
    }

    private void Apply(ConfigTemplate template, ConfigTableProfile profile, CreateUpdateConfigTemplateDto input)
    {
        var lines = new List<(string, string, bool)>();

        foreach (var line in input.Lines ?? [])
        {
            var field = profile.FindImportableField(line.FieldName);

            // A key field identifies a record; a default for it would make every record the same one.
            if (field == null || profile.IsKeyField(field.Name))
            {
                throw new BusinessException(ErpErrorCodes.RapidStart.FieldNotImportable)
                    .WithData("fieldName", line.FieldName)
                    .WithData("entityName", profile.Name);
            }

            lines.Add((field.Name, line.DefaultValue, line.Mandatory));
        }

        template.Update(input.Description, input.Enabled);
        template.SetLines(lines, GuidGenerator.Create);
    }

    private static ConfigTemplateDto ToDto(ConfigTemplate template)
    {
        return new ConfigTemplateDto
        {
            Id = template.Id,
            Code = template.Code,
            Description = template.Description,
            EntityName = template.EntityName,
            Enabled = template.Enabled,
            Lines = template
                .Lines.OrderBy(l => l.FieldName)
                .Select(l => new ConfigTemplateLineDto
                {
                    FieldName = l.FieldName,
                    DefaultValue = l.DefaultValue,
                    Mandatory = l.Mandatory,
                })
                .ToList(),
        };
    }
}
