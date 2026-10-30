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
using ABPmicroservice.Erp.Sales;
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

        // Blank takes the next number of the series set up for this document type.
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
        await UpsertLinesAsync(header, input);
        await AddAutomaticStandardLinesAsync(header);
        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(header, vendor);
        ApplyQuantitiesToPost(header, input);

        await Repository.InsertAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    public override async Task<PurchaseHeaderDto> UpdateAsync(Guid id, CreateUpdatePurchaseHeaderDto input)
    {
        await CheckUpdatePolicyAsync();

        var header = await GetEntityByIdAsync(id);

        // A released document is frozen until it is reopened.
        if (header.Status != DocumentStatus.Open)
        {
            throw new DocumentNotOpenException(header.No);
        }

        var vendor = await _vendorRepository.GetAsync(header.VendorId);
        await ApplyHeaderAsync(header, input, vendor);
        await UpsertLinesAsync(header, input);
        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(header, vendor);
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

        await LazyServiceProvider.LazyGetRequiredService<DropShipmentManager>().UnlinkAsync(header.Lines);
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

    [Authorize(ErpPermissions.PurchaseDocuments.Post)]
    public async Task<PurchasePostingResultDto> RunOrderPostingAsync(Guid id, PurchaseOrderPostingInput input)
    {
        var toPost = await GetEntityByIdAsync(id);
        if (!toPost.IsOrderType)
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNotAnOrder).WithData("documentNo", toPost.No);
        }

        // Posting releases an open document first, so the same approval rule applies.
        await _approvalsManager.EnsureCanReleaseAsync(ApprovalKind, toPost);

        foreach (var line in input.Lines)
        {
            toPost.SetQuantitiesToPost(line.LineId, line.QtyToReceive, line.QtyToInvoice);
        }

        var result = await _purchasePostingEngine.PostAsync(id, input.Receive, input.Invoice);
        return new PurchasePostingResultDto
        {
            Document = await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id)),
            InvoiceNo = result.Invoice?.No,
            ReceiptNo = result.Receipt?.No,
        };
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Create)]
    public async Task<PurchaseHeaderDto> MakeOrderAsync(Guid id, MakeOrderInput input)
    {
        var order = await Flow.MakeOrderAsync(id, input?.PostingDate ?? Clock.Now.Date);
        return await MapToGetOutputDtoAsync(order);
    }

    public async Task<List<OpenReceiptLineDto>> GetOpenReceiptLinesAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        var lines = await Flow.GetOpenReceiptLinesAsync(id);
        return lines
            .Select(x => new OpenReceiptLineDto
            {
                ReceiptLineId = x.Line.Id,
                ReceiptNo = x.Receipt.No,
                PostingDate = x.Receipt.PostingDate,
                OrderNo = x.Receipt.OrderNo,
                LineNo = x.Line.LineNo,
                Type = x.Line.Type,
                No = x.Line.No,
                Description = x.Line.Description,
                Quantity = x.Line.Quantity,
                QtyNotInvoiced = x.Line.QtyNotInvoiced,
                DirectUnitCost = x.Line.DirectUnitCost,
            })
            .ToList();
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Update)]
    public async Task<PurchaseHeaderDto> CollectReceiptLinesAsync(Guid id, GetReceiptLinesInput input)
    {
        return await MapToGetOutputDtoAsync(await Flow.GetReceiptLinesAsync(id, input.ReceiptLineIds));
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Update)]
    public async Task<PurchaseHeaderDto> CopyLinesFromAsync(Guid id, CopyDocumentLinesInput input)
    {
        return await MapToGetOutputDtoAsync(await Flow.CopyLinesAsync(id, input.SourceDocumentId));
    }

    private PurchaseDocumentFlow Flow => LazyServiceProvider.LazyGetRequiredService<PurchaseDocumentFlow>();

    [Authorize(ErpPermissions.PurchaseDocuments.Update)]
    public async Task<PurchaseHeaderDto> CloseOrderAsync(Guid id, CloseCancelPurchaseOrderInput input)
    {
        var header = await GetEntityByIdAsync(id);
        header.CloseLpo(CurrentUser.UserName ?? CurrentUser.Id?.ToString() ?? "System", input.Comments);
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Update)]
    public async Task<PurchaseHeaderDto> CancelOrderAsync(Guid id, CloseCancelPurchaseOrderInput input)
    {
        var header = await GetEntityByIdAsync(id);
        header.CancelLpo(CurrentUser.UserName ?? CurrentUser.Id?.ToString() ?? "System", input.Comments);
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Post)]
    public async Task<PurchasePostingResultDto> RunPrepaymentPostingAsync(Guid id)
    {
        // A prepayment invoice releases the order, so it passes the same approval rule as posting.
        await _approvalsManager.EnsureCanReleaseAsync(ApprovalKind, await GetEntityByIdAsync(id));

        var invoice = await _purchasePostingEngine.PostPrepaymentAsync(id, creditMemo: false);
        return new PurchasePostingResultDto
        {
            Document = await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id)),
            InvoiceNo = invoice.No,
        };
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Post)]
    [Authorize(ErpPermissions.PurchaseDocuments.Update)]
    public async Task<PurchaseHeaderDto> InsertStandardLinesAsync(Guid id, Sales.InsertStandardLinesInput input)
    {
        var header = await GetEntityByIdAsync(id);
        if (header.Status != DocumentStatus.Open)
        {
            throw new DocumentNotOpenException(header.No);
        }

        var normalized = CodeTableEntity.NormalizeCode(input.Code);
        var codes = LazyServiceProvider.LazyGetRequiredService<IRepository<StandardPurchaseCode, Guid>>();
        var found = await codes.FirstOrDefaultAsync(c => c.Code == normalized)
            ?? throw new BusinessException(ErpErrorCodes.StandardCodes.StandardCodeNotFound).WithData("code", normalized ?? "");
        await AddStandardLinesAsync(header, await codes.GetAsync(found.Id, includeDetails: true));
        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(header, await _vendorRepository.GetAsync(header.VendorId));
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    public async Task<PurchasePostingResultDto> RunPrepaymentCreditMemoPostingAsync(Guid id)
    {
        var crMemo = await _purchasePostingEngine.PostPrepaymentAsync(id, creditMemo: true);
        return new PurchasePostingResultDto
        {
            Document = await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id)),
            InvoiceNo = crMemo.No,
        };
    }

    private const ApprovalDocumentKind ApprovalKind = ApprovalDocumentKind.PurchaseDocument;

    private Guid GetUserId()
    {
        return CurrentUser.Id ?? throw new AbpAuthorizationException();
    }

    protected override async Task<PurchaseHeader> GetEntityByIdAsync(Guid id)
    {
        var header = await Repository.GetAsync(id, includeDetails: true);
        await Centers.EnsureAllowedAsync(header.No, header.ResponsibilityCenter, sales: false);
        return header;
    }

    private ResponsibilityCenterManager Centers => LazyServiceProvider.LazyGetRequiredService<ResponsibilityCenterManager>();

    /// <summary>Adds the lines of a standard code, each priced as a line typed in would be.</summary>
    private async Task AddStandardLinesAsync(PurchaseHeader header, StandardPurchaseCode code)
    {
        if (code.Lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.StandardCodes.StandardCodeHasNoLines).WithData("code", code.Code);
        }

        var items = LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>();
        foreach (var standard in code.Lines.OrderBy(l => l.LineNo))
        {
            var input = new PurchaseLineInputDto { Type = standard.Type, No = standard.No, Description = standard.Description, Quantity = standard.Quantity, DirectUnitCost = standard.UnitAmount, UnitOfMeasureCode = standard.UnitOfMeasureCode };
            var (amount, discount) = await CostOfAsync(header, input);
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

    /// <summary>A new quote, order or invoice gets the lines of the vendor's standard codes that go on automatically.</summary>
    private async Task AddAutomaticStandardLinesAsync(PurchaseHeader header)
    {
        var partyNo = header.BuyFromVendorNo;
        var assigned = (await LazyServiceProvider.LazyGetRequiredService<IRepository<StandardVendorPurchaseCode, Guid>>().GetListAsync(c => c.VendorNo == partyNo))
            .Where(c => c.InsertsAutomaticallyOn(header.DocumentType))
            .OrderBy(c => c.Code)
            .ToList();
        foreach (var standard in assigned)
        {
            var code = standard.Code;
            var standardCode = await LazyServiceProvider.LazyGetRequiredService<IRepository<StandardPurchaseCode, Guid>>().FirstOrDefaultAsync(c => c.Code == code);
            if (standardCode != null)
            {
                standardCode = await LazyServiceProvider.LazyGetRequiredService<IRepository<StandardPurchaseCode, Guid>>().GetAsync(standardCode.Id, includeDetails: true);
                if (standardCode.Lines.Count > 0)
                {
                    await AddStandardLinesAsync(header, standardCode);
                }
            }
        }
    }

    protected override async Task<IQueryable<PurchaseHeader>> CreateFilteredQueryAsync(GetPurchaseDocumentListInput input)
    {
        // Lists show headers only; lines are loaded when a document is opened.
        var query = await Repository.GetQueryableAsync();
        var center = await Centers.GetUserCenterAsync(sales: false);

        return query
            .WhereIf(
                !input.Filter.IsNullOrWhiteSpace(),
                x => x.No.Contains(input.Filter) || x.BuyFromVendorNo.Contains(input.Filter) || x.BuyFromVendorName.Contains(input.Filter)
            )
            .WhereIf(input.DocumentType.HasValue, x => x.DocumentType == input.DocumentType.Value)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(input.VendorId.HasValue, x => x.VendorId == input.VendorId.Value)
            .WhereIf(center != null, x => x.ResponsibilityCenter == center);
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
    /// follows from the payment terms.
    /// </summary>
    private async Task ApplyHeaderAsync(PurchaseHeader header, CreateUpdatePurchaseHeaderDto input, Vendor vendor)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<PaymentTerms>(input.PaymentTermsCode);
        await codes.EnsureExistsAsync<Currency>(input.CurrencyCode);
        await codes.EnsureExistsAsync<Location>(input.LocationCode);
        await codes.EnsureExistsAsync<ResponsibilityCenter>(input.ResponsibilityCenter);

        var paymentTermsCode = CodeTableEntity.NormalizeCode(input.PaymentTermsCode) ?? vendor.PaymentTermsCode;
        var dueDate = input.DueDate
            ?? await LazyServiceProvider.LazyGetRequiredService<PaymentTermsManager>().CalculateDueDateAsync(paymentTermsCode, input.PostingDate);

        header.SetDates(input.PostingDate, dueDate, input.ExpectedReceiptDate);
        header.SetCurrency(CodeTableEntity.NormalizeCode(input.CurrencyCode) ?? vendor.CurrencyCode);
        header.SetPaymentTerms(paymentTermsCode);
        header.SetLocation(input.LocationCode ?? vendor.LocationCode);
        header.SetVendorInvoiceNo(input.VendorInvoiceNo);
        header.SetYourReference(input.YourReference);
        header.SetPayTo(
            input.PayToVendorNo ?? vendor.No,
            input.PayToName ?? vendor.Name,
            input.PayToAddress ?? vendor.Address,
            input.PayToCity ?? vendor.City,
            input.PayToPostCode ?? vendor.PostCode,
            input.PayToCountryRegionCode ?? vendor.CountryRegionCode,
            input.PayToContact ?? vendor.Contact
        );
        header.SetShipTo(
            input.ShipToCode,
            input.ShipToName,
            input.ShipToAddress,
            input.ShipToCity,
            input.ShipToPostCode,
            input.ShipToCountryRegionCode,
            input.ShipToContact
        );
        header.SetShipmentMethod(input.ShipmentMethodCode ?? vendor.ShipmentMethodCode);
        header.SetPaymentMethod(input.PaymentMethodCode ?? vendor.PaymentMethodCode);
        header.SetDimensions(input.ShortcutDimension1Code, input.ShortcutDimension2Code);
        header.SetPostingGroups(
            input.VendorPostingGroup ?? vendor.VendorPostingGroup,
            input.GenBusPostingGroup ?? vendor.GenBusPostingGroup,
            input.VatBusPostingGroup ?? vendor.VatBusPostingGroup
        );
        header.SetPricesIncludingVat(input.PricesIncludingVat || vendor.PricesIncludingVAT);
        header.SetOnHold(input.OnHold);
        header.SetAppliesTo(input.AppliesToDocType, input.AppliesToDocNo, input.AppliesToId);
        header.SetTax(input.TaxAreaCode ?? vendor.TaxAreaCode, input.TaxLiable || vendor.TaxLiable);
        header.SetPrepayment(input.PrepaymentPct != 0 ? input.PrepaymentPct : vendor.PrepaymentPct);
        var center = await Centers.DefaultCenterAsync(input.ResponsibilityCenter ?? header.ResponsibilityCenter, vendor.ResponsibilityCenter, sales: false);
        header.SetAdditionalFields(
            input.PostingDescription,
            input.DocumentDate,
            input.PurchaserCode,
            input.VendorOrderNo,
            input.VendorShipmentNo,
            input.VendorCrMemoNo,
            input.BuyFromVendorName2,
            input.BuyFromAddress,
            input.BuyFromAddress2,
            input.BuyFromCity,
            input.BuyFromPostCode,
            input.BuyFromCounty,
            input.BuyFromCountryRegionCode,
            input.BuyFromContact,
            input.PayToName2,
            input.PayToAddress2,
            input.PayToCounty,
            input.ShipToName2,
            input.ShipToAddress2,
            input.ShipToCounty,
            input.OrderAddressCode,
            input.VatRegistrationNo,
            input.PaymentDiscountPct,
            input.PmtDiscountDate,
            input.VatBaseDiscountPct,
            input.LanguageCode,
            input.QuoteNo,
            input.ResponsibilityCenter,
            input.RequestedReceiptDate,
            input.PromisedReceiptDate,
            input.AssignedUserId,
            input.PaymentReference,
            input.InvoiceReceivedDate,
            input.CreditorNo,
            input.ReasonCode
        );
        header.SetProcurementDetails(
            input.RequisitionNo,
            input.TenderNo,
            input.ContractCommencementDate,
            input.ContractCompletionDate
        );
        header.SetResponsibilityCenter(center);
        await Centers.EnsureAllowedAsync(header.No, header.ResponsibilityCenter, sales: false);
    }

    /// <summary>
    /// The client always sends the whole document. Lines it sends back with their id are changed in
    /// place, so what has been received and invoiced on them is kept; lines it leaves out are
    /// removed, and new lines are added. An item line without a cost takes the price list's.
    /// </summary>
    private async Task UpsertLinesAsync(PurchaseHeader header, CreateUpdatePurchaseHeaderDto input)
    {
        var sent = input.Lines.Where(l => l.Id.HasValue).Select(l => l.Id!.Value).ToHashSet();
        var removedLines = header.Lines.Where(l => !sent.Contains(l.Id)).ToList();
        await LazyServiceProvider.LazyGetRequiredService<DropShipmentManager>().UnlinkAsync(removedLines);
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
            var (directUnitCost, discount) = await CostOfAsync(header, lineInput);
            var existing = lineInput.Id.HasValue ? header.Lines.FirstOrDefault(l => l.Id == lineInput.Id.Value) : null;
            var line = existing != null
                ? header.UpdateLine(existing.Id, lineInput.Type, lineInput.No, lineInput.Description, lineInput.Quantity, directUnitCost, discount, lineInput.UnitOfMeasureCode)
                : header.AddLine(GuidGenerator.Create(), lineInput.Type, lineInput.No, lineInput.Description, lineInput.Quantity, directUnitCost, discount, lineInput.UnitOfMeasureCode);

            if (!string.IsNullOrWhiteSpace(lineInput.LocationCode))
            {
                line.SetLocation(lineInput.LocationCode);
            }
            if (lineInput.ExpectedReceiptDate.HasValue)
            {
                line.SetExpectedReceiptDate(lineInput.ExpectedReceiptDate);
            }
            if (!string.IsNullOrWhiteSpace(lineInput.ItemCategoryCode))
            {
                line.SetItemCategory(lineInput.ItemCategoryCode);
            }
            if (!string.IsNullOrWhiteSpace(lineInput.ShortcutDimension1Code) || !string.IsNullOrWhiteSpace(lineInput.ShortcutDimension2Code))
            {
                line.SetDimensions(lineInput.ShortcutDimension1Code, lineInput.ShortcutDimension2Code);
            }
            if (!string.IsNullOrWhiteSpace(lineInput.GenBusPostingGroup) || !string.IsNullOrWhiteSpace(lineInput.GenProdPostingGroup))
            {
                line.SetPostingGroups(lineInput.GenBusPostingGroup, lineInput.GenProdPostingGroup);
            }
            if (!string.IsNullOrWhiteSpace(lineInput.TaxAreaCode) || !string.IsNullOrWhiteSpace(lineInput.TaxGroupCode) || lineInput.TaxLiable)
            {
                line.SetTax(lineInput.TaxAreaCode, lineInput.TaxLiable, lineInput.TaxGroupCode);
            }
            if (!string.IsNullOrWhiteSpace(lineInput.DeferralCode))
            {
                line.SetDeferralCode(lineInput.DeferralCode);
            }
            // Once a prepayment is invoiced the line keeps its percentage; a credit memo frees it again.
            if (lineInput.PrepaymentPct.HasValue && line.PrepmtAmtInv == 0m)
            {
                line.SetPrepaymentPct(lineInput.PrepaymentPct.Value);
            }
            await SetVariantAndUnitAsync(line, lineInput.VariantCode);
        }
        header.RecalculateTotals();
    }

    /// <summary>The variant of an item line and how many base units one unit of its unit of measure holds.</summary>
    private async Task SetVariantAndUnitAsync(PurchaseLine line, string variantCode)
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

    /// <summary>The cost an item line gets when it is sent without one.</summary>
    private async Task<(decimal DirectUnitCost, decimal Discount)> CostOfAsync(PurchaseHeader header, PurchaseLineInputDto line)
    {
        if (line.Type != DocumentLineType.Item || line.DirectUnitCost != 0m || line.No.IsNullOrWhiteSpace())
        {
            return (line.DirectUnitCost, line.LineDiscountPercent);
        }

        var quote = await LazyServiceProvider.LazyGetRequiredService<PriceCalculator>().FindPriceAsync(new PriceQuery
        {
            PriceType = PriceType.Purchase,
            PartyNo = header.BuyFromVendorNo,
            ItemNo = line.No,
            UnitOfMeasureCode = line.UnitOfMeasureCode,
            Quantity = line.Quantity,
            CurrencyCode = header.CurrencyCode,
            Date = header.PostingDate,
        });
        return (quote.UnitPrice, line.LineDiscountPercent != 0m ? line.LineDiscountPercent : quote.LineDiscountPercent);
    }

    /// <summary>The quantities to receive and invoice sent with the lines of an order, in the order the lines were sent.</summary>
    private static void ApplyQuantitiesToPost(PurchaseHeader header, CreateUpdatePurchaseHeaderDto input)
    {
        if (!header.IsOrderType && header.DocumentType != PurchaseDocumentType.BlanketOrder)
        {
            return;
        }

        var lines = header.Lines.OrderBy(l => l.LineNo).ToList();
        for (var i = 0; i < input.Lines.Count && i < lines.Count; i++)
        {
            var sent = input.Lines[i];
            if (sent.QtyToReceive.HasValue || sent.QtyToInvoice.HasValue)
            {
                var line = (sent.Id.HasValue ? lines.FirstOrDefault(l => l.Id == sent.Id.Value) : null) ?? lines[i];
                var qtyToReceive = sent.QtyToReceive ?? line.QtyToReceive;
                header.SetQuantitiesToPost(line.Id, qtyToReceive, sent.QtyToInvoice ?? Math.Min(line.QtyToInvoice, line.QtyReceivedNotInvoiced + qtyToReceive));
            }
        }
    }
}
