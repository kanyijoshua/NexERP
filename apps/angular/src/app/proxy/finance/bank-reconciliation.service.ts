import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { BankRecWorksheetDto, ImportBankStatementInput, MatchAutomaticallyInput, MatchManuallyInput, RemoveMatchInput } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class BankReconciliationService {
  apiName = 'Erp';

  getWorksheet = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankRecWorksheetDto>({
      method: 'GET',
      url: `/api/erp/bank-reconciliation/${id}/worksheet`,
    },
    { apiName: this.apiName,...config });

  importStatement = (id: string, input: ImportBankStatementInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: `/api/erp/bank-reconciliation/${id}/import-statement`,
      body: input,
    },
    { apiName: this.apiName,...config });

  matchAutomatically = (id: string, input: MatchAutomaticallyInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: `/api/erp/bank-reconciliation/${id}/match-automatically`,
      body: input,
    },
    { apiName: this.apiName,...config });

  matchManually = (id: string, input: MatchManuallyInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/bank-reconciliation/${id}/match-manually`,
      body: input,
    },
    { apiName: this.apiName,...config });

  unmatch = (id: string, input: RemoveMatchInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/bank-reconciliation/${id}/unmatch`,
      body: input,
    },
    { apiName: this.apiName,...config });

  transferDifferences = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: `/api/erp/bank-reconciliation/${id}/transfer-differences`,
    },
    { apiName: this.apiName,...config });

  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, string>({
      method: 'POST',
      responseType: 'text',
      url: `/api/erp/bank-reconciliation/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
