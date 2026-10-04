import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { CreateUpdatePostingGroupDto, PostingGroupDto } from '../finance/models';
import type { GenJournalAccountType } from '../finance/gen-journal-account-type.enum';
import type { GLEntryDocumentType } from '../finance/glentry-document-type.enum';
import type { DocumentStatus } from '../documents/document-status.enum';
import type { PaymentDeductionType } from './payment-deduction-type.enum';

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
  name2?: string;
  address2?: string;
  postCode?: string;
  county?: string;
  countryRegionCode?: string;
  email?: string;
  faxNo?: string;
  homePage?: string;
  globalDimension1Code?: string;
  globalDimension2Code?: string;
  ourContactCode?: string;
  minBalance: number;
  lastStatementNo?: string;
  balanceLastStatement: number;
  lastPaymentStatementNo?: string;
  lastCheckNo?: string;
  transitNo?: string;
  bankClearingCode?: string;
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
  name2?: string;
  address2?: string;
  postCode?: string;
  county?: string;
  countryRegionCode?: string;
  email?: string;
  faxNo?: string;
  homePage?: string;
  globalDimension1Code?: string;
  globalDimension2Code?: string;
  ourContactCode?: string;
  minBalance?: number;
  lastStatementNo?: string;
  balanceLastStatement?: number;
  lastPaymentStatementNo?: string;
  lastCheckNo?: string;
  transitNo?: string;
  bankClearingCode?: string;
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
  directDebit: boolean;
  directDebitPmtTermsCode?: string;
  pmtExportLineDefinition?: string;
}

export interface CreateUpdatePaymentMethodDto extends CreateUpdateCodeTableDto {
  balAccountType?: GenJournalAccountType;
  balAccountNo?: string;
  directDebit?: boolean;
  directDebitPmtTermsCode?: string;
  pmtExportLineDefinition?: string;
}

export interface CashManagementSetupDto {
  paymentVoucherNos?: string;
}

export interface PaymentDeductionCodeDto extends CodeTableDto {
  deductionType: PaymentDeductionType;
  ratePct: number;
  payableAccountNo?: string;
}

export interface CreateUpdatePaymentDeductionCodeDto extends CreateUpdateCodeTableDto {
  deductionType?: PaymentDeductionType;
  ratePct?: number;
  payableAccountNo?: string;
}

export interface PaymentTypeDto extends CodeTableDto {
  accountType: GenJournalAccountType;
  accountNo?: string;
  vatRatePct: number;
  withholdingTaxCode?: string;
  withholdingVatCode?: string;
  retentionCode?: string;
  blocked: boolean;
}

export interface CreateUpdatePaymentTypeDto extends CreateUpdateCodeTableDto {
  accountType?: GenJournalAccountType;
  accountNo?: string;
  vatRatePct?: number;
  withholdingTaxCode?: string;
  withholdingVatCode?: string;
  retentionCode?: string;
  blocked?: boolean;
}

export interface PaymentVoucherHeaderDto extends FullAuditedEntityDto<string> {
  no?: string;
  documentDate?: string;
  postingDate?: string;
  payMode?: string;
  payingBankAccountNo?: string;
  currencyCode?: string;
  payee?: string;
  onBehalfOf?: string;
  paymentNarration?: string;
  chequeNo?: string;
  chequeDate?: string;
  status: DocumentStatus;
  totalAmount: number;
  totalWithholdingTaxAmount: number;
  totalWithholdingVatAmount: number;
  totalRetentionAmount: number;
  totalNetAmount: number;
  noOfLines: number;
  postedDate?: string;
  postedBy?: string;
  sourceType?: string;
  sourceNo?: string;
}

export interface CreateUpdatePaymentVoucherHeaderDto {
  no?: string;
  documentDate?: string;
  postingDate?: string;
  payMode?: string;
  payingBankAccountNo?: string;
  payee?: string;
  onBehalfOf?: string;
  paymentNarration?: string;
  chequeNo?: string;
  chequeDate?: string;
}

export interface GetPaymentVoucherListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  payingBankAccountNo?: string;
  status?: DocumentStatus;
}

export interface PaymentVoucherChequeInput {
  chequeNo?: string;
  chequeDate?: string;
}

export interface PaymentVoucherLineDto extends FullAuditedEntityDto<string> {
  documentNo?: string;
  lineNo: number;
  paymentTypeCode?: string;
  accountType: GenJournalAccountType;
  accountNo?: string;
  accountName?: string;
  description?: string;
  appliesToDocNo?: string;
  amount: number;
  vatRatePct: number;
  withholdingTaxCode?: string;
  withholdingTaxAmount: number;
  withholdingVatCode?: string;
  withholdingVatAmount: number;
  retentionCode?: string;
  retentionAmount: number;
  netAmount: number;
}

export interface CreateUpdatePaymentVoucherLineDto {
  documentNo: string;
  lineNo?: number;
  paymentTypeCode?: string;
  accountType?: GenJournalAccountType;
  accountNo?: string;
  description?: string;
  appliesToDocNo?: string;
  amount: number;
  vatRatePct?: number;
  withholdingTaxCode?: string;
  withholdingVatCode?: string;
  retentionCode?: string;
}

export interface GetPaymentVoucherLineListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  documentNo?: string;
}
