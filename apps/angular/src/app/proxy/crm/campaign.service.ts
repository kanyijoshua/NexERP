import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CampaignDto, CreateUpdateCampaignDto } from './crm.models';

@Injectable({
  providedIn: 'root',
})
export class CampaignService {
  apiName = 'Erp';

  create = (input: CreateUpdateCampaignDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CampaignDto>({
      method: 'POST',
      url: '/api/erp/campaign',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/campaign/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CampaignDto>({
      method: 'GET',
      url: `/api/erp/campaign/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CampaignDto>>({
      method: 'GET',
      url: '/api/erp/campaign',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCampaignDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CampaignDto>({
      method: 'PUT',
      url: `/api/erp/campaign/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
