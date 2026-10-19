import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CampaignTargetDto, CreateUpdateCampaignTargetDto, GetCampaignTargetListInput } from './crm.models';

@Injectable({
  providedIn: 'root',
})
export class CampaignTargetService {
  apiName = 'Erp';

  create = (input: CreateUpdateCampaignTargetDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CampaignTargetDto>({
      method: 'POST',
      url: '/api/erp/campaign-target',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/campaign-target/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CampaignTargetDto>({
      method: 'GET',
      url: `/api/erp/campaign-target/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCampaignTargetListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CampaignTargetDto>>({
      method: 'GET',
      url: '/api/erp/campaign-target',
      params: { campaignNo: input.campaignNo, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCampaignTargetDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CampaignTargetDto>({
      method: 'PUT',
      url: `/api/erp/campaign-target/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
