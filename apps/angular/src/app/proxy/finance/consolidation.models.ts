import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';

export interface BusinessUnitDto extends CodeTableDto {
  subsidiaryCompanyId?: string;
  consolidate: boolean;
  consolidationPct: number;
  currencyCode?: string;
  residualAccountNo?: string;
  lastRunStartingDate?: string;
  lastRunEndingDate?: string;
}

export interface CreateUpdateBusinessUnitDto extends CreateUpdateCodeTableDto {
  subsidiaryCompanyId?: string;
  consolidate: boolean;
  consolidationPct: number;
  currencyCode?: string;
  residualAccountNo?: string;
}

export interface ConsolidationPeriodInput {
  startingDate: string;
  endingDate: string;
}

export interface RunConsolidationInput extends ConsolidationPeriodInput {
  documentNo: string;
  businessUnitId?: string;
}

export interface ConsolidationLineDto {
  businessUnitCode?: string;
  subsidiaryAccountNo?: string;
  consolidationAccountNo?: string;
  translationMethod: number;
  subsidiaryAmount: number;
  amount: number;
}
