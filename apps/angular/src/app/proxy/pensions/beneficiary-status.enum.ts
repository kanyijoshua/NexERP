import { mapEnumToOptions } from '@abp/ng.core';

export enum BeneficiaryStatus {
  Active = 0,
  Suspended = 1,
}

export const beneficiaryStatusOptions = mapEnumToOptions(BeneficiaryStatus);
