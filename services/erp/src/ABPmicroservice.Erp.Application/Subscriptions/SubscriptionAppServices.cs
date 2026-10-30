using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Subscriptions;

public class SubscriptionPlanAppService : CodeTableAppServiceBase<SubscriptionPlan, SubscriptionPlanDto, CreateUpdateSubscriptionPlanDto>, ISubscriptionPlanAppService
{
    public SubscriptionPlanAppService(IRepository<SubscriptionPlan, Guid> repository)
        : base(repository, ErpPermissions.Subscriptions.Default) { }

    protected override SubscriptionPlan NewEntity(Guid id, CreateUpdateSubscriptionPlanDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(SubscriptionPlan entity, CreateUpdateSubscriptionPlanDto input)
    {
        var no = input.LineNo?.Trim();
        if (input.LineType == SubscriptionLineType.Item)
        {
            if (!await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().AnyAsync(i => i.No == no))
            {
                throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no ?? "");
            }
        }
        else
        {
            await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().EnsureGLAccountsExistAsync(no);
        }

        entity.Set(input.BillingPeriodMonths, input.Price, input.LineType, no, input.TrialDays);
    }
}

[Authorize(ErpPermissions.Subscriptions.Default)]
public class CustomerSubscriptionAppService
    : ErpTableAppService<CustomerSubscription, CustomerSubscriptionDto, GetCustomerSubscriptionListInput, CreateUpdateCustomerSubscriptionDto>,
        ICustomerSubscriptionAppService
{
    private SubscriptionManager Manager => LazyServiceProvider.LazyGetRequiredService<SubscriptionManager>();

    public CustomerSubscriptionAppService(IRepository<CustomerSubscription, Guid> repository)
        : base(repository, ErpPermissions.Subscriptions.Default) { }

    public override async Task<CustomerSubscriptionDto> CreateAsync(CreateUpdateCustomerSubscriptionDto input)
    {
        await CheckCreatePolicyAsync();
        var customerNo = input.CustomerNo.Trim();
        var customer = await LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>().FirstOrDefaultAsync(c => c.No == customerNo)
            ?? throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", customerNo);
        var no = input.No.IsNullOrWhiteSpace() ? await Manager.NextNoAsync() : input.No.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(s => s.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }

        var subscription = new CustomerSubscription(GuidGenerator.Create(), no, customer);
        subscription.Set(await Manager.GetPlanAsync(input.PlanCode), input.Quantity, input.UnitPrice, input.StartDate ?? Clock.Now.Date, input.EndDate);
        await Repository.InsertAsync(subscription, autoSave: true);
        return await MapToGetOutputDtoAsync(subscription);
    }

    public override async Task<CustomerSubscriptionDto> UpdateAsync(Guid id, CreateUpdateCustomerSubscriptionDto input)
    {
        await CheckUpdatePolicyAsync();
        var subscription = await GetEntityByIdAsync(id);
        subscription.Set(await Manager.GetPlanAsync(input.PlanCode), input.Quantity, input.UnitPrice, input.StartDate ?? subscription.StartDate, input.EndDate);
        await Repository.UpdateAsync(subscription, autoSave: true);
        return await MapToGetOutputDtoAsync(subscription);
    }

    [Authorize(ErpPermissions.Subscriptions.Update)]
    public async Task<CustomerSubscriptionDto> ActivateAsync(Guid id) => await ChangeAsync(id, s => s.Activate());

    [Authorize(ErpPermissions.Subscriptions.Update)]
    public async Task<CustomerSubscriptionDto> PauseAsync(Guid id) => await ChangeAsync(id, s => s.Pause());

    [Authorize(ErpPermissions.Subscriptions.Update)]
    public async Task<CustomerSubscriptionDto> CancelAsync(Guid id) => await ChangeAsync(id, s => s.Cancel());

    [Authorize(ErpPermissions.Subscriptions.Invoice)]
    public async Task<SubscriptionInvoicingResultDto> InvoiceDueAsync(Guid id, InvoiceSubscriptionsInput input) =>
        new() { InvoiceCount = await Manager.InvoiceDueAsync(input?.AsOfDate ?? Clock.Now.Date, input?.Post ?? true, id) };

    [Authorize(ErpPermissions.Subscriptions.Invoice)]
    public async Task<SubscriptionInvoicingResultDto> InvoiceAllDueAsync(InvoiceSubscriptionsInput input) =>
        new() { InvoiceCount = await Manager.InvoiceDueAsync(input?.AsOfDate ?? Clock.Now.Date, input?.Post ?? true) };

    private async Task<CustomerSubscriptionDto> ChangeAsync(Guid id, Action<CustomerSubscription> change)
    {
        var subscription = await GetEntityByIdAsync(id);
        change(subscription);
        await Repository.UpdateAsync(subscription, autoSave: true);
        return await MapToGetOutputDtoAsync(subscription);
    }

    protected override async Task<IQueryable<CustomerSubscription>> CreateFilteredQueryAsync(GetCustomerSubscriptionListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(input.Status.HasValue, s => s.Status == input.Status.Value)
            .WhereIf(!filter.IsNullOrEmpty(), s => s.No.ToLower().Contains(filter) || s.CustomerNo.ToLower().Contains(filter) || (s.CustomerName != null && s.CustomerName.ToLower().Contains(filter)));
    }

    protected override IQueryable<CustomerSubscription> ApplyDefaultSorting(IQueryable<CustomerSubscription> query) => query.OrderByDescending(s => s.No);
}
