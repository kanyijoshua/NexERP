using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Item Charge (for example FREIGHT, DUTY, INSURANCE) used on purchase and sales lines
/// of type <see cref="DocumentLineType.ChargeItem"/> to allocate landed cost to inventory receipts or shipments.
/// </summary>
public class ItemCharge : CodeTableEntity
{
    public string GenProdPostingGroup { get; private set; }
    public string VatProdPostingGroup { get; private set; }

    protected ItemCharge() { }

    public ItemCharge(
        Guid id,
        string code,
        string description,
        string genProdPostingGroup = null,
        string vatProdPostingGroup = null
    )
        : base(id, code, description)
    {
        SetPostingGroups(genProdPostingGroup, vatProdPostingGroup);
    }

    public void SetPostingGroups(string genProdPostingGroup, string vatProdPostingGroup)
    {
        GenProdPostingGroup = NormalizeCode(Check.Length(genProdPostingGroup, nameof(genProdPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
        VatProdPostingGroup = NormalizeCode(Check.Length(vatProdPostingGroup, nameof(vatProdPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
    }
}

/// <summary>
/// Assignment of a Purchase or Sales Charge(Item) line to a specific <see cref="ItemLedgerEntry"/>
/// (typically a purchase receipt or sales shipment entry) for landed cost allocation.
/// </summary>
public class ItemChargeAssignment : CompanyAggregateRoot
{
    public ItemChargeSourceType SourceType { get; private set; }
    public string SourceDocumentNo { get; private set; }
    public Guid SourceLineId { get; private set; }
    public int SourceLineNo { get; private set; }
    public string ItemChargeNo { get; private set; }

    public Guid AppliesToEntryId { get; private set; }
    public long AppliesToEntryNo { get; private set; }
    public string AppliesToDocumentNo { get; private set; }
    public string ItemNo { get; private set; }
    public string Description { get; private set; }

    public decimal QtyToAssign { get; private set; }
    public decimal QtyAssigned { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal AmountToAssign { get; private set; }
    public bool Posted { get; private set; }

    /// <summary>What the invoice actually added to the entry's cost (or, on a sale, to its revenue), in LCY.</summary>
    public decimal AmountPostedLcy { get; private set; }

    protected ItemChargeAssignment() { }

    public ItemChargeAssignment(
        Guid id,
        ItemChargeSourceType sourceType,
        string sourceDocumentNo,
        Guid sourceLineId,
        int sourceLineNo,
        string itemChargeNo,
        Guid appliesToEntryId,
        long appliesToEntryNo,
        string appliesToDocumentNo,
        string itemNo,
        string description,
        decimal qtyToAssign,
        decimal unitCost,
        decimal amountToAssign
    )
        : base(id)
    {
        SourceType = sourceType;
        SourceDocumentNo = Check.NotNullOrWhiteSpace(sourceDocumentNo, nameof(sourceDocumentNo), ErpDomainConsts.MaxDocumentNoLength);
        SourceLineId = sourceLineId;
        SourceLineNo = sourceLineNo;
        ItemChargeNo = Check.NotNullOrWhiteSpace(itemChargeNo, nameof(itemChargeNo), ErpDomainConsts.MaxNoLength);
        AppliesToEntryId = appliesToEntryId;
        AppliesToEntryNo = appliesToEntryNo;
        AppliesToDocumentNo = Check.Length(appliesToDocumentNo, nameof(appliesToDocumentNo), ErpDomainConsts.MaxDocumentNoLength);
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        SetAllocation(qtyToAssign, unitCost, amountToAssign);
    }

    public void SetAllocation(decimal qtyToAssign, decimal unitCost, decimal amountToAssign)
    {
        if (Posted)
        {
            throw new BusinessException(ErpErrorCodes.ItemCharges.AssignmentAlreadyPosted);
        }

        QtyToAssign = Math.Round(qtyToAssign, 5, MidpointRounding.AwayFromZero);
        UnitCost = Math.Round(unitCost, 5, MidpointRounding.AwayFromZero);
        AmountToAssign = Math.Round(amountToAssign, 2, MidpointRounding.AwayFromZero);
    }

    internal void MarkPosted(decimal amountPostedLcy)
    {
        Posted = true;
        QtyAssigned = QtyToAssign;
        AmountPostedLcy = amountPostedLcy;
    }
}

/// <summary>One assignment as an invoice posted it.</summary>
public sealed record ItemChargePosting(ItemLedgerEntry Entry, decimal AmountLcy);

/// <summary>
/// Allocates and posts Item Charges (landed cost: freight, customs duty, insurance) across
/// receipt/shipment <see cref="ItemLedgerEntry"/> rows: equally, by amount, by weight or by volume.
/// </summary>
public class ItemChargeManager : DomainService
{
    private readonly IRepository<ItemCharge, Guid> _itemChargeRepository;
    private readonly IRepository<ItemChargeAssignment, Guid> _assignmentRepository;
    private readonly IRepository<ItemLedgerEntry, Guid> _itemLedgerEntryRepository;
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<PurchaseLine, Guid> _purchaseLineRepository;
    private readonly IRepository<PurchaseHeader, Guid> _purchaseHeaderRepository;
    private readonly IRepository<SalesLine, Guid> _salesLineRepository;
    private readonly IRepository<SalesHeader, Guid> _salesHeaderRepository;
    private readonly ItemJnlPostLine _itemJnlPostLine;

    public ItemChargeManager(
        IRepository<ItemCharge, Guid> itemChargeRepository,
        IRepository<ItemChargeAssignment, Guid> assignmentRepository,
        IRepository<ItemLedgerEntry, Guid> itemLedgerEntryRepository,
        IRepository<Item, Guid> itemRepository,
        IRepository<PurchaseLine, Guid> purchaseLineRepository,
        IRepository<PurchaseHeader, Guid> purchaseHeaderRepository,
        IRepository<SalesLine, Guid> salesLineRepository,
        IRepository<SalesHeader, Guid> salesHeaderRepository,
        ItemJnlPostLine itemJnlPostLine
    )
    {
        _itemChargeRepository = itemChargeRepository;
        _assignmentRepository = assignmentRepository;
        _itemLedgerEntryRepository = itemLedgerEntryRepository;
        _itemRepository = itemRepository;
        _purchaseLineRepository = purchaseLineRepository;
        _purchaseHeaderRepository = purchaseHeaderRepository;
        _salesLineRepository = salesLineRepository;
        _salesHeaderRepository = salesHeaderRepository;
        _itemJnlPostLine = itemJnlPostLine;
    }

    public async Task<ItemCharge> GetByCodeAsync(string code)
    {
        var normalized = CodeTableEntity.NormalizeCode(code);
        var charge = await _itemChargeRepository.FirstOrDefaultAsync(x => x.Code == normalized);
        if (charge == null)
        {
            throw new BusinessException(ErpErrorCodes.ItemCharges.NotFound).WithData("code", normalized ?? "");
        }
        return charge;
    }

    public async Task<List<ItemChargeAssignment>> SuggestAllocationAsync(
        ItemChargeSourceType sourceType,
        Guid sourceLineId,
        ItemChargeAllocationMethod method,
        IReadOnlyCollection<Guid> targetItemLedgerEntryIds
    )
    {
        if (targetItemLedgerEntryIds == null || targetItemLedgerEntryIds.Count == 0)
        {
            return [];
        }

        var (docNo, lineNo, itemChargeNo, totalQty, totalAmount, unitCost) = await GetSourceLineInfoAsync(sourceType, sourceLineId);

        var existingUnposted = await _assignmentRepository.GetListAsync(x =>
            x.SourceType == sourceType && x.SourceLineId == sourceLineId && !x.Posted);
        foreach (var old in existingUnposted)
        {
            await _assignmentRepository.DeleteAsync(old, autoSave: true);
        }

        var alreadyPosted = await _assignmentRepository.GetListAsync(x =>
            x.SourceType == sourceType && x.SourceLineId == sourceLineId && x.Posted);
        var remainingQty = Math.Max(0m, totalQty - alreadyPosted.Sum(x => x.QtyAssigned));
        var remainingAmount = Math.Max(0m, totalAmount - alreadyPosted.Sum(x => x.AmountToAssign));

        if (remainingAmount <= 0m || remainingQty <= 0m)
        {
            throw new BusinessException(ErpErrorCodes.ItemCharges.QuantityExceeded)
                .WithData("itemChargeNo", itemChargeNo)
                .WithData("quantity", totalQty)
                .WithData("assigned", alreadyPosted.Sum(x => x.QtyAssigned));
        }

        var entries = (await _itemLedgerEntryRepository.GetListAsync(e => targetItemLedgerEntryIds.Contains(e.Id)))
            .OrderBy(e => e.EntryNo)
            .ToList();
        if (entries.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.ItemCharges.TargetEntryNotFound);
        }

        var itemIds = entries.Select(e => e.ItemId).Distinct().ToList();
        var items = (await _itemRepository.GetListAsync(i => itemIds.Contains(i.Id)))
            .ToDictionary(i => i.Id);

        var weights = entries.Select<ItemLedgerEntry, (ItemLedgerEntry Entry, Item Item, decimal Weight)>(entry =>
        {
            items.TryGetValue(entry.ItemId, out var item);
            var absQty = Math.Abs(entry.Quantity);
            var weight = method switch
            {
                ItemChargeAllocationMethod.ByAmount => Math.Abs(entry.CostAmountActual) > 0m
                    ? Math.Abs(entry.CostAmountActual)
                    : absQty * Math.Abs(entry.UnitCost),
                ItemChargeAllocationMethod.ByWeight => absQty * (item?.NetWeight > 0m ? item.NetWeight : 0m),
                ItemChargeAllocationMethod.ByVolume => absQty * (item?.UnitVolume > 0m ? item.UnitVolume : 0m),
                _ => 1m,
            };
            return (Entry: entry, Item: item, Weight: weight);
        }).ToList();

        var totalWeight = weights.Sum(w => w.Weight);
        if (totalWeight <= 0m)
        {
            weights = weights.Select(w => (w.Entry, w.Item, Weight: 1m)).ToList();
            totalWeight = weights.Count;
        }

        var result = new List<ItemChargeAssignment>();
        var qtyLeft = remainingQty;
        var amountLeft = remainingAmount;

        for (var i = 0; i < weights.Count; i++)
        {
            var (entry, item, weight) = weights[i];
            var isLast = i == weights.Count - 1;
            var ratio = weight / totalWeight;

            var shareQty = isLast
                ? qtyLeft
                : Math.Round(remainingQty * ratio, 5, MidpointRounding.AwayFromZero);
            var shareAmount = isLast
                ? amountLeft
                : Math.Round(remainingAmount * ratio, 2, MidpointRounding.AwayFromZero);

            qtyLeft -= shareQty;
            amountLeft -= shareAmount;

            var assignment = new ItemChargeAssignment(
                GuidGenerator.Create(),
                sourceType,
                docNo,
                sourceLineId,
                lineNo,
                itemChargeNo,
                entry.Id,
                entry.EntryNo,
                entry.DocumentNo,
                entry.ItemNo,
                item?.Description ?? entry.ItemNo,
                shareQty,
                unitCost,
                shareAmount
            );

            await _assignmentRepository.InsertAsync(assignment, autoSave: true);
            result.Add(assignment);
        }

        return result;
    }

    /// <summary>
    /// Assigns part of a charge line to one receipt or shipment entry by hand. The document, line and
    /// item details are taken from the line and the entry, never from the caller.
    /// </summary>
    public async Task<ItemChargeAssignment> AssignAsync(
        ItemChargeSourceType sourceType,
        Guid sourceLineId,
        Guid itemLedgerEntryId,
        decimal qtyToAssign,
        decimal? amountToAssign = null
    )
    {
        var (docNo, lineNo, itemChargeNo, _, _, unitCost) = await GetSourceLineInfoAsync(sourceType, sourceLineId);
        var entry = await _itemLedgerEntryRepository.FindAsync(itemLedgerEntryId)
            ?? throw new BusinessException(ErpErrorCodes.ItemCharges.TargetEntryNotFound);
        var item = await _itemRepository.FindAsync(entry.ItemId);
        await EnsureWithinLineAsync(sourceType, sourceLineId, qtyToAssign, exceptId: null);

        var assignment = new ItemChargeAssignment(
            GuidGenerator.Create(),
            sourceType,
            docNo,
            sourceLineId,
            lineNo,
            itemChargeNo,
            entry.Id,
            entry.EntryNo,
            entry.DocumentNo,
            entry.ItemNo,
            item?.Description ?? entry.ItemNo,
            qtyToAssign,
            unitCost,
            amountToAssign ?? qtyToAssign * unitCost
        );

        await _assignmentRepository.InsertAsync(assignment, autoSave: true);
        return assignment;
    }

    /// <summary>An assignment's quantity changed: the line may still not be over-assigned.</summary>
    public async Task UpdateAssignmentAsync(ItemChargeAssignment assignment, decimal qtyToAssign, decimal? amountToAssign)
    {
        await EnsureWithinLineAsync(assignment.SourceType, assignment.SourceLineId, qtyToAssign, exceptId: assignment.Id);
        assignment.SetAllocation(qtyToAssign, assignment.UnitCost, amountToAssign ?? qtyToAssign * assignment.UnitCost);
        await _assignmentRepository.UpdateAsync(assignment, autoSave: true);
    }

    /// <summary>
    /// A charge line can only be invoiced once the quantity being invoiced is assigned in full,
    /// so the cost it adds lands on stock instead of disappearing into an expense account.
    /// Checked before anything of the document is posted.
    /// </summary>
    public async Task EnsureFullyAssignedAsync(ItemChargeSourceType sourceType, Guid sourceLineId, decimal quantityToInvoice)
    {
        if (quantityToInvoice == 0m)
        {
            return;
        }

        var assigned = (await _assignmentRepository.GetListAsync(x =>
            x.SourceType == sourceType && x.SourceLineId == sourceLineId && !x.Posted)).Sum(x => x.QtyToAssign);

        if (Math.Abs(assigned - Math.Abs(quantityToInvoice)) > QuantityTolerance)
        {
            var (docNo, lineNo, itemChargeNo, _, _, _) = await GetSourceLineInfoAsync(sourceType, sourceLineId);
            throw new BusinessException(ErpErrorCodes.ItemCharges.NotFullyAssigned)
                .WithData("itemChargeNo", itemChargeNo)
                .WithData("documentNo", docNo)
                .WithData("lineNo", lineNo)
                .WithData("quantity", Math.Abs(quantityToInvoice))
                .WithData("assigned", assigned);
        }
    }

    /// <summary>
    /// Posts the open assignments of a charge line that is being invoiced: <paramref name="amountLcy"/>
    /// (the line's invoiced amount in LCY, negative on a credit memo) is split over them by quantity,
    /// the last one taking the rounding. A purchase charge adds cost to each receipt entry; a sales
    /// charge is revenue and changes no cost, so its assignments are only marked as posted.
    /// </summary>
    public async Task<List<ItemChargePosting>> PostAssignmentsAsync(
        ItemChargeSourceType sourceType,
        Guid sourceLineId,
        decimal quantityInvoiced,
        decimal amountLcy,
        string documentNo,
        DateTime postingDate
    )
    {
        await EnsureFullyAssignedAsync(sourceType, sourceLineId, quantityInvoiced);

        var open = (await _assignmentRepository.GetListAsync(x =>
                x.SourceType == sourceType && x.SourceLineId == sourceLineId && !x.Posted))
            .OrderBy(x => x.AppliesToEntryNo)
            .ToList();
        var result = new List<ItemChargePosting>();
        if (open.Count == 0)
        {
            return result;
        }

        var totalQty = open.Sum(x => x.QtyToAssign);
        var amountLeft = amountLcy;
        for (var i = 0; i < open.Count; i++)
        {
            var assignment = open[i];
            var share = i == open.Count - 1
                ? amountLeft
                : Math.Round(amountLcy * assignment.QtyToAssign / totalQty, 2, MidpointRounding.AwayFromZero);
            amountLeft -= share;

            var entry = await _itemLedgerEntryRepository.FindAsync(assignment.AppliesToEntryId)
                ?? throw new BusinessException(ErpErrorCodes.ItemCharges.TargetEntryNotFound);

            if (sourceType == ItemChargeSourceType.PurchaseLine && share != 0m)
            {
                await _itemJnlPostLine.PostLandedCostAsync(entry, share, documentNo, postingDate);
            }

            assignment.MarkPosted(share);
            await _assignmentRepository.UpdateAsync(assignment, autoSave: true);
            result.Add(new ItemChargePosting(entry, share));
        }

        return result;
    }

    private const decimal QuantityTolerance = 0.00001m;

    /// <summary>Checked before writing: the line may not end up assigned more than its quantity.</summary>
    private async Task EnsureWithinLineAsync(ItemChargeSourceType sourceType, Guid sourceLineId, decimal qtyToAssign, Guid? exceptId)
    {
        var (_, _, itemChargeNo, quantity, _, _) = await GetSourceLineInfoAsync(sourceType, sourceLineId);
        var assigned = qtyToAssign + (await _assignmentRepository.GetListAsync(x =>
                x.SourceType == sourceType && x.SourceLineId == sourceLineId && x.Id != exceptId))
            .Sum(x => x.Posted ? x.QtyAssigned : x.QtyToAssign);
        if (assigned > quantity + QuantityTolerance)
        {
            throw new BusinessException(ErpErrorCodes.ItemCharges.QuantityExceeded)
                .WithData("itemChargeNo", itemChargeNo)
                .WithData("quantity", quantity)
                .WithData("assigned", assigned);
        }
    }

    private async Task<(string DocNo, int LineNo, string ItemChargeNo, decimal Quantity, decimal LineAmount, decimal UnitCost)> GetSourceLineInfoAsync(
        ItemChargeSourceType sourceType,
        Guid sourceLineId
    )
    {
        if (sourceType == ItemChargeSourceType.PurchaseLine)
        {
            var line = await _purchaseLineRepository.GetAsync(sourceLineId);
            if (line.Type != DocumentLineType.ChargeItem)
            {
                throw new BusinessException(ErpErrorCodes.ItemCharges.LineMustBeChargeItem)
                    .WithData("lineNo", line.LineNo);
            }
            var header = await _purchaseHeaderRepository.GetAsync(line.PurchaseHeaderId);
            return (header.No, line.LineNo, line.No, Math.Abs(line.Quantity), Math.Abs(line.LineAmount), line.DirectUnitCost);
        }
        else
        {
            var line = await _salesLineRepository.GetAsync(sourceLineId);
            if (line.Type != DocumentLineType.ChargeItem)
            {
                throw new BusinessException(ErpErrorCodes.ItemCharges.LineMustBeChargeItem)
                    .WithData("lineNo", line.LineNo);
            }
            var header = await _salesHeaderRepository.GetAsync(line.SalesHeaderId);
            return (header.No, line.LineNo, line.No, Math.Abs(line.Quantity), Math.Abs(line.LineAmount), line.UnitPrice);
        }
    }
}
