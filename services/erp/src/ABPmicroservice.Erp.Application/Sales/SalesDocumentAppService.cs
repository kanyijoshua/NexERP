using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Workflows;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Sales;

[Authorize(ErpPermissions.SalesDocuments.Default)]
public class SalesDocumentAppService
    : CrudAppService<
        SalesHeader,
        SalesHeaderDto,
        Guid,
        GetSalesDocumentListInput,
        CreateUpdateSalesHeaderDto,
        CreateUpdateSalesHeaderDto
    >,
        ISalesDocumentAppService
{
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly SalesPostingEngine _salesPostingEngine;
    private readonly SalesReceivablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly ApprovalsManager _approvalsManager;

    public SalesDocumentAppService(
        IRepository<SalesHeader, Guid> repository,
        IRepository<Customer, Guid> customerRepository,
        SalesPostingEngine salesPostingEngine,
        SalesReceivablesSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        ApprovalsManager approvalsManager
    )
        : base(repository)
    {
        _customerRepository = customerRepository;
        _salesPostingEngine = salesPostingEngine;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _approvalsManager = approvalsManager;

        GetPolicyName = ErpPermissions.SalesDocuments.Default;
        GetListPolicyName = ErpPermissions.SalesDocuments.Default;
        CreatePolicyName = ErpPermissions.SalesDocuments.Create;
        UpdatePolicyName = ErpPermissions.SalesDocuments.Update;
        DeletePolicyName = ErpPermissions.SalesDocuments.Delete;
    }

    public override async Task<SalesHeaderDto> CreateAsync(CreateUpdateSalesHeaderDto input)
    {
        await CheckCreatePolicyAsync();

        var customer = await GetSellableCustomerAsync(input.CustomerId);

        // Blank takes the next number of the series set up for this document type (BC: InitSeries).
        var setup = await _setupManager.GetAsync();
        var no = await _noSeriesManager.ResolveNoAsync(setup.GetDocumentNos(input.DocumentType), input.No, input.PostingDate);

        if (await Repository.AnyAsync(x => x.DocumentType == input.DocumentType && x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }

        var header = new SalesHeader(
            GuidGenerator.Create(),
            input.DocumentType,
            no,
            customer.Id,
            customer.No,
            customer.Name,
            input.PostingDate
        );

        ApplyHeader(header, input);
        ReplaceLines(header, input);

        await Repository.InsertAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    public override async Task<SalesHeaderDto> UpdateAsync(Guid id, CreateUpdateSalesHeaderDto input)
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

    [Authorize(ErpPermissions.SalesDocuments.Update)]
    public async Task<SalesHeaderDto> ReleaseAsync(Guid id)
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

    [Authorize(ErpPermissions.SalesDocuments.Update)]
    public async Task<SalesHeaderDto> ReopenAsync(Guid id)
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
    [Authorize(ErpPermissions.SalesDocuments.Post)]
    public async Task<SalesHeaderDto> RunPostingAsync(Guid id)
    {
        // Posting releases an open document first, so the same approval rule applies.
        await _approvalsManager.EnsureCanReleaseAsync(ApprovalKind, await GetEntityByIdAsync(id));

        await _salesPostingEngine.PostAsync(id);
        return await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id));
    }

    [Authorize(ErpPermissions.SalesDocuments.Update)]
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

    [Authorize(ErpPermissions.SalesDocuments.Update)]
    public async Task CancelApprovalRequestAsync(Guid id)
    {
        await _approvalsManager.CancelApprovalRequestAsync(ApprovalKind, id, GetUserId());
    }

    private const ApprovalDocumentKind ApprovalKind = ApprovalDocumentKind.SalesDocument;

    private Guid GetUserId()
    {
        return CurrentUser.Id ?? throw new AbpAuthorizationException();
    }

    protected override async Task<SalesHeader> GetEntityByIdAsync(Guid id)
    {
        return await Repository.GetAsync(id, includeDetails: true);
    }

    protected override async Task<IQueryable<SalesHeader>> CreateFilteredQueryAsync(GetSalesDocumentListInput input)
    {
        // Lists show headers only; lines are loaded when a document is opened.
        var query = await Repository.GetQueryableAsync();

        return query
            .WhereIf(
                !input.Filter.IsNullOrWhiteSpace(),
                x => x.No.Contains(input.Filter) || x.SellToCustomerNo.Contains(input.Filter) || x.SellToCustomerName.Contains(input.Filter)
            )
            .WhereIf(input.DocumentType.HasValue, x => x.DocumentType == input.DocumentType.Value)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId.Value);
    }

    protected override IQueryable<SalesHeader> ApplyDefaultSorting(IQueryable<SalesHeader> query)
    {
        return query.OrderByDescending(x => x.PostingDate).ThenByDescending(x => x.No);
    }

    private async Task<Customer> GetSellableCustomerAsync(Guid customerId)
    {
        var customer = await _customerRepository.GetAsync(customerId);
        if (customer.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.Customers.CustomerBlocked).WithData("customerNo", customer.No);
        }

        return customer;
    }

    private static void ApplyHeader(SalesHeader header, CreateUpdateSalesHeaderDto input)
    {
        header.SetDates(input.PostingDate, input.DueDate);
        header.SetCurrency(input.CurrencyCode);
        header.SetPaymentTerms(input.PaymentTermsCode);
        header.SetExternalDocumentNo(input.ExternalDocumentNo);
    }

    // The client always sends the whole document, so the line set is replaced wholesale.
    private void ReplaceLines(SalesHeader header, CreateUpdateSalesHeaderDto input)
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
                line.UnitPrice,
                line.LineDiscountPercent,
                line.UnitOfMeasureCode
            );
        }
    }
}
