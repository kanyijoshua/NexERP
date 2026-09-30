import type { VatReturnDto, VatReturnInput, VatSettlementDto, VatSettlementInput } from './periodic-activities.models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class VatReportingService {
  apiName = 'Erp';


  calculateReturn = (input: VatReturnInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VatReturnDto>({
      method: 'POST',
      url: '/api/erp/vat-reporting/calculate-return',
      body: input,
    },
    { apiName: this.apiName,...config });


  calculateSettlement = (input: VatSettlementInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VatSettlementDto>({
      method: 'POST',
      url: '/api/erp/vat-reporting/calculate-settlement',
      body: input,
    },
    { apiName: this.apiName,...config });


  settle = (input: VatSettlementInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VatSettlementDto>({
      method: 'POST',
      url: '/api/erp/vat-reporting/settle',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
