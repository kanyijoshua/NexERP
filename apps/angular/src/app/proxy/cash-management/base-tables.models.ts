import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { BankAccRecStmtType } from './bank-acc-rec-stmt-type.enum';
import type { BankAccStatementLineType } from './bank-acc-statement-line-type.enum';
import type { BankPaymentType } from './bank-payment-type.enum';
import type { CheckLedgerEntryCheckType } from './check-ledger-entry-check-type.enum';
import type { CheckLedgerEntryEntryStatus } from './check-ledger-entry-entry-status.enum';
import type { CheckLedgerEntryOriginalEntryStatus } from './check-ledger-entry-original-entry-status.enum';
import type { CheckLedgerEntryStatementStatus } from './check-ledger-entry-statement-status.enum';
import type { GLEntryDocumentType } from '../finance/glentry-document-type.enum';
import type { GenJournalAccountType } from '../finance/gen-journal-account-type.enum';

export interface BankAccReconciliationDto extends FullAuditedEntityDto<string> {
  statementType: BankAccRecStmtType;
  bankAccountNo?: string;
  statementNo?: string;
  statementEndingBalance: number;
  statementDate?: string;
  balanceLastStatement: number;
  shortcutDimension1Code?: string;
  shortcutDimension2Code?: string;
  postPaymentsOnly: boolean;
}

export interface CreateUpdateBankAccReconciliationDto {
  statementType: BankAccRecStmtType;
  bankAccountNo: string;
  statementNo: string;
  statementEndingBalance: number;
  statementDate?: string;
  balanceLastStatement: number;
  shortcutDimension1Code?: string;
  shortcutDimension2Code?: string;
  postPaymentsOnly: boolean;
}

export interface GetBankAccReconciliationListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  bankAccountNo?: string;
}

export interface BankAccReconciliationLineDto extends FullAuditedEntityDto<string> {
  statementType: BankAccRecStmtType;
  bankAccountNo?: string;
  statementNo?: string;
  statementLineNo: number;
  documentNo?: string;
  transactionDate?: string;
  description?: string;
  statementAmount: number;
  difference: number;
  appliedAmount: number;
  valueDate?: string;
  readyForApplication: boolean;
  checkNo?: string;
  relatedPartyName?: string;
  additionalTransactionInfo?: string;
  accountType: GenJournalAccountType;
  accountNo?: string;
  transactionText?: string;
  relatedPartyBankAccNo?: string;
  relatedPartyAddress?: string;
  relatedPartyCity?: string;
  paymentReferenceNo?: string;
  shortcutDimension1Code?: string;
  shortcutDimension2Code?: string;
  transactionId?: string;
}

export interface CreateUpdateBankAccReconciliationLineDto {
  statementType: BankAccRecStmtType;
  bankAccountNo: string;
  statementNo: string;
  statementLineNo: number;
  documentNo?: string;
  transactionDate?: string;
  description?: string;
  statementAmount: number;
  difference: number;
  appliedAmount: number;
  valueDate?: string;
  readyForApplication: boolean;
  checkNo?: string;
  relatedPartyName?: string;
  additionalTransactionInfo?: string;
  accountType: GenJournalAccountType;
  accountNo?: string;
  transactionText?: string;
  relatedPartyBankAccNo?: string;
  relatedPartyAddress?: string;
  relatedPartyCity?: string;
  paymentReferenceNo?: string;
  shortcutDimension1Code?: string;
  shortcutDimension2Code?: string;
  transactionId?: string;
}

export interface GetBankAccReconciliationLineListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  bankAccountNo?: string;
  statementNo?: string;
}

export interface BankAccountStatementDto extends EntityDto<string> {
  bankAccountNo?: string;
  statementNo?: string;
  statementEndingBalance: number;
  statementDate?: string;
  balanceLastStatement: number;
  glBalanceAtPostingDate: number;
  outstdPaymentsAtPosting: number;
  outstdTransactAtPosting: number;
  totalPosDiffAtPosting: number;
  totalNegDiffAtPosting: number;
}

export interface GetBankAccountStatementListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  bankAccountNo?: string;
}

export interface BankAccountStatementLineDto extends EntityDto<string> {
  bankAccountNo?: string;
  statementNo?: string;
  statementLineNo: number;
  documentNo?: string;
  transactionDate?: string;
  description?: string;
  statementAmount: number;
  difference: number;
  appliedAmount: number;
  type: BankAccStatementLineType;
  appliedEntries: number;
  valueDate?: string;
  checkNo?: string;
  transactionId?: string;
}

export interface GetBankAccountStatementLineListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  bankAccountNo?: string;
  statementNo?: string;
}

export interface CheckLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  bankAccountNo?: string;
  bankAccountLedgerEntryNo: number;
  postingDate?: string;
  documentType: GLEntryDocumentType;
  documentNo?: string;
  description?: string;
  amount: number;
  checkDate?: string;
  checkNo?: string;
  checkType: CheckLedgerEntryCheckType;
  bankPaymentType: BankPaymentType;
  entryStatus: CheckLedgerEntryEntryStatus;
  originalEntryStatus: CheckLedgerEntryOriginalEntryStatus;
  balAccountType: GenJournalAccountType;
  balAccountNo?: string;
  open: boolean;
  statementStatus: CheckLedgerEntryStatementStatus;
  statementNo?: string;
  statementLineNo: number;
  userId?: string;
  externalDocumentNo?: string;
}

export interface GetCheckLedgerEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  bankAccountNo?: string;
  documentNo?: string;
}
