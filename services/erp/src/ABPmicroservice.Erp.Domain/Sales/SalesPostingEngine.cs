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

/// <summary>
/// Sales Document Posting Engine.
/// Mirrors Business Central Codeunit 80 "Sales-Post".
/// Atomically posts Sales Headers to Posted Sales Invoices, G/L Entries, Customer Ledger Entries,
/// VAT Entries and Item Ledger Entries.
/// <para>
/// An invoice debits the customer with the amount including VAT, credits revenue and output VAT,
/// and (with automatic cost posting) moves the cost of the goods from inventory to COGS. A credit
/// memo posts the same with every sign turned round and brings the stock back.
/// </para>
/// </summary>
public class SalesPostingEngine : DomainService
{
    /// <summary>Stamped on every entry the engine posts. Mirrors BC's "SALES" source code.</summary>
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

    public async Task<PostedSalesHeader> PostAsync(Guid salesHeaderId)
    {
        var header = await _salesHeaderRepository.GetAsync(salesHeaderId);
        if (header.Posted)
        {
            throw new DocumentAlreadyPostedException(header.No);
        }
        if (!header.Lines.Any())
        {
            throw new UserFriendlyException($"Sales document '{header.No}' has no lines.");
        }

        await _glSetupManager.CheckPostingDateAsync(header.PostingDate);

        var salesSetup = await _setupManager.GetAsync();
        if (salesSetup.ExtDocNoMandatory && header.ExternalDocumentNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Sales.ExternalDocumentNoRequired).WithData("documentNo", header.No);
        }

        var creditMemo = header.DocumentType == SalesDocumentType.CreditMemo;
        // Everything is worked out in invoice terms and turned round once for a credit memo.
        var sign = creditMemo ? -1m : 1m;
        var documentType = creditMemo ? GLEntryDocumentType.CreditMemo : GLEntryDocumentType.Invoice;
        var customer = await _customerRepository.GetAsync(header.CustomerId);

        // Every account is resolved before anything is written, so a missing posting setup
        // stops the document instead of leaving half of it posted.
        var plans = await PlanLinesAsync(header, customer, creditMemo);

        // A foreign currency document posts at its posting date's rate: the customer entry keeps
        // the currency, the G/L gets LCY, and the customer's LCY is the sum of the converted G/L
        // lines so rounding cannot unbalance the document.
        var currencyCode = await CurrencyManager.NormalizeAsync(header.CurrencyCode);
        var factor = await CurrencyManager.GetCurrencyFactorAsync(currencyCode, header.PostingDate);
        decimal Lcy(decimal amount) => CurrencyExchangeRateManager.ToLcy(amount, factor);
        var customerLcy = plans.Sum(p => Lcy(Revenue(p.Line)) + (p.VatSetup == null ? 0m : Lcy(OutputVat(p.Line))));

        // Posted documents get their own number from the posted series; the derived number is the
        // fallback for a company that has not set one up.
        var postedNos = salesSetup.GetPostedDocumentNos(header.DocumentType);
        string postedDocNo = postedNos.IsNullOrWhiteSpace()
            ? $"{(creditMemo ? "PSCM" : "PSI")}-{header.No}"
            : await _noSeriesManager.GetNextNoAsync(postedNos, header.PostingDate);

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
            header.TotalAmount,
            header.TotalAmountIncludingVat
        );

        foreach (var line in header.Lines)
        {
            postedHeader.Lines.Add(new PostedSalesLine(
                GuidGenerator.Create(),
                postedHeader.Id,
                line.LineNo,
                line.Type.ToString(),
                line.No,
                line.Description,
                line.Quantity,
                line.UnitPrice,
                line.LineAmount
            ));
        }

        await _postedSalesHeaderRepository.InsertAsync(postedHeader);

        // Everything this document posts belongs to one register, so it can be navigated and
        // reversed as a unit, exactly like a journal.
        var register = await _registerManager.OpenAsync(header.PostingDate, SourceCode, postedDocNo);
        var context = new GLPostingContext(register, SourceCode);

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
                $"{(creditMemo ? "Sales Credit Memo" : "Sales Invoice")} {postedDocNo}",
                sign * header.TotalAmountIncludingVat
            );
        if (currencyCode != null)
        {
            customerLine.SetCurrency(currencyCode, factor);
            customerLine.SetAmountLcy(sign * customerLcy);
        }

        await _genJnlPostLine.PostLineAsync(customerLine, context, header.DueDate);

        // 3. Per line: stock, revenue, VAT and cost of goods sold
        foreach (var plan in plans)
        {
            var line = plan.Line;

            if (plan.Item != null)
            {
                await _itemJnlPostLine.PostItemEntryAsync(
                    plan.Item.Id,
                    line.No,
                    header.PostingDate,
                    ItemLedgerEntryType.Sale,
                    postedDocNo,
                    line.Description,
                    line.Quantity,
                    plan.Item.UnitCost,
                    locationCode: header.LocationCode,
                    correction: creditMemo
                );
            }

            // Revenue (a credit on an invoice) is the line amount; a Full VAT line is all VAT.
            var revenue = Lcy(Revenue(line));
            if (revenue != 0m)
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
            }

            if (plan.VatSetup != null)
            {
                // Output VAT is owed on a sale; under reverse charge the customer accounts for it.
                await _genJnlPostLine.PostVatAsync(
                    plan.VatSetup,
                    VatEntryType.Sale,
                    -sign * Lcy(line.VatBaseAmount),
                    -sign * Lcy(OutputVat(line)),
                    header.PostingDate,
                    header.PostingDate,
                    documentType,
                    postedDocNo,
                    header.SellToCustomerNo,
                    line.Description,
                    context
                );
            }

            if (plan.CostAmount != 0m)
            {
                // The goods leave inventory at cost and become cost of goods sold.
                await _genJnlPostLine.PostGLDirectAsync(
                    plan.CogsAccountNo,
                    header.PostingDate,
                    documentType,
                    postedDocNo,
                    line.Description,
                    sign * plan.CostAmount,
                    header.SellToCustomerNo,
                    context: context
                );
                await _genJnlPostLine.PostGLDirectAsync(
                    plan.InventoryAccountNo,
                    header.PostingDate,
                    documentType,
                    postedDocNo,
                    line.Description,
                    -sign * plan.CostAmount,
                    header.SellToCustomerNo,
                    context: context
                );
            }
        }

        await _registerManager.CloseAsync(register);

        // 4. Mark header as posted
        header.MarkPosted(postedDocNo);
        await _salesHeaderRepository.UpdateAsync(header);

        return postedHeader;
    }

    private static decimal Revenue(SalesLine line) =>
        line.VatCalculationType == VatCalculationType.FullVat ? 0m : line.LineAmount;

    private static decimal OutputVat(SalesLine line) =>
        line.VatCalculationType == VatCalculationType.ReverseChargeVat ? 0m : line.VatAmount;

    private sealed class LinePlan
    {
        public SalesLine Line { get; init; }
        public Item Item { get; init; }
        public string RevenueAccountNo { get; init; }
        public VatPostingSetup VatSetup { get; init; }
        public decimal CostAmount { get; init; }
        public string CogsAccountNo { get; init; }
        public string InventoryAccountNo { get; init; }
    }

    /// <summary>The accounts, VAT setup and cost of every item and G/L account line.</summary>
    private async Task<List<LinePlan>> PlanLinesAsync(SalesHeader header, Customer customer, bool creditMemo)
    {
        var plans = new List<LinePlan>();
        var automaticCostPosting = (await InventorySetupManager.GetAsync()).AutomaticCostPosting;

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
                plans.Add(new LinePlan { Line = line, RevenueAccountNo = line.No, VatSetup = vatSetup });
                continue;
            }

            var item = await _itemRepository.FirstOrDefaultAsync(i => i.No == line.No)
                ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", line.No);

            var revenueAccountNo = await _postingSetupManager.GetSalesAccountAsync(customer.GenBusPostingGroup, item.GenProdPostingGroup, creditMemo);

            var costAmount = 0m;
            string cogsAccountNo = null, inventoryAccountNo = null;
            if (automaticCostPosting && item.Type == ItemType.Inventory)
            {
                costAmount = Math.Round(line.Quantity * item.UnitCost, 2, MidpointRounding.AwayFromZero);
                if (costAmount != 0m)
                {
                    cogsAccountNo = await _postingSetupManager.GetCogsAccountAsync(customer.GenBusPostingGroup, item.GenProdPostingGroup);
                    inventoryAccountNo = await _postingSetupManager.GetInventoryAccountAsync(item.InventoryPostingGroup, header.LocationCode);
                }
            }

            plans.Add(new LinePlan
            {
                Line = line,
                Item = item,
                RevenueAccountNo = revenueAccountNo,
                VatSetup = vatSetup,
                CostAmount = costAmount,
                CogsAccountNo = cogsAccountNo,
                InventoryAccountNo = inventoryAccountNo,
            });
        }

        return plans;
    }
}
