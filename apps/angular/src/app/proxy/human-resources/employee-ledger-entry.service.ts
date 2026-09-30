import type { EmployeeLedgerEntryDto, GetEmployeeLedgerEntryListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class EmployeeLedgerEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeLedgerEntryDto>({
      method: 'GET',
      url: `/api/erp/employee-ledger-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetEmployeeLedgerEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<EmployeeLedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/employee-ledger-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, employeeNo: input.employeeNo, onlyOpen: input.onlyOpen, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
