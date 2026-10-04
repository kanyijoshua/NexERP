using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>Source Codes.</summary>
public class SourceCodeAppService : CodeTableAppServiceBase<SourceCode, CodeTableDto, CreateUpdateCodeTableDto>, ISourceCodeAppService
{
    public SourceCodeAppService(IRepository<SourceCode, Guid> repository)
        : base(repository, ErpPermissions.FinanceSetup.Default) { }

    protected override SourceCode NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Reason Codes.</summary>
public class ReasonCodeAppService : CodeTableAppServiceBase<ReasonCode, CodeTableDto, CreateUpdateCodeTableDto>, IReasonCodeAppService
{
    public ReasonCodeAppService(IRepository<ReasonCode, Guid> repository)
        : base(repository, ErpPermissions.FinanceSetup.Default) { }

    protected override ReasonCode NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Countries/Regions.</summary>
public class CountryRegionAppService : CodeTableAppServiceBase<CountryRegion, CountryRegionDto, CreateUpdateCountryRegionDto>, ICountryRegionAppService
{
    public CountryRegionAppService(IRepository<CountryRegion, Guid> repository)
        : base(repository, ErpPermissions.FinanceSetup.Default) { }

    protected override CountryRegion NewEntity(Guid id, CreateUpdateCountryRegionDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(CountryRegion entity, CreateUpdateCountryRegionDto input)
    {
        entity.Set(
            input.IsoCode,
            input.IsoNumericCode,
            input.EuCountryRegionCode,
            input.IntrastatCode,
            input.AddressFormat,
            input.ContactAddressFormat,
            input.VatScheme,
            input.CountyName
        );
        return Task.CompletedTask;
    }
}

/// <summary>Post Codes.</summary>
public class PostCodeAppService : ErpTableAppService<PostCode, PostCodeDto, GetPostCodeListInput, CreateUpdatePostCodeDto>, IPostCodeAppService
{
    public PostCodeAppService(IRepository<PostCode, Guid> repository)
        : base(repository, ErpPermissions.FinanceSetup.Default) { }

    public override async Task<PostCodeDto> CreateAsync(CreateUpdatePostCodeDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.Code), input.City?.Trim(), null);

        var entity = new PostCode(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.Code), input.City?.Trim());
        entity.Set(input.SearchCity, input.CountryRegionCode, input.County);

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<PostCodeDto> UpdateAsync(Guid id, CreateUpdatePostCodeDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.Code), input.City?.Trim(), id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.Code), input.City?.Trim());
        entity.Set(input.SearchCity, input.CountryRegionCode, input.County);

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<PostCode>> CreateFilteredQueryAsync(GetPostCodeListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.Code.ToLower().Contains(filter)
                    || x.City.ToLower().Contains(filter)
                    || (x.SearchCity != null && x.SearchCity.ToLower().Contains(filter))
                    || (x.CountryRegionCode != null && x.CountryRegionCode.ToLower().Contains(filter))
                    || (x.County != null && x.County.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<PostCode> ApplyDefaultSorting(IQueryable<PostCode> query) =>
        query.OrderBy(x => x.Code).ThenBy(x => x.City);

    private async Task ValidateAsync(CreateUpdatePostCodeDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<CountryRegion>(input.CountryRegionCode);
    }

    private async Task EnsureKeyIsUniqueAsync(string code, string city, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.Code == code && x.City == city && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Post Code").WithData("key", code + " " + city);
        }
    }
}

/// <summary>Shipment Methods.</summary>
public class ShipmentMethodAppService : CodeTableAppServiceBase<ShipmentMethod, CodeTableDto, CreateUpdateCodeTableDto>, IShipmentMethodAppService
{
    public ShipmentMethodAppService(IRepository<ShipmentMethod, Guid> repository)
        : base(repository, ErpPermissions.FinanceSetup.Default) { }

    protected override ShipmentMethod NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Responsibility Centers.</summary>
public class ResponsibilityCenterAppService : CodeTableAppServiceBase<ResponsibilityCenter, ResponsibilityCenterDto, CreateUpdateResponsibilityCenterDto>, IResponsibilityCenterAppService
{
    private TableRelationChecker Relations => LazyServiceProvider.LazyGetRequiredService<TableRelationChecker>();

    public ResponsibilityCenterAppService(IRepository<ResponsibilityCenter, Guid> repository)
        : base(repository, ErpPermissions.FinanceSetup.Default) { }

    protected override ResponsibilityCenter NewEntity(Guid id, CreateUpdateResponsibilityCenterDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(ResponsibilityCenter entity, CreateUpdateResponsibilityCenterDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<CountryRegion>(input.CountryRegionCode);
        await CodeTableChecker.EnsureExistsAsync<Location>(input.LocationCode);

        entity.Set(
            input.Address,
            input.Address2,
            input.City,
            input.PostCode,
            input.CountryRegionCode,
            input.PhoneNo,
            input.FaxNo,
            input.Name2,
            input.Contact,
            input.GlobalDimension1Code,
            input.GlobalDimension2Code,
            input.LocationCode,
            input.County,
            input.Email
        );
    }
}

/// <summary>User Setup.</summary>
public class UserSetupAppService : ErpTableAppService<UserSetup, UserSetupDto, GetUserSetupListInput, CreateUpdateUserSetupDto>, IUserSetupAppService
{
    public UserSetupAppService(IRepository<UserSetup, Guid> repository)
        : base(repository, ErpPermissions.UserSetup.Default) { }

    public override async Task<UserSetupDto> CreateAsync(CreateUpdateUserSetupDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.UserId), null);

        var entity = new UserSetup(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.UserId));
        entity.Set(
            input.AllowPostingFrom,
            input.AllowPostingTo,
            input.RegisterTime,
            input.AllowDeferralPostingFrom,
            input.AllowDeferralPostingTo,
            input.SalespersPurchCode,
            input.ApproverId,
            input.SalesAmountApprovalLimit,
            input.PurchaseAmountApprovalLimit,
            input.UnlimitedSalesApproval,
            input.UnlimitedPurchaseApproval,
            input.Substitute,
            input.Email,
            input.PhoneNo,
            input.RequestAmountApprovalLimit,
            input.UnlimitedRequestApproval,
            input.ApprovalAdministrator,
            input.AllowVatDateFrom,
            input.AllowVatDateTo,
            input.SalesInvoicePostingPolicy,
            input.PurchInvoicePostingPolicy,
            input.AllowFAPostingFrom,
            input.AllowFAPostingTo,
            input.SalesRespCtrFilter,
            input.PurchaseRespCtrFilter
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<UserSetupDto> UpdateAsync(Guid id, CreateUpdateUserSetupDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.UserId), id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.UserId));
        entity.Set(
            input.AllowPostingFrom,
            input.AllowPostingTo,
            input.RegisterTime,
            input.AllowDeferralPostingFrom,
            input.AllowDeferralPostingTo,
            input.SalespersPurchCode,
            input.ApproverId,
            input.SalesAmountApprovalLimit,
            input.PurchaseAmountApprovalLimit,
            input.UnlimitedSalesApproval,
            input.UnlimitedPurchaseApproval,
            input.Substitute,
            input.Email,
            input.PhoneNo,
            input.RequestAmountApprovalLimit,
            input.UnlimitedRequestApproval,
            input.ApprovalAdministrator,
            input.AllowVatDateFrom,
            input.AllowVatDateTo,
            input.SalesInvoicePostingPolicy,
            input.PurchInvoicePostingPolicy,
            input.AllowFAPostingFrom,
            input.AllowFAPostingTo,
            input.SalesRespCtrFilter,
            input.PurchaseRespCtrFilter
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<UserSetup>> CreateFilteredQueryAsync(GetUserSetupListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.UserId.ToLower().Contains(filter)
                    || (x.SalespersPurchCode != null && x.SalespersPurchCode.ToLower().Contains(filter))
                    || (x.ApproverId != null && x.ApproverId.ToLower().Contains(filter))
                    || (x.Substitute != null && x.Substitute.ToLower().Contains(filter))
                    || (x.Email != null && x.Email.ToLower().Contains(filter))
                    || (x.PhoneNo != null && x.PhoneNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<UserSetup> ApplyDefaultSorting(IQueryable<UserSetup> query) =>
        query.OrderBy(x => x.UserId);

    private async Task ValidateAsync(CreateUpdateUserSetupDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<SalespersonPurchaser>(input.SalespersPurchCode);
        await CodeTableChecker.EnsureExistsAsync<ResponsibilityCenter>(input.SalesRespCtrFilter);
        await CodeTableChecker.EnsureExistsAsync<ResponsibilityCenter>(input.PurchaseRespCtrFilter);
    }

    private async Task EnsureKeyIsUniqueAsync(string userId, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.UserId == userId && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "User Setup").WithData("key", userId);
        }
    }
}

/// <summary>G/L Budgets.</summary>
public class GLBudgetNameAppService : ErpTableAppService<GLBudgetName, GLBudgetNameDto, GetGLBudgetNameListInput, CreateUpdateGLBudgetNameDto>, IGLBudgetNameAppService
{
    public GLBudgetNameAppService(IRepository<GLBudgetName, Guid> repository)
        : base(repository, ErpPermissions.Budgets.Default) { }

    public override async Task<GLBudgetNameDto> CreateAsync(CreateUpdateGLBudgetNameDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.Name), null);

        var entity = new GLBudgetName(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.Name));
        entity.Set(
            input.Description,
            input.Blocked,
            input.BudgetDimension1Code,
            input.BudgetDimension2Code,
            input.BudgetDimension3Code,
            input.BudgetDimension4Code
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<GLBudgetNameDto> UpdateAsync(Guid id, CreateUpdateGLBudgetNameDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.Name), id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.Name));
        entity.Set(
            input.Description,
            input.Blocked,
            input.BudgetDimension1Code,
            input.BudgetDimension2Code,
            input.BudgetDimension3Code,
            input.BudgetDimension4Code
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<GLBudgetName>> CreateFilteredQueryAsync(GetGLBudgetNameListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.Name.ToLower().Contains(filter)
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
                    || (x.BudgetDimension1Code != null && x.BudgetDimension1Code.ToLower().Contains(filter))
                    || (x.BudgetDimension2Code != null && x.BudgetDimension2Code.ToLower().Contains(filter))
                    || (x.BudgetDimension3Code != null && x.BudgetDimension3Code.ToLower().Contains(filter))
                    || (x.BudgetDimension4Code != null && x.BudgetDimension4Code.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<GLBudgetName> ApplyDefaultSorting(IQueryable<GLBudgetName> query) =>
        query.OrderBy(x => x.Name);

    private static Task ValidateAsync(CreateUpdateGLBudgetNameDto input) => Task.CompletedTask;

    private async Task EnsureKeyIsUniqueAsync(string name, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.Name == name && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "G/L Budget Name").WithData("key", name);
        }
    }
}

/// <summary>G/L Budget Entries.</summary>
public class GLBudgetEntryAppService : ErpTableAppService<GLBudgetEntry, GLBudgetEntryDto, GetGLBudgetEntryListInput, CreateUpdateGLBudgetEntryDto>, IGLBudgetEntryAppService
{
    public GLBudgetEntryAppService(IRepository<GLBudgetEntry, Guid> repository)
        : base(repository, ErpPermissions.Budgets.Default) { }

    public override async Task<GLBudgetEntryDto> CreateAsync(CreateUpdateGLBudgetEntryDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var entryNo = input.EntryNo > 0 ? input.EntryNo : await NextEntryNoAsync(input);
        await EnsureKeyIsUniqueAsync(entryNo, null);

        var entity = new GLBudgetEntry(GuidGenerator.Create(), entryNo);
        entity.Set(
            input.BudgetName,
            input.GLAccountNo,
            input.Date,
            input.GlobalDimension1Code,
            input.GlobalDimension2Code,
            input.Amount,
            input.Description,
            input.BusinessUnitCode,
            input.BudgetDimension1Code,
            input.BudgetDimension2Code,
            input.BudgetDimension3Code,
            input.BudgetDimension4Code
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<GLBudgetEntryDto> UpdateAsync(Guid id, CreateUpdateGLBudgetEntryDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var entryNo = input.EntryNo > 0 ? input.EntryNo : entity.EntryNo;
        await EnsureKeyIsUniqueAsync(entryNo, id);

        entity.SetKey(entryNo);
        entity.Set(
            input.BudgetName,
            input.GLAccountNo,
            input.Date,
            input.GlobalDimension1Code,
            input.GlobalDimension2Code,
            input.Amount,
            input.Description,
            input.BusinessUnitCode,
            input.BudgetDimension1Code,
            input.BudgetDimension2Code,
            input.BudgetDimension3Code,
            input.BudgetDimension4Code
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<GLBudgetEntry>> CreateFilteredQueryAsync(GetGLBudgetEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var budgetName = input.BudgetName?.Trim().ToUpperInvariant();
        var glAccountNo = input.GLAccountNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!budgetName.IsNullOrEmpty(), x => x.BudgetName == budgetName)
            .WhereIf(!glAccountNo.IsNullOrEmpty(), x => x.GLAccountNo == glAccountNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.BudgetName.ToLower().Contains(filter)
                    || x.GLAccountNo.ToLower().Contains(filter)
                    || (x.GlobalDimension1Code != null && x.GlobalDimension1Code.ToLower().Contains(filter))
                    || (x.GlobalDimension2Code != null && x.GlobalDimension2Code.ToLower().Contains(filter))
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
                    || (x.BusinessUnitCode != null && x.BusinessUnitCode.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<GLBudgetEntry> ApplyDefaultSorting(IQueryable<GLBudgetEntry> query) =>
        query.OrderBy(x => x.EntryNo);

    private async Task ValidateAsync(CreateUpdateGLBudgetEntryDto input)
    {
        await Relations.EnsureExistsAsync<GLBudgetName>(x => x.Name == CodeTableEntity.NormalizeCode(input.BudgetName), "G/L Budget Name", input.BudgetName);
        await Relations.EnsureGLAccountsExistAsync(
            input.GLAccountNo
        );
    }

    private async Task EnsureKeyIsUniqueAsync(long entryNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.EntryNo == entryNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "G/L Budget Entry").WithData("key", entryNo.ToString());
        }
    }

    /// <summary>The next Entry No. of the table: 1 above the last.</summary>
    private async Task<long> NextEntryNoAsync(CreateUpdateGLBudgetEntryDto input)
    {
        var query = await Repository.GetQueryableAsync();
        var last = await AsyncExecuter.FirstOrDefaultAsync(query.OrderByDescending(x => x.EntryNo));

        return (last?.EntryNo ?? 0) + 1;
    }
}

/// <summary>Comment Lines.</summary>
public class CommentLineAppService : ErpTableAppService<CommentLine, CommentLineDto, GetCommentLineListInput, CreateUpdateCommentLineDto>, ICommentLineAppService
{
    public CommentLineAppService(IRepository<CommentLine, Guid> repository)
        : base(repository, ErpPermissions.Comments.Default) { }

    public override async Task<CommentLineDto> CreateAsync(CreateUpdateCommentLineDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : await NextLineNoAsync(input);
        await EnsureKeyIsUniqueAsync(input.TableName, CodeTableEntity.NormalizeCode(input.No), lineNo, null);

        var entity = new CommentLine(GuidGenerator.Create(), input.TableName, CodeTableEntity.NormalizeCode(input.No), lineNo);
        entity.Set(input.Date, input.Code, input.Comment);

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<CommentLineDto> UpdateAsync(Guid id, CreateUpdateCommentLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : entity.LineNo;
        await EnsureKeyIsUniqueAsync(input.TableName, CodeTableEntity.NormalizeCode(input.No), lineNo, id);

        entity.SetKey(input.TableName, CodeTableEntity.NormalizeCode(input.No), lineNo);
        entity.Set(input.Date, input.Code, input.Comment);

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<CommentLine>> CreateFilteredQueryAsync(GetCommentLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var no = input.No?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!no.IsNullOrEmpty(), x => x.No == no)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || (x.Code != null && x.Code.ToLower().Contains(filter))
                    || (x.Comment != null && x.Comment.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<CommentLine> ApplyDefaultSorting(IQueryable<CommentLine> query) =>
        query.OrderBy(x => x.TableName).ThenBy(x => x.No).ThenBy(x => x.LineNo);

    private static Task ValidateAsync(CreateUpdateCommentLineDto input) => Task.CompletedTask;

    private async Task EnsureKeyIsUniqueAsync(CommentLineTableName tableName, string no, int lineNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.TableName == tableName && x.No == no && x.LineNo == lineNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Comment Line").WithData("key", tableName.ToString() + " " + no + " " + lineNo.ToString());
        }
    }

    /// <summary>The next Line No. of the parent record: 10000 above the last.</summary>
    private async Task<int> NextLineNoAsync(CreateUpdateCommentLineDto input)
    {
        var tableName = input.TableName;
        var no = CodeTableEntity.NormalizeCode(input.No);
        var query = await Repository.GetQueryableAsync();
        var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TableName == tableName && x.No == no).OrderByDescending(x => x.LineNo));

        return (last?.LineNo ?? 0) + 10000;
    }
}
