import type {
  CreateUpdateProcurementPlanDto,
  GetProcurementPlanListInput,
  ProcurementPlanDto,
} from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ProcurementPlanService {
  apiName = 'Erp';

  create = (input: CreateUpdateProcurementPlanDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProcurementPlanDto>(
      {
        method: 'POST',
        url: '/api/erp/procurement-plan',
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  update = (id: string, input: CreateUpdateProcurementPlanDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProcurementPlanDto>(
      {
        method: 'PUT',
        url: `/api/erp/procurement-plan/${id}`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/erp/procurement-plan/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProcurementPlanDto>(
      {
        method: 'GET',
        url: `/api/erp/procurement-plan/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  getList = (input: GetProcurementPlanListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProcurementPlanDto>>(
      {
        method: 'GET',
        url: '/api/erp/procurement-plan',
        params: {
          filter: input.filter,
          status: input.status,
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config }
    );

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProcurementPlanDto>(
      {
        method: 'POST',
        url: `/api/erp/procurement-plan/${id}/approve`,
      },
      { apiName: this.apiName, ...config }
    );

  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProcurementPlanDto>(
      {
        method: 'POST',
        url: `/api/erp/procurement-plan/${id}/reopen`,
      },
      { apiName: this.apiName, ...config }
    );

  close = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProcurementPlanDto>(
      {
        method: 'POST',
        url: `/api/erp/procurement-plan/${id}/close`,
      },
      { apiName: this.apiName, ...config }
    );

  constructor(private restService: RestService) {}
}
