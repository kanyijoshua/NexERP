using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Posts the cost of stock moves that do not come from a sales or purchase document to the G/L:
/// adjustments and revaluations against the Inventory Adjmt. Account, and moves between locations
/// whose inventory accounts differ. Nothing is posted without automatic cost posting.
/// </summary>
public class InventoryCostPoster : DomainService
{
    private readonly PostingSetupManager _postingSetupManager;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly InventorySetupManager _setupManager;

    public InventoryCostPoster(PostingSetupManager postingSetupManager, GenJnlPostLine genJnlPostLine, InventorySetupManager setupManager)
    {
        _postingSetupManager = postingSetupManager;
        _genJnlPostLine = genJnlPostLine;
        _setupManager = setupManager;
    }

    public async Task<bool> IsAutomaticAsync() => (await _setupManager.GetAsync()).AutomaticCostPosting;

    /// <summary>
    /// The stock of <paramref name="item"/> at <paramref name="locationCode"/> went up in value by
    /// <paramref name="amount"/> (down when negative): inventory against the adjustment account.
    /// </summary>
    public async Task PostAdjustmentAsync(Item item, string locationCode, decimal amount, DateTime postingDate, string documentNo, string description, GLPostingContext context)
    {
        if (amount == 0m || item.Type != ItemType.Inventory || !await IsAutomaticAsync())
        {
            return;
        }

        var inventoryAccountNo = await _postingSetupManager.GetInventoryAccountAsync(item.InventoryPostingGroup, locationCode);
        var adjustmentAccountNo = await _postingSetupManager.GetInventoryAdjmtAccountAsync(item.GenProdPostingGroup);

        await _genJnlPostLine.PostGLDirectAsync(inventoryAccountNo, postingDate, GLEntryDocumentType.None, documentNo, description, amount, item.No, context: context);
        await _genJnlPostLine.PostGLDirectAsync(adjustmentAccountNo, postingDate, GLEntryDocumentType.None, documentNo, description, -amount, item.No, context: context);
    }

    /// <summary>Stock worth <paramref name="amount"/> moved between locations: posted only when their inventory accounts differ.</summary>
    public async Task PostTransferAsync(Item item, string fromLocationCode, string toLocationCode, decimal amount, DateTime postingDate, string documentNo, string description, GLPostingContext context)
    {
        if (amount == 0m || item.Type != ItemType.Inventory || !await IsAutomaticAsync())
        {
            return;
        }

        var fromAccountNo = await _postingSetupManager.GetInventoryAccountAsync(item.InventoryPostingGroup, fromLocationCode);
        var toAccountNo = await _postingSetupManager.GetInventoryAccountAsync(item.InventoryPostingGroup, toLocationCode);
        if (fromAccountNo == toAccountNo)
        {
            return;
        }

        await _genJnlPostLine.PostGLDirectAsync(toAccountNo, postingDate, GLEntryDocumentType.None, documentNo, description, amount, item.No, context: context);
        await _genJnlPostLine.PostGLDirectAsync(fromAccountNo, postingDate, GLEntryDocumentType.None, documentNo, description, -amount, item.No, context: context);
    }
}
