using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>What happened to one table when a package was validated or applied.</summary>
public sealed class ConfigTableRunResult
{
    public Guid TableId { get; init; }

    public string EntityName { get; init; }

    public int Inserted { get; init; }

    public int Modified { get; init; }

    public int Deleted { get; init; }

    public int Errors { get; init; }
}

/// <summary>
/// Everything a configuration package does:
/// collect tables, fill them from the database, export them, import them from a package file or an
/// Excel workbook, and validate and apply the staged records.
/// </summary>
public class ConfigPackageManager : DomainService
{
    private static readonly JsonSerializerOptions FileJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly IRepository<ConfigPackage, Guid> _packageRepository;
    private readonly IRepository<ConfigPackageRecord, Guid> _recordRepository;
    private readonly IRepository<ConfigPackageError, Guid> _errorRepository;
    private readonly IRepository<ConfigTemplate, Guid> _templateRepository;
    private readonly ConfigTableRegistry _tables;
    private readonly ConfigRecordApplier _applier;
    private readonly IEntityQueryExecutor _queryExecutor;

    public ConfigPackageManager(
        IRepository<ConfigPackage, Guid> packageRepository,
        IRepository<ConfigPackageRecord, Guid> recordRepository,
        IRepository<ConfigPackageError, Guid> errorRepository,
        IRepository<ConfigTemplate, Guid> templateRepository,
        ConfigTableRegistry tables,
        ConfigRecordApplier applier,
        IEntityQueryExecutor queryExecutor
    )
    {
        _packageRepository = packageRepository;
        _recordRepository = recordRepository;
        _errorRepository = errorRepository;
        _templateRepository = templateRepository;
        _tables = tables;
        _applier = applier;
        _queryExecutor = queryExecutor;
    }

    public async Task<ConfigPackage> CreateAsync(string code, string packageName, string productVersion)
    {
        await EnsureCodeIsFreeAsync(code);
        return new ConfigPackage(GuidGenerator.Create(), code, packageName, productVersion);
    }

    /// <summary>
    /// Adds tables, and with <paramref name="includeRelated"/> the tables they depend on and their
    /// line tables too. Processing order is then worked out afresh from the relations, so the
    /// package applies in a workable order without anyone having to number it.
    /// </summary>
    public IReadOnlyList<ConfigPackageTable> IncludeTables(ConfigPackage package, IEnumerable<string> entityNames, bool includeRelated)
    {
        var names = entityNames.Select(n => _tables.Get(n).Name).ToList();
        if (includeRelated)
        {
            names = _tables.GetRelatedTables(names).ToList();
        }

        var added = new List<ConfigPackageTable>();
        foreach (var name in names.Where(n => package.FindTable(n) == null))
        {
            added.Add(package.AddTable(GuidGenerator.Create(), _tables.Get(name), 0, GuidGenerator.Create));
        }

        RenumberProcessingOrder(package);
        return added;
    }

    public void RenumberProcessingOrder(ConfigPackage package)
    {
        var order = _tables.SortByDependencies(package.Tables.Select(t => t.EntityName));

        foreach (var table in package.Tables)
        {
            var position = order.ToList().FindIndex(n => string.Equals(n, table.EntityName, StringComparison.OrdinalIgnoreCase));
            table.SetOptions((position + 1) * 10, table.DeleteRecordsBeforeProcessing, table.DataTemplateCode);
        }
    }

    public async Task DeleteAsync(ConfigPackage package)
    {
        await ClearDataAsync(package, package.Tables.ToList());
        await _packageRepository.DeleteAsync(package, autoSave: true);
    }

    public async Task RemoveTableAsync(ConfigPackage package, Guid tableId)
    {
        await ClearDataAsync(package, [package.GetTable(tableId)]);
        package.RemoveTable(tableId);
    }

    /// <summary>
    /// Replaces the staged records of the tables with what the database holds now. "Get Data
    /// from Database": the usual way to build a package from a company that is already set up.
    /// </summary>
    public async Task<int> FillFromDatabaseAsync(ConfigPackage package, IReadOnlyList<ConfigPackageTable> tables)
    {
        var total = 0;

        foreach (var table in tables)
        {
            var rows = await ReadDatabaseRowsAsync(table);
            await StageAsync(package, table, rows);
            total += rows.Count;
        }

        return total;
    }

    /// <summary>
    /// Writes the package with the current data of its tables. JSON carries the definition too
    /// and can be imported into another company or system as a whole package; Excel carries the
    /// data only, one sheet per table, to be edited and imported back into the package.
    /// </summary>
    public async Task<ExportFile> ExportAsync(ConfigPackage package, ConfigPackageFileFormat format)
    {
        var tables = package.TablesInProcessingOrder();
        var name = DataExportEngine.Sanitize(package.Code);

        if (format == ConfigPackageFileFormat.Xlsx)
        {
            var sheets = new List<SpreadsheetSheet>();
            foreach (var table in tables)
            {
                var fields = table.IncludedFields().Select(f => f.FieldName).ToList();
                var rows = await ReadDatabaseRowsAsync(table);
                sheets.Add(
                    new SpreadsheetSheet(
                        table.EntityName,
                        fields,
                        rows.Select(r => (IReadOnlyList<object>)fields.Select(f => (object)r.GetValueOrDefault(f)).ToList())
                    )
                );
            }

            return new ExportFile(
                SpreadsheetWriter.Write(sheets),
                $"{name}.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            );
        }

        var file = new ConfigPackageFile
        {
            Code = package.Code,
            PackageName = package.PackageName,
            ProductVersion = package.ProductVersion,
        };

        foreach (var table in tables)
        {
            file.Tables.Add(
                new ConfigPackageFileTable
                {
                    EntityName = table.EntityName,
                    ProcessingOrder = table.ProcessingOrder,
                    DeleteRecordsBeforeProcessing = table.DeleteRecordsBeforeProcessing,
                    DataTemplateCode = table.DataTemplateCode,
                    Filters = table.GetFilters(),
                    Fields = table
                        .Fields.OrderBy(f => f.ProcessingOrder)
                        .Select(f => new ConfigPackageFileField
                        {
                            Name = f.FieldName,
                            Include = f.IncludeField,
                            Validate = f.ValidateField,
                            ProcessingOrder = f.ProcessingOrder,
                            Mappings = f.GetMappings(),
                        })
                        .ToList(),
                    Records = await ReadDatabaseRowsAsync(table),
                }
            );
        }

        // Templates travel with the package, or a table would arrive naming a template the
        // receiving company does not have.
        var templateCodes = tables.Select(t => t.DataTemplateCode).Where(c => c != null).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (templateCodes.Count > 0)
        {
            var templates = await _templateRepository.GetListAsync(includeDetails: true);
            file.Templates = templates
                .Where(t => templateCodes.Contains(t.Code))
                .Select(t => new ConfigPackageFileTemplate
                {
                    Code = t.Code,
                    Description = t.Description,
                    EntityName = t.EntityName,
                    Enabled = t.Enabled,
                    Lines = t
                        .Lines.Select(l => new ConfigPackageFileTemplateLine
                        {
                            FieldName = l.FieldName,
                            DefaultValue = l.DefaultValue,
                            Mandatory = l.Mandatory,
                        })
                        .ToList(),
                })
                .ToList();
        }

        return new ExportFile(JsonSerializer.SerializeToUtf8Bytes(file, FileJsonOptions), $"{name}.json", "application/json");
    }

    /// <summary>
    /// Reads a package file. A package with the same code is replaced — definition, templates and
    /// staged data; otherwise a new package is made. The
    /// records are only staged: they reach the tables when the package is applied.
    /// </summary>
    public async Task<ConfigPackage> ImportPackageAsync(byte[] content)
    {
        ImportFileReader.EnsureSize(content);
        var file = ParsePackageFile(content);

        // Every table is checked before anything changes, so a bad file changes nothing.
        foreach (var fileTable in file.Tables)
        {
            var profile = _tables.Get(fileTable.EntityName);
            foreach (var field in fileTable.Fields.Where(f => profile.FindImportableField(f.Name) == null))
            {
                throw new BusinessException(ErpErrorCodes.RapidStart.FieldNotImportable)
                    .WithData("fieldName", field.Name)
                    .WithData("entityName", profile.Name);
            }

            if (fileTable.Records.Count > ErpDomainConsts.MaxExportRowCount)
            {
                throw new BusinessException(ErpErrorCodes.Exporting.TooManyRows)
                    .WithData("totalCount", fileTable.Records.Count)
                    .WithData("maxRowCount", ErpDomainConsts.MaxExportRowCount);
            }
        }

        var code = file.Code.Trim().ToUpperInvariant();
        var package = await _packageRepository.FirstOrDefaultAsync(p => p.Code == code);
        if (package == null)
        {
            package = new ConfigPackage(GuidGenerator.Create(), file.Code, file.PackageName.IsNullOrWhiteSpace() ? file.Code : file.PackageName, file.ProductVersion);
            await _packageRepository.InsertAsync(package, autoSave: true);
        }
        else
        {
            package = await _packageRepository.GetAsync(package.Id, includeDetails: true);
            await ClearDataAsync(package, package.Tables.ToList());
            package.Tables.Clear();
            package.Update(file.PackageName.IsNullOrWhiteSpace() ? package.PackageName : file.PackageName, file.ProductVersion);
        }

        foreach (var fileTable in file.Tables)
        {
            var profile = _tables.Get(fileTable.EntityName);
            var table = package.AddTable(GuidGenerator.Create(), profile, fileTable.ProcessingOrder, GuidGenerator.Create);
            table.SetOptions(fileTable.ProcessingOrder, fileTable.DeleteRecordsBeforeProcessing, fileTable.DataTemplateCode);
            table.SetFilters(fileTable.Filters);

            if (fileTable.Fields.Count > 0)
            {
                // A field the file leaves out is one the package did not carry.
                foreach (var field in table.Fields)
                {
                    var fileField = fileTable.Fields.FirstOrDefault(f => string.Equals(f.Name, field.FieldName, StringComparison.OrdinalIgnoreCase));
                    field.Set(fileField?.Include ?? false, fileField?.Validate ?? true, fileField?.ProcessingOrder ?? field.ProcessingOrder);
                    field.SetMappings(fileField?.Mappings);
                }
            }
        }

        await ImportTemplatesAsync(file.Templates);
        package.MarkImported(Clock.Now);
        await _packageRepository.UpdateAsync(package, autoSave: true);

        foreach (var fileTable in file.Tables)
        {
            await StageAsync(package, package.FindTable(fileTable.EntityName), fileTable.Records);
        }

        return package;
    }

    /// <summary>
    /// Reads data for the package's tables from an Excel workbook, one sheet per table.
    /// Sheets that match no table of the package are ignored; a matched
    /// table's staged records are replaced.
    /// </summary>
    public async Task<int> ImportDataAsync(ConfigPackage package, string fileName, byte[] content)
    {
        var sheets = ImportFileReader.Read(fileName, content);
        var total = 0;

        foreach (var table in package.Tables)
        {
            var profile = _tables.Get(table.EntityName);
            var sheet = sheets.FirstOrDefault(s => ImportFileReader.SheetIsFor(s, profile))
                // A workbook with a single sheet for a single-table package needs no matching name.
                ?? (sheets.Count == 1 && package.Tables.Count == 1 ? sheets[0] : null);

            if (sheet == null || sheet.Rows.Count == 0)
            {
                continue;
            }

            var columns = sheet.Rows[0].Select(header => ImportFileReader.MatchField(profile, header)).ToList();
            var records = ImportFileReader.ToRecords(sheet.Rows.Skip(1).ToList(), columns);

            await StageAsync(package, table, records);
            total += records.Count;
        }

        if (total == 0)
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.FileHasNoRows);
        }

        package.MarkImported(Clock.Now);
        return total;
    }

    public Task<IReadOnlyList<ConfigTableRunResult>> ValidateAsync(ConfigPackage package, IReadOnlyList<ConfigPackageTable> tables)
    {
        return RunAsync(package, tables, dryRun: true);
    }

    public async Task<IReadOnlyList<ConfigTableRunResult>> ApplyAsync(ConfigPackage package, IReadOnlyList<ConfigPackageTable> tables)
    {
        var results = await RunAsync(package, tables, dryRun: false);
        package.MarkApplied(Clock.Now);
        return results;
    }

    public async Task ClearDataAsync(ConfigPackage package, IReadOnlyList<ConfigPackageTable> tables)
    {
        var tableIds = tables.Select(t => t.Id).ToList();

        await _errorRepository.DeleteDirectAsync(e => e.ConfigPackageId == package.Id && tableIds.Contains(e.ConfigPackageTableId));
        await _recordRepository.DeleteDirectAsync(r => r.ConfigPackageId == package.Id && tableIds.Contains(r.ConfigPackageTableId));

        foreach (var table in tables)
        {
            table.SetCounts(0, 0);
        }
    }

    /// <summary>
    /// Validates or applies the tables in processing order, writing each record's errors next to
    /// it. One run shares its view of which keys exist, so a table may rely on records an earlier
    /// table of the same package creates — also in a dry run, where they are only pretended.
    /// </summary>
    private async Task<IReadOnlyList<ConfigTableRunResult>> RunAsync(ConfigPackage package, IReadOnlyList<ConfigPackageTable> tables, bool dryRun)
    {
        var context = new ConfigApplyContext();
        var results = new List<ConfigTableRunResult>();
        var templates = await _templateRepository.GetListAsync(includeDetails: true);

        foreach (var table in tables.OrderBy(t => t.ProcessingOrder).ThenBy(t => t.EntityName, StringComparer.Ordinal))
        {
            var profile = _tables.Get(table.EntityName);
            var fields = table.IncludedFields();

            var request = new ConfigApplyRequest(profile, fields.Select(f => f.FieldName))
            {
                DryRun = dryRun,
                ValidatedFields = fields.Where(f => f.ValidateField).Select(f => f.FieldName).ToHashSet(StringComparer.OrdinalIgnoreCase),
                Mappings = fields
                    .Select(f => (f.FieldName, Mappings: f.GetMappings()))
                    .Where(m => m.Mappings.Count > 0)
                    .ToDictionary(m => m.FieldName, m => m.Mappings, StringComparer.OrdinalIgnoreCase),
                Template = table.DataTemplateCode == null
                    ? null
                    : templates.FirstOrDefault(t =>
                        string.Equals(t.Code, table.DataTemplateCode, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(t.EntityName, profile.Name, StringComparison.OrdinalIgnoreCase)
                    ),
            };

            var records = (await _recordRepository.GetListAsync(r => r.ConfigPackageId == package.Id && r.ConfigPackageTableId == table.Id))
                .OrderBy(r => r.RecordNo)
                .ToList();

            var deleted = 0;
            if (!dryRun && table.DeleteRecordsBeforeProcessing && records.Count > 0)
            {
                deleted = await _applier.DeleteAllAsync(profile);
            }

            var result = await _applier.ApplyAsync(
                request,
                records.Select(r => new ConfigRecordInput(r.RecordNo, r.GetValues(), r.Id)).ToList(),
                context
            );

            await WriteErrorsAsync(package, table, records, result);

            results.Add(
                new ConfigTableRunResult
                {
                    TableId = table.Id,
                    EntityName = table.EntityName,
                    Inserted = result.Inserted,
                    Modified = result.Modified,
                    Deleted = deleted,
                    Errors = result.FailedRecords,
                }
            );
        }

        return results;
    }

    private async Task WriteErrorsAsync(ConfigPackage package, ConfigPackageTable table, List<ConfigPackageRecord> records, ConfigApplyResult result)
    {
        await _errorRepository.DeleteDirectAsync(e => e.ConfigPackageId == package.Id && e.ConfigPackageTableId == table.Id);

        var failed = result.Errors.Where(e => e.RecordId.HasValue).Select(e => e.RecordId!.Value).ToHashSet();
        foreach (var record in records)
        {
            record.MarkInvalid(failed.Contains(record.Id));
        }

        await _recordRepository.UpdateManyAsync(records, autoSave: true);

        await _errorRepository.InsertManyAsync(
            result.Errors.Select(e => new ConfigPackageError(
                GuidGenerator.Create(),
                package.Id,
                table.Id,
                e.RecordId ?? Guid.Empty,
                e.RecordNo,
                e.FieldName,
                e.Message
            )),
            autoSave: true
        );

        table.SetCounts(records.Count, failed.Count);
    }

    private async Task StageAsync(ConfigPackage package, ConfigPackageTable table, IReadOnlyList<Dictionary<string, string>> rows)
    {
        await ClearDataAsync(package, [table]);

        var recordNo = 0;
        var records = rows.Select(values => new ConfigPackageRecord(GuidGenerator.Create(), package.Id, table.Id, ++recordNo, values)).ToList();

        await _recordRepository.InsertManyAsync(records, autoSave: true);
        table.SetCounts(records.Count, 0);
    }

    /// <summary>
    /// The table's current records, filtered as the package table says, as text. A field that
    /// holds another record's id is written as that record's key, so the package means the same
    /// thing in any company; a line whose header is not in this company is left out.
    /// </summary>
    private async Task<List<Dictionary<string, string>>> ReadDatabaseRowsAsync(ConfigPackageTable table)
    {
        var profile = _tables.Get(table.EntityName);
        var fields = table.IncludedFields().Select(f => f.FieldName).ToList();

        var result = await _queryExecutor.QueryAsync(
            new EntityQueryRequest
            {
                EntityName = profile.Name,
                Fields = fields,
                Filters = table.GetFilters(),
                OrderBy = profile.KeyFields.FirstOrDefault(k => profile.FindRelation(k) is not { IsIdReference: true }),
                MaxResultCount = ErpDomainConsts.MaxExportRowCount,
            }
        );

        if (result.TotalCount > result.Items.Count)
        {
            throw new BusinessException(ErpErrorCodes.Exporting.TooManyRows)
                .WithData("totalCount", result.TotalCount)
                .WithData("maxRowCount", result.Items.Count);
        }

        var context = new ConfigApplyContext();
        var idToKey = new Dictionary<string, Dictionary<Guid, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var relation in profile.Relations.Where(r => r.IsIdReference && fields.Contains(r.FieldName, StringComparer.OrdinalIgnoreCase)))
        {
            var keys = await _applier.GetKeysAsync(relation.TargetEntity, relation.TargetKeyField, context);
            idToKey[relation.FieldName] = keys.GroupBy(p => p.Value).ToDictionary(g => g.Key, g => g.First().Key);
        }

        var rows = new List<Dictionary<string, string>>();
        foreach (var item in result.Items)
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var belongsHere = true;

            foreach (var field in fields)
            {
                var value = item.GetValueOrDefault(field);

                if (idToKey.TryGetValue(field, out var keys) && value is Guid id)
                {
                    if (keys.TryGetValue(id, out var key))
                    {
                        row[field] = key;
                    }
                    else if (profile.IsKeyField(field))
                    {
                        // The header belongs to another company: unscoped line tables show every company's lines.
                        belongsHere = false;
                    }

                    continue;
                }

                row[field] = ConfigValueConverter.ToText(value);
            }

            if (belongsHere)
            {
                rows.Add(row);
            }
        }

        return rows;
    }

    private async Task ImportTemplatesAsync(List<ConfigPackageFileTemplate> templates)
    {
        foreach (var fileTemplate in templates ?? [])
        {
            if (fileTemplate.Code.IsNullOrWhiteSpace())
            {
                continue;
            }

            var profile = _tables.Get(fileTemplate.EntityName);
            var code = fileTemplate.Code.Trim().ToUpperInvariant();
            var template = await _templateRepository.FirstOrDefaultAsync(t => t.Code == code);

            if (template == null)
            {
                template = new ConfigTemplate(GuidGenerator.Create(), code, profile.Name, fileTemplate.Description);
                await _templateRepository.InsertAsync(template, autoSave: true);
            }

            template = await _templateRepository.GetAsync(template.Id, includeDetails: true);
            template.Update(fileTemplate.Description, fileTemplate.Enabled);
            template.SetLines(
                fileTemplate.Lines.Where(l => profile.FindImportableField(l.FieldName) != null).Select(l => (l.FieldName, l.DefaultValue, l.Mandatory)),
                GuidGenerator.Create
            );

            await _templateRepository.UpdateAsync(template, autoSave: true);
        }
    }

    private static ConfigPackageFile ParsePackageFile(byte[] content)
    {
        ConfigPackageFile file;
        try
        {
            file = JsonSerializer.Deserialize<ConfigPackageFile>(DelimitedReader.Decode(content), FileJsonOptions);
        }
        catch (JsonException)
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.FileNotValid);
        }

        if (file == null || file.Format != ConfigPackageFile.FormatName || file.Code.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.FileNotValid);
        }

        file.Tables ??= [];
        foreach (var table in file.Tables)
        {
            table.Fields ??= [];
            table.Records ??= [];
            table.Filters ??= [];
        }

        return file;
    }

    private async Task EnsureCodeIsFreeAsync(string code)
    {
        var normalized = code?.Trim().ToUpperInvariant();
        if (await _packageRepository.AnyAsync(p => p.Code == normalized))
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.PackageCodeAlreadyExists).WithData("code", normalized);
        }
    }
}
