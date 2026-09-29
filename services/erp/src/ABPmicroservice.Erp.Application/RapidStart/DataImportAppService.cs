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
/// The import wizard, after Odoo's: read a CSV or Excel file, match its columns to fields (by the
/// field's name or its label, as Odoo does), test, then import.
/// <para>
/// Unlike a configuration package, which applies the records it can and keeps the rest for
/// correction as BC does, the wizard is all or nothing, as Odoo's import is: a file with one bad
/// row writes no rows, so a half-imported file never has to be untangled.
/// </para>
/// </summary>
[Authorize(ErpPermissions.RapidStart.Apply)]
public class DataImportAppService : ErpAppService, IDataImportAppService
{
    private const int SampleRowCount = 10;

    private readonly ConfigTableRegistry _tables;
    private readonly ConfigRecordApplier _applier;
    private readonly RapidStartAccess _access;
    private readonly IRepository<ConfigTemplate, Guid> _templateRepository;

    public DataImportAppService(
        ConfigTableRegistry tables,
        ConfigRecordApplier applier,
        RapidStartAccess access,
        IRepository<ConfigTemplate, Guid> templateRepository
    )
    {
        _tables = tables;
        _applier = applier;
        _access = access;
        _templateRepository = templateRepository;
    }

    public async Task<ListResultDto<ConfigTableInfoDto>> GetEntitiesAsync()
    {
        return new ListResultDto<ConfigTableInfoDto>((await _access.GetCatalogAsync()).Where(t => t.CanWrite).ToList());
    }

    public async Task<ImportFilePreviewDto> ParseFileAsync(ImportFileInput input)
    {
        var profile = input.EntityName.IsNullOrWhiteSpace() ? null : await _access.CheckReadAsync(input.EntityName);
        var (sheets, sheet) = ReadSheet(input);
        var (headers, rows) = SplitHeader(sheet, input.HasHeaders);

        // A field is proposed for one column only: the first that names it.
        var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var columns = headers
            .Select((header, index) =>
            {
                var field = profile == null || !input.HasHeaders ? null : ImportFileReader.MatchField(profile, header);
                return new ImportColumnDto
                {
                    Index = index,
                    Header = header,
                    FieldName = field != null && taken.Add(field) ? field : null,
                };
            })
            .ToList();

        return new ImportFilePreviewDto
        {
            Sheets = sheets.Select(s => s.Name).ToList(),
            SheetName = sheet.Name,
            Columns = columns,
            SampleRows = rows.Take(SampleRowCount).ToList(),
            TotalRows = rows.Count,
        };
    }

    public Task<ImportResultDto> TestImportAsync(RunImportInput input)
    {
        return ImportAsync(input, test: true);
    }

    public Task<ImportResultDto> RunImportAsync(RunImportInput input)
    {
        return ImportAsync(input, test: false);
    }

    public async Task<IRemoteStreamContent> GetTemplateFileAsync(string entityName)
    {
        var profile = await _access.CheckReadAsync(entityName);
        var headers = profile.ImportableFields.Select(f => f.Name).ToList();

        var content = SpreadsheetWriter.Write(profile.Name, headers, []);
        return new RemoteStreamContent(
            new MemoryStream(content),
            $"{DataExportEngine.Sanitize(profile.Name)}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }

    private async Task<ImportResultDto> ImportAsync(RunImportInput input, bool test)
    {
        var profile = await _access.CheckWriteAsync(input.EntityName);
        var mapping = CheckMapping(profile, input.Columns);

        var (_, sheet) = ReadSheet(input);
        var (_, rows) = SplitHeader(sheet, input.HasHeaders);

        if (rows.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.FileHasNoRows);
        }

        if (rows.Count > ErpDomainConsts.MaxExportRowCount)
        {
            throw new BusinessException(ErpErrorCodes.Exporting.TooManyRows)
                .WithData("totalCount", rows.Count)
                .WithData("maxRowCount", ErpDomainConsts.MaxExportRowCount);
        }

        // Row numbers as the spreadsheet shows them, so an error can be found in the file.
        var firstRowNo = input.HasHeaders ? 2 : 1;
        var records = rows.Select(
                (row, index) =>
                    new ConfigRecordInput(
                        firstRowNo + index,
                        mapping.ToDictionary(m => m.FieldName, m => m.Index < row.Count ? row[m.Index] : string.Empty, StringComparer.OrdinalIgnoreCase)
                    )
            )
            .ToList();

        var template = await FindTemplateAsync(profile, input.DataTemplateCode);
        var fields = mapping.Select(m => m.FieldName).ToList();

        // Every row is checked first. Only a clean file goes on to be written.
        var check = await _applier.ApplyAsync(new ConfigApplyRequest(profile, fields) { DryRun = true, Template = template }, records);

        if (test || check.Errors.Count > 0)
        {
            return ToResult(check, records.Count, dryRun: true);
        }

        var result = await _applier.ApplyAsync(new ConfigApplyRequest(profile, fields) { Template = template }, records);

        // The check passed, so this is not expected; if it happens the unit of work must not commit.
        if (result.Errors.Count > 0)
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.ImportHasErrors).WithData("count", result.FailedRecords);
        }

        return ToResult(result, records.Count, dryRun: false);
    }

    /// <summary>
    /// The columns to import. Each field once, only fields the table accepts, and the key — the
    /// fields that say which record a row is — always, or every row would be a new record with no
    /// way to tell it from the next.
    /// </summary>
    private static List<ImportColumnDto> CheckMapping(ConfigTableProfile profile, List<ImportColumnDto> columns)
    {
        var mapping = new List<ImportColumnDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in (columns ?? []).Where(c => !c.FieldName.IsNullOrWhiteSpace()))
        {
            var field = profile.FindImportableField(column.FieldName)
                ?? throw new BusinessException(ErpErrorCodes.RapidStart.FieldNotImportable)
                    .WithData("fieldName", column.FieldName)
                    .WithData("entityName", profile.Name);

            if (!seen.Add(field.Name))
            {
                throw new BusinessException(ErpErrorCodes.RapidStart.ColumnMappedTwice).WithData("fieldName", field.DisplayName);
            }

            mapping.Add(new ImportColumnDto { Index = column.Index, Header = column.Header, FieldName = field.Name });
        }

        foreach (var key in profile.KeyFields)
        {
            // A reference key may come in as the code it is looked up by (DimensionCode for DimensionId).
            var codeField = profile.FindRelation(key)?.CodeFieldName;
            if (!seen.Contains(key) && (codeField == null || !seen.Contains(codeField)))
            {
                throw new BusinessException(ErpErrorCodes.RapidStart.KeyFieldNotMapped)
                    .WithData("fieldName", profile.Definition.GetField(key).DisplayName);
            }
        }

        return mapping;
    }

    private static (List<SpreadsheetSheetData> Sheets, SpreadsheetSheetData Sheet) ReadSheet(ImportFileInput input)
    {
        char? separator = input.Separator.IsNullOrEmpty() ? null : input.Separator[0];
        var sheets = ImportFileReader.Read(input.FileName, RapidStartAccess.Decode(input), separator);

        var sheet = input.SheetName.IsNullOrWhiteSpace()
            ? sheets.FirstOrDefault()
            : sheets.FirstOrDefault(s => string.Equals(s.Name, input.SheetName, StringComparison.OrdinalIgnoreCase));

        if (sheet == null || sheet.Rows.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.FileHasNoRows);
        }

        return (sheets, sheet);
    }

    /// <summary>Headers from the first row, or "Column 1", "Column 2"… when the file has none.</summary>
    private static (List<string> Headers, List<List<string>> Rows) SplitHeader(SpreadsheetSheetData sheet, bool hasHeaders)
    {
        var width = sheet.Rows.Max(r => r.Count);
        var rows = sheet.Rows.Skip(hasHeaders ? 1 : 0).Where(r => !r.All(string.IsNullOrWhiteSpace)).ToList();

        var headers = hasHeaders
            ? sheet.Rows[0].Select(h => h?.Trim() ?? string.Empty).ToList()
            : new List<string>();

        for (var i = headers.Count; i < width; i++)
        {
            headers.Add($"Column {i + 1}");
        }

        return (headers, rows);
    }

    private async Task<ConfigTemplate> FindTemplateAsync(ConfigTableProfile profile, string code)
    {
        if (code.IsNullOrWhiteSpace())
        {
            return null;
        }

        var normalized = code.Trim().ToUpperInvariant();
        var template = await _templateRepository.FirstOrDefaultAsync(t => t.Code == normalized);

        return template == null || !string.Equals(template.EntityName, profile.Name, StringComparison.OrdinalIgnoreCase)
            ? null
            : await _templateRepository.GetAsync(template.Id, includeDetails: true);
    }

    private static ImportResultDto ToResult(ConfigApplyResult result, int totalRows, bool dryRun)
    {
        return new ImportResultDto
        {
            DryRun = dryRun,
            Succeeded = result.Errors.Count == 0,
            TotalRows = totalRows,
            Inserted = result.Inserted,
            Modified = result.Modified,
            Errors = result
                .Errors.OrderBy(e => e.RecordNo)
                .Select(e => new ImportErrorDto
                {
                    RowNo = e.RecordNo,
                    FieldName = e.FieldName,
                    Message = e.Message,
                })
                .ToList(),
        };
    }
}
