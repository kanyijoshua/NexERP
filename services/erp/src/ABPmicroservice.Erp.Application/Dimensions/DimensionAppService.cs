using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public DimensionAppService(
        IRepository<Dimension, Guid> dimensionRepository,
        IRepository<DimensionValue, Guid> dimensionValueRepository
    )
    {
        _dimensionRepository = dimensionRepository;
        _dimensionValueRepository = dimensionValueRepository;
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
        await _dimensionRepository.InsertAsync(dimension, autoSave: true);

        return ObjectMapper.Map<Dimension, DimensionDto>(dimension);
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
        await _dimensionValueRepository.InsertAsync(value, autoSave: true);

        return ObjectMapper.Map<DimensionValue, DimensionValueDto>(value);
    }
}
