import type { CreateUpdatePensionerSuspensionReasonDto, PensionerSuspensionReasonDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionerSuspensionReasonService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionerSuspensionReasonDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerSuspensionReasonDto>({
      method: 'POST',
      url: '/api/erp/pensioner-suspension-reason',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pensioner-suspension-reason/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerSuspensionReasonDto>({
      method: 'GET',
      url: `/api/erp/pensioner-suspension-reason/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionerSuspensionReasonDto>>({
      method: 'GET',
      url: '/api/erp/pensioner-suspension-reason',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionerSuspensionReasonDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerSuspensionReasonDto>({
      method: 'PUT',
      url: `/api/erp/pensioner-suspension-reason/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
