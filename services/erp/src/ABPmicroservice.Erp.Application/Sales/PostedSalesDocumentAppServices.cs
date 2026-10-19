using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Sales;

/// <summary>Posted shipments and return receipts: read only, apart from undoing a line not invoiced yet.</summary>
[Authorize(ErpPermissions.SalesDocuments.Default)]
public class PostedSalesShipmentAppService
    : ErpReadOnlyAppService<PostedSalesShipment, PostedSalesShipmentDto, Guid, GetPostedSalesDocumentListInput>,
        IPostedSalesShipmentAppService
{
    private readonly SalesPostingEngine _postingEngine;

    public PostedSalesShipmentAppService(IRepository<PostedSalesShipment, Guid> repository, SalesPostingEngine postingEngine)
        : base(repository)
    {
        _postingEngine = postingEngine;
        GetPolicyName = ErpPermissions.SalesDocuments.Default;
        GetListPolicyName = ErpPermissions.SalesDocuments.Default;
    }

    [Authorize(ErpPermissions.SalesDocuments.Post)]
    public async Task UndoLineAsync(Guid id)
    {
        await _postingEngine.UndoShipmentLineAsync(id);
    }

    protected override async Task<IQueryable<PostedSalesShipment>> CreateFilteredQueryAsync(GetPostedSalesDocumentListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(!filter.IsNullOrEmpty(), x => x.No.ToLower().Contains(filter) || x.SellToCustomerNo.ToLower().Contains(filter)
                || (x.SellToCustomerName != null && x.SellToCustomerName.ToLower().Contains(filter)) || (x.OrderNo != null && x.OrderNo.ToLower().Contains(filter)))
            .WhereIf(input.Return.HasValue, x => x.Return == input.Return.Value)
            .WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId.Value)
            .WhereIf(!input.OrderNo.IsNullOrWhiteSpace(), x => x.OrderNo == input.OrderNo);
    }

    protected override IQueryable<PostedSalesShipment> ApplyDefaultSorting(IQueryable<PostedSalesShipment> query)
    {
        return query.OrderByDescending(x => x.PostingDate).ThenByDescending(x => x.No);
    }
}

/// <summary>Posted sales invoices and credit memos, read only.</summary>
[Authorize(ErpPermissions.SalesDocuments.Default)]
public class PostedSalesInvoiceAppService
    : ErpReadOnlyAppService<PostedSalesHeader, PostedSalesInvoiceDto, Guid, GetPostedSalesDocumentListInput>,
        IPostedSalesInvoiceAppService
{
    public PostedSalesInvoiceAppService(IRepository<PostedSalesHeader, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.SalesDocuments.Default;
        GetListPolicyName = ErpPermissions.SalesDocuments.Default;
    }

    protected override async Task<IQueryable<PostedSalesHeader>> CreateFilteredQueryAsync(GetPostedSalesDocumentListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(!filter.IsNullOrEmpty(), x => x.No.ToLower().Contains(filter) || x.SellToCustomerNo.ToLower().Contains(filter)
                || (x.SellToCustomerName != null && x.SellToCustomerName.ToLower().Contains(filter)) || (x.OrderNo != null && x.OrderNo.ToLower().Contains(filter)))
            .WhereIf(input.Return.HasValue, x => x.CreditMemo == input.Return.Value)
            .WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId.Value)
            .WhereIf(!input.OrderNo.IsNullOrWhiteSpace(), x => x.OrderNo == input.OrderNo);
    }

    protected override IQueryable<PostedSalesHeader> ApplyDefaultSorting(IQueryable<PostedSalesHeader> query)
    {
        return query.OrderByDescending(x => x.PostingDate).ThenByDescending(x => x.No);
    }
}
