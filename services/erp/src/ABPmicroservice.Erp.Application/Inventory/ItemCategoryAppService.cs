using System;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Inventory;

[Authorize(ErpPermissions.ItemCategories.Default)]
public class ItemCategoryAppService
    : CrudAppService<
        ItemCategory,
        ItemCategoryDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateItemCategoryDto,
        CreateUpdateItemCategoryDto
    >,
        IItemCategoryAppService
{
    public ItemCategoryAppService(IRepository<ItemCategory, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.ItemCategories.Default;
        GetListPolicyName = ErpPermissions.ItemCategories.Default;
        CreatePolicyName = ErpPermissions.ItemCategories.Create;
        UpdatePolicyName = ErpPermissions.ItemCategories.Update;
        DeletePolicyName = ErpPermissions.ItemCategories.Delete;
    }
}
