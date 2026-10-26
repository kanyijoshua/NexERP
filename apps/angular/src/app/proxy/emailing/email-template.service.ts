import type { CreateUpdateEmailTemplateDto, EmailTemplateDto, GetEmailTemplateListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class EmailTemplateService {
  apiName = 'Erp';


  create = (input: CreateUpdateEmailTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmailTemplateDto>({
      method: 'POST',
      url: '/api/erp/email-template',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/email-template/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmailTemplateDto>({
      method: 'GET',
      url: `/api/erp/email-template/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetEmailTemplateListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<EmailTemplateDto>>({
      method: 'GET',
      url: '/api/erp/email-template',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentType: input.documentType, active: input.active, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  getPlaceholders = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, string[]>({
      method: 'GET',
      url: '/api/erp/email-template/placeholders',
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateEmailTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmailTemplateDto>({
      method: 'PUT',
      url: `/api/erp/email-template/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
