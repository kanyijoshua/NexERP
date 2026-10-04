using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// VAT Posting Setup: per business and product VAT group, the rate, how it is
/// calculated and the accounts the VAT posts to.
/// </summary>
[Authorize(ErpPermissions.PostingSetup.Default)]
public class VatPostingSetupAppService
    : ErpCrudAppService<
        VatPostingSetup,
        VatPostingSetupDto,
        Guid,
        GetCodeTableListInput,
        CreateUpdateVatPostingSetupDto,
        CreateUpdateVatPostingSetupDto
    >,
        IVatPostingSetupAppService
{
    private readonly CodeTableChecker _codeTableChecker;
    private readonly PostingSetupManager _postingSetupManager;

    public VatPostingSetupAppService(
        IRepository<VatPostingSetup, Guid> repository,
        CodeTableChecker codeTableChecker,
        PostingSetupManager postingSetupManager
    )
        : base(repository)
    {
        _codeTableChecker = codeTableChecker;
        _postingSetupManager = postingSetupManager;
        GetPolicyName = ErpPermissions.PostingSetup.Default;
        GetListPolicyName = ErpPermissions.PostingSetup.Default;
        CreatePolicyName = ErpPermissions.PostingSetup.Create;
        UpdatePolicyName = ErpPermissions.PostingSetup.Update;
        DeletePolicyName = ErpPermissions.PostingSetup.Delete;
    }

    public override async Task<VatPostingSetupDto> CreateAsync(CreateUpdateVatPostingSetupDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input, null);

        var setup = new VatPostingSetup(GuidGenerator.Create(), input.VatBusPostingGroup, input.VatProdPostingGroup);
        Apply(setup, input);

        await Repository.InsertAsync(setup, autoSave: true);
        return await MapToGetOutputDtoAsync(setup);
    }

    public override async Task<VatPostingSetupDto> UpdateAsync(Guid id, CreateUpdateVatPostingSetupDto input)
    {
        await CheckUpdatePolicyAsync();

        var setup = await GetEntityByIdAsync(id);
        await ValidateAsync(input, id);

        setup.SetKey(input.VatBusPostingGroup, input.VatProdPostingGroup);
        Apply(setup, input);

        await Repository.UpdateAsync(setup, autoSave: true);
        return await MapToGetOutputDtoAsync(setup);
    }

    protected override async Task<IQueryable<VatPostingSetup>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            x => (x.VatBusPostingGroup != null && x.VatBusPostingGroup.ToLower().Contains(filter))
                || (x.VatProdPostingGroup != null && x.VatProdPostingGroup.ToLower().Contains(filter))
                || (x.VatIdentifier != null && x.VatIdentifier.ToLower().Contains(filter))
        );
    }

    protected override IQueryable<VatPostingSetup> ApplyDefaultSorting(IQueryable<VatPostingSetup> query)
    {
        return query.OrderBy(x => x.VatBusPostingGroup).ThenBy(x => x.VatProdPostingGroup);
    }

    private async Task ValidateAsync(CreateUpdateVatPostingSetupDto input, Guid? exceptId)
    {
        await _codeTableChecker.EnsureExistsAsync<VatBusinessPostingGroup>(input.VatBusPostingGroup);
        await _codeTableChecker.EnsureExistsAsync<VatProductPostingGroup>(input.VatProdPostingGroup);
        await _postingSetupManager.EnsureGLAccountsExistAsync(input.SalesVatAccountNo, input.PurchaseVatAccountNo, input.ReverseChrgVatAccountNo);

        var bus = CodeTableEntity.NormalizeCode(input.VatBusPostingGroup);
        var prod = CodeTableEntity.NormalizeCode(input.VatProdPostingGroup);
        if (await Repository.AnyAsync(x => x.VatBusPostingGroup == bus && x.VatProdPostingGroup == prod && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.SetupAlreadyExists)
                .WithData("key", VatPostingSetup.DescribeKey(bus, prod));
        }
    }

    private static void Apply(VatPostingSetup setup, CreateUpdateVatPostingSetupDto input)
    {
        setup.SetRate(input.VatCalculationType, input.VatPercent, input.VatIdentifier, input.Description);
        setup.SetAccounts(input.SalesVatAccountNo, input.PurchaseVatAccountNo, input.ReverseChrgVatAccountNo);
        setup.SetBlocked(input.Blocked);
    }
}

/// <summary>VAT Entries: what the VAT return is made of.</summary>
[Authorize(ErpPermissions.VatEntries.Default)]
public class VatEntryAppService : ErpReadOnlyAppService<VatEntry, VatEntryDto, Guid, GetVatEntryListInput>, IVatEntryAppService
{
    public VatEntryAppService(IRepository<VatEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.VatEntries.Default;
        GetListPolicyName = ErpPermissions.VatEntries.Default;
    }

    protected override async Task<IQueryable<VatEntry>> CreateFilteredQueryAsync(GetVatEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter) || (x.BillToPayToNo != null && x.BillToPayToNo.ToLower().Contains(filter))
            )
            .WhereIf(input.Type.HasValue, x => x.Type == input.Type.Value)
            .WhereIf(input.FromDate.HasValue, x => x.PostingDate >= input.FromDate.Value)
            .WhereIf(input.ToDate.HasValue, x => x.PostingDate <= input.ToDate.Value);
    }

    protected override IQueryable<VatEntry> ApplyDefaultSorting(IQueryable<VatEntry> query)
    {
        return query.OrderByDescending(x => x.EntryNo);
    }
}
