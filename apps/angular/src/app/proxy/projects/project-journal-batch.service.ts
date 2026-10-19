import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateProjectJournalBatchDto, ProjectJournalBatchDto } from './projects.models';

@Injectable({
  providedIn: 'root',
})
export class ProjectJournalBatchService {
  apiName = 'Erp';

  create = (input: CreateUpdateProjectJournalBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectJournalBatchDto>({
      method: 'POST',
      url: '/api/erp/project-journal-batch',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/project-journal-batch/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectJournalBatchDto>({
      method: 'GET',
      url: `/api/erp/project-journal-batch/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProjectJournalBatchDto>>({
      method: 'GET',
      url: '/api/erp/project-journal-batch',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateProjectJournalBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectJournalBatchDto>({
      method: 'PUT',
      url: `/api/erp/project-journal-batch/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: `/api/erp/project-journal-batch/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
