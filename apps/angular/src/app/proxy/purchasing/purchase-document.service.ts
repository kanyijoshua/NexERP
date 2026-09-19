import type { CreateUpdatePurchaseHeaderDto, GetPurchaseDocumentListInput, PurchaseHeaderDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ApprovalRequestResultDto } from '../workflows/models';

@Injectable({
  providedIn: 'root',
})
export class PurchaseDocumentService {
  apiName = 'Erp';
  

  cancelApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/cancel-approval-request`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdatePurchaseHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>({
      method: 'POST',
      url: '/api/erp/purchase-document',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/purchase-document/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>({
      method: 'GET',
      url: `/api/erp/purchase-document/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetPurchaseDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PurchaseHeaderDto>>({
      method: 'GET',
      url: '/api/erp/purchase-document',
      params: { filter: input.filter, documentType: input.documentType, status: input.status, vendorId: input.vendorId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  release = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/release`,
    },
    { apiName: this.apiName,...config });
  

  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/reopen`,
    },
    { apiName: this.apiName,...config });
  

  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });
  

  sendApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalRequestResultDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/send-approval-request`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdatePurchaseHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>({
      method: 'PUT',
      url: `/api/erp/purchase-document/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
