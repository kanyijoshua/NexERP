using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.PointOfSale;

public class PosTerminalAppService : CodeTableAppServiceBase<PosTerminal, PosTerminalDto, CreateUpdatePosTerminalDto>, IPosTerminalAppService
{
    public PosTerminalAppService(IRepository<PosTerminal, Guid> repository)
        : base(repository, ErpPermissions.PointOfSale.Default) { }

    protected override PosTerminal NewEntity(Guid id, CreateUpdatePosTerminalDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(PosTerminal entity, CreateUpdatePosTerminalDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<Location>(input.LocationCode);
        var customerNo = input.WalkInCustomerNo?.Trim();
        if (!await LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>().AnyAsync(c => c.No == customerNo))
        {
            throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", customerNo ?? "");
        }

        var banks = LazyServiceProvider.LazyGetRequiredService<IRepository<BankAccount, Guid>>();
        foreach (var bankNo in new[] { input.CashBankAccountNo, input.CardBankAccountNo }.Where(b => !b.IsNullOrWhiteSpace()).Select(b => b.Trim()))
        {
            if (!await banks.AnyAsync(b => b.No == bankNo))
            {
                throw new BusinessException(ErpErrorCodes.CashManagement.BankAccountNotFound).WithData("accountNo", bankNo);
            }
        }

        entity.Set(input.LocationCode, customerNo, input.CashBankAccountNo, input.CardBankAccountNo, input.Blocked);
    }
}

[Authorize(ErpPermissions.PointOfSale.Default)]
public class PosSessionAppService : ErpReadOnlyAppService<PosSession, PosSessionDto, Guid, GetPosSessionListInput>, IPosSessionAppService
{
    private PosManager Manager => LazyServiceProvider.LazyGetRequiredService<PosManager>();

    public PosSessionAppService(IRepository<PosSession, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.PointOfSale.Default;
        GetListPolicyName = ErpPermissions.PointOfSale.Default;
    }

    [Authorize(ErpPermissions.PointOfSale.Sell)]
    public async Task<PosSessionDto> OpenAsync(OpenPosSessionInput input) =>
        await MapToGetOutputDtoAsync(await Manager.OpenSessionAsync(input.TerminalCode, input.OpeningFloat, CurrentUser.UserName));

    [Authorize(ErpPermissions.PointOfSale.Sell)]
    public async Task<PosSessionDto> CloseAsync(Guid id, ClosePosSessionInput input) =>
        await MapToGetOutputDtoAsync(await Manager.CloseSessionAsync(id, input.CountedCash));

    protected override async Task<IQueryable<PosSession>> CreateFilteredQueryAsync(GetPosSessionListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var terminal = CodeTableEntity.NormalizeCode(input.TerminalCode);
        return query.WhereIf(terminal != null, s => s.TerminalCode == terminal).WhereIf(input.Status.HasValue, s => s.Status == input.Status.Value);
    }

    protected override IQueryable<PosSession> ApplyDefaultSorting(IQueryable<PosSession> query) => query.OrderByDescending(s => s.OpenedAt);
}

[Authorize(ErpPermissions.PointOfSale.Default)]
public class PosOrderAppService : ErpReadOnlyAppService<PosOrder, PosOrderDto, Guid, GetPosOrderListInput>, IPosOrderAppService
{
    private readonly IRepository<PosOrder, Guid> _orders;

    private PosManager Manager => LazyServiceProvider.LazyGetRequiredService<PosManager>();

    public PosOrderAppService(IRepository<PosOrder, Guid> repository)
        : base(repository)
    {
        _orders = repository;
        GetPolicyName = ErpPermissions.PointOfSale.Default;
        GetListPolicyName = ErpPermissions.PointOfSale.Default;
    }

    protected override async Task<PosOrder> GetEntityByIdAsync(Guid id) => await _orders.GetAsync(id, includeDetails: true);

    [Authorize(ErpPermissions.PointOfSale.Sell)]
    public async Task<PosOrderDto> StartAsync(StartPosOrderInput input) => await MapToGetOutputDtoAsync(await Manager.NewOrderAsync(input.SessionId, input.CustomerNo));

    /// <summary>Adds an item at its own price unless another price is given.</summary>
    [Authorize(ErpPermissions.PointOfSale.Sell)]
    public async Task<PosOrderDto> RingUpAsync(Guid id, RingUpInput input)
    {
        var order = await GetEntityByIdAsync(id);
        var no = input.ItemNo?.Trim();
        var item = await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no ?? "");
        order.AddLine(GuidGenerator.Create(), item, input.Quantity, input.UnitPrice ?? item.UnitPrice, input.DiscountPct);
        await _orders.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    [Authorize(ErpPermissions.PointOfSale.Sell)]
    public async Task<PosOrderDto> PayAsync(Guid id, PayPosOrderInput input) =>
        await MapToGetOutputDtoAsync(await Manager.PayAsync(id, input.Method, input.AmountTendered));

    [Authorize(ErpPermissions.PointOfSale.Sell)]
    public async Task<PosOrderDto> CancelAsync(Guid id)
    {
        var order = await GetEntityByIdAsync(id);
        order.Cancel();
        await _orders.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    protected override async Task<IQueryable<PosOrder>> CreateFilteredQueryAsync(GetPosOrderListInput input)
    {
        var query = await _orders.WithDetailsAsync(o => o.Lines);
        return query.WhereIf(input.SessionId.HasValue, o => o.SessionId == input.SessionId.Value).WhereIf(input.Status.HasValue, o => o.Status == input.Status.Value);
    }

    protected override IQueryable<PosOrder> ApplyDefaultSorting(IQueryable<PosOrder> query) => query.OrderByDescending(o => o.No);
}

[Authorize(ErpPermissions.PointOfSale.Default)]
public class PosOrderLineAppService : ErpReadOnlyAppService<PosOrderLine, PosOrderLineDto, Guid, GetPosOrderLineListInput>, IPosOrderLineAppService
{
    private readonly IRepository<PosOrderLine, Guid> _lines;

    public PosOrderLineAppService(IRepository<PosOrderLine, Guid> repository)
        : base(repository)
    {
        _lines = repository;
        GetPolicyName = ErpPermissions.PointOfSale.Default;
        GetListPolicyName = ErpPermissions.PointOfSale.Default;
    }

    [Authorize(ErpPermissions.PointOfSale.Sell)]
    public async Task DeleteAsync(Guid id)
    {
        var line = await _lines.GetAsync(id);
        (await LazyServiceProvider.LazyGetRequiredService<IRepository<PosOrder, Guid>>().GetAsync(line.PosOrderId)).EnsureOpen();
        await _lines.DeleteAsync(line, autoSave: true);
    }

    protected override async Task<IQueryable<PosOrderLine>> CreateFilteredQueryAsync(GetPosOrderLineListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        return query.WhereIf(input.PosOrderId.HasValue, l => l.PosOrderId == input.PosOrderId.Value);
    }

    protected override IQueryable<PosOrderLine> ApplyDefaultSorting(IQueryable<PosOrderLine> query) => query.OrderBy(l => l.LineNo);
}
