import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateProjectJournalLineDto, GetProjectJournalLineListInput, ProjectJournalLineDto } from './projects.models';

@Injectable({
  providedIn: 'root',
})
export class ProjectJournalLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateProjectJournalLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectJournalLineDto>({
      method: 'POST',
      url: '/api/erp/project-journal-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/project-journal-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectJournalLineDto>({
      method: 'GET',
      url: `/api/erp/project-journal-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetProjectJournalLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProjectJournalLineDto>>({
      method: 'GET',
      url: '/api/erp/project-journal-line',
      params: { projectJournalBatchId: input.projectJournalBatchId, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateProjectJournalLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectJournalLineDto>({
      method: 'PUT',
      url: `/api/erp/project-journal-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
