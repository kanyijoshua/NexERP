import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { CreateUpdatePostingGroupDto, PostingGroupDto } from '../finance/models';
import type { GenJournalAccountType } from '../finance/gen-journal-account-type.enum';
import type { GLEntryDocumentType } from '../finance/glentry-document-type.enum';

export interface BankAccountPostingGroupDto extends PostingGroupDto {
  glAccountNo?: string;
}

export interface CreateUpdateBankAccountPostingGroupDto extends CreateUpdatePostingGroupDto {
  glAccountNo: string;
}

export interface BankAccountDto extends FullAuditedEntityDto<string> {
  no?: string;
  name?: string;
  bankAccountNo?: string;
  bankBranchNo?: string;
  iban?: string;
  swiftCode?: string;
  currencyCode?: string;
  bankAccPostingGroup?: string;
  address?: string;
  city?: string;
  phoneNo?: string;
  contact?: string;
  balance: number;
  balanceLcy: number;
  blocked: boolean;
}

export interface CreateUpdateBankAccountDto {
  no?: string;
  name: string;
  bankAccountNo?: string;
  bankBranchNo?: string;
  iban?: string;
  swiftCode?: string;
  currencyCode?: string;
  bankAccPostingGroup?: string;
  address?: string;
  city?: string;
  phoneNo?: string;
  contact?: string;
}

export interface GetBankAccountListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
}

export interface BankAccountLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  bankAccountId?: string;
  bankAccountNo?: string;
  postingDate?: string;
  documentType: GLEntryDocumentType;
  documentNo?: string;
  description?: string;
  amount: number;
  amountLcy: number;
  currencyCode?: string;
  remainingAmount: number;
  open: boolean;
  reversed: boolean;
}

export interface GetBankAccountLedgerEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  bankAccountId?: string;
}

export interface PaymentMethodDto extends CodeTableDto {
  balAccountType?: GenJournalAccountType;
  balAccountNo?: string;
}

export interface CreateUpdatePaymentMethodDto extends CreateUpdateCodeTableDto {
  balAccountType?: GenJournalAccountType;
  balAccountNo?: string;
}
