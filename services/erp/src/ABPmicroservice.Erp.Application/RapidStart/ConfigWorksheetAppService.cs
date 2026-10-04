using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// The configuration worksheet: a company's set-up checklist,
/// with each table's current record count beside it.
/// </summary>
[Authorize(ErpPermissions.RapidStart.Default)]
public class ConfigWorksheetAppService : ErpAppService, IConfigWorksheetAppService
{
    private const int SortStep = 10;

    private readonly IRepository<ConfigLine, Guid> _lineRepository;
    private readonly ConfigTableRegistry _tables;
    private readonly RapidStartAccess _access;
    private readonly IEntityQueryExecutor _queryExecutor;

    public ConfigWorksheetAppService(
        IRepository<ConfigLine, Guid> lineRepository,
        ConfigTableRegistry tables,
        RapidStartAccess access,
        IEntityQueryExecutor queryExecutor
    )
    {
        _lineRepository = lineRepository;
        _tables = tables;
        _access = access;
        _queryExecutor = queryExecutor;
    }

    public async Task<ListResultDto<ConfigLineDto>> GetListAsync()
    {
        return new ListResultDto<ConfigLineDto>(await ToDtosAsync(await GetOrderedAsync()));
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task<ConfigLineDto> CreateAsync(CreateConfigLineDto input)
    {
        string entityName = null;
        if (input.LineType == ConfigLineType.Table)
        {
            entityName = (await _access.CheckReadAsync(input.EntityName)).Name;
        }

        var lines = await GetOrderedAsync();
        var line = new ConfigLine(GuidGenerator.Create(), input.LineType, input.Name, entityName, (lines.Count + 1) * SortStep);
        line.Update(input.Name, input.PackageCode, input.Status, input.ResponsibleUserName, input.Comments);

        await _lineRepository.InsertAsync(line, autoSave: true);
        return (await ToDtosAsync([line])).Single();
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task<ConfigLineDto> UpdateAsync(Guid id, UpdateConfigLineDto input)
    {
        var line = await _lineRepository.GetAsync(id);
        line.Update(input.Name, input.PackageCode, input.Status, input.ResponsibleUserName, input.Comments);

        await _lineRepository.UpdateAsync(line, autoSave: true);
        return (await ToDtosAsync([line])).Single();
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _lineRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Adds each area as a heading with its tables beneath, leaving out what is already listed.
    /// Lines already there keep their place.
    /// </summary>
    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task<ListResultDto<ConfigLineDto>> SuggestLinesAsync()
    {
        var lines = await GetOrderedAsync();
        var listedTables = lines.Where(l => l.EntityName != null).Select(l => l.EntityName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var catalog = await _access.GetCatalogAsync();
        var added = new List<ConfigLine>();

        foreach (var area in ConfigTableRegistry.Areas)
        {
            var missing = catalog.Where(t => t.Area == area && !listedTables.Contains(t.EntityName)).ToList();
            if (missing.Count == 0)
            {
                continue;
            }

            // New tables go under their area's heading, after whatever the area already lists.
            var heading = lines.FirstOrDefault(l => l.LineType == ConfigLineType.Area && string.Equals(l.Name, area, StringComparison.OrdinalIgnoreCase));
            int position;

            if (heading == null)
            {
                heading = new ConfigLine(GuidGenerator.Create(), ConfigLineType.Area, area, null, 0);
                lines.Add(heading);
                added.Add(heading);
                position = lines.Count;
            }
            else
            {
                position = lines.IndexOf(heading) + 1;
                while (position < lines.Count && lines[position].LineType != ConfigLineType.Area)
                {
                    position++;
                }
            }

            foreach (var table in missing)
            {
                var line = new ConfigLine(GuidGenerator.Create(), ConfigLineType.Table, table.DisplayName, table.EntityName, 0);
                lines.Insert(position++, line);
                added.Add(line);
            }
        }

        Renumber(lines);
        await _lineRepository.InsertManyAsync(added);
        await _lineRepository.UpdateManyAsync(lines.Except(added), autoSave: true);
        return new ListResultDto<ConfigLineDto>(await ToDtosAsync(lines));
    }

    [Authorize(ErpPermissions.RapidStart.Manage)]
    public async Task<ListResultDto<ConfigLineDto>> MoveAsync(Guid id, MoveConfigLineInput input)
    {
        var lines = await GetOrderedAsync();
        var index = lines.FindIndex(l => l.Id == id);
        var target = input.Up ? index - 1 : index + 1;

        if (index >= 0 && target >= 0 && target < lines.Count)
        {
            (lines[index], lines[target]) = (lines[target], lines[index]);
            Renumber(lines);
            await _lineRepository.UpdateManyAsync(lines, autoSave: true);
        }

        return new ListResultDto<ConfigLineDto>(await ToDtosAsync(lines));
    }

    private async Task<List<ConfigLine>> GetOrderedAsync()
    {
        return (await _lineRepository.GetListAsync()).OrderBy(l => l.SortOrder).ThenBy(l => l.CreationTime).ToList();
    }

    private static void Renumber(List<ConfigLine> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            lines[i].MoveTo((i + 1) * SortStep);
        }
    }

    private async Task<List<ConfigLineDto>> ToDtosAsync(IEnumerable<ConfigLine> lines)
    {
        var result = new List<ConfigLineDto>();

        foreach (var line in lines)
        {
            result.Add(
                new ConfigLineDto
                {
                    Id = line.Id,
                    LineType = line.LineType,
                    Name = line.Name,
                    EntityName = line.EntityName,
                    PackageCode = line.PackageCode,
                    Status = line.Status,
                    ResponsibleUserName = line.ResponsibleUserName,
                    Comments = line.Comments,
                    SortOrder = line.SortOrder,
                    NoOfRecords = await CountAsync(line.EntityName),
                }
            );
        }

        return result;
    }

    /// <summary>How many records a table holds now, when the caller may read it.</summary>
    private async Task<long?> CountAsync(string entityName)
    {
        if (entityName == null || _tables.Find(entityName) == null || !await _access.CanReadAsync(entityName))
        {
            return null;
        }

        var result = await _queryExecutor.QueryAsync(new EntityQueryRequest { EntityName = entityName, MaxResultCount = 1 });
        return result.TotalCount;
    }
}
