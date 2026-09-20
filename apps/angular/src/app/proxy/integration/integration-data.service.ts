import type { IntegrationQueryInput, IntegrationQueryResultDto, PublishedWebServiceDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ExportableFieldDto } from '../exporting/models';

@Injectable({
  providedIn: 'root',
})
export class IntegrationDataService {
  apiName = 'Erp';
  

  getFields = (serviceName: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ExportableFieldDto>>({
      method: 'GET',
      url: '/api/erp/integration-data/fields',
      params: { serviceName },
    },
    { apiName: this.apiName,...config });
  

  getServices = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<PublishedWebServiceDto>>({
      method: 'GET',
      url: '/api/erp/integration-data/services',
    },
    { apiName: this.apiName,...config });
  

  query = (input: IntegrationQueryInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IntegrationQueryResultDto>({
      method: 'POST',
      url: '/api/erp/integration-data/query',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
