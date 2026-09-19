import type { CompanyDto, CopyCompanyInput, CreateCompanyDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class companyService {
  apiName = 'Erp';
  

  copy = (input: CopyCompanyInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CompanyDto>({
      method: 'POST',
      url: '/api/erp/company/copy',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateCompanyDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CompanyDto>({
      method: 'POST',
      url: '/api/erp/company',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<CompanyDto>>({
      method: 'GET',
      url: '/api/erp/company',
    },
    { apiName: this.apiName,...config });
  

  setAsDefault = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/company/${id}/set-as-default`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
