import type { AccountScheduleDto, AccountScheduleLineDto, CreateUpdateAccountScheduleDto, CreateUpdateAccountScheduleLineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AccountScheduleService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateAccountScheduleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AccountScheduleDto>({
      method: 'POST',
      url: '/api/erp/account-schedule',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createLine = (input: CreateUpdateAccountScheduleLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AccountScheduleLineDto>({
      method: 'POST',
      url: '/api/erp/account-schedule/line',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/account-schedule/${id}`,
    },
    { apiName: this.apiName,...config });
  

  deleteLine = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/account-schedule/${id}/line`,
    },
    { apiName: this.apiName,...config });
  

  getLines = (scheduleId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<AccountScheduleLineDto>>({
      method: 'GET',
      url: `/api/erp/account-schedule/lines/${scheduleId}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<AccountScheduleDto>>({
      method: 'GET',
      url: '/api/erp/account-schedule',
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateAccountScheduleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AccountScheduleDto>({
      method: 'PUT',
      url: `/api/erp/account-schedule/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateLine = (id: string, input: CreateUpdateAccountScheduleLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AccountScheduleLineDto>({
      method: 'PUT',
      url: `/api/erp/account-schedule/${id}/line`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
