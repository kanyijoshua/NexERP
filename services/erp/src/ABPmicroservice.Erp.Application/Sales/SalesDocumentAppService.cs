using System;
using ABPmicroservice.Erp.Purchasing;
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
        await AddAutomaticStandardLinesAsync(header);
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

        EnsureNotOnPurchaseOrder(header, header.Lines);
        await LazyServiceProvider.LazyGetRequiredService<ReservationManager>().CancelAsync(header.Lines.Select(l => l.Id));
        await Repository.DeleteAsync(header, autoSave: true);
    }

    [Authorize(ErpPermissions.SalesDocuments.Update)]
    public async Task<SalesHeaderDto> InsertStandardLinesAsync(Guid id, InsertStandardLinesInput input)
    {
        var header = await GetEntityByIdAsync(id);
        if (header.Status != DocumentStatus.Open)
        {
            throw new DocumentNotOpenException(header.No);
        }

        var code = await StandardCodeAsync<StandardSalesCode>(input.Code);
        await AddStandardLinesAsync(header, code);
        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(header, await _customerRepository.GetAsync(header.CustomerId));
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    private async Task<T> StandardCodeAsync<T>(string code)
        where T : CodeTableEntity
    {
        var normalized = CodeTableEntity.NormalizeCode(code);
        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<T, Guid>>();
        var found = await repository.FirstOrDefaultAsync(c => c.Code == normalized)
            ?? throw new BusinessException(ErpErrorCodes.StandardCodes.StandardCodeNotFound).WithData("code", normalized ?? "");
        return await repository.GetAsync(found.Id, includeDetails: true);
    }

    [Authorize(ErpPermissions.SalesDocuments.Update)]
    public async Task<DropShipmentOrderResultDto> MakeDropShipmentOrderAsync(Guid id, MakeDropShipmentOrderInput input)
    {
        var order = await LazyServiceProvider.LazyGetRequiredService<DropShipmentManager>()
            .CreatePurchaseOrderAsync(id, input.VendorNo, input.OrderDate ?? Clock.Now.Date);
        return new DropShipmentOrderResultDto { PurchaseOrderId = order.Id, PurchaseOrderNo = order.No, LineCount = order.Lines.Count };
    }

    /// <summary>A line ordered from a vendor as a drop shipment stays until its purchase line is removed.</summary>
    private static void EnsureNotOnPurchaseOrder(SalesHeader header, IEnumerable<SalesLine> lines)
    {
        var linked = lines.FirstOrDefault(l => l.PurchaseOrderLineId.HasValue);
        if (linked != null)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.DropShipmentLinked)
                .WithData("documentNo", header.No)
                .WithData("lineNo", linked.LineNo)
                .WithData("purchaseOrderNo", linked.PurchaseOrderNo ?? "");
        }
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

    [Authorize(ErpPermissions.SalesDocuments.Post)]
    public async Task<SalesPostingResultDto> RunPrepaymentPostingAsync(Guid id)
    {
        // A prepayment invoice releases the order, so it passes the same checks as posting.
        var toPost = await GetEntityByIdAsync(id);
        await _approvalsManager.EnsureCanReleaseAsync(ApprovalKind, toPost);
        if (toPost.Status == DocumentStatus.Open)
        {
            await EnsureWithinCreditLimitAsync(toPost);
        }

        var invoice = await _salesPostingEngine.PostPrepaymentAsync(id, creditMemo: false);
        return new SalesPostingResultDto
        {
            Document = await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id)),
            InvoiceNo = invoice.No,
        };
    }

    [Authorize(ErpPermissions.SalesDocuments.Post)]
    public async Task<SalesPostingResultDto> RunPrepaymentCreditMemoPostingAsync(Guid id)
    {
        var crMemo = await _salesPostingEngine.PostPrepaymentAsync(id, creditMemo: true);
        return new SalesPostingResultDto
        {
            Document = await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id)),
            InvoiceNo = crMemo.No,
        };
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
        var header = await Repository.GetAsync(id, includeDetails: true);
        await Centers.EnsureAllowedAsync(header.No, header.ResponsibilityCenter, sales: true);
        return header;
    }

    private ResponsibilityCenterManager Centers => LazyServiceProvider.LazyGetRequiredService<ResponsibilityCenterManager>();

    /// <summary>Adds the lines of a standard code, each priced as a line typed in would be.</summary>
    private async Task AddStandardLinesAsync(SalesHeader header, StandardSalesCode code)
    {
        if (code.Lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.StandardCodes.StandardCodeHasNoLines).WithData("code", code.Code);
        }

        var items = LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>();
        foreach (var standard in code.Lines.OrderBy(l => l.LineNo))
        {
            var input = new SalesLineInputDto { Type = standard.Type, No = standard.No, Description = standard.Description, Quantity = standard.Quantity, UnitPrice = standard.UnitAmount, UnitOfMeasureCode = standard.UnitOfMeasureCode };
            var (amount, discount) = await PriceOfAsync(header, input);
            var description = standard.Description;
            if (description == null && standard.Type == DocumentLineType.Item && standard.No != null)
            {
                description = (await items.FirstOrDefaultAsync(i => i.No == standard.No))?.Description;
            }

            var line = header.AddLine(GuidGenerator.Create(), standard.Type, standard.No, description, standard.Quantity, amount, discount, standard.UnitOfMeasureCode);
            await SetVariantAndUnitAsync(line, null);
        }

        header.RecalculateTotals();
    }

    /// <summary>A new quote, order or invoice gets the lines of the customer's standard codes that go on automatically.</summary>
    private async Task AddAutomaticStandardLinesAsync(SalesHeader header)
    {
        var partyNo = header.SellToCustomerNo;
        var assigned = (await LazyServiceProvider.LazyGetRequiredService<IRepository<StandardCustomerSalesCode, Guid>>().GetListAsync(c => c.CustomerNo == partyNo))
            .Where(c => c.InsertsAutomaticallyOn(header.DocumentType))
            .OrderBy(c => c.Code)
            .ToList();
        foreach (var standard in assigned)
        {
            var code = standard.Code;
            var standardCode = await LazyServiceProvider.LazyGetRequiredService<IRepository<StandardSalesCode, Guid>>().FirstOrDefaultAsync(c => c.Code == code);
            if (standardCode != null)
            {
                standardCode = await LazyServiceProvider.LazyGetRequiredService<IRepository<StandardSalesCode, Guid>>().GetAsync(standardCode.Id, includeDetails: true);
                if (standardCode.Lines.Count > 0)
                {
                    await AddStandardLinesAsync(header, standardCode);
                }
            }
        }
    }

    protected override async Task<IQueryable<SalesHeader>> CreateFilteredQueryAsync(GetSalesDocumentListInput input)
    {
        // Lists show headers only; lines are loaded when a document is opened.
        var query = await Repository.GetQueryableAsync();
        var center = await Centers.GetUserCenterAsync(sales: true);

        return query
            .WhereIf(
                !input.Filter.IsNullOrWhiteSpace(),
                x => x.No.Contains(input.Filter) || x.SellToCustomerNo.Contains(input.Filter) || x.SellToCustomerName.Contains(input.Filter)
            )
            .WhereIf(input.DocumentType.HasValue, x => x.DocumentType == input.DocumentType.Value)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId.Value)
            .WhereIf(center != null, x => x.ResponsibilityCenter == center);
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
        await codes.EnsureExistsAsync<ResponsibilityCenter>(input.ResponsibilityCenter);

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
        header.SetPrepayment(input.PrepaymentPct != 0m ? input.PrepaymentPct : customer.PrepaymentPct);
        header.SetResponsibilityCenter(await Centers.DefaultCenterAsync(input.ResponsibilityCenter ?? header.ResponsibilityCenter, customer.ResponsibilityCenter, sales: true));
        await Centers.EnsureAllowedAsync(header.No, header.ResponsibilityCenter, sales: true);
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
        var removedLines = header.Lines.Where(l => !sent.Contains(l.Id)).ToList();
        EnsureNotOnPurchaseOrder(header, removedLines);
        await LazyServiceProvider.LazyGetRequiredService<ReservationManager>().CancelAsync(removedLines.Select(l => l.Id));
        foreach (var removed in removedLines.Select(l => l.Id))
        {
            header.RemoveLine(removed);
        }

        foreach (var lineInput in input.Lines)
        {
            if (lineInput.Type == DocumentLineType.ChargeItem)
            {
                await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<ItemCharge>(lineInput.No);
            }
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
            line.SetDropShipment(header.No, header.DocumentType == SalesDocumentType.Order && lineInput.DropShipment);
            // Once a prepayment is invoiced the line keeps its percentage; a credit memo frees it again.
            if (lineInput.PrepaymentPct.HasValue && line.PrepmtAmtInv == 0m)
            {
                line.SetPrepaymentPct(lineInput.PrepaymentPct.Value);
            }
        }
        header.RecalculateTotals();
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
