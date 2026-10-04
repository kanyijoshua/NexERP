using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Sales;

[Authorize(ErpPermissions.Customers.Default)]
public class CustomerAppService
    : ErpCrudAppService<
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
        // Blank takes the next number of the Customer Nos. series.
        var setup = await _setupManager.GetAsync();
        var no = await _noSeriesManager.ResolveNoAsync(setup.CustomerNos, input.No, Clock.Now);

        var customer = await _customerManager.CreateAsync(no, input.Name);
        await EnsurePostingGroupsExistAsync(input);
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
        await EnsurePostingGroupsExistAsync(input);
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

    /// <summary>A posting group on the card must exist; blank means none.</summary>
    private async Task EnsurePostingGroupsExistAsync(CreateUpdateCustomerDto input)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<CustomerPostingGroup>(input.CustomerPostingGroup);
        await codes.EnsureExistsAsync<GenBusinessPostingGroup>(input.GenBusPostingGroup);
        await codes.EnsureExistsAsync<VatBusinessPostingGroup>(input.VatBusPostingGroup);
        await codes.EnsureExistsAsync<PaymentTerms>(input.PaymentTermsCode);
        await codes.EnsureExistsAsync<Currency>(input.CurrencyCode);
        await codes.EnsureExistsAsync<SalespersonPurchaser>(input.SalespersonCode);
        await codes.EnsureExistsAsync<PaymentMethod>(input.PaymentMethodCode);
    }

    private static void ApplyInput(Customer customer, CreateUpdateCustomerDto input)
    {
        customer.SetAddress(input.Address, input.City, input.PostCode, input.CountryRegionCode);
        customer.SetContact(input.PhoneNo, input.Email);
        customer.SetCreditLimit(input.CreditLimit);
        customer.SetPaymentTerms(CodeTableEntity.NormalizeCode(input.PaymentTermsCode));
        customer.SetPostingGroups(
            PostingGroupBase.NormalizeCode(input.CustomerPostingGroup),
            PostingGroupBase.NormalizeCode(input.GenBusPostingGroup)
        );
        customer.SetCurrency(CodeTableEntity.NormalizeCode(input.CurrencyCode));
        customer.SetVatBusPostingGroup(input.VatBusPostingGroup);
        customer.SetSalespersonCode(input.SalespersonCode);
        customer.SetPaymentMethodCode(input.PaymentMethodCode);
        customer.SetAdditionalFields(
            input.SearchName,
            input.Name2,
            input.Address2,
            input.County,
            input.Contact,
            input.MobilePhoneNo,
            input.HomePage,
            input.VatRegistrationNo,
            input.RegistrationNumber,
            input.GlobalDimension1Code,
            input.GlobalDimension2Code,
            input.LanguageCode,
            input.LocationCode,
            input.ShipmentMethodCode,
            input.ResponsibilityCenter,
            input.CustomerPriceGroup,
            input.CustomerDiscGroup,
            input.InvoiceDiscCode,
            input.FinChargeTermsCode,
            input.ReminderTermsCode,
            input.ApplicationMethod,
            input.PricesIncludingVat,
            input.TaxAreaCode,
            input.TaxLiable,
            input.BlockPaymentTolerance,
            input.PrepaymentPct,
            input.PrintStatements,
            input.LastStatementNo,
            input.CombineShipments,
            input.PreferredBankAccountCode,
            input.PrimaryContactNo,
            input.PrivacyBlocked
        );
    }

    protected override async Task<IQueryable<Customer>> CreateFilteredQueryAsync(
        GetCustomerListInput input
    )
    {
        var query = await base.CreateFilteredQueryAsync(input);

        // Lower-cased on both sides: lookups search case-insensitively, on any provider.
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter) || (x.Name != null && x.Name.ToLower().Contains(filter))
            )
            .WhereIf(input.Blocked.HasValue, x => x.Blocked == input.Blocked.Value);
    }
}
