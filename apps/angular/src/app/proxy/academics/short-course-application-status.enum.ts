import { mapEnumToOptions } from '@abp/ng.core';

export enum ShortCourseApplicationStatus {
  Open = 0,
  Submitted = 1,
  Approved = 2,
  Rejected = 3,
  Registered = 4,
}

export const shortCourseApplicationStatusOptions = mapEnumToOptions(ShortCourseApplicationStatus);
