using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Dimensions;

/// <summary>
/// Domain service for Dimension Set ID computation, persistence & Shortcut Dimension (1..8) resolution.
/// </summary>
public class DimensionManagement : DomainService
{
    private IRepository<Dimension, Guid> DimensionRepository =>
        LazyServiceProvider.LazyGetRequiredService<IRepository<Dimension, Guid>>();

    private IRepository<DimensionValue, Guid> DimensionValueRepository =>
        LazyServiceProvider.LazyGetRequiredService<IRepository<DimensionValue, Guid>>();

    private IRepository<DimensionSetEntry, Guid> DimensionSetEntryRepository =>
        LazyServiceProvider.LazyGetRequiredService<IRepository<DimensionSetEntry, Guid>>();

    private GeneralLedgerSetupManager GeneralLedgerSetupManager =>
        LazyServiceProvider.LazyGetRequiredService<GeneralLedgerSetupManager>();

    public Guid GetDimensionSetId(IDictionary<string, string> dimensionValues)
    {
        if (dimensionValues == null || !dimensionValues.Any())
        {
            return Guid.Empty;
        }

        var sortedPairs = dimensionValues
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
            .OrderBy(kv => kv.Key.Trim().ToUpperInvariant())
            .Select(kv => $"{kv.Key.Trim().ToUpperInvariant()}:{kv.Value.Trim().ToUpperInvariant()}")
            .ToList();

        if (sortedPairs.Count == 0)
        {
            return Guid.Empty;
        }

        var rawString = string.Join(";", sortedPairs);
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(rawString));
        return new Guid(hashBytes);
    }

    /// <summary>
    /// Validates the dimension codes and values, computes the deterministic <see cref="DimensionSetEntry.DimensionSetId"/>,
    /// and persists the <see cref="DimensionSetEntry"/> rows if not already stored.
    /// </summary>
    public async Task<Guid> EnsureDimensionSetAsync(IDictionary<string, string> dimensionValues)
    {
        if (dimensionValues == null)
        {
            return Guid.Empty;
        }

        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (rawDim, rawVal) in dimensionValues)
        {
            var dimCode = CodeTableEntity.NormalizeCode(rawDim);
            var valCode = CodeTableEntity.NormalizeCode(rawVal);
            if (dimCode == null || valCode == null)
            {
                continue;
            }

            var dim = await DimensionRepository.FirstOrDefaultAsync(d => d.Code.ToUpper() == dimCode)
                ?? throw new BusinessException(ErpErrorCodes.GeneralLedger.DimensionNotFound).WithData("code", dimCode);

            if (dim.Blocked)
            {
                throw new BusinessException(ErpErrorCodes.Dimensions.DimensionBlocked).WithData("code", dim.Code);
            }

            var val = await DimensionValueRepository.FirstOrDefaultAsync(v => v.DimensionId == dim.Id && v.Code.ToUpper() == valCode)
                ?? throw new BusinessException(ErpErrorCodes.Dimensions.DimensionValueNotFound)
                    .WithData("dimensionCode", dim.Code)
                    .WithData("code", valCode);

            if (val.Blocked)
            {
                throw new BusinessException(ErpErrorCodes.Dimensions.DimensionValueBlocked)
                    .WithData("dimensionCode", dim.Code)
                    .WithData("code", val.Code);
            }

            normalized[dim.Code] = val.Code;
        }

        if (normalized.Count == 0)
        {
            return Guid.Empty;
        }

        var setId = GetDimensionSetId(normalized);
        if (!await DimensionSetEntryRepository.AnyAsync(e => e.DimensionSetId == setId))
        {
            foreach (var (dimCode, valCode) in normalized.OrderBy(kv => kv.Key))
            {
                await DimensionSetEntryRepository.InsertAsync(
                    new DimensionSetEntry(GuidGenerator.Create(), setId, dimCode, valCode),
                    autoSave: true
                );
            }
        }

        return setId;
    }

    /// <summary>Returns the persisted <see cref="DimensionSetEntry"/> rows for a <paramref name="dimensionSetId"/>.</summary>
    public async Task<List<DimensionSetEntry>> GetDimensionSetEntriesAsync(Guid dimensionSetId)
    {
        if (dimensionSetId == Guid.Empty)
        {
            return new List<DimensionSetEntry>();
        }

        var entries = await DimensionSetEntryRepository.GetListAsync(e => e.DimensionSetId == dimensionSetId);
        return entries.OrderBy(e => e.DimensionCode).ToList();
    }

    /// <summary>
    /// Resolves up to 8 shortcut dimension values (slots 1..8 configured on <see cref="GeneralLedgerSetup"/>)
    /// into a persisted <see cref="DimensionSetEntry.DimensionSetId"/>, preserving any additional dimensions
    /// already present on <paramref name="existingDimensionSetId"/>.
    /// </summary>
    public async Task<(Guid DimensionSetId, string[] ShortcutValues)> ResolveShortcutDimensionsAsync(
        IReadOnlyList<string> shortcutValues,
        Guid existingDimensionSetId = default
    )
    {
        var setup = await GeneralLedgerSetupManager.GetAsync();
        var slotDimensionCodes = setup.GetShortcutDimensionCodes();
        var normalizedSlots = new string[GeneralLedgerSetup.MaxShortcutDimensions];

        var existingEntries = await GetDimensionSetEntriesAsync(existingDimensionSetId);
        var combined = existingEntries.ToDictionary(e => e.DimensionCode, e => e.DimensionValueCode, StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < GeneralLedgerSetup.MaxShortcutDimensions; i++)
        {
            var dimCode = slotDimensionCodes[i];
            var rawVal = shortcutValues != null && i < shortcutValues.Count ? shortcutValues[i] : null;
            var valCode = CodeTableEntity.NormalizeCode(rawVal);
            normalizedSlots[i] = valCode;

            if (dimCode.IsNullOrWhiteSpace())
            {
                continue;
            }

            if (valCode == null)
            {
                combined.Remove(dimCode);
            }
            else
            {
                combined[dimCode] = valCode;
            }
        }

        var setId = await EnsureDimensionSetAsync(combined);
        return (setId, normalizedSlots);
    }

    /// <summary>
    /// Reads the 8 shortcut dimension values (slots 1..8) for a single <paramref name="dimensionSetId"/>.
    /// </summary>
    public async Task<string[]> GetShortcutDimensionValuesAsync(Guid dimensionSetId)
    {
        var result = new string[GeneralLedgerSetup.MaxShortcutDimensions];
        if (dimensionSetId == Guid.Empty)
        {
            return result;
        }

        var setup = await GeneralLedgerSetupManager.GetAsync();
        var slotCodes = setup.GetShortcutDimensionCodes();
        var entries = await GetDimensionSetEntriesAsync(dimensionSetId);
        var byDim = entries.ToDictionary(e => e.DimensionCode, e => e.DimensionValueCode, StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < GeneralLedgerSetup.MaxShortcutDimensions; i++)
        {
            var dimCode = slotCodes[i];
            if (!dimCode.IsNullOrWhiteSpace() && byDim.TryGetValue(dimCode, out var val))
            {
                result[i] = val;
            }
        }

        return result;
    }

    /// <summary>
    /// Batch-resolves the 8 shortcut dimension values (slots 1..8) for a set of <paramref name="dimensionSetIds"/>.
    /// </summary>
    public async Task<Dictionary<Guid, string[]>> GetShortcutDimensionValuesBySetIdsAsync(IEnumerable<Guid> dimensionSetIds)
    {
        var ids = dimensionSetIds?.Where(id => id != Guid.Empty).Distinct().ToList() ?? new List<Guid>();
        var result = new Dictionary<Guid, string[]>();
        if (ids.Count == 0)
        {
            return result;
        }

        var setup = await GeneralLedgerSetupManager.GetAsync();
        var slotCodes = setup.GetShortcutDimensionCodes();
        var entries = await DimensionSetEntryRepository.GetListAsync(e => ids.Contains(e.DimensionSetId));

        foreach (var group in entries.GroupBy(e => e.DimensionSetId))
        {
            var byDim = group.ToDictionary(e => e.DimensionCode, e => e.DimensionValueCode, StringComparer.OrdinalIgnoreCase);
            var values = new string[GeneralLedgerSetup.MaxShortcutDimensions];
            for (var i = 0; i < GeneralLedgerSetup.MaxShortcutDimensions; i++)
            {
                var dimCode = slotCodes[i];
                if (!dimCode.IsNullOrWhiteSpace() && byDim.TryGetValue(dimCode, out var val))
                {
                    values[i] = val;
                }
            }

            result[group.Key] = values;
        }

        return result;
    }
}
