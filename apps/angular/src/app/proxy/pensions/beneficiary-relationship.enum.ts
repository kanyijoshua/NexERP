import { mapEnumToOptions } from '@abp/ng.core';

export enum BeneficiaryRelationship {
  None = 0,
  Spouse = 1,
  Child = 2,
  Parent = 3,
  Sibling = 4,
  Dependant = 5,
  Other = 6,
}

export const beneficiaryRelationshipOptions = mapEnumToOptions(BeneficiaryRelationship);
