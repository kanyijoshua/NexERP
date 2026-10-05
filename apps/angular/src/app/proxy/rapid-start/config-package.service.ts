import type { ConfigFieldInfoDto, ConfigPackageDetailDto, ConfigPackageDto, ConfigPackageErrorDto, ConfigPackageRecordDto, ConfigPackageRunResultDto, ConfigPackageTableInput, ConfigPackageTablesInput, ConfigTableInfoDto, CreateConfigPackageDto, ExportConfigPackageInput, GetConfigPackageRecordsInput, GetConfigPackagesInput, ImportConfigDataResultDto, IncludeConfigTablesInput, UpdateConfigPackageDto, UpdateConfigPackageRecordDto, UpdateConfigPackageTableDto, UploadFileInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ConfigPackageService {
  apiName = 'Erp';
  

  applyPackage = (id: string, input: ConfigPackageTablesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageRunResultDto>({
      method: 'POST',
      url: `/api/erp/config-package/${id}/apply-package`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  clearPackageData = (id: string, input: ConfigPackageTablesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageDetailDto>({
      method: 'POST',
      url: `/api/erp/config-package/${id}/clear-package-data`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateConfigPackageDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageDetailDto>({
      method: 'POST',
      url: '/api/erp/config-package',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/config-package/${id}`,
    },
    { apiName: this.apiName,...config });
  

  deleteRecord = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/config-package/${id}/record`,
    },
    { apiName: this.apiName,...config });
  

  excludeTable = (id: string, input: ConfigPackageTableInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageDetailDto>({
      method: 'POST',
      url: `/api/erp/config-package/${id}/exclude-table`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  exportPackage = (id: string, input: ExportConfigPackageInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>(
      {
        method: 'POST',
        responseType: 'blob',
        url: `/api/erp/config-package/${id}/export-package`,
        body: input,
      },
      { apiName: this.apiName, responseType: Rest.ResponseType.Blob, ...config },
    );
  

  fillFromDatabase = (id: string, input: ConfigPackageTablesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageDetailDto>({
      method: 'POST',
      url: `/api/erp/config-package/${id}/fill-from-database`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageDetailDto>({
      method: 'GET',
      url: `/api/erp/config-package/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getErrors = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ConfigPackageErrorDto>>({
      method: 'GET',
      url: `/api/erp/config-package/${id}/errors`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetConfigPackagesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ConfigPackageDto>>({
      method: 'GET',
      url: '/api/erp/config-package',
      params: { filter: input.filter },
    },
    { apiName: this.apiName,...config });
  

  getRecords = (input: GetConfigPackageRecordsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ConfigPackageRecordDto>>({
      method: 'GET',
      url: '/api/erp/config-package/records',
      params: { packageId: input.packageId, tableId: input.tableId, errorsOnly: input.errorsOnly, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getTableCatalog = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ConfigTableInfoDto>>({
      method: 'GET',
      url: '/api/erp/config-package/table-catalog',
    },
    { apiName: this.apiName,...config });
  

  getTableFields = (entityName: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ConfigFieldInfoDto>>({
      method: 'GET',
      url: '/api/erp/config-package/table-fields',
      params: { entityName },
    },
    { apiName: this.apiName,...config });
  

  importData = (id: string, input: UploadFileInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ImportConfigDataResultDto>({
      method: 'POST',
      url: `/api/erp/config-package/${id}/import-data`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  importPackage = (input: UploadFileInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageDetailDto>({
      method: 'POST',
      url: '/api/erp/config-package/import-package',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  includeTables = (id: string, input: IncludeConfigTablesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageDetailDto>({
      method: 'POST',
      url: `/api/erp/config-package/${id}/include-tables`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateConfigPackageDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageDetailDto>({
      method: 'PUT',
      url: `/api/erp/config-package/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateRecord = (id: string, input: UpdateConfigPackageRecordDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageRecordDto>({
      method: 'PUT',
      url: `/api/erp/config-package/${id}/record`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateTable = (id: string, input: UpdateConfigPackageTableDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageDetailDto>({
      method: 'PUT',
      url: `/api/erp/config-package/${id}/table`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  validatePackage = (id: string, input: ConfigPackageTablesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfigPackageRunResultDto>({
      method: 'POST',
      url: `/api/erp/config-package/${id}/validate-package`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
