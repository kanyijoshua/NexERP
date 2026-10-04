import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { DetailedCVLedgerEntryType } from './detailed-cv-ledger-entry-type.enum';
import type { GLEntryDocumentType } from '../finance/glentry-document-type.enum';

export interface CustomerBankAccountDto extends FullAuditedEntityDto<string> {
  customerNo?: string;
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

export interface CreateUpdateCustomerBankAccountDto {
  customerNo: string;
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

export interface GetCustomerBankAccountListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  customerNo?: string;
}

export interface DetailedCustLedgEntryDto extends EntityDto<string> {
  entryNo: number;
  custLedgerEntryNo: number;
  entryType: DetailedCVLedgerEntryType;
  postingDate?: string;
  documentType: GLEntryDocumentType;
  documentNo?: string;
  amount: number;
  amountLcy: number;
  customerNo?: string;
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
  appliedCustLedgerEntryNo: number;
  unapplied: boolean;
  unappliedByEntryNo: number;
  remainingPmtDiscPossible: number;
  maxPaymentTolerance: number;
  applicationNo: number;
  ledgerEntryAmount: boolean;
  postingGroup?: string;
  exchRateAdjmtRegNo: number;
}

export interface GetDetailedCustLedgEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  customerNo?: string;
  documentNo?: string;
}
