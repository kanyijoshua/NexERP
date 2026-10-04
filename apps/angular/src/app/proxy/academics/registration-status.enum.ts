import { mapEnumToOptions } from '@abp/ng.core';

export enum RegistrationStatus {
  Open = 0,
  Submitted = 1,
}

export const registrationStatusOptions = mapEnumToOptions(RegistrationStatus);
