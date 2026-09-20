import type { GLRegisterDto, GetGLRegistersInput, PostingPreviewLineDto, ReversalResultDto, ReverseRegisterInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class GlRegisterService {
  apiName = 'Erp';
  

  getEntries = (registerNo: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<PostingPreviewLineDto>>({
      method: 'GET',
      url: '/api/erp/gl-register/entries',
      params: { registerNo },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetGLRegistersInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<GLRegisterDto>>({
      method: 'GET',
      url: '/api/erp/gl-register',
      params: { fromDate: input.fromDate, toDate: input.toDate, onlyReversible: input.onlyReversible, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  runReversal = (input: ReverseRegisterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReversalResultDto>({
      method: 'POST',
      url: '/api/erp/gl-register/run-reversal',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
