import type { CreateUpdatePensionBeneficiaryDto, GetPensionBeneficiaryListInput, PensionBeneficiaryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionBeneficiaryService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionBeneficiaryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBeneficiaryDto>({
      method: 'POST',
      url: '/api/erp/pension-beneficiary',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-beneficiary/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBeneficiaryDto>({
      method: 'GET',
      url: `/api/erp/pension-beneficiary/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionBeneficiaryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionBeneficiaryDto>>({
      method: 'GET',
      url: '/api/erp/pension-beneficiary',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, memberNo: input.memberNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionBeneficiaryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBeneficiaryDto>({
      method: 'PUT',
      url: `/api/erp/pension-beneficiary/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
