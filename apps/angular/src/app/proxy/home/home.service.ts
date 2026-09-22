import type { HomeSummaryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class HomeService {
  apiName = 'Erp';


  getSummary = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, HomeSummaryDto>({
      method: 'GET',
      url: '/api/erp/home/summary',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
