import type { CreateUpdateHostelAllocationDto, GetStudentDocumentListInput, HostelAllocationDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class HostelAllocationService {
  apiName = 'Erp';


  create = (input: CreateUpdateHostelAllocationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HostelAllocationDto>({
      method: 'POST',
      url: '/api/erp/hostel-allocation',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/hostel-allocation/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HostelAllocationDto>({
      method: 'GET',
      url: `/api/erp/hostel-allocation/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetStudentDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<HostelAllocationDto>>({
      method: 'GET',
      url: '/api/erp/hostel-allocation',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, studentNo: input.studentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateHostelAllocationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HostelAllocationDto>({
      method: 'PUT',
      url: `/api/erp/hostel-allocation/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  allocate = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HostelAllocationDto>({
      method: 'POST',
      url: `/api/erp/hostel-allocation/${id}/allocate`,
    },
    { apiName: this.apiName,...config });

  clear = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HostelAllocationDto>({
      method: 'POST',
      url: `/api/erp/hostel-allocation/${id}/clear`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
