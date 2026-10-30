using System;
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

namespace ABPmicroservice.Erp.Intercompany;

public class ICPartnerAppService : CodeTableAppServiceBase<ICPartner, ICPartnerDto, CreateUpdateICPartnerDto>, IICPartnerAppService
{
    public ICPartnerAppService(IRepository<ICPartner, Guid> repository)
        : base(repository, ErpPermissions.Intercompany.Default) { }

    protected override ICPartner NewEntity(Guid id, CreateUpdateICPartnerDto input) => new(id, input.Code, input.Description, input.PartnerCompanyId);

    protected override async Task ApplyAsync(ICPartner entity, CreateUpdateICPartnerDto input)
    {
        if (input.PartnerCompanyId == Guid.Empty || input.PartnerCompanyId == LazyServiceProvider.LazyGetRequiredService<ICurrentCompany>().Id
            || !await LazyServiceProvider.LazyGetRequiredService<IRepository<Company, Guid>>().AnyAsync(c => c.Id == input.PartnerCompanyId))
        {
            throw new BusinessException(ErpErrorCodes.Intercompany.PartnerCompanyMissing).WithData("code", CodeTableEntity.NormalizeCode(input.Code) ?? "");
        }

        var customerNo = input.CustomerNo?.Trim();
        if (!customerNo.IsNullOrEmpty() && !await LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>().AnyAsync(c => c.No == customerNo))
        {
            throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", customerNo);
        }

        var vendorNo = input.VendorNo?.Trim();
        if (!vendorNo.IsNullOrEmpty() && !await LazyServiceProvider.LazyGetRequiredService<IRepository<Vendor, Guid>>().AnyAsync(v => v.No == vendorNo))
        {
            throw new BusinessException(ErpErrorCodes.Vendors.VendorNotFound).WithData("accountNo", vendorNo);
        }

        await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().EnsureGLAccountsExistAsync(input.DefaultGLAccountNo);
        entity.Set(input.PartnerCompanyId, customerNo, vendorNo, input.DefaultGLAccountNo, input.Blocked);
    }
}

[Authorize(ErpPermissions.Intercompany.Default)]
public class ICOutboxTransactionAppService
    : ErpReadOnlyAppService<ICOutboxTransaction, ICOutboxTransactionDto, Guid, GetICTransactionListInput>,
        IICOutboxTransactionAppService
{
    private IntercompanyManager Manager => LazyServiceProvider.LazyGetRequiredService<IntercompanyManager>();

    public ICOutboxTransactionAppService(IRepository<ICOutboxTransaction, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Intercompany.Default;
        GetListPolicyName = ErpPermissions.Intercompany.Default;
    }

    [Authorize(ErpPermissions.Intercompany.Process)]
    public async Task<ICSendResultDto> SendAsync(Guid id)
    {
        await Manager.SendAsync(id);
        return new ICSendResultDto { SentCount = 1 };
    }

    /// <summary>Sends every new transaction, oldest first.</summary>
    [Authorize(ErpPermissions.Intercompany.Process)]
    public async Task<ICSendResultDto> SendAllAsync()
    {
        var pending = (await ReadOnlyRepository.GetListAsync(t => t.Status == ICOutboxStatus.New)).OrderBy(t => t.TransactionNo).ToList();
        foreach (var transaction in pending)
        {
            await Manager.SendAsync(transaction.Id);
        }

        return new ICSendResultDto { SentCount = pending.Count };
    }

    [Authorize(ErpPermissions.Intercompany.Process)]
    public async Task CancelAsync(Guid id) => await Manager.CancelAsync(id);

    protected override async Task<IQueryable<ICOutboxTransaction>> CreateFilteredQueryAsync(GetICTransactionListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var partner = CodeTableEntity.NormalizeCode(input.PartnerCode);
        return query
            .WhereIf(partner != null, t => t.PartnerCode == partner)
            .WhereIf(input.Status.HasValue, t => (int)t.Status == input.Status.Value);
    }

    protected override IQueryable<ICOutboxTransaction> ApplyDefaultSorting(IQueryable<ICOutboxTransaction> query) => query.OrderByDescending(t => t.TransactionNo);
}

[Authorize(ErpPermissions.Intercompany.Default)]
public class ICInboxTransactionAppService
    : ErpReadOnlyAppService<ICInboxTransaction, ICInboxTransactionDto, Guid, GetICTransactionListInput>,
        IICInboxTransactionAppService
{
    private IntercompanyManager Manager => LazyServiceProvider.LazyGetRequiredService<IntercompanyManager>();

    public ICInboxTransactionAppService(IRepository<ICInboxTransaction, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Intercompany.Default;
        GetListPolicyName = ErpPermissions.Intercompany.Default;
    }

    [Authorize(ErpPermissions.Intercompany.Process)]
    public async Task<ICAcceptResultDto> AcceptAsync(Guid id, AcceptICInboxInput input) =>
        new() { DocumentNo = await Manager.AcceptAsync(id, input?.PostingDate ?? Clock.Now.Date) };

    [Authorize(ErpPermissions.Intercompany.Process)]
    public async Task RejectAsync(Guid id) => await Manager.RejectAsync(id);

    protected override async Task<IQueryable<ICInboxTransaction>> CreateFilteredQueryAsync(GetICTransactionListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var partner = CodeTableEntity.NormalizeCode(input.PartnerCode);
        return query
            .WhereIf(partner != null, t => t.PartnerCode == partner)
            .WhereIf(input.Status.HasValue, t => (int)t.Status == input.Status.Value);
    }

    protected override IQueryable<ICInboxTransaction> ApplyDefaultSorting(IQueryable<ICInboxTransaction> query) => query.OrderByDescending(t => t.TransactionNo);
}

[Authorize(ErpPermissions.Intercompany.Default)]
public class ICInboxLineAppService : ErpReadOnlyAppService<ICInboxLine, ICInboxLineDto, Guid, GetICInboxLineListInput>, IICInboxLineAppService
{
    public ICInboxLineAppService(IRepository<ICInboxLine, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Intercompany.Default;
        GetListPolicyName = ErpPermissions.Intercompany.Default;
    }

    protected override async Task<IQueryable<ICInboxLine>> CreateFilteredQueryAsync(GetICInboxLineListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        return query.WhereIf(input.ICInboxTransactionId.HasValue, l => l.ICInboxTransactionId == input.ICInboxTransactionId.Value);
    }

    protected override IQueryable<ICInboxLine> ApplyDefaultSorting(IQueryable<ICInboxLine> query) => query.OrderBy(l => l.LineNo);
}
