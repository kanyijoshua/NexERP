using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace ABPmicroservice.Erp.Inventory;

[Authorize(ErpPermissions.InventoryCosting.Default)]
public class CostAdjustmentAppService : ErpAppService, ICostAdjustmentAppService
{
    private readonly CostAdjustmentManager _manager;

    public CostAdjustmentAppService(CostAdjustmentManager manager)
    {
        _manager = manager;
    }

    public async Task<CostAdjustmentResultDto> AdjustCostAsync(AdjustCostInput input)
    {
        var itemNos = (input?.ItemNos ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return Map(await _manager.AdjustCostAsync(itemNos, input?.PostingDate ?? Clock.Now.Date));
    }

    public async Task<CostAdjustmentResultDto> RunInventoryCostPostingAsync(PostInventoryCostInput input) =>
        Map(await _manager.PostInventoryCostToGLAsync(input?.PostingDate ?? Clock.Now.Date, input?.DocumentNo?.Trim()));

    private static CostAdjustmentResultDto Map(CostAdjustmentResult result) => new()
    {
        AdjustedEntries = result.AdjustedEntries,
        PostedValueEntries = result.PostedValueEntries,
        Amount = result.Amount,
        SkippedAverageCostItems = result.SkippedAverageCostItems,
    };
}
