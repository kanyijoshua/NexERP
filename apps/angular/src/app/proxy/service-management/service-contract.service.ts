import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateServiceContractDto, InvoiceServiceContractsInput, ServiceContractDto, ServiceContractInvoicingResultDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class ServiceContractService {
  apiName = 'Erp';

  create = (input: CreateUpdateServiceContractDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceContractDto>({
      method: 'POST',
      url: '/api/erp/service-contract',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/service-contract/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceContractDto>({
      method: 'GET',
      url: `/api/erp/service-contract/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ServiceContractDto>>({
      method: 'GET',
      url: '/api/erp/service-contract',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateServiceContractDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceContractDto>({
      method: 'PUT',
      url: `/api/erp/service-contract/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  sign = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceContractDto>({
      method: 'POST',
      url: `/api/erp/service-contract/${id}/sign`,
    },
    { apiName: this.apiName,...config });

  cancel = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceContractDto>({
      method: 'POST',
      url: `/api/erp/service-contract/${id}/cancel`,
    },
    { apiName: this.apiName,...config });

  invoiceDue = (id: string, input: InvoiceServiceContractsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceContractInvoicingResultDto>({
      method: 'POST',
      url: `/api/erp/service-contract/${id}/invoice-due`,
      body: input,
    },
    { apiName: this.apiName,...config });

  invoiceAllDue = (input: InvoiceServiceContractsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceContractInvoicingResultDto>({
      method: 'POST',
      url: '/api/erp/service-contract/invoice-all-due',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
