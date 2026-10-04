import type { CreateUpdatePensionSponsorDto, GetPensionSponsorListInput, PensionSponsorDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionSponsorService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionSponsorDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionSponsorDto>({
      method: 'POST',
      url: '/api/erp/pension-sponsor',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-sponsor/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionSponsorDto>({
      method: 'GET',
      url: `/api/erp/pension-sponsor/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionSponsorListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionSponsorDto>>({
      method: 'GET',
      url: '/api/erp/pension-sponsor',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, schemeCode: input.schemeCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionSponsorDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionSponsorDto>({
      method: 'PUT',
      url: `/api/erp/pension-sponsor/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
