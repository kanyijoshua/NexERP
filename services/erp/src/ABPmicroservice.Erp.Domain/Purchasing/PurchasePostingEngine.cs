using System;
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
/// Mirrors Business Central Codeunit 90 "Purch.-Post".
/// Atomically posts Purchase Headers to Posted Purchase Invoices, G/L Entries, Vendor Ledger Entries, and Item Ledger Entries.
/// </summary>
public class PurchasePostingEngine : DomainService
{
    private readonly IRepository<PurchaseHeader, Guid> _purchaseHeaderRepository;
    private readonly IRepository<PostedPurchaseHeader, Guid> _postedPurchaseHeaderRepository;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly ItemJnlPostLine _itemJnlPostLine;
    private readonly PurchasesPayablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;

    public PurchasePostingEngine(
        IRepository<PurchaseHeader, Guid> purchaseHeaderRepository,
        IRepository<PostedPurchaseHeader, Guid> postedPurchaseHeaderRepository,
        GenJnlPostLine genJnlPostLine,
        ItemJnlPostLine itemJnlPostLine,
        PurchasesPayablesSetupManager setupManager,
        NoSeriesManager noSeriesManager
    )
    {
        _purchaseHeaderRepository = purchaseHeaderRepository;
        _postedPurchaseHeaderRepository = postedPurchaseHeaderRepository;
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

        // Posted documents get their own number from the posted series; the derived number is the
        // fallback for a company that has not set one up.
        var postedNos = (await _setupManager.GetAsync()).GetPostedDocumentNos(header.DocumentType);
        string postedDocNo = postedNos.IsNullOrWhiteSpace()
            ? $"PPI-{header.No}"
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
            header.DueDate ?? header.PostingDate.AddDays(30),
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

        // 2. Post Vendor Ledger Entry (A/P Credit)
        await _genJnlPostLine.PostLineAsync(new GenJournalLine(
            GuidGenerator.Create(),
            Guid.Empty,
            1,
            header.PostingDate,
            GLEntryDocumentType.Invoice,
            postedDocNo,
            "Vendor",
            header.BuyFromVendorNo,
            $"Purchase Invoice {postedDocNo}",
            -header.TotalAmountIncludingVat
        ));

        // 3. Post Inventory / Cost & G/L entries per line
        foreach (var line in header.Lines)
        {
            if (line.Type == DocumentLineType.Item)
            {
                // Post Item Ledger Entry (Inventory Increase)
                await _itemJnlPostLine.PostItemEntryAsync(
                    Guid.Empty,
                    line.No,
                    header.PostingDate,
                    ItemLedgerEntryType.Purchase,
                    postedDocNo,
                    line.Description,
                    line.Quantity,
                    line.DirectUnitCost
                );

                // Post Direct Inventory G/L Entry (Debit)
                await _genJnlPostLine.PostGLDirectAsync(
                    "1400", // Standard Inventory Account
                    header.PostingDate,
                    GLEntryDocumentType.Invoice,
                    postedDocNo,
                    line.Description,
                    line.LineAmount,
                    header.BuyFromVendorNo
                );
            }
        }

        // 4. Mark header as posted
        header.MarkPosted(postedDocNo);
        await _purchaseHeaderRepository.UpdateAsync(header);

        return postedHeader;
    }
}
