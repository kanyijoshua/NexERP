import type { ErpThemeDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  apiName = 'Erp';


  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ErpThemeDto>({
      method: 'GET',
      url: '/api/erp/theme',
    },
    { apiName: this.apiName,...config });


  reset = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ErpThemeDto>({
      method: 'POST',
      url: '/api/erp/theme/reset',
    },
    { apiName: this.apiName,...config });


  save = (input: ErpThemeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ErpThemeDto>({
      method: 'POST',
      url: '/api/erp/theme/save',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
