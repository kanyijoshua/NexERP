import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateSerialNoInformationDto, GetItemChildListInput, SerialNoInformationDto } from './warehouse.models';

@Injectable({
  providedIn: 'root',
})
export class SerialNoInformationService {
  apiName = 'Erp';

  create = (input: CreateUpdateSerialNoInformationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SerialNoInformationDto>({
      method: 'POST',
      url: '/api/erp/serial-no-information',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/serial-no-information/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SerialNoInformationDto>({
      method: 'GET',
      url: `/api/erp/serial-no-information/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetItemChildListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SerialNoInformationDto>>({
      method: 'GET',
      url: '/api/erp/serial-no-information',
      params: { dynamicFilter: input.dynamicFilter, itemNo: input.itemNo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateSerialNoInformationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SerialNoInformationDto>({
      method: 'PUT',
      url: `/api/erp/serial-no-information/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
