import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetPostedPurchaseDocumentListInput, PostedPurchaseInvoiceDto } from './orders.models';

@Injectable({
  providedIn: 'root',
})
export class PostedPurchaseInvoiceService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostedPurchaseInvoiceDto>({
      method: 'GET',
      url: `/api/erp/posted-purchase-invoice/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPostedPurchaseDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PostedPurchaseInvoiceDto>>({
      method: 'GET',
      url: '/api/erp/posted-purchase-invoice',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, return: input.return, vendorId: input.vendorId, orderNo: input.orderNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
