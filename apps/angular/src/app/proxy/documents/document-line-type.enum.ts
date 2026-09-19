import { mapEnumToOptions } from '@abp/ng.core';

export enum DocumentLineType {
  None = 0,
  GLAccount = 1,
  Item = 2,
  Resource = 3,
  FixedAsset = 4,
  ChargeItem = 5,
}

export const documentLineTypeOptions = mapEnumToOptions(DocumentLineType);
