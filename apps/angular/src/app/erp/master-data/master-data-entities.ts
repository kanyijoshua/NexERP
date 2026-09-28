import { Injectable, inject } from '@angular/core';
import {
  CreateUpdateGLAccountDto,
  GLAccountCategory,
  GLAccountDto,
  GlAccountService,
  GLAccountType,
  IncomeBalanceType,
  glAccountCategoryOptions,
  glAccountTypeOptions,
  incomeBalanceTypeOptions,
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
import { Observable, map } from 'rxjs';
import {
  DocumentLineOption,
  RecordAction,
  RecordEntity,
  RecordField,
  SmartButton,
} from '../erp-shared';

/** Enum options labelled `Erp::Enum:<EnumName>.<Member>`. */
function enumOptions(
  options: { key: string; value: number }[],
  enumName: string,
): DocumentLineOption[] {
  return options.map(o => ({ value: o.value, label: `Erp::Enum:${enumName}.${o.key}` }));
}

/** BC's Block / Unblock on customers, vendors, items and G/L accounts. */
function blockActions<T extends { id?: string; blocked: boolean }>(
  permission: string,
  service: { block(id: string): Observable<void>; unblock(id: string): Observable<void> },
): RecordAction<T>[] {
  return [
    {
      key: 'block',
      labelKey: 'Erp::Block',
      icon: 'fas fa-ban',
      permission: `${permission}.Update`,
      visible: dto => !dto.blocked,
      confirmKey: 'Erp::BlockConfirmation',
      run: dto => service.block(dto.id!),
    },
    {
      key: 'unblock',
      labelKey: 'Erp::Unblock',
      icon: 'fas fa-circle-check',
      permission: `${permission}.Update`,
      visible: dto => dto.blocked,
      run: dto => service.unblock(dto.id!),
    },
  ];
}

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
      { field: 'customerPostingGroup', labelKey: 'Erp::CustomerPostingGroup', type: 'text', section: 'invoicing', maxLength: 20, cardOnly: true },
      { field: 'genBusPostingGroup', labelKey: 'Erp::GenBusPostingGroup', type: 'text', section: 'invoicing', maxLength: 20, cardOnly: true },
      { field: 'currencyCode', labelKey: 'Erp::CurrencyCode', type: 'text', section: 'invoicing', maxLength: 10, cardOnly: true },
      { field: 'paymentTermsCode', labelKey: 'Erp::PaymentTermsCode', type: 'text', section: 'payments', maxLength: 10, cardOnly: true },
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
      ),
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
      { field: 'vendorPostingGroup', labelKey: 'Erp::VendorPostingGroup', type: 'text', section: 'invoicing', maxLength: 20, cardOnly: true },
      { field: 'genBusPostingGroup', labelKey: 'Erp::GenBusPostingGroup', type: 'text', section: 'invoicing', maxLength: 20, cardOnly: true },
      { field: 'currencyCode', labelKey: 'Erp::CurrencyCode', type: 'text', section: 'invoicing', maxLength: 10, cardOnly: true },
      { field: 'paymentTermsCode', labelKey: 'Erp::PaymentTermsCode', type: 'text', section: 'payments', maxLength: 10, cardOnly: true },
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
      ),
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
      { field: 'no', labelKey: 'Erp::No', type: 'text', required: true, maxLength: 20 },
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
      { field: 'genProdPostingGroup', labelKey: 'Erp::GenProdPostingGroup', type: 'text', section: 'costs', maxLength: 20, cardOnly: true },
      { field: 'inventoryPostingGroup', labelKey: 'Erp::InventoryPostingGroup', type: 'text', section: 'costs', maxLength: 20, cardOnly: true },
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

  get all(): RecordEntity[] {
    return [
      this.customer,
      this.vendor,
      this.item,
      this.glAccount,
      this.unitOfMeasure,
      this.itemCategory,
    ];
  }
}

/** A typed term as a BC code: upper case, no spaces at the ends, cut to the field length. */
export function codeOf(term: string | undefined, maxLength: number): string {
  return (term ?? '').trim().toUpperCase().substring(0, maxLength);
}
