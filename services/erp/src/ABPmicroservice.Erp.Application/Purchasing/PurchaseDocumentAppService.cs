using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Workflows;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Purchasing;

[Authorize(ErpPermissions.PurchaseDocuments.Default)]
public class PurchaseDocumentAppService
    : ErpCrudAppService<
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
    private readonly PurchasesPayablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly ApprovalsManager _approvalsManager;

    public PurchaseDocumentAppService(
        IRepository<PurchaseHeader, Guid> repository,
        IRepository<Vendor, Guid> vendorRepository,
        PurchasePostingEngine purchasePostingEngine,
        PurchasesPayablesSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        ApprovalsManager approvalsManager
    )
        : base(repository)
    {
        _vendorRepository = vendorRepository;
        _purchasePostingEngine = purchasePostingEngine;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _approvalsManager = approvalsManager;

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

        // Blank takes the next number of the series set up for this document type (BC: InitSeries).
        var setup = await _setupManager.GetAsync();
        var no = await _noSeriesManager.ResolveNoAsync(setup.GetDocumentNos(input.DocumentType), input.No, input.PostingDate);

        if (await Repository.AnyAsync(x => x.DocumentType == input.DocumentType && x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }

        var header = new PurchaseHeader(
            GuidGenerator.Create(),
            input.DocumentType,
            no,
            vendor.Id,
            vendor.No,
            vendor.Name,
            input.PostingDate
        );

        await ApplyHeaderAsync(header, input, vendor);
        ReplaceLines(header, input);
        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(header, vendor);

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

        var vendor = await _vendorRepository.GetAsync(header.VendorId);
        await ApplyHeaderAsync(header, input, vendor);
        ReplaceLines(header, input);
        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(header, vendor);

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

        // With an approval workflow in force, only a completed approval releases the document.
        await _approvalsManager.EnsureCanReleaseAsync(ApprovalKind, header);

        header.Release();
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Update)]
    public async Task<PurchaseHeaderDto> ReopenAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);

        // A pending request has to be canceled, not bypassed by reopening.
        if (header.Status == DocumentStatus.PendingApproval)
        {
            throw new BusinessException(ErpErrorCodes.Approvals.PendingApproval).WithData("documentNo", header.No);
        }

        header.Reopen();
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    // Not named "PostAsync": ABP's conventional routing strips the HTTP-verb prefix,
    // which would expose posting as a bare POST /{id}. This yields POST /{id}/run-posting.
    [Authorize(ErpPermissions.PurchaseDocuments.Post)]
    public async Task<PurchaseHeaderDto> RunPostingAsync(Guid id)
    {
        // Posting releases an open document first, so the same approval rule applies.
        await _approvalsManager.EnsureCanReleaseAsync(ApprovalKind, await GetEntityByIdAsync(id));

        await _purchasePostingEngine.PostAsync(id);
        return await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id));
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Update)]
    public async Task<ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id)
    {
        var result = await _approvalsManager.SendApprovalRequestAsync(ApprovalKind, id, GetUserId());

        return new ApprovalRequestResultDto
        {
            AutoApproved = result.AutoApproved,
            ApproverCount = result.ApproverCount,
            FirstApproverUserName = result.FirstApproverUserName,
        };
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Update)]
    public async Task CancelApprovalRequestAsync(Guid id)
    {
        await _approvalsManager.CancelApprovalRequestAsync(ApprovalKind, id, GetUserId());
    }

    private const ApprovalDocumentKind ApprovalKind = ApprovalDocumentKind.PurchaseDocument;

    private Guid GetUserId()
    {
        return CurrentUser.Id ?? throw new AbpAuthorizationException();
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

    /// <summary>
    /// The header fields; payment terms and currency default to the vendor's, and a blank due date
    /// follows from the payment terms, as BC fills them in from the vendor.
    /// </summary>
    private async Task ApplyHeaderAsync(PurchaseHeader header, CreateUpdatePurchaseHeaderDto input, Vendor vendor)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<PaymentTerms>(input.PaymentTermsCode);
        await codes.EnsureExistsAsync<Currency>(input.CurrencyCode);
        await codes.EnsureExistsAsync<Location>(input.LocationCode);

        var paymentTermsCode = CodeTableEntity.NormalizeCode(input.PaymentTermsCode) ?? vendor.PaymentTermsCode;
        var dueDate = input.DueDate
            ?? await LazyServiceProvider.LazyGetRequiredService<PaymentTermsManager>().CalculateDueDateAsync(paymentTermsCode, input.PostingDate);

        header.SetDates(input.PostingDate, dueDate, input.ExpectedReceiptDate);
        header.SetCurrency(CodeTableEntity.NormalizeCode(input.CurrencyCode) ?? vendor.CurrencyCode);
        header.SetPaymentTerms(paymentTermsCode);
        header.SetLocation(input.LocationCode);
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
