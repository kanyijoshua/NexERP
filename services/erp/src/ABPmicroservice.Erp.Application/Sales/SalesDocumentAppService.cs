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

namespace ABPmicroservice.Erp.Sales;

[Authorize(ErpPermissions.SalesDocuments.Default)]
public class SalesDocumentAppService
    : ErpCrudAppService<
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

        // Blank takes the next number of the series set up for this document type.
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

        await ApplyHeaderAsync(header, input, customer);
        ReplaceLines(header, input);
        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(header, customer);

        await Repository.InsertAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    public override async Task<SalesHeaderDto> UpdateAsync(Guid id, CreateUpdateSalesHeaderDto input)
    {
        await CheckUpdatePolicyAsync();

        var header = await GetEntityByIdAsync(id);

        // A released document is frozen until it is reopened.
        if (header.Status != DocumentStatus.Open)
        {
            throw new DocumentNotOpenException(header.No);
        }

        var customer = await _customerRepository.GetAsync(header.CustomerId);
        await ApplyHeaderAsync(header, input, customer);
        ReplaceLines(header, input);
        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(header, customer);

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
        await EnsureWithinCreditLimitAsync(header);

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
        var toPost = await GetEntityByIdAsync(id);
        await _approvalsManager.EnsureCanReleaseAsync(ApprovalKind, toPost);
        if (toPost.Status == DocumentStatus.Open)
        {
            await EnsureWithinCreditLimitAsync(toPost);
        }

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

    /// <summary>
    /// The header fields; payment terms and currency default to the customer's, and a blank due
    /// date follows from the payment terms.
    /// </summary>
    private async Task ApplyHeaderAsync(SalesHeader header, CreateUpdateSalesHeaderDto input, Customer customer)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<PaymentTerms>(input.PaymentTermsCode);
        await codes.EnsureExistsAsync<Currency>(input.CurrencyCode);
        await codes.EnsureExistsAsync<Location>(input.LocationCode);

        var paymentTermsCode = CodeTableEntity.NormalizeCode(input.PaymentTermsCode) ?? customer.PaymentTermsCode;
        var dueDate = input.DueDate
            ?? await LazyServiceProvider.LazyGetRequiredService<PaymentTermsManager>().CalculateDueDateAsync(paymentTermsCode, input.PostingDate);

        header.SetDates(input.PostingDate, dueDate);
        header.SetCurrency(CodeTableEntity.NormalizeCode(input.CurrencyCode) ?? customer.CurrencyCode);
        header.SetPaymentTerms(paymentTermsCode);
        header.SetLocation(input.LocationCode);
        header.SetExternalDocumentNo(input.ExternalDocumentNo);
    }

    /// <summary>
    /// The credit limit check on release: a customer with a credit limit may not be sold more than
    /// the limit allows. A credit limit of zero means no limit.
    /// </summary>
    private async Task EnsureWithinCreditLimitAsync(SalesHeader header)
    {
        if (header.DocumentType == SalesDocumentType.CreditMemo)
        {
            return;
        }

        var setup = await _setupManager.GetAsync();
        if (setup.CreditWarnings is not (CreditWarnings.BothWarnings or CreditWarnings.CreditLimit))
        {
            return;
        }

        var customer = await _customerRepository.GetAsync(header.CustomerId);
        var exposure = customer.Balance + header.TotalAmountIncludingVat;
        if (customer.CreditLimit > 0 && exposure > customer.CreditLimit)
        {
            throw new BusinessException(ErpErrorCodes.Sales.CreditLimitExceeded)
                .WithData("customerNo", customer.No)
                .WithData("creditLimit", customer.CreditLimit.ToString("N2"))
                .WithData("exposure", exposure.ToString("N2"));
        }
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
