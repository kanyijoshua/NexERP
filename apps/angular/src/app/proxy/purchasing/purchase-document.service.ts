import type {
  CloseCancelPurchaseOrderInput,
  CreateUpdatePurchaseHeaderDto,
  GetPurchaseDocumentListInput,
  PurchaseHeaderDto,
} from './models';
import type {
  GetReceiptLinesInput,
  OpenReceiptLineDto,
  PurchaseOrderPostingInput,
  PurchasePostingResultDto,
} from './orders.models';
import type { CopyDocumentLinesInput, MakeOrderInput } from '../sales/orders.models';
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
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentType: input.documentType, status: input.status, vendorId: input.vendorId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
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

  runOrderPosting = (id: string, input: PurchaseOrderPostingInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchasePostingResultDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/run-order-posting`,
      body: input,
    },
    { apiName: this.apiName,...config });

  runPrepaymentCreditMemoPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchasePostingResultDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/run-prepayment-credit-memo-posting`,
    },
    { apiName: this.apiName,...config });

  runPrepaymentPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchasePostingResultDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/run-prepayment-posting`,
    },
    { apiName: this.apiName,...config });

  makeOrder = (id: string, input: MakeOrderInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/make-order`,
      body: input,
    },
    { apiName: this.apiName,...config });

  getOpenReceiptLines = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpenReceiptLineDto[]>({
      method: 'GET',
      url: `/api/erp/purchase-document/${id}/open-receipt-lines`,
    },
    { apiName: this.apiName,...config });

  collectReceiptLines = (id: string, input: GetReceiptLinesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/collect-receipt-lines`,
      body: input,
    },
    { apiName: this.apiName,...config });

  copyLinesFrom = (id: string, input: CopyDocumentLinesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/copy-lines-from`,
      body: input,
    },
    { apiName: this.apiName,...config });

  closeOrder = (id: string, input: CloseCancelPurchaseOrderInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/close-order`,
      body: input,
    },
    { apiName: this.apiName,...config });

  cancelOrder = (id: string, input: CloseCancelPurchaseOrderInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseHeaderDto>({
      method: 'POST',
      url: `/api/erp/purchase-document/${id}/cancel-order`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}

