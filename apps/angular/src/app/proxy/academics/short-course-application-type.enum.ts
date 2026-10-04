import { mapEnumToOptions } from '@abp/ng.core';

export enum ShortCourseApplicationType {
  Individual = 0,
  Corporate = 1,
}

export const shortCourseApplicationTypeOptions = mapEnumToOptions(ShortCourseApplicationType);
