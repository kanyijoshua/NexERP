import type { CreateUpdateHostelDto, HostelDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class HostelService {
  apiName = 'Erp';


  create = (input: CreateUpdateHostelDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HostelDto>({
      method: 'POST',
      url: '/api/erp/hostel',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/hostel/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HostelDto>({
      method: 'GET',
      url: `/api/erp/hostel/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<HostelDto>>({
      method: 'GET',
      url: '/api/erp/hostel',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateHostelDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HostelDto>({
      method: 'PUT',
      url: `/api/erp/hostel/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
