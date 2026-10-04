import { mapEnumToOptions } from '@abp/ng.core';

export enum MemberWithdrawalType {
  Actual = 0,
  Projection = 1,
}

export const memberWithdrawalTypeOptions = mapEnumToOptions(MemberWithdrawalType);
