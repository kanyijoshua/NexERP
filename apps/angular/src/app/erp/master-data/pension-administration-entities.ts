import { Injectable, inject } from '@angular/core';
import {
  BeneficiaryStatus,
  CreateUpdatePensionBeneficiaryDto,
  CreateUpdatePensionContributionRateDto,
  CreateUpdatePensionIncrementDto,
  CreateUpdatePensionTaxReliefLimitDto,
  CreateUpdatePensionVestingScaleDto,
  MemberSalaryEntryDto,
  MemberSalaryEntryService,
  MemberStatusEntryDto,
  MemberStatusEntryService,
  PensionBeneficiaryDto,
  PensionBeneficiaryService,
  PensionContributionRateDto,
  PensionContributionRateService,
  PensionIncrementDto,
  PensionIncrementService,
  PensionIncrementStatus,
  PensionTaxReliefLimitDto,
  PensionTaxReliefLimitService,
  PensionVestingScaleDto,
  PensionVestingScaleService,
  PensionerChangeEntryDto,
  PensionerChangeEntryService,
  beneficiaryRelationshipOptions,
  beneficiaryStatusOptions,
  memberStatusOptions,
  pensionIncrementStatusOptions,
  pensionerChangeTypeOptions,
} from '@proxy/pensions';
import { EMPTY } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { codeField, enumOptions } from './entity-helpers';

const PERMISSION = 'Erp.Pensions';
const SETUP_PERMISSION = 'Erp.PensionSetup';
const today = () => new Date().toISOString().substring(0, 10);

/** A read-only figure of a card: what the system filled in. */
function figure(field: string, labelKey: string, section = 'general'): RecordField {
  return { field, labelKey, type: 'readonly', section, cardOnly: true };
}

/** A blank date is "none" to the server, not an empty string it cannot read. */
function withoutBlanks<T>(value: Record<string, unknown>): T {
  return Object.fromEntries(Object.entries(value).map(([key, v]) => [key, v === '' ? undefined : v])) as T;
}

/**
 * What keeps the pension records complete between contributions and exits: members'
 * beneficiaries, sponsors' dated rates and vesting scales, the tax relief limit, the members'
 * status and salary history, pension increments and the pensioners' history. Joined into
 * `MasterDataEntities.all`, so every lookup can find them.
 */
@Injectable({ providedIn: 'root' })
export class PensionAdministrationEntities {
  private readonly beneficiaries = inject(PensionBeneficiaryService);
  private readonly rates = inject(PensionContributionRateService);
  private readonly vesting = inject(PensionVestingScaleService);
  private readonly limits = inject(PensionTaxReliefLimitService);
  private readonly statusEntries = inject(MemberStatusEntryService);
  private readonly salaryEntries = inject(MemberSalaryEntryService);
  private readonly increments = inject(PensionIncrementService);
  private readonly changes = inject(PensionerChangeEntryService);

  readonly pensionBeneficiary: RecordEntity<PensionBeneficiaryDto, CreateUpdatePensionBeneficiaryDto> = {
    key: 'pensionBeneficiary',
    titleKey: 'Erp::PensionBeneficiary',
    pluralKey: 'Erp::PensionBeneficiaries',
    icon: 'fas fa-people-roof',
    permission: PERMISSION,
    listRoute: ['/erp/pension-beneficiaries'],
    attachmentEntityType: 'PensionBeneficiary',
    columns: [
      { field: 'memberNo', labelKey: 'Erp::MemberNo', width: 110 },
      { field: 'name', labelKey: 'Erp::Name', width: 220 },
      { field: 'relationship', labelKey: 'Erp::Relationship', type: 'select', options: enumOptions(beneficiaryRelationshipOptions, 'BeneficiaryRelationship') },
      { field: 'dateOfBirth', labelKey: 'Erp::DateOfBirth', type: 'date' },
      { field: 'benefitPct', labelKey: 'Erp::BenefitPct', type: 'number' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(beneficiaryStatusOptions, 'BeneficiaryStatus') },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'contact', labelKey: 'Erp::AddressAndContact', collapsed: true },
    ],
    fields: [
      codeField('memberNo', 'Erp::MemberNo', 'pensionMember', undefined, { required: true, createOnly: true }),
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 100 },
      {
        field: 'relationship',
        labelKey: 'Erp::Relationship',
        type: 'select',
        options: enumOptions(beneficiaryRelationshipOptions, 'BeneficiaryRelationship').filter(o => o.value !== 0),
      },
      { field: 'benefitPct', labelKey: 'Erp::BenefitPct', type: 'number', min: 0, helpKey: 'Erp::BenefitPctHelp' },
      { field: 'dateOfBirth', labelKey: 'Erp::DateOfBirth', type: 'date' },
      { field: 'nationalId', labelKey: 'Erp::NationalId', type: 'text', maxLength: 40, cardOnly: true },
      { field: 'guardianName', labelKey: 'Erp::GuardianName', type: 'text', maxLength: 100, cardOnly: true, helpKey: 'Erp::GuardianNameHelp' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(beneficiaryStatusOptions, 'BeneficiaryStatus'), cardOnly: true },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', maxLength: 30, section: 'contact', cardOnly: true },
      { field: 'email', labelKey: 'Erp::Email', type: 'email', maxLength: 80, section: 'contact', cardOnly: true },
      { field: 'bankName', labelKey: 'Erp::BankName', type: 'text', maxLength: 100, section: 'contact', cardOnly: true },
      { field: 'bankAccountNo', labelKey: 'Erp::BankAccountNo', type: 'text', maxLength: 30, section: 'contact', cardOnly: true },
    ],
    getList: query => this.beneficiaries.getList(query),
    get: id => this.beneficiaries.get(id),
    create: input => this.beneficiaries.create(input),
    update: (id, input) => this.beneficiaries.update(id, input),
    delete: id => this.beneficiaries.delete(id),
    toInput: value => withoutBlanks<CreateUpdatePensionBeneficiaryDto>(value),
    toItem: dto => ({ id: dto.id, code: `${dto.memberNo ?? ''} ${dto.lineNo}`.trim(), name: dto.name ?? undefined }),
    newRecord: () => ({ relationship: 1, benefitPct: 0, status: BeneficiaryStatus.Active }),
  };

  readonly pensionContributionRate: RecordEntity<PensionContributionRateDto, CreateUpdatePensionContributionRateDto> = {
    key: 'pensionContributionRate',
    titleKey: 'Erp::PensionContributionRate',
    pluralKey: 'Erp::PensionContributionRates',
    icon: 'fas fa-percent',
    permission: PERMISSION,
    listRoute: ['/erp/pension-contribution-rates'],
    columns: [
      { field: 'sponsorNo', labelKey: 'Erp::SponsorNo', width: 110 },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
      { field: 'employeeRatePct', labelKey: 'Erp::EmployeeRatePct', type: 'number' },
      { field: 'employerRatePct', labelKey: 'Erp::EmployerRatePct', type: 'number' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('sponsorNo', 'Erp::SponsorNo', 'pensionSponsor', undefined, { required: true }),
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date', required: true },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date', helpKey: 'Erp::ContributionRateEndHelp' },
      { field: 'employeeRatePct', labelKey: 'Erp::EmployeeRatePct', type: 'number', min: 0 },
      { field: 'employerRatePct', labelKey: 'Erp::EmployerRatePct', type: 'number', min: 0 },
    ],
    getList: query => this.rates.getList(query),
    get: id => this.rates.get(id),
    create: input => this.rates.create(input),
    update: (id, input) => this.rates.update(id, input),
    delete: id => this.rates.delete(id),
    toInput: value => withoutBlanks<CreateUpdatePensionContributionRateDto>(value),
    toItem: dto => ({ id: dto.id, code: `${dto.sponsorNo ?? ''} ${dto.startDate?.substring(0, 10) ?? ''}`.trim() }),
    newRecord: () => ({ startDate: today(), employeeRatePct: 0, employerRatePct: 0 }),
  };

  readonly pensionVestingScale: RecordEntity<PensionVestingScaleDto, CreateUpdatePensionVestingScaleDto> = {
    key: 'pensionVestingScale',
    titleKey: 'Erp::PensionVestingScale',
    pluralKey: 'Erp::PensionVestingScales',
    icon: 'fas fa-stairs',
    permission: PERMISSION,
    listRoute: ['/erp/pension-vesting-scales'],
    columns: [
      { field: 'sponsorNo', labelKey: 'Erp::SponsorNo', width: 110 },
      { field: 'fromServiceYears', labelKey: 'Erp::FromServiceYears', type: 'number' },
      { field: 'employerVestedPct', labelKey: 'Erp::EmployerVestedPct', type: 'number' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('sponsorNo', 'Erp::SponsorNo', 'pensionSponsor', undefined, { required: true }),
      { field: 'fromServiceYears', labelKey: 'Erp::FromServiceYears', type: 'number', min: 0, helpKey: 'Erp::FromServiceYearsHelp' },
      { field: 'employerVestedPct', labelKey: 'Erp::EmployerVestedPct', type: 'number', min: 0 },
    ],
    getList: query => this.vesting.getList(query),
    get: id => this.vesting.get(id),
    create: input => this.vesting.create(input),
    update: (id, input) => this.vesting.update(id, input),
    delete: id => this.vesting.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.sponsorNo ?? ''} ${dto.fromServiceYears}`.trim() }),
    newRecord: () => ({ fromServiceYears: 0, employerVestedPct: 0 }),
  };

  readonly pensionTaxReliefLimit: RecordEntity<PensionTaxReliefLimitDto, CreateUpdatePensionTaxReliefLimitDto> = {
    key: 'pensionTaxReliefLimit',
    titleKey: 'Erp::PensionTaxReliefLimit',
    pluralKey: 'Erp::PensionTaxReliefLimits',
    icon: 'fas fa-scale-balanced',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/pension-tax-relief-limits'],
    columns: [
      { field: 'effectiveDate', labelKey: 'Erp::EffectiveDate', type: 'date', width: 140 },
      { field: 'monthlyLimit', labelKey: 'Erp::MonthlyLimit', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'effectiveDate', labelKey: 'Erp::EffectiveDate', type: 'date', required: true },
      { field: 'monthlyLimit', labelKey: 'Erp::MonthlyLimit', type: 'currency', min: 0, helpKey: 'Erp::MonthlyLimitHelp' },
    ],
    getList: query => this.limits.getList(query),
    get: id => this.limits.get(id),
    create: input => this.limits.create(input),
    update: (id, input) => this.limits.update(id, input),
    delete: id => this.limits.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.effectiveDate?.substring(0, 10) ?? '' }),
    newRecord: () => ({ effectiveDate: today(), monthlyLimit: 0 }),
  };

  readonly memberStatusEntry: RecordEntity<MemberStatusEntryDto, never> = {
    key: 'memberStatusEntry',
    titleKey: 'Erp::MemberStatusEntry',
    pluralKey: 'Erp::MemberStatusEntries',
    icon: 'fas fa-timeline',
    permission: PERMISSION,
    listRoute: ['/erp/member-status-entries'],
    readOnly: true,
    columns: [
      { field: 'effectiveDate', labelKey: 'Erp::EffectiveDate', type: 'date', width: 130 },
      { field: 'memberNo', labelKey: 'Erp::MemberNo' },
      { field: 'schemeCode', labelKey: 'Erp::SchemeCode' },
      { field: 'fromStatus', labelKey: 'Erp::FromStatus', type: 'select', options: enumOptions(memberStatusOptions, 'MemberStatus') },
      { field: 'toStatus', labelKey: 'Erp::ToStatus', type: 'select', options: enumOptions(memberStatusOptions, 'MemberStatus') },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo' },
      { field: 'userName', labelKey: 'Erp::UserName' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      figure('effectiveDate', 'Erp::EffectiveDate'),
      figure('memberNo', 'Erp::MemberNo'),
      figure('sponsorNo', 'Erp::SponsorNo'),
      figure('documentNo', 'Erp::DocumentNo'),
      figure('userName', 'Erp::UserName'),
    ],
    getList: query => this.statusEntries.getList(query),
    get: id => this.statusEntries.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: `${dto.memberNo ?? ''} ${dto.effectiveDate?.substring(0, 10) ?? ''}`.trim() }),
    newRecord: () => ({}),
  };

  readonly memberSalaryEntry: RecordEntity<MemberSalaryEntryDto, never> = {
    key: 'memberSalaryEntry',
    titleKey: 'Erp::MemberSalaryEntry',
    pluralKey: 'Erp::MemberSalaryEntries',
    icon: 'fas fa-money-check',
    permission: PERMISSION,
    listRoute: ['/erp/member-salary-entries'],
    readOnly: true,
    columns: [
      { field: 'memberNo', labelKey: 'Erp::MemberNo', width: 110 },
      { field: 'period', labelKey: 'Erp::Period', type: 'date' },
      { field: 'salary', labelKey: 'Erp::Salary', type: 'currency' },
      { field: 'sponsorNo', labelKey: 'Erp::SponsorNo' },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      figure('memberNo', 'Erp::MemberNo'),
      figure('period', 'Erp::Period'),
      figure('salary', 'Erp::Salary'),
      figure('documentNo', 'Erp::DocumentNo'),
    ],
    getList: query => this.salaryEntries.getList(query),
    get: id => this.salaryEntries.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: `${dto.memberNo ?? ''} ${dto.period?.substring(0, 7) ?? ''}`.trim() }),
    newRecord: () => ({}),
  };

  readonly pensionIncrement: RecordEntity<PensionIncrementDto, CreateUpdatePensionIncrementDto> = {
    key: 'pensionIncrement',
    titleKey: 'Erp::PensionIncrement',
    pluralKey: 'Erp::PensionIncrements',
    icon: 'fas fa-arrow-trend-up',
    permission: PERMISSION,
    listRoute: ['/erp/pension-increments'],
    attachmentEntityType: 'PensionIncrement',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'schemeCode', labelKey: 'Erp::SchemeCode' },
      { field: 'effectiveDate', labelKey: 'Erp::EffectiveDate', type: 'date' },
      { field: 'incrementPct', labelKey: 'Erp::IncrementPct', type: 'number' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(pensionIncrementStatusOptions, 'PensionIncrementStatus') },
      { field: 'noOfPensioners', labelKey: 'Erp::NoOfPensioners', type: 'number' },
      { field: 'totalMonthlyIncrease', labelKey: 'Erp::TotalMonthlyIncrease', type: 'currency' },
      { field: 'totalArrears', labelKey: 'Erp::TotalArrears', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('schemeCode', 'Erp::SchemeCode', 'pensionScheme', undefined, { required: true }),
      { field: 'effectiveDate', labelKey: 'Erp::EffectiveDate', type: 'date', required: true, helpKey: 'Erp::IncrementEffectiveHelp' },
      { field: 'incrementPct', labelKey: 'Erp::IncrementPct', type: 'number' },
      { field: 'minimumMonthlyPension', labelKey: 'Erp::MinimumMonthlyPension', type: 'currency', min: 0, helpKey: 'Erp::MinimumMonthlyPensionHelp' },
      codeField('reasonCode', 'Erp::RevisionReason', 'pensionRevisionReason'),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      figure('appliedBy', 'Erp::AppliedBy'),
      figure('appliedDate', 'Erp::AppliedDate'),
    ],
    getList: query => this.increments.getList(query),
    get: id => this.increments.get(id),
    create: input => this.increments.create(input),
    update: (id, input) => this.increments.update(id, input),
    delete: id => this.increments.delete(id),
    toInput: value => withoutBlanks<CreateUpdatePensionIncrementDto>(value),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ effectiveDate: today(), incrementPct: 0, minimumMonthlyPension: 0 }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: PensionIncrementStatus[dto.status] },
      { labelKey: 'Erp::NoOfPensioners', value: dto.noOfPensioners, type: 'number' },
      { labelKey: 'Erp::TotalMonthlyIncrease', value: dto.totalMonthlyIncrease, type: 'currency' },
      { labelKey: 'Erp::TotalArrears', value: dto.totalArrears, type: 'currency' },
    ],
    actions: [
      {
        key: 'apply',
        labelKey: 'Erp::ApplyIncrement',
        icon: 'fas fa-check-double',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === PensionIncrementStatus.Open,
        confirmKey: 'Erp::ApplyIncrementConfirmation',
        run: dto => this.increments.apply(dto.id!),
      },
    ],
  };

  readonly pensionerChangeEntry: RecordEntity<PensionerChangeEntryDto, never> = {
    key: 'pensionerChangeEntry',
    titleKey: 'Erp::PensionerChangeEntry',
    pluralKey: 'Erp::PensionerChangeEntries',
    icon: 'fas fa-clock-rotate-left',
    permission: PERMISSION,
    listRoute: ['/erp/pensioner-change-entries'],
    readOnly: true,
    columns: [
      { field: 'effectiveDate', labelKey: 'Erp::EffectiveDate', type: 'date', width: 130 },
      { field: 'pensionerNo', labelKey: 'Erp::PensionerNo' },
      { field: 'changeType', labelKey: 'Erp::ChangeType', type: 'select', options: enumOptions(pensionerChangeTypeOptions, 'PensionerChangeType') },
      { field: 'oldMonthlyPension', labelKey: 'Erp::OldMonthlyPension', type: 'currency' },
      { field: 'newMonthlyPension', labelKey: 'Erp::NewMonthlyPension', type: 'currency' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
      { field: 'description', labelKey: 'Erp::Description', width: 260 },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      figure('effectiveDate', 'Erp::EffectiveDate'),
      figure('pensionerNo', 'Erp::PensionerNo'),
      figure('description', 'Erp::Description'),
      figure('documentNo', 'Erp::DocumentNo'),
      figure('userName', 'Erp::UserName'),
    ],
    getList: query => this.changes.getList(query),
    get: id => this.changes.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: `${dto.pensionerNo ?? ''} ${dto.effectiveDate?.substring(0, 10) ?? ''}`.trim(), name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  get all(): RecordEntity[] {
    return [
      this.pensionBeneficiary,
      this.pensionContributionRate,
      this.pensionVestingScale,
      this.pensionTaxReliefLimit,
      this.memberStatusEntry,
      this.memberSalaryEntry,
      this.pensionIncrement,
      this.pensionerChangeEntry,
    ];
  }
}
