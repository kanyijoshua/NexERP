import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CloseOpportunityInput, CreateUpdateOpportunityDto, GetOpportunityListInput, MoveOpportunityInput, OpportunityDto, OpportunityEntryDto, PipelineDto } from './crm.models';

@Injectable({
  providedIn: 'root',
})
export class OpportunityService {
  apiName = 'Erp';

  create = (input: CreateUpdateOpportunityDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpportunityDto>({
      method: 'POST',
      url: '/api/erp/opportunity',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/opportunity/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpportunityDto>({
      method: 'GET',
      url: `/api/erp/opportunity/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetOpportunityListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<OpportunityDto>>({
      method: 'GET',
      url: '/api/erp/opportunity',
      params: { kind: input.kind, status: input.status, contactNo: input.contactNo, salespersonCode: input.salespersonCode, dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateOpportunityDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpportunityDto>({
      method: 'PUT',
      url: `/api/erp/opportunity/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  move = (id: string, input: MoveOpportunityInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpportunityDto>({
      method: 'POST',
      url: `/api/erp/opportunity/${id}/move`,
      body: input,
    },
    { apiName: this.apiName,...config });

  convert = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpportunityDto>({
      method: 'POST',
      url: `/api/erp/opportunity/${id}/convert`,
    },
    { apiName: this.apiName,...config });

  win = (id: string, input: CloseOpportunityInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpportunityDto>({
      method: 'POST',
      url: `/api/erp/opportunity/${id}/win`,
      body: input,
    },
    { apiName: this.apiName,...config });

  lose = (id: string, input: CloseOpportunityInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpportunityDto>({
      method: 'POST',
      url: `/api/erp/opportunity/${id}/lose`,
      body: input,
    },
    { apiName: this.apiName,...config });

  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpportunityDto>({
      method: 'POST',
      url: `/api/erp/opportunity/${id}/reopen`,
    },
    { apiName: this.apiName,...config });

  makeSalesQuote = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpportunityDto>({
      method: 'POST',
      url: `/api/erp/opportunity/${id}/make-sales-quote`,
    },
    { apiName: this.apiName,...config });

  getHistory = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpportunityEntryDto[]>({
      method: 'GET',
      url: `/api/erp/opportunity/${id}/history`,
    },
    { apiName: this.apiName,...config });

  getPipeline = (input: { pipeline: string }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PipelineDto>({
      method: 'GET',
      url: '/api/erp/opportunity/pipeline',
      params: { pipeline: input.pipeline },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
