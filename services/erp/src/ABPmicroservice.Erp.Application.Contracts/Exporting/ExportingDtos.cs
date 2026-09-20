using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>A table the caller may export, as the export dialog lists it.</summary>
public class ExportableEntityDto
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public int FieldCount { get; set; }

    public bool IsCompanyScoped { get; set; }
}

public class ExportableFieldDto
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    /// <summary>"string", "number", "date", "boolean", "guid" or "enum": what the UI offers as a filter.</summary>
    public string DataType { get; set; }

    /// <summary>Values to pick from when the field is an enum.</summary>
    public List<string> EnumValues { get; set; }

    /// <summary>Whether this field is offered before the user asks for every column.</summary>
    public bool IncludedByDefault { get; set; }
}

public class EntityFilterDto
{
    [Required]
    public string Field { get; set; }

    public EntityFilterOperator Operator { get; set; }

    public string Value { get; set; }
}

/// <summary>What to export, or to preview before exporting.</summary>
public class DataExportInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxEntityNameLength)]
    public string EntityName { get; set; }

    public List<string> Fields { get; set; } = new();

    public List<EntityFilterDto> Filters { get; set; } = new();

    public string OrderBy { get; set; }

    public bool Descending { get; set; }

    public ExportFormat Format { get; set; } = ExportFormat.Xlsx;

    /// <summary>Caps the export. The service clamps it to the allowed maximum.</summary>
    public int MaxResultCount { get; set; } = ErpDomainConsts.MaxExportRowCount;
}

public class DataPreviewInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxEntityNameLength)]
    public string EntityName { get; set; }

    public List<string> Fields { get; set; } = new();

    public List<EntityFilterDto> Filters { get; set; } = new();

    public string OrderBy { get; set; }

    public bool Descending { get; set; }

    public int SkipCount { get; set; }

    public int MaxResultCount { get; set; } = 25;
}

/// <summary>Rows as name/value pairs, exactly as they would be written to the file.</summary>
public class DataPreviewDto
{
    public long TotalCount { get; set; }

    public List<ExportableFieldDto> Fields { get; set; } = new();

    /// <summary>
    /// Each item is a field-name to value map, serialized as a plain JSON object so the payload
    /// reads the way the file does. It is typed as <c>object</c> because the client proxy
    /// generator cannot express a list of dictionaries, and a wrapper around every row would
    /// show up in the JSON that other systems read.
    /// </summary>
    public List<object> Items { get; set; } = new();
}

public class ExportTemplateDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public string EntityName { get; set; }

    public List<string> Fields { get; set; } = new();

    public ExportFormat Format { get; set; }

    public bool IsShared { get; set; }

    public Guid? OwnerUserId { get; set; }
}

public class CreateUpdateExportTemplateDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxEntityNameLength)]
    public string EntityName { get; set; }

    [Required]
    [MinLength(1)]
    public List<string> Fields { get; set; } = new();

    public ExportFormat Format { get; set; } = ExportFormat.Xlsx;

    public bool IsShared { get; set; }
}

public class GetExportTemplatesInput
{
    /// <summary>Blank returns the templates of every table.</summary>
    public string EntityName { get; set; }
}

/// <summary>
/// Exporting any table, with the columns and filters the caller chooses.
/// Mirrors Odoo's export dialog and the Excel export of a Business Central configuration package.
/// </summary>
public interface IDataExportAppService : IApplicationService
{
    /// <summary>Tables the caller is allowed to export. Routed as GET /api/erp/data-export/entities.</summary>
    Task<ListResultDto<ExportableEntityDto>> GetEntitiesAsync();

    /// <summary>Routed as GET /api/erp/data-export/fields?entityName=Customer.</summary>
    Task<ListResultDto<ExportableFieldDto>> GetFieldsAsync(string entityName);

    /// <summary>The first rows an export would contain, so the choice can be checked first.</summary>
    Task<DataPreviewDto> GetPreviewAsync(DataPreviewInput input);

    /// <summary>Produces the file. Routed as POST /api/erp/data-export/run-export.</summary>
    Task<IRemoteStreamContent> RunExportAsync(DataExportInput input);

    Task<ListResultDto<ExportTemplateDto>> GetTemplatesAsync(GetExportTemplatesInput input);

    Task<ExportTemplateDto> CreateTemplateAsync(CreateUpdateExportTemplateDto input);

    Task<ExportTemplateDto> UpdateTemplateAsync(Guid id, CreateUpdateExportTemplateDto input);

    Task DeleteTemplateAsync(Guid id);
}
