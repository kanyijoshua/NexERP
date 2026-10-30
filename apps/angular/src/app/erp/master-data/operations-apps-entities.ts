import { ToasterService } from '@abp/ng.theme.shared';
import { Injectable, inject } from '@angular/core';
import {
  CampaignMailingEntryDto,
  CampaignMailingService,
  CreateUpdateSegmentDto,
  PickSegmentContactsInput,
  SegmentDto,
  SegmentLineDto,
  SegmentLineService,
  SegmentService,
  SendMailingInput,
  mailingStatusOptions,
} from '@proxy/crm';
import { contactTypeOptions } from '@proxy/crm';
import {
  ContractCostFrequency,
  CreateUpdateVehicleContractDto,
  CreateUpdateVehicleDto,
  CreateVehicleLogEntryDto,
  VehicleContractDto,
  VehicleContractService,
  VehicleContractType,
  VehicleCostDto,
  VehicleDto,
  VehicleLogEntryDto,
  VehicleLogEntryService,
  VehicleLogType,
  VehicleService,
  VehicleStatus,
  contractCostFrequencyOptions,
  vehicleContractTypeOptions,
  vehicleLogTypeOptions,
  vehicleStatusOptions,
} from '@proxy/fleet';
import {
  AppraisalDto,
  AppraisalLineDto,
  AppraisalLineService,
  AppraisalPeriodDto,
  AppraisalPeriodService,
  AppraisalService,
  AppraisalStatus,
  ApplicantStage,
  CreateUpdateAppraisalDto,
  CreateUpdateAppraisalLineDto,
  CreateUpdateAppraisalPeriodDto,
  CreateUpdateJobApplicantDto,
  CreateUpdateJobVacancyDto,
  HireApplicantInput,
  JobApplicantDto,
  JobApplicantService,
  JobVacancyDto,
  JobVacancyService,
  JobVacancyStatus,
  applicantStageOptions,
  appraisalStatusOptions,
  jobVacancyStatusOptions,
} from '@proxy/human-resources';
import {
  CreateUpdateMaintenanceEquipmentDto,
  CreateUpdateMaintenanceRequestDto,
  CreateUpdateMaintenanceTeamDto,
  MaintenanceEquipmentDto,
  MaintenanceEquipmentService,
  MaintenancePriority,
  MaintenanceRequestDto,
  MaintenanceRequestService,
  MaintenanceStage,
  MaintenanceTeamDto,
  MaintenanceTeamService,
  MaintenanceType,
  equipmentStatusOptions,
  maintenancePriorityOptions,
  maintenanceStageOptions,
  maintenanceTypeOptions,
} from '@proxy/maintenance-management';
import {
  CreateUpdatePosTerminalDto,
  OpenPosSessionInput,
  PayPosOrderInput,
  PosOrderDto,
  PosOrderLineDto,
  PosOrderLineService,
  PosOrderService,
  PosOrderStatus,
  PosPaymentMethod,
  PosSessionDto,
  PosSessionService,
  PosSessionStatus,
  PosTerminalDto,
  PosTerminalService,
  RingUpInput,
  StartPosOrderInput,
  posOrderStatusOptions,
  posPaymentMethodOptions,
  posSessionStatusOptions,
} from '@proxy/point-of-sale';
import {
  CreateQualityCheckDto,
  CreateUpdateQualityControlPointDto,
  QualityCheckDto,
  QualityCheckResult,
  QualityCheckService,
  QualityControlPointDto,
  QualityControlPointService,
  QualityOperation,
  QualityTestType,
  RecordQualityCheckInput,
  qualityCheckResultOptions,
  qualityOperationOptions,
  qualityTestTypeOptions,
} from '@proxy/quality';
import {
  CreateUpdateCustomerSubscriptionDto,
  CreateUpdateSubscriptionPlanDto,
  CustomerSubscriptionDto,
  CustomerSubscriptionService,
  InvoiceSubscriptionsInput,
  SubscriptionLineType,
  SubscriptionPlanDto,
  SubscriptionPlanService,
  SubscriptionStatus,
  subscriptionLineTypeOptions,
  subscriptionStatusOptions,
} from '@proxy/subscriptions';
import { EMPTY, Observable, map, tap } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { RecordDialogService } from '../erp-shared/record/record-dialog.service';
import { codeField, codeTableEntity, enumOptions, runWithInput } from './entity-helpers';

const RECRUITMENT = 'Erp.Recruitment';
const APPRAISALS = 'Erp.Appraisals';
const SUBSCRIPTIONS = 'Erp.Subscriptions';
const POS = 'Erp.PointOfSale';
const MAINTENANCE = 'Erp.Maintenance';
const QUALITY = 'Erp.Quality';
const FLEET = 'Erp.Fleet';
const MARKETING = 'Erp.Marketing';
const today = () => new Date().toISOString().substring(0, 10);
const yearStart = () => `${new Date().getFullYear()}-01-01`;
const ALL_LINES = { maxResultCount: 1000, skipCount: 0 };

const vacancyStatuses = enumOptions(jobVacancyStatusOptions, 'JobVacancyStatus');
const applicantStages = enumOptions(applicantStageOptions, 'ApplicantStage');
const appraisalStatuses = enumOptions(appraisalStatusOptions, 'AppraisalStatus');
const subscriptionStatuses = enumOptions(subscriptionStatusOptions, 'SubscriptionStatus');
const subscriptionLineTypes = enumOptions(subscriptionLineTypeOptions, 'SubscriptionLineType');
const sessionStatuses = enumOptions(posSessionStatusOptions, 'PosSessionStatus');
const orderStatuses = enumOptions(posOrderStatusOptions, 'PosOrderStatus');
const paymentMethods = enumOptions(posPaymentMethodOptions, 'PosPaymentMethod');
const equipmentStatuses = enumOptions(equipmentStatusOptions, 'EquipmentStatus');
const maintenanceTypes = enumOptions(maintenanceTypeOptions, 'MaintenanceType');
const maintenancePriorities = enumOptions(maintenancePriorityOptions, 'MaintenancePriority');
const maintenanceStages = enumOptions(maintenanceStageOptions, 'MaintenanceStage');
const qualityOperations = enumOptions(qualityOperationOptions, 'QualityOperation');
const qualityTestTypes = enumOptions(qualityTestTypeOptions, 'QualityTestType');
const qualityResults = enumOptions(qualityCheckResultOptions, 'QualityCheckResult');
const vehicleStatuses = enumOptions(vehicleStatusOptions, 'VehicleStatus');
const vehicleLogTypes = enumOptions(vehicleLogTypeOptions, 'VehicleLogType');
const contractTypes = enumOptions(vehicleContractTypeOptions, 'VehicleContractType');
const costFrequencies = enumOptions(contractCostFrequencyOptions, 'ContractCostFrequency');
const mailingStatuses = enumOptions(mailingStatusOptions, 'MailingStatus');
const contactTypes = enumOptions(contactTypeOptions, 'ContactType');

function readonly(field: string, labelKey: string, section?: string): RecordField {
  return { field, labelKey, type: 'readonly', section };
}

/**
 * Recruitment and appraisals, customer subscriptions, the point of sale, maintenance, quality
 * checks, the fleet, and marketing segments with their mailings.
 */
@Injectable({ providedIn: 'root' })
export class OperationsAppsEntities {
  private readonly dialogs = inject(RecordDialogService);
  private readonly toaster = inject(ToasterService);
  private readonly vacancies = inject(JobVacancyService);
  private readonly applicants = inject(JobApplicantService);
  private readonly appraisalPeriods = inject(AppraisalPeriodService);
  private readonly appraisals = inject(AppraisalService);
  private readonly appraisalLines = inject(AppraisalLineService);
  private readonly plans = inject(SubscriptionPlanService);
  private readonly subscriptions = inject(CustomerSubscriptionService);
  private readonly terminals = inject(PosTerminalService);
  private readonly sessions = inject(PosSessionService);
  private readonly orders = inject(PosOrderService);
  private readonly orderLines = inject(PosOrderLineService);
  private readonly teams = inject(MaintenanceTeamService);
  private readonly equipment = inject(MaintenanceEquipmentService);
  private readonly requests = inject(MaintenanceRequestService);
  private readonly controlPoints = inject(QualityControlPointService);
  private readonly checks = inject(QualityCheckService);
  private readonly vehicles = inject(VehicleService);
  private readonly vehicleLogs = inject(VehicleLogEntryService);
  private readonly vehicleContracts = inject(VehicleContractService);
  private readonly segments = inject(SegmentService);
  private readonly segmentLines = inject(SegmentLineService);
  private readonly mailings = inject(CampaignMailingService);

  /** Says how many records a batch made, e.g. "3 invoices were created." */
  private report<T>(messageKey: string, count: (result: T) => number) {
    return tap<T>(result => this.toaster.success(messageKey, undefined, { messageLocalizationParams: [String(count(result))] }));
  }

  // Recruitment

  readonly jobVacancy: RecordEntity<JobVacancyDto, CreateUpdateJobVacancyDto> = {
    key: 'jobVacancy',
    titleKey: 'Erp::JobVacancy',
    pluralKey: 'Erp::JobVacancies',
    icon: 'fas fa-briefcase',
    permission: RECRUITMENT,
    listRoute: ['/erp/job-vacancies'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'title', labelKey: 'Erp::JobTitle' },
      { field: 'department', labelKey: 'Erp::Department', width: 150 },
      { field: 'positions', labelKey: 'Erp::Positions', type: 'number', width: 100 },
      { field: 'hiredCount', labelKey: 'Erp::Hired', type: 'number', width: 90 },
      { field: 'closingDate', labelKey: 'Erp::ClosingDate', type: 'date', width: 120 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: vacancyStatuses, width: 110 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, helpKey: 'Erp::NoFromSeriesHelp' },
      { field: 'title', labelKey: 'Erp::JobTitle', type: 'text', required: true, maxLength: 100 },
      { field: 'department', labelKey: 'Erp::Department', type: 'text', maxLength: 50 },
      { field: 'positions', labelKey: 'Erp::Positions', type: 'number', min: 1 },
      { field: 'openingDate', labelKey: 'Erp::OpeningDate', type: 'date' },
      { field: 'closingDate', labelKey: 'Erp::ClosingDate', type: 'date' },
      { field: 'description', labelKey: 'Erp::Description', type: 'textarea', wide: true },
    ],
    getList: query => this.vacancies.getList(query),
    get: id => this.vacancies.get(id),
    create: input => this.vacancies.create(input),
    update: (id, input) => this.vacancies.update(id, input),
    delete: id => this.vacancies.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.title ?? undefined }),
    newRecord: () => ({ positions: 1, openingDate: today() }),
    actions: [
      {
        key: 'open',
        labelKey: 'Erp::OpenVacancy',
        icon: 'fas fa-door-open',
        permission: `${RECRUITMENT}.Update`,
        visible: dto => dto.status !== JobVacancyStatus.Open,
        run: dto => this.vacancies.open(dto.id!),
      },
      {
        key: 'close',
        labelKey: 'Erp::CloseVacancy',
        icon: 'fas fa-door-closed',
        permission: `${RECRUITMENT}.Update`,
        visible: dto => dto.status === JobVacancyStatus.Open,
        run: dto => this.vacancies.close(dto.id!),
      },
    ],
    parts: [
      {
        entity: 'jobApplicant',
        lines: dto => this.applicants.getList({ vacancyNo: dto.no, ...ALL_LINES }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ vacancyNo: dto.no }),
        columns: ['no', 'fullName', 'email', 'stage', 'rating'],
      },
    ],
  };

  readonly jobApplicant: RecordEntity<JobApplicantDto, CreateUpdateJobApplicantDto> = {
    key: 'jobApplicant',
    titleKey: 'Erp::JobApplicant',
    pluralKey: 'Erp::JobApplicants',
    icon: 'fas fa-user-tie',
    permission: RECRUITMENT,
    listRoute: ['/erp/job-applicants'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'fullName', labelKey: 'Erp::Name' },
      { field: 'vacancyNo', labelKey: 'Erp::JobVacancy', width: 120 },
      { field: 'email', labelKey: 'Erp::Email', width: 200 },
      { field: 'appliedDate', labelKey: 'Erp::AppliedDate', type: 'date', width: 120 },
      { field: 'rating', labelKey: 'Erp::Rating', type: 'number', width: 80 },
      { field: 'stage', labelKey: 'Erp::Stage', type: 'select', options: applicantStages, width: 120 },
      { field: 'employeeNo', labelKey: 'Erp::EmployeeNo', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, helpKey: 'Erp::NoFromSeriesHelp' },
      codeField('vacancyNo', 'Erp::JobVacancy', 'jobVacancy', 'general', { required: true }),
      { field: 'firstName', labelKey: 'Erp::FirstName', type: 'text', required: true, maxLength: 50 },
      { field: 'lastName', labelKey: 'Erp::LastName', type: 'text', maxLength: 50 },
      { field: 'email', labelKey: 'Erp::Email', type: 'email', maxLength: 80 },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', maxLength: 30 },
      { field: 'appliedDate', labelKey: 'Erp::AppliedDate', type: 'date' },
      { field: 'rating', labelKey: 'Erp::Rating', type: 'number', min: 0, helpKey: 'Erp::RatingHelp' },
      { field: 'expectedSalary', labelKey: 'Erp::ExpectedSalary', type: 'currency', min: 0 },
      { field: 'notes', labelKey: 'Erp::Notes', type: 'textarea', wide: true },
    ],
    getList: query => this.applicants.getList(query),
    get: id => this.applicants.get(id),
    create: input => this.applicants.create(input),
    update: (id, input) => this.applicants.update(id, input),
    delete: id => this.applicants.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.fullName ?? undefined }),
    newRecord: () => ({ appliedDate: today(), rating: 0, expectedSalary: 0 }),
    actions: [
      {
        key: 'stage',
        labelKey: 'Erp::ChangeStage',
        icon: 'fas fa-arrow-right',
        permission: `${RECRUITMENT}.Update`,
        visible: dto => dto.stage !== ApplicantStage.Hired && dto.stage !== ApplicantStage.Rejected,
        run: dto =>
          runWithInput<{ stage: ApplicantStage }>(this.dialogs, {
            titleKey: 'Erp::ChangeStage',
            icon: 'fas fa-arrow-right',
            permission: `${RECRUITMENT}.Update`,
            fields: [{ field: 'stage', labelKey: 'Erp::Stage', type: 'select', required: true, options: applicantStages.filter(o => o.value !== ApplicantStage.Hired) }],
            values: { stage: dto.stage },
            submit: input => this.applicants.changeStage(dto.id!, input),
          }),
      },
      {
        key: 'hire',
        labelKey: 'Erp::Hire',
        icon: 'fas fa-user-check',
        permission: `${RECRUITMENT}.Hire`,
        visible: dto => dto.stage !== ApplicantStage.Hired && dto.stage !== ApplicantStage.Rejected,
        run: dto =>
          runWithInput<HireApplicantInput>(this.dialogs, {
            titleKey: 'Erp::Hire',
            icon: 'fas fa-user-check',
            permission: `${RECRUITMENT}.Hire`,
            fields: [
              { field: 'employmentDate', labelKey: 'Erp::EmploymentDate', type: 'date', required: true },
              { field: 'employeeNo', labelKey: 'Erp::EmployeeNo', type: 'text', maxLength: 20, helpKey: 'Erp::NoFromSeriesHelp' },
            ],
            values: { employmentDate: today() },
            submit: input => this.applicants.hire(dto.id!, input),
          }),
      },
    ],
  };

  // Appraisals

  readonly appraisalPeriod = codeTableEntity<AppraisalPeriodDto, CreateUpdateAppraisalPeriodDto>(this.appraisalPeriods, {
    key: 'appraisalPeriod',
    titleKey: 'Erp::AppraisalPeriod',
    pluralKey: 'Erp::AppraisalPeriods',
    icon: 'fas fa-calendar-check',
    permission: APPRAISALS,
    route: '/erp/appraisal-periods',
    codeLength: 10,
    columns: [
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', width: 120 },
      { field: 'endingDate', labelKey: 'Erp::EndingDate', type: 'date', width: 120 },
      { field: 'closed', labelKey: 'Erp::Closed', type: 'boolean', width: 90 },
    ],
    fields: [
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', required: true },
      { field: 'endingDate', labelKey: 'Erp::EndingDate', type: 'date', required: true },
      { field: 'closed', labelKey: 'Erp::Closed', type: 'checkbox' },
    ],
    defaults: { startingDate: yearStart(), endingDate: `${new Date().getFullYear()}-12-31`, closed: false },
  });

  readonly appraisal: RecordEntity<AppraisalDto, CreateUpdateAppraisalDto> = {
    key: 'appraisal',
    titleKey: 'Erp::Appraisal',
    pluralKey: 'Erp::Appraisals',
    icon: 'fas fa-star-half-stroke',
    permission: APPRAISALS,
    listRoute: ['/erp/appraisals'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'employeeNo', labelKey: 'Erp::EmployeeNo', width: 120 },
      { field: 'employeeName', labelKey: 'Erp::EmployeeName' },
      { field: 'periodCode', labelKey: 'Erp::AppraisalPeriod', width: 120 },
      { field: 'reviewerEmployeeNo', labelKey: 'Erp::Reviewer', width: 120 },
      { field: 'overallScore', labelKey: 'Erp::OverallScore', type: 'number', width: 110 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: appraisalStatuses, width: 140 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, helpKey: 'Erp::NoFromSeriesHelp' },
      codeField('employeeNo', 'Erp::EmployeeNo', 'employee', 'general', { required: true, createOnly: true }),
      codeField('periodCode', 'Erp::AppraisalPeriod', 'appraisalPeriod', 'general', { required: true, createOnly: true }),
      codeField('reviewerEmployeeNo', 'Erp::Reviewer', 'employee'),
      readonly('overallScore', 'Erp::OverallScore'),
      { field: 'employeeComment', labelKey: 'Erp::EmployeeComment', type: 'textarea', wide: true },
      { field: 'reviewerComment', labelKey: 'Erp::ReviewerComment', type: 'textarea', wide: true },
    ],
    getList: query => this.appraisals.getList(query),
    get: id => this.appraisals.get(id),
    create: input => this.appraisals.create(input),
    update: (id, input) => this.appraisals.update(id, input),
    delete: id => this.appraisals.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.employeeName ?? undefined }),
    newRecord: () => ({}),
    actions: [
      {
        key: 'selfReview',
        labelKey: 'Erp::StartSelfReview',
        icon: 'fas fa-user-pen',
        permission: `${APPRAISALS}.Update`,
        visible: dto => dto.status === AppraisalStatus.Draft,
        run: dto => this.appraisals.startSelfReview(dto.id!),
      },
      {
        key: 'submit',
        labelKey: 'Erp::SubmitToReviewer',
        icon: 'fas fa-paper-plane',
        permission: `${APPRAISALS}.Update`,
        visible: dto => dto.status === AppraisalStatus.Draft || dto.status === AppraisalStatus.SelfReview,
        run: dto => this.appraisals.submitToReviewer(dto.id!),
      },
      {
        key: 'complete',
        labelKey: 'Erp::CompleteAppraisal',
        icon: 'fas fa-flag-checkered',
        permission: `${APPRAISALS}.Review`,
        visible: dto => dto.status === AppraisalStatus.ManagerReview,
        run: dto => this.appraisals.complete(dto.id!),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-lock-open',
        permission: `${APPRAISALS}.Review`,
        visible: dto => dto.status === AppraisalStatus.Completed,
        run: dto => this.appraisals.reopen(dto.id!),
      },
    ],
    parts: [
      {
        entity: 'appraisalLine',
        lines: dto => this.appraisalLines.getList({ appraisalId: dto.id, ...ALL_LINES }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ appraisalId: dto.id, weight: 1, selfScore: 0, managerScore: 0 }),
        columns: ['objective', 'weight', 'target', 'selfScore', 'managerScore'],
        editable: dto => dto.status !== AppraisalStatus.Completed,
      },
    ],
  };

  readonly appraisalLine: RecordEntity<AppraisalLineDto, CreateUpdateAppraisalLineDto> = {
    key: 'appraisalLine',
    titleKey: 'Erp::AppraisalObjective',
    pluralKey: 'Erp::AppraisalObjectives',
    icon: 'fas fa-bullseye',
    permission: APPRAISALS,
    editPermission: `${APPRAISALS}.Update`,
    listRoute: ['/erp/appraisals'],
    columns: [
      { field: 'objective', labelKey: 'Erp::Objective' },
      { field: 'weight', labelKey: 'Erp::Weight', type: 'number', width: 90 },
      { field: 'target', labelKey: 'Erp::Target', width: 160 },
      { field: 'selfScore', labelKey: 'Erp::SelfScore', type: 'number', width: 100 },
      { field: 'managerScore', labelKey: 'Erp::ManagerScore', type: 'number', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'objective', labelKey: 'Erp::Objective', type: 'text', required: true, maxLength: 250 },
      { field: 'weight', labelKey: 'Erp::Weight', type: 'number', min: 0 },
      { field: 'target', labelKey: 'Erp::Target', type: 'text', maxLength: 250 },
      { field: 'selfScore', labelKey: 'Erp::SelfScore', type: 'number', min: 0, helpKey: 'Erp::ScoreHelp' },
      { field: 'managerScore', labelKey: 'Erp::ManagerScore', type: 'number', min: 0, helpKey: 'Erp::ScoreHelp' },
      { field: 'comment', labelKey: 'Erp::Comment', type: 'textarea', wide: true },
    ],
    getList: query => this.appraisalLines.getList(query),
    get: id => this.appraisalLines.get(id),
    create: input => this.appraisalLines.create(input),
    update: (id, input) => this.appraisalLines.update(id, input),
    delete: id => this.appraisalLines.delete(id),
    toItem: dto => ({ id: dto.id, code: String(dto.lineNo), name: dto.objective ?? undefined }),
    newRecord: () => ({ weight: 1, selfScore: 0, managerScore: 0 }),
  };

  // Subscriptions

  readonly subscriptionPlan = codeTableEntity<SubscriptionPlanDto, CreateUpdateSubscriptionPlanDto>(this.plans, {
    key: 'subscriptionPlan',
    titleKey: 'Erp::SubscriptionPlan',
    pluralKey: 'Erp::SubscriptionPlans',
    icon: 'fas fa-repeat',
    permission: SUBSCRIPTIONS,
    route: '/erp/subscription-plans',
    columns: [
      { field: 'billingPeriodMonths', labelKey: 'Erp::BillingPeriodMonths', type: 'number', width: 140 },
      { field: 'price', labelKey: 'Erp::Price', type: 'currency', width: 120 },
      { field: 'lineType', labelKey: 'Erp::Type', type: 'select', options: subscriptionLineTypes, width: 120 },
      { field: 'lineNo', labelKey: 'Erp::No', width: 110 },
      { field: 'trialDays', labelKey: 'Erp::TrialDays', type: 'number', width: 100 },
    ],
    fields: [
      { field: 'billingPeriodMonths', labelKey: 'Erp::BillingPeriodMonths', type: 'number', min: 1, required: true },
      { field: 'price', labelKey: 'Erp::Price', type: 'currency', min: 0 },
      { field: 'lineType', labelKey: 'Erp::Type', type: 'select', options: subscriptionLineTypes },
      { field: 'lineNo', labelKey: 'Erp::SubscriptionLineNo', type: 'text', required: true, maxLength: 20, helpKey: 'Erp::SubscriptionLineNoHelp' },
      { field: 'trialDays', labelKey: 'Erp::TrialDays', type: 'number', min: 0 },
    ],
    defaults: { billingPeriodMonths: 1, price: 0, lineType: SubscriptionLineType.GLAccount, trialDays: 0 },
  });

  readonly customerSubscription: RecordEntity<CustomerSubscriptionDto, CreateUpdateCustomerSubscriptionDto> = {
    key: 'customerSubscription',
    titleKey: 'Erp::CustomerSubscription',
    pluralKey: 'Erp::CustomerSubscriptions',
    icon: 'fas fa-arrows-rotate',
    permission: SUBSCRIPTIONS,
    listRoute: ['/erp/customer-subscriptions'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'customerNo', labelKey: 'Erp::CustomerNo', width: 120 },
      { field: 'customerName', labelKey: 'Erp::CustomerName' },
      { field: 'planCode', labelKey: 'Erp::SubscriptionPlan', width: 130 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 90 },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', width: 110 },
      { field: 'nextBillingDate', labelKey: 'Erp::NextBillingDate', type: 'date', width: 130 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: subscriptionStatuses, width: 110 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, helpKey: 'Erp::NoFromSeriesHelp' },
      codeField('customerNo', 'Erp::CustomerNo', 'customer', 'general', { required: true }),
      codeField('planCode', 'Erp::SubscriptionPlan', 'subscriptionPlan', 'general', { required: true }),
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', min: 0 },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', min: 0, helpKey: 'Erp::SubscriptionPriceHelp' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date', required: true },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
      readonly('nextBillingDate', 'Erp::NextBillingDate'),
      readonly('lastInvoiceNo', 'Erp::LastInvoiceNo'),
    ],
    getList: query => this.subscriptions.getList(query),
    get: id => this.subscriptions.get(id),
    create: input => this.subscriptions.create(input),
    update: (id, input) => this.subscriptions.update(id, input),
    delete: id => this.subscriptions.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.customerName ?? undefined }),
    newRecord: () => ({ quantity: 1, unitPrice: 0, startDate: today() }),
    actions: [
      {
        key: 'activate',
        labelKey: 'Erp::Activate',
        icon: 'fas fa-play',
        permission: `${SUBSCRIPTIONS}.Update`,
        visible: dto => dto.status === SubscriptionStatus.Draft || dto.status === SubscriptionStatus.Paused,
        run: dto => this.subscriptions.activate(dto.id!),
      },
      {
        key: 'pause',
        labelKey: 'Erp::Pause',
        icon: 'fas fa-pause',
        permission: `${SUBSCRIPTIONS}.Update`,
        visible: dto => dto.status === SubscriptionStatus.Active,
        run: dto => this.subscriptions.pause(dto.id!),
      },
      {
        key: 'cancel',
        labelKey: 'Erp::Cancel',
        icon: 'fas fa-ban',
        permission: `${SUBSCRIPTIONS}.Update`,
        confirmKey: 'Erp::CancelSubscriptionConfirmation',
        visible: dto => dto.status === SubscriptionStatus.Active || dto.status === SubscriptionStatus.Paused,
        run: dto => this.subscriptions.cancel(dto.id!),
      },
      {
        key: 'invoice',
        labelKey: 'Erp::InvoiceDue',
        icon: 'fas fa-file-invoice-dollar',
        permission: `${SUBSCRIPTIONS}.Invoice`,
        visible: dto => dto.status === SubscriptionStatus.Active,
        run: dto => this.invoiceDialog(input => this.subscriptions.invoiceDue(dto.id!, input)),
      },
    ],
    listActions: [
      {
        key: 'invoiceAll',
        labelKey: 'Erp::InvoiceAllDue',
        icon: 'fas fa-file-invoice-dollar',
        permission: `${SUBSCRIPTIONS}.Invoice`,
        run: () => this.invoiceDialog(input => this.subscriptions.invoiceAllDue(input)),
      },
    ],
  };

  private invoiceDialog(invoice: (input: InvoiceSubscriptionsInput) => Observable<{ invoiceCount: number }>) {
    return runWithInput<InvoiceSubscriptionsInput>(this.dialogs, {
      titleKey: 'Erp::InvoiceDue',
      icon: 'fas fa-file-invoice-dollar',
      permission: `${SUBSCRIPTIONS}.Invoice`,
      fields: [
        { field: 'asOfDate', labelKey: 'Erp::AsOfDate', type: 'date', required: true },
        { field: 'post', labelKey: 'Erp::PostInvoices', type: 'checkbox', helpKey: 'Erp::PostInvoicesHelp' },
      ],
      values: { asOfDate: today(), post: false },
      submit: input => invoice(input).pipe(this.report('Erp::InvoicesCreated', (r: { invoiceCount: number }) => r.invoiceCount)),
    });
  }

  // Point of sale

  readonly posTerminal = codeTableEntity<PosTerminalDto, CreateUpdatePosTerminalDto>(this.terminals, {
    key: 'posTerminal',
    titleKey: 'Erp::PosTerminal',
    pluralKey: 'Erp::PosTerminals',
    icon: 'fas fa-cash-register',
    permission: POS,
    route: '/erp/pos-terminals',
    codeLength: 10,
    columns: [
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 110 },
      { field: 'walkInCustomerNo', labelKey: 'Erp::WalkInCustomerNo', width: 150 },
      { field: 'cashBankAccountNo', labelKey: 'Erp::CashBankAccountNo', width: 160 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    fields: [
      codeField('locationCode', 'Erp::LocationCode', 'location'),
      codeField('walkInCustomerNo', 'Erp::WalkInCustomerNo', 'customer', 'general', { required: true, helpKey: 'Erp::WalkInCustomerHelp' }),
      codeField('cashBankAccountNo', 'Erp::CashBankAccountNo', 'bankAccount', 'general', { required: true }),
      codeField('cardBankAccountNo', 'Erp::CardBankAccountNo', 'bankAccount'),
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    defaults: { blocked: false },
  });

  readonly posSession: RecordEntity<PosSessionDto, never> = {
    key: 'posSession',
    titleKey: 'Erp::PosSession',
    pluralKey: 'Erp::PosSessions',
    icon: 'fas fa-cash-register',
    permission: POS,
    listRoute: ['/erp/pos-sessions'],
    readOnly: true,
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 130 },
      { field: 'terminalCode', labelKey: 'Erp::PosTerminal', width: 110 },
      { field: 'openedAt', labelKey: 'Erp::OpenedAt', type: 'datetime', width: 160 },
      { field: 'openedBy', labelKey: 'Erp::OpenedBy', width: 120 },
      { field: 'cashSales', labelKey: 'Erp::CashSales', type: 'currency', width: 120 },
      { field: 'cardSales', labelKey: 'Erp::CardSales', type: 'currency', width: 120 },
      { field: 'difference', labelKey: 'Erp::CashDifference', type: 'currency', width: 120 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: sessionStatuses, width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('no', 'Erp::No'),
      readonly('terminalCode', 'Erp::PosTerminal'),
      readonly('openingFloat', 'Erp::OpeningFloat'),
      readonly('cashSales', 'Erp::CashSales'),
      readonly('cardSales', 'Erp::CardSales'),
      readonly('expectedCash', 'Erp::ExpectedCash'),
      readonly('countedCash', 'Erp::CountedCash'),
      readonly('difference', 'Erp::CashDifference'),
    ],
    getList: query => this.sessions.getList(query),
    get: id => this.sessions.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.terminalCode ?? undefined }),
    newRecord: () => ({}),
    actions: [
      {
        key: 'newOrder',
        labelKey: 'Erp::NewPosOrder',
        icon: 'fas fa-cart-plus',
        permission: `${POS}.Sell`,
        visible: dto => dto.status === PosSessionStatus.Open,
        run: dto =>
          runWithInput<StartPosOrderInput>(this.dialogs, {
            titleKey: 'Erp::NewPosOrder',
            icon: 'fas fa-cart-plus',
            permission: `${POS}.Sell`,
            fields: [codeField('customerNo', 'Erp::CustomerNo', 'customer', 'general', { helpKey: 'Erp::PosCustomerHelp' })],
            submit: input => this.orders.start({ ...input, sessionId: dto.id }),
          }),
      },
      {
        key: 'close',
        labelKey: 'Erp::CloseSession',
        icon: 'fas fa-lock',
        permission: `${POS}.Sell`,
        visible: dto => dto.status === PosSessionStatus.Open,
        run: dto =>
          runWithInput<{ countedCash: number }>(this.dialogs, {
            titleKey: 'Erp::CloseSession',
            icon: 'fas fa-lock',
            permission: `${POS}.Sell`,
            fields: [{ field: 'countedCash', labelKey: 'Erp::CountedCash', type: 'currency', required: true, min: 0, helpKey: 'Erp::CountedCashHelp' }],
            values: { countedCash: dto.expectedCash },
            submit: input => this.sessions.close(dto.id!, input),
          }),
      },
    ],
    listActions: [
      {
        key: 'open',
        labelKey: 'Erp::OpenSession',
        icon: 'fas fa-lock-open',
        permission: `${POS}.Sell`,
        run: () =>
          runWithInput<OpenPosSessionInput>(this.dialogs, {
            titleKey: 'Erp::OpenSession',
            icon: 'fas fa-cash-register',
            permission: `${POS}.Sell`,
            fields: [
              codeField('terminalCode', 'Erp::PosTerminal', 'posTerminal', 'general', { required: true }),
              { field: 'openingFloat', labelKey: 'Erp::OpeningFloat', type: 'currency', min: 0 },
            ],
            values: { openingFloat: 0 },
            submit: input => this.sessions.open(input),
          }),
      },
    ],
    parts: [
      {
        entity: 'posOrder',
        lines: dto => this.orders.getList({ sessionId: dto.id, ...ALL_LINES }).pipe(map(r => r.items ?? [])),
        columns: ['no', 'customerNo', 'paymentMethod', 'totalPaid', 'status'],
        totals: ['totalPaid'],
        editable: () => false,
      },
    ],
  };

  readonly posOrder: RecordEntity<PosOrderDto, never> = {
    key: 'posOrder',
    titleKey: 'Erp::PosOrder',
    pluralKey: 'Erp::PosOrders',
    icon: 'fas fa-receipt',
    permission: POS,
    listRoute: ['/erp/pos-orders'],
    readOnly: true,
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 130 },
      { field: 'terminalCode', labelKey: 'Erp::PosTerminal', width: 110 },
      { field: 'orderDate', labelKey: 'Erp::OrderDate', type: 'date', width: 120 },
      { field: 'customerNo', labelKey: 'Erp::CustomerNo', width: 120 },
      { field: 'paymentMethod', labelKey: 'Erp::PaymentMethod', type: 'select', options: paymentMethods, width: 110 },
      { field: 'subtotal', labelKey: 'Erp::Subtotal', type: 'currency', width: 120 },
      { field: 'totalPaid', labelKey: 'Erp::TotalPaid', type: 'currency', width: 120 },
      { field: 'postedInvoiceNo', labelKey: 'Erp::PostedInvoiceNo', width: 140 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: orderStatuses, width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('no', 'Erp::No'),
      readonly('customerNo', 'Erp::CustomerNo'),
      readonly('subtotal', 'Erp::Subtotal'),
      readonly('totalPaid', 'Erp::TotalPaid'),
      readonly('amountTendered', 'Erp::AmountTendered'),
      readonly('change', 'Erp::Change'),
      readonly('postedInvoiceNo', 'Erp::PostedInvoiceNo'),
    ],
    getList: query => this.orders.getList(query),
    get: id => this.orders.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.customerNo ?? undefined }),
    newRecord: () => ({}),
    actions: [
      {
        key: 'ringUp',
        labelKey: 'Erp::RingUp',
        icon: 'fas fa-barcode',
        permission: `${POS}.Sell`,
        visible: dto => dto.status === PosOrderStatus.Open,
        run: dto =>
          runWithInput<RingUpInput>(this.dialogs, {
            titleKey: 'Erp::RingUp',
            icon: 'fas fa-barcode',
            permission: `${POS}.Sell`,
            fields: [
              codeField('itemNo', 'Erp::ItemNo', 'item', 'general', { required: true }),
              { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', required: true, min: 0 },
              { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', min: 0, helpKey: 'Erp::PosUnitPriceHelp' },
              { field: 'discountPct', labelKey: 'Erp::LineDiscountPct', type: 'number', min: 0 },
            ],
            values: { quantity: 1, discountPct: 0 },
            submit: input => this.orders.ringUp(dto.id!, input),
          }),
      },
      {
        key: 'pay',
        labelKey: 'Erp::Pay',
        icon: 'fas fa-money-bill-wave',
        permission: `${POS}.Sell`,
        visible: dto => dto.status === PosOrderStatus.Open && dto.subtotal > 0,
        run: dto =>
          runWithInput<PayPosOrderInput>(this.dialogs, {
            titleKey: 'Erp::Pay',
            icon: 'fas fa-money-bill-wave',
            permission: `${POS}.Sell`,
            fields: [
              { field: 'method', labelKey: 'Erp::PaymentMethod', type: 'select', options: paymentMethods, required: true },
              { field: 'amountTendered', labelKey: 'Erp::AmountTendered', type: 'currency', min: 0, helpKey: 'Erp::AmountTenderedHelp' },
            ],
            values: { method: PosPaymentMethod.Cash, amountTendered: dto.subtotal },
            submit: input => this.orders.pay(dto.id!, input),
          }),
      },
      {
        key: 'cancel',
        labelKey: 'Erp::Cancel',
        icon: 'fas fa-ban',
        permission: `${POS}.Sell`,
        confirmKey: 'Erp::CancelPosOrderConfirmation',
        visible: dto => dto.status === PosOrderStatus.Open,
        run: dto => this.orders.cancel(dto.id!),
      },
    ],
    parts: [
      {
        entity: 'posOrderLine',
        lines: dto => this.orderLines.getList({ posOrderId: dto.id, ...ALL_LINES }).pipe(map(r => r.items ?? [])),
        columns: ['itemNo', 'description', 'quantity', 'unitPrice', 'discountPct', 'lineAmount'],
        totals: ['lineAmount'],
        editable: () => false,
      },
    ],
  };

  readonly posOrderLine: RecordEntity<PosOrderLineDto, never> = {
    key: 'posOrderLine',
    titleKey: 'Erp::PosOrderLine',
    pluralKey: 'Erp::PosOrderLines',
    icon: 'fas fa-list',
    permission: POS,
    listRoute: ['/erp/pos-orders'],
    readOnly: true,
    columns: [
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 110 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 90 },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', width: 110 },
      { field: 'discountPct', labelKey: 'Erp::LineDiscountPct', type: 'number', width: 100 },
      { field: 'lineAmount', labelKey: 'Erp::LineAmount', type: 'currency', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('itemNo', 'Erp::ItemNo'), readonly('description', 'Erp::Description'), readonly('quantity', 'Erp::Quantity'), readonly('lineAmount', 'Erp::LineAmount')],
    getList: query => this.orderLines.getList(query),
    get: id => this.orderLines.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: id => this.orderLines.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.itemNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  // Maintenance

  readonly maintenanceTeam = codeTableEntity<MaintenanceTeamDto, CreateUpdateMaintenanceTeamDto>(this.teams, {
    key: 'maintenanceTeam',
    titleKey: 'Erp::MaintenanceTeam',
    pluralKey: 'Erp::MaintenanceTeams',
    icon: 'fas fa-people-carry-box',
    permission: MAINTENANCE,
    route: '/erp/maintenance-teams',
  });

  readonly maintenanceEquipment: RecordEntity<MaintenanceEquipmentDto, CreateUpdateMaintenanceEquipmentDto> = {
    key: 'maintenanceEquipment',
    titleKey: 'Erp::Equipment',
    pluralKey: 'Erp::EquipmentPlural',
    icon: 'fas fa-gears',
    permission: MAINTENANCE,
    listRoute: ['/erp/maintenance-equipment'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'category', labelKey: 'Erp::Category', width: 130 },
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 110 },
      { field: 'teamCode', labelKey: 'Erp::MaintenanceTeam', width: 130 },
      { field: 'nextPreventiveDate', labelKey: 'Erp::NextPreventiveDate', type: 'date', width: 150 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: equipmentStatuses, width: 100 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'maintenance', labelKey: 'Erp::Maintenance' },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, helpKey: 'Erp::NoFromSeriesHelp' },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', required: true, maxLength: 100 },
      { field: 'category', labelKey: 'Erp::Category', type: 'text', maxLength: 50 },
      { field: 'serialNo', labelKey: 'Erp::SerialNo', type: 'text', maxLength: 50 },
      codeField('fixedAssetNo', 'Erp::FixedAssetNo', 'fixedAsset'),
      codeField('locationCode', 'Erp::LocationCode', 'location'),
      codeField('teamCode', 'Erp::MaintenanceTeam', 'maintenanceTeam', 'maintenance'),
      codeField('technicianEmployeeNo', 'Erp::Technician', 'employee', 'maintenance'),
      { field: 'preventiveIntervalDays', labelKey: 'Erp::PreventiveIntervalDays', type: 'number', min: 0, section: 'maintenance', helpKey: 'Erp::PreventiveIntervalHelp' },
      { field: 'nextPreventiveDate', labelKey: 'Erp::NextPreventiveDate', type: 'date', section: 'maintenance' },
      readonly('lastMaintenanceDate', 'Erp::LastMaintenanceDate', 'maintenance'),
    ],
    getList: query => this.equipment.getList(query),
    get: id => this.equipment.get(id),
    create: input => this.equipment.create(input),
    update: (id, input) => this.equipment.update(id, input),
    delete: id => this.equipment.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ preventiveIntervalDays: 0 }),
    parts: [
      {
        entity: 'maintenanceRequest',
        lines: dto => this.requests.getList({ equipmentNo: dto.no, ...ALL_LINES }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ equipmentNo: dto.no }),
        columns: ['no', 'subject', 'type', 'requestDate', 'stage'],
      },
    ],
  };

  readonly maintenanceRequest: RecordEntity<MaintenanceRequestDto, CreateUpdateMaintenanceRequestDto> = {
    key: 'maintenanceRequest',
    titleKey: 'Erp::MaintenanceRequest',
    pluralKey: 'Erp::MaintenanceRequests',
    icon: 'fas fa-screwdriver-wrench',
    permission: MAINTENANCE,
    listRoute: ['/erp/maintenance-requests'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'equipmentNo', labelKey: 'Erp::Equipment', width: 120 },
      { field: 'subject', labelKey: 'Erp::Subject' },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: maintenanceTypes, width: 110 },
      { field: 'priority', labelKey: 'Erp::Priority', type: 'select', options: maintenancePriorities, width: 100 },
      { field: 'scheduledDate', labelKey: 'Erp::ScheduledDate', type: 'date', width: 120 },
      { field: 'stage', labelKey: 'Erp::Stage', type: 'select', options: maintenanceStages, width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('equipmentNo', 'Erp::Equipment', 'maintenanceEquipment', 'general', { required: true, createOnly: true }),
      { field: 'subject', labelKey: 'Erp::Subject', type: 'text', required: true, maxLength: 100 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: maintenanceTypes },
      { field: 'priority', labelKey: 'Erp::Priority', type: 'select', options: maintenancePriorities },
      { field: 'requestDate', labelKey: 'Erp::RequestDate', type: 'date' },
      { field: 'scheduledDate', labelKey: 'Erp::ScheduledDate', type: 'date' },
      codeField('teamCode', 'Erp::MaintenanceTeam', 'maintenanceTeam'),
      codeField('technicianEmployeeNo', 'Erp::Technician', 'employee'),
      { field: 'durationHours', labelKey: 'Erp::DurationHours', type: 'number', min: 0 },
      { field: 'cost', labelKey: 'Erp::Cost', type: 'currency', min: 0 },
      { field: 'notes', labelKey: 'Erp::Notes', type: 'textarea', wide: true },
    ],
    getList: query => this.requests.getList(query),
    get: id => this.requests.get(id),
    create: input => this.requests.create(input),
    update: (id, input) => this.requests.update(id, input),
    delete: id => this.requests.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.subject ?? undefined }),
    newRecord: () => ({ type: MaintenanceType.Corrective, priority: MaintenancePriority.Normal, requestDate: today(), durationHours: 0, cost: 0 }),
    actions: [
      {
        key: 'start',
        labelKey: 'Erp::StartWork',
        icon: 'fas fa-play',
        permission: `${MAINTENANCE}.Update`,
        visible: dto => dto.stage === MaintenanceStage.New,
        run: dto => this.requests.start(dto.id!),
      },
      {
        key: 'complete',
        labelKey: 'Erp::MarkRepaired',
        icon: 'fas fa-check',
        permission: `${MAINTENANCE}.Update`,
        visible: dto => dto.stage === MaintenanceStage.New || dto.stage === MaintenanceStage.InProgress,
        run: dto => this.requests.complete(dto.id!),
      },
      {
        key: 'scrap',
        labelKey: 'Erp::Scrap',
        icon: 'fas fa-trash-can',
        permission: `${MAINTENANCE}.Update`,
        confirmKey: 'Erp::ScrapEquipmentConfirmation',
        visible: dto => dto.stage === MaintenanceStage.New || dto.stage === MaintenanceStage.InProgress,
        run: dto => this.requests.scrap(dto.id!),
      },
    ],
    listActions: [
      {
        key: 'preventive',
        labelKey: 'Erp::GeneratePreventiveRequests',
        icon: 'fas fa-calendar-plus',
        permission: `${MAINTENANCE}.Create`,
        run: () =>
          runWithInput<{ asOfDate: string }>(this.dialogs, {
            titleKey: 'Erp::GeneratePreventiveRequests',
            icon: 'fas fa-calendar-plus',
            permission: `${MAINTENANCE}.Create`,
            fields: [{ field: 'asOfDate', labelKey: 'Erp::AsOfDate', type: 'date', required: true }],
            values: { asOfDate: today() },
            submit: input => this.requests.generatePreventive(input).pipe(this.report('Erp::MaintenanceRequestsCreated', (r: { requestsCreated: number }) => r.requestsCreated)),
          }),
      },
    ],
  };

  // Quality

  readonly qualityControlPoint = codeTableEntity<QualityControlPointDto, CreateUpdateQualityControlPointDto>(this.controlPoints, {
    key: 'qualityControlPoint',
    titleKey: 'Erp::QualityControlPoint',
    pluralKey: 'Erp::QualityControlPoints',
    icon: 'fas fa-clipboard-check',
    permission: QUALITY,
    route: '/erp/quality-control-points',
    columns: [
      { field: 'operation', labelKey: 'Erp::Operation', type: 'select', options: qualityOperations, width: 120 },
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 110 },
      { field: 'itemCategoryCode', labelKey: 'Erp::ItemCategoryCode', width: 130 },
      { field: 'testType', labelKey: 'Erp::TestType', type: 'select', options: qualityTestTypes, width: 110 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    fields: [
      { field: 'operation', labelKey: 'Erp::Operation', type: 'select', options: qualityOperations },
      codeField('itemNo', 'Erp::ItemNo', 'item', 'general', { helpKey: 'Erp::ControlPointItemHelp' }),
      codeField('itemCategoryCode', 'Erp::ItemCategoryCode', 'itemCategory'),
      { field: 'testType', labelKey: 'Erp::TestType', type: 'select', options: qualityTestTypes },
      { field: 'minValue', labelKey: 'Erp::MinValue', type: 'number', helpKey: 'Erp::MeasureRangeHelp' },
      { field: 'maxValue', labelKey: 'Erp::MaxValue', type: 'number' },
      { field: 'unitOfMeasure', labelKey: 'Erp::UnitOfMeasureCode', type: 'text', maxLength: 10 },
      { field: 'instructions', labelKey: 'Erp::Instructions', type: 'textarea', wide: true },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    defaults: { operation: QualityOperation.Receipt, testType: QualityTestType.PassFail, minValue: 0, maxValue: 0, blocked: false },
  });

  readonly qualityCheck: RecordEntity<QualityCheckDto, never> = {
    key: 'qualityCheck',
    titleKey: 'Erp::QualityCheck',
    pluralKey: 'Erp::QualityChecks',
    icon: 'fas fa-magnifying-glass-chart',
    permission: QUALITY,
    listRoute: ['/erp/quality-checks'],
    readOnly: true,
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'controlPointCode', labelKey: 'Erp::QualityControlPoint', width: 140 },
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 110 },
      { field: 'itemDescription', labelKey: 'Erp::Description' },
      { field: 'sourceDocumentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 90 },
      { field: 'measuredValue', labelKey: 'Erp::MeasuredValue', type: 'number', width: 120 },
      { field: 'result', labelKey: 'Erp::Result', type: 'select', options: qualityResults, width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('no', 'Erp::No'),
      readonly('controlPointCode', 'Erp::QualityControlPoint'),
      readonly('itemNo', 'Erp::ItemNo'),
      readonly('sourceDocumentNo', 'Erp::DocumentNo'),
      readonly('lotNo', 'Erp::LotNo'),
      readonly('minValue', 'Erp::MinValue'),
      readonly('maxValue', 'Erp::MaxValue'),
      readonly('measuredValue', 'Erp::MeasuredValue'),
      readonly('checkedBy', 'Erp::CheckedBy'),
      readonly('note', 'Erp::Note'),
    ],
    getList: query => this.checks.getList(query),
    get: id => this.checks.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.itemDescription ?? undefined }),
    newRecord: () => ({}),
    actions: [
      {
        key: 'record',
        labelKey: 'Erp::RecordResult',
        icon: 'fas fa-pen-to-square',
        permission: `${QUALITY}.Record`,
        visible: dto => dto.result === QualityCheckResult.Pending,
        run: dto =>
          runWithInput<RecordQualityCheckInput>(this.dialogs, {
            titleKey: 'Erp::RecordResult',
            icon: 'fas fa-pen-to-square',
            permission: `${QUALITY}.Record`,
            fields:
              dto.testType === QualityTestType.Measure
                ? [{ field: 'measuredValue', labelKey: 'Erp::MeasuredValue', type: 'number', required: true }, { field: 'note', labelKey: 'Erp::Note', type: 'textarea', wide: true }]
                : [{ field: 'passed', labelKey: 'Erp::Passed', type: 'checkbox' }, { field: 'note', labelKey: 'Erp::Note', type: 'textarea', wide: true }],
            values: { passed: true },
            submit: input => this.checks.record(dto.id!, input),
          }),
      },
      {
        key: 'reset',
        labelKey: 'Erp::ResetCheck',
        icon: 'fas fa-rotate-left',
        permission: `${QUALITY}.Record`,
        visible: dto => dto.result !== QualityCheckResult.Pending,
        run: dto => this.checks.reset(dto.id!),
      },
    ],
    listActions: [
      {
        key: 'create',
        labelKey: 'Erp::NewQualityCheck',
        icon: 'fas fa-plus',
        permission: `${QUALITY}.Create`,
        run: () =>
          runWithInput<CreateQualityCheckDto>(this.dialogs, {
            titleKey: 'Erp::NewQualityCheck',
            icon: 'fas fa-clipboard-check',
            permission: `${QUALITY}.Create`,
            fields: [
              codeField('controlPointCode', 'Erp::QualityControlPoint', 'qualityControlPoint', 'general', { required: true }),
              codeField('itemNo', 'Erp::ItemNo', 'item', 'general', { required: true }),
              { field: 'sourceDocumentNo', labelKey: 'Erp::DocumentNo', type: 'text', maxLength: 20 },
              { field: 'lotNo', labelKey: 'Erp::LotNo', type: 'text', maxLength: 50 },
              { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', min: 0 },
            ],
            values: { quantity: 1 },
            submit: input => this.checks.create(input),
          }),
      },
    ],
  };

  // Fleet

  readonly vehicle: RecordEntity<VehicleDto, CreateUpdateVehicleDto> = {
    key: 'vehicle',
    titleKey: 'Erp::Vehicle',
    pluralKey: 'Erp::Vehicles',
    icon: 'fas fa-truck',
    permission: FLEET,
    listRoute: ['/erp/vehicles'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'licensePlate', labelKey: 'Erp::LicensePlate', width: 130 },
      { field: 'make', labelKey: 'Erp::Make', width: 120 },
      { field: 'model', labelKey: 'Erp::Model' },
      { field: 'driverEmployeeNo', labelKey: 'Erp::Driver', width: 120 },
      { field: 'odometer', labelKey: 'Erp::Odometer', type: 'number', width: 110 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: vehicleStatuses, width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, helpKey: 'Erp::NoFromSeriesHelp' },
      { field: 'licensePlate', labelKey: 'Erp::LicensePlate', type: 'text', required: true, maxLength: 20 },
      { field: 'make', labelKey: 'Erp::Make', type: 'text', maxLength: 50 },
      { field: 'model', labelKey: 'Erp::Model', type: 'text', maxLength: 50 },
      { field: 'year', labelKey: 'Erp::Year', type: 'number', min: 0 },
      { field: 'vin', labelKey: 'Erp::Vin', type: 'text', maxLength: 30 },
      { field: 'fuelType', labelKey: 'Erp::FuelType', type: 'text', maxLength: 20 },
      codeField('driverEmployeeNo', 'Erp::Driver', 'employee'),
      codeField('fixedAssetNo', 'Erp::FixedAssetNo', 'fixedAsset'),
      { field: 'acquisitionDate', labelKey: 'Erp::AcquisitionDate', type: 'date' },
      readonly('odometer', 'Erp::Odometer'),
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: vehicleStatuses },
    ],
    getList: query => this.vehicles.getList(query),
    get: id => this.vehicles.get(id),
    create: input => this.vehicles.create(input),
    update: (id, input) => this.vehicles.update(id, input),
    delete: id => this.vehicles.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.licensePlate ?? undefined }),
    newRecord: () => ({ status: VehicleStatus.Active, year: new Date().getFullYear() }),
    parts: [
      {
        entity: 'vehicleLogEntry',
        lines: dto => this.vehicleLogs.getList({ vehicleNo: dto.no, ...ALL_LINES }).pipe(map(r => r.items ?? [])),
        columns: ['date', 'type', 'odometer', 'liters', 'cost'],
        totals: ['liters', 'cost'],
        editable: () => false,
      },
      {
        entity: 'vehicleContract',
        lines: dto => this.vehicleContracts.getList({ vehicleNo: dto.no, ...ALL_LINES }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ vehicleNo: dto.no }),
        columns: ['type', 'vendorNo', 'startDate', 'endDate', 'recurringCost', 'costFrequency'],
      },
    ],
  };

  readonly vehicleLogEntry: RecordEntity<VehicleLogEntryDto, never> = {
    key: 'vehicleLogEntry',
    titleKey: 'Erp::VehicleLogEntry',
    pluralKey: 'Erp::VehicleLogEntries',
    icon: 'fas fa-gas-pump',
    permission: FLEET,
    listRoute: ['/erp/vehicle-log-entries'],
    readOnly: true,
    columns: [
      { field: 'date', labelKey: 'Erp::Date', type: 'date', width: 120 },
      { field: 'vehicleNo', labelKey: 'Erp::Vehicle', width: 110 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: vehicleLogTypes, width: 110 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'odometer', labelKey: 'Erp::Odometer', type: 'number', width: 110 },
      { field: 'liters', labelKey: 'Erp::Liters', type: 'number', width: 90 },
      { field: 'cost', labelKey: 'Erp::Cost', type: 'currency', width: 110 },
      { field: 'vendorNo', labelKey: 'Erp::VendorNo', width: 110 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('vehicleNo', 'Erp::Vehicle'), readonly('date', 'Erp::Date'), readonly('odometer', 'Erp::Odometer'), readonly('cost', 'Erp::Cost')],
    getList: query => this.vehicleLogs.getList(query),
    get: id => this.vehicleLogs.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: id => this.vehicleLogs.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.vehicleNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({}),
    listActions: [
      {
        key: 'log',
        labelKey: 'Erp::LogVehicleEntry',
        icon: 'fas fa-gas-pump',
        permission: `${FLEET}.Create`,
        run: () =>
          runWithInput<CreateVehicleLogEntryDto>(this.dialogs, {
            titleKey: 'Erp::LogVehicleEntry',
            icon: 'fas fa-gas-pump',
            permission: `${FLEET}.Create`,
            fields: [
              codeField('vehicleNo', 'Erp::Vehicle', 'vehicle', 'general', { required: true }),
              { field: 'date', labelKey: 'Erp::Date', type: 'date', required: true },
              { field: 'type', labelKey: 'Erp::Type', type: 'select', options: vehicleLogTypes },
              { field: 'odometer', labelKey: 'Erp::Odometer', type: 'number', min: 0, helpKey: 'Erp::OdometerHelp' },
              { field: 'liters', labelKey: 'Erp::Liters', type: 'number', min: 0 },
              { field: 'cost', labelKey: 'Erp::Cost', type: 'currency', min: 0 },
              codeField('vendorNo', 'Erp::VendorNo', 'vendor'),
              { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 100 },
            ],
            values: { date: today(), type: VehicleLogType.Fuel, odometer: 0, liters: 0, cost: 0 },
            submit: input => this.vehicleLogs.create(input),
          }),
      },
    ],
  };

  readonly vehicleContract: RecordEntity<VehicleContractDto, CreateUpdateVehicleContractDto> = {
    key: 'vehicleContract',
    titleKey: 'Erp::VehicleContract',
    pluralKey: 'Erp::VehicleContracts',
    icon: 'fas fa-file-contract',
    permission: FLEET,
    listRoute: ['/erp/vehicle-contracts'],
    columns: [
      { field: 'vehicleNo', labelKey: 'Erp::Vehicle', width: 110 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: contractTypes, width: 110 },
      { field: 'vendorNo', labelKey: 'Erp::VendorNo', width: 110 },
      { field: 'reference', labelKey: 'Erp::Reference' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date', width: 120 },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date', width: 120 },
      { field: 'recurringCost', labelKey: 'Erp::RecurringCost', type: 'currency', width: 120 },
      { field: 'costFrequency', labelKey: 'Erp::CostFrequency', type: 'select', options: costFrequencies, width: 110 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('vehicleNo', 'Erp::Vehicle', 'vehicle', 'general', { required: true, createOnly: true }),
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: contractTypes },
      codeField('vendorNo', 'Erp::VendorNo', 'vendor'),
      { field: 'reference', labelKey: 'Erp::Reference', type: 'text', maxLength: 50 },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date', required: true },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
      { field: 'recurringCost', labelKey: 'Erp::RecurringCost', type: 'currency', min: 0 },
      { field: 'costFrequency', labelKey: 'Erp::CostFrequency', type: 'select', options: costFrequencies },
    ],
    getList: query => this.vehicleContracts.getList(query),
    get: id => this.vehicleContracts.get(id),
    create: input => this.vehicleContracts.create(input),
    update: (id, input) => this.vehicleContracts.update(id, input),
    delete: id => this.vehicleContracts.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.vehicleNo ?? '', name: dto.reference ?? undefined }),
    newRecord: () => ({ type: VehicleContractType.Leasing, startDate: today(), recurringCost: 0, costFrequency: ContractCostFrequency.Monthly }),
  };

  /** What each vehicle cost this year, per kilometre driven. */
  readonly vehicleCost: RecordEntity<VehicleCostDto & { id?: string }, never> = {
    key: 'vehicleCost',
    titleKey: 'Erp::VehicleCost',
    pluralKey: 'Erp::VehicleCosts',
    icon: 'fas fa-chart-column',
    permission: FLEET,
    listRoute: ['/erp/vehicle-costs'],
    readOnly: true,
    noCreate: true,
    columns: [
      { field: 'vehicleNo', labelKey: 'Erp::Vehicle', width: 110 },
      { field: 'licensePlate', labelKey: 'Erp::LicensePlate', width: 130 },
      { field: 'distance', labelKey: 'Erp::Distance', type: 'number', width: 110 },
      { field: 'liters', labelKey: 'Erp::Liters', type: 'number', width: 100 },
      { field: 'fuelCost', labelKey: 'Erp::FuelCost', type: 'currency', width: 120 },
      { field: 'serviceCost', labelKey: 'Erp::ServiceCost', type: 'currency', width: 120 },
      { field: 'contractCost', labelKey: 'Erp::ContractCost', type: 'currency', width: 120 },
      { field: 'totalCost', labelKey: 'Erp::TotalCost', type: 'currency', width: 120 },
      { field: 'costPerKm', labelKey: 'Erp::CostPerKm', type: 'currency', width: 110 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('vehicleNo', 'Erp::Vehicle'), readonly('totalCost', 'Erp::TotalCost'), readonly('costPerKm', 'Erp::CostPerKm')],
    getList: query =>
      this.vehicles.getCosts({ fromDate: yearStart(), toDate: today() }).pipe(
        map(costs => {
          const term = (query.filter ?? '').toLowerCase();
          const items = costs
            .filter(c => !term || (c.vehicleNo ?? '').toLowerCase().includes(term) || (c.licensePlate ?? '').toLowerCase().includes(term))
            .map(c => ({ ...c, id: c.vehicleNo }));
          return { totalCount: items.length, items: items.slice(query.skipCount, query.skipCount + query.maxResultCount) };
        }),
      ),
    get: id => this.vehicles.getCosts({ fromDate: yearStart(), toDate: today() }).pipe(map(costs => ({ ...costs.find(c => c.vehicleNo === id)!, id }))),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.vehicleNo ?? '', name: dto.licensePlate ?? undefined }),
    newRecord: () => ({}),
  };

  // Marketing

  readonly segment: RecordEntity<SegmentDto, CreateUpdateSegmentDto> = {
    key: 'segment',
    titleKey: 'Erp::Segment',
    pluralKey: 'Erp::Segments',
    icon: 'fas fa-users-viewfinder',
    permission: MARKETING,
    listRoute: ['/erp/segments'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'contactCount', labelKey: 'Erp::Contacts', type: 'number', width: 100, sortable: false },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, helpKey: 'Erp::NoFromSeriesHelp' },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 100 },
    ],
    getList: query => this.segments.getList(query),
    get: id => this.segments.get(id),
    create: input => this.segments.create(input),
    update: (id, input) => this.segments.update(id, input),
    delete: id => this.segments.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({}),
    actions: [
      {
        key: 'pick',
        labelKey: 'Erp::AddContacts',
        icon: 'fas fa-user-plus',
        permission: `${MARKETING}.Update`,
        run: dto =>
          runWithInput<PickSegmentContactsInput>(this.dialogs, {
            titleKey: 'Erp::AddContacts',
            icon: 'fas fa-user-plus',
            permission: `${MARKETING}.Update`,
            fields: [
              { field: 'city', labelKey: 'Erp::City', type: 'text', maxLength: 30 },
              codeField('salespersonCode', 'Erp::SalespersonCode', 'salespersonPurchaser'),
              { field: 'type', labelKey: 'Erp::Type', type: 'select', options: contactTypes },
              { field: 'onlyWithEmail', labelKey: 'Erp::OnlyWithEmail', type: 'checkbox' },
            ],
            values: { onlyWithEmail: true },
            submit: input => this.segments.pickContacts(dto.id!, input).pipe(this.report('Erp::ContactsAdded', (r: { count: number }) => r.count)),
          }),
      },
      {
        key: 'apply',
        labelKey: 'Erp::ApplyToCampaign',
        icon: 'fas fa-bullhorn',
        permission: `${MARKETING}.Update`,
        visible: dto => dto.contactCount > 0,
        run: dto =>
          runWithInput<{ campaignNo: string }>(this.dialogs, {
            titleKey: 'Erp::ApplyToCampaign',
            icon: 'fas fa-bullhorn',
            permission: `${MARKETING}.Update`,
            fields: [codeField('campaignNo', 'Erp::CampaignNo', 'campaign', 'general', { required: true })],
            submit: input => this.segments.applyToCampaign(dto.id!, input).pipe(this.report('Erp::CampaignTargetsAdded', (r: { count: number }) => r.count)),
          }),
      },
    ],
    parts: [
      {
        entity: 'segmentLine',
        lines: dto => this.segmentLines.getList({ segmentId: dto.id, ...ALL_LINES }).pipe(map(r => r.items ?? [])),
        columns: ['contactNo', 'contactName', 'email'],
        editable: () => false,
      },
    ],
  };

  readonly segmentLine: RecordEntity<SegmentLineDto, never> = {
    key: 'segmentLine',
    titleKey: 'Erp::SegmentLine',
    pluralKey: 'Erp::SegmentLines',
    icon: 'fas fa-user',
    permission: MARKETING,
    listRoute: ['/erp/segments'],
    readOnly: true,
    columns: [
      { field: 'contactNo', labelKey: 'Erp::ContactNo', width: 120 },
      { field: 'contactName', labelKey: 'Erp::ContactName' },
      { field: 'email', labelKey: 'Erp::Email', width: 220 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('contactNo', 'Erp::ContactNo'), readonly('contactName', 'Erp::ContactName'), readonly('email', 'Erp::Email')],
    getList: query => this.segmentLines.getList(query),
    get: id => this.segmentLines.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: id => this.segmentLines.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.contactNo ?? '', name: dto.contactName ?? undefined }),
    newRecord: () => ({}),
  };

  readonly campaignMailing: RecordEntity<CampaignMailingEntryDto, never> = {
    key: 'campaignMailing',
    titleKey: 'Erp::CampaignMailingEntry',
    pluralKey: 'Erp::CampaignMailingEntries',
    icon: 'fas fa-envelopes-bulk',
    permission: MARKETING,
    listRoute: ['/erp/campaign-mailings'],
    readOnly: true,
    columns: [
      { field: 'sentAt', labelKey: 'Erp::SentAt', type: 'datetime', width: 160 },
      { field: 'campaignNo', labelKey: 'Erp::CampaignNo', width: 120 },
      { field: 'contactNo', labelKey: 'Erp::ContactNo', width: 120 },
      { field: 'contactName', labelKey: 'Erp::ContactName' },
      { field: 'email', labelKey: 'Erp::Email', width: 200 },
      { field: 'subject', labelKey: 'Erp::Subject', width: 200 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: mailingStatuses, width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('campaignNo', 'Erp::CampaignNo'), readonly('email', 'Erp::Email'), readonly('subject', 'Erp::Subject'), readonly('errorMessage', 'Erp::ErrorMessage')],
    getList: query => this.mailings.getList(query),
    get: id => this.mailings.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.contactNo ?? '', name: dto.subject ?? undefined }),
    newRecord: () => ({}),
    listActions: [
      {
        key: 'send',
        labelKey: 'Erp::SendMailing',
        icon: 'fas fa-paper-plane',
        permission: `${MARKETING}.Send`,
        run: () =>
          runWithInput<SendMailingInput>(this.dialogs, {
            titleKey: 'Erp::SendMailing',
            icon: 'fas fa-envelopes-bulk',
            permission: `${MARKETING}.Send`,
            fields: [
              codeField('campaignNo', 'Erp::CampaignNo', 'campaign', 'general', { required: true }),
              { field: 'subject', labelKey: 'Erp::Subject', type: 'text', required: true, maxLength: 250 },
              { field: 'bodyHtml', labelKey: 'Erp::MailingBody', type: 'textarea', required: true, wide: true, helpKey: 'Erp::MailingBodyHelp' },
            ],
            submit: input => this.mailings.send(input).pipe(this.report('Erp::MailingSent', (r: { sent: number }) => r.sent)),
          }),
      },
    ],
  };

  get all(): RecordEntity[] {
    return [
      this.jobVacancy,
      this.jobApplicant,
      this.appraisalPeriod,
      this.appraisal,
      this.appraisalLine,
      this.subscriptionPlan,
      this.customerSubscription,
      this.posTerminal,
      this.posSession,
      this.posOrder,
      this.posOrderLine,
      this.maintenanceTeam,
      this.maintenanceEquipment,
      this.maintenanceRequest,
      this.qualityControlPoint,
      this.qualityCheck,
      this.vehicle,
      this.vehicleLogEntry,
      this.vehicleContract,
      this.vehicleCost,
      this.segment,
      this.segmentLine,
      this.campaignMailing,
    ] as RecordEntity[];
  }
}
