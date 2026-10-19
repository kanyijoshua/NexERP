import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import {
  CashFlowForecastDto,
  CashFlowForecastEntryDto,
  CashFlowForecastEntryService,
  CashFlowForecastService,
  CashFlowManualEntryDto,
  CashFlowManualEntryService,
  CashFlowManualKind,
  CashFlowSourceType,
  CashFlowWorksheetLineDto,
  CashFlowWorksheetLineService,
  CollectionDocumentDto,
  CollectionDocumentStatus,
  CreateUpdateCashFlowForecastDto,
  CreateUpdateCashFlowManualEntryDto,
  CreateUpdateCashFlowWorksheetLineDto,
  CreateUpdateDeferralTemplateDto,
  CreateUpdateFinanceChargeTermsDto,
  CreateUpdateReminderLevelDto,
  CreateUpdateReminderTermsDto,
  DeferralCalculationMethod,
  DeferralStartDate,
  DeferralTemplateDto,
  DeferralTemplateService,
  FinanceChargeMemoService,
  FinanceChargeTermsDto,
  FinanceChargeTermsService,
  PostedDeferralHeaderDto,
  PostedDeferralService,
  ReminderLevelDto,
  ReminderLevelService,
  ReminderService,
  ReminderTermsDto,
  ReminderTermsService,
  UpdateCollectionDocumentDto,
  cashFlowManualKindOptions,
  cashFlowSourceTypeOptions,
  collectionDocumentStatusOptions,
  collectionLineTypeOptions,
  deferralCalculationMethodOptions,
  deferralSourceTypeOptions,
  deferralStartDateOptions,
} from '@proxy/finance';
import { EMPTY, Observable, map, of } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { accountField, codeField, codeTableEntity, enumOptions } from './entity-helpers';

const COLLECTION_SETUP = 'Erp.CollectionSetup';
const DEFERRALS = 'Erp.Deferrals';
const CASH_FLOW = 'Erp.CashFlow';

const today = () => new Date().toISOString().substring(0, 10);

function readonly(field: string, labelKey: string, section = 'general'): RecordField {
  return { field, labelKey, type: 'readonly', section };
}

/** What the reminder and the finance charge memo services both offer. */
interface CollectionApi {
  getList(input: object): Observable<{ items?: CollectionDocumentDto[]; totalCount?: number }>;
  get(id: string): Observable<CollectionDocumentDto>;
  update(id: string, input: UpdateCollectionDocumentDto): Observable<CollectionDocumentDto>;
  delete(id: string): Observable<void>;
  suggest(input: { documentDate?: string }): Observable<number>;
  issue(id: string, input: { postingDate?: string }): Observable<CollectionDocumentDto>;
}

/**
 * Reminders and finance charge memos, their terms, deferral templates and posted schedules, and
 * the cash flow forecast with its manual lines, worksheet and registered entries.
 */
@Injectable({ providedIn: 'root' })
export class FinanceOperationsEntities {
  private readonly router = inject(Router);
  private readonly reminderTermsService = inject(ReminderTermsService);
  private readonly reminderLevels = inject(ReminderLevelService);
  private readonly financeTerms = inject(FinanceChargeTermsService);
  private readonly reminders = inject(ReminderService);
  private readonly memos = inject(FinanceChargeMemoService);
  private readonly deferralTemplates = inject(DeferralTemplateService);
  private readonly postedDeferrals = inject(PostedDeferralService);
  private readonly forecasts = inject(CashFlowForecastService);
  private readonly manualEntries = inject(CashFlowManualEntryService);
  private readonly worksheetLines = inject(CashFlowWorksheetLineService);
  private readonly forecastEntries = inject(CashFlowForecastEntryService);

  readonly reminderTerms = codeTableEntity<ReminderTermsDto, CreateUpdateReminderTermsDto>(this.reminderTermsService, {
    key: 'reminderTerms',
    titleKey: 'Erp::ReminderTerms',
    pluralKey: 'Erp::ReminderTermsPlural',
    icon: 'fas fa-bell',
    permission: COLLECTION_SETUP,
    route: '/erp/reminder-terms',
    codeLength: 10,
    columns: [
      { field: 'maxNoOfReminders', labelKey: 'Erp::MaxNoOfReminders', type: 'number', width: 150 },
      { field: 'postInterest', labelKey: 'Erp::PostInterest', type: 'boolean', width: 120 },
      { field: 'postAdditionalFee', labelKey: 'Erp::PostAdditionalFee', type: 'boolean', width: 150 },
    ],
    fields: [
      { field: 'postInterest', labelKey: 'Erp::PostInterest', type: 'checkbox' },
      { field: 'postAdditionalFee', labelKey: 'Erp::PostAdditionalFee', type: 'checkbox' },
      { field: 'maxNoOfReminders', labelKey: 'Erp::MaxNoOfReminders', type: 'number', min: 0 },
      { field: 'minimumAmountLcy', labelKey: 'Erp::MinimumAmountLcy', type: 'currency', min: 0 },
    ],
    defaults: { postInterest: true, postAdditionalFee: true, maxNoOfReminders: 3, minimumAmountLcy: 0 },
  });

  readonly reminderLevel: RecordEntity<ReminderLevelDto, CreateUpdateReminderLevelDto> = {
    key: 'reminderLevel',
    titleKey: 'Erp::ReminderLevel',
    pluralKey: 'Erp::ReminderLevels',
    icon: 'fas fa-stairs',
    permission: COLLECTION_SETUP,
    listRoute: ['/erp/reminder-terms'],
    columns: [
      { field: 'reminderTermsCode', labelKey: 'Erp::ReminderTermsCode', width: 130 },
      { field: 'no', labelKey: 'Erp::No', type: 'number', width: 80 },
      { field: 'gracePeriod', labelKey: 'Erp::GracePeriod', width: 120 },
      { field: 'dueDateCalculation', labelKey: 'Erp::DueDateCalculation', width: 150 },
      { field: 'additionalFeeLcy', labelKey: 'Erp::AdditionalFeeLcy', type: 'currency', width: 150 },
      { field: 'calculateInterest', labelKey: 'Erp::CalculateInterest', type: 'boolean', width: 140 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('reminderTermsCode', 'Erp::ReminderTermsCode', 'reminderTerms', 'general', { required: true, createOnly: true }),
      { field: 'no', labelKey: 'Erp::No', type: 'number', min: 0 },
      { field: 'gracePeriod', labelKey: 'Erp::GracePeriod', type: 'text', maxLength: 32, helpKey: 'Erp::DateFormulaHelp' },
      { field: 'dueDateCalculation', labelKey: 'Erp::DueDateCalculation', type: 'text', maxLength: 32 },
      { field: 'additionalFeeLcy', labelKey: 'Erp::AdditionalFeeLcy', type: 'currency', min: 0 },
      { field: 'calculateInterest', labelKey: 'Erp::CalculateInterest', type: 'checkbox' },
    ],
    getList: query => this.reminderLevels.getList(query),
    get: id => this.reminderLevels.get(id),
    create: input => this.reminderLevels.create(input),
    update: (id, input) => this.reminderLevels.update(id, input),
    delete: id => this.reminderLevels.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.reminderTermsCode ?? ''} ${dto.no}`, name: dto.gracePeriod ?? undefined }),
    newRecord: () => ({ no: 0, additionalFeeLcy: 0, calculateInterest: false }),
  };

  readonly financeChargeTerms = codeTableEntity<FinanceChargeTermsDto, CreateUpdateFinanceChargeTermsDto>(this.financeTerms, {
    key: 'financeChargeTerms',
    titleKey: 'Erp::FinanceChargeTerms',
    pluralKey: 'Erp::FinanceChargeTermsPlural',
    icon: 'fas fa-percent',
    permission: COLLECTION_SETUP,
    route: '/erp/finance-charge-terms',
    codeLength: 10,
    columns: [
      { field: 'interestRate', labelKey: 'Erp::InterestRate', type: 'number', width: 120 },
      { field: 'interestPeriodDays', labelKey: 'Erp::InterestPeriodDays', type: 'number', width: 150 },
      { field: 'additionalFeeLcy', labelKey: 'Erp::AdditionalFeeLcy', type: 'currency', width: 140 },
    ],
    fields: [
      { field: 'interestRate', labelKey: 'Erp::InterestRate', type: 'number', min: 0 },
      { field: 'interestPeriodDays', labelKey: 'Erp::InterestPeriodDays', type: 'number', min: 1 },
      { field: 'minimumAmountLcy', labelKey: 'Erp::MinimumAmountLcy', type: 'currency', min: 0 },
      { field: 'additionalFeeLcy', labelKey: 'Erp::AdditionalFeeLcy', type: 'currency', min: 0 },
      { field: 'gracePeriod', labelKey: 'Erp::GracePeriod', type: 'text', maxLength: 32 },
      { field: 'dueDateCalculation', labelKey: 'Erp::DueDateCalculation', type: 'text', maxLength: 32 },
      { field: 'postInterest', labelKey: 'Erp::PostInterest', type: 'checkbox' },
      { field: 'postAdditionalFee', labelKey: 'Erp::PostAdditionalFee', type: 'checkbox' },
    ],
    defaults: { interestRate: 1.5, interestPeriodDays: 30, minimumAmountLcy: 0, additionalFeeLcy: 0, postInterest: true, postAdditionalFee: true },
  });

  readonly reminder = this.collectionEntity('reminder', 'Erp::Reminder', 'Erp::Reminders', 'fas fa-bell', 'Erp.Reminders', '/erp/reminders', this.reminders as unknown as CollectionApi, 'reminderLine', 'reminderTerms');
  readonly reminderLine = this.collectionLineEntity('reminderLine', '/erp/reminders', 'Erp.Reminders');
  readonly financeChargeMemo = this.collectionEntity(
    'financeChargeMemo',
    'Erp::FinanceChargeMemo',
    'Erp::FinanceChargeMemos',
    'fas fa-file-invoice-dollar',
    'Erp.FinanceChargeMemos',
    '/erp/finance-charge-memos',
    this.memos as unknown as CollectionApi,
    'financeChargeMemoLine',
    'financeChargeTerms',
  );
  readonly financeChargeMemoLine = this.collectionLineEntity('financeChargeMemoLine', '/erp/finance-charge-memos', 'Erp.FinanceChargeMemos');

  readonly deferralTemplate = codeTableEntity<DeferralTemplateDto, CreateUpdateDeferralTemplateDto>(this.deferralTemplates, {
    key: 'deferralTemplate',
    titleKey: 'Erp::DeferralTemplate',
    pluralKey: 'Erp::DeferralTemplates',
    icon: 'fas fa-calendar-days',
    permission: DEFERRALS,
    route: '/erp/deferral-templates',
    codeLength: 10,
    columns: [
      { field: 'deferralAccountNo', labelKey: 'Erp::DeferralAccountNo', width: 150 },
      { field: 'calcMethod', labelKey: 'Erp::CalcMethod', type: 'select', options: enumOptions(deferralCalculationMethodOptions, 'DeferralCalculationMethod'), width: 150 },
      { field: 'noOfPeriods', labelKey: 'Erp::NoOfPeriods', type: 'number', width: 120 },
    ],
    fields: [
      accountField('deferralAccountNo', 'Erp::DeferralAccountNo', 'general', true),
      { field: 'deferralPct', labelKey: 'Erp::DeferralPct', type: 'number', min: 0 },
      { field: 'calcMethod', labelKey: 'Erp::CalcMethod', type: 'select', options: enumOptions(deferralCalculationMethodOptions, 'DeferralCalculationMethod') },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'select', options: enumOptions(deferralStartDateOptions, 'DeferralStartDate') },
      { field: 'noOfPeriods', labelKey: 'Erp::NoOfPeriods', type: 'number', min: 1 },
      { field: 'periodDescription', labelKey: 'Erp::PeriodDescription', type: 'text', maxLength: 100, helpKey: 'Erp::PeriodDescriptionHelp' },
    ],
    defaults: { deferralPct: 100, calcMethod: DeferralCalculationMethod.StraightLine, startDate: DeferralStartDate.BeginningOfPeriod, noOfPeriods: 12 },
  });

  readonly postedDeferral: RecordEntity<PostedDeferralHeaderDto, never> = {
    key: 'postedDeferral',
    titleKey: 'Erp::PostedDeferral',
    pluralKey: 'Erp::PostedDeferrals',
    icon: 'fas fa-calendar-check',
    permission: DEFERRALS,
    listRoute: ['/erp/posted-deferrals'],
    readOnly: true,
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 140 },
      { field: 'sourceType', labelKey: 'Erp::SourceType', type: 'select', options: enumOptions(deferralSourceTypeOptions, 'DeferralSourceType'), width: 140 },
      { field: 'deferralCode', labelKey: 'Erp::DeferralCode', width: 120 },
      { field: 'accountNo', labelKey: 'Erp::AccountNo', width: 120 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'noOfPeriods', labelKey: 'Erp::NoOfPeriods', type: 'number', width: 110 },
      { field: 'amountToDefer', labelKey: 'Erp::AmountToDefer', type: 'currency', width: 140 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('documentNo', 'Erp::DocumentNo'),
      readonly('lineNo', 'Erp::LineNo'),
      readonly('deferralCode', 'Erp::DeferralCode'),
      readonly('accountNo', 'Erp::AccountNo'),
      readonly('deferralAccountNo', 'Erp::DeferralAccountNo'),
      readonly('postingDate', 'Erp::PostingDate'),
      readonly('startDate', 'Erp::StartDate'),
      readonly('amountToDefer', 'Erp::AmountToDefer'),
      readonly('description', 'Erp::Description'),
    ],
    getList: query => this.postedDeferrals.getList(query),
    get: id => this.postedDeferrals.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.documentNo ?? '', name: dto.deferralCode ?? undefined }),
    newRecord: () => ({}),
    parts: [{ entity: 'postedDeferralLine', lines: dto => of(dto.lines ?? []), totals: ['amount'], editable: () => false }],
  };

  readonly postedDeferralLine: RecordEntity = this.readOnlyLines('postedDeferralLine', 'Erp::PostedDeferralLine', '/erp/posted-deferrals', DEFERRALS, [
    { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
    { field: 'description', labelKey: 'Erp::Description' },
    { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 140 },
  ]);

  readonly cashFlowForecast: RecordEntity<CashFlowForecastDto, CreateUpdateCashFlowForecastDto> = {
    key: 'cashFlowForecast',
    titleKey: 'Erp::CashFlowForecast',
    pluralKey: 'Erp::CashFlowForecasts',
    icon: 'fas fa-money-bill-trend-up',
    permission: CASH_FLOW,
    listRoute: ['/erp/cash-flow-forecasts'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 130 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'glBudgetName', labelKey: 'Erp::GLBudgetName', width: 130 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'sources', labelKey: 'Erp::CashFlowSources' },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', required: true, maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'considerLiquidFunds', labelKey: 'Erp::ConsiderLiquidFunds', type: 'checkbox', section: 'sources' },
      { field: 'considerReceivables', labelKey: 'Erp::ConsiderReceivables', type: 'checkbox', section: 'sources' },
      { field: 'considerPayables', labelKey: 'Erp::ConsiderPayables', type: 'checkbox', section: 'sources' },
      { field: 'considerSalesOrders', labelKey: 'Erp::ConsiderSalesOrders', type: 'checkbox', section: 'sources' },
      { field: 'considerPurchaseOrders', labelKey: 'Erp::ConsiderPurchaseOrders', type: 'checkbox', section: 'sources' },
      { field: 'considerManual', labelKey: 'Erp::ConsiderManual', type: 'checkbox', section: 'sources' },
      { field: 'manualPaymentsTo', labelKey: 'Erp::ManualPaymentsTo', type: 'date', section: 'sources' },
      { field: 'glBudgetName', labelKey: 'Erp::GLBudgetName', type: 'text', maxLength: 10, section: 'sources' },
      { field: 'glBudgetFrom', labelKey: 'Erp::GLBudgetFrom', type: 'date', section: 'sources' },
      { field: 'glBudgetTo', labelKey: 'Erp::GLBudgetTo', type: 'date', section: 'sources' },
    ],
    getList: query => this.forecasts.getList(query),
    get: id => this.forecasts.get(id),
    create: input => this.forecasts.create(input),
    update: (id, input) => this.forecasts.update(id, input),
    delete: id => this.forecasts.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: term => ({
      no: (term ?? '').toUpperCase(),
      considerLiquidFunds: true,
      considerReceivables: true,
      considerPayables: true,
      considerSalesOrders: true,
      considerPurchaseOrders: true,
      considerManual: true,
    }),
    actions: [
      {
        key: 'suggest',
        labelKey: 'Erp::SuggestWorksheetLines',
        icon: 'fas fa-wand-magic-sparkles',
        permission: `${CASH_FLOW}.Update`,
        run: dto => {
          const from = today();
          const to = new Date(new Date().setFullYear(new Date().getFullYear() + 1)).toISOString().substring(0, 10);
          return this.forecasts.suggestWorksheetLines(dto.id!, { fromDate: from, toDate: to });
        },
      },
      {
        key: 'register',
        labelKey: 'Erp::Register',
        icon: 'fas fa-check-double',
        permission: `${CASH_FLOW}.Register`,
        confirmKey: 'Erp::RegisterCashFlowConfirmation',
        run: dto => this.forecasts.register(dto.id!),
      },
      {
        key: 'byPeriod',
        labelKey: 'Erp::CashFlowByPeriod',
        icon: 'fas fa-table-columns',
        run: dto => {
          void this.router.navigate(['/erp/finance/cash-flow-by-period', dto.id]);
          return EMPTY;
        },
      },
    ],
    parts: [
      {
        entity: 'cashFlowWorksheetLine',
        lines: dto => this.worksheetLines.getList({ cashFlowForecastNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ cashFlowForecastNo: dto.no, cashFlowDate: today(), sourceType: CashFlowSourceType.Manual }),
        columns: ['cashFlowDate', 'sourceType', 'sourceNo', 'documentNo', 'description', 'amount'],
        totals: ['amount'],
      },
    ],
  };

  readonly cashFlowWorksheetLine: RecordEntity<CashFlowWorksheetLineDto, CreateUpdateCashFlowWorksheetLineDto> = {
    key: 'cashFlowWorksheetLine',
    titleKey: 'Erp::CashFlowWorksheetLine',
    pluralKey: 'Erp::CashFlowWorksheet',
    icon: 'fas fa-list',
    permission: CASH_FLOW,
    listRoute: ['/erp/cash-flow-worksheet'],
    columns: [
      { field: 'cashFlowForecastNo', labelKey: 'Erp::CashFlowForecastNo', width: 130 },
      { field: 'cashFlowDate', labelKey: 'Erp::CashFlowDate', type: 'date', width: 120 },
      { field: 'sourceType', labelKey: 'Erp::SourceType', type: 'select', options: enumOptions(cashFlowSourceTypeOptions, 'CashFlowSourceType'), width: 140 },
      { field: 'sourceNo', labelKey: 'Erp::SourceNo', width: 110 },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 130 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'cashFlowForecastNo', labelKey: 'Erp::CashFlowForecastNo', type: 'lookup', lookupEntity: 'cashFlowForecast', required: true, createOnly: true },
      { field: 'cashFlowDate', labelKey: 'Erp::CashFlowDate', type: 'date', required: true },
      { field: 'sourceType', labelKey: 'Erp::SourceType', type: 'select', options: enumOptions(cashFlowSourceTypeOptions, 'CashFlowSourceType') },
      { field: 'sourceNo', labelKey: 'Erp::SourceNo', type: 'text', maxLength: 20 },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', type: 'text', maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
    ],
    getList: query => this.worksheetLines.getList(query),
    get: id => this.worksheetLines.get(id),
    create: input => this.worksheetLines.create(input),
    update: (id, input) => this.worksheetLines.update(id, input),
    delete: id => this.worksheetLines.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.documentNo ?? String(dto.lineNo), name: dto.description ?? undefined }),
    newRecord: () => ({ cashFlowDate: today(), sourceType: CashFlowSourceType.Manual, amount: 0 }),
  };

  readonly cashFlowManualEntry = codeTableEntity<CashFlowManualEntryDto, CreateUpdateCashFlowManualEntryDto>(this.manualEntries, {
    key: 'cashFlowManualEntry',
    titleKey: 'Erp::CashFlowManualEntry',
    pluralKey: 'Erp::CashFlowManualEntries',
    icon: 'fas fa-hand-holding-dollar',
    permission: CASH_FLOW,
    route: '/erp/cash-flow-manual-entries',
    columns: [
      { field: 'kind', labelKey: 'Erp::Kind', type: 'select', options: enumOptions(cashFlowManualKindOptions, 'CashFlowManualKind'), width: 110 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 130 },
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', width: 120 },
      { field: 'recurringFrequency', labelKey: 'Erp::RecurringFrequency', width: 130 },
      { field: 'endingDate', labelKey: 'Erp::EndingDate', type: 'date', width: 120 },
    ],
    fields: [
      { field: 'kind', labelKey: 'Erp::Kind', type: 'select', options: enumOptions(cashFlowManualKindOptions, 'CashFlowManualKind') },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', min: 0 },
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', required: true },
      { field: 'recurringFrequency', labelKey: 'Erp::RecurringFrequency', type: 'text', maxLength: 32 },
      { field: 'endingDate', labelKey: 'Erp::EndingDate', type: 'date' },
    ],
    defaults: { kind: CashFlowManualKind.Expense, amount: 0, startingDate: today() },
    quickCreate: false,
  });

  readonly cashFlowForecastEntry: RecordEntity<CashFlowForecastEntryDto, never> = {
    key: 'cashFlowForecastEntry',
    titleKey: 'Erp::CashFlowForecastEntry',
    pluralKey: 'Erp::CashFlowForecastEntries',
    icon: 'fas fa-chart-line',
    permission: CASH_FLOW,
    listRoute: ['/erp/cash-flow-forecast-entries'],
    readOnly: true,
    columns: [
      { field: 'entryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 90 },
      { field: 'cashFlowForecastNo', labelKey: 'Erp::CashFlowForecastNo', width: 130 },
      { field: 'cashFlowDate', labelKey: 'Erp::CashFlowDate', type: 'date', width: 120 },
      { field: 'sourceType', labelKey: 'Erp::SourceType', type: 'select', options: enumOptions(cashFlowSourceTypeOptions, 'CashFlowSourceType'), width: 140 },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 130 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('cashFlowForecastNo', 'Erp::CashFlowForecastNo'), readonly('cashFlowDate', 'Erp::CashFlowDate'), readonly('description', 'Erp::Description'), readonly('amount', 'Erp::Amount')],
    getList: query => this.forecastEntries.getList(query),
    get: id => this.forecastEntries.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.entryNo), name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  constructor() {
    // Reminder terms carry their levels as lines.
    this.reminderTerms.parts = [
      {
        entity: 'reminderLevel',
        lines: dto =>
          this.reminderLevels.getList({ reminderTermsCode: dto.code, maxResultCount: 100, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ reminderTermsCode: dto.code }),
        columns: ['no', 'gracePeriod', 'dueDateCalculation', 'additionalFeeLcy', 'calculateInterest'],
      },
    ];
  }

  get all(): RecordEntity[] {
    return [
      this.reminderTerms,
      this.reminderLevel,
      this.financeChargeTerms,
      this.reminder,
      this.reminderLine,
      this.financeChargeMemo,
      this.financeChargeMemoLine,
      this.deferralTemplate,
      this.postedDeferral,
      this.postedDeferralLine,
      this.cashFlowForecast,
      this.cashFlowWorksheetLine,
      this.cashFlowManualEntry,
      this.cashFlowForecastEntry,
    ];
  }

  /** A reminder or finance charge memo: created by Suggest, its dates changed, then issued. */
  private collectionEntity(
    key: string,
    titleKey: string,
    pluralKey: string,
    icon: string,
    permission: string,
    route: string,
    service: CollectionApi,
    lineEntity: string,
    termsEntity: string,
  ): RecordEntity<CollectionDocumentDto, UpdateCollectionDocumentDto> {
    return {
      key,
      titleKey,
      pluralKey,
      icon,
      permission,
      listRoute: [route],
      columns: [
        { field: 'no', labelKey: 'Erp::No', width: 140 },
        { field: 'customerNo', labelKey: 'Erp::CustomerNo', width: 120 },
        { field: 'customerName', labelKey: 'Erp::CustomerName' },
        { field: 'documentDate', labelKey: 'Erp::DocumentDate', type: 'date', width: 120 },
        { field: 'reminderLevel', labelKey: 'Erp::ReminderLevel', type: 'number', width: 120, filterable: false },
        { field: 'totalAmount', labelKey: 'Erp::TotalAmount', type: 'currency', width: 130, sortable: false, filterable: false },
        { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(collectionDocumentStatusOptions, 'CollectionDocumentStatus'), width: 110 },
        { field: 'issuedNo', labelKey: 'Erp::IssuedNo', width: 140 },
      ],
      sections: [{ key: 'general', labelKey: 'Erp::General' }],
      fields: [
        readonly('no', 'Erp::No'),
        readonly('customerNo', 'Erp::CustomerNo'),
        readonly('customerName', 'Erp::CustomerName'),
        { ...codeField('termsCode', 'Erp::Terms', termsEntity), type: 'readonly' },
        { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true },
        { field: 'documentDate', labelKey: 'Erp::DocumentDate', type: 'date', required: true },
        { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date' },
        readonly('issuedNo', 'Erp::IssuedNo'),
      ],
      getList: query => service.getList(query) as never,
      get: id => service.get(id),
      create: () => EMPTY,
      update: (id, input) => service.update(id, input),
      delete: id => service.delete(id),
      toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.customerName ?? undefined }),
      newRecord: () => ({}),
      facts: dto => [
        { labelKey: 'Erp::Status', value: CollectionDocumentStatus[dto.status] },
        { labelKey: 'Erp::RemainingAmount', value: dto.remainingAmount, type: 'currency' },
        { labelKey: 'Erp::TotalAmount', value: dto.totalAmount, type: 'currency' },
      ],
      listActions: [
        {
          key: 'suggest',
          labelKey: key === 'reminder' ? 'Erp::CreateReminders' : 'Erp::CreateFinanceChargeMemos',
          icon: 'fas fa-wand-magic-sparkles',
          permission: `${permission}.Create`,
          run: () => service.suggest({ documentDate: today() }),
        },
      ],
      actions: [
        {
          key: 'issue',
          labelKey: 'Erp::Issue',
          icon: 'fas fa-paper-plane',
          permission: `${permission}.Issue`,
          confirmKey: 'Erp::IssueConfirmation',
          visible: dto => dto.status === CollectionDocumentStatus.Open,
          run: dto => service.issue(dto.id!, {}),
        },
      ],
      parts: [{ entity: lineEntity, lines: dto => of(dto.lines ?? []), totals: ['amount'], editable: () => false }],
    };
  }

  private collectionLineEntity(key: string, route: string, permission: string): RecordEntity {
    return this.readOnlyLines(key, 'Erp::Line', route, permission, [
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(collectionLineTypeOptions, 'CollectionLineType'), width: 130 },
      { field: 'documentType', labelKey: 'Erp::DocumentType', width: 110 },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date', width: 110 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'remainingAmount', labelKey: 'Erp::RemainingAmount', type: 'currency', width: 130 },
      { field: 'overdueDays', labelKey: 'Erp::OverdueDays', type: 'number', width: 110 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 120 },
    ]);
  }

  private readOnlyLines(key: string, titleKey: string, route: string, permission: string, columns: RecordEntity['columns']): RecordEntity {
    return {
      key,
      titleKey,
      pluralKey: 'Erp::Lines',
      icon: 'fas fa-list',
      permission,
      listRoute: [route],
      readOnly: true,
      columns,
      sections: [{ key: 'general', labelKey: 'Erp::General' }],
      fields: columns.map(c => readonly(c.field, c.labelKey)),
      getList: () => of({ items: [], totalCount: 0 }),
      get: () => EMPTY,
      create: () => EMPTY,
      update: () => EMPTY,
      delete: () => EMPTY,
      toItem: dto => ({ id: dto.id, code: dto.documentNo ?? String(dto.lineNo ?? ''), name: dto.description ?? undefined }),
      newRecord: () => ({}),
    };
  }
}
