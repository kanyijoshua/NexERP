import type {
  ExchRateAdjmtRegisterDto,
  ExchRateAdjustmentDto,
  ExchRateAdjustmentInput,
  GetExchRateAdjmtRegisterListInput,
} from './periodic-activities.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExchRateAdjustmentService {
  apiName = 'Erp';


  calculate = (input: ExchRateAdjustmentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExchRateAdjustmentDto>({
      method: 'POST',
      url: '/api/erp/exch-rate-adjustment/calculate',
      body: input,
    },
    { apiName: this.apiName,...config });


  adjust = (input: ExchRateAdjustmentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExchRateAdjustmentDto>({
      method: 'POST',
      url: '/api/erp/exch-rate-adjustment/adjust',
      body: input,
    },
    { apiName: this.apiName,...config });


  getRegisters = (input: GetExchRateAdjmtRegisterListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ExchRateAdjmtRegisterDto>>({
      method: 'GET',
      url: '/api/erp/exch-rate-adjustment/registers',
      params: { filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
