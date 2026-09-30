using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// General Posting Setup (BC page 252): one row per business and product posting group pair,
/// naming the accounts sales and purchase lines of that pair post to.
/// </summary>
[Authorize(ErpPermissions.PostingSetup.Default)]
public class GeneralPostingSetupAppService
    : ErpCrudAppService<
        GeneralPostingSetup,
        GeneralPostingSetupDto,
        Guid,
        GetCodeTableListInput,
        CreateUpdateGeneralPostingSetupDto,
        CreateUpdateGeneralPostingSetupDto
    >,
        IGeneralPostingSetupAppService
{
    private readonly PostingSetupManager _postingSetupManager;

    public GeneralPostingSetupAppService(IRepository<GeneralPostingSetup, Guid> repository, PostingSetupManager postingSetupManager)
        : base(repository)
    {
        _postingSetupManager = postingSetupManager;
        GetPolicyName = ErpPermissions.PostingSetup.Default;
        GetListPolicyName = ErpPermissions.PostingSetup.Default;
        CreatePolicyName = ErpPermissions.PostingSetup.Create;
        UpdatePolicyName = ErpPermissions.PostingSetup.Update;
        DeletePolicyName = ErpPermissions.PostingSetup.Delete;
    }

    public override async Task<GeneralPostingSetupDto> CreateAsync(CreateUpdateGeneralPostingSetupDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input, null);

        var setup = new GeneralPostingSetup(GuidGenerator.Create(), input.GenBusPostingGroup, input.GenProdPostingGroup);
        Apply(setup, input);

        await Repository.InsertAsync(setup, autoSave: true);
        return await MapToGetOutputDtoAsync(setup);
    }

    public override async Task<GeneralPostingSetupDto> UpdateAsync(Guid id, CreateUpdateGeneralPostingSetupDto input)
    {
        await CheckUpdatePolicyAsync();

        var setup = await GetEntityByIdAsync(id);
        await ValidateAsync(input, id);

        setup.SetKey(input.GenBusPostingGroup, input.GenProdPostingGroup);
        Apply(setup, input);

        await Repository.UpdateAsync(setup, autoSave: true);
        return await MapToGetOutputDtoAsync(setup);
    }

    protected override async Task<IQueryable<GeneralPostingSetup>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            x => (x.GenBusPostingGroup != null && x.GenBusPostingGroup.ToLower().Contains(filter)) || x.GenProdPostingGroup.ToLower().Contains(filter)
        );
    }

    protected override IQueryable<GeneralPostingSetup> ApplyDefaultSorting(IQueryable<GeneralPostingSetup> query)
    {
        return query.OrderBy(x => x.GenBusPostingGroup).ThenBy(x => x.GenProdPostingGroup);
    }

    private async Task ValidateAsync(CreateUpdateGeneralPostingSetupDto input, Guid? exceptId)
    {
        await _postingSetupManager.EnsureGroupExistsAsync<GenBusinessPostingGroup>(input.GenBusPostingGroup);
        await _postingSetupManager.EnsureGroupExistsAsync<GenProductPostingGroup>(input.GenProdPostingGroup);
        await _postingSetupManager.EnsureGLAccountsExistAsync(
            input.SalesAccountNo,
            input.SalesCreditMemoAccountNo,
            input.SalesDiscountAccountNo,
            input.PurchAccountNo,
            input.PurchCreditMemoAccountNo,
            input.PurchDiscountAccountNo,
            input.COGSAccountNo,
            input.InventoryAdjmtAccountNo
        );

        var bus = PostingGroupBase.NormalizeCode(input.GenBusPostingGroup);
        var prod = PostingGroupBase.NormalizeCode(input.GenProdPostingGroup);
        if (await Repository.AnyAsync(x => x.GenBusPostingGroup == bus && x.GenProdPostingGroup == prod && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.SetupAlreadyExists)
                .WithData("key", GeneralPostingSetup.DescribeKey(bus, prod));
        }
    }

    private static void Apply(GeneralPostingSetup setup, CreateUpdateGeneralPostingSetupDto input)
    {
        setup.SetSalesAccounts(input.SalesAccountNo, input.SalesCreditMemoAccountNo, input.SalesDiscountAccountNo);
        setup.SetPurchaseAccounts(input.PurchAccountNo, input.PurchCreditMemoAccountNo, input.PurchDiscountAccountNo);
        setup.SetInventoryAccounts(input.COGSAccountNo, input.InventoryAdjmtAccountNo);
    }
}

/// <summary>Inventory Posting Setup (BC page 5826): the inventory account per inventory posting group.</summary>
[Authorize(ErpPermissions.PostingSetup.Default)]
public class InventoryPostingSetupAppService
    : ErpCrudAppService<
        InventoryPostingSetup,
        InventoryPostingSetupDto,
        Guid,
        GetCodeTableListInput,
        CreateUpdateInventoryPostingSetupDto,
        CreateUpdateInventoryPostingSetupDto
    >,
        IInventoryPostingSetupAppService
{
    private readonly PostingSetupManager _postingSetupManager;

    public InventoryPostingSetupAppService(IRepository<InventoryPostingSetup, Guid> repository, PostingSetupManager postingSetupManager)
        : base(repository)
    {
        _postingSetupManager = postingSetupManager;
        GetPolicyName = ErpPermissions.PostingSetup.Default;
        GetListPolicyName = ErpPermissions.PostingSetup.Default;
        CreatePolicyName = ErpPermissions.PostingSetup.Create;
        UpdatePolicyName = ErpPermissions.PostingSetup.Update;
        DeletePolicyName = ErpPermissions.PostingSetup.Delete;
    }

    public override async Task<InventoryPostingSetupDto> CreateAsync(CreateUpdateInventoryPostingSetupDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input, null);

        var setup = new InventoryPostingSetup(GuidGenerator.Create(), input.InventoryPostingGroup, input.InventoryAccountNo, input.LocationCode);

        await Repository.InsertAsync(setup, autoSave: true);
        return await MapToGetOutputDtoAsync(setup);
    }

    public override async Task<InventoryPostingSetupDto> UpdateAsync(Guid id, CreateUpdateInventoryPostingSetupDto input)
    {
        await CheckUpdatePolicyAsync();

        var setup = await GetEntityByIdAsync(id);
        await ValidateAsync(input, id);

        setup.SetLocation(input.LocationCode);
        setup.SetInventoryPostingGroup(input.InventoryPostingGroup);
        setup.SetInventoryAccount(input.InventoryAccountNo);

        await Repository.UpdateAsync(setup, autoSave: true);
        return await MapToGetOutputDtoAsync(setup);
    }

    protected override async Task<IQueryable<InventoryPostingSetup>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            x => x.InventoryPostingGroup.ToLower().Contains(filter) || (x.LocationCode != null && x.LocationCode.ToLower().Contains(filter))
        );
    }

    protected override IQueryable<InventoryPostingSetup> ApplyDefaultSorting(IQueryable<InventoryPostingSetup> query)
    {
        return query.OrderBy(x => x.LocationCode).ThenBy(x => x.InventoryPostingGroup);
    }

    private async Task ValidateAsync(CreateUpdateInventoryPostingSetupDto input, Guid? exceptId)
    {
        await _postingSetupManager.EnsureGroupExistsAsync<InventoryPostingGroup>(input.InventoryPostingGroup);
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<Location>(input.LocationCode);
        await _postingSetupManager.EnsureGLAccountsExistAsync(input.InventoryAccountNo);

        var location = CodeTableEntity.NormalizeCode(input.LocationCode);
        var group = PostingGroupBase.NormalizeCode(input.InventoryPostingGroup);
        if (await Repository.AnyAsync(x => x.LocationCode == location && x.InventoryPostingGroup == group && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.SetupAlreadyExists).WithData("key", $"{location} {group}".Trim());
        }
    }
}

/// <summary>General Ledger Setup (BC page 118): one row per company, created blank on first read.</summary>
[Authorize(ErpPermissions.GeneralLedgerSetup.Default)]
public class GeneralLedgerSetupAppService : ErpAppService, IGeneralLedgerSetupAppService
{
    private readonly GeneralLedgerSetupManager _setupManager;
    private readonly IRepository<GeneralLedgerSetup, Guid> _repository;
    private readonly IRepository<Dimension, Guid> _dimensionRepository;

    public GeneralLedgerSetupAppService(
        GeneralLedgerSetupManager setupManager,
        IRepository<GeneralLedgerSetup, Guid> repository,
        IRepository<Dimension, Guid> dimensionRepository
    )
    {
        _setupManager = setupManager;
        _repository = repository;
        _dimensionRepository = dimensionRepository;
    }

    public async Task<GeneralLedgerSetupDto> GetAsync()
    {
        return ObjectMapper.Map<GeneralLedgerSetup, GeneralLedgerSetupDto>(await _setupManager.GetAsync());
    }

    [Authorize(ErpPermissions.GeneralLedgerSetup.Update)]
    public async Task<GeneralLedgerSetupDto> UpdateAsync(GeneralLedgerSetupDto input)
    {
        await EnsureDimensionExistsAsync(input.GlobalDimension1Code);
        await EnsureDimensionExistsAsync(input.GlobalDimension2Code);
        await LazyServiceProvider.LazyGetRequiredService<Numbering.NoSeriesCodeValidator>().EnsureExistAsync(input.BankAccountNos);

        var setup = await _setupManager.GetAsync();
        setup.SetAllowedPostingDates(input.AllowPostingFrom, input.AllowPostingTo);
        setup.SetLocalCurrency(input.LcyCode);
        setup.SetRoundingPrecisions(input.AmountRoundingPrecision, input.UnitAmountRoundingPrecision, input.InvRoundingPrecisionLcy);
        setup.SetGlobalDimensions(input.GlobalDimension1Code, input.GlobalDimension2Code);
        setup.SetNumbering(input.BankAccountNos);

        await _repository.UpdateAsync(setup, autoSave: true);
        return ObjectMapper.Map<GeneralLedgerSetup, GeneralLedgerSetupDto>(setup);
    }

    private async Task EnsureDimensionExistsAsync(string code)
    {
        if (code.IsNullOrWhiteSpace())
        {
            return;
        }

        var normalized = code.Trim().ToUpperInvariant();
        if (!await _dimensionRepository.AnyAsync(d => d.Code.ToUpper() == normalized))
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.DimensionNotFound).WithData("code", normalized);
        }
    }
}
