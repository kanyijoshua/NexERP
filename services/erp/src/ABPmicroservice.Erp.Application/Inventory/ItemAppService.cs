using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Inventory;

[Authorize(ErpPermissions.Items.Default)]
public class ItemAppService
    : ErpCrudAppService<Item, ItemDto, Guid, GetItemListInput, CreateUpdateItemDto, CreateUpdateItemDto>,
        IItemAppService
{
    private readonly ItemManager _itemManager;

    public ItemAppService(IRepository<Item, Guid> repository, ItemManager itemManager)
        : base(repository)
    {
        _itemManager = itemManager;
        GetPolicyName = ErpPermissions.Items.Default;
        GetListPolicyName = ErpPermissions.Items.Default;
        CreatePolicyName = ErpPermissions.Items.Create;
        UpdatePolicyName = ErpPermissions.Items.Update;
        DeletePolicyName = ErpPermissions.Items.Delete;
    }

    public override async Task<ItemDto> CreateAsync(CreateUpdateItemDto input)
    {
        await EnsurePostingGroupsExistAsync(input);

        // Blank takes the next number of the Inventory Setup's Item Nos. series (BC: InitSeries).
        var setup = await LazyServiceProvider.LazyGetRequiredService<InventorySetupManager>().GetAsync();
        var no = await LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>().ResolveNoAsync(setup.ItemNos, input.No, Clock.Now);

        var item = await _itemManager.CreateAsync(
            no,
            input.Description,
            input.Type,
            input.BaseUnitOfMeasureCode,
            input.UnitPrice,
            input.UnitCost,
            input.ItemCategoryId,
            input.ItemCategoryCode
        );
        item.SetPostingGroups(
            PostingGroupBase.NormalizeCode(input.GenProdPostingGroup),
            PostingGroupBase.NormalizeCode(input.InventoryPostingGroup)
        );
        item.SetVatProdPostingGroup(input.VatProdPostingGroup);

        await Repository.InsertAsync(item, autoSave: true);
        return await MapToGetOutputDtoAsync(item);
    }

    public override async Task<ItemDto> UpdateAsync(Guid id, CreateUpdateItemDto input)
    {
        var item = await GetEntityByIdAsync(id);

        if (!string.Equals(item.No, input.No, StringComparison.OrdinalIgnoreCase))
        {
            await _itemManager.EnsureNoIsUniqueAsync(input.No, id);
            item.SetNo(input.No);
        }

        await EnsurePostingGroupsExistAsync(input);

        item.SetDescription(input.Description);
        item.SetType(input.Type);
        item.SetBaseUnitOfMeasureCode(input.BaseUnitOfMeasureCode);
        item.SetPrice(input.UnitPrice);
        item.SetCost(input.UnitCost);
        item.SetCategory(input.ItemCategoryId, input.ItemCategoryCode);
        item.SetPostingGroups(
            PostingGroupBase.NormalizeCode(input.GenProdPostingGroup),
            PostingGroupBase.NormalizeCode(input.InventoryPostingGroup)
        );
        item.SetVatProdPostingGroup(input.VatProdPostingGroup);

        await Repository.UpdateAsync(item, autoSave: true);
        return await MapToGetOutputDtoAsync(item);
    }

    /// <summary>A posting group on the card must exist (BC TableRelation); blank means none.</summary>
    private async Task EnsurePostingGroupsExistAsync(CreateUpdateItemDto input)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<GenProductPostingGroup>(input.GenProdPostingGroup);
        await codes.EnsureExistsAsync<InventoryPostingGroup>(input.InventoryPostingGroup);
        await codes.EnsureExistsAsync<VatProductPostingGroup>(input.VatProdPostingGroup);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var item = await GetEntityByIdAsync(id);
        await _itemManager.EnsureCanDeleteAsync(item);
        await Repository.DeleteAsync(item, autoSave: true);
    }

    public async Task<ItemDto> GetByNoAsync(string no)
    {
        var item = await Repository.FirstOrDefaultAsync(x => x.No == no);
        return await MapToGetOutputDtoAsync(item);
    }

    [Authorize(ErpPermissions.Items.Update)]
    public async Task BlockAsync(Guid id)
    {
        var item = await GetEntityByIdAsync(id);
        item.Block();
        await Repository.UpdateAsync(item, autoSave: true);
    }

    [Authorize(ErpPermissions.Items.Update)]
    public async Task UnblockAsync(Guid id)
    {
        var item = await GetEntityByIdAsync(id);
        item.Unblock();
        await Repository.UpdateAsync(item, autoSave: true);
    }

    protected override async Task<IQueryable<Item>> CreateFilteredQueryAsync(GetItemListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        // Lower-cased on both sides: lookups search the way Odoo's ilike does, on any provider.
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter) || (x.Description != null && x.Description.ToLower().Contains(filter))
            )
            .WhereIf(input.Type.HasValue, x => x.Type == input.Type.Value)
            .WhereIf(input.ItemCategoryId.HasValue, x => x.ItemCategoryId == input.ItemCategoryId.Value);
    }
}
