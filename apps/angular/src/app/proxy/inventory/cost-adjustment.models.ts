export interface AdjustCostInput {
  itemNos?: string;
  postingDate?: string;
}

export interface PostInventoryCostInput {
  postingDate?: string;
  documentNo?: string;
}

export interface CostAdjustmentResultDto {
  adjustedEntries: number;
  postedValueEntries: number;
  amount: number;
  skippedAverageCostItems: number;
}
