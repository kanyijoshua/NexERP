import type { FASetupDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FaSetupService {
  apiName = 'Erp';


  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, FASetupDto>({
      method: 'GET',
      url: '/api/erp/fa-setup',
    },
    { apiName: this.apiName,...config });


  update = (input: FASetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FASetupDto>({
      method: 'PUT',
      url: '/api/erp/fa-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
