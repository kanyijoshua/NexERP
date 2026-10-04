import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { DetailedCVLedgerEntryType } from '../sales/detailed-cv-ledger-entry-type.enum';
import type { GLEntryDocumentType } from '../finance/glentry-document-type.enum';
import type { PurchaseCommentDocumentType } from './purchase-comment-document-type.enum';

export interface VendorBankAccountDto extends FullAuditedEntityDto<string> {
  vendorNo?: string;
  code?: string;
  name?: string;
  name2?: string;
  address?: string;
  address2?: string;
  city?: string;
  postCode?: string;
  contact?: string;
  phoneNo?: string;
  bankBranchNo?: string;
  bankAccountNo?: string;
  transitNo?: string;
  currencyCode?: string;
  countryRegionCode?: string;
  county?: string;
  faxNo?: string;
  email?: string;
  iban?: string;
  swiftCode?: string;
  bankClearingCode?: string;
  bankClearingStandard?: string;
}

export interface CreateUpdateVendorBankAccountDto {
  vendorNo: string;
  code: string;
  name?: string;
  name2?: string;
  address?: string;
  address2?: string;
  city?: string;
  postCode?: string;
  contact?: string;
  phoneNo?: string;
  bankBranchNo?: string;
  bankAccountNo?: string;
  transitNo?: string;
  currencyCode?: string;
  countryRegionCode?: string;
  county?: string;
  faxNo?: string;
  email?: string;
  iban?: string;
  swiftCode?: string;
  bankClearingCode?: string;
  bankClearingStandard?: string;
}

export interface GetVendorBankAccountListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  vendorNo?: string;
}

export interface DetailedVendorLedgEntryDto extends EntityDto<string> {
  entryNo: number;
  vendorLedgerEntryNo: number;
  entryType: DetailedCVLedgerEntryType;
  postingDate?: string;
  documentType: GLEntryDocumentType;
  documentNo?: string;
  amount: number;
  amountLcy: number;
  vendorNo?: string;
  currencyCode?: string;
  userId?: string;
  sourceCode?: string;
  transactionNo: number;
  journalBatchName?: string;
  reasonCode?: string;
  debitAmount: number;
  creditAmount: number;
  debitAmountLcy: number;
  creditAmountLcy: number;
  initialEntryDueDate?: string;
  initialEntryGlobalDim1?: string;
  initialEntryGlobalDim2?: string;
  genBusPostingGroup?: string;
  genProdPostingGroup?: string;
  vatBusPostingGroup?: string;
  vatProdPostingGroup?: string;
  initialDocumentType: GLEntryDocumentType;
  appliedVendLedgerEntryNo: number;
  unapplied: boolean;
  unappliedByEntryNo: number;
  remainingPmtDiscPossible: number;
  maxPaymentTolerance: number;
  applicationNo: number;
  ledgerEntryAmount: boolean;
  postingGroup?: string;
  exchRateAdjmtRegNo: number;
}

export interface GetDetailedVendorLedgEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  vendorNo?: string;
  documentNo?: string;
}

export interface OrderAddressDto extends FullAuditedEntityDto<string> {
  vendorNo?: string;
  code?: string;
  name?: string;
  name2?: string;
  address?: string;
  address2?: string;
  city?: string;
  contact?: string;
  phoneNo?: string;
  countryRegionCode?: string;
  faxNo?: string;
  postCode?: string;
  county?: string;
  email?: string;
}

export interface CreateUpdateOrderAddressDto {
  vendorNo: string;
  code: string;
  name?: string;
  name2?: string;
  address?: string;
  address2?: string;
  city?: string;
  contact?: string;
  phoneNo?: string;
  countryRegionCode?: string;
  faxNo?: string;
  postCode?: string;
  county?: string;
  email?: string;
}

export interface GetOrderAddressListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  vendorNo?: string;
}

export interface ItemVendorDto extends FullAuditedEntityDto<string> {
  vendorNo?: string;
  itemNo?: string;
  leadTimeCalculation?: string;
  vendorItemNo?: string;
}

export interface CreateUpdateItemVendorDto {
  vendorNo: string;
  itemNo: string;
  leadTimeCalculation?: string;
  vendorItemNo?: string;
}

export interface GetItemVendorListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  vendorNo?: string;
  itemNo?: string;
}

export interface PurchCommentLineDto extends FullAuditedEntityDto<string> {
  documentType: PurchaseCommentDocumentType;
  no?: string;
  documentLineNo: number;
  lineNo: number;
  date?: string;
  code?: string;
  comment?: string;
}

export interface CreateUpdatePurchCommentLineDto {
  documentType: PurchaseCommentDocumentType;
  no: string;
  documentLineNo: number;
  lineNo: number;
  date?: string;
  code?: string;
  comment?: string;
}

export interface GetPurchCommentLineListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  no?: string;
}
