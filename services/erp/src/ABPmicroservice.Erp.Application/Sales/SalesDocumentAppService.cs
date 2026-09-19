using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
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

    public SalesDocumentAppService(
        IRepository<SalesHeader, Guid> repository,
        IRepository<Customer, Guid> customerRepository,
        SalesPostingEngine salesPostingEngine
    )
        : base(repository)
    {
        _customerRepository = customerRepository;
        _salesPostingEngine = salesPostingEngine;

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

        if (await Repository.AnyAsync(x => x.DocumentType == input.DocumentType && x.No == input.No))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", input.No);
        }

        var header = new SalesHeader(
            GuidGenerator.Create(),
            input.DocumentType,
            input.No,
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

        header.Release();
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.SalesDocuments.Update)]
    public async Task<SalesHeaderDto> ReopenAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        header.Reopen();
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    // Not named "PostAsync": ABP's conventional routing strips the HTTP-verb prefix,
    // which would expose posting as a bare POST /{id}. This yields POST /{id}/run-posting.
    [Authorize(ErpPermissions.SalesDocuments.Post)]
    public async Task<SalesHeaderDto> RunPostingAsync(Guid id)
    {
        await _salesPostingEngine.PostAsync(id);
        return await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id));
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
