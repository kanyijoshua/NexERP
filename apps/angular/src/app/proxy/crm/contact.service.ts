import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ContactDto, CreateUpdateContactDto, GetContactListInput } from './crm.models';

@Injectable({
  providedIn: 'root',
})
export class ContactService {
  apiName = 'Erp';

  create = (input: CreateUpdateContactDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ContactDto>({
      method: 'POST',
      url: '/api/erp/contact',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/contact/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ContactDto>({
      method: 'GET',
      url: `/api/erp/contact/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetContactListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ContactDto>>({
      method: 'GET',
      url: '/api/erp/contact',
      params: { type: input.type, companyNo: input.companyNo, dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateContactDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ContactDto>({
      method: 'PUT',
      url: `/api/erp/contact/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  makeCustomer = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ContactDto>({
      method: 'POST',
      url: `/api/erp/contact/${id}/make-customer`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
