import type {
  BaseCalendarChangeDto,
  CreateUpdateBaseCalendarChangeDto,
  GetBaseCalendarChangeListInput,
} from './base-calendar.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BaseCalendarChangeService {
  apiName = 'Erp';


  create = (input: CreateUpdateBaseCalendarChangeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BaseCalendarChangeDto>({
      method: 'POST',
      url: '/api/erp/base-calendar-change',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/base-calendar-change/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BaseCalendarChangeDto>({
      method: 'GET',
      url: `/api/erp/base-calendar-change/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetBaseCalendarChangeListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BaseCalendarChangeDto>>({
      method: 'GET',
      url: '/api/erp/base-calendar-change',
      params: { baseCalendarCode: input.baseCalendarCode, dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateBaseCalendarChangeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BaseCalendarChangeDto>({
      method: 'PUT',
      url: `/api/erp/base-calendar-change/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
