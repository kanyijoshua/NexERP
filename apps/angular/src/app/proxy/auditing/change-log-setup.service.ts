import type { ChangeLogLoggableTableDto, ChangeLogSetupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ChangeLogSetupService {
  apiName = 'Erp';


  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ChangeLogSetupDto>({
      method: 'GET',
      url: '/api/erp/change-log-setup',
    },
    { apiName: this.apiName,...config });


  getLoggableTables = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ChangeLogLoggableTableDto[]>({
      method: 'GET',
      url: '/api/erp/change-log-setup/loggable-tables',
    },
    { apiName: this.apiName,...config });


  update = (input: ChangeLogSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ChangeLogSetupDto>({
      method: 'PUT',
      url: '/api/erp/change-log-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
