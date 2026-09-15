using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Dimensions;

public class CreateDimensionDto
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class CreateDimensionValueDto
{
    public Guid DimensionId { get; set; }
    public string DimensionCode { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
}

public class DimensionAppService : ApplicationService
{
    private readonly IRepository<Dimension, Guid> _dimensionRepository;
    private readonly IRepository<DimensionValue, Guid> _dimensionValueRepository;
    private readonly DimensionManagement _dimensionManagement;

    public DimensionAppService(
        IRepository<Dimension, Guid> dimensionRepository,
        IRepository<DimensionValue, Guid> dimensionValueRepository,
        DimensionManagement dimensionManagement
    )
    {
        _dimensionRepository = dimensionRepository;
        _dimensionValueRepository = dimensionValueRepository;
        _dimensionManagement = dimensionManagement;
    }

    [Authorize(ErpPermissions.Dimensions.Default)]
    public async Task<List<Dimension>> GetDimensionsAsync()
    {
        return await _dimensionRepository.GetListAsync();
    }

    [Authorize(ErpPermissions.Dimensions.Default)]
    public async Task<List<DimensionValue>> GetValuesAsync(Guid dimensionId)
    {
        return await _dimensionValueRepository.GetListAsync(v => v.DimensionId == dimensionId);
    }

    [Authorize(ErpPermissions.Dimensions.Create)]
    public async Task<Dimension> CreateDimensionAsync(CreateDimensionDto input)
    {
        var dimension = new Dimension(GuidGenerator.Create(), input.Code, input.Name, input.Description);
        return await _dimensionRepository.InsertAsync(dimension, autoSave: true);
    }

    [Authorize(ErpPermissions.Dimensions.Create)]
    public async Task<DimensionValue> CreateDimensionValueAsync(CreateDimensionValueDto input)
    {
        var value = new DimensionValue(GuidGenerator.Create(), input.DimensionId, input.DimensionCode, input.Code, input.Name);
        return await _dimensionValueRepository.InsertAsync(value, autoSave: true);
    }

    public Guid ResolveDimensionSetId(Dictionary<string, string> values)
    {
        return _dimensionManagement.GetDimensionSetId(values);
    }
}
