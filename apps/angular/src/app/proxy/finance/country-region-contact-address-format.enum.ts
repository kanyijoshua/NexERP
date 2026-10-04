import { mapEnumToOptions } from '@abp/ng.core';

export enum CountryRegionContactAddressFormat {
  First = 0,
  AfterCompanyName = 1,
  Last = 2,
}

export const countryRegionContactAddressFormatOptions = mapEnumToOptions(CountryRegionContactAddressFormat);
