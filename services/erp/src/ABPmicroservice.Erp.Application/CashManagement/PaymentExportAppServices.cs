using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.CashManagement;

[Authorize(ErpPermissions.PaymentExport.Default)]
public class PaymentJournalAppService : ErpAppService, IPaymentJournalAppService
{
    private readonly PaymentJournalManager _manager;

    public PaymentJournalAppService(PaymentJournalManager manager)
    {
        _manager = manager;
    }

    /// <summary>The id is the general journal batch the payment lines go to.</summary>
    public async Task<SuggestVendorPaymentsResultDto> SuggestVendorPaymentsAsync(Guid id, SuggestVendorPaymentsInput input)
    {
        var postingDate = input.PostingDate ?? Clock.Now.Date;
        var added = await _manager.SuggestVendorPaymentsAsync(id, new VendorPaymentSuggestion
        {
            PostingDate = postingDate,
            LastDueDate = input.LastDueDate ?? postingDate,
            UsePaymentDiscounts = input.UsePaymentDiscounts,
            VendorNos = (input.VendorNos ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            BankAccountNo = input.BankAccountNo,
            DocumentNo = input.DocumentNo,
        });
        return new SuggestVendorPaymentsResultDto { LinesAdded = added };
    }

    public async Task<BankFileDto> ExportAsync(Guid id, ExportPaymentsInput input) =>
        Map(await _manager.ExportAsync(id, input?.Format ?? PaymentFileFormat.Iso20022CreditTransfer, Clock.Now));

    internal static BankFileDto Map(BankFile file) => new()
    {
        FileName = file.FileName,
        ContentType = file.ContentType,
        Content = file.Content,
        TransactionCount = file.TransactionCount,
        TotalAmount = file.TotalAmount,
    };
}

[Authorize(ErpPermissions.DirectDebits.Default)]
public class DirectDebitMandateAppService
    : ErpTableAppService<DirectDebitMandate, DirectDebitMandateDto, GetDirectDebitMandateListInput, CreateUpdateDirectDebitMandateDto>,
        IDirectDebitMandateAppService
{
    public DirectDebitMandateAppService(IRepository<DirectDebitMandate, Guid> repository)
        : base(repository, ErpPermissions.DirectDebits.Default) { }

    public override async Task<DirectDebitMandateDto> CreateAsync(CreateUpdateDirectDebitMandateDto input)
    {
        await CheckCreatePolicyAsync();
        var code = Companies.CodeTableEntity.NormalizeCode(input.Code);
        if (await Repository.AnyAsync(m => m.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.MandateCodeExists).WithData("code", code);
        }

        var mandate = new DirectDebitMandate(GuidGenerator.Create(), code, (await CustomerAsync(input.CustomerNo)).No);
        await ApplyAsync(mandate, input);
        await Repository.InsertAsync(mandate, autoSave: true);
        return await MapToGetOutputDtoAsync(mandate);
    }

    /// <summary>The customer and code of a mandate stay; its account, dates and limits change.</summary>
    public override async Task<DirectDebitMandateDto> UpdateAsync(Guid id, CreateUpdateDirectDebitMandateDto input)
    {
        await CheckUpdatePolicyAsync();
        var mandate = await GetEntityByIdAsync(id);
        await ApplyAsync(mandate, input);
        await Repository.UpdateAsync(mandate, autoSave: true);
        return await MapToGetOutputDtoAsync(mandate);
    }

    private async Task ApplyAsync(DirectDebitMandate mandate, CreateUpdateDirectDebitMandateDto input)
    {
        var account = Companies.CodeTableEntity.NormalizeCode(input.CustomerBankAccountCode);
        var customerNo = mandate.CustomerNo;
        if (!await LazyServiceProvider.LazyGetRequiredService<IRepository<CustomerBankAccount, Guid>>().AnyAsync(b => b.CustomerNo == customerNo && b.Code == account))
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.CustomerBankAccountMissing).WithData("customerNo", customerNo).WithData("code", account ?? "");
        }

        mandate.Set(account, input.ValidFrom == default ? Clock.Now.Date : input.ValidFrom, input.ValidTo, input.Type, input.ExpectedUses, input.Blocked);
    }

    private async Task<Customer> CustomerAsync(string customerNo)
    {
        var no = customerNo?.Trim();
        return await LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>().FirstOrDefaultAsync(c => c.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", no ?? "");
    }

    protected override async Task<IQueryable<DirectDebitMandate>> CreateFilteredQueryAsync(GetDirectDebitMandateListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var customer = input.CustomerNo?.Trim();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!customer.IsNullOrEmpty(), m => m.CustomerNo == customer)
            .WhereIf(!filter.IsNullOrEmpty(), m => m.Code.ToLower().Contains(filter) || m.CustomerNo.ToLower().Contains(filter));
    }

    protected override IQueryable<DirectDebitMandate> ApplyDefaultSorting(IQueryable<DirectDebitMandate> query) => query.OrderBy(m => m.Code);
}

[Authorize(ErpPermissions.DirectDebits.Default)]
public class DirectDebitCollectionAppService
    : ErpReadOnlyAppService<DirectDebitCollection, DirectDebitCollectionDto, Guid, GetDirectDebitCollectionListInput>,
        IDirectDebitCollectionAppService
{
    private readonly IRepository<DirectDebitCollection, Guid> _repository;

    private DirectDebitManager Manager => LazyServiceProvider.LazyGetRequiredService<DirectDebitManager>();

    public DirectDebitCollectionAppService(IRepository<DirectDebitCollection, Guid> repository)
        : base(repository)
    {
        _repository = repository;
        GetPolicyName = ErpPermissions.DirectDebits.Default;
        GetListPolicyName = ErpPermissions.DirectDebits.Default;
    }

    protected override async Task<DirectDebitCollection> GetEntityByIdAsync(Guid id) => await _repository.GetAsync(id, includeDetails: true);

    [Authorize(ErpPermissions.DirectDebits.Create)]
    public async Task<DirectDebitCollectionDto> MakeAsync(MakeDirectDebitCollectionInput input) =>
        await MapToGetOutputDtoAsync(await Manager.CreateCollectionAsync(input.FromDueDate, input.ToDueDate, input.BankAccountNo, input.CreditorIdentifier, Clock.Now.Date));

    [Authorize(ErpPermissions.DirectDebits.Update)]
    public async Task<BankFileDto> ExportAsync(Guid id) => PaymentJournalAppService.Map(await Manager.ExportAsync(id, Clock.Now));

    [Authorize(ErpPermissions.DirectDebits.Update)]
    public async Task<DirectDebitReceiptLinesResultDto> MakeReceiptLinesAsync(Guid id, MakeDirectDebitReceiptLinesInput input) =>
        new() { LinesAdded = await Manager.CreateReceiptLinesAsync(id, input.GenJournalBatchId, input.PostingDate ?? Clock.Now.Date) };

    [Authorize(ErpPermissions.DirectDebits.Update)]
    public async Task<DirectDebitCollectionDto> CloseAsync(Guid id) => await MapToGetOutputDtoAsync(await Manager.CloseAsync(id));

    /// <summary>Only a collection that has not gone to the bank can be deleted.</summary>
    [Authorize(ErpPermissions.DirectDebits.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var collection = await GetEntityByIdAsync(id);
        if (collection.Status != DirectDebitCollectionStatus.New)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.CollectionStatusWrong).WithData("no", collection.No).WithData("status", collection.Status.ToString());
        }

        await _repository.DeleteAsync(collection, autoSave: true);
    }

    protected override async Task<IQueryable<DirectDebitCollection>> CreateFilteredQueryAsync(GetDirectDebitCollectionListInput input)
    {
        var query = await _repository.WithDetailsAsync(c => c.Entries);
        return query.WhereIf(input.Status.HasValue, c => c.Status == input.Status.Value);
    }

    protected override IQueryable<DirectDebitCollection> ApplyDefaultSorting(IQueryable<DirectDebitCollection> query) => query.OrderByDescending(c => c.No);
}

[Authorize(ErpPermissions.DirectDebits.Default)]
public class DirectDebitCollectionEntryAppService
    : ErpReadOnlyAppService<DirectDebitCollectionEntry, DirectDebitCollectionEntryDto, Guid, GetDirectDebitCollectionEntryListInput>,
        IDirectDebitCollectionEntryAppService
{
    public DirectDebitCollectionEntryAppService(IRepository<DirectDebitCollectionEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.DirectDebits.Default;
        GetListPolicyName = ErpPermissions.DirectDebits.Default;
    }

    protected override async Task<IQueryable<DirectDebitCollectionEntry>> CreateFilteredQueryAsync(GetDirectDebitCollectionEntryListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        return query.WhereIf(input.DirectDebitCollectionId.HasValue, e => e.DirectDebitCollectionId == input.DirectDebitCollectionId.Value);
    }

    protected override IQueryable<DirectDebitCollectionEntry> ApplyDefaultSorting(IQueryable<DirectDebitCollectionEntry> query) => query.OrderBy(e => e.LineNo);
}
