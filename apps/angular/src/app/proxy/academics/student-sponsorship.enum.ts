import { mapEnumToOptions } from '@abp/ng.core';

export enum StudentSponsorship {
  None = 0,
  Self = 1,
  Corporate = 2,
  Government = 3,
}

export const studentSponsorshipOptions = mapEnumToOptions(StudentSponsorship);
