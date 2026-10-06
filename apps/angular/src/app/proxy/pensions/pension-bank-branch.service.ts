import type { CreateUpdatePensionBankBranchDto, GetPensionBankBranchListInput, PensionBankBranchDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionBankBranchService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionBankBranchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBankBranchDto>({
      method: 'POST',
      url: '/api/erp/pension-bank-branch',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-bank-branch/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBankBranchDto>({
      method: 'GET',
      url: `/api/erp/pension-bank-branch/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionBankBranchListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionBankBranchDto>>({
      method: 'GET',
      url: '/api/erp/pension-bank-branch',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, bankCode: input.bankCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionBankBranchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBankBranchDto>({
      method: 'PUT',
      url: `/api/erp/pension-bank-branch/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
