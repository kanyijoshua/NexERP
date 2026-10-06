import type { CreateUpdatePensionVestingScaleDto, GetPensionVestingScaleListInput, PensionVestingScaleDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionVestingScaleService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionVestingScaleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionVestingScaleDto>({
      method: 'POST',
      url: '/api/erp/pension-vesting-scale',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-vesting-scale/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionVestingScaleDto>({
      method: 'GET',
      url: `/api/erp/pension-vesting-scale/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionVestingScaleListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionVestingScaleDto>>({
      method: 'GET',
      url: '/api/erp/pension-vesting-scale',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sponsorNo: input.sponsorNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionVestingScaleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionVestingScaleDto>({
      method: 'PUT',
      url: `/api/erp/pension-vesting-scale/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
