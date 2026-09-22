import type { ErpModuleDto, SetModuleEnabledInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ModuleService {
  apiName = 'Erp';


  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ErpModuleDto>>({
      method: 'GET',
      url: '/api/erp/module',
    },
    { apiName: this.apiName,...config });


  setEnabled = (input: SetModuleEnabledInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ErpModuleDto>({
      method: 'POST',
      url: '/api/erp/module/set-enabled',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
