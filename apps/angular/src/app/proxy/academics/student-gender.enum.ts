import { mapEnumToOptions } from '@abp/ng.core';

export enum StudentGender {
  None = 0,
  Male = 1,
  Female = 2,
}

export const studentGenderOptions = mapEnumToOptions(StudentGender);
