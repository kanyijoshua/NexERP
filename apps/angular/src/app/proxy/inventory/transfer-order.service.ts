import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateTransferHeaderDto, GetTransferListInput, TransferHeaderDto } from './warehouse.models';

@Injectable({
  providedIn: 'root',
})
export class TransferOrderService {
  apiName = 'Erp';

  create = (input: CreateUpdateTransferHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TransferHeaderDto>({
      method: 'POST',
      url: '/api/erp/transfer-order',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/transfer-order/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TransferHeaderDto>({
      method: 'GET',
      url: `/api/erp/transfer-order/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetTransferListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<TransferHeaderDto>>({
      method: 'GET',
      url: '/api/erp/transfer-order',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, completed: input.completed, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateTransferHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TransferHeaderDto>({
      method: 'PUT',
      url: `/api/erp/transfer-order/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  release = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TransferHeaderDto>({
      method: 'POST',
      url: `/api/erp/transfer-order/${id}/release`,
    },
    { apiName: this.apiName,...config });

  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TransferHeaderDto>({
      method: 'POST',
      url: `/api/erp/transfer-order/${id}/reopen`,
    },
    { apiName: this.apiName,...config });

  ship = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TransferHeaderDto>({
      method: 'POST',
      url: `/api/erp/transfer-order/${id}/ship`,
    },
    { apiName: this.apiName,...config });

  receive = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TransferHeaderDto>({
      method: 'POST',
      url: `/api/erp/transfer-order/${id}/receive`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
