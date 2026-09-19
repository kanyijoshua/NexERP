using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Numbering;

/// <summary>
/// Hands out numbers. Mirrors Business Central codeunit 396 "NoSeriesManagement".
/// </summary>
public class NoSeriesManager : DomainService
{
    private readonly IRepository<NoSeries, Guid> _noSeriesRepository;

    public NoSeriesManager(IRepository<NoSeries, Guid> noSeriesRepository)
    {
        _noSeriesRepository = noSeriesRepository;
    }

    /// <summary>
    /// Takes the next number of the series and records it as used. The number is only
    /// consumed if the surrounding unit of work commits. (BC: GetNextNo with ModifySeries = true.)
    /// </summary>
    public async Task<string> GetNextNoAsync(string seriesCode, DateTime usageDate)
    {
        var series = await GetSeriesAsync(seriesCode);
        var line = FindLineOrThrow(series, usageDate);

        if (series.DateOrder && line.LastDateUsed.HasValue && usageDate.Date < line.LastDateUsed.Value)
        {
            throw new BusinessException(ErpErrorCodes.NoSeries.DateOrderViolation)
                .WithData("code", series.Code)
                .WithData("date", usageDate.ToString("yyyy-MM-dd"))
                .WithData("lastDateUsed", line.LastDateUsed.Value.ToString("yyyy-MM-dd"));
        }

        var next = line.PeekNextNo();
        if (next == null)
        {
            throw new BusinessException(ErpErrorCodes.NoSeries.SeriesExhausted)
                .WithData("code", series.Code)
                .WithData("endingNo", line.EndingNo);
        }

        line.Use(next, usageDate);

        // Flushed at once so a concurrent taker of the same number fails here, not at commit.
        await _noSeriesRepository.UpdateAsync(series, autoSave: true);

        return next;
    }

    /// <summary>The number GetNextNoAsync would return, without consuming it. Null if none is available.</summary>
    public async Task<string> PeekNextNoAsync(string seriesCode, DateTime usageDate)
    {
        var series = await GetSeriesAsync(seriesCode);
        return series.FindLine(usageDate)?.PeekNextNo();
    }

    /// <summary>
    /// Resolves the number of a new record, as BC's InitSeries does: a typed number needs a series
    /// that allows manual numbers; a blank one takes the next number of a default-numbers series.
    /// With no series configured the typed number is simply required.
    /// </summary>
    public async Task<string> ResolveNoAsync(string seriesCode, string typedNo, DateTime usageDate)
    {
        if (seriesCode.IsNullOrWhiteSpace())
        {
            if (typedNo.IsNullOrWhiteSpace())
            {
                throw new BusinessException(ErpErrorCodes.NoSeries.NumberRequired);
            }

            return typedNo.Trim();
        }

        var series = await GetSeriesAsync(seriesCode);

        if (!typedNo.IsNullOrWhiteSpace())
        {
            if (!series.ManualNos)
            {
                throw new BusinessException(ErpErrorCodes.NoSeries.ManualNumbersNotAllowed).WithData("code", series.Code);
            }

            return typedNo.Trim();
        }

        if (!series.DefaultNos)
        {
            throw new BusinessException(ErpErrorCodes.NoSeries.NumberRequired);
        }

        return await GetNextNoAsync(series.Code, usageDate);
    }

    private async Task<NoSeries> GetSeriesAsync(string seriesCode)
    {
        // includeDetails brings the lines (see DefaultWithDetailsFunc in the EF Core module).
        return await _noSeriesRepository.FindAsync(s => s.Code == seriesCode, includeDetails: true)
            ?? throw new BusinessException(ErpErrorCodes.NoSeries.NoSeriesNotFound).WithData("code", seriesCode);
    }

    private static NoSeriesLine FindLineOrThrow(NoSeries series, DateTime usageDate)
    {
        return series.FindLine(usageDate)
            ?? throw new BusinessException(ErpErrorCodes.NoSeries.NoOpenLine)
                .WithData("code", series.Code)
                .WithData("date", usageDate.ToString("yyyy-MM-dd"));
    }
}
