import { Injectable, inject } from '@angular/core';
import {
  CreateUpdateLumpsumTaxBandDto,
  CreateUpdateMemberExitDto,
  ExitReasonDocumentService,
  MemberExitDocumentService,
  PensionAgeFactorService,
  PensionableSalaryBasis,
  pensionableSalaryBasisOptions,
  CreateUpdatePensionContributionHeaderDto,
  CreateUpdatePensionContributionLineDto,
  CreateUpdatePensionInterestRateDto,
  CreateUpdatePensionMemberDto,
  CreateUpdatePensionSponsorDto,
  ExitReasonService,
  LumpsumTaxBandDto,
  LumpsumTaxBandService,
  LumpsumTaxTableService,
  MemberExitDto,
  MemberExitService,
  MemberExitStatus,
  MemberLedgerEntryDto,
  MemberLedgerEntryService,
  MemberStatus,
  MemberStatusEntryService,
  MemberSalaryEntryService,
  PensionBeneficiaryService,
  PensionContributionRateService,
  PensionVestingScaleService,
  PensionerService,
  PensionContributionHeaderDto,
  PensionContributionLineDto,
  PensionContributionLineService,
  PensionContributionMode,
  PensionContributionService,
  PensionDocumentStatus,
  PensionInterestRateDto,
  PensionInterestRateService,
  PensionMemberDto,
  PensionMemberService,
  PensionPlanType,
  PensionSchemeService,
  PensionSponsorDto,
  PensionSponsorService,
  exitPaymentOptionOptions,
  interestCalculationModeOptions,
  memberContributionStatusOptions,
  memberExitStatusOptions,
  memberGenderOptions,
  memberMaritalStatusOptions,
  memberStatusOptions,
  memberWithdrawalTypeOptions,
  pensionContributionModeOptions,
  pensionContributionTypeOptions,
  pensionDocumentStatusOptions,
  pensionExemptionTypeOptions,
  pensionPlanTypeOptions,
  pensionSchemeModeOptions,
  pensionSchemeStatusOptions,
  pensionSchemeTypeOptions,
  pensionTransactionTypeOptions,
} from '@proxy/pensions';
import { EMPTY, forkJoin, map } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { codeField, codeTableEntity, enumOptions } from './entity-helpers';

const PERMISSION = 'Erp.Pensions';
const SETUP_PERMISSION = 'Erp.PensionSetup';
const today = () => new Date().toISOString().substring(0, 10);

/** A read-only figure of a card: a balance or a calculated benefit. */
function figure(field: string, labelKey: string, section: string): RecordField {
  return { field, labelKey, type: 'readonly', section, cardOnly: true };
}

/** One money-type amount of a contribution line. */
function amount(field: string, labelKey: string, section: string): RecordField {
  return { field, labelKey, type: 'currency', min: 0, section };
}

/**
 * The pension module: schemes (each a value of the scheme dimension), sponsors, members,
 * contribution schedules, declared interest, exits and the member ledger. Joined into
 * `MasterDataEntities.all`, so every lookup can find them.
 */
@Injectable({ providedIn: 'root' })
export class PensionEntities {
  private readonly schemes = inject(PensionSchemeService);
  private readonly sponsors = inject(PensionSponsorService);
  private readonly members = inject(PensionMemberService);
  private readonly ledger = inject(MemberLedgerEntryService);
  private readonly contributions = inject(PensionContributionService);
  private readonly contributionLines = inject(PensionContributionLineService);
  private readonly interestRates = inject(PensionInterestRateService);
  private readonly exitReasons = inject(ExitReasonService);
  private readonly taxTables = inject(LumpsumTaxTableService);
  private readonly taxBands = inject(LumpsumTaxBandService);
  private readonly exits = inject(MemberExitService);
  private readonly beneficiaries = inject(PensionBeneficiaryService);
  private readonly rates = inject(PensionContributionRateService);
  private readonly vesting = inject(PensionVestingScaleService);
  private readonly statusEntries = inject(MemberStatusEntryService);
  private readonly salaryEntries = inject(MemberSalaryEntryService);
  private readonly pensioners = inject(PensionerService);
  private readonly reasonDocuments = inject(ExitReasonDocumentService);
  private readonly exitDocuments = inject(MemberExitDocumentService);
  private readonly ageFactors = inject(PensionAgeFactorService);

  // ---------------------------------------------------------------- Setup

  readonly pensionScheme = codeTableEntity(this.schemes, {
    key: 'pensionScheme',
    titleKey: 'Erp::PensionScheme',
    pluralKey: 'Erp::PensionSchemes',
    icon: 'fas fa-landmark',
    permission: SETUP_PERMISSION,
    route: '/erp/pension-schemes',
    descriptionKey: 'Erp::Name',
    columns: [
      { field: 'planType', labelKey: 'Erp::PlanType', type: 'select', options: enumOptions(pensionPlanTypeOptions, 'PensionPlanType') },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(pensionSchemeStatusOptions, 'PensionSchemeStatus') },
      { field: 'normalRetirementAge', labelKey: 'Erp::NormalRetirementAge', type: 'number' },
    ],
    fields: [
      { field: 'schemeType', labelKey: 'Erp::SchemeType', type: 'select', options: enumOptions(pensionSchemeTypeOptions, 'PensionSchemeType') },
      { field: 'planType', labelKey: 'Erp::PlanType', type: 'select', options: enumOptions(pensionPlanTypeOptions, 'PensionPlanType') },
      { field: 'schemeMode', labelKey: 'Erp::SchemeMode', type: 'select', options: enumOptions(pensionSchemeModeOptions, 'PensionSchemeMode') },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(pensionSchemeStatusOptions, 'PensionSchemeStatus') },
      {
        field: 'interestCalculationMode',
        labelKey: 'Erp::InterestCalculationMode',
        type: 'select',
        options: enumOptions(interestCalculationModeOptions, 'InterestCalculationMode'),
      },
      { field: 'regulatorReferenceNo', labelKey: 'Erp::RegulatorReferenceNo', type: 'text', maxLength: 35 },
      { field: 'taxPinNo', labelKey: 'Erp::TaxPinNo', type: 'text', maxLength: 20 },
      { field: 'normalRetirementAge', labelKey: 'Erp::NormalRetirementAge', type: 'number', min: 18 },
      { field: 'minimumRetirementAge', labelKey: 'Erp::MinimumRetirementAge', type: 'number', min: 18 },
      { field: 'accrualRatePct', labelKey: 'Erp::AccrualRatePct', type: 'number', min: 0, section: 'definedBenefit', cardOnly: true, helpKey: 'Erp::AccrualRateHelp' },
      { field: 'maxPensionableServiceYears', labelKey: 'Erp::MaxPensionableServiceYears', type: 'number', min: 0, section: 'definedBenefit', cardOnly: true },
      { field: 'maxCommutationPct', labelKey: 'Erp::MaxCommutationPct', type: 'number', min: 0, section: 'definedBenefit', cardOnly: true },
      { field: 'commutationFactor', labelKey: 'Erp::CommutationFactor', type: 'number', min: 0, section: 'definedBenefit', cardOnly: true, helpKey: 'Erp::CommutationFactorHelp' },
      {
        field: 'earlyRetirementReductionPct',
        labelKey: 'Erp::EarlyRetirementReductionPct',
        type: 'number',
        min: 0,
        section: 'definedBenefit',
        cardOnly: true,
        helpKey: 'Erp::EarlyRetirementReductionHelp',
      },
      {
        field: 'pensionableSalaryBasis',
        labelKey: 'Erp::PensionableSalaryBasis',
        type: 'select',
        options: enumOptions(pensionableSalaryBasisOptions, 'PensionableSalaryBasis'),
        section: 'definedBenefit',
        cardOnly: true,
        helpKey: 'Erp::PensionableSalaryBasisHelp',
      },
      { field: 'salaryAveragingYears', labelKey: 'Erp::SalaryAveragingYears', type: 'number', min: 0, section: 'definedBenefit', cardOnly: true },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'definedBenefit', labelKey: 'Erp::DefinedBenefitFormula', collapsed: true },
    ],
    defaults: {
      schemeType: 0,
      planType: PensionPlanType.DefinedContribution,
      schemeMode: 0,
      status: 0,
      interestCalculationMode: 0,
      normalRetirementAge: 60,
      minimumRetirementAge: 50,
      accrualRatePct: 0,
      maxPensionableServiceYears: 0,
      maxCommutationPct: 0,
      commutationFactor: 0,
      earlyRetirementReductionPct: 0,
      pensionableSalaryBasis: PensionableSalaryBasis.CurrentSalary,
      salaryAveragingYears: 3,
    },
    quickCreate: false,
  });

  constructor() {
    // Run from the scheme, since the overdue certificates are counted scheme by scheme.
    this.pensionScheme.actions = [
      {
        key: 'suspendOverdue',
        labelKey: 'Erp::SuspendOverduePensioners',
        icon: 'fas fa-user-clock',
        permission: `${PERMISSION}.Update`,
        confirmKey: 'Erp::SuspendOverduePensionersConfirmation',
        run: dto => this.pensioners.suspendOverdue({ schemeCode: dto.code! }),
      },
    ];
    // Factor tables replace the formula's flat early retirement cut and commutation factor for the ages they cover.
    this.pensionScheme.parts = [
      {
        entity: 'pensionAgeFactor',
        titleKey: 'Erp::PensionAgeFactors',
        lines: dto => this.ageFactors.getList({ schemeCode: dto.code, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ schemeCode: dto.code }),
        columns: ['factorType', 'age', 'maleFactor', 'femaleFactor'],
      },
    ];
    // Copied onto each exit for the reason, to be ticked off as they come in.
    this.exitReason.parts = [
      {
        entity: 'exitReasonDocument',
        lines: dto => this.reasonDocuments.getList({ exitReasonCode: dto.code, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ exitReasonCode: dto.code }),
        columns: ['documentName', 'mandatory'],
      },
    ];
  }

  readonly lumpsumTaxTable = codeTableEntity(this.taxTables, {
    key: 'lumpsumTaxTable',
    titleKey: 'Erp::LumpsumTaxTable',
    pluralKey: 'Erp::LumpsumTaxTables',
    icon: 'fas fa-percent',
    permission: SETUP_PERMISSION,
    route: '/erp/lumpsum-tax-tables',
    columns: [
      { field: 'annualTaxFreeAmount', labelKey: 'Erp::AnnualTaxFreeAmount', type: 'currency' },
      { field: 'maxTaxFreeAmount', labelKey: 'Erp::MaxTaxFreeAmount', type: 'currency' },
    ],
    fields: [
      { field: 'annualTaxFreeAmount', labelKey: 'Erp::AnnualTaxFreeAmount', type: 'currency', min: 0 },
      { field: 'maxTaxFreeAmount', labelKey: 'Erp::MaxTaxFreeAmount', type: 'currency', min: 0 },
      { field: 'maxAgeTaxable', labelKey: 'Erp::MaxAgeTaxable', type: 'number', min: 0 },
    ],
    defaults: { annualTaxFreeAmount: 0, maxTaxFreeAmount: 0, maxAgeTaxable: 0 },
    quickCreate: false,
  });

  readonly lumpsumTaxBand: RecordEntity<LumpsumTaxBandDto, CreateUpdateLumpsumTaxBandDto> = {
    key: 'lumpsumTaxBand',
    titleKey: 'Erp::LumpsumTaxBand',
    pluralKey: 'Erp::LumpsumTaxBands',
    icon: 'fas fa-layer-group',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/lumpsum-tax-bands'],
    columns: [
      { field: 'taxTableCode', labelKey: 'Erp::TaxTableCode' },
      { field: 'lowerLimit', labelKey: 'Erp::LowerLimit', type: 'currency' },
      { field: 'upperLimit', labelKey: 'Erp::UpperLimit', type: 'currency' },
      { field: 'ratePct', labelKey: 'Erp::RatePct', type: 'number' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('taxTableCode', 'Erp::TaxTableCode', 'lumpsumTaxTable', undefined, { required: true }),
      { field: 'lowerLimit', labelKey: 'Erp::LowerLimit', type: 'currency', min: 0 },
      { field: 'upperLimit', labelKey: 'Erp::UpperLimit', type: 'currency', min: 0, helpKey: 'Erp::UpperLimitHelp' },
      { field: 'ratePct', labelKey: 'Erp::RatePct', type: 'number', min: 0 },
    ],
    getList: query => this.taxBands.getList(query),
    get: id => this.taxBands.get(id),
    create: input => this.taxBands.create(input),
    update: (id, input) => this.taxBands.update(id, input),
    delete: id => this.taxBands.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.taxTableCode ?? ''} ${dto.lowerLimit}`.trim() }),
    newRecord: () => ({ lowerLimit: 0, upperLimit: 0, ratePct: 0 }),
  };

  readonly exitReason = codeTableEntity(this.exitReasons, {
    key: 'exitReason',
    titleKey: 'Erp::ExitReason',
    pluralKey: 'Erp::ExitReasons',
    icon: 'fas fa-door-open',
    permission: SETUP_PERMISSION,
    route: '/erp/exit-reasons',
    columns: [
      { field: 'employerPortionPct', labelKey: 'Erp::EmployerPortionPct', type: 'number' },
      { field: 'taxTableCode', labelKey: 'Erp::TaxTableCode' },
      { field: 'statusAfterExit', labelKey: 'Erp::StatusAfterExit', type: 'select', options: enumOptions(memberStatusOptions, 'MemberStatus') },
    ],
    fields: [
      { field: 'paymentOption', labelKey: 'Erp::PaymentOption', type: 'select', options: enumOptions(exitPaymentOptionOptions, 'ExitPaymentOption') },
      { field: 'employerPortionPct', labelKey: 'Erp::EmployerPortionPct', type: 'number', min: 0, helpKey: 'Erp::EmployerPortionPctHelp' },
      codeField('taxTableCode', 'Erp::TaxTableCode', 'lumpsumTaxTable'),
      { field: 'lumpsumTaxFree', labelKey: 'Erp::LumpsumTaxFree', type: 'checkbox' },
      { field: 'statusAfterExit', labelKey: 'Erp::StatusAfterExit', type: 'select', options: enumOptions(memberStatusOptions, 'MemberStatus') },
      { field: 'applyVestingScale', labelKey: 'Erp::ApplyVestingScale', type: 'checkbox', helpKey: 'Erp::ApplyVestingScaleHelp' },
    ],
    defaults: { paymentOption: 1, employerPortionPct: 100, lumpsumTaxFree: false, statusAfterExit: MemberStatus.Inactive, applyVestingScale: false },
    quickCreate: false,
  });

  readonly pensionInterestRate: RecordEntity<PensionInterestRateDto, CreateUpdatePensionInterestRateDto> = {
    key: 'pensionInterestRate',
    titleKey: 'Erp::PensionInterestRate',
    pluralKey: 'Erp::PensionInterestRates',
    icon: 'fas fa-chart-line',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/pension-interest-rates'],
    columns: [
      { field: 'schemeCode', labelKey: 'Erp::SchemeCode' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
      { field: 'registeredRatePct', labelKey: 'Erp::RegisteredRatePct', type: 'number' },
      { field: 'unregisteredRatePct', labelKey: 'Erp::UnregisteredRatePct', type: 'number' },
      { field: 'posted', labelKey: 'Erp::Posted', type: 'boolean' },
      { field: 'totalInterest', labelKey: 'Erp::TotalInterest', type: 'currency' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'allocation', labelKey: 'Erp::InterestAllocation' },
    ],
    fields: [
      codeField('schemeCode', 'Erp::SchemeCode', 'pensionScheme', undefined, { required: true }),
      { field: 'dateDeclared', labelKey: 'Erp::DateDeclared', type: 'date' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date', required: true },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date', required: true },
      { field: 'registeredRatePct', labelKey: 'Erp::RegisteredRatePct', type: 'number', min: 0 },
      { field: 'unregisteredRatePct', labelKey: 'Erp::UnregisteredRatePct', type: 'number', min: 0 },
      { field: 'taxRatePct', labelKey: 'Erp::TaxRatePct', type: 'number', min: 0, helpKey: 'Erp::TaxRatePctHelp' },
      figure('posted', 'Erp::Posted', 'allocation'),
      figure('postedDocumentNo', 'Erp::PostedDocumentNo', 'allocation'),
      figure('totalInterest', 'Erp::TotalInterest', 'allocation'),
      figure('totalTax', 'Erp::TotalTax', 'allocation'),
    ],
    getList: query => this.interestRates.getList(query),
    get: id => this.interestRates.get(id),
    create: input => this.interestRates.create(input),
    update: (id, input) => this.interestRates.update(id, input),
    delete: id => this.interestRates.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.schemeCode ?? ''} ${dto.startDate?.substring(0, 10) ?? ''}`.trim() }),
    newRecord: () => ({
      startDate: `${new Date().getFullYear()}-01-01`,
      endDate: `${new Date().getFullYear()}-12-31`,
      registeredRatePct: 0,
      unregisteredRatePct: 0,
      taxRatePct: 0,
    }),
    facts: dto => [
      { labelKey: 'Erp::Posted', value: dto.posted, type: 'boolean' },
      { labelKey: 'Erp::TotalInterest', value: dto.totalInterest, type: 'currency' },
      { labelKey: 'Erp::TotalTax', value: dto.totalTax, type: 'currency' },
    ],
    actions: [
      {
        key: 'allocate',
        labelKey: 'Erp::AllocateInterest',
        icon: 'fas fa-coins',
        permission: `${PERMISSION}.Post`,
        visible: dto => !dto.posted,
        confirmKey: 'Erp::AllocateInterestConfirmation',
        run: dto => this.interestRates.allocate(dto.id!, {}),
      },
    ],
  };

  // ---------------------------------------------------------------- Sponsors and members

  readonly pensionSponsor: RecordEntity<PensionSponsorDto, CreateUpdatePensionSponsorDto> = {
    key: 'pensionSponsor',
    titleKey: 'Erp::PensionSponsor',
    pluralKey: 'Erp::PensionSponsors',
    icon: 'fas fa-building-user',
    permission: PERMISSION,
    listRoute: ['/erp/pension-sponsors'],
    attachmentEntityType: 'PensionSponsor',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'name', labelKey: 'Erp::Name', width: 240 },
      { field: 'schemeCode', labelKey: 'Erp::SchemeCode' },
      { field: 'employeeRatePct', labelKey: 'Erp::EmployeeRatePct', type: 'number' },
      { field: 'employerRatePct', labelKey: 'Erp::EmployerRatePct', type: 'number' },
      { field: 'lastScheduleDate', labelKey: 'Erp::LastScheduleDate', type: 'date' },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'contributions', labelKey: 'Erp::Contributions' },
      { key: 'contact', labelKey: 'Erp::AddressAndContact', collapsed: true },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries' },
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 100 },
      codeField('schemeCode', 'Erp::SchemeCode', 'pensionScheme', undefined, { required: true }),
      codeField('customerNo', 'Erp::CustomerNo', 'customer', undefined, { helpKey: 'Erp::SponsorCustomerHelp', cardOnly: true }),
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox', cardOnly: true },
      { field: 'employeeRatePct', labelKey: 'Erp::EmployeeRatePct', type: 'number', min: 0, section: 'contributions' },
      { field: 'employerRatePct', labelKey: 'Erp::EmployerRatePct', type: 'number', min: 0, section: 'contributions' },
      { field: 'taxPinNo', labelKey: 'Erp::TaxPinNo', type: 'text', maxLength: 20, section: 'contact', cardOnly: true },
      { field: 'contact', labelKey: 'Erp::Contact', type: 'text', maxLength: 100, section: 'contact', cardOnly: true },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', maxLength: 30, section: 'contact', cardOnly: true },
      { field: 'email', labelKey: 'Erp::Email', type: 'email', maxLength: 80, section: 'contact', cardOnly: true },
      { field: 'address', labelKey: 'Erp::Address', type: 'text', maxLength: 100, section: 'contact', cardOnly: true },
      { field: 'city', labelKey: 'Erp::City', type: 'text', maxLength: 50, section: 'contact', cardOnly: true },
    ],
    getList: query => this.sponsors.getList(query),
    get: id => this.sponsors.get(id),
    create: input => this.sponsors.create(input),
    update: (id, input) => this.sponsors.update(id, input),
    delete: id => this.sponsors.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.name ?? undefined }),
    newRecord: term => ({ name: term ?? '', employeeRatePct: 0, employerRatePct: 0, blocked: false }),
    related: dto =>
      forkJoin({
        members: this.members.getList({ sponsorNo: dto.no, maxResultCount: 1, skipCount: 0 }),
        schedules: this.contributions.getList({ sponsorNo: dto.no, maxResultCount: 1, skipCount: 0 }),
      }).pipe(
        map(({ members, schedules }) => [
          {
            labelKey: 'Erp::PensionMembers',
            icon: 'fas fa-users',
            count: members.totalCount ?? 0,
            routerLink: ['/erp/pension-members'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
          {
            labelKey: 'Erp::PensionContributions',
            icon: 'fas fa-file-invoice-dollar',
            count: schedules.totalCount ?? 0,
            routerLink: ['/erp/pension-contributions'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
        ]),
      ),
    // Dated rates win over the card's rates for the months they cover; the scale vests the employer's money by service.
    parts: [
      {
        entity: 'pensionContributionRate',
        lines: dto => this.rates.getList({ sponsorNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ sponsorNo: dto.no }),
        columns: ['startDate', 'endDate', 'employeeRatePct', 'employerRatePct'],
      },
      {
        entity: 'pensionVestingScale',
        lines: dto => this.vesting.getList({ sponsorNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ sponsorNo: dto.no }),
        columns: ['fromServiceYears', 'employerVestedPct'],
      },
    ],
  };

  readonly pensionMember: RecordEntity<PensionMemberDto, CreateUpdatePensionMemberDto> = {
    key: 'pensionMember',
    titleKey: 'Erp::PensionMember',
    pluralKey: 'Erp::PensionMembers',
    icon: 'fas fa-user-shield',
    permission: PERMISSION,
    listRoute: ['/erp/pension-members'],
    attachmentEntityType: 'PensionMember',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'fullName', labelKey: 'Erp::FullName', width: 220 },
      { field: 'schemeCode', labelKey: 'Erp::SchemeCode' },
      { field: 'sponsorNo', labelKey: 'Erp::SponsorNo' },
      { field: 'payrollNo', labelKey: 'Erp::PayrollNo' },
      { field: 'nationalId', labelKey: 'Erp::NationalId' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(memberStatusOptions, 'MemberStatus') },
      { field: 'currentSalary', labelKey: 'Erp::CurrentSalary', type: 'currency' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'employment', labelKey: 'Erp::Employment' },
      { key: 'personal', labelKey: 'Erp::Personal', collapsed: true },
      { key: 'contact', labelKey: 'Erp::AddressAndContact', collapsed: true },
      { key: 'payments', labelKey: 'Erp::Payments', collapsed: true },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries' },
      codeField('sponsorNo', 'Erp::SponsorNo', 'pensionSponsor', undefined, { required: true }),
      { field: 'firstName', labelKey: 'Erp::FirstName', type: 'text', required: true, maxLength: 50 },
      { field: 'otherName', labelKey: 'Erp::OtherName', type: 'text', maxLength: 50 },
      { field: 'lastName', labelKey: 'Erp::LastName', type: 'text', maxLength: 50 },
      { field: 'schemeCode', labelKey: 'Erp::SchemeCode', type: 'readonly', cardOnly: true },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(memberStatusOptions, 'MemberStatus'), cardOnly: true },
      {
        field: 'contributionStatus',
        labelKey: 'Erp::ContributionStatus',
        type: 'select',
        options: enumOptions(memberContributionStatusOptions, 'MemberContributionStatus'),
        cardOnly: true,
      },
      { field: 'payrollNo', labelKey: 'Erp::PayrollNo', type: 'text', maxLength: 20, section: 'employment' },
      { field: 'designation', labelKey: 'Erp::Designation', type: 'text', maxLength: 50, section: 'employment', cardOnly: true },
      { field: 'dateOfEmployment', labelKey: 'Erp::DateOfEmployment', type: 'date', section: 'employment', cardOnly: true },
      { field: 'joinSchemeDate', labelKey: 'Erp::JoinSchemeDate', type: 'date', section: 'employment' },
      { field: 'currentSalary', labelKey: 'Erp::CurrentSalary', type: 'currency', min: 0, section: 'employment' },
      figure('expectedRetirementDate', 'Erp::ExpectedRetirementDate', 'employment'),
      figure('exitDate', 'Erp::ExitDate', 'employment'),
      { field: 'nationalId', labelKey: 'Erp::NationalId', type: 'text', maxLength: 40, section: 'personal', cardOnly: true },
      { field: 'taxPinNo', labelKey: 'Erp::TaxPinNo', type: 'text', maxLength: 20, section: 'personal', cardOnly: true },
      { field: 'dateOfBirth', labelKey: 'Erp::DateOfBirth', type: 'date', section: 'personal', cardOnly: true },
      { field: 'gender', labelKey: 'Erp::Gender', type: 'select', options: enumOptions(memberGenderOptions, 'MemberGender'), section: 'personal', cardOnly: true },
      {
        field: 'maritalStatus',
        labelKey: 'Erp::MaritalStatus',
        type: 'select',
        options: enumOptions(memberMaritalStatusOptions, 'MemberMaritalStatus'),
        section: 'personal',
        cardOnly: true,
      },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', maxLength: 30, section: 'contact', cardOnly: true },
      { field: 'email', labelKey: 'Erp::Email', type: 'email', maxLength: 80, section: 'contact', cardOnly: true },
      { field: 'address', labelKey: 'Erp::Address', type: 'text', maxLength: 100, section: 'contact', cardOnly: true },
      { field: 'city', labelKey: 'Erp::City', type: 'text', maxLength: 50, section: 'contact', cardOnly: true },
      { field: 'bankName', labelKey: 'Erp::BankName', type: 'text', maxLength: 100, section: 'payments', cardOnly: true },
      { field: 'bankBranch', labelKey: 'Erp::BankBranch', type: 'text', maxLength: 100, section: 'payments', cardOnly: true },
      { field: 'bankAccountNo', labelKey: 'Erp::BankAccountNo', type: 'text', maxLength: 30, section: 'payments', cardOnly: true },
    ],
    getList: query => this.members.getList(query),
    get: id => this.members.get(id),
    create: input => this.members.create(input),
    update: (id, input) => this.members.update(id, input),
    delete: id => this.members.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.fullName ?? undefined }),
    newRecord: term => ({
      firstName: term ?? '',
      status: MemberStatus.Active,
      contributionStatus: 0,
      gender: 0,
      maritalStatus: 0,
      currentSalary: 0,
      joinSchemeDate: today(),
    }),
    // The fund is read from the ledger, so the card shows what a statement would.
    related: dto =>
      forkJoin({
        balance: this.members.getBalance(dto.id!),
        entries: this.ledger.getList({ memberNo: dto.no, maxResultCount: 1, skipCount: 0 }),
        exits: this.exits.getList({ memberNo: dto.no, maxResultCount: 1, skipCount: 0 }),
        statuses: this.statusEntries.getList({ memberNo: dto.no, maxResultCount: 1, skipCount: 0 }),
        salaries: this.salaryEntries.getList({ memberNo: dto.no, maxResultCount: 1, skipCount: 0 }),
      }).pipe(
        map(({ balance, entries, exits, statuses, salaries }) => [
          {
            labelKey: 'Erp::FundValue',
            icon: 'fas fa-piggy-bank',
            count: (balance.total ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
            routerLink: ['/erp/member-ledger-entries'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
          {
            labelKey: 'Erp::MemberLedgerEntries',
            icon: 'fas fa-receipt',
            count: entries.totalCount ?? 0,
            routerLink: ['/erp/member-ledger-entries'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
          {
            labelKey: 'Erp::MemberExits',
            icon: 'fas fa-door-open',
            count: exits.totalCount ?? 0,
            routerLink: ['/erp/member-exits'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
          {
            labelKey: 'Erp::MemberStatusEntries',
            icon: 'fas fa-timeline',
            count: statuses.totalCount ?? 0,
            routerLink: ['/erp/member-status-entries'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
          {
            labelKey: 'Erp::MemberSalaryEntries',
            icon: 'fas fa-money-check',
            count: salaries.totalCount ?? 0,
            routerLink: ['/erp/member-salary-entries'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
        ]),
      ),
    // A death benefit is shared out by these percentages, which must come to 100% before it is approved.
    parts: [
      {
        entity: 'pensionBeneficiary',
        lines: dto => this.beneficiaries.getList({ memberNo: dto.no, sorting: 'lineNo', maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ memberNo: dto.no }),
        columns: ['name', 'relationship', 'dateOfBirth', 'benefitPct', 'status'],
        totals: ['benefitPct'],
      },
    ],
  };

  readonly memberLedgerEntry: RecordEntity<MemberLedgerEntryDto, never> = {
    key: 'memberLedgerEntry',
    titleKey: 'Erp::MemberLedgerEntry',
    pluralKey: 'Erp::MemberLedgerEntries',
    icon: 'fas fa-receipt',
    permission: PERMISSION,
    listRoute: ['/erp/member-ledger-entries'],
    readOnly: true,
    columns: [
      { field: 'entryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 100 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date' },
      { field: 'memberNo', labelKey: 'Erp::MemberNo' },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo' },
      { field: 'transactionType', labelKey: 'Erp::TransactionType', type: 'select', options: enumOptions(pensionTransactionTypeOptions, 'PensionTransactionType') },
      { field: 'contributionType', labelKey: 'Erp::ContributionType', type: 'select', options: enumOptions(pensionContributionTypeOptions, 'PensionContributionType') },
      { field: 'exemptionType', labelKey: 'Erp::ExemptionType', type: 'select', options: enumOptions(pensionExemptionTypeOptions, 'PensionExemptionType') },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
      { field: 'schemeCode', labelKey: 'Erp::SchemeCode' },
      { field: 'contributionPeriod', labelKey: 'Erp::ContributionPeriod', type: 'date' },
      { field: 'description', labelKey: 'Erp::Description', width: 240 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      'entryNo',
      'postingDate',
      'schemeCode',
      'memberNo',
      'sponsorNo',
      'documentNo',
      'description',
      'contributionPeriod',
      'amount',
      'salary',
      'userName',
    ].map(field => figure(field, `Erp::${field[0].toUpperCase()}${field.substring(1)}`, 'general')),
    getList: query => this.ledger.getList(query),
    get: id => this.ledger.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.entryNo), name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  // ---------------------------------------------------------------- Contributions

  readonly pensionContribution: RecordEntity<PensionContributionHeaderDto, CreateUpdatePensionContributionHeaderDto> = {
    key: 'pensionContribution',
    titleKey: 'Erp::PensionContribution',
    pluralKey: 'Erp::PensionContributions',
    icon: 'fas fa-file-invoice-dollar',
    permission: PERMISSION,
    listRoute: ['/erp/pension-contributions'],
    parts: [
      {
        entity: 'pensionContributionLine',
        lines: dto => this.contributionLines.getList({ documentNo: dto.no, sorting: 'lineNo', maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ documentNo: dto.no }),
        columns: ['memberNo', 'memberName', 'basicSalary', 'employeeTaxExempt', 'employerTaxExempt', 'employeeAvcTaxExempt', 'employerAvcTaxExempt', 'employeeNonTaxExempt', 'employerNonTaxExempt', 'employeeAvcNonTaxExempt', 'employerAvcNonTaxExempt', 'totalAmount'],
        totals: ['totalAmount'],
        editable: dto => dto.status === PensionDocumentStatus.Open,
      },
    ],
    attachmentEntityType: 'PensionContributionHeader',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'sponsorNo', labelKey: 'Erp::SponsorNo' },
      { field: 'sponsorName', labelKey: 'Erp::SponsorName', width: 220 },
      { field: 'contributionPeriod', labelKey: 'Erp::ContributionPeriod', type: 'date' },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(pensionDocumentStatusOptions, 'PensionDocumentStatus') },
      { field: 'noOfMembers', labelKey: 'Erp::NoOfMembers', type: 'number' },
      { field: 'totalAmount', labelKey: 'Erp::TotalAmount', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('sponsorNo', 'Erp::SponsorNo', 'pensionSponsor', undefined, { required: true }),
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true },
      { field: 'contributionPeriod', labelKey: 'Erp::ContributionPeriod', type: 'date', required: true, helpKey: 'Erp::ContributionPeriodHelp' },
      {
        field: 'contributionMode',
        labelKey: 'Erp::ContributionMode',
        type: 'select',
        options: enumOptions(pensionContributionModeOptions, 'PensionContributionMode').filter(o => o.value !== 0),
      },
      codeField('transferSchemeCode', 'Erp::TransferSchemeCode', 'otherPensionScheme', undefined, { cardOnly: true, helpKey: 'Erp::TransferSchemeCodeHelp' }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      figure('schemeCode', 'Erp::SchemeCode', 'general'),
      figure('postedBy', 'Erp::PostedBy', 'general'),
    ],
    getList: query => this.contributions.getList(query),
    get: id => this.contributions.get(id),
    create: input => this.contributions.create(input),
    update: (id, input) => this.contributions.update(id, input),
    delete: id => this.contributions.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.sponsorName ?? undefined }),
    newRecord: () => ({ postingDate: today(), contributionPeriod: today(), contributionMode: PensionContributionMode.Normal }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: PensionDocumentStatus[dto.status] },
      { labelKey: 'Erp::NoOfMembers', value: dto.noOfMembers, type: 'number' },
      { labelKey: 'Erp::TotalAmount', value: dto.totalAmount, type: 'currency' },
    ],
    actions: [
      {
        key: 'suggest',
        labelKey: 'Erp::SuggestLines',
        icon: 'fas fa-wand-magic-sparkles',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === PensionDocumentStatus.Open,
        run: dto => this.contributions.suggestLines(dto.id!),
      },
      {
        key: 'split',
        labelKey: 'Erp::SplitByTaxRelief',
        icon: 'fas fa-scale-balanced',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === PensionDocumentStatus.Open,
        run: dto => this.contributions.splitLines(dto.id!),
      },
      {
        key: 'release',
        labelKey: 'Erp::Release',
        icon: 'fas fa-lock',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === PensionDocumentStatus.Open,
        run: dto => this.contributions.release(dto.id!),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-lock-open',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === PensionDocumentStatus.Released,
        run: dto => this.contributions.reopen(dto.id!),
      },
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === PensionDocumentStatus.Released,
        confirmKey: 'Erp::PostContributionConfirmation',
        run: dto => this.contributions.runPosting(dto.id!),
      },
    ],
    related: dto =>
      this.contributionLines.getList({ documentNo: dto.no, maxResultCount: 1, skipCount: 0 }).pipe(
        map(lines => [
          {
            labelKey: 'Erp::PensionContributionLines',
            icon: 'fas fa-list',
            count: lines.totalCount ?? 0,
            routerLink: ['/erp/pension-contribution-lines'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
          {
            labelKey: 'Erp::MemberLedgerEntries',
            icon: 'fas fa-receipt',
            routerLink: ['/erp/member-ledger-entries'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
        ]),
      ),
  };

  readonly pensionContributionLine: RecordEntity<PensionContributionLineDto, CreateUpdatePensionContributionLineDto> = {
    key: 'pensionContributionLine',
    titleKey: 'Erp::PensionContributionLine',
    pluralKey: 'Erp::PensionContributionLines',
    icon: 'fas fa-list',
    permission: PERMISSION,
    listRoute: ['/erp/pension-contribution-lines'],
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 120 },
      { field: 'memberNo', labelKey: 'Erp::MemberNo' },
      { field: 'memberName', labelKey: 'Erp::MemberName', width: 220 },
      { field: 'basicSalary', labelKey: 'Erp::BasicSalary', type: 'currency' },
      { field: 'employeeTaxExempt', labelKey: 'Erp::EmployeeTaxExempt', type: 'currency' },
      { field: 'employerTaxExempt', labelKey: 'Erp::EmployerTaxExempt', type: 'currency' },
      { field: 'employeeAvcTaxExempt', labelKey: 'Erp::EmployeeAvcTaxExempt', type: 'currency' },
      { field: 'totalAmount', labelKey: 'Erp::TotalAmount', type: 'currency' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'registered', labelKey: 'Erp::RegisteredContributions' },
      { key: 'unregistered', labelKey: 'Erp::UnregisteredContributions', collapsed: true },
    ],
    fields: [
      codeField('documentNo', 'Erp::DocumentNo', 'pensionContribution', undefined, { required: true, createOnly: true }),
      codeField('memberNo', 'Erp::MemberNo', 'pensionMember', undefined, { required: true }),
      { field: 'basicSalary', labelKey: 'Erp::BasicSalary', type: 'currency', min: 0 },
      amount('employeeTaxExempt', 'Erp::EmployeeTaxExempt', 'registered'),
      amount('employerTaxExempt', 'Erp::EmployerTaxExempt', 'registered'),
      amount('employeeAvcTaxExempt', 'Erp::EmployeeAvcTaxExempt', 'registered'),
      amount('employerAvcTaxExempt', 'Erp::EmployerAvcTaxExempt', 'registered'),
      amount('employeeNonTaxExempt', 'Erp::EmployeeNonTaxExempt', 'unregistered'),
      amount('employerNonTaxExempt', 'Erp::EmployerNonTaxExempt', 'unregistered'),
      amount('employeeAvcNonTaxExempt', 'Erp::EmployeeAvcNonTaxExempt', 'unregistered'),
      amount('employerAvcNonTaxExempt', 'Erp::EmployerAvcNonTaxExempt', 'unregistered'),
    ],
    getList: query => this.contributionLines.getList(query),
    get: id => this.contributionLines.get(id),
    create: input => this.contributionLines.create(input),
    update: (id, input) => this.contributionLines.update(id, input),
    delete: id => this.contributionLines.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.documentNo ?? ''} ${dto.memberNo ?? ''}`.trim(), name: dto.memberName ?? undefined }),
    newRecord: () => ({
      basicSalary: 0,
      employeeTaxExempt: 0,
      employeeNonTaxExempt: 0,
      employeeAvcTaxExempt: 0,
      employeeAvcNonTaxExempt: 0,
      employerTaxExempt: 0,
      employerNonTaxExempt: 0,
      employerAvcTaxExempt: 0,
      employerAvcNonTaxExempt: 0,
    }),
  };

  // ---------------------------------------------------------------- Exits

  readonly memberExit: RecordEntity<MemberExitDto, CreateUpdateMemberExitDto> = {
    key: 'memberExit',
    titleKey: 'Erp::MemberExit',
    pluralKey: 'Erp::MemberExits',
    icon: 'fas fa-door-open',
    permission: PERMISSION,
    listRoute: ['/erp/member-exits'],
    attachmentEntityType: 'MemberExit',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'memberNo', labelKey: 'Erp::MemberNo' },
      { field: 'memberName', labelKey: 'Erp::MemberName', width: 220 },
      { field: 'reasonCode', labelKey: 'Erp::ReasonCode' },
      { field: 'exitDate', labelKey: 'Erp::ExitDate', type: 'date' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(memberExitStatusOptions, 'MemberExitStatus') },
      { field: 'grossLumpsum', labelKey: 'Erp::GrossLumpsum', type: 'currency' },
      { field: 'netPayable', labelKey: 'Erp::NetPayable', type: 'currency' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'benefit', labelKey: 'Erp::Benefit' },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('memberNo', 'Erp::MemberNo', 'pensionMember', undefined, { required: true }),
      codeField('reasonCode', 'Erp::ReasonCode', 'exitReason', undefined, { required: true }),
      { field: 'withdrawalType', labelKey: 'Erp::WithdrawalType', type: 'select', options: enumOptions(memberWithdrawalTypeOptions, 'MemberWithdrawalType') },
      { field: 'exitDate', labelKey: 'Erp::ExitDate', type: 'date', required: true },
      { field: 'dateOfCalculation', labelKey: 'Erp::DateOfCalculation', type: 'date', helpKey: 'Erp::DateOfCalculationHelp' },
      { field: 'comment', labelKey: 'Erp::Comment', type: 'text', maxLength: 250, wide: true },
      figure('ageAtExit', 'Erp::AgeAtExit', 'benefit'),
      figure('serviceYears', 'Erp::ServiceYears', 'benefit'),
      figure('employeeBalance', 'Erp::EmployeeBalance', 'benefit'),
      figure('employerBalance', 'Erp::EmployerBalance', 'benefit'),
      figure('employeePayable', 'Erp::EmployeePayable', 'benefit'),
      figure('employerPayable', 'Erp::EmployerPayable', 'benefit'),
      figure('deferredAmount', 'Erp::DeferredAmount', 'benefit'),
      figure('taxFreeAmount', 'Erp::TaxFreeAmount', 'benefit'),
      figure('taxableAmount', 'Erp::TaxableAmount', 'benefit'),
      figure('taxOnLumpsum', 'Erp::TaxOnLumpsum', 'benefit'),
      figure('paymentVoucherNo', 'Erp::PaymentVoucherNo', 'benefit'),
    ],
    getList: query => this.exits.getList(query),
    get: id => this.exits.get(id),
    create: input => this.exits.create(input),
    update: (id, input) => this.exits.update(id, input),
    delete: id => this.exits.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.memberName ?? undefined }),
    newRecord: () => ({ exitDate: today(), withdrawalType: 0 }),
    // The documents the exit needs before it is approved, ticked off as they come in.
    parts: [
      {
        entity: 'memberExitDocument',
        lines: dto => this.exitDocuments.getList({ exitNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ exitNo: dto.no }),
        columns: ['documentName', 'mandatory', 'received', 'receivedDate', 'remarks'],
        editable: dto => dto.status === MemberExitStatus.Open,
      },
    ],
    facts: dto => [
      { labelKey: 'Erp::Status', value: MemberExitStatus[dto.status] },
      { labelKey: 'Erp::GrossLumpsum', value: dto.grossLumpsum, type: 'currency' },
      { labelKey: 'Erp::TaxOnLumpsum', value: dto.taxOnLumpsum, type: 'currency' },
      { labelKey: 'Erp::NetPayable', value: dto.netPayable, type: 'currency' },
      { labelKey: 'Erp::DeferredAmount', value: dto.deferredAmount, type: 'currency' },
    ],
    actions: [
      {
        key: 'calculate',
        labelKey: 'Erp::CalculateBenefit',
        icon: 'fas fa-calculator',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === MemberExitStatus.Open,
        run: dto => this.exits.calculate(dto.id!),
      },
      {
        key: 'copyDocuments',
        labelKey: 'Erp::CopyRequiredDocuments',
        icon: 'fas fa-file-import',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === MemberExitStatus.Open,
        run: dto => this.exits.copyRequiredDocuments(dto.id!),
      },
      {
        key: 'approve',
        labelKey: 'Erp::Approve',
        icon: 'fas fa-check',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === MemberExitStatus.Open,
        run: dto => this.exits.approve(dto.id!),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-lock-open',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === MemberExitStatus.Approved,
        run: dto => this.exits.reopen(dto.id!),
      },
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === MemberExitStatus.Approved,
        confirmKey: 'Erp::PostExitConfirmation',
        run: dto => this.exits.runPosting(dto.id!, {}),
      },
      {
        key: 'voucher',
        labelKey: 'Erp::RaisePaymentVoucher',
        icon: 'fas fa-money-check-dollar',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === MemberExitStatus.Posted && !dto.paymentVoucherNo,
        confirmKey: 'Erp::RaisePaymentVoucherConfirmation',
        run: dto => this.exits.raisePaymentVoucher(dto.id!),
      },
    ],
  };

  get all(): RecordEntity[] {
    return [
      this.pensionScheme,
      this.lumpsumTaxTable,
      this.lumpsumTaxBand,
      this.exitReason,
      this.pensionInterestRate,
      this.pensionSponsor,
      this.pensionMember,
      this.memberLedgerEntry,
      this.pensionContribution,
      this.pensionContributionLine,
      this.memberExit,
    ];
  }
}
