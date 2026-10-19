using ABPmicroservice.Erp.Attachments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Numbering;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Sales;

/// <summary>What one posting run of a sales document produced.</summary>
public sealed class SalesPostingResult
{
    /// <summary>The posted invoice or credit memo; null when the run only shipped.</summary>
    public PostedSalesHeader Invoice { get; init; }

    /// <summary>The posted shipment or return receipt; null for an invoice or a run that only invoiced.</summary>
    public PostedSalesShipment Shipment { get; init; }
}

/// <summary>
/// Sales Document Posting Engine.
///
/// Ships and invoices sales documents: Item Ledger Entries and Posted Sales Shipments for what
/// leaves, and Posted Sales Invoices, G/L Entries, Customer Ledger Entries and VAT Entries for what
/// is invoiced.
/// <para>
/// An invoice or credit memo ships and invoices everything in one run. An order is shipped and
/// invoiced in parts, as its lines' quantities to ship and to invoice say; a return order does the
/// same with goods coming back and a credit memo. A quote or blanket order is never posted.
/// </para>
/// <para>
/// Shipping moves the stock at expected cost and posts nothing to the G/L. Invoicing debits the
/// customer with the amount including VAT, credits revenue and output VAT, turns the shipped
/// items' expected cost into actual cost, and (with automatic cost posting) moves that cost from
/// inventory to COGS. A credit memo or return order posts the same with every sign turned round.
/// </para>
/// </summary>
public class SalesPostingEngine : DomainService
{
    /// <summary>Stamped on every entry the engine posts.</summary>
    private const string SourceCode = "SALES";

    private readonly IRepository<SalesHeader, Guid> _salesHeaderRepository;
    private readonly IRepository<PostedSalesHeader, Guid> _postedSalesHeaderRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly PostingSetupManager _postingSetupManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly ItemJnlPostLine _itemJnlPostLine;
    private readonly SalesReceivablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly GLRegisterManager _registerManager;

    private CurrencyExchangeRateManager CurrencyManager => LazyServiceProvider.LazyGetRequiredService<CurrencyExchangeRateManager>();
    private InventorySetupManager InventorySetupManager => LazyServiceProvider.LazyGetRequiredService<InventorySetupManager>();
    private IRepository<PostedSalesShipment, Guid> ShipmentRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<PostedSalesShipment, Guid>>();
    private IRepository<PostedSalesShipmentLine, Guid> ShipmentLineRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<PostedSalesShipmentLine, Guid>>();
    private IRepository<ItemLedgerEntry, Guid> ItemLedgerEntryRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<ItemLedgerEntry, Guid>>();
    private ItemTrackingManager TrackingManager => LazyServiceProvider.LazyGetRequiredService<ItemTrackingManager>();
    private DeferralPoster DeferralPoster => LazyServiceProvider.LazyGetRequiredService<DeferralPoster>();
    private FixedAssets.FAJnlPostLine FAPostLine => LazyServiceProvider.LazyGetRequiredService<FixedAssets.FAJnlPostLine>();

    public SalesPostingEngine(
        IRepository<SalesHeader, Guid> salesHeaderRepository,
        IRepository<PostedSalesHeader, Guid> postedSalesHeaderRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Item, Guid> itemRepository,
        PostingSetupManager postingSetupManager,
        GeneralLedgerSetupManager glSetupManager,
        GenJnlPostLine genJnlPostLine,
        ItemJnlPostLine itemJnlPostLine,
        SalesReceivablesSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        GLRegisterManager registerManager
    )
    {
        _registerManager = registerManager;
        _salesHeaderRepository = salesHeaderRepository;
        _postedSalesHeaderRepository = postedSalesHeaderRepository;
        _customerRepository = customerRepository;
        _itemRepository = itemRepository;
        _postingSetupManager = postingSetupManager;
        _glSetupManager = glSetupManager;
        _genJnlPostLine = genJnlPostLine;
        _itemJnlPostLine = itemJnlPostLine;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
    }

    /// <summary>
    /// Posts a sales document. <paramref name="ship"/> and <paramref name="invoice"/> only matter for
    /// an order or return order; any other document is always shipped and invoiced in full.
    /// </summary>
    public async Task<SalesPostingResult> PostAsync(Guid salesHeaderId, bool ship = true, bool invoice = true)
    {
        var header = await _salesHeaderRepository.GetAsync(salesHeaderId);
        if (header.Posted)
        {
            throw new DocumentAlreadyPostedException(header.No);
        }
        if (header.DocumentType is SalesDocumentType.Quote or SalesDocumentType.BlanketOrder)
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentTypeCannotBePosted)
                .WithData("documentType", header.DocumentType.ToString())
                .WithData("documentNo", header.No);
        }
        if (!header.Lines.Any())
        {
            throw new UserFriendlyException($"Sales document '{header.No}' has no lines.");
        }

        if (!header.IsOrderType)
        {
            ship = true;
            invoice = true;
            foreach (var line in header.Lines)
            {
                line.ResetQuantitiesToPost();
            }
        }

        // The quantities may have been set before the last line change; check them again.
        foreach (var line in header.Lines)
        {
            line.SetQuantitiesToPost(header.No, line.QtyToShip, line.QtyToInvoice);
        }

        var toShip = ship ? header.Lines.Where(l => !l.IsFromShipment && l.QtyToShip != 0m).ToList() : [];
        var toInvoice = invoice ? header.Lines.Where(l => l.QtyToInvoice != 0m).ToList() : [];
        if (toShip.Count == 0 && toInvoice.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Documents.NothingToPost).WithData("documentNo", header.No);
        }

        await _glSetupManager.CheckPostingDateAsync(header.PostingDate);

        var salesSetup = await _setupManager.GetAsync();
        if (toInvoice.Count > 0 && salesSetup.ExtDocNoMandatory && header.ExternalDocumentNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Sales.ExternalDocumentNoRequired).WithData("documentNo", header.No);
        }

        // Posting releases an open document first.
        if (header.Status == DocumentStatus.Open)
        {
            header.Release();
        }

        var isReturn = header.IsReturn;
        // Everything is worked out in invoice terms and turned round once for a credit memo.
        var sign = isReturn ? -1m : 1m;
        var documentType = isReturn ? GLEntryDocumentType.CreditMemo : GLEntryDocumentType.Invoice;
        var customer = await _customerRepository.GetAsync(header.CustomerId);

        // Every account is resolved before anything is written, so a missing posting setup
        // stops the document instead of leaving half of it posted.
        var plans = await PlanLinesAsync(header, customer, toShip.Union(toInvoice).ToList());

        // A foreign currency document posts at its posting date's rate: the customer entry keeps
        // the currency, the G/L gets LCY, and the customer's LCY is the sum of the converted G/L
        // lines so rounding cannot unbalance the document.
        var currencyCode = await CurrencyManager.NormalizeAsync(header.CurrencyCode);
        var factor = await CurrencyManager.GetCurrencyFactorAsync(currencyCode, header.PostingDate);
        decimal Lcy(decimal amount) => CurrencyExchangeRateManager.ToLcy(amount, factor);

        // Posted documents get their own number from the posted series; the derived number is the
        // fallback for a company that has not set one up.
        string postedDocNo = null;
        if (toInvoice.Count > 0)
        {
            var postedNos = salesSetup.GetPostedDocumentNos(header.DocumentType);
            postedDocNo = postedNos.IsNullOrWhiteSpace()
                ? await DerivedNoAsync(isReturn ? "PSCM" : "PSI", header, _postedSalesHeaderRepository, x => x.PreAssignedNo == header.No)
                : await _noSeriesManager.GetNextNoAsync(postedNos, header.PostingDate);
        }

        string shipmentNo = null;
        if (toShip.Count > 0 && header.IsOrderType)
        {
            var shipmentNos = salesSetup.GetShipmentNos(isReturn);
            shipmentNo = shipmentNos.IsNullOrWhiteSpace()
                ? await DerivedNoAsync(isReturn ? "SRR" : "SSH", header, ShipmentRepository, x => x.OrderNo == header.No)
                : await _noSeriesManager.GetNextNoAsync(shipmentNos, header.PostingDate);
        }

        // Everything this run posts belongs to one register, so it can be navigated and reversed
        // as a unit, exactly like a journal.
        var register = await _registerManager.OpenAsync(header.PostingDate, SourceCode, postedDocNo ?? shipmentNo);
        var context = new GLPostingContext(register, SourceCode);

        // 1. Ship: the stock leaves (or, on a return, comes back) at expected cost.
        var shipped = await ShipAsync(header, toShip, plans, shipmentNo ?? postedDocNo, shipmentNo, isReturn);

        // 2. Invoice: the customer, revenue, VAT and the cost of what was shipped.
        PostedSalesHeader postedHeader = null;
        if (toInvoice.Count > 0)
        {
            postedHeader = await InvoiceAsync(header, toInvoice, plans, shipped, customer, postedDocNo, currencyCode, factor, sign, documentType, context);
        }

        await _registerManager.CloseAsync(register);

        // 3. Move the document on: posted once nothing is left to invoice.
        header.RecordPosting(shipped.Shipment?.No, postedDocNo);
        await _salesHeaderRepository.UpdateAsync(header);

        if (postedHeader != null)
        {
            // Attachments marked to flow follow the document onto the posted document.
            await LazyServiceProvider
                .LazyGetRequiredService<DocumentAttachmentManager>()
                .FlowToPostedDocumentAsync(nameof(SalesHeader), header.Id, nameof(PostedSalesHeader), postedHeader.Id, postedHeader.No, purchase: false);
        }

        return new SalesPostingResult { Invoice = postedHeader, Shipment = shipped.Shipment };
    }

    /// <summary>
    /// Undoes a posted shipment (or return receipt) line that has not been invoiced: the stock moves
    /// back, the line is marked as a correction, and the order line can be shipped again.
    /// </summary>
    public async Task UndoShipmentLineAsync(Guid shipmentLineId)
    {
        var shipmentLine = await ShipmentLineRepository.GetAsync(shipmentLineId);
        var shipment = await ShipmentRepository.GetAsync(shipmentLine.PostedSalesShipmentId);
        if (shipmentLine.Correction || shipmentLine.QuantityInvoiced != 0m)
        {
            throw new BusinessException(ErpErrorCodes.Documents.CannotUndoInvoicedLine)
                .WithData("documentNo", shipment.No)
                .WithData("lineNo", shipmentLine.LineNo);
        }

        await _glSetupManager.CheckPostingDateAsync(shipment.PostingDate);

        foreach (var entry in await EntriesOfAsync(shipmentLine.Id))
        {
            // The mirror movement at the same cost and lot, taken from the very entry it undoes;
            // both are invoiced at once so neither is left waiting for an invoice.
            var mirror = await _itemJnlPostLine.PostAsync(new ItemPostingRequest
            {
                ItemId = entry.ItemId,
                ItemNo = entry.ItemNo,
                PostingDate = shipment.PostingDate,
                EntryType = entry.EntryType,
                DocumentNo = shipment.No,
                Description = shipmentLine.Description,
                Quantity = Math.Abs(entry.Quantity),
                UnitCost = entry.UnitCost,
                LocationCode = entry.LocationCode,
                VariantCode = entry.VariantCode,
                Tracking = TrackingOf(entry),
                Correction = !shipment.Return,
                AppliesToEntryId = entry.Positive ? entry.Id : null,
                ExpectedCost = true,
            });
            await _itemJnlPostLine.PostInvoicedCostAsync(entry, Math.Abs(entry.Quantity), Math.Abs(entry.Quantity) * entry.UnitCost, shipment.No, shipment.PostingDate);
            await _itemJnlPostLine.PostInvoicedCostAsync(mirror, Math.Abs(mirror.Quantity), Math.Abs(mirror.Quantity) * mirror.UnitCost, shipment.No, shipment.PostingDate);
        }

        shipmentLine.MarkUndone();
        await ShipmentLineRepository.UpdateAsync(shipmentLine);

        var order = await FindOrderOfLineAsync(shipmentLine.OrderLineId);
        if (order != null)
        {
            order.Lines.First(l => l.Id == shipmentLine.OrderLineId).RecordShipmentUndone(shipmentLine.Quantity);
            await _salesHeaderRepository.UpdateAsync(order);
        }
    }

    private sealed class ShippedParts
    {
        public PostedSalesShipment Shipment { get; init; }

        /// <summary>The item ledger entries each line shipped in this run, by line id.</summary>
        public Dictionary<Guid, (List<ItemLedgerEntry> Entries, PostedSalesShipmentLine ShipmentLine)> Lines { get; } = new();
    }

    private async Task<ShippedParts> ShipAsync(
        SalesHeader header,
        List<SalesLine> toShip,
        Dictionary<Guid, LinePlan> plans,
        string entryDocumentNo,
        string shipmentNo,
        bool isReturn
    )
    {
        var shipment = shipmentNo == null ? null : new PostedSalesShipment(GuidGenerator.Create(), shipmentNo, isReturn, header);
        var parts = new ShippedParts { Shipment = shipment };

        foreach (var line in toShip)
        {
            var locationCode = line.LocationCode ?? header.LocationCode;
            var shipmentLineId = shipment != null && !line.No.IsNullOrWhiteSpace() ? GuidGenerator.Create() : (Guid?)null;
            var entries = new List<ItemLedgerEntry>();
            if (plans.TryGetValue(line.Id, out var plan) && plan.Item != null)
            {
                var baseQty = line.QtyToShip * line.QtyPerUnitOfMeasure;
                entries = await _itemJnlPostLine.PostAllAsync(new ItemPostingRequest
                {
                    ItemId = plan.Item.Id,
                    ItemNo = line.No,
                    PostingDate = header.PostingDate,
                    EntryType = ItemLedgerEntryType.Sale,
                    DocumentNo = entryDocumentNo,
                    Description = line.Description,
                    Quantity = baseQty,
                    UnitCost = plan.Item.UnitCost,
                    LocationCode = locationCode,
                    VariantCode = line.VariantCode,
                    Tracking = await TrackingManager.TakeAsync(plan.Item, ItemTrackingSourceType.SalesLine, line.Id, baseQty),
                    DocumentLineId = shipmentLineId ?? line.Id,
                    Correction = isReturn,
                    ExpectedCost = true,
                });
            }

            PostedSalesShipmentLine shipmentLine = null;
            if (shipmentLineId.HasValue)
            {
                shipmentLine = new PostedSalesShipmentLine(shipmentLineId.Value, shipment.Id, line, line.QtyToShip, locationCode, entries.FirstOrDefault()?.Id);
                shipment.Lines.Add(shipmentLine);
            }

            parts.Lines[line.Id] = (entries, shipmentLine);
            line.RecordShipped(line.QtyToShip);
        }

        if (shipment != null)
        {
            await ShipmentRepository.InsertAsync(shipment);
        }

        return parts;
    }

    private async Task<PostedSalesHeader> InvoiceAsync(
        SalesHeader header,
        List<SalesLine> toInvoice,
        Dictionary<Guid, LinePlan> plans,
        ShippedParts shipped,
        Customer customer,
        string postedDocNo,
        string currencyCode,
        decimal factor,
        decimal sign,
        GLEntryDocumentType documentType,
        GLPostingContext context
    )
    {
        decimal Lcy(decimal amount) => CurrencyExchangeRateManager.ToLcy(amount, factor);

        // The share of each line this invoice covers, with VAT worked out on that share.
        var invoiced = toInvoice
            .Select(line =>
            {
                var amount = line.AmountToInvoice(line.QtyToInvoice);
                return (Line: line, Quantity: line.QtyToInvoice, Amount: amount, Vat: LineVat.Calculate(amount, line.VatCalculationType, line.VatPercent));
            })
            .ToList();
        var posting = invoiced.Where(i => plans.ContainsKey(i.Line.Id)).ToList();

        var totalAmount = posting.Sum(i => i.Amount);
        var totalAmountIncludingVat = posting.Sum(i => i.Vat.AmountIncludingVat);
        var customerLcy = posting.Sum(i =>
            Lcy(Revenue(i.Line, i.Amount)) + (plans[i.Line.Id].VatSetup == null ? 0m : Lcy(OutputVat(i.Line, i.Vat))));

        // 1. Create Posted Sales Invoice
        var postedHeader = new PostedSalesHeader(
            GuidGenerator.Create(),
            postedDocNo,
            header.No,
            header.CustomerId,
            header.SellToCustomerNo,
            header.SellToCustomerName,
            header.PostingDate,
            header.DueDate ?? header.PostingDate.AddDays(ErpDomainConsts.DefaultPaymentDueDays),
            totalAmount,
            totalAmountIncludingVat
        );
        postedHeader.SetDetails(header.IsReturn, header.IsOrderType ? header.No : null, currencyCode, header.ExternalDocumentNo);

        foreach (var (line, quantity, amount, vat) in invoiced.Where(i => !i.Line.No.IsNullOrWhiteSpace()))
        {
            var postedLine = new PostedSalesLine(
                GuidGenerator.Create(),
                postedHeader.Id,
                line.LineNo,
                line.Type.ToString(),
                line.No,
                line.Description,
                quantity,
                line.UnitPrice,
                amount
            );
            postedLine.SetDetails(line.LineDiscountPercent, line.UnitOfMeasureCode, line.VatPercent, vat.Amount, vat.AmountIncludingVat);
            postedLine.SetSource(header.IsOrderType ? line.LineNo : null, line.ShipmentNo, line.ShipmentLineNo);
            postedHeader.Lines.Add(postedLine);
        }

        await _postedSalesHeaderRepository.InsertAsync(postedHeader);

        // 2. Post Customer Ledger Entry (A/R) and the receivables control account with it
        var customerLine = new GenJournalLine(
                GuidGenerator.Create(),
                Guid.Empty,
                1,
                header.PostingDate,
                documentType,
                postedDocNo,
                GenJournalAccountType.Customer,
                header.SellToCustomerNo,
                $"{(header.IsReturn ? "Sales Credit Memo" : "Sales Invoice")} {postedDocNo}",
                sign * totalAmountIncludingVat
            );
        if (currencyCode != null)
        {
            customerLine.SetCurrency(currencyCode, factor);
            customerLine.SetAmountLcy(sign * customerLcy);
        }

        await _genJnlPostLine.PostLineAsync(customerLine, context, header.DueDate);

        // 3. Per line: revenue, VAT and cost of goods sold
        foreach (var (line, quantity, amount, vat) in posting)
        {
            var plan = plans[line.Id];

            // Revenue (a credit on an invoice) is the line amount; a Full VAT line is all VAT.
            var revenue = Lcy(Revenue(line, amount));
            if (plan.FixedAsset)
            {
                await FAPostLine.PostAsync(
                    new FixedAssets.FAPostingRequest
                    {
                        FANo = line.No,
                        PostingType = FixedAssets.FAJournalPostingType.Disposal,
                        PostingDate = header.PostingDate,
                        DocumentType = documentType,
                        DocumentNo = postedDocNo,
                        Description = line.Description,
                        Amount = revenue,
                        SourceNo = header.SellToCustomerNo,
                    },
                    context
                );
            }
            else if (revenue != 0m)
            {
                await _genJnlPostLine.PostGLDirectAsync(
                    plan.RevenueAccountNo,
                    header.PostingDate,
                    documentType,
                    postedDocNo,
                    line.Description,
                    -sign * revenue,
                    header.SellToCustomerNo,
                    context: context
                );

                if (line.DeferralCode != null)
                {
                    await DeferralPoster.PostAsync(line.DeferralCode, DeferralSourceType.Sales, plan.RevenueAccountNo, -sign * revenue, header.PostingDate, documentType, postedDocNo, line.LineNo, line.Description, header.SellToCustomerNo, Guid.Empty, context);
                }
            }

            if (line.ProjectNo != null)
            {
                await LazyServiceProvider.LazyGetRequiredService<Projects.ProjectManager>().PostDocumentLineAsync(
                    line.ProjectNo, line.ProjectTaskNo, Projects.ProjectLedgerEntryType.Sale, Projects.ProjectLineType.GLAccount, line.No, line.Description,
                    sign * quantity, sign * revenue, header.PostingDate, postedDocNo, context);
            }

            if (plan.VatSetup != null)
            {
                // Output VAT is owed on a sale; under reverse charge the customer accounts for it.
                await _genJnlPostLine.PostVatAsync(
                    plan.VatSetup,
                    VatEntryType.Sale,
                    -sign * Lcy(vat.Base),
                    -sign * Lcy(OutputVat(line, vat)),
                    header.PostingDate,
                    header.PostingDate,
                    documentType,
                    postedDocNo,
                    header.SellToCustomerNo,
                    line.Description,
                    context
                );
            }

            if (plan.Item != null)
            {
                var cost = await InvoiceShippedCostAsync(header, line, quantity, shipped, postedDocNo);
                if (plan.PostCost && cost != 0m)
                {
                    // The goods leave inventory at cost and become cost of goods sold.
                    await _genJnlPostLine.PostGLDirectAsync(
                        plan.CogsAccountNo,
                        header.PostingDate,
                        documentType,
                        postedDocNo,
                        line.Description,
                        sign * cost,
                        header.SellToCustomerNo,
                        context: context
                    );
                    await _genJnlPostLine.PostGLDirectAsync(
                        plan.InventoryAccountNo,
                        header.PostingDate,
                        documentType,
                        postedDocNo,
                        line.Description,
                        -sign * cost,
                        header.SellToCustomerNo,
                        context: context
                    );
                }
            }
        }

        foreach (var (line, quantity, _, _) in invoiced)
        {
            line.RecordInvoiced(quantity);
        }

        // A line taken from a shipment invoices that shipment's order line too.
        foreach (var group in invoiced.Where(i => i.Line.IsFromShipment).GroupBy(i => i.Line.ShipmentLineId!.Value))
        {
            var shipmentLine = await ShipmentLineRepository.GetAsync(group.Key);
            var order = await FindOrderOfLineAsync(shipmentLine.OrderLineId);
            if (order != null && order.Id != header.Id)
            {
                order.Lines.First(l => l.Id == shipmentLine.OrderLineId).RecordInvoiced(group.Sum(i => i.Quantity));
                order.RecordPosting(null, postedDocNo);
                await _salesHeaderRepository.UpdateAsync(order);
            }
        }

        return postedHeader;
    }

    /// <summary>
    /// Turns the expected cost of the shipped goods this invoice covers into actual cost and returns
    /// it. An order line invoices its earlier shipments first, then what this run shipped; a line
    /// taken from a shipment invoices that shipment line.
    /// </summary>
    private async Task<decimal> InvoiceShippedCostAsync(SalesHeader header, SalesLine line, decimal quantity, ShippedParts shipped, string postedDocNo)
    {
        var remaining = quantity;
        var cost = 0m;

        var thisRun = shipped.Lines.GetValueOrDefault(line.Id);
        List<PostedSalesShipmentLine> earlierLines;
        if (line.IsFromShipment)
        {
            earlierLines = [await ShipmentLineRepository.GetAsync(line.ShipmentLineId!.Value)];
        }
        else
        {
            earlierLines = (await ShipmentLineRepository.GetListAsync(l => l.OrderLineId == line.Id && !l.Correction))
                .Where(l => l.QtyNotInvoiced > 0m && l.Id != thisRun.ShipmentLine?.Id)
                .OrderBy(l => l.Id)
                .ToList();
        }

        foreach (var shipmentLine in earlierLines)
        {
            if (remaining == 0m)
            {
                break;
            }
            // A line taken from a posted shipment may not invoice more than is left on it.
            if (shipmentLine.QtyNotInvoiced <= 0m || (line.IsFromShipment && shipmentLine.QtyNotInvoiced < remaining))
            {
                throw new BusinessException(ErpErrorCodes.Documents.ShipmentLineFullyInvoiced)
                    .WithData("documentNo", line.ShipmentNo ?? header.No)
                    .WithData("lineNo", shipmentLine.LineNo);
            }

            var take = Math.Min(remaining, shipmentLine.QtyNotInvoiced);
            cost += await InvoiceEntriesAsync(await EntriesOfAsync(shipmentLine.Id), take * line.QtyPerUnitOfMeasure, postedDocNo, header.PostingDate);
            shipmentLine.RecordInvoiced(take);
            await ShipmentLineRepository.UpdateAsync(shipmentLine);
            remaining -= take;
        }

        if (remaining > 0m && thisRun.Entries != null)
        {
            cost += await InvoiceEntriesAsync(thisRun.Entries, remaining * line.QtyPerUnitOfMeasure, postedDocNo, header.PostingDate);
            thisRun.ShipmentLine?.RecordInvoiced(remaining);
        }

        return cost;
    }

    /// <summary>Invoices <paramref name="baseQuantity"/> of the entries at their cost, in order, and returns that cost.</summary>
    private async Task<decimal> InvoiceEntriesAsync(List<ItemLedgerEntry> entries, decimal baseQuantity, string postedDocNo, DateTime postingDate)
    {
        var cost = 0m;
        foreach (var entry in entries)
        {
            var open = Math.Abs(entry.Quantity) - Math.Abs(entry.InvoicedQuantity);
            if (baseQuantity == 0m || open <= 0m)
            {
                continue;
            }

            var take = Math.Min(baseQuantity, open);
            var actual = DocumentAmounts.Round(take * entry.UnitCost);
            await _itemJnlPostLine.PostInvoicedCostAsync(entry, take, actual, postedDocNo, postingDate);
            cost += actual;
            baseQuantity -= take;
        }

        return cost;
    }

    /// <summary>The item ledger entries a posted shipment line (or a document line) posted.</summary>
    private async Task<List<ItemLedgerEntry>> EntriesOfAsync(Guid documentLineId)
    {
        return (await ItemLedgerEntryRepository.GetListAsync(e => e.DocumentLineId == documentLineId))
            .OrderBy(e => e.EntryNo)
            .ToList();
    }

    /// <summary>The lot or serial number of an entry, to post its mirror with.</summary>
    private static IReadOnlyList<TrackingPart> TrackingOf(ItemLedgerEntry entry) =>
        entry.LotNo == null && entry.SerialNo == null
            ? null
            : [new TrackingPart(entry.LotNo, entry.SerialNo, Math.Abs(entry.Quantity), entry.ExpirationDate)];

    private async Task<SalesHeader> FindOrderOfLineAsync(Guid orderLineId)
    {
        var lines = await LazyServiceProvider.LazyGetRequiredService<IRepository<SalesLine, Guid>>().GetListAsync(l => l.Id == orderLineId);
        var line = lines.FirstOrDefault();
        return line == null ? null : await _salesHeaderRepository.FindAsync(line.SalesHeaderId);
    }

    /// <summary>"PREFIX-ORDER" for the first document posted from a document, then "PREFIX-ORDER-2" and so on.</summary>
    private static async Task<string> DerivedNoAsync<TPosted>(
        string prefix,
        SalesHeader header,
        IRepository<TPosted, Guid> repository,
        System.Linq.Expressions.Expression<Func<TPosted, bool>> fromThisDocument
    )
        where TPosted : class, Volo.Abp.Domain.Entities.IEntity<Guid>
    {
        var count = await repository.CountAsync(fromThisDocument);
        return count == 0 ? $"{prefix}-{header.No}" : $"{prefix}-{header.No}-{count + 1}";
    }

    private static decimal Revenue(SalesLine line, decimal amount) =>
        line.VatCalculationType == VatCalculationType.FullVat ? 0m : amount;

    private static decimal OutputVat(SalesLine line, LineVat vat) =>
        line.VatCalculationType == VatCalculationType.ReverseChargeVat ? 0m : vat.Amount;

    private sealed class LinePlan
    {
        public SalesLine Line { get; init; }
        public Item Item { get; init; }
        public string RevenueAccountNo { get; init; }
        public VatPostingSetup VatSetup { get; init; }
        public bool PostCost { get; init; }
        public string CogsAccountNo { get; init; }
        public string InventoryAccountNo { get; init; }
        public bool FixedAsset { get; init; }
    }

    /// <summary>The accounts, VAT setup and item of every item and G/L account line being posted.</summary>
    private async Task<Dictionary<Guid, LinePlan>> PlanLinesAsync(SalesHeader header, Customer customer, List<SalesLine> lines)
    {
        var plans = new Dictionary<Guid, LinePlan>();
        var automaticCostPosting = (await InventorySetupManager.GetAsync()).AutomaticCostPosting;

        foreach (var line in lines)
        {
            if (line.Type is not (DocumentLineType.Item or DocumentLineType.GLAccount or DocumentLineType.FixedAsset))
            {
                continue;
            }

            var vatSetup = line.VatBusPostingGroup == null && line.VatProdPostingGroup == null
                ? null
                : await _postingSetupManager.GetVatPostingSetupAsync(line.VatBusPostingGroup, line.VatProdPostingGroup);

            if (line.Type == DocumentLineType.GLAccount)
            {
                plans[line.Id] = new LinePlan { Line = line, RevenueAccountNo = line.No, VatSetup = vatSetup };
                continue;
            }

            if (line.Type == DocumentLineType.FixedAsset)
            {
                if (header.IsReturn)
                {
                    throw new BusinessException(ErpErrorCodes.FixedAssets.CreditMemoNotSupported).WithData("no", line.No);
                }

                // Selling an asset disposes of it; the disposal posts the G/L side of the line.
                await FAPostLine.GetAssetBookAsync(line.No, null);
                plans[line.Id] = new LinePlan { Line = line, VatSetup = vatSetup, FixedAsset = true };
                continue;
            }

            var item = await _itemRepository.FirstOrDefaultAsync(i => i.No == line.No)
                ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", line.No);

            var revenueAccountNo = await _postingSetupManager.GetSalesAccountAsync(customer.GenBusPostingGroup, item.GenProdPostingGroup, header.IsReturn);

            var postCost = automaticCostPosting && item.Type == ItemType.Inventory;
            string cogsAccountNo = null, inventoryAccountNo = null;
            if (postCost && line.QtyToInvoice != 0m)
            {
                cogsAccountNo = await _postingSetupManager.GetCogsAccountAsync(customer.GenBusPostingGroup, item.GenProdPostingGroup);
                inventoryAccountNo = await _postingSetupManager.GetInventoryAccountAsync(item.InventoryPostingGroup, line.LocationCode ?? header.LocationCode);
            }

            plans[line.Id] = new LinePlan
            {
                Line = line,
                Item = item,
                RevenueAccountNo = revenueAccountNo,
                VatSetup = vatSetup,
                PostCost = postCost,
                CogsAccountNo = cogsAccountNo,
                InventoryAccountNo = inventoryAccountNo,
            };
        }

        return plans;
    }
}
