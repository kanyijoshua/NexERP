import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { CommentLineTableName } from './comment-line-table-name.enum';
import type { CountryRegionAddressFormat } from './country-region-address-format.enum';
import type { CountryRegionContactAddressFormat } from './country-region-contact-address-format.enum';
import type { InvoicePostingPolicy } from './invoice-posting-policy.enum';

export interface CountryRegionDto extends CodeTableDto {
  isoCode?: string;
  isoNumericCode?: string;
  euCountryRegionCode?: string;
  intrastatCode?: string;
  addressFormat: CountryRegionAddressFormat;
  contactAddressFormat: CountryRegionContactAddressFormat;
  vatScheme?: string;
  countyName?: string;
}

export interface CreateUpdateCountryRegionDto extends CreateUpdateCodeTableDto {
  isoCode?: string;
  isoNumericCode?: string;
  euCountryRegionCode?: string;
  intrastatCode?: string;
  addressFormat: CountryRegionAddressFormat;
  contactAddressFormat: CountryRegionContactAddressFormat;
  vatScheme?: string;
  countyName?: string;
}

export interface PostCodeDto extends FullAuditedEntityDto<string> {
  code?: string;
  city?: string;
  searchCity?: string;
  countryRegionCode?: string;
  county?: string;
}

export interface CreateUpdatePostCodeDto {
  code: string;
  city: string;
  searchCity?: string;
  countryRegionCode?: string;
  county?: string;
}

export interface GetPostCodeListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
}

export interface ResponsibilityCenterDto extends CodeTableDto {
  address?: string;
  address2?: string;
  city?: string;
  postCode?: string;
  countryRegionCode?: string;
  phoneNo?: string;
  faxNo?: string;
  name2?: string;
  contact?: string;
  globalDimension1Code?: string;
  globalDimension2Code?: string;
  locationCode?: string;
  county?: string;
  email?: string;
}

export interface CreateUpdateResponsibilityCenterDto extends CreateUpdateCodeTableDto {
  address?: string;
  address2?: string;
  city?: string;
  postCode?: string;
  countryRegionCode?: string;
  phoneNo?: string;
  faxNo?: string;
  name2?: string;
  contact?: string;
  globalDimension1Code?: string;
  globalDimension2Code?: string;
  locationCode?: string;
  county?: string;
  email?: string;
}

export interface UserSetupDto extends FullAuditedEntityDto<string> {
  userId?: string;
  allowPostingFrom?: string;
  allowPostingTo?: string;
  registerTime: boolean;
  allowDeferralPostingFrom?: string;
  allowDeferralPostingTo?: string;
  salespersPurchCode?: string;
  approverId?: string;
  salesAmountApprovalLimit: number;
  purchaseAmountApprovalLimit: number;
  unlimitedSalesApproval: boolean;
  unlimitedPurchaseApproval: boolean;
  substitute?: string;
  email?: string;
  phoneNo?: string;
  requestAmountApprovalLimit: number;
  unlimitedRequestApproval: boolean;
  approvalAdministrator: boolean;
  allowVatDateFrom?: string;
  allowVatDateTo?: string;
  salesInvoicePostingPolicy: InvoicePostingPolicy;
  purchInvoicePostingPolicy: InvoicePostingPolicy;
  allowFAPostingFrom?: string;
  allowFAPostingTo?: string;
  salesRespCtrFilter?: string;
  purchaseRespCtrFilter?: string;
}

export interface CreateUpdateUserSetupDto {
  userId: string;
  allowPostingFrom?: string;
  allowPostingTo?: string;
  registerTime: boolean;
  allowDeferralPostingFrom?: string;
  allowDeferralPostingTo?: string;
  salespersPurchCode?: string;
  approverId?: string;
  salesAmountApprovalLimit: number;
  purchaseAmountApprovalLimit: number;
  unlimitedSalesApproval: boolean;
  unlimitedPurchaseApproval: boolean;
  substitute?: string;
  email?: string;
  phoneNo?: string;
  requestAmountApprovalLimit: number;
  unlimitedRequestApproval: boolean;
  approvalAdministrator: boolean;
  allowVatDateFrom?: string;
  allowVatDateTo?: string;
  salesInvoicePostingPolicy: InvoicePostingPolicy;
  purchInvoicePostingPolicy: InvoicePostingPolicy;
  allowFAPostingFrom?: string;
  allowFAPostingTo?: string;
  salesRespCtrFilter?: string;
  purchaseRespCtrFilter?: string;
}

export interface GetUserSetupListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
}

export interface GLBudgetNameDto extends FullAuditedEntityDto<string> {
  name?: string;
  description?: string;
  blocked: boolean;
  budgetDimension1Code?: string;
  budgetDimension2Code?: string;
  budgetDimension3Code?: string;
  budgetDimension4Code?: string;
}

export interface CreateUpdateGLBudgetNameDto {
  name: string;
  description?: string;
  blocked: boolean;
  budgetDimension1Code?: string;
  budgetDimension2Code?: string;
  budgetDimension3Code?: string;
  budgetDimension4Code?: string;
}

export interface GetGLBudgetNameListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
}

export interface GLBudgetEntryDto extends FullAuditedEntityDto<string> {
  entryNo: number;
  budgetName?: string;
  glAccountNo?: string;
  date?: string;
  globalDimension1Code?: string;
  globalDimension2Code?: string;
  amount: number;
  description?: string;
  businessUnitCode?: string;
  budgetDimension1Code?: string;
  budgetDimension2Code?: string;
  budgetDimension3Code?: string;
  budgetDimension4Code?: string;
}

export interface CreateUpdateGLBudgetEntryDto {
  entryNo: number;
  budgetName: string;
  glAccountNo: string;
  date: string;
  globalDimension1Code?: string;
  globalDimension2Code?: string;
  amount: number;
  description?: string;
  businessUnitCode?: string;
  budgetDimension1Code?: string;
  budgetDimension2Code?: string;
  budgetDimension3Code?: string;
  budgetDimension4Code?: string;
}

export interface GetGLBudgetEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  budgetName?: string;
  glAccountNo?: string;
}

export interface CommentLineDto extends FullAuditedEntityDto<string> {
  tableName: CommentLineTableName;
  no?: string;
  lineNo: number;
  date?: string;
  code?: string;
  comment?: string;
}

export interface CreateUpdateCommentLineDto {
  tableName: CommentLineTableName;
  no: string;
  lineNo: number;
  date?: string;
  code?: string;
  comment?: string;
}

export interface GetCommentLineListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  no?: string;
}
