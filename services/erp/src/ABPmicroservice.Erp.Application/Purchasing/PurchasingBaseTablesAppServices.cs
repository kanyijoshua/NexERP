using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Permissions;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>Vendor Bank Accounts.</summary>
public class VendorBankAccountAppService : ErpTableAppService<VendorBankAccount, VendorBankAccountDto, GetVendorBankAccountListInput, CreateUpdateVendorBankAccountDto>, IVendorBankAccountAppService
{
    public VendorBankAccountAppService(IRepository<VendorBankAccount, Guid> repository)
        : base(repository, ErpPermissions.Vendors.Default) { }

    public override async Task<VendorBankAccountDto> CreateAsync(CreateUpdateVendorBankAccountDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.Code), null);

        var entity = new VendorBankAccount(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.Code));
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

    public override async Task<VendorBankAccountDto> UpdateAsync(Guid id, CreateUpdateVendorBankAccountDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.Code), id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.Code));
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

    protected override async Task<IQueryable<VendorBankAccount>> CreateFilteredQueryAsync(GetVendorBankAccountListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var vendorNo = input.VendorNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!vendorNo.IsNullOrEmpty(), x => x.VendorNo == vendorNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.VendorNo.ToLower().Contains(filter)
                    || x.Code.ToLower().Contains(filter)
                    || (x.Name != null && x.Name.ToLower().Contains(filter))
                    || (x.Name2 != null && x.Name2.ToLower().Contains(filter))
                    || (x.Address != null && x.Address.ToLower().Contains(filter))
                    || (x.Address2 != null && x.Address2.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<VendorBankAccount> ApplyDefaultSorting(IQueryable<VendorBankAccount> query) =>
        query.OrderBy(x => x.VendorNo).ThenBy(x => x.Code);

    private async Task ValidateAsync(CreateUpdateVendorBankAccountDto input)
    {
        await Relations.EnsureNoExistsAsync<Vendor>(input.VendorNo);
        await CodeTableChecker.EnsureExistsAsync<Currency>(input.CurrencyCode);
        await CodeTableChecker.EnsureExistsAsync<CountryRegion>(input.CountryRegionCode);
    }

    private async Task EnsureKeyIsUniqueAsync(string vendorNo, string code, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.VendorNo == vendorNo && x.Code == code && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Vendor Bank Account").WithData("key", vendorNo + " " + code);
        }
    }
}

/// <summary>Detailed Vendor Ledg. Entries.</summary>
public class DetailedVendorLedgEntryAppService : ErpReadOnlyAppService<DetailedVendorLedgEntry, DetailedVendorLedgEntryDto, Guid, GetDetailedVendorLedgEntryListInput>, IDetailedVendorLedgEntryAppService
{
    public DetailedVendorLedgEntryAppService(IRepository<DetailedVendorLedgEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Vendors.Default;
        GetListPolicyName = ErpPermissions.Vendors.Default;
    }

    protected override async Task<IQueryable<DetailedVendorLedgEntry>> CreateFilteredQueryAsync(GetDetailedVendorLedgEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var vendorNo = input.VendorNo?.Trim().ToUpperInvariant();
        var documentNo = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!vendorNo.IsNullOrEmpty(), x => x.VendorNo == vendorNo)
            .WhereIf(!documentNo.IsNullOrEmpty(), x => x.DocumentNo == documentNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => (x.DocumentNo != null && x.DocumentNo.ToLower().Contains(filter))
                    || (x.VendorNo != null && x.VendorNo.ToLower().Contains(filter))
                    || (x.CurrencyCode != null && x.CurrencyCode.ToLower().Contains(filter))
                    || (x.UserId != null && x.UserId.ToLower().Contains(filter))
                    || (x.SourceCode != null && x.SourceCode.ToLower().Contains(filter))
                    || (x.JournalBatchName != null && x.JournalBatchName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<DetailedVendorLedgEntry> ApplyDefaultSorting(IQueryable<DetailedVendorLedgEntry> query) =>
        query.OrderByDescending(x => x.EntryNo);
}

/// <summary>Order Addresses.</summary>
public class OrderAddressAppService : ErpTableAppService<OrderAddress, OrderAddressDto, GetOrderAddressListInput, CreateUpdateOrderAddressDto>, IOrderAddressAppService
{
    public OrderAddressAppService(IRepository<OrderAddress, Guid> repository)
        : base(repository, ErpPermissions.Vendors.Default) { }

    public override async Task<OrderAddressDto> CreateAsync(CreateUpdateOrderAddressDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.Code), null);

        var entity = new OrderAddress(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.Code));
        entity.Set(
            input.Name,
            input.Name2,
            input.Address,
            input.Address2,
            input.City,
            input.Contact,
            input.PhoneNo,
            input.CountryRegionCode,
            input.FaxNo,
            input.PostCode,
            input.County,
            input.Email
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<OrderAddressDto> UpdateAsync(Guid id, CreateUpdateOrderAddressDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.Code), id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.Code));
        entity.Set(
            input.Name,
            input.Name2,
            input.Address,
            input.Address2,
            input.City,
            input.Contact,
            input.PhoneNo,
            input.CountryRegionCode,
            input.FaxNo,
            input.PostCode,
            input.County,
            input.Email
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<OrderAddress>> CreateFilteredQueryAsync(GetOrderAddressListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var vendorNo = input.VendorNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!vendorNo.IsNullOrEmpty(), x => x.VendorNo == vendorNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.VendorNo.ToLower().Contains(filter)
                    || x.Code.ToLower().Contains(filter)
                    || (x.Name != null && x.Name.ToLower().Contains(filter))
                    || (x.Name2 != null && x.Name2.ToLower().Contains(filter))
                    || (x.Address != null && x.Address.ToLower().Contains(filter))
                    || (x.Address2 != null && x.Address2.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<OrderAddress> ApplyDefaultSorting(IQueryable<OrderAddress> query) =>
        query.OrderBy(x => x.VendorNo).ThenBy(x => x.Code);

    private async Task ValidateAsync(CreateUpdateOrderAddressDto input)
    {
        await Relations.EnsureNoExistsAsync<Vendor>(input.VendorNo);
        await CodeTableChecker.EnsureExistsAsync<CountryRegion>(input.CountryRegionCode);
    }

    private async Task EnsureKeyIsUniqueAsync(string vendorNo, string code, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.VendorNo == vendorNo && x.Code == code && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Order Address").WithData("key", vendorNo + " " + code);
        }
    }
}

/// <summary>Item Vendor Catalog.</summary>
public class ItemVendorAppService : ErpTableAppService<ItemVendor, ItemVendorDto, GetItemVendorListInput, CreateUpdateItemVendorDto>, IItemVendorAppService
{
    public ItemVendorAppService(IRepository<ItemVendor, Guid> repository)
        : base(repository, ErpPermissions.Vendors.Default) { }

    public override async Task<ItemVendorDto> CreateAsync(CreateUpdateItemVendorDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.ItemNo), null);

        var entity = new ItemVendor(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.ItemNo));
        entity.Set(input.LeadTimeCalculation, input.VendorItemNo);

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<ItemVendorDto> UpdateAsync(Guid id, CreateUpdateItemVendorDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.ItemNo), id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.VendorNo), CodeTableEntity.NormalizeCode(input.ItemNo));
        entity.Set(input.LeadTimeCalculation, input.VendorItemNo);

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<ItemVendor>> CreateFilteredQueryAsync(GetItemVendorListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var vendorNo = input.VendorNo?.Trim().ToUpperInvariant();
        var itemNo = input.ItemNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!vendorNo.IsNullOrEmpty(), x => x.VendorNo == vendorNo)
            .WhereIf(!itemNo.IsNullOrEmpty(), x => x.ItemNo == itemNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.ItemNo.ToLower().Contains(filter)
                    || x.VendorNo.ToLower().Contains(filter)
                    || (x.LeadTimeCalculation != null && x.LeadTimeCalculation.ToLower().Contains(filter))
                    || (x.VendorItemNo != null && x.VendorItemNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<ItemVendor> ApplyDefaultSorting(IQueryable<ItemVendor> query) =>
        query.OrderBy(x => x.VendorNo).ThenBy(x => x.ItemNo);

    private async Task ValidateAsync(CreateUpdateItemVendorDto input)
    {
        await Relations.EnsureNoExistsAsync<Vendor>(input.VendorNo);
        await Relations.EnsureNoExistsAsync<Item>(input.ItemNo);
    }

    private async Task EnsureKeyIsUniqueAsync(string vendorNo, string itemNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.VendorNo == vendorNo && x.ItemNo == itemNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Item Vendor").WithData("key", vendorNo + " " + itemNo);
        }
    }
}

/// <summary>Purchase Comment Lines.</summary>
public class PurchCommentLineAppService : ErpTableAppService<PurchCommentLine, PurchCommentLineDto, GetPurchCommentLineListInput, CreateUpdatePurchCommentLineDto>, IPurchCommentLineAppService
{
    public PurchCommentLineAppService(IRepository<PurchCommentLine, Guid> repository)
        : base(repository, ErpPermissions.PurchaseDocuments.Default) { }

    public override async Task<PurchCommentLineDto> CreateAsync(CreateUpdatePurchCommentLineDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : await NextLineNoAsync(input);
        await EnsureKeyIsUniqueAsync(input.DocumentType, CodeTableEntity.NormalizeCode(input.No), input.DocumentLineNo, lineNo, null);

        var entity = new PurchCommentLine(GuidGenerator.Create(), input.DocumentType, CodeTableEntity.NormalizeCode(input.No), input.DocumentLineNo, lineNo);
        entity.Set(input.Date, input.Code, input.Comment);

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<PurchCommentLineDto> UpdateAsync(Guid id, CreateUpdatePurchCommentLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : entity.LineNo;
        await EnsureKeyIsUniqueAsync(input.DocumentType, CodeTableEntity.NormalizeCode(input.No), input.DocumentLineNo, lineNo, id);

        entity.SetKey(input.DocumentType, CodeTableEntity.NormalizeCode(input.No), input.DocumentLineNo, lineNo);
        entity.Set(input.Date, input.Code, input.Comment);

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<PurchCommentLine>> CreateFilteredQueryAsync(GetPurchCommentLineListInput input)
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

    protected override IQueryable<PurchCommentLine> ApplyDefaultSorting(IQueryable<PurchCommentLine> query) =>
        query.OrderBy(x => x.DocumentType).ThenBy(x => x.No).ThenBy(x => x.DocumentLineNo).ThenBy(x => x.LineNo);

    private static Task ValidateAsync(CreateUpdatePurchCommentLineDto input) => Task.CompletedTask;

    private async Task EnsureKeyIsUniqueAsync(PurchaseCommentDocumentType documentType, string no, int documentLineNo, int lineNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.DocumentType == documentType && x.No == no && x.DocumentLineNo == documentLineNo && x.LineNo == lineNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Purch. Comment Line").WithData("key", documentType.ToString() + " " + no + " " + documentLineNo.ToString() + " " + lineNo.ToString());
        }
    }

    /// <summary>The next Line No. of the parent record: 10000 above the last.</summary>
    private async Task<int> NextLineNoAsync(CreateUpdatePurchCommentLineDto input)
    {
        var documentType = input.DocumentType;
        var no = CodeTableEntity.NormalizeCode(input.No);
        var documentLineNo = input.DocumentLineNo;
        var query = await Repository.GetQueryableAsync();
        var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.DocumentType == documentType && x.No == no && x.DocumentLineNo == documentLineNo).OrderByDescending(x => x.LineNo));

        return (last?.LineNo ?? 0) + 10000;
    }
}
