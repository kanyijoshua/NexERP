import type { PayrollSetupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PayrollSetupService {
  apiName = 'Erp';


  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollSetupDto>({
      method: 'GET',
      url: '/api/erp/payroll-setup',
    },
    { apiName: this.apiName,...config });


  update = (input: PayrollSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollSetupDto>({
      method: 'PUT',
      url: '/api/erp/payroll-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
