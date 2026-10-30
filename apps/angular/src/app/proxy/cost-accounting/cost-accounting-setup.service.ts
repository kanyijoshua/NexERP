import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CostAccountingSetupDto } from './cost-accounting.models';

@Injectable({
  providedIn: 'root',
})
export class CostAccountingSetupService {
  apiName = 'Erp';

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostAccountingSetupDto>({
      method: 'GET',
      url: '/api/erp/cost-accounting-setup',
    },
    { apiName: this.apiName,...config });

  update = (input: CostAccountingSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostAccountingSetupDto>({
      method: 'PUT',
      url: '/api/erp/cost-accounting-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
