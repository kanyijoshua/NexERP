import type { ClinicPrescriptionDto, CreateUpdateClinicPrescriptionDto, GetDocumentLineListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ClinicPrescriptionService {
  apiName = 'Erp';


  create = (input: CreateUpdateClinicPrescriptionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ClinicPrescriptionDto>({
      method: 'POST',
      url: '/api/erp/clinic-prescription',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/clinic-prescription/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ClinicPrescriptionDto>({
      method: 'GET',
      url: `/api/erp/clinic-prescription/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetDocumentLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ClinicPrescriptionDto>>({
      method: 'GET',
      url: '/api/erp/clinic-prescription',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateClinicPrescriptionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ClinicPrescriptionDto>({
      method: 'PUT',
      url: `/api/erp/clinic-prescription/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
