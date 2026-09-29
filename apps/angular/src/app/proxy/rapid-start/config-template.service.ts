import type { ConfigTemplateDto, CreateUpdateConfigTemplateDto, GetConfigTemplatesInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ConfigTemplateService {
  apiName = 'Erp';


  create = (input: CreateUpdateConfigTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigTemplateDto>({
      method: 'POST',
      url: '/api/erp/config-template',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/config-template/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigTemplateDto>({
      method: 'GET',
      url: `/api/erp/config-template/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetConfigTemplatesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ConfigTemplateDto>>({
      method: 'GET',
      url: '/api/erp/config-template',
      params: { entityName: input.entityName },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateConfigTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigTemplateDto>({
      method: 'PUT',
      url: `/api/erp/config-template/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
