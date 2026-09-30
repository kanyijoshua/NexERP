import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ExchRateAdjmtAccountType } from './exch-rate-adjmt-account-type.enum';
import type { VatCalculationType } from './vat-calculation-type.enum';
import type { VatEntrySelection } from './vat-entry-selection.enum';
import type { VatEntryType } from './vat-entry-type.enum';

export interface VatReturnInput {
  startingDate?: string;
  endingDate?: string;
  selection: VatEntrySelection;
}

export interface VatReturnLineDto {
  type: VatEntryType;
  vatIdentifier?: string;
  vatCalculationType: VatCalculationType;
  vatPercent: number;
  base: number;
  amount: number;
}

export interface VatReturnDto {
  startingDate?: string;
  endingDate?: string;
  selection: VatEntrySelection;
  lines: VatReturnLineDto[];
  outputVatOnSales: number;
  outputVatOnReverseCharge: number;
  totalOutputVat: number;
  inputVat: number;
  netVatDue: number;
  salesExcludingVat: number;
  purchasesExcludingVat: number;
}

export interface VatSettlementInput {
  startingDate?: string;
  endingDate?: string;
  postingDate?: string;
  documentNo?: string;
  settlementAccountNo?: string;
}

export interface VatSettlementLineDto {
  vatBusPostingGroup?: string;
  vatProdPostingGroup?: string;
  vatIdentifier?: string;
  vatCalculationType: VatCalculationType;
  type: VatEntryType;
  vatPercent: number;
  entryCount: number;
  base: number;
  amount: number;
}

export interface VatSettlementDto {
  lines: VatSettlementLineDto[];
  netVatPayable: number;
  posted: boolean;
  registerNo: number;
}

export interface ExchRateAdjustmentInput {
  endingDate?: string;
  postingDate?: string;
  documentNo?: string;
  currencyCode?: string;
  adjustCustomers: boolean;
  adjustVendors: boolean;
  adjustBankAccounts: boolean;
}

export interface ExchRateAdjustmentLineDto {
  accountType: ExchRateAdjmtAccountType;
  accountNo?: string;
  currencyCode?: string;
  currencyFactor: number;
  base: number;
  oldAmountLcy: number;
  newAmountLcy: number;
  difference: number;
}

export interface ExchRateAdjustmentDto {
  lines: ExchRateAdjustmentLineDto[];
  totalGains: number;
  totalLosses: number;
  posted: boolean;
  registerNo: number;
}

export interface ExchRateAdjmtRegisterDto extends EntityDto<string> {
  postingDate?: string;
  documentNo?: string;
  accountType: ExchRateAdjmtAccountType;
  accountNo?: string;
  currencyCode?: string;
  currencyFactor: number;
  adjustedBase: number;
  adjustedBaseLcy: number;
  adjustedAmount: number;
  glRegisterNo: number;
}

export interface GetExchRateAdjmtRegisterListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
}

export interface PartyLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  partyNo?: string;
  postingDate?: string;
  documentType?: string;
  documentNo?: string;
  description?: string;
  currencyCode?: string;
  amount: number;
  amountLcy: number;
  remainingAmount: number;
  remainingAmountLcy: number;
  dueDate?: string;
  open: boolean;
  closedByEntryNo: number;
  reversed: boolean;
}

export interface GetPartyLedgerEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  partyNo?: string;
  onlyOpen?: boolean;
}
