import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { BankFileDto, ExportPaymentsInput, SuggestVendorPaymentsInput, SuggestVendorPaymentsResultDto } from './payment-export.models';

@Injectable({
  providedIn: 'root',
})
export class PaymentJournalService {
  apiName = 'Erp';

  export = (id: string, input: ExportPaymentsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankFileDto>({
      method: 'POST',
      url: `/api/erp/payment-journal/${id}/export`,
      body: input,
    },
    { apiName: this.apiName,...config });

  suggestVendorPayments = (id: string, input: SuggestVendorPaymentsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SuggestVendorPaymentsResultDto>({
      method: 'POST',
      url: `/api/erp/payment-journal/${id}/suggest-vendor-payments`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
