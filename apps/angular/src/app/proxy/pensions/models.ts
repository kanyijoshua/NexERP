import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { ExitPaymentOption } from './exit-payment-option.enum';
import type { InterestCalculationMode } from './interest-calculation-mode.enum';
import type { MemberContributionStatus } from './member-contribution-status.enum';
import type { MemberExitStatus } from './member-exit-status.enum';
import type { MemberGender } from './member-gender.enum';
import type { MemberMaritalStatus } from './member-marital-status.enum';
import type { MemberStatus } from './member-status.enum';
import type { MemberWithdrawalType } from './member-withdrawal-type.enum';
import type { PensionContributionMode } from './pension-contribution-mode.enum';
import type { PensionContributionType } from './pension-contribution-type.enum';
import type { PensionDocumentStatus } from './pension-document-status.enum';
import type { PensionerStatus } from './pensioner-status.enum';
import type { PensionExemptionType } from './pension-exemption-type.enum';
import type { PensionPlanType } from './pension-plan-type.enum';
import type { PensionSchemeMode } from './pension-scheme-mode.enum';
import type { PensionSchemeStatus } from './pension-scheme-status.enum';
import type { PensionSchemeType } from './pension-scheme-type.enum';
import type { PensionTransactionType } from './pension-transaction-type.enum';
import type { BenefitCalculationStatus } from './benefit-calculation-status.enum';

export interface PensionSetupDto {
  schemeDimensionCode?: string;
  memberNos?: string;
  sponsorNos?: string;
  contributionNos?: string;
  interestBatchNos?: string;
  exitNos?: string;
  memberFundsAccountNo?: string;
  contributionAccrualAccountNo?: string;
  interestAccountNo?: string;
  benefitsPayableAccountNo?: string;
  taxAccountNo?: string;
  allowContributionDuplication: boolean;
  noOfDaysInAYear: number;
  pensionerNos?: string;
  payrollNos?: string;
  pensionsPaidAccountNo?: string;
  benefitCalculationNos?: string;
}

export interface PensionSchemeDto extends CodeTableDto {
  schemeType: PensionSchemeType;
  planType: PensionPlanType;
  schemeMode: PensionSchemeMode;
  status: PensionSchemeStatus;
  interestCalculationMode: InterestCalculationMode;
  regulatorReferenceNo?: string;
  taxPinNo?: string;
  normalRetirementAge: number;
  minimumRetirementAge: number;
  accrualRatePct: number;
  maxPensionableServiceYears: number;
  maxCommutationPct: number;
  commutationFactor: number;
  earlyRetirementReductionPct: number;
}

export interface CreateUpdatePensionSchemeDto extends CreateUpdateCodeTableDto {
  schemeType?: PensionSchemeType;
  planType?: PensionPlanType;
  schemeMode?: PensionSchemeMode;
  status?: PensionSchemeStatus;
  interestCalculationMode?: InterestCalculationMode;
  regulatorReferenceNo?: string;
  taxPinNo?: string;
  normalRetirementAge?: number;
  minimumRetirementAge?: number;
  accrualRatePct?: number;
  maxPensionableServiceYears?: number;
  maxCommutationPct?: number;
  commutationFactor?: number;
  earlyRetirementReductionPct?: number;
}

export interface PensionSponsorDto extends FullAuditedEntityDto<string> {
  no?: string;
  name?: string;
  schemeCode?: string;
  customerNo?: string;
  address?: string;
  city?: string;
  phoneNo?: string;
  email?: string;
  contact?: string;
  taxPinNo?: string;
  employeeRatePct: number;
  employerRatePct: number;
  lastScheduleDate?: string;
  blocked: boolean;
}

export interface CreateUpdatePensionSponsorDto {
  no?: string;
  name: string;
  schemeCode: string;
  customerNo?: string;
  address?: string;
  city?: string;
  phoneNo?: string;
  email?: string;
  contact?: string;
  taxPinNo?: string;
  employeeRatePct?: number;
  employerRatePct?: number;
  blocked?: boolean;
}

export interface GetPensionSponsorListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  schemeCode?: string;
}

export interface PensionMemberDto extends FullAuditedEntityDto<string> {
  no?: string;
  schemeCode?: string;
  sponsorNo?: string;
  firstName?: string;
  otherName?: string;
  lastName?: string;
  fullName?: string;
  nationalId?: string;
  taxPinNo?: string;
  gender: MemberGender;
  dateOfBirth?: string;
  maritalStatus: MemberMaritalStatus;
  payrollNo?: string;
  designation?: string;
  dateOfEmployment?: string;
  joinSchemeDate?: string;
  currentSalary: number;
  expectedRetirementDate?: string;
  status: MemberStatus;
  contributionStatus: MemberContributionStatus;
  exitDate?: string;
  address?: string;
  city?: string;
  phoneNo?: string;
  email?: string;
  bankName?: string;
  bankBranch?: string;
  bankAccountNo?: string;
}

export interface CreateUpdatePensionMemberDto {
  no?: string;
  sponsorNo: string;
  firstName: string;
  otherName?: string;
  lastName?: string;
  nationalId?: string;
  taxPinNo?: string;
  gender?: MemberGender;
  dateOfBirth?: string;
  maritalStatus?: MemberMaritalStatus;
  payrollNo?: string;
  designation?: string;
  dateOfEmployment?: string;
  joinSchemeDate?: string;
  currentSalary?: number;
  status?: MemberStatus;
  contributionStatus?: MemberContributionStatus;
  address?: string;
  city?: string;
  phoneNo?: string;
  email?: string;
  bankName?: string;
  bankBranch?: string;
  bankAccountNo?: string;
}

export interface GetPensionMemberListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  schemeCode?: string;
  sponsorNo?: string;
  status?: MemberStatus;
}

export interface MemberBalanceLineDto {
  contributionType: PensionContributionType;
  exemptionType: PensionExemptionType;
  amount: number;
}

export interface MemberBalanceDto {
  memberNo?: string;
  asOfDate?: string;
  total: number;
  employee: number;
  employer: number;
  registered: number;
  unregistered: number;
  lines: MemberBalanceLineDto[];
}

export interface MemberLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  schemeCode?: string;
  memberNo?: string;
  sponsorNo?: string;
  postingDate?: string;
  contributionPeriod?: string;
  documentNo?: string;
  description?: string;
  transactionType: PensionTransactionType;
  contributionType: PensionContributionType;
  contributionMode: PensionContributionMode;
  exemptionType: PensionExemptionType;
  amount: number;
  salary: number;
  userName?: string;
}

export interface GetMemberLedgerEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  memberNo?: string;
  schemeCode?: string;
  documentNo?: string;
}

export interface PensionContributionHeaderDto extends FullAuditedEntityDto<string> {
  no?: string;
  schemeCode?: string;
  sponsorNo?: string;
  sponsorName?: string;
  postingDate?: string;
  contributionPeriod?: string;
  description?: string;
  contributionMode: PensionContributionMode;
  status: PensionDocumentStatus;
  postedDate?: string;
  postedBy?: string;
  totalAmount: number;
  noOfMembers: number;
}

export interface CreateUpdatePensionContributionHeaderDto {
  no?: string;
  sponsorNo: string;
  postingDate?: string;
  contributionPeriod?: string;
  description?: string;
  contributionMode?: PensionContributionMode;
}

export interface GetPensionContributionListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  sponsorNo?: string;
  status?: PensionDocumentStatus;
}

export interface PensionContributionLineDto extends FullAuditedEntityDto<string> {
  documentNo?: string;
  lineNo: number;
  memberNo?: string;
  memberName?: string;
  basicSalary: number;
  employeeTaxExempt: number;
  employeeNonTaxExempt: number;
  employeeAvcTaxExempt: number;
  employeeAvcNonTaxExempt: number;
  employerTaxExempt: number;
  employerNonTaxExempt: number;
  employerAvcTaxExempt: number;
  employerAvcNonTaxExempt: number;
  totalAmount: number;
}

export interface CreateUpdatePensionContributionLineDto {
  documentNo: string;
  lineNo?: number;
  memberNo: string;
  basicSalary?: number;
  employeeTaxExempt?: number;
  employeeNonTaxExempt?: number;
  employeeAvcTaxExempt?: number;
  employeeAvcNonTaxExempt?: number;
  employerTaxExempt?: number;
  employerNonTaxExempt?: number;
  employerAvcTaxExempt?: number;
  employerAvcNonTaxExempt?: number;
}

export interface GetPensionContributionLineListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  documentNo?: string;
}

export interface PensionInterestRateDto extends FullAuditedEntityDto<string> {
  schemeCode?: string;
  startDate?: string;
  endDate?: string;
  dateDeclared?: string;
  registeredRatePct: number;
  unregisteredRatePct: number;
  taxRatePct: number;
  posted: boolean;
  postedDocumentNo?: string;
  postedDate?: string;
  totalInterest: number;
  totalTax: number;
}

export interface CreateUpdatePensionInterestRateDto {
  schemeCode: string;
  startDate?: string;
  endDate?: string;
  dateDeclared?: string;
  registeredRatePct?: number;
  unregisteredRatePct?: number;
  taxRatePct?: number;
}

export interface GetPensionInterestRateListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  schemeCode?: string;
}

export interface InterestAllocationLineDto {
  memberNo?: string;
  memberName?: string;
  contributionType: PensionContributionType;
  exemptionType: PensionExemptionType;
  balance: number;
  interest: number;
  tax: number;
}

export interface InterestAllocationDto {
  totalInterest: number;
  totalTax: number;
  noOfMembers: number;
  lines: InterestAllocationLineDto[];
}

export interface AllocateInterestInput {
  postingDate?: string;
}

export interface ExitReasonDto extends CodeTableDto {
  paymentOption: ExitPaymentOption;
  employerPortionPct: number;
  taxTableCode?: string;
  lumpsumTaxFree: boolean;
  statusAfterExit: MemberStatus;
}

export interface CreateUpdateExitReasonDto extends CreateUpdateCodeTableDto {
  paymentOption?: ExitPaymentOption;
  employerPortionPct?: number;
  taxTableCode?: string;
  lumpsumTaxFree?: boolean;
  statusAfterExit?: MemberStatus;
}

export interface LumpsumTaxTableDto extends CodeTableDto {
  annualTaxFreeAmount: number;
  maxTaxFreeAmount: number;
  maxAgeTaxable: number;
}

export interface CreateUpdateLumpsumTaxTableDto extends CreateUpdateCodeTableDto {
  annualTaxFreeAmount?: number;
  maxTaxFreeAmount?: number;
  maxAgeTaxable?: number;
}

export interface LumpsumTaxBandDto extends FullAuditedEntityDto<string> {
  taxTableCode?: string;
  lowerLimit: number;
  upperLimit: number;
  ratePct: number;
}

export interface CreateUpdateLumpsumTaxBandDto {
  taxTableCode: string;
  lowerLimit?: number;
  upperLimit?: number;
  ratePct?: number;
}

export interface GetLumpsumTaxBandListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  taxTableCode?: string;
}

export interface MemberExitDto extends FullAuditedEntityDto<string> {
  no?: string;
  memberNo?: string;
  memberName?: string;
  schemeCode?: string;
  sponsorNo?: string;
  reasonCode?: string;
  withdrawalType: MemberWithdrawalType;
  exitDate?: string;
  dateOfCalculation?: string;
  status: MemberExitStatus;
  comment?: string;
  ageAtExit: number;
  serviceYears: number;
  employeeBalance: number;
  employerBalance: number;
  employeePayable: number;
  employerPayable: number;
  deferredAmount: number;
  registeredPayable: number;
  unregisteredPayable: number;
  grossLumpsum: number;
  taxFreeAmount: number;
  taxableAmount: number;
  taxOnLumpsum: number;
  netPayable: number;
  postedDate?: string;
  postedBy?: string;
  paymentVoucherNo?: string;
}

export interface CreateUpdateMemberExitDto {
  no?: string;
  memberNo: string;
  reasonCode: string;
  withdrawalType?: MemberWithdrawalType;
  exitDate?: string;
  dateOfCalculation?: string;
  comment?: string;
}

export interface GetMemberExitListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  memberNo?: string;
  status?: MemberExitStatus;
}

export interface PostMemberExitInput {
  postingDate?: string;
}

export interface PensionerDto extends FullAuditedEntityDto<string> {
  no?: string;
  schemeCode?: string;
  memberNo?: string;
  name?: string;
  nationalId?: string;
  taxPinNo?: string;
  dateOfBirth?: string;
  monthlyPension: number;
  startDate?: string;
  endDate?: string;
  status: PensionerStatus;
  phoneNo?: string;
  email?: string;
  bankName?: string;
  bankBranch?: string;
  bankAccountNo?: string;
  lastPaidPeriod?: string;
}

export interface CreateUpdatePensionerDto {
  no?: string;
  schemeCode?: string;
  memberNo?: string;
  name?: string;
  nationalId?: string;
  taxPinNo?: string;
  dateOfBirth?: string;
  monthlyPension?: number;
  startDate?: string;
  endDate?: string;
  status?: PensionerStatus;
  phoneNo?: string;
  email?: string;
  bankName?: string;
  bankBranch?: string;
  bankAccountNo?: string;
}

export interface GetPensionerListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  schemeCode?: string;
  status?: PensionerStatus;
}

export interface PensionPayrollHeaderDto extends FullAuditedEntityDto<string> {
  no?: string;
  schemeCode?: string;
  payPeriod?: string;
  postingDate?: string;
  description?: string;
  taxRatePct: number;
  taxFreeAmount: number;
  status: PensionDocumentStatus;
  postedDate?: string;
  postedBy?: string;
  totalGross: number;
  totalTax: number;
  totalNet: number;
  noOfPensioners: number;
  paymentVoucherNo?: string;
}

export interface CreateUpdatePensionPayrollHeaderDto {
  no?: string;
  schemeCode: string;
  payPeriod?: string;
  postingDate?: string;
  description?: string;
  taxRatePct?: number;
  taxFreeAmount?: number;
}

export interface GetPensionPayrollListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  schemeCode?: string;
  status?: PensionDocumentStatus;
}

export interface PensionPayrollLineDto extends FullAuditedEntityDto<string> {
  documentNo?: string;
  lineNo: number;
  pensionerNo?: string;
  pensionerName?: string;
  grossPension: number;
  taxAmount: number;
  netPension: number;
}

export interface CreateUpdatePensionPayrollLineDto {
  documentNo: string;
  lineNo?: number;
  pensionerNo: string;
  grossPension?: number;
  taxAmount?: number;
}

export interface GetPensionPayrollLineListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  documentNo?: string;
  pensionerNo?: string;
}

export interface PensionBenefitCalculationDto extends FullAuditedEntityDto<string> {
  no?: string;
  memberNo?: string;
  memberName?: string;
  schemeCode?: string;
  calculationDate?: string;
  retirementDate?: string;
  finalPensionableSalary: number;
  commutationPct: number;
  comment?: string;
  ageAtRetirement: number;
  pensionableServiceYears: number;
  accrualRatePct: number;
  commutationFactor: number;
  earlyReductionPct: number;
  fullAnnualPension: number;
  reducedAnnualPension: number;
  commutedAnnualPension: number;
  lumpSum: number;
  annualPension: number;
  monthlyPension: number;
  status: BenefitCalculationStatus;
  approvedDate?: string;
  approvedBy?: string;
  pensionerNo?: string;
  paymentVoucherNo?: string;
}

export interface CreateUpdatePensionBenefitCalculationDto {
  no?: string;
  memberNo: string;
  calculationDate?: string;
  retirementDate?: string;
  finalPensionableSalary: number;
  commutationPct: number;
  comment?: string;
}

export interface GetPensionBenefitCalculationListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  schemeCode?: string;
  status?: BenefitCalculationStatus;
}