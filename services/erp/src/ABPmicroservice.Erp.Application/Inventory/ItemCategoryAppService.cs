using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
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

    // Mapped by hand: the entity has private setters and guards, which AutoMapper cannot honour.
    public override async Task<ItemCategoryDto> CreateAsync(CreateUpdateItemCategoryDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureCodeIsUniqueAsync(input.Code, null);

        var parent = await FindParentAsync(input.ParentCategoryId, null);
        var category = new ItemCategory(GuidGenerator.Create(), input.Code, input.Description, parent?.Id, parent?.Code);

        await Repository.InsertAsync(category, autoSave: true);
        return await MapToGetOutputDtoAsync(category);
    }

    public override async Task<ItemCategoryDto> UpdateAsync(Guid id, CreateUpdateItemCategoryDto input)
    {
        await CheckUpdatePolicyAsync();

        var category = await GetEntityByIdAsync(id);
        await EnsureCodeIsUniqueAsync(input.Code, id);

        var parent = await FindParentAsync(input.ParentCategoryId, id);
        category.SetCode(input.Code);
        category.SetDescription(input.Description);
        category.SetParent(parent?.Id, parent?.Code);

        await Repository.UpdateAsync(category, autoSave: true);
        return await MapToGetOutputDtoAsync(category);
    }

    protected override IQueryable<ItemCategory> ApplyDefaultSorting(IQueryable<ItemCategory> query)
    {
        return query.OrderBy(x => x.Code);
    }

    private async Task EnsureCodeIsUniqueAsync(string code, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.Code == code && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.Items.CodeAlreadyExists).WithData("code", code);
        }
    }

    private async Task<ItemCategory> FindParentAsync(Guid? parentCategoryId, Guid? selfId)
    {
        if (!parentCategoryId.HasValue)
        {
            return null;
        }

        if (parentCategoryId == selfId)
        {
            throw new UserFriendlyException("An item category cannot be its own parent.");
        }

        return await Repository.GetAsync(parentCategoryId.Value);
    }
}
