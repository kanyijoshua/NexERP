import { Injectable, inject } from '@angular/core';
import {
  BankAccountDto,
  BankAccountLedgerEntryService,
  BankAccountPostingGroupService,
  BankAccountService,
  CreateUpdateBankAccountDto,
  PaymentMethodService,
} from '@proxy/cash-management';
import {
  CreateUpdateCurrencyExchangeRateDto,
  CreateUpdateVatPostingSetupDto,
  CurrencyExchangeRateDto,
  CurrencyExchangeRateService,
  CurrencyService,
  GenJournalAccountType,
  PaymentTermsService,
  VatBusinessPostingGroupService,
  VatCalculationType,
  VatPostingSetupDto,
  VatPostingSetupService,
  VatProductPostingGroupService,
  vatCalculationTypeOptions,
} from '@proxy/finance';
import {
  CauseOfAbsenceService,
  CreateUpdateEmployeeAbsenceDto,
  CreateUpdateEmployeeDto,
  EmployeeAbsenceDto,
  EmployeeAbsenceService,
  EmployeeDto,
  EmployeePostingGroupService,
  EmployeeService,
  EmployeeStatus,
  EmploymentContractService,
  GroundsForTerminationService,
  HumanResourceUnitOfMeasureService,
  QualificationService,
  UnionService,
  employeeStatusOptions,
  EmployeeLedgerEntryService,
} from '@proxy/human-resources';
import { LocationService } from '@proxy/inventory';
import { SalespersonPurchaserService } from '@proxy/sales';
import { forkJoin, map } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { accountField, blockActions, codeField, codeTableEntity, enumOptions, postingGroupEntity } from './entity-helpers';

/** Only the two account types a payment method balances against. */
const BAL_ACCOUNT_TYPE_OPTIONS = [
  { value: GenJournalAccountType.GLAccount, label: 'Erp::Enum:GenJournalAccountType.GLAccount' },
  { value: GenJournalAccountType.BankAccount, label: 'Erp::Enum:GenJournalAccountType.BankAccount' },
];

/** A number series fills a blank number (BC InitSeries), so the number is optional there. */
const SERIES_NO_FIELD: RecordField = {
  field: 'no',
  labelKey: 'Erp::No',
  type: 'text',
  maxLength: 20,
  placeholderKey: 'Erp::NextFromSeries',
};

/**
 * The setup tables behind tax, cash management, finance, inventory, sales and human resources:
 * posting groups, payment terms, currencies, bank accounts, employees and the HR code tables.
 * They are joined into `MasterDataEntities.all`, so every lookup can find them.
 */
@Injectable({ providedIn: 'root' })
export class SetupEntities {
  private readonly vatBusGroups = inject(VatBusinessPostingGroupService);
  private readonly vatProdGroups = inject(VatProductPostingGroupService);
  private readonly vatPostingSetups = inject(VatPostingSetupService);
  private readonly paymentTermsService = inject(PaymentTermsService);
  private readonly currencies = inject(CurrencyService);
  private readonly exchangeRates = inject(CurrencyExchangeRateService);
  private readonly bankPostingGroups = inject(BankAccountPostingGroupService);
  private readonly bankAccounts = inject(BankAccountService);
  private readonly bankLedgerEntries = inject(BankAccountLedgerEntryService);
  private readonly paymentMethods = inject(PaymentMethodService);
  private readonly locations = inject(LocationService);
  private readonly salespeople = inject(SalespersonPurchaserService);
  private readonly hrUnits = inject(HumanResourceUnitOfMeasureService);
  private readonly employeePostingGroups = inject(EmployeePostingGroupService);
  private readonly causes = inject(CauseOfAbsenceService);
  private readonly qualifications = inject(QualificationService);
  private readonly unions = inject(UnionService);
  private readonly contracts = inject(EmploymentContractService);
  private readonly terminationGrounds = inject(GroundsForTerminationService);
  private readonly employees = inject(EmployeeService);
  private readonly absences = inject(EmployeeAbsenceService);
  private readonly employeeLedgerEntries = inject(EmployeeLedgerEntryService);

  // ---------------------------------------------------------------- Tax

  readonly vatBusPostingGroup = postingGroupEntity(
    'vatBusPostingGroup',
    'Erp::VatBusPostingGroup',
    'Erp::VatBusPostingGroups',
    'fas fa-user-shield',
    '/erp/vat-bus-posting-groups',
    this.vatBusGroups,
  );

  readonly vatProdPostingGroup = postingGroupEntity(
    'vatProdPostingGroup',
    'Erp::VatProdPostingGroup',
    'Erp::VatProdPostingGroups',
    'fas fa-percent',
    '/erp/vat-prod-posting-groups',
    this.vatProdGroups,
  );

  /** BC VAT Posting Setup: the rate and accounts per business and product VAT group. */
  readonly vatPostingSetup: RecordEntity<VatPostingSetupDto, CreateUpdateVatPostingSetupDto> = {
    key: 'vatPostingSetup',
    titleKey: 'Erp::VatPostingSetup',
    pluralKey: 'Erp::VatPostingSetup',
    icon: 'fas fa-receipt',
    permission: 'Erp.PostingSetup',
    listRoute: ['/erp/vat-posting-setup'],
    columns: [
      { field: 'vatBusPostingGroup', labelKey: 'Erp::VatBusPostingGroup', width: 160 },
      { field: 'vatProdPostingGroup', labelKey: 'Erp::VatProdPostingGroup', width: 160 },
      { field: 'vatIdentifier', labelKey: 'Erp::VatIdentifier', width: 120 },
      { field: 'vatPercent', labelKey: 'Erp::VatPercent', type: 'number', width: 90 },
      {
        field: 'vatCalculationType',
        labelKey: 'Erp::VatCalculationType',
        type: 'select',
        options: enumOptions(vatCalculationTypeOptions, 'VatCalculationType'),
      },
      { field: 'salesVatAccountNo', labelKey: 'Erp::SalesVatAccount', sortable: false },
      { field: 'purchaseVatAccountNo', labelKey: 'Erp::PurchaseVatAccount', sortable: false },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'posting', labelKey: 'Erp::Posting' },
    ],
    fields: [
      codeField('vatBusPostingGroup', 'Erp::VatBusPostingGroup', 'vatBusPostingGroup'),
      codeField('vatProdPostingGroup', 'Erp::VatProdPostingGroup', 'vatProdPostingGroup'),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'vatIdentifier', labelKey: 'Erp::VatIdentifier', type: 'text', maxLength: 20 },
      {
        field: 'vatCalculationType',
        labelKey: 'Erp::VatCalculationType',
        type: 'select',
        required: true,
        options: enumOptions(vatCalculationTypeOptions, 'VatCalculationType'),
      },
      { field: 'vatPercent', labelKey: 'Erp::VatPercent', type: 'number', min: 0 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
      accountField('salesVatAccountNo', 'Erp::SalesVatAccount', 'posting'),
      accountField('purchaseVatAccountNo', 'Erp::PurchaseVatAccount', 'posting'),
      accountField('reverseChrgVatAccountNo', 'Erp::ReverseChrgVatAccount', 'posting'),
    ],
    getList: query => this.vatPostingSetups.getList(query),
    get: id => this.vatPostingSetups.get(id),
    create: input => this.vatPostingSetups.create(input),
    update: (id, input) => this.vatPostingSetups.update(id, input),
    delete: id => this.vatPostingSetups.delete(id),
    toItem: dto => ({
      id: dto.id,
      code: `${dto.vatBusPostingGroup ?? ''} / ${dto.vatProdPostingGroup ?? ''}`.trim(),
      name: dto.vatIdentifier ?? undefined,
    }),
    newRecord: () => ({ vatCalculationType: VatCalculationType.NormalVat, vatPercent: 0, blocked: false }),
  };

  // ---------------------------------------------------------------- Finance

  readonly paymentTerms = codeTableEntity(this.paymentTermsService, {
    key: 'paymentTerms',
    titleKey: 'Erp::PaymentTerms',
    pluralKey: 'Erp::Menu:PaymentTerms',
    icon: 'fas fa-calendar-day',
    permission: 'Erp.FinanceSetup',
    route: '/erp/payment-terms',
    codeLength: 10,
    columns: [
      { field: 'dueDateCalculation', labelKey: 'Erp::DueDateCalculation', width: 150 },
      { field: 'discountDateCalculation', labelKey: 'Erp::DiscountDateCalculation', width: 160 },
      { field: 'discountPercent', labelKey: 'Erp::DiscountPercent', type: 'number', width: 110 },
    ],
    fields: [
      { field: 'dueDateCalculation', labelKey: 'Erp::DueDateCalculation', type: 'text', maxLength: 32, helpKey: 'Erp::DateFormulaHelp' },
      { field: 'discountDateCalculation', labelKey: 'Erp::DiscountDateCalculation', type: 'text', maxLength: 32 },
      { field: 'discountPercent', labelKey: 'Erp::DiscountPercent', type: 'number', min: 0 },
    ],
    defaults: { discountPercent: 0 },
  });

  readonly currency = codeTableEntity(this.currencies, {
    key: 'currency',
    titleKey: 'Erp::Currency',
    pluralKey: 'Erp::Currencies',
    icon: 'fas fa-money-bill-wave',
    permission: 'Erp.FinanceSetup',
    route: '/erp/currencies',
    codeLength: 10,
    columns: [
      { field: 'symbol', labelKey: 'Erp::Symbol', width: 90 },
      { field: 'amountRoundingPrecision', labelKey: 'Erp::AmountRoundingPrecisionShort', type: 'number', width: 170 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'posting', labelKey: 'Erp::Posting', collapsed: true },
    ],
    fields: [
      { field: 'symbol', labelKey: 'Erp::Symbol', type: 'text', maxLength: 10 },
      { field: 'amountRoundingPrecision', labelKey: 'Erp::AmountRoundingPrecisionShort', type: 'number', min: 0 },
      accountField('realizedGainsAccountNo', 'Erp::RealizedGainsAccount', 'posting'),
      accountField('realizedLossesAccountNo', 'Erp::RealizedLossesAccount', 'posting'),
      accountField('unrealizedGainsAccountNo', 'Erp::UnrealizedGainsAccount', 'posting'),
      accountField('unrealizedLossesAccountNo', 'Erp::UnrealizedLossesAccount', 'posting'),
    ],
    defaults: { amountRoundingPrecision: 0.01 },
  });

  /** BC Currency Exchange Rates: from a starting date, what an amount of the currency is in LCY. */
  readonly currencyExchangeRate: RecordEntity<CurrencyExchangeRateDto, CreateUpdateCurrencyExchangeRateDto> = {
    key: 'currencyExchangeRate',
    titleKey: 'Erp::CurrencyExchangeRate',
    pluralKey: 'Erp::CurrencyExchangeRates',
    icon: 'fas fa-right-left',
    permission: 'Erp.FinanceSetup',
    listRoute: ['/erp/currency-exchange-rates'],
    columns: [
      { field: 'currencyCode', labelKey: 'Erp::CurrencyCode', width: 130 },
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', width: 140 },
      { field: 'exchangeRateAmount', labelKey: 'Erp::ExchangeRateAmount', type: 'number', width: 170 },
      { field: 'relationalExchangeRateAmount', labelKey: 'Erp::RelationalExchangeRateAmount', type: 'number', width: 230 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('currencyCode', 'Erp::CurrencyCode', 'currency', undefined, { required: true }),
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', required: true },
      { field: 'exchangeRateAmount', labelKey: 'Erp::ExchangeRateAmount', type: 'number', required: true, min: 0 },
      {
        field: 'relationalExchangeRateAmount',
        labelKey: 'Erp::RelationalExchangeRateAmount',
        type: 'number',
        required: true,
        min: 0,
        helpKey: 'Erp::ExchangeRateHelp',
      },
    ],
    getList: query => this.exchangeRates.getList(query),
    get: id => this.exchangeRates.get(id),
    create: input => this.exchangeRates.create(input),
    update: (id, input) => this.exchangeRates.update(id, input),
    delete: id => this.exchangeRates.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.currencyCode ?? ''} ${dto.startingDate?.substring(0, 10) ?? ''}`.trim() }),
    newRecord: () => ({ exchangeRateAmount: 1, startingDate: new Date().toISOString().substring(0, 10) }),
  };

  // ---------------------------------------------------------------- Cash management

  readonly bankAccountPostingGroup = postingGroupEntity(
    'bankAccountPostingGroup',
    'Erp::BankAccountPostingGroup',
    'Erp::BankAccountPostingGroups',
    'fas fa-vault',
    '/erp/bank-account-posting-groups',
    this.bankPostingGroups,
    { field: 'glAccountNo', labelKey: 'Erp::GLAccountNo' },
  );

  readonly bankAccount: RecordEntity<BankAccountDto, CreateUpdateBankAccountDto> = {
    key: 'bankAccount',
    titleKey: 'Erp::BankAccount',
    pluralKey: 'Erp::BankAccounts',
    icon: 'fas fa-building-columns',
    permission: 'Erp.BankAccounts',
    listRoute: ['/erp/bank-accounts'],
    chatterEntityType: 'BankAccount',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 140 },
      { field: 'name', labelKey: 'Erp::Name', width: 240 },
      { field: 'bankAccountNo', labelKey: 'Erp::BankAccountNo', width: 150 },
      { field: 'currencyCode', labelKey: 'Erp::CurrencyCode', width: 100 },
      { field: 'balance', labelKey: 'Erp::Balance', type: 'currency', sortable: false, width: 130 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'communication', labelKey: 'Erp::Communication', collapsed: true },
      { key: 'posting', labelKey: 'Erp::Posting' },
    ],
    fields: [
      SERIES_NO_FIELD,
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 100 },
      { field: 'bankAccountNo', labelKey: 'Erp::BankAccountNo', type: 'text', maxLength: 30 },
      { field: 'bankBranchNo', labelKey: 'Erp::BankBranchNo', type: 'text', maxLength: 20, cardOnly: true },
      { field: 'iban', labelKey: 'Erp::Iban', type: 'text', maxLength: 50, cardOnly: true },
      { field: 'swiftCode', labelKey: 'Erp::SwiftCode', type: 'text', maxLength: 20, cardOnly: true },
      { field: 'balance', labelKey: 'Erp::Balance', type: 'readonly' },
      { field: 'balanceLcy', labelKey: 'Erp::BalanceLcy', type: 'readonly', cardOnly: true },
      { field: 'address', labelKey: 'Erp::Address', type: 'text', section: 'communication', maxLength: 100, wide: true, cardOnly: true },
      { field: 'city', labelKey: 'Erp::City', type: 'text', section: 'communication', maxLength: 50, cardOnly: true },
      { field: 'contact', labelKey: 'Erp::Contact', type: 'text', section: 'communication', maxLength: 100, cardOnly: true },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', section: 'communication', maxLength: 30, cardOnly: true },
      codeField('bankAccPostingGroup', 'Erp::BankAccPostingGroup', 'bankAccountPostingGroup', 'posting', { required: true }),
      codeField('currencyCode', 'Erp::CurrencyCode', 'currency', 'posting', { cardOnly: true }),
    ],
    getList: query => this.bankAccounts.getList(query),
    get: id => this.bankAccounts.get(id),
    create: input => this.bankAccounts.create(input),
    update: (id, input) => this.bankAccounts.update(id, input),
    delete: id => this.bankAccounts.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.name ?? undefined }),
    newRecord: term => ({ name: term ?? '' }),
    facts: dto => [
      { labelKey: 'Erp::Balance', value: dto.balance, type: 'currency' },
      { labelKey: 'Erp::BalanceLcy', value: dto.balanceLcy, type: 'currency' },
      { labelKey: 'Erp::Blocked', value: dto.blocked, type: 'boolean', warnWhenTrue: true },
    ],
    actions: blockActions<BankAccountDto>('Erp.BankAccounts', this.bankAccounts),
    related: dto =>
      this.bankLedgerEntries.getList({ bankAccountId: dto.id, maxResultCount: 1, skipCount: 0 }).pipe(
        map(result => [
          {
            labelKey: 'Erp::BankAccountLedgerEntries',
            icon: 'fas fa-list-ul',
            count: result.totalCount ?? 0,
            routerLink: ['/erp/finance/bank-ledger-entries'],
            queryParams: { bankAccountId: dto.id, bankAccountNo: dto.no },
            permission: 'Erp.BankAccounts',
          },
        ]),
      ),
  };

  readonly paymentMethod = codeTableEntity(this.paymentMethods, {
    key: 'paymentMethod',
    titleKey: 'Erp::PaymentMethod',
    pluralKey: 'Erp::PaymentMethods',
    icon: 'fas fa-credit-card',
    permission: 'Erp.FinanceSetup',
    route: '/erp/payment-methods',
    columns: [
      { field: 'balAccountType', labelKey: 'Erp::BalAccountType', type: 'select', options: BAL_ACCOUNT_TYPE_OPTIONS, width: 140 },
      { field: 'balAccountNo', labelKey: 'Erp::BalAccountNo', width: 160 },
    ],
    fields: [
      { field: 'balAccountType', labelKey: 'Erp::BalAccountType', type: 'select', options: BAL_ACCOUNT_TYPE_OPTIONS },
      { field: 'balAccountNo', labelKey: 'Erp::BalAccountNo', type: 'text', maxLength: 20 },
    ],
  });

  // ---------------------------------------------------------------- Inventory and sales

  readonly location = codeTableEntity(this.locations, {
    key: 'location',
    titleKey: 'Erp::Location',
    pluralKey: 'Erp::Locations',
    icon: 'fas fa-warehouse',
    permission: 'Erp.Locations',
    route: '/erp/locations',
    codeLength: 10,
    descriptionKey: 'Erp::Name',
    columns: [
      { field: 'city', labelKey: 'Erp::City' },
      { field: 'contact', labelKey: 'Erp::Contact', sortable: false },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'contact', labelKey: 'Erp::AddressAndContact' },
    ],
    fields: [
      { field: 'address', labelKey: 'Erp::Address', type: 'text', section: 'contact', maxLength: 100, wide: true, cardOnly: true },
      { field: 'city', labelKey: 'Erp::City', type: 'text', section: 'contact', maxLength: 50 },
      { field: 'postCode', labelKey: 'Erp::PostCode', type: 'text', section: 'contact', maxLength: 20, cardOnly: true },
      { field: 'countryRegionCode', labelKey: 'Erp::CountryRegionCode', type: 'text', section: 'contact', maxLength: 10, cardOnly: true },
      { field: 'contact', labelKey: 'Erp::Contact', type: 'text', section: 'contact', maxLength: 100, cardOnly: true },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', section: 'contact', maxLength: 30, cardOnly: true },
    ],
  });

  readonly salespersonPurchaser = codeTableEntity(this.salespeople, {
    key: 'salespersonPurchaser',
    titleKey: 'Erp::SalespersonPurchaser',
    pluralKey: 'Erp::SalespeoplePurchasers',
    icon: 'fas fa-user-tag',
    permission: 'Erp.SalespeoplePurchasers',
    route: '/erp/salespeople-purchasers',
    descriptionKey: 'Erp::Name',
    columns: [
      { field: 'jobTitle', labelKey: 'Erp::JobTitle' },
      { field: 'email', labelKey: 'Erp::Email', sortable: false },
      { field: 'commissionPercent', labelKey: 'Erp::CommissionPercent', type: 'number', width: 120 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    fields: [
      { field: 'jobTitle', labelKey: 'Erp::JobTitle', type: 'text', maxLength: 50, cardOnly: true },
      { field: 'email', labelKey: 'Erp::Email', type: 'email', maxLength: 80 },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', maxLength: 30, cardOnly: true },
      { field: 'commissionPercent', labelKey: 'Erp::CommissionPercent', type: 'number', min: 0, cardOnly: true },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox', cardOnly: true },
    ],
    defaults: { commissionPercent: 0, blocked: false },
  });

  // ---------------------------------------------------------------- Human resources

  readonly hrUnitOfMeasure = codeTableEntity(this.hrUnits, {
    key: 'hrUnitOfMeasure',
    titleKey: 'Erp::HumanResourceUnitOfMeasure',
    pluralKey: 'Erp::HumanResourceUnitsOfMeasure',
    icon: 'fas fa-hourglass-half',
    permission: 'Erp.HumanResourcesSetup',
    route: '/erp/hr-units-of-measure',
    codeLength: 10,
    columns: [{ field: 'qtyPerUnitOfMeasure', labelKey: 'Erp::QtyPerUnitOfMeasure', type: 'number', width: 170 }],
    fields: [{ field: 'qtyPerUnitOfMeasure', labelKey: 'Erp::QtyPerUnitOfMeasure', type: 'number', min: 0 }],
    defaults: { qtyPerUnitOfMeasure: 1 },
  });

  readonly employeePostingGroup = postingGroupEntity(
    'employeePostingGroup',
    'Erp::EmployeePostingGroup',
    'Erp::EmployeePostingGroups',
    'fas fa-people-arrows',
    '/erp/employee-posting-groups',
    this.employeePostingGroups,
    { field: 'payablesAccountNo', labelKey: 'Erp::PayablesAccount' },
    'Erp.HumanResourcesSetup',
  );

  readonly causeOfAbsence = codeTableEntity(this.causes, {
    key: 'causeOfAbsence',
    titleKey: 'Erp::CauseOfAbsence',
    pluralKey: 'Erp::CausesOfAbsence',
    icon: 'fas fa-umbrella-beach',
    permission: 'Erp.HumanResourcesSetup',
    route: '/erp/causes-of-absence',
    columns: [{ field: 'unitOfMeasureCode', labelKey: 'Erp::UnitOfMeasureCode', width: 160 }],
    fields: [codeField('unitOfMeasureCode', 'Erp::UnitOfMeasureCode', 'hrUnitOfMeasure')],
  });

  readonly qualification = codeTableEntity(this.qualifications, {
    key: 'qualification',
    titleKey: 'Erp::Qualification',
    pluralKey: 'Erp::Qualifications',
    icon: 'fas fa-graduation-cap',
    permission: 'Erp.HumanResourcesSetup',
    route: '/erp/qualifications',
  });

  readonly union = codeTableEntity(this.unions, {
    key: 'union',
    titleKey: 'Erp::Union',
    pluralKey: 'Erp::Unions',
    icon: 'fas fa-handshake',
    permission: 'Erp.HumanResourcesSetup',
    route: '/erp/unions',
    descriptionKey: 'Erp::Name',
  });

  readonly employmentContract = codeTableEntity(this.contracts, {
    key: 'employmentContract',
    titleKey: 'Erp::EmploymentContract',
    pluralKey: 'Erp::EmploymentContracts',
    icon: 'fas fa-file-signature',
    permission: 'Erp.HumanResourcesSetup',
    route: '/erp/employment-contracts',
  });

  readonly groundsForTermination = codeTableEntity(this.terminationGrounds, {
    key: 'groundsForTermination',
    titleKey: 'Erp::GroundsForTermination',
    pluralKey: 'Erp::GroundsForTermination',
    icon: 'fas fa-door-open',
    permission: 'Erp.HumanResourcesSetup',
    route: '/erp/grounds-for-termination',
  });

  readonly employee: RecordEntity<EmployeeDto, CreateUpdateEmployeeDto> = {
    key: 'employee',
    titleKey: 'Erp::Employee',
    pluralKey: 'Erp::Employees',
    icon: 'fas fa-id-badge',
    permission: 'Erp.Employees',
    listRoute: ['/erp/employees'],
    chatterEntityType: 'Employee',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'fullName', labelKey: 'Erp::FullName', width: 220, sortable: false, filterable: false },
      { field: 'jobTitle', labelKey: 'Erp::JobTitle' },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', sortable: false },
      { field: 'employmentDate', labelKey: 'Erp::EmploymentDate', type: 'date', width: 140 },
      {
        field: 'status',
        labelKey: 'Erp::Status',
        type: 'select',
        options: enumOptions(employeeStatusOptions, 'EmployeeStatus'),
        width: 120,
      },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'communication', labelKey: 'Erp::AddressAndContact' },
      { key: 'administration', labelKey: 'Erp::Administration', collapsed: true },
      { key: 'personal', labelKey: 'Erp::Personal', collapsed: true },
      { key: 'payments', labelKey: 'Erp::Payments', collapsed: true },
    ],
    fields: [
      SERIES_NO_FIELD,
      { field: 'firstName', labelKey: 'Erp::FirstName', type: 'text', required: true, maxLength: 50 },
      { field: 'middleName', labelKey: 'Erp::MiddleName', type: 'text', maxLength: 50, cardOnly: true },
      { field: 'lastName', labelKey: 'Erp::LastName', type: 'text', maxLength: 50 },
      { field: 'jobTitle', labelKey: 'Erp::JobTitle', type: 'text', maxLength: 50 },
      { field: 'balance', labelKey: 'Erp::BalanceLcy', type: 'readonly', cardOnly: true },
      { field: 'address', labelKey: 'Erp::Address', type: 'text', section: 'communication', maxLength: 100, wide: true, cardOnly: true },
      { field: 'city', labelKey: 'Erp::City', type: 'text', section: 'communication', maxLength: 50, cardOnly: true },
      { field: 'postCode', labelKey: 'Erp::PostCode', type: 'text', section: 'communication', maxLength: 20, cardOnly: true },
      { field: 'countryRegionCode', labelKey: 'Erp::CountryRegionCode', type: 'text', section: 'communication', maxLength: 10, cardOnly: true },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', section: 'communication', maxLength: 30 },
      { field: 'mobilePhoneNo', labelKey: 'Erp::MobilePhoneNo', type: 'text', section: 'communication', maxLength: 30, cardOnly: true },
      { field: 'email', labelKey: 'Erp::Email', type: 'email', section: 'communication', maxLength: 80, cardOnly: true },
      { field: 'companyEmail', labelKey: 'Erp::CompanyEmail', type: 'email', section: 'communication', maxLength: 80, cardOnly: true },
      { field: 'employmentDate', labelKey: 'Erp::EmploymentDate', type: 'date', section: 'administration', cardOnly: true },
      {
        field: 'status',
        labelKey: 'Erp::Status',
        type: 'select',
        section: 'administration',
        required: true,
        options: enumOptions(employeeStatusOptions, 'EmployeeStatus'),
        cardOnly: true,
      },
      { field: 'inactiveDate', labelKey: 'Erp::InactiveDate', type: 'date', section: 'administration', cardOnly: true },
      { field: 'terminationDate', labelKey: 'Erp::TerminationDate', type: 'date', section: 'administration', cardOnly: true },
      codeField('groundsForTermCode', 'Erp::GroundsForTermCode', 'groundsForTermination', 'administration', { cardOnly: true }),
      codeField('emplymtContractCode', 'Erp::EmplymtContractCode', 'employmentContract', 'administration', { cardOnly: true }),
      codeField('unionCode', 'Erp::UnionCode', 'union', 'administration', { cardOnly: true }),
      { field: 'birthDate', labelKey: 'Erp::BirthDate', type: 'date', section: 'personal', cardOnly: true },
      { field: 'socialSecurityNo', labelKey: 'Erp::SocialSecurityNo', type: 'text', section: 'personal', maxLength: 40, cardOnly: true },
      codeField('employeePostingGroup', 'Erp::EmployeePostingGroup', 'employeePostingGroup', 'payments', { cardOnly: true }),
      { field: 'bankAccountNo', labelKey: 'Erp::BankAccountNo', type: 'text', section: 'payments', maxLength: 30, cardOnly: true },
      { field: 'iban', labelKey: 'Erp::Iban', type: 'text', section: 'payments', maxLength: 50, cardOnly: true },
      codeField('salespersPurchCode', 'Erp::SalespersPurchCode', 'salespersonPurchaser', 'payments', { cardOnly: true }),
    ],
    getList: query => this.employees.getList(query),
    get: id => this.employees.get(id),
    create: input => this.employees.create(input),
    update: (id, input) => this.employees.update(id, input),
    delete: id => this.employees.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.fullName ?? undefined }),
    newRecord: term => ({ firstName: term ?? '', status: EmployeeStatus.Active }),
    quickCreate: term => ({ firstName: term, status: EmployeeStatus.Active }),
    facts: dto => [
      { labelKey: 'Erp::JobTitle', value: dto.jobTitle },
      { labelKey: 'Erp::EmploymentDate', value: dto.employmentDate?.substring(0, 10) },
      // Negative is what the company owes the employee back.
      { labelKey: 'Erp::BalanceLcy', value: dto.balance, type: 'currency' },
    ],
    related: dto =>
      forkJoin({
        absences: this.absences.getList({ employeeId: dto.id, maxResultCount: 1, skipCount: 0 }),
        entries: this.employeeLedgerEntries.getList({ employeeNo: dto.no, maxResultCount: 1, skipCount: 0 }),
      }).pipe(
        map(({ absences, entries }) => [
          {
            labelKey: 'Erp::EmployeeAbsences',
            icon: 'fas fa-umbrella-beach',
            count: absences.totalCount ?? 0,
            routerLink: ['/erp/employee-absences'],
            queryParams: { filter: dto.no },
            permission: 'Erp.Employees',
          },
          {
            labelKey: 'Erp::EmployeeLedgerEntries',
            icon: 'fas fa-receipt',
            count: entries.totalCount ?? 0,
            routerLink: ['/erp/finance/employee-ledger-entries'],
            queryParams: { partyNo: dto.no },
            permission: 'Erp.Employees',
          },
        ]),
      ),
  };

  readonly employeeAbsence: RecordEntity<EmployeeAbsenceDto, CreateUpdateEmployeeAbsenceDto> = {
    key: 'employeeAbsence',
    titleKey: 'Erp::EmployeeAbsence',
    pluralKey: 'Erp::EmployeeAbsences',
    icon: 'fas fa-umbrella-beach',
    permission: 'Erp.Employees',
    listRoute: ['/erp/employee-absences'],
    columns: [
      { field: 'employeeNo', labelKey: 'Erp::EmployeeNo', width: 130 },
      { field: 'fromDate', labelKey: 'Erp::FromDate', type: 'date', width: 130 },
      { field: 'toDate', labelKey: 'Erp::ToDate', type: 'date', width: 130 },
      { field: 'causeOfAbsenceCode', labelKey: 'Erp::CauseOfAbsenceCode', width: 170 },
      { field: 'description', labelKey: 'Erp::Description', width: 220 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'unitOfMeasureCode', labelKey: 'Erp::UnitOfMeasureCode', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('employeeNo', 'Erp::EmployeeNo', 'employee', undefined, { required: true }),
      { field: 'fromDate', labelKey: 'Erp::FromDate', type: 'date', required: true },
      { field: 'toDate', labelKey: 'Erp::ToDate', type: 'date' },
      codeField('causeOfAbsenceCode', 'Erp::CauseOfAbsenceCode', 'causeOfAbsence'),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', min: 0 },
      codeField('unitOfMeasureCode', 'Erp::UnitOfMeasureCode', 'hrUnitOfMeasure'),
    ],
    getList: query => this.absences.getList(query),
    get: id => this.absences.get(id),
    create: input => this.absences.create(input),
    update: (id, input) => this.absences.update(id, input),
    delete: id => this.absences.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.employeeNo ?? ''} ${dto.fromDate?.substring(0, 10) ?? ''}`.trim(), name: dto.description ?? undefined }),
    newRecord: () => ({ fromDate: new Date().toISOString().substring(0, 10), quantity: 0 }),
  };

  get all(): RecordEntity[] {
    return [
      this.vatBusPostingGroup,
      this.vatProdPostingGroup,
      this.vatPostingSetup,
      this.paymentTerms,
      this.currency,
      this.currencyExchangeRate,
      this.bankAccountPostingGroup,
      this.bankAccount,
      this.paymentMethod,
      this.location,
      this.salespersonPurchaser,
      this.hrUnitOfMeasure,
      this.employeePostingGroup,
      this.causeOfAbsence,
      this.qualification,
      this.union,
      this.employmentContract,
      this.groundsForTermination,
      this.employee,
      this.employeeAbsence,
    ];
  }
}
