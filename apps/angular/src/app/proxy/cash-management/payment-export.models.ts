import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';

export enum PaymentFileFormat {
  Csv = 0,
  Iso20022CreditTransfer = 1,
}
export const paymentFileFormatOptions = mapEnumToOptions(PaymentFileFormat);

export enum DirectDebitMandateType {
  Recurrent = 0,
  OneOff = 1,
}
export const directDebitMandateTypeOptions = mapEnumToOptions(DirectDebitMandateType);

export enum DirectDebitCollectionStatus {
  New = 0,
  FileCreated = 1,
  Posted = 2,
  Closed = 3,
}
export const directDebitCollectionStatusOptions = mapEnumToOptions(DirectDebitCollectionStatus);

export enum DirectDebitSequenceType {
  First = 0,
  Recurrent = 1,
  Final = 2,
  OneOff = 3,
}
export const directDebitSequenceTypeOptions = mapEnumToOptions(DirectDebitSequenceType);

interface ErpPagedListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
}

export interface SuggestVendorPaymentsInput {
  postingDate?: string;
  lastDueDate?: string;
  usePaymentDiscounts: boolean;
  vendorNos?: string;
  bankAccountNo: string;
  documentNo?: string;
}

export interface SuggestVendorPaymentsResultDto {
  linesAdded: number;
}

export interface ExportPaymentsInput {
  format: PaymentFileFormat;
}

export interface BankFileDto {
  fileName?: string;
  contentType?: string;
  content?: string;
  transactionCount: number;
  totalAmount: number;
}

export interface DirectDebitMandateDto extends FullAuditedEntityDto<string> {
  code?: string;
  customerNo?: string;
  customerBankAccountCode?: string;
  validFrom?: string;
  validTo?: string;
  type: DirectDebitMandateType;
  expectedUses: number;
  usedCount: number;
  blocked: boolean;
}

export interface CreateUpdateDirectDebitMandateDto {
  code: string;
  customerNo: string;
  customerBankAccountCode: string;
  validFrom?: string;
  validTo?: string;
  type: DirectDebitMandateType;
  expectedUses: number;
  blocked: boolean;
}

export interface GetDirectDebitMandateListInput extends ErpPagedListInput {
  customerNo?: string;
  filter?: string;
}

export interface DirectDebitCollectionDto extends FullAuditedEntityDto<string> {
  no?: string;
  bankAccountNo?: string;
  creditorIdentifier?: string;
  createdDate?: string;
  status: DirectDebitCollectionStatus;
  entryCount: number;
  totalAmount: number;
}

export interface DirectDebitCollectionEntryDto extends EntityDto<string> {
  directDebitCollectionId?: string;
  lineNo: number;
  customerNo?: string;
  customerName?: string;
  documentNo?: string;
  dueDate?: string;
  currencyCode?: string;
  amount: number;
  mandateCode?: string;
  sequenceType: DirectDebitSequenceType;
}

export interface GetDirectDebitCollectionListInput extends ErpPagedListInput {
  status?: DirectDebitCollectionStatus;
}

export interface GetDirectDebitCollectionEntryListInput extends ErpPagedListInput {
  directDebitCollectionId?: string;
}

export interface MakeDirectDebitCollectionInput {
  fromDueDate: string;
  toDueDate: string;
  bankAccountNo: string;
  creditorIdentifier?: string;
}

export interface MakeDirectDebitReceiptLinesInput {
  genJournalBatchId: string;
  postingDate?: string;
}

export interface DirectDebitReceiptLinesResultDto {
  linesAdded: number;
}
