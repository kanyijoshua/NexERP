using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

[Authorize(ErpPermissions.GLAccounts.Default)]
public class GLAccountAppService
    : CrudAppService<
        GLAccount,
        GLAccountDto,
        Guid,
        GetGLAccountListInput,
        CreateUpdateGLAccountDto,
        CreateUpdateGLAccountDto
    >,
        IGLAccountAppService
{
    private readonly GLAccountManager _glAccountManager;

    public GLAccountAppService(
        IRepository<GLAccount, Guid> repository,
        GLAccountManager glAccountManager
    )
        : base(repository)
    {
        _glAccountManager = glAccountManager;
        GetPolicyName = ErpPermissions.GLAccounts.Default;
        GetListPolicyName = ErpPermissions.GLAccounts.Default;
        CreatePolicyName = ErpPermissions.GLAccounts.Create;
        UpdatePolicyName = ErpPermissions.GLAccounts.Update;
        DeletePolicyName = ErpPermissions.GLAccounts.Delete;
    }

    public override async Task<GLAccountDto> CreateAsync(CreateUpdateGLAccountDto input)
    {
        var account = await _glAccountManager.CreateAsync(
            input.No,
            input.Name,
            input.AccountType,
            input.AccountCategory,
            input.IncomeBalance,
            input.Subcategory,
            input.DirectPosting
        );

        await Repository.InsertAsync(account, autoSave: true);
        return await MapToGetOutputDtoAsync(account);
    }

    public override async Task<GLAccountDto> UpdateAsync(Guid id, CreateUpdateGLAccountDto input)
    {
        var account = await GetEntityByIdAsync(id);

        if (!string.Equals(account.No, input.No, StringComparison.OrdinalIgnoreCase))
        {
            await _glAccountManager.EnsureNoIsUniqueAsync(input.No, id);
            account.SetNo(input.No);
        }

        account.SetName(input.Name);
        account.SetAccountType(input.AccountType);
        account.SetAccountCategory(input.AccountCategory);
        account.SetIncomeBalance(input.IncomeBalance);
        account.SetSubcategory(input.Subcategory);
        account.SetDirectPosting(input.DirectPosting);

        await Repository.UpdateAsync(account, autoSave: true);
        return await MapToGetOutputDtoAsync(account);
    }

    public async Task<GLAccountDto> GetByNoAsync(string no)
    {
        var account = await Repository.FirstOrDefaultAsync(x => x.No == no);
        return await MapToGetOutputDtoAsync(account);
    }

    [Authorize(ErpPermissions.GLAccounts.Update)]
    public async Task BlockAsync(Guid id)
    {
        var account = await GetEntityByIdAsync(id);
        account.Block();
        await Repository.UpdateAsync(account, autoSave: true);
    }

    [Authorize(ErpPermissions.GLAccounts.Update)]
    public async Task UnblockAsync(Guid id)
    {
        var account = await GetEntityByIdAsync(id);
        account.Unblock();
        await Repository.UpdateAsync(account, autoSave: true);
    }

    protected override async Task<IQueryable<GLAccount>> CreateFilteredQueryAsync(
        GetGLAccountListInput input
    )
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(
                !input.Filter.IsNullOrWhiteSpace(),
                x => x.No.Contains(input.Filter) || x.Name.Contains(input.Filter)
            )
            .WhereIf(
                input.AccountCategory.HasValue,
                x => x.AccountCategory == input.AccountCategory.Value
            )
            .WhereIf(input.AccountType.HasValue, x => x.AccountType == input.AccountType.Value);
    }
}
