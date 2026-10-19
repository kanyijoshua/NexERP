import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetPostedPurchaseDocumentListInput, PostedPurchaseReceiptDto } from './orders.models';

@Injectable({
  providedIn: 'root',
})
export class PostedPurchaseReceiptService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostedPurchaseReceiptDto>({
      method: 'GET',
      url: `/api/erp/posted-purchase-receipt/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPostedPurchaseDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PostedPurchaseReceiptDto>>({
      method: 'GET',
      url: '/api/erp/posted-purchase-receipt',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, return: input.return, vendorId: input.vendorId, orderNo: input.orderNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  undoLine = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/posted-purchase-receipt/${id}/undo-line`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
