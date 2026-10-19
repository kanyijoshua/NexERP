import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetPostedSalesDocumentListInput, PostedSalesInvoiceDto } from './orders.models';

@Injectable({
  providedIn: 'root',
})
export class PostedSalesInvoiceService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostedSalesInvoiceDto>({
      method: 'GET',
      url: `/api/erp/posted-sales-invoice/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPostedSalesDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PostedSalesInvoiceDto>>({
      method: 'GET',
      url: '/api/erp/posted-sales-invoice',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, return: input.return, customerId: input.customerId, orderNo: input.orderNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
