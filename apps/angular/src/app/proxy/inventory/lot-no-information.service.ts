import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateLotNoInformationDto, GetItemChildListInput, LotNoInformationDto } from './warehouse.models';

@Injectable({
  providedIn: 'root',
})
export class LotNoInformationService {
  apiName = 'Erp';

  create = (input: CreateUpdateLotNoInformationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LotNoInformationDto>({
      method: 'POST',
      url: '/api/erp/lot-no-information',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/lot-no-information/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LotNoInformationDto>({
      method: 'GET',
      url: `/api/erp/lot-no-information/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetItemChildListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LotNoInformationDto>>({
      method: 'GET',
      url: '/api/erp/lot-no-information',
      params: { dynamicFilter: input.dynamicFilter, itemNo: input.itemNo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateLotNoInformationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LotNoInformationDto>({
      method: 'PUT',
      url: `/api/erp/lot-no-information/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
