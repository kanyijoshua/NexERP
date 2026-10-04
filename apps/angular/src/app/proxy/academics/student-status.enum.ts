import { mapEnumToOptions } from '@abp/ng.core';

export enum StudentStatus {
  Registration = 0,
  Current = 1,
  Alumni = 2,
  Deferred = 3,
  Suspended = 4,
  Discontinued = 5,
  Dropped = 6,
  Expelled = 7,
  Deceased = 8,
}

export const studentStatusOptions = mapEnumToOptions(StudentStatus);
