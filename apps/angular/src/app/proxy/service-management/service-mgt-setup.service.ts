import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ServiceMgtSetupDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class ServiceMgtSetupService {
  apiName = 'Erp';

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceMgtSetupDto>({
      method: 'GET',
      url: '/api/erp/service-mgt-setup',
    },
    { apiName: this.apiName,...config });

  update = (input: ServiceMgtSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceMgtSetupDto>({
      method: 'PUT',
      url: '/api/erp/service-mgt-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
