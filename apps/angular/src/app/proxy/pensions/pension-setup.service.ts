import type { PensionSetupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionSetupService {
  apiName = 'Erp';


  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionSetupDto>({
      method: 'GET',
      url: '/api/erp/pension-setup',
    },
    { apiName: this.apiName,...config });


  update = (input: PensionSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionSetupDto>({
      method: 'PUT',
      url: '/api/erp/pension-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
