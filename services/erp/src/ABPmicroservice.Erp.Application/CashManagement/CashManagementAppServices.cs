using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.CashManagement;

/// <summary>Bank Account Posting Groups.</summary>
public class BankAccountPostingGroupAppService
    : PostingGroupAppServiceBase<BankAccountPostingGroup, BankAccountPostingGroupDto, CreateUpdateBankAccountPostingGroupDto>,
        IBankAccountPostingGroupAppService
{
    public BankAccountPostingGroupAppService(IRepository<BankAccountPostingGroup, Guid> repository)
        : base(repository) { }

    protected override BankAccountPostingGroup NewEntity(Guid id, CreateUpdateBankAccountPostingGroupDto input) =>
        new(id, input.Code, input.GLAccountNo, input.Description);

    protected override async Task ApplyAsync(BankAccountPostingGroup entity, CreateUpdateBankAccountPostingGroupDto input)
    {
        await PostingSetupManager.EnsureGLAccountsExistAsync(input.GLAccountNo);
        entity.SetGLAccount(input.GLAccountNo);
    }
}

/// <summary>Bank Accounts.</summary>
[Authorize(ErpPermissions.BankAccounts.Default)]
public class BankAccountAppService
    : ErpCrudAppService<BankAccount, BankAccountDto, Guid, GetBankAccountListInput, CreateUpdateBankAccountDto, CreateUpdateBankAccountDto>,
        IBankAccountAppService
{
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly CodeTableChecker _codeTableChecker;
    private readonly IRepository<BankAccountLedgerEntry, Guid> _ledgerEntryRepository;

    public BankAccountAppService(
        IRepository<BankAccount, Guid> repository,
        GeneralLedgerSetupManager glSetupManager,
        NoSeriesManager noSeriesManager,
        CodeTableChecker codeTableChecker,
        IRepository<BankAccountLedgerEntry, Guid> ledgerEntryRepository
    )
        : base(repository)
    {
        _glSetupManager = glSetupManager;
        _noSeriesManager = noSeriesManager;
        _codeTableChecker = codeTableChecker;
        _ledgerEntryRepository = ledgerEntryRepository;
        GetPolicyName = ErpPermissions.BankAccounts.Default;
        GetListPolicyName = ErpPermissions.BankAccounts.Default;
        CreatePolicyName = ErpPermissions.BankAccounts.Create;
        UpdatePolicyName = ErpPermissions.BankAccounts.Update;
        DeletePolicyName = ErpPermissions.BankAccounts.Delete;
    }

    public override async Task<BankAccountDto> CreateAsync(CreateUpdateBankAccountDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        // Blank takes the next number of the Bank Account Nos. series.
        var setup = await _glSetupManager.GetAsync();
        var no = await _noSeriesManager.ResolveNoAsync(setup.BankAccountNos, input.No, Clock.Now);
        await EnsureNoIsUniqueAsync(no, null);

        var bankAccount = new BankAccount(GuidGenerator.Create(), no, input.Name);
        Apply(bankAccount, input);

        await Repository.InsertAsync(bankAccount, autoSave: true);
        return await MapToGetOutputDtoAsync(bankAccount);
    }

    public override async Task<BankAccountDto> UpdateAsync(Guid id, CreateUpdateBankAccountDto input)
    {
        await CheckUpdatePolicyAsync();

        var bankAccount = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        if (!input.No.IsNullOrWhiteSpace() && !string.Equals(bankAccount.No, input.No.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            await EnsureNoIsUniqueAsync(input.No.Trim(), id);
            bankAccount.SetNo(input.No);
        }

        bankAccount.SetName(input.Name);
        Apply(bankAccount, input);

        await Repository.UpdateAsync(bankAccount, autoSave: true);
        return await MapToGetOutputDtoAsync(bankAccount);
    }

    /// <summary>A bank account with entries keeps its history; block it instead.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        if (await _ledgerEntryRepository.AnyAsync(e => e.BankAccountId == id))
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.CannotDeleteWithEntries);
        }

        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.BankAccounts.Update)]
    public async Task BlockAsync(Guid id)
    {
        var bankAccount = await GetEntityByIdAsync(id);
        bankAccount.Block();
        await Repository.UpdateAsync(bankAccount, autoSave: true);
    }

    [Authorize(ErpPermissions.BankAccounts.Update)]
    public async Task UnblockAsync(Guid id)
    {
        var bankAccount = await GetEntityByIdAsync(id);
        bankAccount.Unblock();
        await Repository.UpdateAsync(bankAccount, autoSave: true);
    }

    protected override async Task<IQueryable<BankAccount>> CreateFilteredQueryAsync(GetBankAccountListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            x => x.No.ToLower().Contains(filter) || x.Name.ToLower().Contains(filter)
                || (x.BankAccountNo != null && x.BankAccountNo.ToLower().Contains(filter))
        );
    }

    protected override IQueryable<BankAccount> ApplyDefaultSorting(IQueryable<BankAccount> query)
    {
        return query.OrderBy(x => x.No);
    }

    private async Task ValidateAsync(CreateUpdateBankAccountDto input)
    {
        await _codeTableChecker.EnsureExistsAsync<BankAccountPostingGroup>(input.BankAccPostingGroup);
        await _codeTableChecker.EnsureExistsAsync<Currency>(input.CurrencyCode);
    }

    private async Task EnsureNoIsUniqueAsync(string no, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.No == no && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.BankAccountAlreadyExists).WithData("no", no);
        }
    }

    private static void Apply(BankAccount bankAccount, CreateUpdateBankAccountDto input)
    {
        bankAccount.SetBankDetails(input.BankAccountNo, input.BankBranchNo, input.Iban, input.SwiftCode);
        bankAccount.SetAddress(input.Address, input.City, input.Contact, input.PhoneNo);
        bankAccount.SetPosting(input.BankAccPostingGroup, input.CurrencyCode);
        bankAccount.SetAdditionalFields(
            input.Name2,
            input.Address2,
            input.PostCode,
            input.County,
            input.CountryRegionCode,
            input.Email,
            input.FaxNo,
            input.HomePage,
            input.GlobalDimension1Code,
            input.GlobalDimension2Code,
            input.OurContactCode,
            input.MinBalance,
            input.LastStatementNo,
            input.BalanceLastStatement,
            input.LastPaymentStatementNo,
            input.LastCheckNo,
            input.TransitNo,
            input.BankClearingCode
        );
    }
}

/// <summary>Bank Account Ledger Entries.</summary>
[Authorize(ErpPermissions.BankAccounts.Default)]
public class BankAccountLedgerEntryAppService
    : ErpReadOnlyAppService<BankAccountLedgerEntry, BankAccountLedgerEntryDto, Guid, GetBankAccountLedgerEntryListInput>,
        IBankAccountLedgerEntryAppService
{
    public BankAccountLedgerEntryAppService(IRepository<BankAccountLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.BankAccounts.Default;
        GetListPolicyName = ErpPermissions.BankAccounts.Default;
    }

    protected override async Task<IQueryable<BankAccountLedgerEntry>> CreateFilteredQueryAsync(GetBankAccountLedgerEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(input.BankAccountId.HasValue, x => x.BankAccountId == input.BankAccountId.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter) || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<BankAccountLedgerEntry> ApplyDefaultSorting(IQueryable<BankAccountLedgerEntry> query)
    {
        return query.OrderByDescending(x => x.EntryNo);
    }
}

/// <summary>Payment Methods.</summary>
[Authorize(ErpPermissions.FinanceSetup.Default)]
public class PaymentMethodAppService
    : CodeTableAppServiceBase<PaymentMethod, PaymentMethodDto, CreateUpdatePaymentMethodDto>,
        IPaymentMethodAppService
{
    public PaymentMethodAppService(IRepository<PaymentMethod, Guid> repository)
        : base(repository, ErpPermissions.FinanceSetup.Default) { }

    protected override PaymentMethod NewEntity(Guid id, CreateUpdatePaymentMethodDto input) =>
        new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(PaymentMethod entity, CreateUpdatePaymentMethodDto input)
    {
        if (!input.BalAccountNo.IsNullOrWhiteSpace())
        {
            if (input.BalAccountType == GenJournalAccountType.BankAccount)
            {
                var bankAccounts = LazyServiceProvider.LazyGetRequiredService<IRepository<BankAccount, Guid>>();
                if (!await bankAccounts.AnyAsync(b => b.No == input.BalAccountNo.Trim()))
                {
                    throw new BusinessException(ErpErrorCodes.CashManagement.BankAccountNotFound).WithData("accountNo", input.BalAccountNo);
                }
            }
            else
            {
                await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().EnsureGLAccountsExistAsync(input.BalAccountNo);
            }
        }

        entity.SetBalancingAccount(input.BalAccountType ?? GenJournalAccountType.GLAccount, input.BalAccountNo);
        entity.SetAdditionalFields(input.DirectDebit, input.DirectDebitPmtTermsCode, input.PmtExportLineDefinition);
    }
}
