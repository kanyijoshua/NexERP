import type {
  CreateJobQueueEntryDto,
  CreateUpdateJobQueueCategoryDto,
  GetJobQueueEntriesInput,
  GetJobQueueLogsInput,
  JobQueueCategoryDto,
  JobQueueEntryDto,
  JobQueueLogEntryDto,
  JobQueueRunResultDto,
  JobTypeInfoDto,
  UpdateJobQueueEntryDto,
} from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class JobQueueService {
  apiName = 'Erp';

  // Categories
  getCategories = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<JobQueueCategoryDto>>(
      {
        method: 'GET',
        url: '/api/erp/job-queue/categories',
      },
      { apiName: this.apiName, ...config },
    );

  createCategory = (input: CreateUpdateJobQueueCategoryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobQueueCategoryDto>(
      {
        method: 'POST',
        url: '/api/erp/job-queue/category',
        body: input,
      },
      { apiName: this.apiName, ...config },
    );

  updateCategory = (
    id: string,
    input: CreateUpdateJobQueueCategoryDto,
    config?: Partial<Rest.Config>,
  ) =>
    this.restService.request<any, JobQueueCategoryDto>(
      {
        method: 'PUT',
        url: `/api/erp/job-queue/${id}/category`,
        body: input,
      },
      { apiName: this.apiName, ...config },
    );

  deleteCategory = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/erp/job-queue/${id}/category`,
      },
      { apiName: this.apiName, ...config },
    );

  // Entries
  getList = (input: GetJobQueueEntriesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<JobQueueEntryDto>>(
      {
        method: 'GET',
        url: '/api/erp/job-queue',
        params: {
          categoryCode: input.categoryCode,
          jobType: input.jobType,
          status: input.status,
          filter: input.filter,
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config },
    );

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobQueueEntryDto>(
      {
        method: 'GET',
        url: `/api/erp/job-queue/${id}`,
      },
      { apiName: this.apiName, ...config },
    );

  create = (input: CreateJobQueueEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobQueueEntryDto>(
      {
        method: 'POST',
        url: '/api/erp/job-queue',
        body: input,
      },
      { apiName: this.apiName, ...config },
    );

  update = (id: string, input: UpdateJobQueueEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobQueueEntryDto>(
      {
        method: 'PUT',
        url: `/api/erp/job-queue/${id}`,
        body: input,
      },
      { apiName: this.apiName, ...config },
    );

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/erp/job-queue/${id}`,
      },
      { apiName: this.apiName, ...config },
    );

  // Actions
  setStatusReady = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobQueueEntryDto>(
      {
        method: 'POST',
        url: `/api/erp/job-queue/${id}/set-status-ready`,
      },
      { apiName: this.apiName, ...config },
    );

  setStatusOnHold = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobQueueEntryDto>(
      {
        method: 'POST',
        url: `/api/erp/job-queue/${id}/set-status-on-hold`,
      },
      { apiName: this.apiName, ...config },
    );

  restart = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobQueueEntryDto>(
      {
        method: 'POST',
        url: `/api/erp/job-queue/${id}/restart`,
      },
      { apiName: this.apiName, ...config },
    );

  runOnce = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobQueueRunResultDto>(
      {
        method: 'POST',
        url: `/api/erp/job-queue/${id}/run-once`,
      },
      { apiName: this.apiName, ...config },
    );

  // Handlers metadata
  getAvailableJobTypes = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<JobTypeInfoDto>>(
      {
        method: 'GET',
        url: '/api/erp/job-queue/available-job-types',
      },
      { apiName: this.apiName, ...config },
    );

  // Logs
  getLogs = (input: GetJobQueueLogsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<JobQueueLogEntryDto>>(
      {
        method: 'GET',
        url: '/api/erp/job-queue/logs',
        params: {
          jobQueueEntryId: input.jobQueueEntryId,
          status: input.status,
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config },
    );

  clearLogs = (jobQueueEntryId?: string, olderThanDays?: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'POST',
        url: '/api/erp/job-queue/clear-logs',
        params: {
          jobQueueEntryId,
          olderThanDays,
        },
      },
      { apiName: this.apiName, ...config },
    );

  constructor(private restService: RestService) {}
}
