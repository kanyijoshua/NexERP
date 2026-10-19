using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>Posted receipts and return shipments: read only, apart from undoing a line not invoiced yet.</summary>
[Authorize(ErpPermissions.PurchaseDocuments.Default)]
public class PostedPurchaseReceiptAppService
    : ErpReadOnlyAppService<PostedPurchaseReceipt, PostedPurchaseReceiptDto, Guid, GetPostedPurchaseDocumentListInput>,
        IPostedPurchaseReceiptAppService
{
    private readonly PurchasePostingEngine _postingEngine;

    public PostedPurchaseReceiptAppService(IRepository<PostedPurchaseReceipt, Guid> repository, PurchasePostingEngine postingEngine)
        : base(repository)
    {
        _postingEngine = postingEngine;
        GetPolicyName = ErpPermissions.PurchaseDocuments.Default;
        GetListPolicyName = ErpPermissions.PurchaseDocuments.Default;
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Post)]
    public async Task UndoLineAsync(Guid id)
    {
        await _postingEngine.UndoReceiptLineAsync(id);
    }

    protected override async Task<IQueryable<PostedPurchaseReceipt>> CreateFilteredQueryAsync(GetPostedPurchaseDocumentListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(!filter.IsNullOrEmpty(), x => x.No.ToLower().Contains(filter) || x.BuyFromVendorNo.ToLower().Contains(filter)
                || (x.BuyFromVendorName != null && x.BuyFromVendorName.ToLower().Contains(filter)) || (x.OrderNo != null && x.OrderNo.ToLower().Contains(filter)))
            .WhereIf(input.Return.HasValue, x => x.Return == input.Return.Value)
            .WhereIf(input.VendorId.HasValue, x => x.VendorId == input.VendorId.Value)
            .WhereIf(!input.OrderNo.IsNullOrWhiteSpace(), x => x.OrderNo == input.OrderNo);
    }

    protected override IQueryable<PostedPurchaseReceipt> ApplyDefaultSorting(IQueryable<PostedPurchaseReceipt> query)
    {
        return query.OrderByDescending(x => x.PostingDate).ThenByDescending(x => x.No);
    }
}

/// <summary>Posted purchase invoices and credit memos, read only.</summary>
[Authorize(ErpPermissions.PurchaseDocuments.Default)]
public class PostedPurchaseInvoiceAppService
    : ErpReadOnlyAppService<PostedPurchaseHeader, PostedPurchaseInvoiceDto, Guid, GetPostedPurchaseDocumentListInput>,
        IPostedPurchaseInvoiceAppService
{
    public PostedPurchaseInvoiceAppService(IRepository<PostedPurchaseHeader, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.PurchaseDocuments.Default;
        GetListPolicyName = ErpPermissions.PurchaseDocuments.Default;
    }

    protected override async Task<IQueryable<PostedPurchaseHeader>> CreateFilteredQueryAsync(GetPostedPurchaseDocumentListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(!filter.IsNullOrEmpty(), x => x.No.ToLower().Contains(filter) || x.BuyFromVendorNo.ToLower().Contains(filter)
                || (x.BuyFromVendorName != null && x.BuyFromVendorName.ToLower().Contains(filter)) || (x.OrderNo != null && x.OrderNo.ToLower().Contains(filter)))
            .WhereIf(input.Return.HasValue, x => x.CreditMemo == input.Return.Value)
            .WhereIf(input.VendorId.HasValue, x => x.VendorId == input.VendorId.Value)
            .WhereIf(!input.OrderNo.IsNullOrWhiteSpace(), x => x.OrderNo == input.OrderNo);
    }

    protected override IQueryable<PostedPurchaseHeader> ApplyDefaultSorting(IQueryable<PostedPurchaseHeader> query)
    {
        return query.OrderByDescending(x => x.PostingDate).ThenByDescending(x => x.No);
    }
}
