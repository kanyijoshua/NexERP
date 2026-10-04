import type { CreateUpdateLumpsumTaxBandDto, GetLumpsumTaxBandListInput, LumpsumTaxBandDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LumpsumTaxBandService {
  apiName = 'Erp';


  create = (input: CreateUpdateLumpsumTaxBandDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LumpsumTaxBandDto>({
      method: 'POST',
      url: '/api/erp/lumpsum-tax-band',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/lumpsum-tax-band/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LumpsumTaxBandDto>({
      method: 'GET',
      url: `/api/erp/lumpsum-tax-band/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetLumpsumTaxBandListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LumpsumTaxBandDto>>({
      method: 'GET',
      url: '/api/erp/lumpsum-tax-band',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, taxTableCode: input.taxTableCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateLumpsumTaxBandDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LumpsumTaxBandDto>({
      method: 'PUT',
      url: `/api/erp/lumpsum-tax-band/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
