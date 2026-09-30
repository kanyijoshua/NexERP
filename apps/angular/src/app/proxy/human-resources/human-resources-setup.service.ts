import type { HumanResourcesSetupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class HumanResourcesSetupService {
  apiName = 'Erp';


  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, HumanResourcesSetupDto>({
      method: 'GET',
      url: '/api/erp/human-resources-setup',
    },
    { apiName: this.apiName,...config });


  update = (input: HumanResourcesSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HumanResourcesSetupDto>({
      method: 'PUT',
      url: '/api/erp/human-resources-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
