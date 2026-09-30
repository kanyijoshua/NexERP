import type { AccountingPeriodDto, FiscalYearClosedDto, GetAccountingPeriodListInput, NewFiscalYearDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AccountingPeriodService {
  apiName = 'Erp';


  closeFiscalYear = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, FiscalYearClosedDto>({
      method: 'POST',
      url: '/api/erp/accounting-period/close-fiscal-year',
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/accounting-period/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetAccountingPeriodListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AccountingPeriodDto>>({
      method: 'GET',
      url: '/api/erp/accounting-period',
      params: { filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount, dynamicFilter: input.dynamicFilter },
    },
    { apiName: this.apiName,...config });


  newFiscalYear = (input: NewFiscalYearDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<AccountingPeriodDto>>({
      method: 'POST',
      url: '/api/erp/accounting-period/new-fiscal-year',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
