import type {
  ConvertRequisitionToOrderInput,
  ConvertRequisitionToTenderInput,
  CreateUpdatePurchaseRequisitionDto,
  GetPurchaseRequisitionListInput,
  PurchaseRequisitionDto,
  TenderHeaderDto,
} from './models';
import type { PurchaseHeaderDto } from '../purchasing/models';
import type { ApprovalRequestResultDto } from '../workflows/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PurchaseRequisitionService {
  apiName = 'Erp';

  create = (input: CreateUpdatePurchaseRequisitionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseRequisitionDto>(
      {
        method: 'POST',
        url: '/api/erp/purchase-requisition',
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  update = (id: string, input: CreateUpdatePurchaseRequisitionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseRequisitionDto>(
      {
        method: 'PUT',
        url: `/api/erp/purchase-requisition/${id}`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/erp/purchase-requisition/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseRequisitionDto>(
      {
        method: 'GET',
        url: `/api/erp/purchase-requisition/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  getList = (input: GetPurchaseRequisitionListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PurchaseRequisitionDto>>(
      {
        method: 'GET',
        url: '/api/erp/purchase-requisition',
        params: {
          filter: input.filter,
          requestType: input.requestType,
          status: input.status,
          departmentCode: input.departmentCode,
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config }
    );

  release = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseRequisitionDto>(
      {
        method: 'POST',
        url: `/api/erp/purchase-requisition/${id}/release`,
      },
      { apiName: this.apiName, ...config }
    );

  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseRequisitionDto>(
      {
        method: 'POST',
        url: `/api/erp/purchase-requisition/${id}/reopen`,
      },
      { apiName: this.apiName, ...config }
    );

  reject = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseRequisitionDto>(
      {
        method: 'POST',
        url: `/api/erp/purchase-requisition/${id}/reject`,
      },
      { apiName: this.apiName, ...config }
    );

  close = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseRequisitionDto>(
      {
        method: 'POST',
        url: `/api/erp/purchase-requisition/${id}/close`,
      },
      { apiName: this.apiName, ...config }
    );

  convertToPurchaseOrder = (id: string, input: ConvertRequisitionToOrderInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>(
      {
        method: 'POST',
        url: `/api/erp/purchase-requisition/${id}/convert-to-purchase-order`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  convertToTender = (id: string, input: ConvertRequisitionToTenderInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TenderHeaderDto>(
      {
        method: 'POST',
        url: `/api/erp/purchase-requisition/${id}/convert-to-tender`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  sendApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalRequestResultDto>(
      {
        method: 'POST',
        url: `/api/erp/purchase-requisition/${id}/send-approval-request`,
      },
      { apiName: this.apiName, ...config }
    );

  cancelApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'POST',
        url: `/api/erp/purchase-requisition/${id}/cancel-approval-request`,
      },
      { apiName: this.apiName, ...config }
    );

  constructor(private restService: RestService) {}
}
