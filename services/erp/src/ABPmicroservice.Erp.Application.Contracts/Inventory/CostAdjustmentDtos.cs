using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Inventory;

public class AdjustCostInput
{
    /// <summary>Item numbers separated by commas; blank adjusts every item.</summary>
    [StringLength(1000)]
    public string ItemNos { get; set; }

    public DateTime? PostingDate { get; set; }
}

public class PostInventoryCostInput
{
    public DateTime? PostingDate { get; set; }

    /// <summary>One document no. for every G/L entry; blank keeps each value entry's own.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }
}

public class CostAdjustmentResultDto
{
    public int AdjustedEntries { get; set; }
    public int PostedValueEntries { get; set; }
    public decimal Amount { get; set; }
    public int SkippedAverageCostItems { get; set; }
}

public interface ICostAdjustmentAppService : IApplicationService
{
    Task<CostAdjustmentResultDto> AdjustCostAsync(AdjustCostInput input);

    /// <summary>Posts the inventory cost not in the G/L yet. Routed as POST .../run-inventory-cost-posting.</summary>
    Task<CostAdjustmentResultDto> RunInventoryCostPostingAsync(PostInventoryCostInput input);
}
