using System;
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
}

public interface IDimensionAppService : IApplicationService
{
    Task<ListResultDto<DimensionDto>> GetListAsync();

    Task<DimensionDto> CreateAsync(CreateDimensionDto input);

    /// <summary>Routed as GET /api/erp/dimension/values/{dimensionId}.</summary>
    Task<ListResultDto<DimensionValueDto>> GetValuesAsync(Guid dimensionId);

    /// <summary>Routed as POST /api/erp/dimension/value.</summary>
    Task<DimensionValueDto> CreateValueAsync(CreateDimensionValueDto input);
}
