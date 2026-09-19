import { mapEnumToOptions } from '@abp/ng.core';

export enum GLAccountType {
  Posting = 0,
  Heading = 1,
  Total = 2,
  BeginTotal = 3,
  EndTotal = 4,
}

export const glAccountTypeOptions = mapEnumToOptions(GLAccountType);
