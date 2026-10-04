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

/// <summary>
/// Purchase Document Posting Engine.
///
/// Atomically posts Purchase Headers to Posted Purchase Invoices, G/L Entries, Vendor Ledger
/// Entries, VAT Entries and Item Ledger Entries.
/// <para>
/// An invoice credits the vendor with the amount including VAT, debits stock (or the purchase
/// account for anything that is not stock) and input VAT. A reverse charge line also credits the
/// reverse charge account with the self-assessed VAT. A credit memo turns every sign round.
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

    public async Task<PostedPurchaseHeader> PostAsync(Guid purchaseHeaderId)
    {
        var header = await _purchaseHeaderRepository.GetAsync(purchaseHeaderId);
        if (header.Posted)
        {
            throw new DocumentAlreadyPostedException(header.No);
        }
        if (!header.Lines.Any())
        {
            throw new UserFriendlyException($"Purchase document '{header.No}' has no lines.");
        }

        await _glSetupManager.CheckPostingDateAsync(header.PostingDate);

        var purchaseSetup = await _setupManager.GetAsync();
        if (purchaseSetup.ExtDocNoMandatory && header.VendorInvoiceNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Purchasing.VendorInvoiceNoRequired).WithData("documentNo", header.No);
        }

        var creditMemo = header.DocumentType == PurchaseDocumentType.CreditMemo;
        // Everything is worked out in invoice terms and turned round once for a credit memo.
        var sign = creditMemo ? -1m : 1m;
        var documentType = creditMemo ? GLEntryDocumentType.CreditMemo : GLEntryDocumentType.Invoice;
        var vendor = await _vendorRepository.GetAsync(header.VendorId);

        // Every account is resolved before anything is written, so a missing posting setup stops
        // the document instead of leaving half of it posted.
        var plans = await PlanLinesAsync(header, vendor, creditMemo);

        // As on a sales document: the vendor entry keeps the currency, the G/L and the item costs
        // get LCY, and the vendor's LCY is the sum of the converted G/L lines.
        var currencyCode = await CurrencyManager.NormalizeAsync(header.CurrencyCode);
        var factor = await CurrencyManager.GetCurrencyFactorAsync(currencyCode, header.PostingDate);
        decimal Lcy(decimal amount) => CurrencyExchangeRateManager.ToLcy(amount, factor);
        var vendorLcy = plans.Sum(p => Lcy(Cost(p.Line)) + (p.VatSetup == null || p.Line.VatCalculationType == VatCalculationType.ReverseChargeVat ? 0m : Lcy(p.Line.VatAmount)));

        // Posted documents get their own number from the posted series; the derived number is the
        // fallback for a company that has not set one up.
        var postedNos = purchaseSetup.GetPostedDocumentNos(header.DocumentType);
        string postedDocNo = postedNos.IsNullOrWhiteSpace()
            ? $"{(creditMemo ? "PPCM" : "PPI")}-{header.No}"
            : await _noSeriesManager.GetNextNoAsync(postedNos, header.PostingDate);

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
            header.TotalAmount,
            header.TotalAmountIncludingVat
        );

        foreach (var line in header.Lines)
        {
            postedHeader.Lines.Add(new PostedPurchaseLine(
                GuidGenerator.Create(),
                postedHeader.Id,
                line.LineNo,
                line.Type.ToString(),
                line.No,
                line.Description,
                line.Quantity,
                line.DirectUnitCost,
                line.LineAmount
            ));
        }

        await _postedPurchaseHeaderRepository.InsertAsync(postedHeader);

        // Everything this document posts belongs to one register, so it can be navigated and
        // reversed as a unit, exactly like a journal.
        var register = await _registerManager.OpenAsync(header.PostingDate, SourceCode, postedDocNo);
        var context = new GLPostingContext(register, SourceCode);

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
                $"{(creditMemo ? "Purchase Credit Memo" : "Purchase Invoice")} {postedDocNo}",
                -sign * header.TotalAmountIncludingVat
            );
        if (currencyCode != null)
        {
            vendorLine.SetCurrency(currencyCode, factor);
            vendorLine.SetAmountLcy(-sign * vendorLcy);
        }

        await _genJnlPostLine.PostLineAsync(vendorLine, context, header.DueDate);

        // 3. Per line: stock, the debit account and VAT
        foreach (var plan in plans)
        {
            var line = plan.Line;

            if (plan.Item != null)
            {
                await _itemJnlPostLine.PostItemEntryAsync(
                    plan.Item.Id,
                    line.No,
                    header.PostingDate,
                    ItemLedgerEntryType.Purchase,
                    postedDocNo,
                    line.Description,
                    line.Quantity,
                    factor == 1m ? line.DirectUnitCost : Math.Round(line.DirectUnitCost / factor, 5, MidpointRounding.AwayFromZero),
                    locationCode: header.LocationCode,
                    correction: creditMemo
                );
            }

            var cost = Lcy(Cost(line));
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
            }

            if (plan.VatSetup != null)
            {
                await _genJnlPostLine.PostVatAsync(
                    plan.VatSetup,
                    VatEntryType.Purchase,
                    sign * Lcy(line.VatBaseAmount),
                    sign * Lcy(line.VatAmount),
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

        await _registerManager.CloseAsync(register);

        // 4. Mark header as posted
        header.MarkPosted(postedDocNo);
        await _purchaseHeaderRepository.UpdateAsync(header);

        // Attachments marked to flow follow the document onto the posted document.
        await LazyServiceProvider
            .LazyGetRequiredService<DocumentAttachmentManager>()
            .FlowToPostedDocumentAsync(nameof(PurchaseHeader), header.Id, nameof(PostedPurchaseHeader), postedHeader.Id, postedHeader.No, purchase: true);

        return postedHeader;
    }

    private static decimal Cost(PurchaseLine line) =>
        line.VatCalculationType == VatCalculationType.FullVat ? 0m : line.LineAmount;

    private sealed class LinePlan
    {
        public PurchaseLine Line { get; init; }
        public Item Item { get; init; }
        public string AccountNo { get; init; }
        public VatPostingSetup VatSetup { get; init; }
    }

    /// <summary>
    /// The account each item and G/L account line debits. Stock goes to the Inventory Posting
    /// Setup's inventory account; services and non-inventory items to the General Posting Setup's
    /// purchase account; a G/L account line to its own account.
    /// </summary>
    private async Task<List<LinePlan>> PlanLinesAsync(PurchaseHeader header, Vendor vendor, bool creditMemo)
    {
        var plans = new List<LinePlan>();

        foreach (var line in header.Lines)
        {
            if (line.Type is not (DocumentLineType.Item or DocumentLineType.GLAccount))
            {
                continue;
            }

            var vatSetup = line.VatBusPostingGroup == null && line.VatProdPostingGroup == null
                ? null
                : await _postingSetupManager.GetVatPostingSetupAsync(line.VatBusPostingGroup, line.VatProdPostingGroup);

            if (line.Type == DocumentLineType.GLAccount)
            {
                plans.Add(new LinePlan { Line = line, AccountNo = line.No, VatSetup = vatSetup });
                continue;
            }

            var item = await _itemRepository.FirstOrDefaultAsync(i => i.No == line.No)
                ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", line.No);

            var accountNo = item.Type == ItemType.Inventory
                ? await _postingSetupManager.GetInventoryAccountAsync(item.InventoryPostingGroup, header.LocationCode)
                : await _postingSetupManager.GetPurchaseAccountAsync(vendor.GenBusPostingGroup, item.GenProdPostingGroup, creditMemo);

            plans.Add(new LinePlan { Line = line, Item = item, AccountNo = accountNo, VatSetup = vatSetup });
        }

        return plans;
    }
}
