import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { DepreciationBookDisposalCalculationMethod } from './depreciation-book-disposal-calculation-method.enum';
import type { FAComponentType } from './fa-component-type.enum';
import type { FADepreciationMethod } from './fa-depreciation-method.enum';
import type { FALedgerEntryDisposalCalculationMethod } from './fa-ledger-entry-disposal-calculation-method.enum';
import type { FALedgerEntryFAPostingCategory } from './fa-ledger-entry-fa-posting-category.enum';
import type { FALedgerEntryFAPostingType } from './fa-ledger-entry-fa-posting-type.enum';
import type { GLEntryDocumentType } from '../finance/glentry-document-type.enum';
import type { GenJournalAccountType } from '../finance/gen-journal-account-type.enum';

export interface FASubclassDto extends CodeTableDto {
  faClassCode?: string;
  defaultFAPostingGroup?: string;
}

export interface CreateUpdateFASubclassDto extends CreateUpdateCodeTableDto {
  faClassCode?: string;
  defaultFAPostingGroup?: string;
}

export interface DepreciationBookDto extends CodeTableDto {
  glIntegrationAcqCost: boolean;
  glIntegrationDepreciation: boolean;
  glIntegrationWriteDown: boolean;
  glIntegrationAppreciation: boolean;
  glIntegrationDisposal: boolean;
  glIntegrationMaintenance: boolean;
  disposalCalculationMethod: DepreciationBookDisposalCalculationMethod;
  allowDeprBelowZero: boolean;
  allowIndexation: boolean;
  useSameFAAndGLPostingDates: boolean;
  useRoundingInPeriodicDepr: boolean;
  allowChangesInDeprFields: boolean;
  defaultFinalRoundingAmount: number;
  defaultEndingBookValue: number;
  markErrorsAsCorrections: boolean;
  allowAcqCostBelowZero: boolean;
  allowIdenticalDocumentNo: boolean;
  fiscalYear365Days: boolean;
}

export interface CreateUpdateDepreciationBookDto extends CreateUpdateCodeTableDto {
  glIntegrationAcqCost: boolean;
  glIntegrationDepreciation: boolean;
  glIntegrationWriteDown: boolean;
  glIntegrationAppreciation: boolean;
  glIntegrationDisposal: boolean;
  glIntegrationMaintenance: boolean;
  disposalCalculationMethod: DepreciationBookDisposalCalculationMethod;
  allowDeprBelowZero: boolean;
  allowIndexation: boolean;
  useSameFAAndGLPostingDates: boolean;
  useRoundingInPeriodicDepr: boolean;
  allowChangesInDeprFields: boolean;
  defaultFinalRoundingAmount: number;
  defaultEndingBookValue: number;
  markErrorsAsCorrections: boolean;
  allowAcqCostBelowZero: boolean;
  allowIdenticalDocumentNo: boolean;
  fiscalYear365Days: boolean;
}

export interface FAPostingGroupDto extends CodeTableDto {
  acquisitionCostAccount?: string;
  accumDepreciationAccount?: string;
  writeDownAccount?: string;
  appreciationAccount?: string;
  acqCostAccOnDisposal?: string;
  accumDeprAccOnDisposal?: string;
  writeDownAccOnDisposal?: string;
  appreciationAccOnDisposal?: string;
  gainsAccOnDisposal?: string;
  lossesAccOnDisposal?: string;
  bookValAccOnDispGain?: string;
  salesAccOnDispGain?: string;
  writeDownBalAccOnDisp?: string;
  apprecBalAccOnDisp?: string;
  maintenanceExpenseAccount?: string;
  maintenanceBalAcc?: string;
  acquisitionCostBalAcc?: string;
  depreciationExpenseAcc?: string;
  writeDownExpenseAcc?: string;
  appreciationBalAccount?: string;
  salesBalAcc?: string;
  salesAccOnDispLoss?: string;
  bookValAccOnDispLoss?: string;
}

export interface CreateUpdateFAPostingGroupDto extends CreateUpdateCodeTableDto {
  acquisitionCostAccount?: string;
  accumDepreciationAccount?: string;
  writeDownAccount?: string;
  appreciationAccount?: string;
  acqCostAccOnDisposal?: string;
  accumDeprAccOnDisposal?: string;
  writeDownAccOnDisposal?: string;
  appreciationAccOnDisposal?: string;
  gainsAccOnDisposal?: string;
  lossesAccOnDisposal?: string;
  bookValAccOnDispGain?: string;
  salesAccOnDispGain?: string;
  writeDownBalAccOnDisp?: string;
  apprecBalAccOnDisp?: string;
  maintenanceExpenseAccount?: string;
  maintenanceBalAcc?: string;
  acquisitionCostBalAcc?: string;
  depreciationExpenseAcc?: string;
  writeDownExpenseAcc?: string;
  appreciationBalAccount?: string;
  salesBalAcc?: string;
  salesAccOnDispLoss?: string;
  bookValAccOnDispLoss?: string;
}

export interface FASetupDto {
  allowPostingToMainAssets: boolean;
  defaultDeprBook?: string;
  allowFAPostingFrom?: string;
  allowFAPostingTo?: string;
  fixedAssetNos?: string;
}

export interface FixedAssetDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  searchDescription?: string;
  description2?: string;
  faClassCode?: string;
  faSubclassCode?: string;
  globalDimension1Code?: string;
  globalDimension2Code?: string;
  locationCode?: string;
  faLocationCode?: string;
  vendorNo?: string;
  mainAssetComponent: FAComponentType;
  componentOfMainAsset?: string;
  budgetedAsset: boolean;
  warrantyDate?: string;
  responsibleEmployee?: string;
  serialNo?: string;
  blocked: boolean;
  maintenanceVendorNo?: string;
  underMaintenance: boolean;
  nextServiceDate?: string;
  inactive: boolean;
  faPostingGroup?: string;
}

export interface CreateUpdateFixedAssetDto {
  no?: string;
  description: string;
  searchDescription?: string;
  description2?: string;
  faClassCode?: string;
  faSubclassCode?: string;
  globalDimension1Code?: string;
  globalDimension2Code?: string;
  locationCode?: string;
  faLocationCode?: string;
  vendorNo?: string;
  mainAssetComponent: FAComponentType;
  componentOfMainAsset?: string;
  budgetedAsset: boolean;
  warrantyDate?: string;
  responsibleEmployee?: string;
  serialNo?: string;
  blocked: boolean;
  maintenanceVendorNo?: string;
  underMaintenance: boolean;
  nextServiceDate?: string;
  inactive: boolean;
  faPostingGroup?: string;
}

export interface GetFixedAssetListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
}

export interface FADepreciationBookDto extends FullAuditedEntityDto<string> {
  faNo?: string;
  depreciationBookCode?: string;
  depreciationMethod: FADepreciationMethod;
  depreciationStartingDate?: string;
  straightLinePct: number;
  noOfDepreciationYears: number;
  noOfDepreciationMonths: number;
  fixedDeprAmount: number;
  decliningBalancePct: number;
  finalRoundingAmount: number;
  endingBookValue: number;
  faPostingGroup?: string;
  depreciationEndingDate?: string;
  acquisitionDate?: string;
  glAcquisitionDate?: string;
  disposalDate?: string;
  lastAcquisitionCostDate?: string;
  lastDepreciationDate?: string;
  lastWriteDownDate?: string;
  lastAppreciationDate?: string;
  lastSalvageValueDate?: string;
  lastMaintenanceDate?: string;
  projectedDisposalDate?: string;
  projectedProceedsOnDisposal: number;
  useHalfYearConvention: boolean;
  defaultFADepreciationBook: boolean;
}

export interface CreateUpdateFADepreciationBookDto {
  faNo: string;
  depreciationBookCode: string;
  depreciationMethod: FADepreciationMethod;
  depreciationStartingDate?: string;
  straightLinePct: number;
  noOfDepreciationYears: number;
  noOfDepreciationMonths: number;
  fixedDeprAmount: number;
  decliningBalancePct: number;
  finalRoundingAmount: number;
  endingBookValue: number;
  faPostingGroup?: string;
  depreciationEndingDate?: string;
  projectedDisposalDate?: string;
  projectedProceedsOnDisposal: number;
  useHalfYearConvention: boolean;
  defaultFADepreciationBook: boolean;
}

export interface GetFADepreciationBookListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  faNo?: string;
}

export interface FALedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  glEntryNo: number;
  faNo?: string;
  faPostingDate?: string;
  postingDate?: string;
  documentType: GLEntryDocumentType;
  documentDate?: string;
  documentNo?: string;
  externalDocumentNo?: string;
  description?: string;
  depreciationBookCode?: string;
  faPostingCategory: FALedgerEntryFAPostingCategory;
  faPostingType: FALedgerEntryFAPostingType;
  amount: number;
  debitAmount: number;
  creditAmount: number;
  reclassificationEntry: boolean;
  partOfBookValue: boolean;
  partOfDepreciableBasis: boolean;
  disposalCalculationMethod: FALedgerEntryDisposalCalculationMethod;
  disposalEntryNo: number;
  noOfDepreciationDays: number;
  quantity: number;
  faSubclassCode?: string;
  faLocationCode?: string;
  faPostingGroup?: string;
  globalDimension1Code?: string;
  globalDimension2Code?: string;
  locationCode?: string;
  userId?: string;
  depreciationMethod: FADepreciationMethod;
  depreciationStartingDate?: string;
  straightLinePct: number;
  noOfDepreciationYears: number;
  journalBatchName?: string;
  sourceCode?: string;
  reasonCode?: string;
  transactionNo: number;
  balAccountType: GenJournalAccountType;
  balAccountNo?: string;
  faClassCode?: string;
  depreciationEndingDate?: string;
  reversed: boolean;
  reversedByEntryNo: number;
  reversedEntryNo: number;
}

export interface GetFALedgerEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  faNo?: string;
  documentNo?: string;
}

export interface MaintenanceRegistrationDto extends FullAuditedEntityDto<string> {
  faNo?: string;
  lineNo: number;
  serviceDate?: string;
  maintenanceVendorNo?: string;
  comment?: string;
  serviceAgentName?: string;
  serviceAgentPhoneNo?: string;
  serviceAgentMobilePhone?: string;
}

export interface CreateUpdateMaintenanceRegistrationDto {
  faNo: string;
  lineNo: number;
  serviceDate?: string;
  maintenanceVendorNo?: string;
  comment?: string;
  serviceAgentName?: string;
  serviceAgentPhoneNo?: string;
  serviceAgentMobilePhone?: string;
}

export interface GetMaintenanceRegistrationListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  faNo?: string;
}

export interface MainAssetComponentDto extends FullAuditedEntityDto<string> {
  mainAssetNo?: string;
  faNo?: string;
  description?: string;
}

export interface CreateUpdateMainAssetComponentDto {
  mainAssetNo: string;
  faNo: string;
  description?: string;
}

export interface GetMainAssetComponentListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  mainAssetNo?: string;
}
