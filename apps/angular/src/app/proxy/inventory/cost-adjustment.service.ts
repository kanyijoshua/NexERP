import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { AdjustCostInput, CostAdjustmentResultDto, PostInventoryCostInput } from './cost-adjustment.models';

@Injectable({
  providedIn: 'root',
})
export class CostAdjustmentService {
  apiName = 'Erp';

  adjustCost = (input: AdjustCostInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostAdjustmentResultDto>({
      method: 'POST',
      url: '/api/erp/cost-adjustment/adjust-cost',
      body: input,
    },
    { apiName: this.apiName,...config });

  runInventoryCostPosting = (input: PostInventoryCostInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostAdjustmentResultDto>({
      method: 'POST',
      url: '/api/erp/cost-adjustment/run-inventory-cost-posting',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
