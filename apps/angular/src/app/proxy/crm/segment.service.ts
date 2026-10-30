import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ApplySegmentInput, CreateUpdateSegmentDto, GetSegmentListInput, PickSegmentContactsInput, SegmentDto, SegmentResultDto } from './marketing.models';

@Injectable({
  providedIn: 'root',
})
export class SegmentService {
  apiName = 'Erp';

  applyToCampaign = (id: string, input: ApplySegmentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SegmentResultDto>({
      method: 'POST',
      url: `/api/erp/segment/${id}/apply-to-campaign`,
      body: input,
    },
    { apiName: this.apiName,...config });

  create = (input: CreateUpdateSegmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SegmentDto>({
      method: 'POST',
      url: '/api/erp/segment',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/segment/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SegmentDto>({
      method: 'GET',
      url: `/api/erp/segment/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetSegmentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SegmentDto>>({
      method: 'GET',
      url: '/api/erp/segment',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  pickContacts = (id: string, input: PickSegmentContactsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SegmentResultDto>({
      method: 'POST',
      url: `/api/erp/segment/${id}/pick-contacts`,
      body: input,
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateSegmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SegmentDto>({
      method: 'PUT',
      url: `/api/erp/segment/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
