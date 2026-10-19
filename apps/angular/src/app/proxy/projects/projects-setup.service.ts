import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ProjectsSetupDto } from './projects.models';

@Injectable({
  providedIn: 'root',
})
export class ProjectsSetupService {
  apiName = 'Erp';

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectsSetupDto>({
      method: 'GET',
      url: '/api/erp/projects-setup',
    },
    { apiName: this.apiName,...config });

  update = (input: ProjectsSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectsSetupDto>({
      method: 'PUT',
      url: '/api/erp/projects-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
