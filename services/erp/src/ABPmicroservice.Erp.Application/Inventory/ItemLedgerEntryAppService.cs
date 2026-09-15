using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Inventory;

[Authorize(ErpPermissions.Items.Default)]
public class ItemLedgerEntryAppService
    : ReadOnlyAppService<ItemLedgerEntry, ItemLedgerEntryDto, Guid, GetItemLedgerEntryListInput>,
        IItemLedgerEntryAppService
{
    public ItemLedgerEntryAppService(IRepository<ItemLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Items.Default;
        GetListPolicyName = ErpPermissions.Items.Default;
    }

    protected override async Task<IQueryable<ItemLedgerEntry>> CreateFilteredQueryAsync(
        GetItemLedgerEntryListInput input
    )
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(input.ItemId.HasValue, x => x.ItemId == input.ItemId.Value)
            .WhereIf(
                !input.DocumentNo.IsNullOrWhiteSpace(),
                x => x.DocumentNo == input.DocumentNo
            )
            .WhereIf(input.EntryType.HasValue, x => x.EntryType == input.EntryType.Value);
    }
}
