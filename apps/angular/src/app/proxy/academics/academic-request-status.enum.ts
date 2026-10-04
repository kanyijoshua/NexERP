import { mapEnumToOptions } from '@abp/ng.core';

export enum AcademicRequestStatus {
  Open = 0,
  Approved = 1,
}

export const academicRequestStatusOptions = mapEnumToOptions(AcademicRequestStatus);
