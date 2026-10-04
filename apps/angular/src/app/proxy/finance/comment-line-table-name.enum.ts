import { mapEnumToOptions } from '@abp/ng.core';

export enum CommentLineTableName {
  GLAccount = 0,
  Customer = 1,
  Vendor = 2,
  Item = 3,
  Resource = 4,
  Job = 5,
  ResourceGroup = 7,
  BankAccount = 8,
  Campaign = 9,
  FixedAsset = 10,
  Insurance = 11,
  NonstockItem = 12,
  ICPartner = 13,
  VendorAgreement = 23,
  CustomerAgreement = 24,
}

export const commentLineTableNameOptions = mapEnumToOptions(CommentLineTableName);
