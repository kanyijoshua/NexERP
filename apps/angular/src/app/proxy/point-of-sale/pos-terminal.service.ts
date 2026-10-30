import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdatePosTerminalDto, PosTerminalDto } from './point-of-sale.models';

@Injectable({
  providedIn: 'root',
})
export class PosTerminalService {
  apiName = 'Erp';

  create = (input: CreateUpdatePosTerminalDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosTerminalDto>({
      method: 'POST',
      url: '/api/erp/pos-terminal',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pos-terminal/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosTerminalDto>({
      method: 'GET',
      url: `/api/erp/pos-terminal/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PosTerminalDto>>({
      method: 'GET',
      url: '/api/erp/pos-terminal',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePosTerminalDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosTerminalDto>({
      method: 'PUT',
      url: `/api/erp/pos-terminal/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
