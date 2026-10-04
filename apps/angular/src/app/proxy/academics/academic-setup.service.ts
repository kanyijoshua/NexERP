import type { AcademicSetupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AcademicSetupService {
  apiName = 'Erp';


  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, AcademicSetupDto>({
      method: 'GET',
      url: '/api/erp/academic-setup',
    },
    { apiName: this.apiName,...config });


  update = (input: AcademicSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AcademicSetupDto>({
      method: 'PUT',
      url: '/api/erp/academic-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
