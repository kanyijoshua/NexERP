import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionerStatus {
  Active = 0,
  Suspended = 1,
  Ceased = 2,
}

export const pensionerStatusOptions = mapEnumToOptions(PensionerStatus);
