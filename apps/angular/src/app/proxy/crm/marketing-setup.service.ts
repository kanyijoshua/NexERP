import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { MarketingSetupDto } from './crm.models';

@Injectable({
  providedIn: 'root',
})
export class MarketingSetupService {
  apiName = 'Erp';

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, MarketingSetupDto>({
      method: 'GET',
      url: '/api/erp/marketing-setup',
    },
    { apiName: this.apiName,...config });

  update = (input: MarketingSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MarketingSetupDto>({
      method: 'PUT',
      url: '/api/erp/marketing-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
