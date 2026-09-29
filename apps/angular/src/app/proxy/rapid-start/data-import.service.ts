import type { ConfigTableInfoDto, ImportFileInput, ImportFilePreviewDto, ImportResultDto, RunImportInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DataImportService {
  apiName = 'Erp';


  getEntities = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ConfigTableInfoDto>>({
      method: 'GET',
      url: '/api/erp/data-import/entities',
    },
    { apiName: this.apiName,...config });


  getTemplateFile = (entityName: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'GET',
      responseType: 'blob',
      url: '/api/erp/data-import/template-file',
      params: { entityName },
    },
    { apiName: this.apiName,...config });


  parseFile = (input: ImportFileInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ImportFilePreviewDto>({
      method: 'POST',
      url: '/api/erp/data-import/parse-file',
      body: input,
    },
    { apiName: this.apiName,...config });


  runImport = (input: RunImportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ImportResultDto>({
      method: 'POST',
      url: '/api/erp/data-import/run-import',
      body: input,
    },
    { apiName: this.apiName,...config });


  testImport = (input: RunImportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ImportResultDto>({
      method: 'POST',
      url: '/api/erp/data-import/test-import',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
