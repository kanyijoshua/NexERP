using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Pricing;
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
        await UpsertLinesAsync(header, input);
        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(header, customer);
        ApplyQuantitiesToPost(header, input);

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
        await UpsertLinesAsync(header, input);
        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(header, customer);
        ApplyQuantitiesToPost(header, input);

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

    [Authorize(ErpPermissions.SalesDocuments.Post)]
    public async Task<SalesPostingResultDto> RunOrderPostingAsync(Guid id, SalesOrderPostingInput input)
    {
        var toPost = await GetEntityByIdAsync(id);
        await _approvalsManager.EnsureCanReleaseAsync(ApprovalKind, toPost);
        if (toPost.Status == DocumentStatus.Open)
        {
            await EnsureWithinCreditLimitAsync(toPost);
        }

        foreach (var line in input.Lines)
        {
            toPost.SetQuantitiesToPost(line.LineId, line.QtyToShip, line.QtyToInvoice);
        }

        var result = await _salesPostingEngine.PostAsync(id, input.Ship, input.Invoice);
        return new SalesPostingResultDto
        {
            Document = await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id)),
            InvoiceNo = result.Invoice?.No,
            ShipmentNo = result.Shipment?.No,
        };
    }

    [Authorize(ErpPermissions.SalesDocuments.Create)]
    public async Task<SalesHeaderDto> MakeOrderAsync(Guid id, MakeOrderInput input)
    {
        var order = await Flow.MakeOrderAsync(id, input?.PostingDate ?? Clock.Now.Date);
        return await MapToGetOutputDtoAsync(order);
    }

    public async Task<List<OpenShipmentLineDto>> GetOpenShipmentLinesAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        var lines = await Flow.GetOpenShipmentLinesAsync(id);
        return lines
            .Select(x => new OpenShipmentLineDto
            {
                ShipmentLineId = x.Line.Id,
                ShipmentNo = x.Shipment.No,
                PostingDate = x.Shipment.PostingDate,
                OrderNo = x.Shipment.OrderNo,
                LineNo = x.Line.LineNo,
                Type = x.Line.Type,
                No = x.Line.No,
                Description = x.Line.Description,
                Quantity = x.Line.Quantity,
                QtyNotInvoiced = x.Line.QtyNotInvoiced,
                UnitPrice = x.Line.UnitPrice,
            })
            .ToList();
    }

    [Authorize(ErpPermissions.SalesDocuments.Update)]
    public async Task<SalesHeaderDto> CollectShipmentLinesAsync(Guid id, GetShipmentLinesInput input)
    {
        return await MapToGetOutputDtoAsync(await Flow.GetShipmentLinesAsync(id, input.ShipmentLineIds));
    }

    [Authorize(ErpPermissions.SalesDocuments.Update)]
    public async Task<SalesHeaderDto> CopyLinesFromAsync(Guid id, CopyDocumentLinesInput input)
    {
        return await MapToGetOutputDtoAsync(await Flow.CopyLinesAsync(id, input.SourceDocumentId));
    }

    private SalesDocumentFlow Flow => LazyServiceProvider.LazyGetRequiredService<SalesDocumentFlow>();

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
        header.SetShipTo(
            input.ShipToCode,
            input.ShipToName,
            input.ShipToAddress,
            input.ShipToCity,
            input.ShipToPostCode,
            input.ShipToCountryRegionCode,
            input.ShipToContact
        );
        header.SetShipmentDate(input.ShipmentDate);
        header.SetCustomerPriceGroup(customer.CustomerPriceGroup);
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

    /// <summary>
    /// The client always sends the whole document. Lines it sends back with their id are changed in
    /// place, so what has been shipped and invoiced on them is kept; lines it leaves out are removed,
    /// and new lines are added. An item line without a price takes the price list's.
    /// </summary>
    private async Task UpsertLinesAsync(SalesHeader header, CreateUpdateSalesHeaderDto input)
    {
        var sent = input.Lines.Where(l => l.Id.HasValue).Select(l => l.Id!.Value).ToHashSet();
        foreach (var removed in header.Lines.Where(l => !sent.Contains(l.Id)).Select(l => l.Id).ToList())
        {
            header.RemoveLine(removed);
        }

        foreach (var lineInput in input.Lines)
        {
            var (unitPrice, discount) = await PriceOfAsync(header, lineInput);
            var existing = lineInput.Id.HasValue ? header.Lines.FirstOrDefault(l => l.Id == lineInput.Id.Value) : null;
            var line = existing != null
                ? header.UpdateLine(existing.Id, lineInput.Type, lineInput.No, lineInput.Description, lineInput.Quantity, unitPrice, discount, lineInput.UnitOfMeasureCode)
                : header.AddLine(GuidGenerator.Create(), lineInput.Type, lineInput.No, lineInput.Description, lineInput.Quantity, unitPrice, discount, lineInput.UnitOfMeasureCode);

            line.SetLocation(lineInput.LocationCode);
            line.SetShipmentDate(lineInput.ShipmentDate);
            await SetVariantAndUnitAsync(line, lineInput.VariantCode);
            await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<ABPmicroservice.Erp.Finance.DeferralTemplate>(lineInput.DeferralCode);
            line.SetDeferralCode(lineInput.DeferralCode);
            line.SetProject(lineInput.ProjectNo, lineInput.ProjectTaskNo);
        }
    }

    /// <summary>The variant of an item line and how many base units one unit of its unit of measure holds.</summary>
    private async Task SetVariantAndUnitAsync(SalesLine line, string variantCode)
    {
        line.SetVariant(variantCode);
        if (line.Type != DocumentLineType.Item || line.No.IsNullOrWhiteSpace())
        {
            line.SetQtyPerUnitOfMeasure(1m);
            return;
        }

        var item = await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == line.No);
        if (item == null)
        {
            return;
        }

        var units = LazyServiceProvider.LazyGetRequiredService<ItemUnitOfMeasureManager>();
        await units.EnsureVariantAsync(item.No, line.VariantCode);
        line.SetQtyPerUnitOfMeasure(await units.GetQtyPerAsync(item, line.UnitOfMeasureCode));
    }

    /// <summary>The price an item line gets when it is sent without one.</summary>
    private async Task<(decimal UnitPrice, decimal Discount)> PriceOfAsync(SalesHeader header, SalesLineInputDto line)
    {
        if (line.Type != DocumentLineType.Item || line.UnitPrice != 0m || line.No.IsNullOrWhiteSpace())
        {
            return (line.UnitPrice, line.LineDiscountPercent);
        }

        var quote = await LazyServiceProvider.LazyGetRequiredService<PriceCalculator>().FindPriceAsync(new PriceQuery
        {
            PriceType = PriceType.Sale,
            PartyNo = header.SellToCustomerNo,
            CustomerPriceGroup = header.CustomerPriceGroup,
            ItemNo = line.No,
            UnitOfMeasureCode = line.UnitOfMeasureCode,
            Quantity = line.Quantity,
            CurrencyCode = header.CurrencyCode,
            Date = header.PostingDate,
        });
        return (quote.UnitPrice, line.LineDiscountPercent != 0m ? line.LineDiscountPercent : quote.LineDiscountPercent);
    }

    /// <summary>The quantities to ship and invoice sent with the lines of an order (on a blanket order: to put on the next order), in the order the lines were sent.</summary>
    private static void ApplyQuantitiesToPost(SalesHeader header, CreateUpdateSalesHeaderDto input)
    {
        if (!header.IsOrderType && header.DocumentType != SalesDocumentType.BlanketOrder)
        {
            return;
        }

        var lines = header.Lines.OrderBy(l => l.LineNo).ToList();
        for (var i = 0; i < input.Lines.Count && i < lines.Count; i++)
        {
            var sent = input.Lines[i];
            if (sent.QtyToShip.HasValue || sent.QtyToInvoice.HasValue)
            {
                var line = (sent.Id.HasValue ? lines.FirstOrDefault(l => l.Id == sent.Id.Value) : null) ?? lines[i];
                var qtyToShip = sent.QtyToShip ?? line.QtyToShip;
                header.SetQuantitiesToPost(line.Id, qtyToShip, sent.QtyToInvoice ?? Math.Min(line.QtyToInvoice, line.QtyShippedNotInvoiced + qtyToShip));
            }
        }
    }
}
