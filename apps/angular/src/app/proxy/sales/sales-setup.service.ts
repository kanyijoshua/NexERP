import type { SalesReceivablesSetupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SalesSetupService {
  apiName = 'Erp';
  

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesReceivablesSetupDto>({
      method: 'GET',
      url: '/api/erp/sales-setup',
    },
    { apiName: this.apiName,...config });
  

  update = (input: SalesReceivablesSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesReceivablesSetupDto>({
      method: 'PUT',
      url: '/api/erp/sales-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
