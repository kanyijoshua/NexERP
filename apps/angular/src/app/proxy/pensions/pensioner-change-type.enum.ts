import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionerChangeType {
  None = 0,
  Increment = 1,
  Suspension = 2,
  Reinstatement = 3,
  LifeCertificate = 4,
  ArrearsPaid = 5,
}

export const pensionerChangeTypeOptions = mapEnumToOptions(PensionerChangeType);
