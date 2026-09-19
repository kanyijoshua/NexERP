using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Numbering;

[Authorize(ErpPermissions.NoSeries.Default)]
public class NoSeriesAppService
    : CrudAppService<NoSeries, NoSeriesDto, Guid, GetNoSeriesListInput, CreateUpdateNoSeriesDto, CreateUpdateNoSeriesDto>,
        INoSeriesAppService
{
    public NoSeriesAppService(IRepository<NoSeries, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.NoSeries.Default;
        GetListPolicyName = ErpPermissions.NoSeries.Default;
        CreatePolicyName = ErpPermissions.NoSeries.Create;
        UpdatePolicyName = ErpPermissions.NoSeries.Update;
        DeletePolicyName = ErpPermissions.NoSeries.Delete;
    }

    public override async Task<NoSeriesDto> CreateAsync(CreateUpdateNoSeriesDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureCodeIsUniqueAsync(input.Code, null);

        var series = new NoSeries(GuidGenerator.Create(), input.Code.Trim().ToUpperInvariant(), input.Description,
            input.DefaultNos, input.ManualNos, input.DateOrder);

        foreach (var line in input.Lines)
        {
            series.AddLine(GuidGenerator.Create(), line.StartingDate, line.StartingNo, line.EndingNo, line.WarningNo, line.IncrementByNo);
        }

        await Repository.InsertAsync(series, autoSave: true);
        return await MapToGetOutputDtoAsync(series);
    }

    public override async Task<NoSeriesDto> UpdateAsync(Guid id, CreateUpdateNoSeriesDto input)
    {
        await CheckUpdatePolicyAsync();

        var series = await GetEntityByIdAsync(id);

        // The code is the key other tables refer to (setup, journals), so it does not change.
        series.Update(input.Description, input.DefaultNos, input.ManualNos, input.DateOrder);

        // Lines are matched by id so each keeps its Last No. Used; that value never comes from the client.
        var keptIds = input.Lines.Where(l => l.Id.HasValue).Select(l => l.Id.Value).ToHashSet();
        foreach (var removed in series.Lines.Where(l => !keptIds.Contains(l.Id)).ToList())
        {
            series.RemoveLine(removed.Id);
        }

        foreach (var line in input.Lines)
        {
            var existing = line.Id.HasValue ? series.Lines.FirstOrDefault(l => l.Id == line.Id.Value) : null;
            if (existing != null)
            {
                existing.Update(line.StartingDate, line.StartingNo, line.EndingNo, line.WarningNo, line.IncrementByNo);
            }
            else
            {
                series.AddLine(GuidGenerator.Create(), line.StartingDate, line.StartingNo, line.EndingNo, line.WarningNo, line.IncrementByNo);
            }
        }

        await Repository.UpdateAsync(series, autoSave: true);
        return await MapToGetOutputDtoAsync(series);
    }

    public async Task<NextNoPreviewDto> GetNextNoPreviewAsync(NextNoPreviewInput input)
    {
        var series = await Repository.FindAsync(s => s.Code == input.Code, includeDetails: true)
            ?? throw new BusinessException(ErpErrorCodes.NoSeries.NoSeriesNotFound).WithData("code", input.Code);

        return new NextNoPreviewDto
        {
            Code = series.Code,
            NextNo = series.FindLine(input.Date ?? Clock.Now)?.PeekNextNo(),
            ManualNos = series.ManualNos,
            DefaultNos = series.DefaultNos,
        };
    }

    protected override async Task<NoSeries> GetEntityByIdAsync(Guid id)
    {
        return await Repository.GetAsync(id, includeDetails: true);
    }

    protected override async Task<IQueryable<NoSeries>> CreateFilteredQueryAsync(GetNoSeriesListInput input)
    {
        // The list shows the current line's numbers, so lines are needed here too.
        var query = await Repository.WithDetailsAsync();

        return query.WhereIf(
            !input.Filter.IsNullOrWhiteSpace(),
            x => x.Code.Contains(input.Filter) || x.Description.Contains(input.Filter)
        );
    }

    protected override IQueryable<NoSeries> ApplyDefaultSorting(IQueryable<NoSeries> query)
    {
        return query.OrderBy(x => x.Code);
    }

    protected override NoSeriesDto MapToGetOutputDto(NoSeries entity)
    {
        return Summarize(base.MapToGetOutputDto(entity), entity);
    }

    protected override NoSeriesDto MapToGetListOutputDto(NoSeries entity)
    {
        return Summarize(base.MapToGetListOutputDto(entity), entity);
    }

    private NoSeriesDto Summarize(NoSeriesDto dto, NoSeries entity)
    {
        var current = entity.FindLine(Clock.Now) ?? entity.Lines.OrderByDescending(l => l.LineNo).FirstOrDefault();

        dto.StartingNo = current?.StartingNo;
        dto.EndingNo = current?.EndingNo;
        dto.LastNoUsed = current?.LastNoUsed;
        dto.NextNo = entity.FindLine(Clock.Now)?.PeekNextNo();
        dto.Warning = current?.IsPastWarningNo() ?? false;
        dto.Lines = dto.Lines.OrderBy(l => l.LineNo).ToList();
        return dto;
    }

    private async Task EnsureCodeIsUniqueAsync(string code, Guid? exceptId)
    {
        var normalized = code.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(x => x.Code == normalized && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.NoSeries.NoSeriesCodeAlreadyExists).WithData("code", normalized);
        }
    }
}
