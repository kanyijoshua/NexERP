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
import type { BeneficiaryRelationship } from './beneficiary-relationship.enum';
import type { BeneficiaryStatus } from './beneficiary-status.enum';
import type { ExcessContributionAllocation } from './excess-contribution-allocation.enum';
import type { PensionerChangeType } from './pensioner-change-type.enum';
import type { PensionIncrementStatus } from './pension-increment-status.enum';
import type { PensionerPaymentType } from './pensioner-payment-type.enum';
import type { PensionerPayItemType } from './pensioner-pay-item-type.enum';
import type { PensionerPayItemCalculation } from './pensioner-pay-item-calculation.enum';
import type { PensionFactorType } from './pension-factor-type.enum';
import type { PensionableSalaryBasis } from './pensionable-salary-basis.enum';

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
  transfersInAccountNo?: string;
  excessContributionAllocation: ExcessContributionAllocation;
  lifeCertificateFrequencyMonths: number;
  incrementNos?: string;
  defaultPayModeCode?: string;
  trivialPensionLimit: number;
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
  pensionableSalaryBasis: PensionableSalaryBasis;
  salaryAveragingYears: number;
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
  pensionableSalaryBasis?: PensionableSalaryBasis;
  salaryAveragingYears?: number;
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
  transferSchemeCode?: string;
}

export interface CreateUpdatePensionContributionHeaderDto {
  no?: string;
  sponsorNo: string;
  postingDate?: string;
  contributionPeriod?: string;
  description?: string;
  contributionMode?: PensionContributionMode;
  transferSchemeCode?: string;
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
  applyVestingScale: boolean;
}

export interface CreateUpdateExitReasonDto extends CreateUpdateCodeTableDto {
  paymentOption?: ExitPaymentOption;
  employerPortionPct?: number;
  taxTableCode?: string;
  lumpsumTaxFree?: boolean;
  statusAfterExit?: MemberStatus;
  applyVestingScale?: boolean;
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
  taxExempt: boolean;
  arrearsAmount: number;
  arrearsMonths: number;
  suspensionReason?: string;
  lastLifeCertificateDate?: string;
  lifeCertificateDueDate?: string;
  payModeCode?: string;
  bankCode?: string;
  bankBranchCode?: string;
  suspensionReasonCode?: string;
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
  taxExempt?: boolean;
  payModeCode?: string;
  bankCode?: string;
  bankBranchCode?: string;
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
  taxTableCode?: string;
  personalRelief: number;
  totalDeductions: number;
}

export interface CreateUpdatePensionPayrollHeaderDto {
  no?: string;
  schemeCode: string;
  payPeriod?: string;
  postingDate?: string;
  description?: string;
  taxRatePct?: number;
  taxFreeAmount?: number;
  taxTableCode?: string;
  personalRelief?: number;
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
  arrearsAmount: number;
  arrearsMonths: number;
  monthlyPension: number;
  otherEarnings: number;
  deductions: number;
  payModeCode?: string;
}

export interface CreateUpdatePensionPayrollLineDto {
  documentNo: string;
  lineNo?: number;
  pensionerNo: string;
  monthlyPension?: number;
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
  ageFactor: number;
  trivial: boolean;
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

export interface PensionBeneficiaryDto extends FullAuditedEntityDto<string> {
  memberNo?: string;
  lineNo: number;
  name?: string;
  relationship: BeneficiaryRelationship;
  dateOfBirth?: string;
  nationalId?: string;
  benefitPct: number;
  status: BeneficiaryStatus;
  guardianName?: string;
  phoneNo?: string;
  email?: string;
  bankName?: string;
  bankAccountNo?: string;
}

export interface CreateUpdatePensionBeneficiaryDto {
  memberNo: string;
  lineNo?: number;
  name: string;
  relationship?: BeneficiaryRelationship;
  dateOfBirth?: string;
  nationalId?: string;
  benefitPct?: number;
  status?: BeneficiaryStatus;
  guardianName?: string;
  phoneNo?: string;
  email?: string;
  bankName?: string;
  bankAccountNo?: string;
}

export interface GetPensionBeneficiaryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  memberNo?: string;
}

export interface PensionContributionRateDto extends FullAuditedEntityDto<string> {
  sponsorNo?: string;
  startDate?: string;
  endDate?: string;
  employeeRatePct: number;
  employerRatePct: number;
}

export interface CreateUpdatePensionContributionRateDto {
  sponsorNo: string;
  startDate?: string;
  endDate?: string;
  employeeRatePct?: number;
  employerRatePct?: number;
}

export interface GetPensionContributionRateListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  sponsorNo?: string;
}

export interface PensionVestingScaleDto extends FullAuditedEntityDto<string> {
  sponsorNo?: string;
  fromServiceYears: number;
  employerVestedPct: number;
}

export interface CreateUpdatePensionVestingScaleDto {
  sponsorNo: string;
  fromServiceYears?: number;
  employerVestedPct?: number;
}

export interface GetPensionVestingScaleListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  sponsorNo?: string;
}

export interface PensionTaxReliefLimitDto extends FullAuditedEntityDto<string> {
  effectiveDate?: string;
  monthlyLimit: number;
}

export interface CreateUpdatePensionTaxReliefLimitDto {
  effectiveDate?: string;
  monthlyLimit?: number;
}

export interface GetPensionTaxReliefLimitListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
}

export interface MemberStatusEntryDto extends EntityDto<string> {
  memberNo?: string;
  schemeCode?: string;
  sponsorNo?: string;
  effectiveDate?: string;
  fromStatus: MemberStatus;
  toStatus: MemberStatus;
  documentNo?: string;
  userName?: string;
}

export interface GetMemberStatusEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  memberNo?: string;
  schemeCode?: string;
}

export interface MemberSalaryEntryDto extends EntityDto<string> {
  memberNo?: string;
  sponsorNo?: string;
  period?: string;
  salary: number;
  documentNo?: string;
}

export interface GetMemberSalaryEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  memberNo?: string;
}

export interface PensionIncrementDto extends FullAuditedEntityDto<string> {
  no?: string;
  schemeCode?: string;
  effectiveDate?: string;
  incrementPct: number;
  minimumMonthlyPension: number;
  description?: string;
  status: PensionIncrementStatus;
  appliedDate?: string;
  appliedBy?: string;
  noOfPensioners: number;
  totalMonthlyIncrease: number;
  totalArrears: number;
  reasonCode?: string;
}

export interface CreateUpdatePensionIncrementDto {
  no?: string;
  schemeCode: string;
  effectiveDate?: string;
  incrementPct?: number;
  minimumMonthlyPension?: number;
  description?: string;
  reasonCode?: string;
}

export interface GetPensionIncrementListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  schemeCode?: string;
  status?: PensionIncrementStatus;
}

export interface PensionerChangeEntryDto extends EntityDto<string> {
  pensionerNo?: string;
  schemeCode?: string;
  changeType: PensionerChangeType;
  effectiveDate?: string;
  oldMonthlyPension: number;
  newMonthlyPension: number;
  amount: number;
  description?: string;
  documentNo?: string;
  userName?: string;
}

export interface GetPensionerChangeEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  pensionerNo?: string;
}

export interface PensionerActionInput {
  date?: string;
  reason?: string;
  reasonCode?: string;
}

export interface SuspendOverduePensionersInput {
  schemeCode: string;
  asOfDate?: string;
}

export interface SuspendOverduePensionersResultDto {
  noOfPensioners: number;
}

export interface PensionBankDto extends CodeTableDto {
  swiftCode?: string;
}

export interface CreateUpdatePensionBankDto extends CreateUpdateCodeTableDto {
  swiftCode?: string;
}

export interface PensionBankBranchDto extends FullAuditedEntityDto<string> {
  bankCode?: string;
  branchCode?: string;
  name?: string;
  swiftCode?: string;
}

export interface CreateUpdatePensionBankBranchDto {
  bankCode: string;
  branchCode: string;
  name: string;
  swiftCode?: string;
}

export interface GetPensionBankBranchListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  bankCode?: string;
}

export interface PensionerPayModeDto extends CodeTableDto {
  paymentType: PensionerPaymentType;
}

export interface CreateUpdatePensionerPayModeDto extends CreateUpdateCodeTableDto {
  paymentType?: PensionerPaymentType;
}

export interface PensionerSuspensionReasonDto extends CodeTableDto {
  lifeCertificate: boolean;
}

export interface CreateUpdatePensionerSuspensionReasonDto extends CreateUpdateCodeTableDto {
  lifeCertificate?: boolean;
}

export interface OtherPensionSchemeDto extends CodeTableDto {
  regulatorReferenceNo?: string;
  address?: string;
  city?: string;
  contactName?: string;
  phoneNo?: string;
  email?: string;
  bankCode?: string;
  bankBranchCode?: string;
  bankAccountNo?: string;
}

export interface CreateUpdateOtherPensionSchemeDto extends CreateUpdateCodeTableDto {
  regulatorReferenceNo?: string;
  address?: string;
  city?: string;
  contactName?: string;
  phoneNo?: string;
  email?: string;
  bankCode?: string;
  bankBranchCode?: string;
  bankAccountNo?: string;
}

export interface PensionerPayItemDto extends CodeTableDto {
  itemType: PensionerPayItemType;
  calculation: PensionerPayItemCalculation;
  amount: number;
  pct: number;
  taxable: boolean;
  accountNo?: string;
  blocked: boolean;
}

export interface CreateUpdatePensionerPayItemDto extends CreateUpdateCodeTableDto {
  itemType?: PensionerPayItemType;
  calculation?: PensionerPayItemCalculation;
  amount?: number;
  pct?: number;
  taxable?: boolean;
  accountNo?: string;
  blocked?: boolean;
}

export interface PensionerPayItemAssignmentDto extends FullAuditedEntityDto<string> {
  pensionerNo?: string;
  payItemCode?: string;
  payItemDescription?: string;
  itemType: PensionerPayItemType;
  amount: number;
  startDate?: string;
  endDate?: string;
  comment?: string;
}

export interface CreateUpdatePensionerPayItemAssignmentDto {
  pensionerNo: string;
  payItemCode: string;
  amount?: number;
  startDate?: string;
  endDate?: string;
  comment?: string;
}

export interface GetPensionerPayItemAssignmentListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  pensionerNo?: string;
}

export interface PensionPayrollLineItemDto extends EntityDto<string> {
  documentNo?: string;
  lineNo: number;
  pensionerNo?: string;
  payItemCode?: string;
  description?: string;
  itemType: PensionerPayItemType;
  taxable: boolean;
  accountNo?: string;
  amount: number;
}

export interface GetPensionPayrollLineItemListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  documentNo?: string;
  lineNo?: number;
  pensionerNo?: string;
}

export interface ExitReasonDocumentDto extends FullAuditedEntityDto<string> {
  exitReasonCode?: string;
  lineNo: number;
  documentName?: string;
  mandatory: boolean;
}

export interface CreateUpdateExitReasonDocumentDto {
  exitReasonCode: string;
  lineNo?: number;
  documentName: string;
  mandatory?: boolean;
}

export interface GetExitReasonDocumentListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  exitReasonCode?: string;
}

export interface MemberExitDocumentDto extends FullAuditedEntityDto<string> {
  exitNo?: string;
  lineNo: number;
  documentName?: string;
  mandatory: boolean;
  received: boolean;
  receivedDate?: string;
  remarks?: string;
}

export interface CreateUpdateMemberExitDocumentDto {
  exitNo: string;
  lineNo?: number;
  documentName: string;
  mandatory?: boolean;
  received?: boolean;
  receivedDate?: string;
  remarks?: string;
}

export interface GetMemberExitDocumentListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  exitNo?: string;
}

export interface PensionAgeFactorDto extends FullAuditedEntityDto<string> {
  schemeCode?: string;
  factorType: PensionFactorType;
  age: number;
  maleFactor: number;
  femaleFactor: number;
}

export interface CreateUpdatePensionAgeFactorDto {
  schemeCode: string;
  factorType?: PensionFactorType;
  age?: number;
  maleFactor?: number;
  femaleFactor?: number;
}

export interface GetPensionAgeFactorListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  schemeCode?: string;
  factorType?: PensionFactorType;
}

export interface CopyExitDocumentsResultDto {
  noOfDocuments: number;
}
