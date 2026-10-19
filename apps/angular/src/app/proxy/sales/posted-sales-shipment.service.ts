import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetPostedSalesDocumentListInput, PostedSalesShipmentDto } from './orders.models';

@Injectable({
  providedIn: 'root',
})
export class PostedSalesShipmentService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostedSalesShipmentDto>({
      method: 'GET',
      url: `/api/erp/posted-sales-shipment/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPostedSalesDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PostedSalesShipmentDto>>({
      method: 'GET',
      url: '/api/erp/posted-sales-shipment',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, return: input.return, customerId: input.customerId, orderNo: input.orderNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  undoLine = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/posted-sales-shipment/${id}/undo-line`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
