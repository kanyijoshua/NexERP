import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateStandardPurchaseLineDto, GetStandardPurchaseLineListInput, StandardPurchaseLineDto } from './standard-codes.models';

@Injectable({
  providedIn: 'root',
})
export class StandardPurchaseLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateStandardPurchaseLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardPurchaseLineDto>({
      method: 'POST',
      url: '/api/erp/standard-purchase-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/standard-purchase-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardPurchaseLineDto>({
      method: 'GET',
      url: `/api/erp/standard-purchase-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetStandardPurchaseLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StandardPurchaseLineDto>>({
      method: 'GET',
      url: '/api/erp/standard-purchase-line',
      params: { dynamicFilter: input.dynamicFilter, standardPurchaseCodeId: input.standardPurchaseCodeId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateStandardPurchaseLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardPurchaseLineDto>({
      method: 'PUT',
      url: `/api/erp/standard-purchase-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
