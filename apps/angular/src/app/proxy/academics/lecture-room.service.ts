import type { CreateUpdateLectureRoomDto, LectureRoomDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LectureRoomService {
  apiName = 'Erp';


  create = (input: CreateUpdateLectureRoomDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LectureRoomDto>({
      method: 'POST',
      url: '/api/erp/lecture-room',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/lecture-room/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LectureRoomDto>({
      method: 'GET',
      url: `/api/erp/lecture-room/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LectureRoomDto>>({
      method: 'GET',
      url: '/api/erp/lecture-room',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateLectureRoomDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LectureRoomDto>({
      method: 'PUT',
      url: `/api/erp/lecture-room/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
