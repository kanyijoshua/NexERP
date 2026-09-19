import type { CreateUpdateSalesHeaderDto, GetSalesDocumentListInput, SalesHeaderDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ApprovalRequestResultDto } from '../workflows/models';

@Injectable({
  providedIn: 'root',
})
export class SalesDocumentService {
  apiName = 'Erp';
  

  cancelApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/sales-document/${id}/cancel-approval-request`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateSalesHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesHeaderDto>({
      method: 'POST',
      url: '/api/erp/sales-document',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/sales-document/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesHeaderDto>({
      method: 'GET',
      url: `/api/erp/sales-document/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetSalesDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SalesHeaderDto>>({
      method: 'GET',
      url: '/api/erp/sales-document',
      params: { filter: input.filter, documentType: input.documentType, status: input.status, customerId: input.customerId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  release = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesHeaderDto>({
      method: 'POST',
      url: `/api/erp/sales-document/${id}/release`,
    },
    { apiName: this.apiName,...config });
  

  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesHeaderDto>({
      method: 'POST',
      url: `/api/erp/sales-document/${id}/reopen`,
    },
    { apiName: this.apiName,...config });
  

  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesHeaderDto>({
      method: 'POST',
      url: `/api/erp/sales-document/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });
  

  sendApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalRequestResultDto>({
      method: 'POST',
      url: `/api/erp/sales-document/${id}/send-approval-request`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateSalesHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesHeaderDto>({
      method: 'PUT',
      url: `/api/erp/sales-document/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
