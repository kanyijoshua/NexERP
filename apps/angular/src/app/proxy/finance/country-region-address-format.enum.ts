import { mapEnumToOptions } from '@abp/ng.core';

export enum CountryRegionAddressFormat {
  PostCodeCity = 0,
  CityPostCode = 1,
  CityCountyPostCode = 2,
  BlankLinePostCodeCity = 3,
  Custom = 13,
}

export const countryRegionAddressFormatOptions = mapEnumToOptions(CountryRegionAddressFormat);
