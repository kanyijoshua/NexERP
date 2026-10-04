using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Purchasing;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.FixedAssets;

/// <summary>FA Classes.</summary>
public class FAClassAppService : CodeTableAppServiceBase<FAClass, CodeTableDto, CreateUpdateCodeTableDto>, IFAClassAppService
{
    public FAClassAppService(IRepository<FAClass, Guid> repository)
        : base(repository, ErpPermissions.FixedAssetSetup.Default) { }

    protected override FAClass NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>FA Subclasses.</summary>
public class FASubclassAppService : CodeTableAppServiceBase<FASubclass, FASubclassDto, CreateUpdateFASubclassDto>, IFASubclassAppService
{
    private TableRelationChecker Relations => LazyServiceProvider.LazyGetRequiredService<TableRelationChecker>();

    public FASubclassAppService(IRepository<FASubclass, Guid> repository)
        : base(repository, ErpPermissions.FixedAssetSetup.Default) { }

    protected override FASubclass NewEntity(Guid id, CreateUpdateFASubclassDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(FASubclass entity, CreateUpdateFASubclassDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<FAClass>(input.FAClassCode);
        await CodeTableChecker.EnsureExistsAsync<FAPostingGroup>(input.DefaultFAPostingGroup);

        entity.Set(input.FAClassCode, input.DefaultFAPostingGroup);
    }
}

/// <summary>FA Locations.</summary>
public class FALocationAppService : CodeTableAppServiceBase<FALocation, CodeTableDto, CreateUpdateCodeTableDto>, IFALocationAppService
{
    public FALocationAppService(IRepository<FALocation, Guid> repository)
        : base(repository, ErpPermissions.FixedAssetSetup.Default) { }

    protected override FALocation NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Maintenance Codes.</summary>
public class MaintenanceAppService : CodeTableAppServiceBase<Maintenance, CodeTableDto, CreateUpdateCodeTableDto>, IMaintenanceAppService
{
    public MaintenanceAppService(IRepository<Maintenance, Guid> repository)
        : base(repository, ErpPermissions.FixedAssetSetup.Default) { }

    protected override Maintenance NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Depreciation Books.</summary>
public class DepreciationBookAppService : CodeTableAppServiceBase<DepreciationBook, DepreciationBookDto, CreateUpdateDepreciationBookDto>, IDepreciationBookAppService
{
    public DepreciationBookAppService(IRepository<DepreciationBook, Guid> repository)
        : base(repository, ErpPermissions.FixedAssetSetup.Default) { }

    protected override DepreciationBook NewEntity(Guid id, CreateUpdateDepreciationBookDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(DepreciationBook entity, CreateUpdateDepreciationBookDto input)
    {
        entity.Set(
            input.GLIntegrationAcqCost,
            input.GLIntegrationDepreciation,
            input.GLIntegrationWriteDown,
            input.GLIntegrationAppreciation,
            input.GLIntegrationDisposal,
            input.GLIntegrationMaintenance,
            input.DisposalCalculationMethod,
            input.AllowDeprBelowZero,
            input.AllowIndexation,
            input.UseSameFAAndGLPostingDates,
            input.UseRoundingInPeriodicDepr,
            input.AllowChangesInDeprFields,
            input.DefaultFinalRoundingAmount,
            input.DefaultEndingBookValue,
            input.MarkErrorsAsCorrections,
            input.AllowAcqCostBelowZero,
            input.AllowIdenticalDocumentNo,
            input.FiscalYear365Days
        );
        return Task.CompletedTask;
    }
}

/// <summary>FA Posting Groups.</summary>
public class FAPostingGroupAppService : CodeTableAppServiceBase<FAPostingGroup, FAPostingGroupDto, CreateUpdateFAPostingGroupDto>, IFAPostingGroupAppService
{
    private TableRelationChecker Relations => LazyServiceProvider.LazyGetRequiredService<TableRelationChecker>();

    public FAPostingGroupAppService(IRepository<FAPostingGroup, Guid> repository)
        : base(repository, ErpPermissions.FixedAssetSetup.Default) { }

    protected override FAPostingGroup NewEntity(Guid id, CreateUpdateFAPostingGroupDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(FAPostingGroup entity, CreateUpdateFAPostingGroupDto input)
    {
        await Relations.EnsureGLAccountsExistAsync(
            input.AcquisitionCostAccount,
            input.AccumDepreciationAccount,
            input.WriteDownAccount,
            input.AppreciationAccount,
            input.AcqCostAccOnDisposal,
            input.AccumDeprAccOnDisposal,
            input.WriteDownAccOnDisposal,
            input.AppreciationAccOnDisposal,
            input.GainsAccOnDisposal,
            input.LossesAccOnDisposal,
            input.BookValAccOnDispGain,
            input.SalesAccOnDispGain,
            input.WriteDownBalAccOnDisp,
            input.ApprecBalAccOnDisp,
            input.MaintenanceExpenseAccount,
            input.MaintenanceBalAcc,
            input.AcquisitionCostBalAcc,
            input.DepreciationExpenseAcc,
            input.WriteDownExpenseAcc,
            input.AppreciationBalAccount,
            input.SalesBalAcc,
            input.SalesAccOnDispLoss,
            input.BookValAccOnDispLoss
        );

        entity.Set(
            input.AcquisitionCostAccount,
            input.AccumDepreciationAccount,
            input.WriteDownAccount,
            input.AppreciationAccount,
            input.AcqCostAccOnDisposal,
            input.AccumDeprAccOnDisposal,
            input.WriteDownAccOnDisposal,
            input.AppreciationAccOnDisposal,
            input.GainsAccOnDisposal,
            input.LossesAccOnDisposal,
            input.BookValAccOnDispGain,
            input.SalesAccOnDispGain,
            input.WriteDownBalAccOnDisp,
            input.ApprecBalAccOnDisp,
            input.MaintenanceExpenseAccount,
            input.MaintenanceBalAcc,
            input.AcquisitionCostBalAcc,
            input.DepreciationExpenseAcc,
            input.WriteDownExpenseAcc,
            input.AppreciationBalAccount,
            input.SalesBalAcc,
            input.SalesAccOnDispLoss,
            input.BookValAccOnDispLoss
        );
    }
}

/// <summary>FA Setup.</summary>
[Authorize(ErpPermissions.FixedAssetSetup.Default)]
public class FASetupAppService : ErpAppService, IFASetupAppService
{
    private readonly FASetupManager _setupManager;
    private readonly IRepository<FASetup, Guid> _repository;

    private CodeTableChecker CodeTableChecker => LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
    private TableRelationChecker Relations => LazyServiceProvider.LazyGetRequiredService<TableRelationChecker>();

    public FASetupAppService(FASetupManager setupManager, IRepository<FASetup, Guid> repository)
    {
        _setupManager = setupManager;
        _repository = repository;
    }

    public async Task<FASetupDto> GetAsync()
    {
        return ObjectMapper.Map<FASetup, FASetupDto>(await _setupManager.GetAsync());
    }

    [Authorize(ErpPermissions.FixedAssetSetup.Update)]
    public async Task<FASetupDto> UpdateAsync(FASetupDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<DepreciationBook>(input.DefaultDeprBook);
        await LazyServiceProvider.LazyGetRequiredService<NoSeriesCodeValidator>().EnsureExistAsync(input.FixedAssetNos);

        var setup = await _setupManager.GetAsync();
        setup.Set(
            input.AllowPostingToMainAssets,
            input.DefaultDeprBook,
            input.AllowFAPostingFrom,
            input.AllowFAPostingTo,
            input.FixedAssetNos
        );

        await _repository.UpdateAsync(setup, autoSave: true);
        return ObjectMapper.Map<FASetup, FASetupDto>(setup);
    }
}

/// <summary>Fixed Assets.</summary>
public class FixedAssetAppService : ErpTableAppService<FixedAsset, FixedAssetDto, GetFixedAssetListInput, CreateUpdateFixedAssetDto>, IFixedAssetAppService
{
    public FixedAssetAppService(IRepository<FixedAsset, Guid> repository)
        : base(repository, ErpPermissions.FixedAssets.Default) { }

    public override async Task<FixedAssetDto> CreateAsync(CreateUpdateFixedAssetDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        // Blank takes the next number of the series.
        var setup = await LazyServiceProvider.LazyGetRequiredService<FASetupManager>().GetAsync();
        var no = await LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>().ResolveNoAsync(setup.FixedAssetNos, input.No, Clock.Now);
        await EnsureKeyIsUniqueAsync(no, null);

        var entity = new FixedAsset(GuidGenerator.Create(), no);
        entity.Set(
            input.Description,
            input.SearchDescription,
            input.Description2,
            input.FAClassCode,
            input.FASubclassCode,
            input.GlobalDimension1Code,
            input.GlobalDimension2Code,
            input.LocationCode,
            input.FALocationCode,
            input.VendorNo,
            input.MainAssetComponent,
            input.ComponentOfMainAsset,
            input.BudgetedAsset,
            input.WarrantyDate,
            input.ResponsibleEmployee,
            input.SerialNo,
            input.Blocked,
            input.MaintenanceVendorNo,
            input.UnderMaintenance,
            input.NextServiceDate,
            input.Inactive,
            input.FAPostingGroup
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<FixedAssetDto> UpdateAsync(Guid id, CreateUpdateFixedAssetDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var no = input.No.IsNullOrWhiteSpace() ? entity.No : input.No.Trim();
        await EnsureKeyIsUniqueAsync(no, id);

        entity.SetKey(no);
        entity.Set(
            input.Description,
            input.SearchDescription,
            input.Description2,
            input.FAClassCode,
            input.FASubclassCode,
            input.GlobalDimension1Code,
            input.GlobalDimension2Code,
            input.LocationCode,
            input.FALocationCode,
            input.VendorNo,
            input.MainAssetComponent,
            input.ComponentOfMainAsset,
            input.BudgetedAsset,
            input.WarrantyDate,
            input.ResponsibleEmployee,
            input.SerialNo,
            input.Blocked,
            input.MaintenanceVendorNo,
            input.UnderMaintenance,
            input.NextServiceDate,
            input.Inactive,
            input.FAPostingGroup
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<FixedAsset>> CreateFilteredQueryAsync(GetFixedAssetListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.Description.ToLower().Contains(filter)
                    || (x.SearchDescription != null && x.SearchDescription.ToLower().Contains(filter))
                    || (x.Description2 != null && x.Description2.ToLower().Contains(filter))
                    || (x.FAClassCode != null && x.FAClassCode.ToLower().Contains(filter))
                    || (x.FASubclassCode != null && x.FASubclassCode.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<FixedAsset> ApplyDefaultSorting(IQueryable<FixedAsset> query) =>
        query.OrderBy(x => x.No);

    private async Task ValidateAsync(CreateUpdateFixedAssetDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<FAClass>(input.FAClassCode);
        await CodeTableChecker.EnsureExistsAsync<FASubclass>(input.FASubclassCode);
        await CodeTableChecker.EnsureExistsAsync<Location>(input.LocationCode);
        await CodeTableChecker.EnsureExistsAsync<FALocation>(input.FALocationCode);
        await Relations.EnsureNoExistsAsync<Vendor>(input.VendorNo);
        await Relations.EnsureNoExistsAsync<FixedAsset>(input.ComponentOfMainAsset);
        await Relations.EnsureNoExistsAsync<Employee>(input.ResponsibleEmployee);
        await Relations.EnsureNoExistsAsync<Vendor>(input.MaintenanceVendorNo);
        await CodeTableChecker.EnsureExistsAsync<FAPostingGroup>(input.FAPostingGroup);
    }

    private async Task EnsureKeyIsUniqueAsync(string no, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.No == no && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Fixed Asset").WithData("key", no);
        }
    }
}

/// <summary>FA Depreciation Books.</summary>
public class FADepreciationBookAppService : ErpTableAppService<FADepreciationBook, FADepreciationBookDto, GetFADepreciationBookListInput, CreateUpdateFADepreciationBookDto>, IFADepreciationBookAppService
{
    public FADepreciationBookAppService(IRepository<FADepreciationBook, Guid> repository)
        : base(repository, ErpPermissions.FixedAssets.Default) { }

    public override async Task<FADepreciationBookDto> CreateAsync(CreateUpdateFADepreciationBookDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.FANo), CodeTableEntity.NormalizeCode(input.DepreciationBookCode), null);

        var entity = new FADepreciationBook(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.FANo), CodeTableEntity.NormalizeCode(input.DepreciationBookCode));
        entity.Set(
            input.DepreciationMethod,
            input.DepreciationStartingDate,
            input.StraightLinePct,
            input.NoOfDepreciationYears,
            input.NoOfDepreciationMonths,
            input.FixedDeprAmount,
            input.DecliningBalancePct,
            input.FinalRoundingAmount,
            input.EndingBookValue,
            input.FAPostingGroup,
            input.DepreciationEndingDate,
            input.ProjectedDisposalDate,
            input.ProjectedProceedsOnDisposal,
            input.UseHalfYearConvention,
            input.DefaultFADepreciationBook
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<FADepreciationBookDto> UpdateAsync(Guid id, CreateUpdateFADepreciationBookDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.FANo), CodeTableEntity.NormalizeCode(input.DepreciationBookCode), id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.FANo), CodeTableEntity.NormalizeCode(input.DepreciationBookCode));
        entity.Set(
            input.DepreciationMethod,
            input.DepreciationStartingDate,
            input.StraightLinePct,
            input.NoOfDepreciationYears,
            input.NoOfDepreciationMonths,
            input.FixedDeprAmount,
            input.DecliningBalancePct,
            input.FinalRoundingAmount,
            input.EndingBookValue,
            input.FAPostingGroup,
            input.DepreciationEndingDate,
            input.ProjectedDisposalDate,
            input.ProjectedProceedsOnDisposal,
            input.UseHalfYearConvention,
            input.DefaultFADepreciationBook
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<FADepreciationBook>> CreateFilteredQueryAsync(GetFADepreciationBookListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var faNo = input.FANo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!faNo.IsNullOrEmpty(), x => x.FANo == faNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.FANo.ToLower().Contains(filter)
                    || x.DepreciationBookCode.ToLower().Contains(filter)
                    || (x.FAPostingGroup != null && x.FAPostingGroup.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<FADepreciationBook> ApplyDefaultSorting(IQueryable<FADepreciationBook> query) =>
        query.OrderBy(x => x.FANo).ThenBy(x => x.DepreciationBookCode);

    private async Task ValidateAsync(CreateUpdateFADepreciationBookDto input)
    {
        await Relations.EnsureNoExistsAsync<FixedAsset>(input.FANo);
        await CodeTableChecker.EnsureExistsAsync<DepreciationBook>(input.DepreciationBookCode);
        await CodeTableChecker.EnsureExistsAsync<FAPostingGroup>(input.FAPostingGroup);
    }

    private async Task EnsureKeyIsUniqueAsync(string faNo, string depreciationBookCode, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.FANo == faNo && x.DepreciationBookCode == depreciationBookCode && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "FA Depreciation Book").WithData("key", faNo + " " + depreciationBookCode);
        }
    }
}

/// <summary>FA Ledger Entries.</summary>
public class FALedgerEntryAppService : ErpReadOnlyAppService<FALedgerEntry, FALedgerEntryDto, Guid, GetFALedgerEntryListInput>, IFALedgerEntryAppService
{
    public FALedgerEntryAppService(IRepository<FALedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.FixedAssets.Default;
        GetListPolicyName = ErpPermissions.FixedAssets.Default;
    }

    protected override async Task<IQueryable<FALedgerEntry>> CreateFilteredQueryAsync(GetFALedgerEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var faNo = input.FANo?.Trim().ToUpperInvariant();
        var documentNo = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!faNo.IsNullOrEmpty(), x => x.FANo == faNo)
            .WhereIf(!documentNo.IsNullOrEmpty(), x => x.DocumentNo == documentNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => (x.FANo != null && x.FANo.ToLower().Contains(filter))
                    || (x.DocumentNo != null && x.DocumentNo.ToLower().Contains(filter))
                    || (x.ExternalDocumentNo != null && x.ExternalDocumentNo.ToLower().Contains(filter))
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
                    || (x.DepreciationBookCode != null && x.DepreciationBookCode.ToLower().Contains(filter))
                    || (x.FASubclassCode != null && x.FASubclassCode.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<FALedgerEntry> ApplyDefaultSorting(IQueryable<FALedgerEntry> query) =>
        query.OrderByDescending(x => x.EntryNo);
}

/// <summary>Maintenance Registrations.</summary>
public class MaintenanceRegistrationAppService : ErpTableAppService<MaintenanceRegistration, MaintenanceRegistrationDto, GetMaintenanceRegistrationListInput, CreateUpdateMaintenanceRegistrationDto>, IMaintenanceRegistrationAppService
{
    public MaintenanceRegistrationAppService(IRepository<MaintenanceRegistration, Guid> repository)
        : base(repository, ErpPermissions.FixedAssets.Default) { }

    public override async Task<MaintenanceRegistrationDto> CreateAsync(CreateUpdateMaintenanceRegistrationDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : await NextLineNoAsync(input);
        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.FANo), lineNo, null);

        var entity = new MaintenanceRegistration(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.FANo), lineNo);
        entity.Set(
            input.ServiceDate,
            input.MaintenanceVendorNo,
            input.Comment,
            input.ServiceAgentName,
            input.ServiceAgentPhoneNo,
            input.ServiceAgentMobilePhone
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<MaintenanceRegistrationDto> UpdateAsync(Guid id, CreateUpdateMaintenanceRegistrationDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : entity.LineNo;
        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.FANo), lineNo, id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.FANo), lineNo);
        entity.Set(
            input.ServiceDate,
            input.MaintenanceVendorNo,
            input.Comment,
            input.ServiceAgentName,
            input.ServiceAgentPhoneNo,
            input.ServiceAgentMobilePhone
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<MaintenanceRegistration>> CreateFilteredQueryAsync(GetMaintenanceRegistrationListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var faNo = input.FANo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!faNo.IsNullOrEmpty(), x => x.FANo == faNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.FANo.ToLower().Contains(filter)
                    || (x.MaintenanceVendorNo != null && x.MaintenanceVendorNo.ToLower().Contains(filter))
                    || (x.Comment != null && x.Comment.ToLower().Contains(filter))
                    || (x.ServiceAgentName != null && x.ServiceAgentName.ToLower().Contains(filter))
                    || (x.ServiceAgentPhoneNo != null && x.ServiceAgentPhoneNo.ToLower().Contains(filter))
                    || (x.ServiceAgentMobilePhone != null && x.ServiceAgentMobilePhone.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<MaintenanceRegistration> ApplyDefaultSorting(IQueryable<MaintenanceRegistration> query) =>
        query.OrderBy(x => x.FANo).ThenBy(x => x.LineNo);

    private async Task ValidateAsync(CreateUpdateMaintenanceRegistrationDto input)
    {
        await Relations.EnsureNoExistsAsync<FixedAsset>(input.FANo);
        await Relations.EnsureNoExistsAsync<Vendor>(input.MaintenanceVendorNo);
    }

    private async Task EnsureKeyIsUniqueAsync(string faNo, int lineNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.FANo == faNo && x.LineNo == lineNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Maintenance Registration").WithData("key", faNo + " " + lineNo.ToString());
        }
    }

    /// <summary>The next Line No. of the parent record: 10000 above the last.</summary>
    private async Task<int> NextLineNoAsync(CreateUpdateMaintenanceRegistrationDto input)
    {
        var faNo = CodeTableEntity.NormalizeCode(input.FANo);
        var query = await Repository.GetQueryableAsync();
        var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.FANo == faNo).OrderByDescending(x => x.LineNo));

        return (last?.LineNo ?? 0) + 10000;
    }
}

/// <summary>Main Asset Components.</summary>
public class MainAssetComponentAppService : ErpTableAppService<MainAssetComponent, MainAssetComponentDto, GetMainAssetComponentListInput, CreateUpdateMainAssetComponentDto>, IMainAssetComponentAppService
{
    public MainAssetComponentAppService(IRepository<MainAssetComponent, Guid> repository)
        : base(repository, ErpPermissions.FixedAssets.Default) { }

    public override async Task<MainAssetComponentDto> CreateAsync(CreateUpdateMainAssetComponentDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.MainAssetNo), CodeTableEntity.NormalizeCode(input.FANo), null);

        var entity = new MainAssetComponent(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.MainAssetNo), CodeTableEntity.NormalizeCode(input.FANo));
        entity.Set(input.Description);

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<MainAssetComponentDto> UpdateAsync(Guid id, CreateUpdateMainAssetComponentDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.MainAssetNo), CodeTableEntity.NormalizeCode(input.FANo), id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.MainAssetNo), CodeTableEntity.NormalizeCode(input.FANo));
        entity.Set(input.Description);

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<MainAssetComponent>> CreateFilteredQueryAsync(GetMainAssetComponentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var mainAssetNo = input.MainAssetNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!mainAssetNo.IsNullOrEmpty(), x => x.MainAssetNo == mainAssetNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.MainAssetNo.ToLower().Contains(filter)
                    || x.FANo.ToLower().Contains(filter)
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<MainAssetComponent> ApplyDefaultSorting(IQueryable<MainAssetComponent> query) =>
        query.OrderBy(x => x.MainAssetNo).ThenBy(x => x.FANo);

    private async Task ValidateAsync(CreateUpdateMainAssetComponentDto input)
    {
        await Relations.EnsureNoExistsAsync<FixedAsset>(input.MainAssetNo);
        await Relations.EnsureNoExistsAsync<FixedAsset>(input.FANo);
    }

    private async Task EnsureKeyIsUniqueAsync(string mainAssetNo, string faNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.MainAssetNo == mainAssetNo && x.FANo == faNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Main Asset Component").WithData("key", mainAssetNo + " " + faNo);
        }
    }
}
