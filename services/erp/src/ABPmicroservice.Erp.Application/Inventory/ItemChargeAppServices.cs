using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>Item Charges: freight, duty, insurance and the like, with the posting groups they post under.</summary>
[Authorize(ErpPermissions.ItemCharges.Default)]
public class ItemChargeAppService
    : CodeTableAppServiceBase<ItemCharge, ItemChargeDto, CreateUpdateItemChargeDto>,
        IItemChargeAppService
{
    public ItemChargeAppService(IRepository<ItemCharge, Guid> repository)
        : base(repository, ErpPermissions.ItemCharges.Default) { }

    protected override ItemCharge NewEntity(Guid id, CreateUpdateItemChargeDto input) =>
        new(id, input.Code, input.Description, input.GenProdPostingGroup, input.VatProdPostingGroup);

    protected override async Task ApplyAsync(ItemCharge entity, CreateUpdateItemChargeDto input)
    {
        var postingSetup = LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>();
        await postingSetup.EnsureGroupExistsAsync<GenProductPostingGroup>(input.GenProdPostingGroup);
        await postingSetup.EnsureGroupExistsAsync<VatProductPostingGroup>(input.VatProdPostingGroup);
        entity.SetPostingGroups(input.GenProdPostingGroup, input.VatProdPostingGroup);
    }
}

/// <summary>
/// Which receipts or shipments a charge line adds its cost to. Assignments are posted only by
/// the invoice that posts the charge line.
/// </summary>
[Authorize(ErpPermissions.ItemCharges.Default)]
public class ItemChargeAssignmentAppService
    : ErpCrudAppService<
        ItemChargeAssignment,
        ItemChargeAssignmentDto,
        Guid,
        GetItemChargeAssignmentListInput,
        CreateItemChargeAssignmentDto,
        UpdateItemChargeAssignmentDto
    >,
        IItemChargeAssignmentAppService
{
    private readonly ItemChargeManager _itemChargeManager;
    private readonly IRepository<ItemLedgerEntry, Guid> _itemLedgerEntryRepository;

    public ItemChargeAssignmentAppService(
        IRepository<ItemChargeAssignment, Guid> repository,
        IRepository<ItemLedgerEntry, Guid> itemLedgerEntryRepository,
        ItemChargeManager itemChargeManager
    )
        : base(repository)
    {
        _itemChargeManager = itemChargeManager;
        _itemLedgerEntryRepository = itemLedgerEntryRepository;
        GetPolicyName = ErpPermissions.ItemCharges.Default;
        GetListPolicyName = ErpPermissions.ItemCharges.Default;
        CreatePolicyName = ErpPermissions.ItemCharges.Assign;
        UpdatePolicyName = ErpPermissions.ItemCharges.Assign;
        DeletePolicyName = ErpPermissions.ItemCharges.Assign;
    }

    public override async Task<ItemChargeAssignmentDto> CreateAsync(CreateItemChargeAssignmentDto input)
    {
        await CheckCreatePolicyAsync();
        var assignment = await _itemChargeManager.AssignAsync(
            input.SourceType,
            input.SourceLineId,
            input.AppliesToEntryId,
            input.QtyToAssign,
            input.AmountToAssign
        );
        return await MapToGetOutputDtoAsync(assignment);
    }

    public override async Task<ItemChargeAssignmentDto> UpdateAsync(Guid id, UpdateItemChargeAssignmentDto input)
    {
        await CheckUpdatePolicyAsync();
        var assignment = await GetEntityByIdAsync(id);
        await _itemChargeManager.UpdateAssignmentAsync(assignment, input.QtyToAssign, input.AmountToAssign);
        return await MapToGetOutputDtoAsync(assignment);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var assignment = await GetEntityByIdAsync(id);
        if (assignment.Posted)
        {
            throw new BusinessException(ErpErrorCodes.ItemCharges.AssignmentAlreadyPosted);
        }

        await Repository.DeleteAsync(assignment, autoSave: true);
    }

    /// <summary>Replaces the open assignments of a line with an even, amount, weight or volume split.</summary>
    [Authorize(ErpPermissions.ItemCharges.Assign)]
    public async Task<List<ItemChargeAssignmentDto>> SuggestAllocationAsync(SuggestItemChargeAllocationInput input)
    {
        var assignments = await _itemChargeManager.SuggestAllocationAsync(
            input.SourceType,
            input.SourceLineId,
            input.Method,
            input.TargetItemLedgerEntryIds ?? []
        );
        return ObjectMapper.Map<List<ItemChargeAssignment>, List<ItemChargeAssignmentDto>>(assignments);
    }

    /// <summary>
    /// The entries a charge line can be assigned to: purchase receipts for a purchase charge,
    /// sales shipments for a sales charge, of the same vendor or customer, newest first.
    /// Routed as GET /api/erp/item-charge-assignment/assignable-entries.
    /// </summary>
    public async Task<List<ItemLedgerEntryDto>> GetAssignableEntriesAsync(GetAssignableEntriesInput input)
    {
        var (entryType, documentNos) = await GetSourceDocumentsAsync(input.SourceType, input.SourceLineId);
        var filter = input.Filter?.Trim().ToLower();

        var query = (await _itemLedgerEntryRepository.GetQueryableAsync())
            .Where(e => e.EntryType == entryType && documentNos.Contains(e.DocumentNo))
            .Where(e => entryType == ItemLedgerEntryType.Purchase ? e.Quantity > 0m : e.Quantity < 0m)
            .WhereIf(!filter.IsNullOrEmpty(), e => e.ItemNo.ToLower().Contains(filter) || e.DocumentNo.ToLower().Contains(filter));

        var entries = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.PostingDate).ThenByDescending(e => e.EntryNo).Take(input.MaxResultCount > 0 ? input.MaxResultCount : 200)
        );
        return ObjectMapper.Map<List<ItemLedgerEntry>, List<ItemLedgerEntryDto>>(entries);
    }

    /// <summary>
    /// The posted receipts (or shipments) and posted invoices of the line's vendor (or customer):
    /// the documents whose item entries the charge can land on. A direct invoice posts its stock
    /// under its own number, which is why the invoices count too.
    /// </summary>
    private async Task<(ItemLedgerEntryType EntryType, List<string> DocumentNos)> GetSourceDocumentsAsync(
        ItemChargeSourceType sourceType,
        Guid sourceLineId
    )
    {
        const int maxDocuments = 500;

        if (sourceType == ItemChargeSourceType.PurchaseLine)
        {
            var line = await LazyServiceProvider.LazyGetRequiredService<IRepository<PurchaseLine, Guid>>().GetAsync(sourceLineId);
            var header = await LazyServiceProvider.LazyGetRequiredService<IRepository<PurchaseHeader, Guid>>().GetAsync(line.PurchaseHeaderId, includeDetails: false);
            var vendorNo = header.BuyFromVendorNo;

            var receipts = await AsyncExecuter.ToListAsync(
                (await LazyServiceProvider.LazyGetRequiredService<IRepository<PostedPurchaseReceipt, Guid>>().GetQueryableAsync())
                    .Where(r => r.BuyFromVendorNo == vendorNo && !r.Return)
                    .OrderByDescending(r => r.PostingDate)
                    .Select(r => r.No)
                    .Take(maxDocuments)
            );
            var invoices = await AsyncExecuter.ToListAsync(
                (await LazyServiceProvider.LazyGetRequiredService<IRepository<PostedPurchaseHeader, Guid>>().GetQueryableAsync())
                    .Where(h => h.BuyFromVendorNo == vendorNo && !h.CreditMemo)
                    .OrderByDescending(h => h.PostingDate)
                    .Select(h => h.No)
                    .Take(maxDocuments)
            );
            return (ItemLedgerEntryType.Purchase, receipts.Concat(invoices).Distinct().ToList());
        }

        var salesLine = await LazyServiceProvider.LazyGetRequiredService<IRepository<SalesLine, Guid>>().GetAsync(sourceLineId);
        var salesHeader = await LazyServiceProvider.LazyGetRequiredService<IRepository<SalesHeader, Guid>>().GetAsync(salesLine.SalesHeaderId, includeDetails: false);
        var customerNo = salesHeader.SellToCustomerNo;

        var shipments = await AsyncExecuter.ToListAsync(
            (await LazyServiceProvider.LazyGetRequiredService<IRepository<PostedSalesShipment, Guid>>().GetQueryableAsync())
                .Where(s => s.SellToCustomerNo == customerNo && !s.Return)
                .OrderByDescending(s => s.PostingDate)
                .Select(s => s.No)
                .Take(maxDocuments)
        );
        var salesInvoices = await AsyncExecuter.ToListAsync(
            (await LazyServiceProvider.LazyGetRequiredService<IRepository<PostedSalesHeader, Guid>>().GetQueryableAsync())
                .Where(h => h.SellToCustomerNo == customerNo && !h.CreditMemo)
                .OrderByDescending(h => h.PostingDate)
                .Select(h => h.No)
                .Take(maxDocuments)
        );
        return (ItemLedgerEntryType.Sale, shipments.Concat(salesInvoices).Distinct().ToList());
    }

    protected override async Task<IQueryable<ItemChargeAssignment>> CreateFilteredQueryAsync(GetItemChargeAssignmentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.SourceDocumentNo.ToLower().Contains(filter)
                     || x.ItemChargeNo.ToLower().Contains(filter)
                     || x.ItemNo.ToLower().Contains(filter)
                     || (x.AppliesToDocumentNo != null && x.AppliesToDocumentNo.ToLower().Contains(filter))
            )
            .WhereIf(input.SourceType.HasValue, x => x.SourceType == input.SourceType.Value)
            .WhereIf(input.SourceLineId.HasValue, x => x.SourceLineId == input.SourceLineId.Value)
            .WhereIf(!input.SourceDocumentNo.IsNullOrWhiteSpace(), x => x.SourceDocumentNo == input.SourceDocumentNo)
            .WhereIf(input.Posted.HasValue, x => x.Posted == input.Posted.Value);
    }

    protected override IQueryable<ItemChargeAssignment> ApplyDefaultSorting(IQueryable<ItemChargeAssignment> query)
    {
        return query.OrderBy(x => x.SourceDocumentNo).ThenBy(x => x.SourceLineNo).ThenBy(x => x.AppliesToEntryNo);
    }
}
