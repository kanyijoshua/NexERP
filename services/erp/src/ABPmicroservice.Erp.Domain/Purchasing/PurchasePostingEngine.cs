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

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>What one posting run of a purchase document produced.</summary>
public sealed class PurchasePostingResult
{
    /// <summary>The posted invoice or credit memo; null when the run only received.</summary>
    public PostedPurchaseHeader Invoice { get; init; }

    /// <summary>The posted receipt or return shipment; null for an invoice or a run that only invoiced.</summary>
    public PostedPurchaseReceipt Receipt { get; init; }
}

/// <summary>
/// Purchase Document Posting Engine.
///
/// Receives and invoices purchase documents: Item Ledger Entries and Posted Purchase Receipts for
/// what comes in, and Posted Purchase Invoices, G/L Entries, Vendor Ledger Entries and VAT Entries
/// for what is invoiced.
/// <para>
/// An invoice or credit memo receives and invoices everything in one run. An order is received
/// and invoiced in parts, as its lines' quantities to receive and to invoice say; a return order
/// does the same with goods going back and a credit memo. A quote or blanket order is never posted.
/// </para>
/// <para>
/// Receiving brings the stock in at the order's cost as expected cost and posts nothing to the
/// G/L. Invoicing credits the vendor with the amount including VAT, debits stock (or the purchase
/// account for anything that is not stock) and input VAT, and turns the received items' expected
/// cost into the invoiced cost. A reverse charge line also credits the reverse charge account with
/// the self-assessed VAT. A credit memo or return order turns every sign round.
/// </para>
/// </summary>
public class PurchasePostingEngine : DomainService
{
    /// <summary>Stamped on every entry the engine posts.</summary>
    private const string SourceCode = "PURCHASES";

    private readonly IRepository<PurchaseHeader, Guid> _purchaseHeaderRepository;
    private readonly IRepository<PostedPurchaseHeader, Guid> _postedPurchaseHeaderRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly PostingSetupManager _postingSetupManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly ItemJnlPostLine _itemJnlPostLine;
    private readonly PurchasesPayablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly GLRegisterManager _registerManager;

    private CurrencyExchangeRateManager CurrencyManager => LazyServiceProvider.LazyGetRequiredService<CurrencyExchangeRateManager>();
    private IRepository<PostedPurchaseReceipt, Guid> ReceiptRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<PostedPurchaseReceipt, Guid>>();
    private IRepository<PostedPurchaseReceiptLine, Guid> ReceiptLineRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<PostedPurchaseReceiptLine, Guid>>();
    private IRepository<ItemLedgerEntry, Guid> ItemLedgerEntryRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<ItemLedgerEntry, Guid>>();
    private ItemTrackingManager TrackingManager => LazyServiceProvider.LazyGetRequiredService<ItemTrackingManager>();
    private DeferralPoster DeferralPoster => LazyServiceProvider.LazyGetRequiredService<DeferralPoster>();
    private FixedAssets.FAJnlPostLine FAPostLine => LazyServiceProvider.LazyGetRequiredService<FixedAssets.FAJnlPostLine>();

    public PurchasePostingEngine(
        IRepository<PurchaseHeader, Guid> purchaseHeaderRepository,
        IRepository<PostedPurchaseHeader, Guid> postedPurchaseHeaderRepository,
        IRepository<Vendor, Guid> vendorRepository,
        IRepository<Item, Guid> itemRepository,
        PostingSetupManager postingSetupManager,
        GeneralLedgerSetupManager glSetupManager,
        GenJnlPostLine genJnlPostLine,
        ItemJnlPostLine itemJnlPostLine,
        PurchasesPayablesSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        GLRegisterManager registerManager
    )
    {
        _registerManager = registerManager;
        _purchaseHeaderRepository = purchaseHeaderRepository;
        _postedPurchaseHeaderRepository = postedPurchaseHeaderRepository;
        _vendorRepository = vendorRepository;
        _itemRepository = itemRepository;
        _postingSetupManager = postingSetupManager;
        _glSetupManager = glSetupManager;
        _genJnlPostLine = genJnlPostLine;
        _itemJnlPostLine = itemJnlPostLine;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
    }

    /// <summary>
    /// Posts a purchase document. <paramref name="receive"/> and <paramref name="invoice"/> only
    /// matter for an order or return order; any other document is always received and invoiced in full.
    /// </summary>
    public async Task<PurchasePostingResult> PostAsync(Guid purchaseHeaderId, bool receive = true, bool invoice = true)
    {
        var header = await _purchaseHeaderRepository.GetAsync(purchaseHeaderId);
        if (header.Posted)
        {
            throw new DocumentAlreadyPostedException(header.No);
        }
        if (header.DocumentType is PurchaseDocumentType.Quote or PurchaseDocumentType.BlanketOrder)
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentTypeCannotBePosted)
                .WithData("documentType", header.DocumentType.ToString())
                .WithData("documentNo", header.No);
        }
        header.EnsureNotClosed();
        if (!header.Lines.Any())
        {
            throw new UserFriendlyException($"Purchase document '{header.No}' has no lines.");
        }

        if (!header.IsOrderType)
        {
            receive = true;
            invoice = true;
            foreach (var line in header.Lines)
            {
                line.ResetQuantitiesToPost();
            }
        }

        // The quantities may have been set before the last line change; check them again.
        foreach (var line in header.Lines)
        {
            line.SetQuantitiesToPost(header.No, line.QtyToReceive, line.QtyToInvoice);
        }

        var toReceive = receive ? header.Lines.Where(l => !l.IsFromReceipt && l.QtyToReceive != 0m).ToList() : [];
        var toInvoice = invoice ? header.Lines.Where(l => l.QtyToInvoice != 0m).ToList() : [];
        if (toReceive.Count == 0 && toInvoice.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Documents.NothingToPost).WithData("documentNo", header.No);
        }

        await _glSetupManager.CheckPostingDateAsync(header.PostingDate);

        var purchaseSetup = await _setupManager.GetAsync();
        if (toInvoice.Count > 0 && purchaseSetup.ExtDocNoMandatory && header.VendorInvoiceNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Purchasing.VendorInvoiceNoRequired).WithData("documentNo", header.No);
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
        var vendor = await _vendorRepository.GetAsync(header.VendorId);

        // Every account is resolved before anything is written, so a missing posting setup stops
        // the document instead of leaving half of it posted.
        var plans = await PlanLinesAsync(header, vendor, toReceive.Union(toInvoice).ToList());

        // As on a sales document: the vendor entry keeps the currency, the G/L and the item costs
        // get LCY, and the vendor's LCY is the sum of the converted G/L lines.
        var currencyCode = await CurrencyManager.NormalizeAsync(header.CurrencyCode);
        var factor = await CurrencyManager.GetCurrencyFactorAsync(currencyCode, header.PostingDate);

        // Posted documents get their own number from the posted series; the derived number is the
        // fallback for a company that has not set one up.
        string postedDocNo = null;
        if (toInvoice.Count > 0)
        {
            var postedNos = purchaseSetup.GetPostedDocumentNos(header.DocumentType);
            postedDocNo = postedNos.IsNullOrWhiteSpace()
                ? await DerivedNoAsync(isReturn ? "PPCM" : "PPI", header, _postedPurchaseHeaderRepository, x => x.PreAssignedNo == header.No)
                : await _noSeriesManager.GetNextNoAsync(postedNos, header.PostingDate);
        }

        string receiptNo = null;
        if (toReceive.Count > 0 && header.IsOrderType)
        {
            var receiptNos = purchaseSetup.GetReceiptNos(isReturn);
            receiptNo = receiptNos.IsNullOrWhiteSpace()
                ? await DerivedNoAsync(isReturn ? "PRS" : "PRC", header, ReceiptRepository, x => x.OrderNo == header.No)
                : await _noSeriesManager.GetNextNoAsync(receiptNos, header.PostingDate);
        }

        // Everything this run posts belongs to one register, so it can be navigated and reversed
        // as a unit, exactly like a journal.
        var register = await _registerManager.OpenAsync(header.PostingDate, SourceCode, postedDocNo ?? receiptNo);
        var context = new GLPostingContext(register, SourceCode);

        // 1. Receive: the stock comes in (or, on a return, goes back) at the order's cost.
        var received = await ReceiveAsync(header, toReceive, plans, receiptNo ?? postedDocNo, receiptNo, isReturn, factor);

        // 2. Invoice: the vendor, the debit accounts, VAT and the actual cost of the goods.
        PostedPurchaseHeader postedHeader = null;
        if (toInvoice.Count > 0)
        {
            postedHeader = await InvoiceAsync(header, toInvoice, plans, received, postedDocNo, currencyCode, factor, sign, documentType, context);
        }

        await _registerManager.CloseAsync(register);

        // 3. Move the document on: posted once nothing is left to invoice.
        header.RecordPosting(received.Receipt?.No, postedDocNo);
        await _purchaseHeaderRepository.UpdateAsync(header);

        if (postedHeader != null)
        {
            // Attachments marked to flow follow the document onto the posted document.
            await LazyServiceProvider
                .LazyGetRequiredService<DocumentAttachmentManager>()
                .FlowToPostedDocumentAsync(nameof(PurchaseHeader), header.Id, nameof(PostedPurchaseHeader), postedHeader.Id, postedHeader.No, purchase: true);
        }

        return new PurchasePostingResult { Invoice = postedHeader, Receipt = received.Receipt };
    }

    /// <summary>
    /// Undoes a posted receipt (or return shipment) line that has not been invoiced: the stock moves
    /// back, the line is marked as a correction, and the order line can be received again.
    /// </summary>
    public async Task UndoReceiptLineAsync(Guid receiptLineId)
    {
        var receiptLine = await ReceiptLineRepository.GetAsync(receiptLineId);
        var receipt = await ReceiptRepository.GetAsync(receiptLine.PostedPurchaseReceiptId);
        if (receiptLine.Correction || receiptLine.QuantityInvoiced != 0m)
        {
            throw new BusinessException(ErpErrorCodes.Documents.CannotUndoInvoicedLine)
                .WithData("documentNo", receipt.No)
                .WithData("lineNo", receiptLine.LineNo);
        }

        await _glSetupManager.CheckPostingDateAsync(receipt.PostingDate);

        foreach (var entry in await EntriesOfAsync(receiptLine.Id))
        {
            // The mirror movement at the same cost and lot, taken from the very entry it undoes;
            // both are invoiced at once so neither is left waiting for an invoice.
            var mirror = await _itemJnlPostLine.PostAsync(new ItemPostingRequest
            {
                ItemId = entry.ItemId,
                ItemNo = entry.ItemNo,
                PostingDate = receipt.PostingDate,
                EntryType = entry.EntryType,
                DocumentNo = receipt.No,
                Description = receiptLine.Description,
                Quantity = Math.Abs(entry.Quantity),
                UnitCost = entry.UnitCost,
                LocationCode = entry.LocationCode,
                VariantCode = entry.VariantCode,
                Tracking = TrackingOf(entry),
                Correction = !receipt.Return,
                AppliesToEntryId = entry.Positive ? entry.Id : null,
                ExpectedCost = true,
            });
            await _itemJnlPostLine.PostInvoicedCostAsync(entry, Math.Abs(entry.Quantity), Math.Abs(entry.Quantity) * entry.UnitCost, receipt.No, receipt.PostingDate);
            await _itemJnlPostLine.PostInvoicedCostAsync(mirror, Math.Abs(mirror.Quantity), Math.Abs(mirror.Quantity) * mirror.UnitCost, receipt.No, receipt.PostingDate);
        }

        receiptLine.MarkUndone();
        await ReceiptLineRepository.UpdateAsync(receiptLine);

        var order = await FindOrderOfLineAsync(receiptLine.OrderLineId);
        if (order != null)
        {
            order.Lines.First(l => l.Id == receiptLine.OrderLineId).RecordReceiptUndone(receiptLine.Quantity);
            await _purchaseHeaderRepository.UpdateAsync(order);
        }
    }

    private sealed class ReceivedParts
    {
        public PostedPurchaseReceipt Receipt { get; init; }

        /// <summary>The item ledger entries each line received in this run, by line id.</summary>
        public Dictionary<Guid, (List<ItemLedgerEntry> Entries, PostedPurchaseReceiptLine ReceiptLine)> Lines { get; } = new();
    }

    private async Task<ReceivedParts> ReceiveAsync(
        PurchaseHeader header,
        List<PurchaseLine> toReceive,
        Dictionary<Guid, LinePlan> plans,
        string entryDocumentNo,
        string receiptNo,
        bool isReturn,
        decimal factor
    )
    {
        var receipt = receiptNo == null ? null : new PostedPurchaseReceipt(GuidGenerator.Create(), receiptNo, isReturn, header);
        var parts = new ReceivedParts { Receipt = receipt };

        foreach (var line in toReceive)
        {
            var locationCode = line.LocationCode ?? header.LocationCode;
            var receiptLineId = receipt != null && !line.No.IsNullOrWhiteSpace() ? GuidGenerator.Create() : (Guid?)null;
            var entries = new List<ItemLedgerEntry>();
            if (plans.TryGetValue(line.Id, out var plan) && plan.Item != null)
            {
                var baseQty = line.QtyToReceive * line.QtyPerUnitOfMeasure;
                entries = await _itemJnlPostLine.PostAllAsync(new ItemPostingRequest
                {
                    ItemId = plan.Item.Id,
                    ItemNo = line.No,
                    PostingDate = header.PostingDate,
                    EntryType = ItemLedgerEntryType.Purchase,
                    DocumentNo = entryDocumentNo,
                    Description = line.Description,
                    Quantity = baseQty,
                    UnitCost = UnitCostLcy(line, factor),
                    LocationCode = locationCode,
                    VariantCode = line.VariantCode,
                    Tracking = await TrackingManager.TakeAsync(plan.Item, ItemTrackingSourceType.PurchaseLine, line.Id, baseQty),
                    DocumentLineId = receiptLineId ?? line.Id,
                    Correction = isReturn,
                    ExpectedCost = true,
                });
            }

            PostedPurchaseReceiptLine receiptLine = null;
            if (receiptLineId.HasValue)
            {
                receiptLine = new PostedPurchaseReceiptLine(receiptLineId.Value, receipt.Id, line, line.QtyToReceive, locationCode, entries.FirstOrDefault()?.Id);
                receipt.Lines.Add(receiptLine);
            }

            parts.Lines[line.Id] = (entries, receiptLine);
            line.RecordReceived(line.QtyToReceive);
        }

        if (receipt != null)
        {
            await ReceiptRepository.InsertAsync(receipt);
        }

        return parts;
    }

    private async Task<PostedPurchaseHeader> InvoiceAsync(
        PurchaseHeader header,
        List<PurchaseLine> toInvoice,
        Dictionary<Guid, LinePlan> plans,
        ReceivedParts received,
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
        var vendorLcy = posting.Sum(i =>
            Lcy(Cost(i.Line, i.Amount))
            + (plans[i.Line.Id].VatSetup == null || i.Line.VatCalculationType == VatCalculationType.ReverseChargeVat ? 0m : Lcy(i.Vat.Amount)));

        // 1. Create Posted Purchase Invoice
        var postedHeader = new PostedPurchaseHeader(
            GuidGenerator.Create(),
            postedDocNo,
            header.No,
            header.VendorId,
            header.BuyFromVendorNo,
            header.BuyFromVendorName,
            header.PostingDate,
            header.DueDate ?? header.PostingDate.AddDays(ErpDomainConsts.DefaultPaymentDueDays),
            totalAmount,
            totalAmountIncludingVat
        );
        postedHeader.SetDetails(header.IsReturn, header.IsOrderType ? header.No : null, currencyCode, header.VendorInvoiceNo);

        foreach (var (line, quantity, amount, vat) in invoiced.Where(i => !i.Line.No.IsNullOrWhiteSpace()))
        {
            var postedLine = new PostedPurchaseLine(
                GuidGenerator.Create(),
                postedHeader.Id,
                line.LineNo,
                line.Type.ToString(),
                line.No,
                line.Description,
                quantity,
                line.DirectUnitCost,
                amount
            );
            postedLine.SetDetails(line.LineDiscountPercent, line.UnitOfMeasureCode, line.VatPercent, vat.Amount, vat.AmountIncludingVat);
            postedLine.SetSource(header.IsOrderType ? line.LineNo : null, line.ReceiptNo, line.ReceiptLineNo);
            postedHeader.Lines.Add(postedLine);
        }

        await _postedPurchaseHeaderRepository.InsertAsync(postedHeader);

        // 2. Post Vendor Ledger Entry (A/P) and the payables control account with it
        var vendorLine = new GenJournalLine(
                GuidGenerator.Create(),
                Guid.Empty,
                1,
                header.PostingDate,
                documentType,
                postedDocNo,
                GenJournalAccountType.Vendor,
                header.BuyFromVendorNo,
                $"{(header.IsReturn ? "Purchase Credit Memo" : "Purchase Invoice")} {postedDocNo}",
                -sign * totalAmountIncludingVat
            );
        if (currencyCode != null)
        {
            vendorLine.SetCurrency(currencyCode, factor);
            vendorLine.SetAmountLcy(-sign * vendorLcy);
        }
        if (header.PaymentDiscountPct > 0m && header.PmtDiscountDate.HasValue)
        {
            vendorLine.SetPaymentDiscount(header.PmtDiscountDate, header.PaymentDiscountPct);
        }

        await _genJnlPostLine.PostLineAsync(vendorLine, context, header.DueDate);

        // 3. Per line: the actual cost of the goods, the debit account and VAT
        foreach (var (line, quantity, amount, vat) in posting)
        {
            var plan = plans[line.Id];
            var cost = Lcy(Cost(line, amount));

            if (plan.Item != null)
            {
                await InvoiceReceivedCostAsync(header, line, quantity, cost, received, postedDocNo);
            }

            if (cost != 0m)
            {
                await _genJnlPostLine.PostGLDirectAsync(
                    plan.AccountNo,
                    header.PostingDate,
                    documentType,
                    postedDocNo,
                    line.Description,
                    sign * cost,
                    header.BuyFromVendorNo,
                    context: context
                );

                if (line.ProjectNo != null)
                {
                    await LazyServiceProvider.LazyGetRequiredService<Projects.ProjectManager>().PostDocumentLineAsync(
                        line.ProjectNo, line.ProjectTaskNo, Projects.ProjectLedgerEntryType.Usage,
                        plan.Item != null ? Projects.ProjectLineType.Item : Projects.ProjectLineType.GLAccount, line.No, line.Description,
                        sign * quantity, sign * cost, header.PostingDate, postedDocNo, context);
                }

                if (plan.FixedAsset)
                {
                    // The G/L is posted above; the asset's book records the acquisition cost.
                    await FAPostLine.PostAsync(
                        new FixedAssets.FAPostingRequest
                        {
                            FANo = line.No,
                            PostingType = FixedAssets.FAJournalPostingType.AcquisitionCost,
                            PostingDate = header.PostingDate,
                            DocumentType = documentType,
                            DocumentNo = postedDocNo,
                            Description = line.Description,
                            Amount = sign * cost,
                            PostGL = false,
                            SourceNo = header.BuyFromVendorNo,
                        },
                        context
                    );
                }

                // A prepaid service is spread over the periods it covers; stock is never deferred.
                if (plan.Item == null && !plan.FixedAsset && line.DeferralCode != null)
                {
                    await DeferralPoster.PostAsync(line.DeferralCode, DeferralSourceType.Purchase, plan.AccountNo, sign * cost, header.PostingDate, documentType, postedDocNo, line.LineNo, line.Description, header.BuyFromVendorNo, Guid.Empty, context);
                }
            }

            if (plan.VatSetup != null)
            {
                await _genJnlPostLine.PostVatAsync(
                    plan.VatSetup,
                    VatEntryType.Purchase,
                    sign * Lcy(vat.Base),
                    sign * Lcy(vat.Amount),
                    header.PostingDate,
                    header.PostingDate,
                    documentType,
                    postedDocNo,
                    header.BuyFromVendorNo,
                    line.Description,
                    context
                );
            }
        }

        foreach (var (line, quantity, _, _) in invoiced)
        {
            line.RecordInvoiced(quantity);
        }

        // A line taken from a receipt invoices that receipt's order line too.
        foreach (var group in invoiced.Where(i => i.Line.IsFromReceipt).GroupBy(i => i.Line.ReceiptLineId!.Value))
        {
            var receiptLine = await ReceiptLineRepository.GetAsync(group.Key);
            var order = await FindOrderOfLineAsync(receiptLine.OrderLineId);
            if (order != null && order.Id != header.Id)
            {
                order.Lines.First(l => l.Id == receiptLine.OrderLineId).RecordInvoiced(group.Sum(i => i.Quantity));
                order.RecordPosting(null, postedDocNo);
                await _purchaseHeaderRepository.UpdateAsync(order);
            }
        }

        return postedHeader;
    }

    /// <summary>
    /// Turns the expected cost of the received goods this invoice covers into the invoiced cost. An
    /// order line invoices its earlier receipts first, then what this run received; a line taken
    /// from a receipt invoices that receipt line. The invoiced cost is shared over them by quantity.
    /// </summary>
    private async Task InvoiceReceivedCostAsync(PurchaseHeader header, PurchaseLine line, decimal quantity, decimal costLcy, ReceivedParts received, string postedDocNo)
    {
        var parts = new List<(List<ItemLedgerEntry> Entries, decimal Quantity)>();
        var remaining = quantity;

        var thisRun = received.Lines.GetValueOrDefault(line.Id);
        List<PostedPurchaseReceiptLine> earlierLines;
        if (line.IsFromReceipt)
        {
            earlierLines = [await ReceiptLineRepository.GetAsync(line.ReceiptLineId!.Value)];
        }
        else
        {
            earlierLines = (await ReceiptLineRepository.GetListAsync(l => l.OrderLineId == line.Id && !l.Correction))
                .Where(l => l.QtyNotInvoiced > 0m && l.Id != thisRun.ReceiptLine?.Id)
                .OrderBy(l => l.Id)
                .ToList();
        }

        foreach (var receiptLine in earlierLines)
        {
            if (remaining == 0m)
            {
                break;
            }
            // A line taken from a posted receipt may not invoice more than is left on it.
            if (receiptLine.QtyNotInvoiced <= 0m || (line.IsFromReceipt && receiptLine.QtyNotInvoiced < remaining))
            {
                throw new BusinessException(ErpErrorCodes.Documents.ShipmentLineFullyInvoiced)
                    .WithData("documentNo", line.ReceiptNo ?? header.No)
                    .WithData("lineNo", receiptLine.LineNo);
            }

            var take = Math.Min(remaining, receiptLine.QtyNotInvoiced);
            parts.Add((await EntriesOfAsync(receiptLine.Id), take));
            receiptLine.RecordInvoiced(take);
            await ReceiptLineRepository.UpdateAsync(receiptLine);
            remaining -= take;
        }

        if (remaining > 0m && thisRun.Entries != null)
        {
            parts.Add((thisRun.Entries, remaining));
            thisRun.ReceiptLine?.RecordInvoiced(remaining);
        }

        // The invoiced cost is shared over the entries by base quantity; the last share takes the rounding.
        var costLeft = costLcy;
        var baseLeft = quantity * line.QtyPerUnitOfMeasure;
        foreach (var (entries, take) in parts)
        {
            var baseQuantity = take * line.QtyPerUnitOfMeasure;
            foreach (var entry in entries)
            {
                var open = Math.Abs(entry.Quantity) - Math.Abs(entry.InvoicedQuantity);
                if (baseQuantity == 0m || open <= 0m)
                {
                    continue;
                }

                var share = Math.Min(baseQuantity, open);
                var amount = share == baseLeft ? costLeft : DocumentAmounts.Round(costLcy * share / (quantity * line.QtyPerUnitOfMeasure));
                await _itemJnlPostLine.PostInvoicedCostAsync(entry, share, amount, postedDocNo, header.PostingDate);
                costLeft -= amount;
                baseLeft -= share;
                baseQuantity -= share;
            }
        }
    }

    /// <summary>The item ledger entries a posted receipt line (or a document line) posted.</summary>
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

    private async Task<PurchaseHeader> FindOrderOfLineAsync(Guid orderLineId)
    {
        var lines = await LazyServiceProvider.LazyGetRequiredService<IRepository<PurchaseLine, Guid>>().GetListAsync(l => l.Id == orderLineId);
        var line = lines.FirstOrDefault();
        return line == null ? null : await _purchaseHeaderRepository.FindAsync(line.PurchaseHeaderId);
    }

    /// <summary>"PREFIX-ORDER" for the first document posted from a document, then "PREFIX-ORDER-2" and so on.</summary>
    private static async Task<string> DerivedNoAsync<TPosted>(
        string prefix,
        PurchaseHeader header,
        IRepository<TPosted, Guid> repository,
        System.Linq.Expressions.Expression<Func<TPosted, bool>> fromThisDocument
    )
        where TPosted : class, Volo.Abp.Domain.Entities.IEntity<Guid>
    {
        var count = await repository.CountAsync(fromThisDocument);
        return count == 0 ? $"{prefix}-{header.No}" : $"{prefix}-{header.No}-{count + 1}";
    }

    /// <summary>The cost of one unit after the line discount, in LCY.</summary>
    private static decimal UnitCostLcy(PurchaseLine line, decimal factor)
    {
        if (line.Quantity == 0m)
        {
            return 0m;
        }

        var unitCost = Cost(line, line.LineAmount) / (line.Quantity * line.QtyPerUnitOfMeasure);
        return Math.Round(factor == 1m ? unitCost : unitCost / factor, 5, MidpointRounding.AwayFromZero);
    }

    private static decimal Cost(PurchaseLine line, decimal amount) =>
        line.VatCalculationType == VatCalculationType.FullVat ? 0m : amount;

    private sealed class LinePlan
    {
        public PurchaseLine Line { get; init; }
        public Item Item { get; init; }
        public string AccountNo { get; init; }
        public VatPostingSetup VatSetup { get; init; }
        public bool FixedAsset { get; init; }
    }

    /// <summary>
    /// The account each item and G/L account line debits. Stock goes to the Inventory Posting
    /// Setup's inventory account; services and non-inventory items to the General Posting Setup's
    /// purchase account; a G/L account line to its own account.
    /// </summary>
    private async Task<Dictionary<Guid, LinePlan>> PlanLinesAsync(PurchaseHeader header, Vendor vendor, List<PurchaseLine> lines)
    {
        var plans = new Dictionary<Guid, LinePlan>();

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
                plans[line.Id] = new LinePlan { Line = line, AccountNo = line.No, VatSetup = vatSetup };
                continue;
            }

            if (line.Type == DocumentLineType.FixedAsset)
            {
                // Buying an asset acquires it: the cost goes to its acquisition cost account.
                plans[line.Id] = new LinePlan { Line = line, AccountNo = await FAPostLine.GetAcquisitionAccountAsync(line.No), VatSetup = vatSetup, FixedAsset = true };
                continue;
            }

            var item = await _itemRepository.FirstOrDefaultAsync(i => i.No == line.No)
                ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", line.No);

            var accountNo = item.Type == ItemType.Inventory
                ? await _postingSetupManager.GetInventoryAccountAsync(item.InventoryPostingGroup, line.LocationCode ?? header.LocationCode)
                : await _postingSetupManager.GetPurchaseAccountAsync(vendor.GenBusPostingGroup, item.GenProdPostingGroup, header.IsReturn);

            plans[line.Id] = new LinePlan { Line = line, Item = item, AccountNo = accountNo, VatSetup = vatSetup };
        }

        return plans;
    }
}
