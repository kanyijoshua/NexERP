import { mapEnumToOptions } from '@abp/ng.core';

export enum GeneralPostingType {
  None = 0,
  Purchase = 1,
  Sale = 2,
}

export const generalPostingTypeOptions = mapEnumToOptions(GeneralPostingType);
