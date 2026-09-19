using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Purchasing;

[Authorize(ErpPermissions.PurchaseDocuments.Default)]
public class PurchaseDocumentAppService
    : CrudAppService<
        PurchaseHeader,
        PurchaseHeaderDto,
        Guid,
        GetPurchaseDocumentListInput,
        CreateUpdatePurchaseHeaderDto,
        CreateUpdatePurchaseHeaderDto
    >,
        IPurchaseDocumentAppService
{
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly PurchasePostingEngine _purchasePostingEngine;

    public PurchaseDocumentAppService(
        IRepository<PurchaseHeader, Guid> repository,
        IRepository<Vendor, Guid> vendorRepository,
        PurchasePostingEngine purchasePostingEngine
    )
        : base(repository)
    {
        _vendorRepository = vendorRepository;
        _purchasePostingEngine = purchasePostingEngine;

        GetPolicyName = ErpPermissions.PurchaseDocuments.Default;
        GetListPolicyName = ErpPermissions.PurchaseDocuments.Default;
        CreatePolicyName = ErpPermissions.PurchaseDocuments.Create;
        UpdatePolicyName = ErpPermissions.PurchaseDocuments.Update;
        DeletePolicyName = ErpPermissions.PurchaseDocuments.Delete;
    }

    public override async Task<PurchaseHeaderDto> CreateAsync(CreateUpdatePurchaseHeaderDto input)
    {
        await CheckCreatePolicyAsync();

        var vendor = await GetPurchasableVendorAsync(input.VendorId);

        if (await Repository.AnyAsync(x => x.DocumentType == input.DocumentType && x.No == input.No))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", input.No);
        }

        var header = new PurchaseHeader(
            GuidGenerator.Create(),
            input.DocumentType,
            input.No,
            vendor.Id,
            vendor.No,
            vendor.Name,
            input.PostingDate
        );

        ApplyHeader(header, input);
        ReplaceLines(header, input);

        await Repository.InsertAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    public override async Task<PurchaseHeaderDto> UpdateAsync(Guid id, CreateUpdatePurchaseHeaderDto input)
    {
        await CheckUpdatePolicyAsync();

        var header = await GetEntityByIdAsync(id);

        // A released document is frozen until it is reopened, as in Business Central.
        if (header.Status != DocumentStatus.Open)
        {
            throw new DocumentNotOpenException(header.No);
        }

        ApplyHeader(header, input);
        ReplaceLines(header, input);

        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var header = await GetEntityByIdAsync(id);
        if (header.Posted)
        {
            throw new DocumentAlreadyPostedException(header.No);
        }

        await Repository.DeleteAsync(header, autoSave: true);
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Update)]
    public async Task<PurchaseHeaderDto> ReleaseAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        if (!header.Lines.Any())
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentHasNoLines).WithData("documentNo", header.No);
        }

        header.Release();
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Update)]
    public async Task<PurchaseHeaderDto> ReopenAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        header.Reopen();
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    // Not named "PostAsync": ABP's conventional routing strips the HTTP-verb prefix,
    // which would expose posting as a bare POST /{id}. This yields POST /{id}/run-posting.
    [Authorize(ErpPermissions.PurchaseDocuments.Post)]
    public async Task<PurchaseHeaderDto> RunPostingAsync(Guid id)
    {
        await _purchasePostingEngine.PostAsync(id);
        return await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id));
    }

    protected override async Task<PurchaseHeader> GetEntityByIdAsync(Guid id)
    {
        return await Repository.GetAsync(id, includeDetails: true);
    }

    protected override async Task<IQueryable<PurchaseHeader>> CreateFilteredQueryAsync(GetPurchaseDocumentListInput input)
    {
        // Lists show headers only; lines are loaded when a document is opened.
        var query = await Repository.GetQueryableAsync();

        return query
            .WhereIf(
                !input.Filter.IsNullOrWhiteSpace(),
                x => x.No.Contains(input.Filter) || x.BuyFromVendorNo.Contains(input.Filter) || x.BuyFromVendorName.Contains(input.Filter)
            )
            .WhereIf(input.DocumentType.HasValue, x => x.DocumentType == input.DocumentType.Value)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(input.VendorId.HasValue, x => x.VendorId == input.VendorId.Value);
    }

    protected override IQueryable<PurchaseHeader> ApplyDefaultSorting(IQueryable<PurchaseHeader> query)
    {
        return query.OrderByDescending(x => x.PostingDate).ThenByDescending(x => x.No);
    }

    private async Task<Vendor> GetPurchasableVendorAsync(Guid vendorId)
    {
        var vendor = await _vendorRepository.GetAsync(vendorId);
        if (vendor.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.Vendors.VendorBlocked).WithData("vendorNo", vendor.No);
        }

        return vendor;
    }

    private static void ApplyHeader(PurchaseHeader header, CreateUpdatePurchaseHeaderDto input)
    {
        header.SetDates(input.PostingDate, input.DueDate, input.ExpectedReceiptDate);
        header.SetCurrency(input.CurrencyCode);
        header.SetPaymentTerms(input.PaymentTermsCode);
        header.SetVendorInvoiceNo(input.VendorInvoiceNo);
    }

    // The client always sends the whole document, so the line set is replaced wholesale.
    private void ReplaceLines(PurchaseHeader header, CreateUpdatePurchaseHeaderDto input)
    {
        header.ClearLines();

        foreach (var line in input.Lines)
        {
            header.AddLine(
                GuidGenerator.Create(),
                line.Type,
                line.No,
                line.Description,
                line.Quantity,
                line.DirectUnitCost,
                line.LineDiscountPercent,
                line.UnitOfMeasureCode
            );
        }
    }
}
