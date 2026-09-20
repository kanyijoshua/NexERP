using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// Exporting any table the caller is allowed to read.
/// <para>
/// Mirrors Odoo's export dialog — pick a model, pick fields, filter, choose a format — and does
/// the job Business Central gives to a configuration package's Excel export. The permission that
/// guards a table on screen guards it here too, so the export cannot be a way around it.
/// </para>
/// </summary>
[Authorize(ErpPermissions.DataExport.Default)]
public class DataExportAppService : ErpAppService, IDataExportAppService
{
    private readonly ErpEntityRegistry _registry;
    private readonly IEntityQueryExecutor _queryExecutor;
    private readonly DataExportEngine _exportEngine;
    private readonly IRepository<ExportTemplate, Guid> _templateRepository;

    public DataExportAppService(
        ErpEntityRegistry registry,
        IEntityQueryExecutor queryExecutor,
        DataExportEngine exportEngine,
        IRepository<ExportTemplate, Guid> templateRepository
    )
    {
        _registry = registry;
        _queryExecutor = queryExecutor;
        _exportEngine = exportEngine;
        _templateRepository = templateRepository;
    }

    public async Task<ListResultDto<ExportableEntityDto>> GetEntitiesAsync()
    {
        var allowed = new List<ExportableEntityDto>();

        foreach (var definition in _registry.GetAll())
        {
            var permission = ExportFieldMapper.PermissionOf(definition);
            if (permission == null)
            {
                continue;
            }

            if (await AuthorizationService.IsGrantedAsync(permission))
            {
                allowed.Add(ExportFieldMapper.ToDto(definition));
            }
        }

        return new ListResultDto<ExportableEntityDto>(allowed);
    }

    public async Task<ListResultDto<ExportableFieldDto>> GetFieldsAsync(string entityName)
    {
        var definition = await AuthorizeEntityAsync(entityName);
        return new ListResultDto<ExportableFieldDto>(ExportFieldMapper.ToDtos(definition));
    }

    public async Task<DataPreviewDto> GetPreviewAsync(DataPreviewInput input)
    {
        var definition = await AuthorizeEntityAsync(input.EntityName);

        var result = await _queryExecutor.QueryAsync(
            new EntityQueryRequest
            {
                EntityName = definition.Name,
                Fields = input.Fields ?? new List<string>(),
                Filters = ExportFieldMapper.ToFilters(input.Filters),
                OrderBy = input.OrderBy,
                Descending = input.Descending,
                SkipCount = input.SkipCount,
                MaxResultCount = Math.Clamp(input.MaxResultCount, 1, 200),
            }
        );

        return new DataPreviewDto
        {
            TotalCount = result.TotalCount,
            Fields = result.Fields.Select(f => ExportFieldMapper.ToDto(f, definition)).ToList(),
            Items = result.Items.Cast<object>().ToList(),
        };
    }

    public async Task<IRemoteStreamContent> RunExportAsync(DataExportInput input)
    {
        var definition = await AuthorizeEntityAsync(input.EntityName);

        var result = await _queryExecutor.QueryAsync(
            new EntityQueryRequest
            {
                EntityName = definition.Name,
                Fields = input.Fields ?? new List<string>(),
                Filters = ExportFieldMapper.ToFilters(input.Filters),
                OrderBy = input.OrderBy,
                Descending = input.Descending,
                MaxResultCount = Math.Clamp(input.MaxResultCount, 1, ErpDomainConsts.MaxExportRowCount),
            }
        );

        // A silently truncated export is worse than a refused one: the file would look complete.
        if (result.TotalCount > result.Items.Count)
        {
            throw new BusinessException(ErpErrorCodes.Exporting.TooManyRows)
                .WithData("totalCount", result.TotalCount)
                .WithData("maxRowCount", result.Items.Count);
        }

        var columns = result.Fields.Select(f => new ExportColumn(f.Name, f.DisplayName)).ToList();
        var rows = result.Items.Select(row => (IReadOnlyDictionary<string, object>)row).ToList();

        var file = _exportEngine.Write(definition.Name, columns, rows, input.Format);

        return new RemoteStreamContent(new MemoryStream(file.Content), file.FileName, file.ContentType);
    }

    public async Task<ListResultDto<ExportTemplateDto>> GetTemplatesAsync(GetExportTemplatesInput input)
    {
        var templates = (await _templateRepository.GetListAsync())
            .Where(t => input?.EntityName.IsNullOrWhiteSpace() != false || t.EntityName == input.EntityName)
            .Where(t => t.IsVisibleTo(CurrentUser.Id))
            .OrderBy(t => t.EntityName)
            .ThenBy(t => t.Name)
            .ToList();

        return new ListResultDto<ExportTemplateDto>(
            ObjectMapper.Map<List<ExportTemplate>, List<ExportTemplateDto>>(templates)
        );
    }

    [Authorize(ErpPermissions.DataExport.ManageTemplates)]
    public async Task<ExportTemplateDto> CreateTemplateAsync(CreateUpdateExportTemplateDto input)
    {
        var definition = await AuthorizeEntityAsync(input.EntityName);
        EnsureFieldsExist(definition, input.Fields);

        if (await _templateRepository.AnyAsync(t => t.EntityName == definition.Name && t.Name == input.Name))
        {
            throw new BusinessException(ErpErrorCodes.Exporting.TemplateNameAlreadyExists).WithData("name", input.Name);
        }

        var template = new ExportTemplate(
            GuidGenerator.Create(),
            input.Name,
            definition.Name,
            input.Fields,
            input.Format,
            input.IsShared,
            CurrentUser.Id
        );

        await _templateRepository.InsertAsync(template, autoSave: true);
        return ObjectMapper.Map<ExportTemplate, ExportTemplateDto>(template);
    }

    [Authorize(ErpPermissions.DataExport.ManageTemplates)]
    public async Task<ExportTemplateDto> UpdateTemplateAsync(Guid id, CreateUpdateExportTemplateDto input)
    {
        var template = await _templateRepository.GetAsync(id);
        var definition = await AuthorizeEntityAsync(template.EntityName);
        EnsureFieldsExist(definition, input.Fields);

        template.Update(input.Name, input.Fields, input.Format, input.IsShared);

        await _templateRepository.UpdateAsync(template, autoSave: true);
        return ObjectMapper.Map<ExportTemplate, ExportTemplateDto>(template);
    }

    [Authorize(ErpPermissions.DataExport.ManageTemplates)]
    public async Task DeleteTemplateAsync(Guid id)
    {
        await _templateRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Resolves the table and checks the caller may read it. Everything that touches data goes
    /// through here, so there is one place where that check can be missed and it is obvious.
    /// </summary>
    private async Task<ErpEntityDefinition> AuthorizeEntityAsync(string entityName)
    {
        var definition = _registry.Get(entityName);
        var permission = ExportFieldMapper.PermissionOf(definition);

        if (permission == null)
        {
            throw new BusinessException(ErpErrorCodes.Exporting.UnknownEntity).WithData("entityName", entityName);
        }

        await AuthorizationService.CheckAsync(permission);
        return definition;
    }

    private static void EnsureFieldsExist(ErpEntityDefinition definition, IEnumerable<string> fields)
    {
        foreach (var field in fields ?? Enumerable.Empty<string>())
        {
            definition.GetField(field);
        }
    }
}
