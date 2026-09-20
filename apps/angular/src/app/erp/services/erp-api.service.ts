import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface GLAccountDto {
  id: string;
  no: string;
  name: string;
  accountType: number;
  accountCategory: number;
  incomeBalance: number;
  netChange: number;
  balance: number;
}

export interface SalesHeaderDto {
  id: string;
  no: string;
  sellToCustomerNo: string;
  sellToCustomerName: string;
  postingDate: string;
  totalAmount: number;
  totalAmountIncludingVat: number;
  posted: boolean;
}

export interface PurchaseHeaderDto {
  id: string;
  no: string;
  buyFromVendorNo: string;
  buyFromVendorName: string;
  postingDate: string;
  totalAmount: number;
  totalAmountIncludingVat: number;
  posted: boolean;
}

export interface FinancialReportDto {
  reportTitle: string;
  fromDate: string;
  toDate: string;
  rows: { rowNo: string; description: string; amount: number }[];
}

@Injectable({
  providedIn: 'root',
})
export class ErpApiService {
  constructor(private restService: RestService) {}

  getGLAccounts(): Observable<GLAccountDto[]> {
    return this.restService.request<void, GLAccountDto[]>({
      method: 'GET',
      url: '/api/erp/gl-account',
    });
  }

  getSalesInvoices(): Observable<SalesHeaderDto[]> {
    return this.restService.request<void, SalesHeaderDto[]>({
      method: 'GET',
      url: '/api/erp/sales-document',
    });
  }

  postSalesInvoice(id: string): Observable<any> {
    return this.restService.request<void, any>({
      method: 'POST',
      url: `/api/erp/sales-document/${id}/post`,
    });
  }

  getPurchaseInvoices(): Observable<PurchaseHeaderDto[]> {
    return this.restService.request<void, PurchaseHeaderDto[]>({
      method: 'GET',
      url: '/api/erp/purchase-document',
    });
  }

  getTrialBalance(fromDate: string, toDate: string): Observable<FinancialReportDto> {
    return this.restService.request<void, FinancialReportDto>({
      method: 'GET',
      url: `/api/erp/financial-report/trial-balance?fromDate=${fromDate}&toDate=${toDate}`,
    });
  }

  // Report layouts are served by the generated ReportLayoutService in @proxy/reporting; the
  // hand-written calls that were here described a shape the API no longer has.
}
