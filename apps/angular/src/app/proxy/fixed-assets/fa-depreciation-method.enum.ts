import { mapEnumToOptions } from '@abp/ng.core';

export enum FADepreciationMethod {
  StraightLine = 0,
  DecliningBalance1 = 1,
  DecliningBalance2 = 2,
  Db1SL = 3,
  Db2SL = 4,
  UserDefined = 5,
  Manual = 6,
}

export const faDepreciationMethodOptions = mapEnumToOptions(FADepreciationMethod);
