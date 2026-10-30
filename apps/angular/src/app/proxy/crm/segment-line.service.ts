import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetSegmentLineListInput, SegmentLineDto } from './marketing.models';

@Injectable({
  providedIn: 'root',
})
export class SegmentLineService {
  apiName = 'Erp';

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/segment-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SegmentLineDto>({
      method: 'GET',
      url: `/api/erp/segment-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetSegmentLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SegmentLineDto>>({
      method: 'GET',
      url: '/api/erp/segment-line',
      params: { dynamicFilter: input.dynamicFilter, segmentId: input.segmentId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
