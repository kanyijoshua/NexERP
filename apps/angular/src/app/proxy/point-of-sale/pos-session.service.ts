import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ClosePosSessionInput, GetPosSessionListInput, OpenPosSessionInput, PosSessionDto } from './point-of-sale.models';

@Injectable({
  providedIn: 'root',
})
export class PosSessionService {
  apiName = 'Erp';

  close = (id: string, input: ClosePosSessionInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosSessionDto>({
      method: 'POST',
      url: `/api/erp/pos-session/${id}/close`,
      body: input,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosSessionDto>({
      method: 'GET',
      url: `/api/erp/pos-session/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPosSessionListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PosSessionDto>>({
      method: 'GET',
      url: '/api/erp/pos-session',
      params: { dynamicFilter: input.dynamicFilter, terminalCode: input.terminalCode, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  open = (input: OpenPosSessionInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosSessionDto>({
      method: 'POST',
      url: '/api/erp/pos-session/open',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
