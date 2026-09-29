import type { ConfigLineDto, CreateConfigLineDto, MoveConfigLineInput, UpdateConfigLineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ConfigWorksheetService {
  apiName = 'Erp';


  create = (input: CreateConfigLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigLineDto>({
      method: 'POST',
      url: '/api/erp/config-worksheet',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/config-worksheet/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ConfigLineDto>>({
      method: 'GET',
      url: '/api/erp/config-worksheet',
    },
    { apiName: this.apiName,...config });


  move = (id: string, input: MoveConfigLineInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ConfigLineDto>>({
      method: 'POST',
      url: `/api/erp/config-worksheet/${id}/move`,
      body: input,
    },
    { apiName: this.apiName,...config });


  suggestLines = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ConfigLineDto>>({
      method: 'POST',
      url: '/api/erp/config-worksheet/suggest-lines',
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: UpdateConfigLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigLineDto>({
      method: 'PUT',
      url: `/api/erp/config-worksheet/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
