using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// Configuration packages. Mirrors Business Central's RapidStart Services: design a package from
/// tables and fields, fill it from the database or from a file, check it, and apply it.
/// Routed under /api/erp/config-package.
/// </summary>
public interface IConfigPackageAppService : IApplicationService
{
    Task<ListResultDto<ConfigPackageDto>> GetListAsync(GetConfigPackagesInput input);

    Task<ConfigPackageDetailDto> GetAsync(Guid id);

    Task<ConfigPackageDetailDto> CreateAsync(CreateConfigPackageDto input);

    Task<ConfigPackageDetailDto> UpdateAsync(Guid id, UpdateConfigPackageDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Tables a package may carry. GET /api/erp/config-package/table-catalog.</summary>
    Task<ListResultDto<ConfigTableInfoDto>> GetTableCatalogAsync();

    /// <summary>Fields of one table a package may carry. GET /api/erp/config-package/table-fields?entityName=Customer.</summary>
    Task<ListResultDto<ConfigFieldInfoDto>> GetTableFieldsAsync(string entityName);

    Task<ConfigPackageDetailDto> IncludeTablesAsync(Guid id, IncludeConfigTablesInput input);

    Task<ConfigPackageDetailDto> ExcludeTableAsync(Guid id, ConfigPackageTableInput input);

    /// <summary>Changes a table's options, filters and fields. PUT /api/erp/config-package/{id}/table.</summary>
    Task<ConfigPackageDetailDto> UpdateTableAsync(Guid id, UpdateConfigPackageTableDto input);

    /// <summary>BC's "Get Data from Database": stages what the tables hold now.</summary>
    Task<ConfigPackageDetailDto> FillFromDatabaseAsync(Guid id, ConfigPackageTablesInput input);

    Task<ConfigPackageRunResultDto> ValidatePackageAsync(Guid id, ConfigPackageTablesInput input);

    Task<ConfigPackageRunResultDto> ApplyPackageAsync(Guid id, ConfigPackageTablesInput input);

    Task<ConfigPackageDetailDto> ClearPackageDataAsync(Guid id, ConfigPackageTablesInput input);

    /// <summary>The package as a JSON package file or an Excel workbook. POST /api/erp/config-package/{id}/export-package.</summary>
    Task<IRemoteStreamContent> ExportPackageAsync(Guid id, ExportConfigPackageInput input);

    /// <summary>Reads a JSON package file, replacing a package with the same code.</summary>
    Task<ConfigPackageDetailDto> ImportPackageAsync(UploadFileInput input);

    /// <summary>Stages data from an Excel workbook, one sheet per table.</summary>
    Task<ImportConfigDataResultDto> ImportDataAsync(Guid id, UploadFileInput input);

    Task<PagedResultDto<ConfigPackageRecordDto>> GetRecordsAsync(GetConfigPackageRecordsInput input);

    /// <summary>Corrects a staged record. The id is the record's. PUT /api/erp/config-package/{id}/record.</summary>
    Task<ConfigPackageRecordDto> UpdateRecordAsync(Guid id, UpdateConfigPackageRecordDto input);

    /// <summary>Drops a staged record. The id is the record's. DELETE /api/erp/config-package/{id}/record.</summary>
    Task DeleteRecordAsync(Guid id);

    /// <summary>Every error of the package. GET /api/erp/config-package/{id}/errors.</summary>
    Task<ListResultDto<ConfigPackageErrorDto>> GetErrorsAsync(Guid id);
}

/// <summary>Configuration templates: default values for new records. Routed under /api/erp/config-template.</summary>
public interface IConfigTemplateAppService : IApplicationService
{
    Task<ListResultDto<ConfigTemplateDto>> GetListAsync(GetConfigTemplatesInput input);

    Task<ConfigTemplateDto> GetAsync(Guid id);

    Task<ConfigTemplateDto> CreateAsync(CreateUpdateConfigTemplateDto input);

    Task<ConfigTemplateDto> UpdateAsync(Guid id, CreateUpdateConfigTemplateDto input);

    Task DeleteAsync(Guid id);
}

/// <summary>The configuration worksheet: a company's set-up checklist. Routed under /api/erp/config-worksheet.</summary>
public interface IConfigWorksheetAppService : IApplicationService
{
    Task<ListResultDto<ConfigLineDto>> GetListAsync();

    Task<ConfigLineDto> CreateAsync(CreateConfigLineDto input);

    Task<ConfigLineDto> UpdateAsync(Guid id, UpdateConfigLineDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Adds an area heading and a line for every table not on the worksheet yet.</summary>
    Task<ListResultDto<ConfigLineDto>> SuggestLinesAsync();

    Task<ListResultDto<ConfigLineDto>> MoveAsync(Guid id, MoveConfigLineInput input);
}

/// <summary>
/// The import wizard. Mirrors Odoo's import: upload a CSV or Excel file, match its columns to
/// fields, test, then import — all or nothing. Routed under /api/erp/data-import.
/// </summary>
public interface IDataImportAppService : IApplicationService
{
    /// <summary>Tables the caller may import into.</summary>
    Task<ListResultDto<ConfigTableInfoDto>> GetEntitiesAsync();

    /// <summary>Reads the file and proposes a field for each column.</summary>
    Task<ImportFilePreviewDto> ParseFileAsync(ImportFileInput input);

    /// <summary>Checks every row as the import would, and writes nothing.</summary>
    Task<ImportResultDto> TestImportAsync(RunImportInput input);

    /// <summary>Imports every row, or none when any row has an error.</summary>
    Task<ImportResultDto> RunImportAsync(RunImportInput input);

    /// <summary>An empty workbook with a column for each field. GET /api/erp/data-import/template-file?entityName=Customer.</summary>
    Task<IRemoteStreamContent> GetTemplateFileAsync(string entityName);
}
