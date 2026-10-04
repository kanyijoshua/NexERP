using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Permissions;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Sales;

/// <summary>Customer Bank Accounts.</summary>
public class CustomerBankAccountAppService : ErpTableAppService<CustomerBankAccount, CustomerBankAccountDto, GetCustomerBankAccountListInput, CreateUpdateCustomerBankAccountDto>, ICustomerBankAccountAppService
{
    public CustomerBankAccountAppService(IRepository<CustomerBankAccount, Guid> repository)
        : base(repository, ErpPermissions.Customers.Default) { }

    public override async Task<CustomerBankAccountDto> CreateAsync(CreateUpdateCustomerBankAccountDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.CustomerNo), CodeTableEntity.NormalizeCode(input.Code), null);

        var entity = new CustomerBankAccount(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.CustomerNo), CodeTableEntity.NormalizeCode(input.Code));
        entity.Set(
            input.Name,
            input.Name2,
            input.Address,
            input.Address2,
            input.City,
            input.PostCode,
            input.Contact,
            input.PhoneNo,
            input.BankBranchNo,
            input.BankAccountNo,
            input.TransitNo,
            input.CurrencyCode,
            input.CountryRegionCode,
            input.County,
            input.FaxNo,
            input.Email,
            input.Iban,
            input.SwiftCode,
            input.BankClearingCode,
            input.BankClearingStandard
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<CustomerBankAccountDto> UpdateAsync(Guid id, CreateUpdateCustomerBankAccountDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.CustomerNo), CodeTableEntity.NormalizeCode(input.Code), id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.CustomerNo), CodeTableEntity.NormalizeCode(input.Code));
        entity.Set(
            input.Name,
            input.Name2,
            input.Address,
            input.Address2,
            input.City,
            input.PostCode,
            input.Contact,
            input.PhoneNo,
            input.BankBranchNo,
            input.BankAccountNo,
            input.TransitNo,
            input.CurrencyCode,
            input.CountryRegionCode,
            input.County,
            input.FaxNo,
            input.Email,
            input.Iban,
            input.SwiftCode,
            input.BankClearingCode,
            input.BankClearingStandard
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<CustomerBankAccount>> CreateFilteredQueryAsync(GetCustomerBankAccountListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var customerNo = input.CustomerNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!customerNo.IsNullOrEmpty(), x => x.CustomerNo == customerNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.CustomerNo.ToLower().Contains(filter)
                    || x.Code.ToLower().Contains(filter)
                    || (x.Name != null && x.Name.ToLower().Contains(filter))
                    || (x.Name2 != null && x.Name2.ToLower().Contains(filter))
                    || (x.Address != null && x.Address.ToLower().Contains(filter))
                    || (x.Address2 != null && x.Address2.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<CustomerBankAccount> ApplyDefaultSorting(IQueryable<CustomerBankAccount> query) =>
        query.OrderBy(x => x.CustomerNo).ThenBy(x => x.Code);

    private async Task ValidateAsync(CreateUpdateCustomerBankAccountDto input)
    {
        await Relations.EnsureNoExistsAsync<Customer>(input.CustomerNo);
        await CodeTableChecker.EnsureExistsAsync<Currency>(input.CurrencyCode);
        await CodeTableChecker.EnsureExistsAsync<CountryRegion>(input.CountryRegionCode);
    }

    private async Task EnsureKeyIsUniqueAsync(string customerNo, string code, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.CustomerNo == customerNo && x.Code == code && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Customer Bank Account").WithData("key", customerNo + " " + code);
        }
    }
}

/// <summary>Detailed Cust. Ledg. Entries.</summary>
public class DetailedCustLedgEntryAppService : ErpReadOnlyAppService<DetailedCustLedgEntry, DetailedCustLedgEntryDto, Guid, GetDetailedCustLedgEntryListInput>, IDetailedCustLedgEntryAppService
{
    public DetailedCustLedgEntryAppService(IRepository<DetailedCustLedgEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Customers.Default;
        GetListPolicyName = ErpPermissions.Customers.Default;
    }

    protected override async Task<IQueryable<DetailedCustLedgEntry>> CreateFilteredQueryAsync(GetDetailedCustLedgEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var customerNo = input.CustomerNo?.Trim().ToUpperInvariant();
        var documentNo = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!customerNo.IsNullOrEmpty(), x => x.CustomerNo == customerNo)
            .WhereIf(!documentNo.IsNullOrEmpty(), x => x.DocumentNo == documentNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => (x.DocumentNo != null && x.DocumentNo.ToLower().Contains(filter))
                    || (x.CustomerNo != null && x.CustomerNo.ToLower().Contains(filter))
                    || (x.CurrencyCode != null && x.CurrencyCode.ToLower().Contains(filter))
                    || (x.UserId != null && x.UserId.ToLower().Contains(filter))
                    || (x.SourceCode != null && x.SourceCode.ToLower().Contains(filter))
                    || (x.JournalBatchName != null && x.JournalBatchName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<DetailedCustLedgEntry> ApplyDefaultSorting(IQueryable<DetailedCustLedgEntry> query) =>
        query.OrderByDescending(x => x.EntryNo);
}
