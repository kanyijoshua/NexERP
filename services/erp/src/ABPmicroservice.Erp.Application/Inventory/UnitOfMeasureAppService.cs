using System;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Inventory;

[Authorize(ErpPermissions.UnitsOfMeasure.Default)]
public class UnitOfMeasureAppService
    : CrudAppService<
        UnitOfMeasure,
        UnitOfMeasureDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateUnitOfMeasureDto,
        CreateUpdateUnitOfMeasureDto
    >,
        IUnitOfMeasureAppService
{
    public UnitOfMeasureAppService(IRepository<UnitOfMeasure, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.UnitsOfMeasure.Default;
        GetListPolicyName = ErpPermissions.UnitsOfMeasure.Default;
        CreatePolicyName = ErpPermissions.UnitsOfMeasure.Create;
        UpdatePolicyName = ErpPermissions.UnitsOfMeasure.Update;
        DeletePolicyName = ErpPermissions.UnitsOfMeasure.Delete;
    }
}
