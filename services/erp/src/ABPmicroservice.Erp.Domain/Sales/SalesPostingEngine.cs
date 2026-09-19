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

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Sales Document Posting Engine.
/// Mirrors Business Central Codeunit 80 "Sales-Post".
/// Atomically posts Sales Headers to Posted Sales Invoices, G/L Entries, Customer Ledger Entries, and Item Ledger Entries.
/// </summary>
public class SalesPostingEngine : DomainService
{
    private readonly IRepository<SalesHeader, Guid> _salesHeaderRepository;
    private readonly IRepository<PostedSalesHeader, Guid> _postedSalesHeaderRepository;
    private readonly IRepository<CustomerPostingGroup, Guid> _customerPostingGroupRepository;
    private readonly IRepository<GeneralPostingSetup, Guid> _generalPostingSetupRepository;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly ItemJnlPostLine _itemJnlPostLine;
    private readonly SalesReceivablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;

    public SalesPostingEngine(
        IRepository<SalesHeader, Guid> salesHeaderRepository,
        IRepository<PostedSalesHeader, Guid> postedSalesHeaderRepository,
        IRepository<CustomerPostingGroup, Guid> customerPostingGroupRepository,
        IRepository<GeneralPostingSetup, Guid> generalPostingSetupRepository,
        GenJnlPostLine genJnlPostLine,
        ItemJnlPostLine itemJnlPostLine,
        SalesReceivablesSetupManager setupManager,
        NoSeriesManager noSeriesManager
    )
    {
        _salesHeaderRepository = salesHeaderRepository;
        _postedSalesHeaderRepository = postedSalesHeaderRepository;
        _customerPostingGroupRepository = customerPostingGroupRepository;
        _generalPostingSetupRepository = generalPostingSetupRepository;
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

        // Posted documents get their own number from the posted series; the derived number is the
        // fallback for a company that has not set one up.
        var postedNos = (await _setupManager.GetAsync()).GetPostedDocumentNos(header.DocumentType);
        string postedDocNo = postedNos.IsNullOrWhiteSpace()
            ? $"PSI-{header.No}"
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
            header.DueDate ?? header.PostingDate.AddDays(30),
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

        // 2. Post Customer Ledger Entry (A/R Debit)
        await _genJnlPostLine.PostLineAsync(new GenJournalLine(
            GuidGenerator.Create(),
            Guid.Empty,
            1,
            header.PostingDate,
            GLEntryDocumentType.Invoice,
            postedDocNo,
            "Customer",
            header.SellToCustomerNo,
            $"Sales Invoice {postedDocNo}",
            header.TotalAmountIncludingVat
        ));

        // 3. Post Revenue & Item Ledger entries per line
        foreach (var line in header.Lines)
        {
            if (line.Type == DocumentLineType.Item)
            {
                // Post Item Ledger Entry (Inventory Decrease)
                await _itemJnlPostLine.PostItemEntryAsync(
                    Guid.Empty,
                    line.No,
                    header.PostingDate,
                    ItemLedgerEntryType.Sale,
                    postedDocNo,
                    line.Description,
                    line.Quantity,
                    line.UnitPrice
                );

                // Post Revenue G/L Entry (Credit)
                await _genJnlPostLine.PostGLDirectAsync(
                    "4000", // Standard Sales Revenue Account
                    header.PostingDate,
                    GLEntryDocumentType.Invoice,
                    postedDocNo,
                    line.Description,
                    -line.LineAmount,
                    header.SellToCustomerNo
                );
            }
        }

        // 4. Mark header as posted
        header.MarkPosted(postedDocNo);
        await _salesHeaderRepository.UpdateAsync(header);

        return postedHeader;
    }
}
