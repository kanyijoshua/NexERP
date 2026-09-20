import { mapEnumToOptions } from '@abp/ng.core';

export enum EntityChangeKind {
  None = 0,
  Created = 1,
  Updated = 2,
  Deleted = 4,
  All = 7,
}

export const entityChangeKindOptions = mapEnumToOptions(EntityChangeKind);
