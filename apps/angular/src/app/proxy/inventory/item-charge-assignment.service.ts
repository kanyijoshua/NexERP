import type { CreateItemChargeAssignmentDto, GetAssignableEntriesInput, GetItemChargeAssignmentListInput, ItemChargeAssignmentDto, SuggestItemChargeAllocationInput, UpdateItemChargeAssignmentDto } from './item-charge.models';
import type { ItemLedgerEntryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ItemChargeAssignmentService {
  apiName = 'Erp';


  create = (input: CreateItemChargeAssignmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemChargeAssignmentDto>({
      method: 'POST',
      url: '/api/erp/item-charge-assignment',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item-charge-assignment/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemChargeAssignmentDto>({
      method: 'GET',
      url: `/api/erp/item-charge-assignment/${id}`,
    },
    { apiName: this.apiName,...config });


  getAssignableEntries = (input: GetAssignableEntriesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemLedgerEntryDto[]>({
      method: 'GET',
      url: '/api/erp/item-charge-assignment/assignable-entries',
      params: { sourceType: input.sourceType, sourceLineId: input.sourceLineId, filter: input.filter, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  getList = (input: GetItemChargeAssignmentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemChargeAssignmentDto>>({
      method: 'GET',
      url: '/api/erp/item-charge-assignment',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sourceType: input.sourceType, sourceLineId: input.sourceLineId, sourceDocumentNo: input.sourceDocumentNo, posted: input.posted, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  suggestAllocation = (input: SuggestItemChargeAllocationInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemChargeAssignmentDto[]>({
      method: 'POST',
      url: '/api/erp/item-charge-assignment/suggest-allocation',
      body: input,
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: UpdateItemChargeAssignmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemChargeAssignmentDto>({
      method: 'PUT',
      url: `/api/erp/item-charge-assignment/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
