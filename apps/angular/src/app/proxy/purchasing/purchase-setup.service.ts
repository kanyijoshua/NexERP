import type { PurchasesPayablesSetupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PurchaseSetupService {
  apiName = 'Erp';
  

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchasesPayablesSetupDto>({
      method: 'GET',
      url: '/api/erp/purchase-setup',
    },
    { apiName: this.apiName,...config });
  

  update = (input: PurchasesPayablesSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchasesPayablesSetupDto>({
      method: 'PUT',
      url: '/api/erp/purchase-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
