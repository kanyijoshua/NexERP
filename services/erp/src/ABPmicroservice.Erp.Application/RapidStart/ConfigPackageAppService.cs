using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// Configuration packages: design, fill,
/// export, import, validate and apply.
/// </summary>
[Authorize(ErpPermissions.RapidStart.Default)]
public class ConfigPackageAppService : ErpAppService, IConfigPackageAppService
{
    private readonly IRepository<ConfigPackage, Guid> _packageRepository;
    private readonly IRepository<ConfigPackageRecord, Guid> _recordRepository;
    private readonly IRepository<ConfigPackageError, Guid> _errorRepository;
    private readonly ConfigPackageManager _packageManager;
    private readonly ConfigTableRegistry _tables;
    private readonly RapidStartAccess _access;

    public ConfigPackageAppService(
        IRepository<ConfigPackage, Guid> packageRepository,
        IRepository<ConfigPackageRecord, Guid> recordRepository,
        IRepository<ConfigPackageError, Guid> errorRepository,
        ConfigPackageManager packageManager,
        ConfigTableRegistry tables,
        RapidStartAccess access
    )
    {
        _packageRepository = packageRepository;
        _recordRepository = recordRepository;
        _errorRepository = errorRepository;
        _packageManager = packageManager;
        _tables = tables;
        _access = access;
    }

    public async Task<ListResultDto<ConfigPackageDto>> GetListAsync(GetConfigPackagesInput input)
    {
        var filter = input?.Filter?.Trim().ToLowerInvariant();

        var packages = (await _packageRepository.GetListAsync(includeDetails: true))
            .Where(p => filter.IsNullOrEmpty() || p.Code.ToLowerInvariant().Contains(filter) || p.PackageName.ToLowerInvariant().Contains(filter))
            .OrderBy(p => p.Code)
            .Select(p => Fill(new ConfigPackageDto(), p))
            .ToList();

        return new ListResultDto<ConfigPackageDto>(packages);
    }

    public async Task<ConfigPackageDetailDto> GetAsync(Guid id)
    {
        return ToDetail(await GetPackageAsync(id));
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task<ConfigPackageDetailDto> CreateAsync(CreateConfigPackageDto input)
    {
        var package = await _packageManager.CreateAsync(input.Code, input.PackageName, input.ProductVersion);
        await _packageRepository.InsertAsync(package, autoSave: true);
        return ToDetail(package);
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task<ConfigPackageDetailDto> UpdateAsync(Guid id, UpdateConfigPackageDto input)
    {
        var package = await GetPackageAsync(id);
        package.Update(input.PackageName, input.ProductVersion);
        await _packageRepository.UpdateAsync(package, autoSave: true);
        return ToDetail(package);
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _packageManager.DeleteAsync(await GetPackageAsync(id));
    }

    public async Task<ListResultDto<ConfigTableInfoDto>> GetTableCatalogAsync()
    {
        return new ListResultDto<ConfigTableInfoDto>(await _access.GetCatalogAsync());
    }

    public async Task<ListResultDto<ConfigFieldInfoDto>> GetTableFieldsAsync(string entityName)
    {
        var profile = await _access.CheckReadAsync(entityName);
        return new ListResultDto<ConfigFieldInfoDto>(RapidStartAccess.DescribeFields(profile));
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task<ConfigPackageDetailDto> IncludeTablesAsync(Guid id, IncludeConfigTablesInput input)
    {
        var package = await GetPackageAsync(id);

        var names = input.IncludeRelatedTables ? _tables.GetRelatedTables(input.EntityNames) : input.EntityNames;
        foreach (var name in names)
        {
            await _access.CheckReadAsync(name);
        }

        _packageManager.IncludeTables(package, names, includeRelated: false);
        await _packageRepository.UpdateAsync(package, autoSave: true);
        return ToDetail(package);
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task<ConfigPackageDetailDto> ExcludeTableAsync(Guid id, ConfigPackageTableInput input)
    {
        var package = await GetPackageAsync(id);
        await _packageManager.RemoveTableAsync(package, input.TableId);
        await _packageRepository.UpdateAsync(package, autoSave: true);
        return ToDetail(package);
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task<ConfigPackageDetailDto> UpdateTableAsync(Guid id, UpdateConfigPackageTableDto input)
    {
        var package = await GetPackageAsync(id);
        var table = package.GetTable(input.TableId);
        var profile = _tables.Get(table.EntityName);

        var filters = ExportFieldMapper.ToFilters(input.Filters);
        foreach (var filter in filters)
        {
            profile.Definition.GetField(filter.Field);
        }

        table.SetOptions(input.ProcessingOrder, input.DeleteRecordsBeforeProcessing, input.DataTemplateCode);
        table.SetFilters(filters);

        foreach (var fieldInput in input.Fields ?? [])
        {
            var field = table.FindField(fieldInput.FieldName)
                ?? throw new BusinessException(ErpErrorCodes.RapidStart.FieldNotImportable)
                    .WithData("fieldName", fieldInput.FieldName)
                    .WithData("entityName", table.EntityName);

            field.Set(fieldInput.IncludeField, fieldInput.ValidateField, fieldInput.ProcessingOrder);
            field.SetMappings(
                (fieldInput.Mappings ?? []).Select(m => new ConfigFieldMapping { OldValue = m.OldValue?.Trim(), NewValue = m.NewValue })
            );
        }

        await _packageRepository.UpdateAsync(package, autoSave: true);
        return ToDetail(package);
    }

    [Authorize(ErpPermissions.RapidStart.Import)]
    public async Task<ConfigPackageDetailDto> FillFromDatabaseAsync(Guid id, ConfigPackageTablesInput input)
    {
        var package = await GetPackageAsync(id);
        var tables = await SelectTablesAsync(package, input, write: false);

        await _packageManager.FillFromDatabaseAsync(package, tables);
        await _packageRepository.UpdateAsync(package, autoSave: true);
        return ToDetail(package);
    }

    [Authorize(ErpPermissions.RapidStart.Apply)]
    public async Task<ConfigPackageRunResultDto> ValidatePackageAsync(Guid id, ConfigPackageTablesInput input)
    {
        var package = await GetPackageAsync(id);
        var tables = await SelectTablesAsync(package, input, write: false);

        var results = await _packageManager.ValidateAsync(package, tables);
        await _packageRepository.UpdateAsync(package, autoSave: true);
        return ToRunResult(results);
    }

    [Authorize(ErpPermissions.RapidStart.Apply)]
    public async Task<ConfigPackageRunResultDto> ApplyPackageAsync(Guid id, ConfigPackageTablesInput input)
    {
        var package = await GetPackageAsync(id);
        var tables = await SelectTablesAsync(package, input, write: true);

        var results = await _packageManager.ApplyAsync(package, tables);
        await _packageRepository.UpdateAsync(package, autoSave: true);
        return ToRunResult(results);
    }

    [Authorize(ErpPermissions.RapidStart.Import)]
    public async Task<ConfigPackageDetailDto> ClearPackageDataAsync(Guid id, ConfigPackageTablesInput input)
    {
        var package = await GetPackageAsync(id);
        await _packageManager.ClearDataAsync(package, Select(package, input));
        await _packageRepository.UpdateAsync(package, autoSave: true);
        return ToDetail(package);
    }

    [Authorize(ErpPermissions.RapidStart.Export)]
    public async Task<IRemoteStreamContent> ExportPackageAsync(Guid id, ExportConfigPackageInput input)
    {
        var package = await GetPackageAsync(id);
        foreach (var table in package.Tables)
        {
            await _access.CheckReadAsync(table.EntityName);
        }

        var file = await _packageManager.ExportAsync(package, input.Format);
        return new RemoteStreamContent(new MemoryStream(file.Content), file.FileName, file.ContentType);
    }

    [Authorize(ErpPermissions.RapidStart.Import)]
    public async Task<ConfigPackageDetailDto> ImportPackageAsync(UploadFileInput input)
    {
        var package = await _packageManager.ImportPackageAsync(RapidStartAccess.Decode(input));
        return ToDetail(await GetPackageAsync(package.Id));
    }

    [Authorize(ErpPermissions.RapidStart.Import)]
    public async Task<ImportConfigDataResultDto> ImportDataAsync(Guid id, UploadFileInput input)
    {
        var package = await GetPackageAsync(id);
        var count = await _packageManager.ImportDataAsync(package, input.FileName, RapidStartAccess.Decode(input));
        await _packageRepository.UpdateAsync(package, autoSave: true);

        return new ImportConfigDataResultDto { NoOfRecords = count };
    }

    public async Task<PagedResultDto<ConfigPackageRecordDto>> GetRecordsAsync(GetConfigPackageRecordsInput input)
    {
        var package = await GetPackageAsync(input.PackageId);
        var table = package.GetTable(input.TableId);

        var queryable = (await _recordRepository.GetQueryableAsync())
            .Where(r => r.ConfigPackageId == package.Id && r.ConfigPackageTableId == table.Id)
            .WhereIf(input.ErrorsOnly, r => r.Invalid);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var records = await AsyncExecuter.ToListAsync(
            queryable.OrderBy(r => r.RecordNo).Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        var recordIds = records.Select(r => r.Id).ToList();
        var errors = (await _errorRepository.GetListAsync(e => recordIds.Contains(e.ConfigPackageRecordId)))
            .ToLookup(e => e.ConfigPackageRecordId);

        return new PagedResultDto<ConfigPackageRecordDto>(
            totalCount,
            records.Select(r => ToDto(r, errors[r.Id], table)).ToList()
        );
    }

    [Authorize(ErpPermissions.RapidStart.Import)]
    public async Task<ConfigPackageRecordDto> UpdateRecordAsync(Guid id, UpdateConfigPackageRecordDto input)
    {
        var record = await _recordRepository.GetAsync(id);
        var package = await GetPackageAsync(record.ConfigPackageId);
        var table = package.GetTable(record.ConfigPackageTableId);
        var profile = _tables.Get(table.EntityName);

        var values = record.GetValues();
        foreach (var (fieldName, value) in input.Values ?? new Dictionary<string, string>())
        {
            var field = profile.FindImportableField(fieldName)
                ?? throw new BusinessException(ErpErrorCodes.RapidStart.FieldNotImportable)
                    .WithData("fieldName", fieldName)
                    .WithData("entityName", profile.Name);

            values[field.Name] = value;
        }

        record.SetValues(values);

        // The errors described the old values; the next validation says whether the new ones pass.
        record.MarkInvalid(false);
        await _errorRepository.DeleteDirectAsync(e => e.ConfigPackageRecordId == record.Id);
        await _recordRepository.UpdateAsync(record, autoSave: true);

        await RecountAsync(package, table);
        return ToDto(record, [], table);
    }

    [Authorize(ErpPermissions.RapidStart.Import)]
    public async Task DeleteRecordAsync(Guid id)
    {
        var record = await _recordRepository.GetAsync(id);
        var package = await GetPackageAsync(record.ConfigPackageId);
        var table = package.GetTable(record.ConfigPackageTableId);

        await _errorRepository.DeleteDirectAsync(e => e.ConfigPackageRecordId == record.Id);
        await _recordRepository.DeleteAsync(record, autoSave: true);

        await RecountAsync(package, table);
    }

    public async Task<ListResultDto<ConfigPackageErrorDto>> GetErrorsAsync(Guid id)
    {
        var package = await GetPackageAsync(id);
        var tables = package.Tables.ToDictionary(t => t.Id);

        var errors = (await _errorRepository.GetListAsync(e => e.ConfigPackageId == package.Id))
            .OrderBy(e => tables.TryGetValue(e.ConfigPackageTableId, out var t) ? t.ProcessingOrder : int.MaxValue)
            .ThenBy(e => e.RecordNo)
            .Select(e => ToDto(e, tables.GetValueOrDefault(e.ConfigPackageTableId)))
            .ToList();

        return new ListResultDto<ConfigPackageErrorDto>(errors);
    }

    private async Task<ConfigPackage> GetPackageAsync(Guid id)
    {
        return await _packageRepository.GetAsync(id, includeDetails: true);
    }

    private static List<ConfigPackageTable> Select(ConfigPackage package, ConfigPackageTablesInput input)
    {
        var ids = input?.TableIds ?? [];
        return ids.Count == 0 ? package.Tables.ToList() : ids.Select(package.GetTable).ToList();
    }

    /// <summary>The tables to act on, once the caller is known to hold the table permissions for them.</summary>
    private async Task<List<ConfigPackageTable>> SelectTablesAsync(ConfigPackage package, ConfigPackageTablesInput input, bool write)
    {
        var tables = Select(package, input);

        foreach (var table in tables)
        {
            if (write)
            {
                await _access.CheckWriteAsync(table.EntityName);
            }
            else
            {
                await _access.CheckReadAsync(table.EntityName);
            }
        }

        return tables;
    }

    private async Task RecountAsync(ConfigPackage package, ConfigPackageTable table)
    {
        var records = await _recordRepository.CountAsync(r => r.ConfigPackageId == package.Id && r.ConfigPackageTableId == table.Id);
        var invalid = await _recordRepository.CountAsync(r => r.ConfigPackageId == package.Id && r.ConfigPackageTableId == table.Id && r.Invalid);

        table.SetCounts(records, invalid);
        await _packageRepository.UpdateAsync(package, autoSave: true);
    }

    private static T Fill<T>(T dto, ConfigPackage package)
        where T : ConfigPackageDto
    {
        dto.Id = package.Id;
        dto.Code = package.Code;
        dto.PackageName = package.PackageName;
        dto.ProductVersion = package.ProductVersion;
        dto.LastImportedTime = package.LastImportedTime;
        dto.LastAppliedTime = package.LastAppliedTime;
        dto.NoOfTables = package.Tables.Count;
        dto.NoOfRecords = package.Tables.Sum(t => t.NoOfRecords);
        dto.NoOfErrors = package.Tables.Sum(t => t.NoOfErrors);
        return dto;
    }

    private ConfigPackageDetailDto ToDetail(ConfigPackage package)
    {
        var dto = Fill(new ConfigPackageDetailDto(), package);

        foreach (var table in package.TablesInProcessingOrder())
        {
            var profile = _tables.Find(table.EntityName);
            var fields = profile == null
                ? new Dictionary<string, ConfigFieldInfoDto>(StringComparer.OrdinalIgnoreCase)
                : RapidStartAccess.DescribeFields(profile).ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

            dto.Tables.Add(
                new ConfigPackageTableDto
                {
                    Id = table.Id,
                    EntityName = table.EntityName,
                    DisplayName = ErpEntityField.Humanize(table.EntityName),
                    ProcessingOrder = table.ProcessingOrder,
                    DeleteRecordsBeforeProcessing = table.DeleteRecordsBeforeProcessing,
                    DataTemplateCode = table.DataTemplateCode,
                    Filters = table
                        .GetFilters()
                        .Select(f => new EntityFilterDto { Field = f.Field, Operator = f.Operator, Value = f.Value })
                        .ToList(),
                    NoOfRecords = table.NoOfRecords,
                    NoOfErrors = table.NoOfErrors,
                    Fields = table
                        .Fields.OrderBy(f => f.ProcessingOrder)
                        .ThenBy(f => f.FieldName, StringComparer.Ordinal)
                        .Select(f =>
                        {
                            var info = fields.GetValueOrDefault(f.FieldName);
                            return new ConfigPackageFieldDto
                            {
                                Id = f.Id,
                                FieldName = f.FieldName,
                                DisplayName = info?.DisplayName ?? ErpEntityField.Humanize(f.FieldName),
                                DataType = info?.DataType,
                                IncludeField = f.IncludeField,
                                ValidateField = f.ValidateField,
                                ProcessingOrder = f.ProcessingOrder,
                                PrimaryKey = f.PrimaryKey,
                                RelatedTable = info?.RelatedTable,
                                Mappings = f
                                    .GetMappings()
                                    .Select(m => new ConfigFieldMappingDto { OldValue = m.OldValue, NewValue = m.NewValue })
                                    .ToList(),
                            };
                        })
                        .ToList(),
                }
            );
        }

        return dto;
    }

    private static ConfigPackageRecordDto ToDto(ConfigPackageRecord record, IEnumerable<ConfigPackageError> errors, ConfigPackageTable table)
    {
        return new ConfigPackageRecordDto
        {
            Id = record.Id,
            RecordNo = record.RecordNo,
            Values = record.GetValues(),
            Invalid = record.Invalid,
            Errors = errors.Select(e => ToDto(e, table)).ToList(),
        };
    }

    private static ConfigPackageErrorDto ToDto(ConfigPackageError error, ConfigPackageTable table)
    {
        return new ConfigPackageErrorDto
        {
            TableId = error.ConfigPackageTableId,
            EntityName = table?.EntityName,
            RecordId = error.ConfigPackageRecordId,
            RecordNo = error.RecordNo,
            FieldName = error.FieldName,
            ErrorText = error.ErrorText,
        };
    }

    private static ConfigPackageRunResultDto ToRunResult(IReadOnlyList<ConfigTableRunResult> results)
    {
        return new ConfigPackageRunResultDto
        {
            Tables = results
                .Select(r => new ConfigTableRunResultDto
                {
                    TableId = r.TableId,
                    EntityName = r.EntityName,
                    Inserted = r.Inserted,
                    Modified = r.Modified,
                    Deleted = r.Deleted,
                    Errors = r.Errors,
                })
                .ToList(),
            Inserted = results.Sum(r => r.Inserted),
            Modified = results.Sum(r => r.Modified),
            Errors = results.Sum(r => r.Errors),
        };
    }
}
