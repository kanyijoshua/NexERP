import { mapEnumToOptions } from '@abp/ng.core';

export enum GLSetupVatCalculation {
  BillToPayToNo = 0,
  SellToBuyFromNo = 1,
}

export const glSetupVatCalculationOptions = mapEnumToOptions(GLSetupVatCalculation);
