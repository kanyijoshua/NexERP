import type { CreateUpdateExportTemplateDto, DataExportInput, DataPreviewDto, DataPreviewInput, ExportTemplateDto, ExportableEntityDto, ExportableFieldDto, GetExportTemplatesInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DataExportService {
  apiName = 'Erp';
  

  createTemplate = (input: CreateUpdateExportTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExportTemplateDto>({
      method: 'POST',
      url: '/api/erp/data-export/template',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  deleteTemplate = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/data-export/${id}/template`,
    },
    { apiName: this.apiName,...config });
  

  getEntities = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ExportableEntityDto>>({
      method: 'GET',
      url: '/api/erp/data-export/entities',
    },
    { apiName: this.apiName,...config });
  

  getFields = (entityName: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ExportableFieldDto>>({
      method: 'GET',
      url: '/api/erp/data-export/fields',
      params: { entityName },
    },
    { apiName: this.apiName,...config });
  

  getPreview = (input: DataPreviewInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DataPreviewDto>({
      method: 'GET',
      url: '/api/erp/data-export/preview',
      params: { entityName: input.entityName, fields: input.fields, filters: input.filters, orderBy: input.orderBy, descending: input.descending, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getTemplates = (input: GetExportTemplatesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ExportTemplateDto>>({
      method: 'GET',
      url: '/api/erp/data-export/templates',
      params: { entityName: input.entityName },
    },
    { apiName: this.apiName,...config });
  

  runExport = (input: DataExportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/erp/data-export/run-export',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateTemplate = (id: string, input: CreateUpdateExportTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExportTemplateDto>({
      method: 'PUT',
      url: `/api/erp/data-export/${id}/template`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
