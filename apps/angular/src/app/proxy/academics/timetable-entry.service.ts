import type { CreateUpdateTimetableEntryDto, GetTimetableEntryListInput, TimetableEntryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class TimetableEntryService {
  apiName = 'Erp';


  create = (input: CreateUpdateTimetableEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TimetableEntryDto>({
      method: 'POST',
      url: '/api/erp/timetable-entry',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/timetable-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TimetableEntryDto>({
      method: 'GET',
      url: `/api/erp/timetable-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetTimetableEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<TimetableEntryDto>>({
      method: 'GET',
      url: '/api/erp/timetable-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, semesterCode: input.semesterCode, programmeCode: input.programmeCode, timetableType: input.timetableType, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateTimetableEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TimetableEntryDto>({
      method: 'PUT',
      url: `/api/erp/timetable-entry/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
