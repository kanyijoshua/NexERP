import { Injectable, inject } from '@angular/core';
import {
  CreateUpdateCustomerPostingGroupDto,
  CreateUpdateGeneralPostingSetupDto,
  CreateUpdateGLAccountDto,
  CreateUpdateInventoryPostingSetupDto,
  CreateUpdatePostingGroupDto,
  CreateUpdateVendorPostingGroupDto,
  CustomerPostingGroupDto,
  CustomerPostingGroupService,
  GenBusinessPostingGroupService,
  GeneralPostingSetupDto,
  GeneralPostingSetupService,
  GenProductPostingGroupService,
  InventoryPostingGroupService,
  InventoryPostingSetupDto,
  InventoryPostingSetupService,
  PostingGroupDto,
  VendorPostingGroupDto,
  VendorPostingGroupService,
  GLAccountCategory,
  GLAccountDto,
  GlAccountService,
  GLAccountType,
  IncomeBalanceType,
  glAccountCategoryOptions,
  glAccountTypeOptions,
  incomeBalanceTypeOptions,
  CustomerLedgerEntryService,
  VendorLedgerEntryService,
  GeneralPostingType,
  generalPostingTypeOptions,
} from '@proxy/finance';
import {
  CreateUpdateItemCategoryDto,
  CreateUpdateItemDto,
  CreateUpdateUnitOfMeasureDto,
  ItemCategoryDto,
  ItemCategoryService,
  ItemDto,
  ItemService,
  ItemType,
  UnitOfMeasureDto,
  UnitOfMeasureService,
  itemTypeOptions,
} from '@proxy/inventory';
import {
  CreateUpdateVendorDto,
  PurchaseDocumentService,
  PurchaseDocumentType,
  VendorDto,
  VendorService,
} from '@proxy/purchasing';
import {
  CreateUpdateCustomerDto,
  CustomerDto,
  CustomerService,
  SalesDocumentService,
  SalesDocumentType,
} from '@proxy/sales';
import { Observable, forkJoin, map } from 'rxjs';
import { accountField, blockActions, codeField, codeOf, enumOptions, postingGroupEntity } from './entity-helpers';
import { SetupEntities } from './setup-entities';

export { codeOf } from './entity-helpers';
import { RecordEntity, RecordField, SmartButton } from '../erp-shared';

/** The address and contact FastTab shared by customers and vendors (BC "Address & Contact"). */
const ADDRESS_FIELDS: RecordField[] = [
  { field: 'address', labelKey: 'Erp::Address', type: 'text', section: 'contact', maxLength: 100, wide: true, cardOnly: true },
  { field: 'city', labelKey: 'Erp::City', type: 'text', section: 'contact', maxLength: 50 },
  { field: 'postCode', labelKey: 'Erp::PostCode', type: 'text', section: 'contact', maxLength: 20, cardOnly: true },
  { field: 'countryRegionCode', labelKey: 'Erp::CountryRegionCode', type: 'text', section: 'contact', maxLength: 10, cardOnly: true },
  { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', section: 'contact', maxLength: 30 },
  { field: 'email', labelKey: 'Erp::Email', type: 'email', section: 'contact', maxLength: 80 },
];

/** A number series fills a blank number (BC InitSeries), so the number is optional there. */
const SERIES_NO_FIELD: RecordField = {
  field: 'no',
  labelKey: 'Erp::No',
  type: 'text',
  maxLength: 20,
  placeholderKey: 'Erp::NextFromSeries',
};

/** How many documents of a party there are, as an Odoo smart button linking to them. */
function documentCount(
  count$: Observable<{ totalCount?: number }>,
  labelKey: string,
  icon: string,
  route: string,
  partyId: string,
  permission: string,
): Observable<SmartButton[]> {
  return count$.pipe(
    map(result => [
      {
        labelKey,
        icon,
        count: result.totalCount ?? 0,
        routerLink: [route],
        queryParams: { partyId },
        permission,
      },
    ]),
  );
}

/** A party's ledger entries as a smart button, ahead of its other related buttons. */
function withLedger(
  count$: Observable<{ totalCount?: number }>,
  labelKey: string,
  route: string,
  partyNo: string,
  permission: string,
  others$: Observable<SmartButton[]>,
): Observable<SmartButton[]> {
  return forkJoin({ ledger: count$, others: others$ }).pipe(
    map(({ ledger, others }) => [
      {
        labelKey,
        icon: 'fas fa-file-invoice',
        count: ledger.totalCount ?? 0,
        routerLink: [route],
        queryParams: { partyNo },
        permission,
      },
      ...others,
    ]),
  );
}

/**
 * The master-data tables the generic record UI works on: lookups, "Search More...", the card
 * dialog and the list and card pages all read these descriptors. Registered once by `ErpModule`.
 */
@Injectable({ providedIn: 'root' })
export class MasterDataEntities {
  private readonly customers = inject(CustomerService);
  private readonly vendors = inject(VendorService);
  private readonly items = inject(ItemService);
  private readonly glAccounts = inject(GlAccountService);
  private readonly units = inject(UnitOfMeasureService);
  private readonly categories = inject(ItemCategoryService);
  private readonly salesDocuments = inject(SalesDocumentService);
  private readonly purchaseDocuments = inject(PurchaseDocumentService);
  private readonly genBusGroups = inject(GenBusinessPostingGroupService);
  private readonly genProdGroups = inject(GenProductPostingGroupService);
  private readonly customerGroups = inject(CustomerPostingGroupService);
  private readonly vendorGroups = inject(VendorPostingGroupService);
  private readonly inventoryGroups = inject(InventoryPostingGroupService);
  private readonly generalPostingSetups = inject(GeneralPostingSetupService);
  private readonly inventoryPostingSetups = inject(InventoryPostingSetupService);
  private readonly setup = inject(SetupEntities);
  private readonly customerLedgerEntries = inject(CustomerLedgerEntryService);
  private readonly vendorLedgerEntries = inject(VendorLedgerEntryService);

  readonly customer: RecordEntity<CustomerDto, CreateUpdateCustomerDto> = {
    key: 'customer',
    titleKey: 'Erp::Customer',
    pluralKey: 'Erp::Customers',
    icon: 'fas fa-user-tie',
    permission: 'Erp.Customers',
    listRoute: ['/erp/customers'],
    chatterEntityType: 'Customer',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'name', labelKey: 'Erp::Name', width: 220 },
      { field: 'city', labelKey: 'Erp::City' },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', sortable: false },
      { field: 'balance', labelKey: 'Erp::BalanceLcy', type: 'currency', sortable: false, width: 130 },
      { field: 'creditLimit', labelKey: 'Erp::CreditLimit', type: 'currency', width: 130 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'contact', labelKey: 'Erp::AddressAndContact' },
      { key: 'invoicing', labelKey: 'Erp::Invoicing', collapsed: true },
      { key: 'payments', labelKey: 'Erp::Payments', collapsed: true },
    ],
    fields: [
      SERIES_NO_FIELD,
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 100 },
      { field: 'creditLimit', labelKey: 'Erp::CreditLimit', type: 'currency', min: 0, cardOnly: true },
      { field: 'balance', labelKey: 'Erp::BalanceLcy', type: 'readonly' },
      ...ADDRESS_FIELDS,
      { field: 'customerPostingGroup', labelKey: 'Erp::CustomerPostingGroup', type: 'lookup', lookupEntity: 'customerPostingGroup', section: 'invoicing', cardOnly: true },
      { field: 'genBusPostingGroup', labelKey: 'Erp::GenBusPostingGroup', type: 'lookup', lookupEntity: 'genBusPostingGroup', section: 'invoicing', cardOnly: true },
      codeField('vatBusPostingGroup', 'Erp::VatBusPostingGroup', 'vatBusPostingGroup', 'invoicing', { cardOnly: true }),
      codeField('currencyCode', 'Erp::CurrencyCode', 'currency', 'invoicing', { cardOnly: true }),
      codeField('salespersonCode', 'Erp::SalespersonCode', 'salespersonPurchaser', 'general', { cardOnly: true }),
      codeField('paymentTermsCode', 'Erp::PaymentTermsCode', 'paymentTerms', 'payments', { cardOnly: true }),
      codeField('paymentMethodCode', 'Erp::PaymentMethodCode', 'paymentMethod', 'payments', { cardOnly: true }),
    ],
    getList: query => this.customers.getList(query),
    get: id => this.customers.get(id),
    create: input => this.customers.create(input),
    update: (id, input) => this.customers.update(id, input),
    delete: id => this.customers.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.name ?? undefined }),
    newRecord: term => ({ name: term ?? '', creditLimit: 0 }),
    quickCreate: term => ({ name: term, creditLimit: 0 }),
    facts: dto => [
      { labelKey: 'Erp::BalanceLcy', value: dto.balance, type: 'currency' },
      { labelKey: 'Erp::CreditLimit', value: dto.creditLimit, type: 'currency' },
      { labelKey: 'Erp::Blocked', value: dto.blocked, type: 'boolean', warnWhenTrue: true },
    ],
    actions: blockActions<CustomerDto>('Erp.Customers', this.customers),
    related: dto =>
      withLedger(
        this.customerLedgerEntries.getList({ partyNo: dto.no, maxResultCount: 1, skipCount: 0 }),
        'Erp::CustomerLedgerEntries',
        '/erp/finance/customer-ledger-entries',
        dto.no ?? '',
        'Erp.Customers',
      documentCount(
        this.salesDocuments.getList({
          customerId: dto.id,
          documentType: SalesDocumentType.Invoice,
          maxResultCount: 1,
          skipCount: 0,
        }),
        'Erp::SalesInvoices',
        'fas fa-file-invoice-dollar',
        '/erp/sales-invoices',
        dto.id!,
        'Erp.SalesDocuments',
      )),
  };

  readonly vendor: RecordEntity<VendorDto, CreateUpdateVendorDto> = {
    key: 'vendor',
    titleKey: 'Erp::Vendor',
    pluralKey: 'Erp::Vendors',
    icon: 'fas fa-truck-field',
    permission: 'Erp.Vendors',
    listRoute: ['/erp/vendors'],
    chatterEntityType: 'Vendor',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'name', labelKey: 'Erp::Name', width: 220 },
      { field: 'city', labelKey: 'Erp::City' },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', sortable: false },
      { field: 'balance', labelKey: 'Erp::BalanceLcy', type: 'currency', sortable: false, width: 130 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'contact', labelKey: 'Erp::AddressAndContact' },
      { key: 'invoicing', labelKey: 'Erp::Invoicing', collapsed: true },
      { key: 'payments', labelKey: 'Erp::Payments', collapsed: true },
    ],
    fields: [
      SERIES_NO_FIELD,
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 100 },
      { field: 'balance', labelKey: 'Erp::BalanceLcy', type: 'readonly' },
      ...ADDRESS_FIELDS,
      { field: 'vendorPostingGroup', labelKey: 'Erp::VendorPostingGroup', type: 'lookup', lookupEntity: 'vendorPostingGroup', section: 'invoicing', cardOnly: true },
      { field: 'genBusPostingGroup', labelKey: 'Erp::GenBusPostingGroup', type: 'lookup', lookupEntity: 'genBusPostingGroup', section: 'invoicing', cardOnly: true },
      codeField('vatBusPostingGroup', 'Erp::VatBusPostingGroup', 'vatBusPostingGroup', 'invoicing', { cardOnly: true }),
      codeField('currencyCode', 'Erp::CurrencyCode', 'currency', 'invoicing', { cardOnly: true }),
      codeField('purchaserCode', 'Erp::PurchaserCode', 'salespersonPurchaser', 'general', { cardOnly: true }),
      codeField('paymentTermsCode', 'Erp::PaymentTermsCode', 'paymentTerms', 'payments', { cardOnly: true }),
      codeField('paymentMethodCode', 'Erp::PaymentMethodCode', 'paymentMethod', 'payments', { cardOnly: true }),
    ],
    getList: query => this.vendors.getList(query),
    get: id => this.vendors.get(id),
    create: input => this.vendors.create(input),
    update: (id, input) => this.vendors.update(id, input),
    delete: id => this.vendors.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.name ?? undefined }),
    newRecord: term => ({ name: term ?? '' }),
    quickCreate: term => ({ name: term }),
    facts: dto => [
      { labelKey: 'Erp::BalanceLcy', value: dto.balance, type: 'currency' },
      { labelKey: 'Erp::Blocked', value: dto.blocked, type: 'boolean', warnWhenTrue: true },
    ],
    actions: blockActions<VendorDto>('Erp.Vendors', this.vendors),
    related: dto =>
      withLedger(
        this.vendorLedgerEntries.getList({ partyNo: dto.no, maxResultCount: 1, skipCount: 0 }),
        'Erp::VendorLedgerEntries',
        '/erp/finance/vendor-ledger-entries',
        dto.no ?? '',
        'Erp.Vendors',
      documentCount(
        this.purchaseDocuments.getList({
          vendorId: dto.id,
          documentType: PurchaseDocumentType.Invoice,
          maxResultCount: 1,
          skipCount: 0,
        }),
        'Erp::PurchaseInvoices',
        'fas fa-shopping-cart',
        '/erp/purchase-invoices',
        dto.id!,
        'Erp.PurchaseDocuments',
      )),
  };

  readonly item: RecordEntity<ItemDto, CreateUpdateItemDto> = {
    key: 'item',
    titleKey: 'Erp::Item',
    pluralKey: 'Erp::Items',
    icon: 'fas fa-box',
    permission: 'Erp.Items',
    listRoute: ['/erp/items'],
    chatterEntityType: 'Item',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'description', labelKey: 'Erp::Description', width: 240 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(itemTypeOptions, 'ItemType') },
      { field: 'baseUnitOfMeasureCode', labelKey: 'Erp::BaseUnitOfMeasure', width: 110 },
      { field: 'inventory', labelKey: 'Erp::Inventory', type: 'number', sortable: false, width: 110 },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', width: 120 },
      { field: 'unitCost', labelKey: 'Erp::UnitCost', type: 'currency', width: 120 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'prices', labelKey: 'Erp::PricesAndSales' },
      { key: 'costs', labelKey: 'Erp::CostsAndPosting', collapsed: true },
    ],
    fields: [
      SERIES_NO_FIELD,
      { field: 'description', labelKey: 'Erp::Description', type: 'text', required: true, maxLength: 250 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', required: true, options: enumOptions(itemTypeOptions, 'ItemType') },
      { field: 'baseUnitOfMeasureCode', labelKey: 'Erp::BaseUnitOfMeasure', type: 'lookup', required: true, lookupEntity: 'unitOfMeasure' },
      {
        field: 'itemCategoryId',
        labelKey: 'Erp::ItemCategory',
        type: 'lookup',
        lookupEntity: 'itemCategory',
        lookupValueField: 'id',
        lookupDisplayField: 'itemCategoryCode',
        cardOnly: true,
      },
      { field: 'inventory', labelKey: 'Erp::Inventory', type: 'readonly' },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', section: 'prices', min: 0 },
      { field: 'unitCost', labelKey: 'Erp::UnitCost', type: 'currency', section: 'costs', min: 0, cardOnly: true },
      { field: 'genProdPostingGroup', labelKey: 'Erp::GenProdPostingGroup', type: 'lookup', lookupEntity: 'genProdPostingGroup', section: 'costs', cardOnly: true },
      { field: 'inventoryPostingGroup', labelKey: 'Erp::InventoryPostingGroup', type: 'lookup', lookupEntity: 'inventoryPostingGroup', section: 'costs', cardOnly: true },
      codeField('vatProdPostingGroup', 'Erp::VatProdPostingGroup', 'vatProdPostingGroup', 'costs', { cardOnly: true }),
    ],
    getList: query => this.items.getList(query),
    get: id => this.items.get(id),
    create: input => this.items.create(input),
    update: (id, input) => this.items.update(id, input),
    delete: id => this.items.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    // An item needs a number and a unit of measure, so there is no one-click "Create 'term'".
    newRecord: term => ({ description: term ?? '', type: ItemType.Inventory, unitPrice: 0, unitCost: 0 }),
    facts: dto => [
      { labelKey: 'Erp::Inventory', value: dto.inventory, type: 'number' },
      { labelKey: 'Erp::UnitPrice', value: dto.unitPrice, type: 'currency' },
      { labelKey: 'Erp::UnitCost', value: dto.unitCost, type: 'currency' },
      { labelKey: 'Erp::Blocked', value: dto.blocked, type: 'boolean', warnWhenTrue: true },
    ],
    actions: blockActions<ItemDto>('Erp.Items', this.items),
  };

  readonly glAccount: RecordEntity<GLAccountDto, CreateUpdateGLAccountDto> = {
    key: 'glAccount',
    titleKey: 'Erp::GLAccount',
    pluralKey: 'Erp::ChartOfAccounts',
    icon: 'fas fa-list-ol',
    permission: 'Erp.GLAccounts',
    listRoute: ['/erp/chart-of-accounts'],
    chatterEntityType: 'GLAccount',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 100 },
      { field: 'name', labelKey: 'Erp::Name', width: 240 },
      { field: 'incomeBalance', labelKey: 'Erp::IncomeBalance', type: 'select', options: enumOptions(incomeBalanceTypeOptions, 'IncomeBalanceType') },
      { field: 'accountCategory', labelKey: 'Erp::AccountCategory', type: 'select', options: enumOptions(glAccountCategoryOptions, 'GLAccountCategory') },
      { field: 'accountType', labelKey: 'Erp::AccountType', type: 'select', options: enumOptions(glAccountTypeOptions, 'GLAccountType'), width: 110 },
      { field: 'netChange', labelKey: 'Erp::NetChange', type: 'currency', sortable: false, width: 130 },
      { field: 'balance', labelKey: 'Erp::Balance', type: 'currency', sortable: false, width: 130 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'posting', labelKey: 'Erp::Posting' },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', required: true, maxLength: 20 },
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 100 },
      { field: 'incomeBalance', labelKey: 'Erp::IncomeBalance', type: 'select', required: true, options: enumOptions(incomeBalanceTypeOptions, 'IncomeBalanceType') },
      { field: 'accountCategory', labelKey: 'Erp::AccountCategory', type: 'select', required: true, options: enumOptions(glAccountCategoryOptions, 'GLAccountCategory') },
      { field: 'subcategory', labelKey: 'Erp::Subcategory', type: 'text', maxLength: 80, cardOnly: true },
      { field: 'balance', labelKey: 'Erp::Balance', type: 'readonly' },
      { field: 'accountType', labelKey: 'Erp::AccountType', type: 'select', section: 'posting', required: true, options: enumOptions(glAccountTypeOptions, 'GLAccountType') },
      { field: 'directPosting', labelKey: 'Erp::DirectPosting', type: 'checkbox', section: 'posting' },
      codeField('vatProdPostingGroup', 'Erp::VatProdPostingGroup', 'vatProdPostingGroup', 'posting'),
      // What a general journal line on this account defaults to (BC's Gen. Posting Type and VAT Bus. Posting Group).
      {
        field: 'genPostingType',
        labelKey: 'Erp::GenPostingType',
        type: 'select',
        section: 'posting',
        cardOnly: true,
        options: enumOptions(generalPostingTypeOptions, 'GeneralPostingType'),
      },
      codeField('vatBusPostingGroup', 'Erp::VatBusPostingGroup', 'vatBusPostingGroup', 'posting', { cardOnly: true }),
      { field: 'netChange', labelKey: 'Erp::NetChange', type: 'readonly', section: 'posting' },
    ],
    getList: query => this.glAccounts.getList(query),
    get: id => this.glAccounts.get(id),
    create: input => this.glAccounts.create(input),
    update: (id, input) => this.glAccounts.update(id, input),
    delete: id => this.glAccounts.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.name ?? undefined }),
    newRecord: term => ({
      name: term ?? '',
      accountType: GLAccountType.Posting,
      accountCategory: GLAccountCategory.Assets,
      incomeBalance: IncomeBalanceType.BalanceSheet,
      directPosting: true,
      genPostingType: GeneralPostingType.None,
    }),
    facts: dto => [
      { labelKey: 'Erp::NetChange', value: dto.netChange, type: 'currency' },
      { labelKey: 'Erp::Balance', value: dto.balance, type: 'currency' },
      { labelKey: 'Erp::Blocked', value: dto.blocked, type: 'boolean', warnWhenTrue: true },
    ],
    actions: blockActions<GLAccountDto>('Erp.GLAccounts', this.glAccounts),
  };

  readonly unitOfMeasure: RecordEntity<UnitOfMeasureDto, CreateUpdateUnitOfMeasureDto> = {
    key: 'unitOfMeasure',
    titleKey: 'Erp::UnitOfMeasure',
    pluralKey: 'Erp::UnitsOfMeasure',
    icon: 'fas fa-ruler-combined',
    permission: 'Erp.UnitsOfMeasure',
    listRoute: ['/erp/units-of-measure'],
    columns: [
      { field: 'code', labelKey: 'Erp::Code', width: 120 },
      { field: 'description', labelKey: 'Erp::Description', width: 300 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true, maxLength: 10 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
    ],
    getList: query => this.units.getList(query),
    get: id => this.units.get(id),
    create: input => this.units.create(input),
    update: (id, input) => this.units.update(id, input),
    delete: id => this.units.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: dto.description ?? undefined }),
    newRecord: term => ({ code: codeOf(term, 10) }),
    quickCreate: term => ({ code: codeOf(term, 10), description: '' }),
  };

  readonly itemCategory: RecordEntity<ItemCategoryDto, CreateUpdateItemCategoryDto> = {
    key: 'itemCategory',
    titleKey: 'Erp::ItemCategory',
    pluralKey: 'Erp::ItemCategories',
    icon: 'fas fa-sitemap',
    permission: 'Erp.ItemCategories',
    listRoute: ['/erp/item-categories'],
    columns: [
      { field: 'code', labelKey: 'Erp::Code', width: 140 },
      { field: 'description', labelKey: 'Erp::Description', width: 260 },
      { field: 'parentCategoryCode', labelKey: 'Erp::ParentCategory', sortable: false },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true, maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      {
        field: 'parentCategoryId',
        labelKey: 'Erp::ParentCategory',
        type: 'lookup',
        lookupEntity: 'itemCategory',
        lookupValueField: 'id',
        lookupDisplayField: 'parentCategoryCode',
      },
    ],
    getList: query => this.categories.getList(query),
    get: id => this.categories.get(id),
    create: input => this.categories.create(input),
    update: (id, input) => this.categories.update(id, input),
    delete: id => this.categories.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: dto.description ?? undefined }),
    newRecord: term => ({ code: codeOf(term, 20) }),
    quickCreate: term => ({ code: codeOf(term, 20), description: '' }),
  };

  readonly genBusPostingGroup = postingGroupEntity(
    'genBusPostingGroup',
    'Erp::GenBusPostingGroup',
    'Erp::GenBusPostingGroups',
    'fas fa-people-group',
    '/erp/gen-bus-posting-groups',
    this.genBusGroups,
  );

  readonly genProdPostingGroup = postingGroupEntity(
    'genProdPostingGroup',
    'Erp::GenProdPostingGroup',
    'Erp::GenProdPostingGroups',
    'fas fa-tags',
    '/erp/gen-prod-posting-groups',
    this.genProdGroups,
  );

  readonly customerPostingGroup = postingGroupEntity(
    'customerPostingGroup',
    'Erp::CustomerPostingGroup',
    'Erp::CustomerPostingGroups',
    'fas fa-user-tag',
    '/erp/customer-posting-groups',
    this.customerGroups,
    { field: 'receivablesAccountNo', labelKey: 'Erp::ReceivablesAccount' },
  );

  readonly vendorPostingGroup = postingGroupEntity(
    'vendorPostingGroup',
    'Erp::VendorPostingGroup',
    'Erp::VendorPostingGroups',
    'fas fa-truck-ramp-box',
    '/erp/vendor-posting-groups',
    this.vendorGroups,
    { field: 'payablesAccountNo', labelKey: 'Erp::PayablesAccount' },
  );

  readonly inventoryPostingGroup = postingGroupEntity(
    'inventoryPostingGroup',
    'Erp::InventoryPostingGroup',
    'Erp::InventoryPostingGroups',
    'fas fa-warehouse',
    '/erp/inventory-posting-groups',
    this.inventoryGroups,
  );

  /** BC General Posting Setup: the accounts per business and product group pair. */
  readonly generalPostingSetup: RecordEntity<GeneralPostingSetupDto, CreateUpdateGeneralPostingSetupDto> = {
    key: 'generalPostingSetup',
    titleKey: 'Erp::GeneralPostingSetup',
    pluralKey: 'Erp::GeneralPostingSetup',
    icon: 'fas fa-table-cells',
    permission: 'Erp.PostingSetup',
    listRoute: ['/erp/general-posting-setup'],
    columns: [
      { field: 'genBusPostingGroup', labelKey: 'Erp::GenBusPostingGroup', width: 160 },
      { field: 'genProdPostingGroup', labelKey: 'Erp::GenProdPostingGroup', width: 160 },
      { field: 'salesAccountNo', labelKey: 'Erp::SalesAccount', sortable: false },
      { field: 'purchAccountNo', labelKey: 'Erp::PurchAccount', sortable: false },
      { field: 'cogsAccountNo', labelKey: 'Erp::COGSAccount', sortable: false },
      { field: 'inventoryAdjmtAccountNo', labelKey: 'Erp::InventoryAdjmtAccount', sortable: false },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'sales', labelKey: 'Erp::Sales' },
      { key: 'purchases', labelKey: 'Erp::Purchases' },
      { key: 'inventory', labelKey: 'Erp::Inventory' },
    ],
    fields: [
      {
        field: 'genBusPostingGroup',
        labelKey: 'Erp::GenBusPostingGroup',
        type: 'lookup',
        lookupEntity: 'genBusPostingGroup',
        helpKey: 'Erp::GenBusPostingGroupBlankHelp',
      },
      {
        field: 'genProdPostingGroup',
        labelKey: 'Erp::GenProdPostingGroup',
        type: 'lookup',
        lookupEntity: 'genProdPostingGroup',
        required: true,
      },
      accountField('salesAccountNo', 'Erp::SalesAccount', 'sales'),
      accountField('salesCreditMemoAccountNo', 'Erp::SalesCreditMemoAccount', 'sales'),
      accountField('salesDiscountAccountNo', 'Erp::SalesDiscountAccount', 'sales'),
      accountField('purchAccountNo', 'Erp::PurchAccount', 'purchases'),
      accountField('purchCreditMemoAccountNo', 'Erp::PurchCreditMemoAccount', 'purchases'),
      accountField('purchDiscountAccountNo', 'Erp::PurchDiscountAccount', 'purchases'),
      accountField('cogsAccountNo', 'Erp::COGSAccount', 'inventory'),
      accountField('inventoryAdjmtAccountNo', 'Erp::InventoryAdjmtAccount', 'inventory'),
    ],
    getList: query => this.generalPostingSetups.getList(query),
    get: id => this.generalPostingSetups.get(id),
    create: input => this.generalPostingSetups.create(input),
    update: (id, input) => this.generalPostingSetups.update(id, input),
    delete: id => this.generalPostingSetups.delete(id),
    toItem: dto => ({
      id: dto.id,
      code: `${dto.genBusPostingGroup ?? ''} / ${dto.genProdPostingGroup ?? ''}`.trim(),
    }),
    newRecord: () => ({}),
  };

  /** BC Inventory Posting Setup: the inventory account per inventory posting group. */
  readonly inventoryPostingSetup: RecordEntity<InventoryPostingSetupDto, CreateUpdateInventoryPostingSetupDto> = {
    key: 'inventoryPostingSetup',
    titleKey: 'Erp::InventoryPostingSetup',
    pluralKey: 'Erp::InventoryPostingSetup',
    icon: 'fas fa-boxes-stacked',
    permission: 'Erp.PostingSetup',
    listRoute: ['/erp/inventory-posting-setup'],
    columns: [
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 140 },
      { field: 'inventoryPostingGroup', labelKey: 'Erp::InventoryPostingGroup', width: 200 },
      { field: 'inventoryAccountNo', labelKey: 'Erp::InventoryAccount', sortable: false },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('locationCode', 'Erp::LocationCode', 'location'),
      {
        field: 'inventoryPostingGroup',
        labelKey: 'Erp::InventoryPostingGroup',
        type: 'lookup',
        lookupEntity: 'inventoryPostingGroup',
        required: true,
      },
      accountField('inventoryAccountNo', 'Erp::InventoryAccount', 'general'),
    ],
    getList: query => this.inventoryPostingSetups.getList(query),
    get: id => this.inventoryPostingSetups.get(id),
    create: input => this.inventoryPostingSetups.create(input),
    update: (id, input) => this.inventoryPostingSetups.update(id, input),
    delete: id => this.inventoryPostingSetups.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.locationCode ?? ''} ${dto.inventoryPostingGroup ?? ''}`.trim() }),
    newRecord: () => ({}),
  };

  get all(): RecordEntity[] {
    return [
      this.customer,
      this.vendor,
      this.item,
      this.glAccount,
      this.unitOfMeasure,
      this.itemCategory,
      this.genBusPostingGroup,
      this.genProdPostingGroup,
      this.customerPostingGroup,
      this.vendorPostingGroup,
      this.inventoryPostingGroup,
      this.generalPostingSetup,
      this.inventoryPostingSetup,
      ...this.setup.all,
    ];
  }
}
