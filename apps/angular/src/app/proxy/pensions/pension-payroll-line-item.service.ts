import type { GetPensionPayrollLineItemListInput, PensionPayrollLineItemDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionPayrollLineItemService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollLineItemDto>({
      method: 'GET',
      url: `/api/erp/pension-payroll-line-item/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionPayrollLineItemListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionPayrollLineItemDto>>({
      method: 'GET',
      url: '/api/erp/pension-payroll-line-item',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentNo: input.documentNo, lineNo: input.lineNo, pensionerNo: input.pensionerNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
