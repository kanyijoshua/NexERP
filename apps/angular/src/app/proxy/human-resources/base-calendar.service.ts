import type {
  CalculateWorkingPeriodInput,
  CalendarDatesDto,
  GetCalendarDatesInput,
  SetCalendarDateStatusInput,
  WorkingPeriodDto,
} from './base-calendar.models';
import type { CodeTableDto, CreateUpdateCodeTableDto, GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BaseCalendarService {
  apiName = 'Erp';


  calculateWorkingPeriod = (input: CalculateWorkingPeriodInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkingPeriodDto>({
      method: 'POST',
      url: '/api/erp/base-calendar/calculate-working-period',
      body: input,
    },
    { apiName: this.apiName,...config });


  create = (input: CreateUpdateCodeTableDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CodeTableDto>({
      method: 'POST',
      url: '/api/erp/base-calendar',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/base-calendar/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CodeTableDto>({
      method: 'GET',
      url: `/api/erp/base-calendar/${id}`,
    },
    { apiName: this.apiName,...config });


  getDates = (input: GetCalendarDatesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CalendarDatesDto>({
      method: 'GET',
      url: '/api/erp/base-calendar/dates',
      params: { calendarCode: input.calendarCode, employeeNo: input.employeeNo, fromDate: input.fromDate, toDate: input.toDate },
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CodeTableDto>>({
      method: 'GET',
      url: '/api/erp/base-calendar',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  setDateStatus = (input: SetCalendarDateStatusInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/erp/base-calendar/set-date-status',
      body: input,
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateCodeTableDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CodeTableDto>({
      method: 'PUT',
      url: `/api/erp/base-calendar/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
