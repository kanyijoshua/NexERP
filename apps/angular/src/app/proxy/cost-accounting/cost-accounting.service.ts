import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { AllocateCostsInput, CostBalanceDto, CostRegisterDto, GetCostBalancesInput } from './cost-accounting.models';

@Injectable({
  providedIn: 'root',
})
export class CostAccountingService {
  apiName = 'Erp';

  allocate = (input: AllocateCostsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostRegisterDto>({
      method: 'POST',
      url: '/api/erp/cost-accounting/allocate',
      body: input,
    },
    { apiName: this.apiName,...config });

  getBalances = (input: GetCostBalancesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostBalanceDto[]>({
      method: 'GET',
      url: '/api/erp/cost-accounting/balances',
      params: { fromDate: input.fromDate, toDate: input.toDate, byCostObject: input.byCostObject },
    },
    { apiName: this.apiName,...config });

  transferFromGeneralLedger = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostRegisterDto>({
      method: 'POST',
      url: '/api/erp/cost-accounting/transfer-from-general-ledger',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
