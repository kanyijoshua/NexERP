import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ManufacturingSetupDto } from './manufacturing.models';

@Injectable({
  providedIn: 'root',
})
export class ManufacturingSetupService {
  apiName = 'Erp';

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ManufacturingSetupDto>({
      method: 'GET',
      url: '/api/erp/manufacturing-setup',
    },
    { apiName: this.apiName,...config });

  update = (input: ManufacturingSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ManufacturingSetupDto>({
      method: 'PUT',
      url: '/api/erp/manufacturing-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
