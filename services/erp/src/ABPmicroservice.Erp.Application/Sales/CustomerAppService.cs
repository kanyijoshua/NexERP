using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Sales;

[Authorize(ErpPermissions.Customers.Default)]
public class CustomerAppService
    : CrudAppService<
        Customer,
        CustomerDto,
        Guid,
        GetCustomerListInput,
        CreateUpdateCustomerDto,
        CreateUpdateCustomerDto
    >,
        ICustomerAppService
{
    private readonly CustomerManager _customerManager;
    private readonly SalesReceivablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;

    public CustomerAppService(
        IRepository<Customer, Guid> repository,
        CustomerManager customerManager,
        SalesReceivablesSetupManager setupManager,
        NoSeriesManager noSeriesManager
    )
        : base(repository)
    {
        _customerManager = customerManager;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        GetPolicyName = ErpPermissions.Customers.Default;
        GetListPolicyName = ErpPermissions.Customers.Default;
        CreatePolicyName = ErpPermissions.Customers.Create;
        UpdatePolicyName = ErpPermissions.Customers.Update;
        DeletePolicyName = ErpPermissions.Customers.Delete;
    }

    public override async Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
    {
        // Blank takes the next number of the Customer Nos. series (BC: InitSeries).
        var setup = await _setupManager.GetAsync();
        var no = await _noSeriesManager.ResolveNoAsync(setup.CustomerNos, input.No, Clock.Now);

        var customer = await _customerManager.CreateAsync(no, input.Name);
        ApplyInput(customer, input);

        await Repository.InsertAsync(customer, autoSave: true);
        return await MapToGetOutputDtoAsync(customer);
    }

    public override async Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input)
    {
        var customer = await GetEntityByIdAsync(id);

        if (!string.Equals(customer.No, input.No, StringComparison.OrdinalIgnoreCase))
        {
            await _customerManager.EnsureNoIsUniqueAsync(input.No, id);
            customer.SetNo(input.No);
        }

        customer.SetName(input.Name);
        ApplyInput(customer, input);

        await Repository.UpdateAsync(customer, autoSave: true);
        return await MapToGetOutputDtoAsync(customer);
    }

    public async Task<CustomerDto> GetByNoAsync(string no)
    {
        var customer = await Repository.FirstOrDefaultAsync(x => x.No == no);
        return await MapToGetOutputDtoAsync(customer);
    }

    [Authorize(ErpPermissions.Customers.Update)]
    public async Task BlockAsync(Guid id)
    {
        var customer = await GetEntityByIdAsync(id);
        customer.Block();
        await Repository.UpdateAsync(customer, autoSave: true);
    }

    [Authorize(ErpPermissions.Customers.Update)]
    public async Task UnblockAsync(Guid id)
    {
        var customer = await GetEntityByIdAsync(id);
        customer.Unblock();
        await Repository.UpdateAsync(customer, autoSave: true);
    }

    private static void ApplyInput(Customer customer, CreateUpdateCustomerDto input)
    {
        customer.SetAddress(input.Address, input.City, input.PostCode, input.CountryRegionCode);
        customer.SetContact(input.PhoneNo, input.Email);
        customer.SetCreditLimit(input.CreditLimit);
        customer.SetPaymentTerms(input.PaymentTermsCode);
        customer.SetPostingGroups(input.CustomerPostingGroup, input.GenBusPostingGroup);
        customer.SetCurrency(input.CurrencyCode);
    }

    protected override async Task<IQueryable<Customer>> CreateFilteredQueryAsync(
        GetCustomerListInput input
    )
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(
                !input.Filter.IsNullOrWhiteSpace(),
                x => x.No.Contains(input.Filter) || x.Name.Contains(input.Filter)
            )
            .WhereIf(input.Blocked.HasValue, x => x.Blocked == input.Blocked.Value);
    }
}
