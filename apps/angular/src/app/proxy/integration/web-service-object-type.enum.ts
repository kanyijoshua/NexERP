import { mapEnumToOptions } from '@abp/ng.core';

export enum WebServiceObjectType {
  Page = 0,
  Query = 1,
}

export const webServiceObjectTypeOptions = mapEnumToOptions(WebServiceObjectType);
