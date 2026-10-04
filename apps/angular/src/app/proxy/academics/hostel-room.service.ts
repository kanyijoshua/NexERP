import type { CreateUpdateHostelRoomDto, GetHostelRoomListInput, HostelRoomDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class HostelRoomService {
  apiName = 'Erp';


  create = (input: CreateUpdateHostelRoomDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HostelRoomDto>({
      method: 'POST',
      url: '/api/erp/hostel-room',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/hostel-room/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HostelRoomDto>({
      method: 'GET',
      url: `/api/erp/hostel-room/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetHostelRoomListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<HostelRoomDto>>({
      method: 'GET',
      url: '/api/erp/hostel-room',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, hostelCode: input.hostelCode, vacantOnly: input.vacantOnly, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateHostelRoomDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HostelRoomDto>({
      method: 'PUT',
      url: `/api/erp/hostel-room/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
