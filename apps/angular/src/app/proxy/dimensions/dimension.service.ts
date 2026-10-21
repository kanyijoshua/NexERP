import type {
  CreateDimensionDto,
  CreateDimensionValueDto,
  DimensionDto,
  DimensionSetDto,
  DimensionValueDto,
  ResolveDimensionSetInput,
  ShortcutDimensionSlotDto,
  UpdateDimensionDto,
  UpdateDimensionValueDto,
} from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DimensionService {
  apiName = 'Erp';

  create = (input: CreateDimensionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DimensionDto>({
      method: 'POST',
      url: '/api/erp/dimension',
      body: input,
    },
    { apiName: this.apiName, ...config });

  update = (id: string, input: UpdateDimensionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DimensionDto>({
      method: 'PUT',
      url: `/api/erp/dimension/${id}`,
      body: input,
    },
    { apiName: this.apiName, ...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/dimension/${id}`,
    },
    { apiName: this.apiName, ...config });

  createValue = (input: CreateDimensionValueDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DimensionValueDto>({
      method: 'POST',
      url: '/api/erp/dimension/value',
      body: input,
    },
    { apiName: this.apiName, ...config });

  updateValue = (id: string, input: UpdateDimensionValueDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DimensionValueDto>({
      method: 'PUT',
      url: `/api/erp/dimension/value/${id}`,
      body: input,
    },
    { apiName: this.apiName, ...config });

  deleteValue = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/dimension/value/${id}`,
    },
    { apiName: this.apiName, ...config });

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<DimensionDto>>({
      method: 'GET',
      url: '/api/erp/dimension',
    },
    { apiName: this.apiName, ...config });

  getValues = (dimensionId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<DimensionValueDto>>({
      method: 'GET',
      url: `/api/erp/dimension/values/${dimensionId}`,
    },
    { apiName: this.apiName, ...config });

  getShortcutSlots = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShortcutDimensionSlotDto>>({
      method: 'GET',
      url: '/api/erp/dimension/shortcut-slots',
    },
    { apiName: this.apiName, ...config });

  getDimensionSet = (dimensionSetId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DimensionSetDto>({
      method: 'GET',
      url: `/api/erp/dimension/dimension-set/${dimensionSetId}`,
    },
    { apiName: this.apiName, ...config });

  resolveDimensionSet = (input: ResolveDimensionSetInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DimensionSetDto>({
      method: 'POST',
      url: '/api/erp/dimension/resolve-dimension-set',
      body: input,
    },
    { apiName: this.apiName, ...config });

  constructor(private restService: RestService) {}
}
