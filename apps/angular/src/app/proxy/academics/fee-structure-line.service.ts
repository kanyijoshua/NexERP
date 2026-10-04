import type { CreateUpdateFeeStructureLineDto, FeeStructureLineDto, GetProgrammeTableListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FeeStructureLineService {
  apiName = 'Erp';


  create = (input: CreateUpdateFeeStructureLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeStructureLineDto>({
      method: 'POST',
      url: '/api/erp/fee-structure-line',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/fee-structure-line/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeStructureLineDto>({
      method: 'GET',
      url: `/api/erp/fee-structure-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetProgrammeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FeeStructureLineDto>>({
      method: 'GET',
      url: '/api/erp/fee-structure-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, programmeCode: input.programmeCode, stageCode: input.stageCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateFeeStructureLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeStructureLineDto>({
      method: 'PUT',
      url: `/api/erp/fee-structure-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
