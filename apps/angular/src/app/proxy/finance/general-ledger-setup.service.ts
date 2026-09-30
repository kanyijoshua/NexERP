import type { GeneralLedgerSetupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class GeneralLedgerSetupService {
  apiName = 'Erp';
  

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, GeneralLedgerSetupDto>({
      method: 'GET',
      url: '/api/erp/general-ledger-setup',
    },
    { apiName: this.apiName,...config });
  

  update = (input: GeneralLedgerSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GeneralLedgerSetupDto>({
      method: 'PUT',
      url: '/api/erp/general-ledger-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
