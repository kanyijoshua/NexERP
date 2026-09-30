import type { InventorySetupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InventorySetupService {
  apiName = 'Erp';


  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventorySetupDto>({
      method: 'GET',
      url: '/api/erp/inventory-setup',
    },
    { apiName: this.apiName,...config });


  update = (input: InventorySetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventorySetupDto>({
      method: 'PUT',
      url: '/api/erp/inventory-setup',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
