import { mapEnumToOptions } from '@abp/ng.core';

export enum ClinicVisitStatus {
  Open = 0,
  Completed = 1,
  Referred = 2,
}

export const clinicVisitStatusOptions = mapEnumToOptions(ClinicVisitStatus);
