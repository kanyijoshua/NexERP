import type { CashManagementSetupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CashManagementSetupService {
  apiName = 'Erp';


  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashManagementSetupDto>({
      method: 'GET',
      url: '/api/erp/cash-management-setup',
    },
    { apiName: this.apiName,...config });


  update = (input: CashManagementSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashManagementSetupDto>({
      method: 'PUT',
      url: '/api/erp/cash-management-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
