using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Dimensions;

[Authorize(ErpPermissions.Dimensions.Default)]
public class DimensionAppService : ErpAppService, IDimensionAppService
{
    private readonly IRepository<Dimension, Guid> _dimensionRepository;
    private readonly IRepository<DimensionValue, Guid> _dimensionValueRepository;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly DimensionManagement _dimensionManagement;

    public DimensionAppService(
        IRepository<Dimension, Guid> dimensionRepository,
        IRepository<DimensionValue, Guid> dimensionValueRepository,
        GeneralLedgerSetupManager glSetupManager,
        DimensionManagement dimensionManagement
    )
    {
        _dimensionRepository = dimensionRepository;
        _dimensionValueRepository = dimensionValueRepository;
        _glSetupManager = glSetupManager;
        _dimensionManagement = dimensionManagement;
    }

    public async Task<ListResultDto<DimensionDto>> GetListAsync()
    {
        var dimensions = await _dimensionRepository.GetListAsync();

        return new ListResultDto<DimensionDto>(
            ObjectMapper.Map<List<Dimension>, List<DimensionDto>>(dimensions.OrderBy(d => d.Code).ToList())
        );
    }

    [Authorize(ErpPermissions.Dimensions.Create)]
    public async Task<DimensionDto> CreateAsync(CreateDimensionDto input)
    {
        if (await _dimensionRepository.AnyAsync(d => d.Code == input.Code))
        {
            throw new BusinessException(ErpErrorCodes.Dimensions.DimensionCodeAlreadyExists).WithData("code", input.Code);
        }

        var dimension = new Dimension(GuidGenerator.Create(), input.Code, input.Name, input.Description);
        dimension.SetBlocked(input.Blocked);
        await _dimensionRepository.InsertAsync(dimension, autoSave: true);

        return ObjectMapper.Map<Dimension, DimensionDto>(dimension);
    }

    [Authorize(ErpPermissions.Dimensions.Update)]
    public async Task<DimensionDto> UpdateAsync(Guid id, UpdateDimensionDto input)
    {
        var dimension = await _dimensionRepository.GetAsync(id);
        dimension.SetName(input.Name);
        dimension.SetDescription(input.Description);
        dimension.SetBlocked(input.Blocked);
        await _dimensionRepository.UpdateAsync(dimension, autoSave: true);

        return ObjectMapper.Map<Dimension, DimensionDto>(dimension);
    }

    [Authorize(ErpPermissions.Dimensions.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _dimensionValueRepository.DeleteAsync(v => v.DimensionId == id, autoSave: true);
        await _dimensionRepository.DeleteAsync(id, autoSave: true);
    }

    public async Task<ListResultDto<DimensionValueDto>> GetValuesAsync(Guid dimensionId)
    {
        var values = await _dimensionValueRepository.GetListAsync(v => v.DimensionId == dimensionId);

        return new ListResultDto<DimensionValueDto>(
            ObjectMapper.Map<List<DimensionValue>, List<DimensionValueDto>>(values.OrderBy(v => v.Code).ToList())
        );
    }

    [Authorize(ErpPermissions.Dimensions.Create)]
    public async Task<DimensionValueDto> CreateValueAsync(CreateDimensionValueDto input)
    {
        // The dimension code is taken from the dimension, not trusted from the client.
        var dimension = await _dimensionRepository.GetAsync(input.DimensionId);

        if (await _dimensionValueRepository.AnyAsync(v => v.DimensionId == dimension.Id && v.Code == input.Code))
        {
            throw new BusinessException(ErpErrorCodes.Dimensions.DimensionValueCodeAlreadyExists)
                .WithData("dimensionCode", dimension.Code)
                .WithData("code", input.Code);
        }

        var value = new DimensionValue(GuidGenerator.Create(), dimension.Id, dimension.Code, input.Code, input.Name);
        value.SetBlocked(input.Blocked);
        await _dimensionValueRepository.InsertAsync(value, autoSave: true);

        return ObjectMapper.Map<DimensionValue, DimensionValueDto>(value);
    }

    [Authorize(ErpPermissions.Dimensions.Update)]
    public async Task<DimensionValueDto> UpdateValueAsync(Guid id, UpdateDimensionValueDto input)
    {
        var value = await _dimensionValueRepository.GetAsync(id);
        value.SetName(input.Name);
        value.SetBlocked(input.Blocked);
        await _dimensionValueRepository.UpdateAsync(value, autoSave: true);

        return ObjectMapper.Map<DimensionValue, DimensionValueDto>(value);
    }

    [Authorize(ErpPermissions.Dimensions.Delete)]
    public async Task DeleteValueAsync(Guid id)
    {
        await _dimensionValueRepository.DeleteAsync(id, autoSave: true);
    }

    [AllowAnonymous]
    public async Task<ListResultDto<ShortcutDimensionSlotDto>> GetShortcutSlotsAsync()
    {
        var setup = await _glSetupManager.GetAsync();
        var codes = setup.GetShortcutDimensionCodes();
        var dimensions = await _dimensionRepository.GetListAsync();
        var values = await _dimensionValueRepository.GetListAsync(v => !v.Blocked);

        var dimByCode = dimensions.ToDictionary(d => d.Code, StringComparer.OrdinalIgnoreCase);
        var valuesByDimId = values
            .GroupBy(v => v.DimensionId)
            .ToDictionary(g => g.Key, g => ObjectMapper.Map<List<DimensionValue>, List<DimensionValueDto>>(g.OrderBy(v => v.Code).ToList()));

        var slots = new List<ShortcutDimensionSlotDto>(GeneralLedgerSetup.MaxShortcutDimensions);
        for (var i = 0; i < GeneralLedgerSetup.MaxShortcutDimensions; i++)
        {
            var code = codes[i];
            Dimension dim = null;
            if (!code.IsNullOrWhiteSpace())
            {
                dimByCode.TryGetValue(code, out dim);
            }

            slots.Add(
                new ShortcutDimensionSlotDto
                {
                    SlotNumber = i + 1,
                    IsGlobal = i < 2,
                    DimensionCode = dim?.Code ?? code,
                    DimensionName = dim?.Name ?? code,
                    Values = dim != null && valuesByDimId.TryGetValue(dim.Id, out var dimValues)
                        ? dimValues
                        : new List<DimensionValueDto>(),
                }
            );
        }

        return new ListResultDto<ShortcutDimensionSlotDto>(slots);
    }

    public async Task<DimensionSetDto> GetDimensionSetAsync(Guid dimensionSetId)
    {
        var entries = await _dimensionManagement.GetDimensionSetEntriesAsync(dimensionSetId);
        var shortcuts = await _dimensionManagement.GetShortcutDimensionValuesAsync(dimensionSetId);

        return new DimensionSetDto
        {
            DimensionSetId = dimensionSetId,
            ShortcutDimension1Code = shortcuts[0],
            ShortcutDimension2Code = shortcuts[1],
            ShortcutDimension3Code = shortcuts[2],
            ShortcutDimension4Code = shortcuts[3],
            ShortcutDimension5Code = shortcuts[4],
            ShortcutDimension6Code = shortcuts[5],
            ShortcutDimension7Code = shortcuts[6],
            ShortcutDimension8Code = shortcuts[7],
            Entries = entries
                .Select(e => new DimensionSetEntryDto
                {
                    DimensionCode = e.DimensionCode,
                    DimensionValueCode = e.DimensionValueCode,
                })
                .ToList(),
        };
    }

    public async Task<DimensionSetDto> ResolveDimensionSetAsync(ResolveDimensionSetInput input)
    {
        Check.NotNull(input, nameof(input));

        var (setId, _) = await _dimensionManagement.ResolveShortcutDimensionsAsync(
            new[]
            {
                input.ShortcutDimension1Code,
                input.ShortcutDimension2Code,
                input.ShortcutDimension3Code,
                input.ShortcutDimension4Code,
                input.ShortcutDimension5Code,
                input.ShortcutDimension6Code,
                input.ShortcutDimension7Code,
                input.ShortcutDimension8Code,
            },
            input.ExistingDimensionSetId
        );

        if (input.Entries != null && input.Entries.Count > 0)
        {
            var currentEntries = await _dimensionManagement.GetDimensionSetEntriesAsync(setId);
            var merged = currentEntries.ToDictionary(e => e.DimensionCode, e => e.DimensionValueCode, StringComparer.OrdinalIgnoreCase);
            foreach (var (dimCode, valCode) in input.Entries)
            {
                if (valCode.IsNullOrWhiteSpace())
                {
                    merged.Remove(dimCode);
                }
                else
                {
                    merged[dimCode] = valCode;
                }
            }

            setId = await _dimensionManagement.EnsureDimensionSetAsync(merged);
        }

        return await GetDimensionSetAsync(setId);
    }
}
