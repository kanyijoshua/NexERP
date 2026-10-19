import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { BudgetMatrixDto, BudgetMatrixInput, CopyBudgetInput, SetBudgetCellInput } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class BudgetMatrixService {
  apiName = 'Erp';

  getMatrix = (input: BudgetMatrixInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BudgetMatrixDto>({
      method: 'GET',
      url: '/api/erp/budget-matrix/matrix',
      params: { budgetName: input.budgetName, accountFilter: input.accountFilter, withActual: input.withActual, incomeStatementOnly: input.incomeStatementOnly, startDate: input.startDate, periodType: input.periodType, periodCount: input.periodCount },
    },
    { apiName: this.apiName,...config });

  setCell = (input: SetBudgetCellInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/erp/budget-matrix/set-cell',
      body: input,
    },
    { apiName: this.apiName,...config });

  copyBudget = (input: CopyBudgetInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: '/api/erp/budget-matrix/copy-budget',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
