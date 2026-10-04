import { mapEnumToOptions } from '@abp/ng.core';

export enum DepreciationBookDisposalCalculationMethod {
  Net = 0,
  Gross = 1,
}

export const depreciationBookDisposalCalculationMethodOptions = mapEnumToOptions(DepreciationBookDisposalCalculationMethod);
