import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateTransferLineDto, GetTransferLineListInput, TransferLineDto } from './warehouse.models';

@Injectable({
  providedIn: 'root',
})
export class TransferLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateTransferLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TransferLineDto>({
      method: 'POST',
      url: '/api/erp/transfer-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/transfer-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TransferLineDto>({
      method: 'GET',
      url: `/api/erp/transfer-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetTransferLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<TransferLineDto>>({
      method: 'GET',
      url: '/api/erp/transfer-line',
      params: { dynamicFilter: input.dynamicFilter, transferHeaderId: input.transferHeaderId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateTransferLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TransferLineDto>({
      method: 'PUT',
      url: `/api/erp/transfer-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
