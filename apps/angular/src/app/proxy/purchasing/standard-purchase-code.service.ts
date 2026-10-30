import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateStandardPurchaseCodeDto, StandardPurchaseCodeDto } from './standard-codes.models';

@Injectable({
  providedIn: 'root',
})
export class StandardPurchaseCodeService {
  apiName = 'Erp';

  create = (input: CreateUpdateStandardPurchaseCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardPurchaseCodeDto>({
      method: 'POST',
      url: '/api/erp/standard-purchase-code',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/standard-purchase-code/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardPurchaseCodeDto>({
      method: 'GET',
      url: `/api/erp/standard-purchase-code/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StandardPurchaseCodeDto>>({
      method: 'GET',
      url: '/api/erp/standard-purchase-code',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateStandardPurchaseCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardPurchaseCodeDto>({
      method: 'PUT',
      url: `/api/erp/standard-purchase-code/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
