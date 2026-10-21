using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Dimensions;

public class DimensionDto : EntityDto<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Blocked { get; set; }
}

public class CreateDimensionDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDimensionCodeLength)]
    public string Code { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public bool Blocked { get; set; }
}

public class UpdateDimensionDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public bool Blocked { get; set; }
}

public class DimensionValueDto : EntityDto<Guid>
{
    public Guid DimensionId { get; set; }
    public string DimensionCode { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public bool Blocked { get; set; }
}

public class CreateDimensionValueDto
{
    public Guid DimensionId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDimensionValueCodeLength)]
    public string Code { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    public bool Blocked { get; set; }
}

public class UpdateDimensionValueDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    public bool Blocked { get; set; }
}

/// <summary>
/// Represents one of the 8 Business Central dimension slots (1..2 Global, 3..8 Shortcut)
/// configured on General Ledger Setup, along with its allowed dimension values.
/// </summary>
public class ShortcutDimensionSlotDto
{
    public int SlotNumber { get; set; }
    public bool IsGlobal { get; set; }
    public string DimensionCode { get; set; }
    public string DimensionName { get; set; }
    public List<DimensionValueDto> Values { get; set; } = new();
}

public class DimensionSetEntryDto
{
    public string DimensionCode { get; set; }
    public string DimensionValueCode { get; set; }
}

public class DimensionSetDto
{
    public Guid DimensionSetId { get; set; }
    public string ShortcutDimension1Code { get; set; }
    public string ShortcutDimension2Code { get; set; }
    public string ShortcutDimension3Code { get; set; }
    public string ShortcutDimension4Code { get; set; }
    public string ShortcutDimension5Code { get; set; }
    public string ShortcutDimension6Code { get; set; }
    public string ShortcutDimension7Code { get; set; }
    public string ShortcutDimension8Code { get; set; }
    public List<DimensionSetEntryDto> Entries { get; set; } = new();
}

public class ResolveDimensionSetInput
{
    public Guid ExistingDimensionSetId { get; set; }
    public string ShortcutDimension1Code { get; set; }
    public string ShortcutDimension2Code { get; set; }
    public string ShortcutDimension3Code { get; set; }
    public string ShortcutDimension4Code { get; set; }
    public string ShortcutDimension5Code { get; set; }
    public string ShortcutDimension6Code { get; set; }
    public string ShortcutDimension7Code { get; set; }
    public string ShortcutDimension8Code { get; set; }
    public Dictionary<string, string> Entries { get; set; } = new();
}

public interface IDimensionAppService : IApplicationService
{
    Task<ListResultDto<DimensionDto>> GetListAsync();

    Task<DimensionDto> CreateAsync(CreateDimensionDto input);

    Task<DimensionDto> UpdateAsync(Guid id, UpdateDimensionDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Routed as GET /api/erp/dimension/values/{dimensionId}.</summary>
    Task<ListResultDto<DimensionValueDto>> GetValuesAsync(Guid dimensionId);

    /// <summary>Routed as POST /api/erp/dimension/value.</summary>
    Task<DimensionValueDto> CreateValueAsync(CreateDimensionValueDto input);

    /// <summary>Routed as PUT /api/erp/dimension/value/{id}.</summary>
    Task<DimensionValueDto> UpdateValueAsync(Guid id, UpdateDimensionValueDto input);

    /// <summary>Routed as DELETE /api/erp/dimension/value/{id}.</summary>
    Task DeleteValueAsync(Guid id);

    /// <summary>Returns the 8 configured dimension slots (Global 1..2 + Shortcut 3..8) and their available values.</summary>
    Task<ListResultDto<ShortcutDimensionSlotDto>> GetShortcutSlotsAsync();

    /// <summary>Reads the entries and 1..8 shortcut dimension values for a DimensionSetId.</summary>
    Task<DimensionSetDto> GetDimensionSetAsync(Guid dimensionSetId);

    /// <summary>Resolves and persists a DimensionSetId from up to 8 shortcut dimension values and/or explicit entries.</summary>
    Task<DimensionSetDto> ResolveDimensionSetAsync(ResolveDimensionSetInput input);
}
