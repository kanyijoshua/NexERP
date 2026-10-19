import { Injectable, inject } from '@angular/core';
import { documentLineTypeOptions } from '@proxy/documents';
import {
  CreateUpdateCustomerPriceGroupDto,
  CreateUpdatePriceListDto,
  CreateUpdatePriceListLineDto,
  CustomerPriceGroupDto,
  CustomerPriceGroupService,
  PriceAssetType,
  PriceListDto,
  PriceListLineDto,
  PriceListLineService,
  PriceListService,
  PriceListStatus,
  PriceSourceType,
  PriceType,
  priceAssetTypeOptions,
  priceListStatusOptions,
  priceSourceTypeOptions,
  priceTypeOptions,
} from '@proxy/pricing';
import {
  PostedPurchaseInvoiceDto,
  PostedPurchaseInvoiceService,
  PostedPurchaseReceiptDto,
  PostedPurchaseReceiptService,
} from '@proxy/purchasing';
import {
  PostedSalesInvoiceDto,
  PostedSalesInvoiceService,
  PostedSalesShipmentDto,
  PostedSalesShipmentService,
} from '@proxy/sales';
import { EMPTY, Observable, concat, map, of, toArray } from 'rxjs';
import { RecordColumn, RecordEntity, RecordField } from '../erp-shared';
import { codeField, codeTableEntity, enumOptions } from './entity-helpers';

const SALES = 'Erp.SalesDocuments';
const PURCHASES = 'Erp.PurchaseDocuments';
const PRICES = 'Erp.PriceLists';

const noList = () => of({ items: [], totalCount: 0 });

function readonly(field: string, labelKey: string, section = 'general'): RecordField {
  return { field, labelKey, type: 'readonly', section };
}

/** A line entity shown only inside its posted document, whose list it opens on. */
function postedLineEntity(key: string, titleKey: string, pluralKey: string, permission: string, listRoute: string, columns: RecordColumn[]): RecordEntity {
  return {
    key,
    titleKey,
    pluralKey,
    icon: 'fas fa-list',
    permission,
    listRoute: [listRoute],
    readOnly: true,
    columns,
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: columns.map(c => readonly(c.field, c.labelKey)),
    getList: noList,
    get: () => EMPTY,
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.lineNo ?? ''), name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };
}

const lineTypeColumn: RecordColumn = {
  field: 'type',
  labelKey: 'Erp::Type',
  type: 'select',
  options: enumOptions(documentLineTypeOptions, 'DocumentLineType'),
};

/** Posted shipments, receipts and invoices, price lists and customer price groups. */
@Injectable({ providedIn: 'root' })
export class OrdersEntities {
  private readonly shipments = inject(PostedSalesShipmentService);
  private readonly salesInvoices = inject(PostedSalesInvoiceService);
  private readonly receipts = inject(PostedPurchaseReceiptService);
  private readonly purchaseInvoices = inject(PostedPurchaseInvoiceService);
  private readonly priceLists = inject(PriceListService);
  private readonly priceListLines = inject(PriceListLineService);
  private readonly priceGroups = inject(CustomerPriceGroupService);

  readonly postedSalesShipment: RecordEntity<PostedSalesShipmentDto, never> = {
    key: 'postedSalesShipment',
    titleKey: 'Erp::PostedSalesShipment',
    pluralKey: 'Erp::PostedSalesShipments',
    icon: 'fas fa-truck-fast',
    permission: SALES,
    listRoute: ['/erp/posted-sales-shipments'],
    readOnly: true,
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 150 },
      { field: 'return', labelKey: 'Erp::ReturnReceipt', type: 'boolean', width: 110 },
      { field: 'orderNo', labelKey: 'Erp::OrderNo', width: 140 },
      { field: 'sellToCustomerNo', labelKey: 'Erp::Customer', width: 120 },
      { field: 'sellToCustomerName', labelKey: 'Erp::Name' },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 110 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'shipping', labelKey: 'Erp::Shipping' },
    ],
    fields: [
      readonly('no', 'Erp::No'),
      readonly('orderNo', 'Erp::OrderNo'),
      readonly('sellToCustomerNo', 'Erp::Customer'),
      readonly('sellToCustomerName', 'Erp::Name'),
      readonly('postingDate', 'Erp::PostingDate'),
      readonly('externalDocumentNo', 'Erp::ExternalDocumentNo'),
      readonly('locationCode', 'Erp::LocationCode', 'shipping'),
      readonly('shipToName', 'Erp::ShipToName', 'shipping'),
      readonly('shipToAddress', 'Erp::ShipToAddress', 'shipping'),
      readonly('shipToCity', 'Erp::ShipToCity', 'shipping'),
    ],
    getList: query => this.shipments.getList(query),
    get: id => this.shipments.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.sellToCustomerName ?? undefined }),
    newRecord: () => ({}),
    actions: [
      {
        key: 'undo',
        labelKey: 'Erp::UndoShipment',
        icon: 'fas fa-rotate-left',
        permission: `${SALES}.Post`,
        confirmKey: 'Erp::UndoShipmentConfirmation',
        visible: dto => (dto.lines ?? []).some(l => !l.correction && l.quantityInvoiced === 0),
        run: dto => undoAll(dto.lines ?? [], id => this.shipments.undoLine(id)),
      },
    ],
    parts: [
      {
        entity: 'postedSalesShipmentLine',
        lines: dto => of(dto.lines ?? []),
        totals: ['quantity'],
        editable: () => false,
      },
    ],
  };

  readonly postedSalesShipmentLine = postedLineEntity(
    'postedSalesShipmentLine', 'Erp::PostedSalesShipmentLine', 'Erp::Lines', SALES, '/erp/posted-sales-shipments',
    [
      { field: 'lineNo', labelKey: 'Erp::LineNo', type: 'number' },
      lineTypeColumn,
      { field: 'no', labelKey: 'Erp::No' },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number' },
      { field: 'quantityInvoiced', labelKey: 'Erp::QuantityInvoiced', type: 'number' },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency' },
      { field: 'locationCode', labelKey: 'Erp::LocationCode' },
      { field: 'correction', labelKey: 'Erp::Correction', type: 'boolean' },
    ],
  );

  readonly postedSalesInvoice: RecordEntity<PostedSalesInvoiceDto, never> = {
    key: 'postedSalesInvoice',
    titleKey: 'Erp::PostedSalesInvoice',
    pluralKey: 'Erp::PostedSalesInvoices',
    icon: 'fas fa-file-invoice-dollar',
    permission: SALES,
    listRoute: ['/erp/posted-sales-invoices'],
    readOnly: true,
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 150 },
      { field: 'creditMemo', labelKey: 'Erp::CreditMemo', type: 'boolean', width: 110 },
      { field: 'orderNo', labelKey: 'Erp::OrderNo', width: 140 },
      { field: 'sellToCustomerNo', labelKey: 'Erp::Customer', width: 120 },
      { field: 'sellToCustomerName', labelKey: 'Erp::Name' },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date', width: 120 },
      { field: 'totalAmount', labelKey: 'Erp::Amount', type: 'currency', width: 130 },
      { field: 'totalAmountIncludingVat', labelKey: 'Erp::AmountIncludingVat', type: 'currency', width: 150 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('no', 'Erp::No'),
      readonly('preAssignedNo', 'Erp::PreAssignedNo'),
      readonly('orderNo', 'Erp::OrderNo'),
      readonly('sellToCustomerNo', 'Erp::Customer'),
      readonly('sellToCustomerName', 'Erp::Name'),
      readonly('postingDate', 'Erp::PostingDate'),
      readonly('dueDate', 'Erp::DueDate'),
      readonly('currencyCode', 'Erp::CurrencyCode'),
      readonly('externalDocumentNo', 'Erp::ExternalDocumentNo'),
    ],
    getList: query => this.salesInvoices.getList(query),
    get: id => this.salesInvoices.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.sellToCustomerName ?? undefined }),
    newRecord: () => ({}),
    facts: dto => [
      { labelKey: 'Erp::Amount', value: dto.totalAmount, type: 'currency' },
      { labelKey: 'Erp::AmountIncludingVat', value: dto.totalAmountIncludingVat, type: 'currency' },
    ],
    parts: [
      {
        entity: 'postedSalesInvoiceLine',
        lines: dto => of(dto.lines ?? []),
        totals: ['lineAmount', 'vatAmount', 'amountIncludingVat'],
        editable: () => false,
      },
    ],
  };

  readonly postedSalesInvoiceLine = postedLineEntity(
    'postedSalesInvoiceLine', 'Erp::PostedSalesInvoiceLine', 'Erp::Lines', SALES, '/erp/posted-sales-invoices',
    [
      { field: 'lineNo', labelKey: 'Erp::LineNo', type: 'number' },
      { field: 'type', labelKey: 'Erp::Type' },
      { field: 'no', labelKey: 'Erp::No' },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number' },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency' },
      { field: 'lineDiscountPercent', labelKey: 'Erp::LineDiscountPercent', type: 'number' },
      { field: 'lineAmount', labelKey: 'Erp::LineAmount', type: 'currency' },
      { field: 'vatAmount', labelKey: 'Erp::VatAmount', type: 'currency' },
      { field: 'amountIncludingVat', labelKey: 'Erp::AmountIncludingVat', type: 'currency' },
      { field: 'shipmentNo', labelKey: 'Erp::ShipmentNo' },
    ],
  );

  readonly postedPurchaseReceipt: RecordEntity<PostedPurchaseReceiptDto, never> = {
    key: 'postedPurchaseReceipt',
    titleKey: 'Erp::PostedPurchaseReceipt',
    pluralKey: 'Erp::PostedPurchaseReceipts',
    icon: 'fas fa-dolly',
    permission: PURCHASES,
    listRoute: ['/erp/posted-purchase-receipts'],
    readOnly: true,
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 150 },
      { field: 'return', labelKey: 'Erp::ReturnShipment', type: 'boolean', width: 110 },
      { field: 'orderNo', labelKey: 'Erp::OrderNo', width: 140 },
      { field: 'buyFromVendorNo', labelKey: 'Erp::Vendor', width: 120 },
      { field: 'buyFromVendorName', labelKey: 'Erp::Name' },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 110 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('no', 'Erp::No'),
      readonly('orderNo', 'Erp::OrderNo'),
      readonly('buyFromVendorNo', 'Erp::Vendor'),
      readonly('buyFromVendorName', 'Erp::Name'),
      readonly('postingDate', 'Erp::PostingDate'),
      readonly('vendorShipmentNo', 'Erp::VendorShipmentNo'),
      readonly('locationCode', 'Erp::LocationCode'),
    ],
    getList: query => this.receipts.getList(query),
    get: id => this.receipts.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.buyFromVendorName ?? undefined }),
    newRecord: () => ({}),
    actions: [
      {
        key: 'undo',
        labelKey: 'Erp::UndoReceipt',
        icon: 'fas fa-rotate-left',
        permission: `${PURCHASES}.Post`,
        confirmKey: 'Erp::UndoReceiptConfirmation',
        visible: dto => (dto.lines ?? []).some(l => !l.correction && l.quantityInvoiced === 0),
        run: dto => undoAll(dto.lines ?? [], id => this.receipts.undoLine(id)),
      },
    ],
    parts: [
      {
        entity: 'postedPurchaseReceiptLine',
        lines: dto => of(dto.lines ?? []),
        totals: ['quantity'],
        editable: () => false,
      },
    ],
  };

  readonly postedPurchaseReceiptLine = postedLineEntity(
    'postedPurchaseReceiptLine', 'Erp::PostedPurchaseReceiptLine', 'Erp::Lines', PURCHASES, '/erp/posted-purchase-receipts',
    [
      { field: 'lineNo', labelKey: 'Erp::LineNo', type: 'number' },
      lineTypeColumn,
      { field: 'no', labelKey: 'Erp::No' },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number' },
      { field: 'quantityInvoiced', labelKey: 'Erp::QuantityInvoiced', type: 'number' },
      { field: 'directUnitCost', labelKey: 'Erp::DirectUnitCost', type: 'currency' },
      { field: 'locationCode', labelKey: 'Erp::LocationCode' },
      { field: 'correction', labelKey: 'Erp::Correction', type: 'boolean' },
    ],
  );

  readonly postedPurchaseInvoice: RecordEntity<PostedPurchaseInvoiceDto, never> = {
    key: 'postedPurchaseInvoice',
    titleKey: 'Erp::PostedPurchaseInvoice',
    pluralKey: 'Erp::PostedPurchaseInvoices',
    icon: 'fas fa-file-invoice',
    permission: PURCHASES,
    listRoute: ['/erp/posted-purchase-invoices'],
    readOnly: true,
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 150 },
      { field: 'creditMemo', labelKey: 'Erp::CreditMemo', type: 'boolean', width: 110 },
      { field: 'orderNo', labelKey: 'Erp::OrderNo', width: 140 },
      { field: 'buyFromVendorNo', labelKey: 'Erp::Vendor', width: 120 },
      { field: 'buyFromVendorName', labelKey: 'Erp::Name' },
      { field: 'vendorInvoiceNo', labelKey: 'Erp::VendorInvoiceNo', width: 140 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'totalAmount', labelKey: 'Erp::Amount', type: 'currency', width: 130 },
      { field: 'totalAmountIncludingVat', labelKey: 'Erp::AmountIncludingVat', type: 'currency', width: 150 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('no', 'Erp::No'),
      readonly('preAssignedNo', 'Erp::PreAssignedNo'),
      readonly('orderNo', 'Erp::OrderNo'),
      readonly('buyFromVendorNo', 'Erp::Vendor'),
      readonly('buyFromVendorName', 'Erp::Name'),
      readonly('vendorInvoiceNo', 'Erp::VendorInvoiceNo'),
      readonly('postingDate', 'Erp::PostingDate'),
      readonly('dueDate', 'Erp::DueDate'),
      readonly('currencyCode', 'Erp::CurrencyCode'),
    ],
    getList: query => this.purchaseInvoices.getList(query),
    get: id => this.purchaseInvoices.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.buyFromVendorName ?? undefined }),
    newRecord: () => ({}),
    facts: dto => [
      { labelKey: 'Erp::Amount', value: dto.totalAmount, type: 'currency' },
      { labelKey: 'Erp::AmountIncludingVat', value: dto.totalAmountIncludingVat, type: 'currency' },
    ],
    parts: [
      {
        entity: 'postedPurchaseInvoiceLine',
        lines: dto => of(dto.lines ?? []),
        totals: ['lineAmount', 'vatAmount', 'amountIncludingVat'],
        editable: () => false,
      },
    ],
  };

  readonly postedPurchaseInvoiceLine = postedLineEntity(
    'postedPurchaseInvoiceLine', 'Erp::PostedPurchaseInvoiceLine', 'Erp::Lines', PURCHASES, '/erp/posted-purchase-invoices',
    [
      { field: 'lineNo', labelKey: 'Erp::LineNo', type: 'number' },
      { field: 'type', labelKey: 'Erp::Type' },
      { field: 'no', labelKey: 'Erp::No' },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number' },
      { field: 'directUnitCost', labelKey: 'Erp::DirectUnitCost', type: 'currency' },
      { field: 'lineDiscountPercent', labelKey: 'Erp::LineDiscountPercent', type: 'number' },
      { field: 'lineAmount', labelKey: 'Erp::LineAmount', type: 'currency' },
      { field: 'vatAmount', labelKey: 'Erp::VatAmount', type: 'currency' },
      { field: 'amountIncludingVat', labelKey: 'Erp::AmountIncludingVat', type: 'currency' },
      { field: 'receiptNo', labelKey: 'Erp::ReceiptNo' },
    ],
  );

  readonly priceList: RecordEntity<PriceListDto, CreateUpdatePriceListDto> = {
    key: 'priceList',
    titleKey: 'Erp::PriceList',
    pluralKey: 'Erp::PriceLists',
    icon: 'fas fa-tags',
    permission: PRICES,
    listRoute: ['/erp/price-lists'],
    columns: [
      { field: 'code', labelKey: 'Erp::Code', width: 130 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'priceType', labelKey: 'Erp::PriceType', type: 'select', options: enumOptions(priceTypeOptions, 'PriceType'), width: 110 },
      { field: 'sourceType', labelKey: 'Erp::AppliesTo', type: 'select', options: enumOptions(priceSourceTypeOptions, 'PriceSourceType'), width: 170 },
      { field: 'sourceNo', labelKey: 'Erp::AppliesToNo', width: 120 },
      { field: 'currencyCode', labelKey: 'Erp::CurrencyCode', width: 100 },
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', width: 120 },
      { field: 'endingDate', labelKey: 'Erp::EndingDate', type: 'date', width: 120 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(priceListStatusOptions, 'PriceListStatus'), width: 110 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'validity', labelKey: 'Erp::Validity' },
    ],
    fields: [
      { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true, maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      { field: 'priceType', labelKey: 'Erp::PriceType', type: 'select', options: enumOptions(priceTypeOptions, 'PriceType'), createOnly: true },
      { field: 'sourceType', labelKey: 'Erp::AppliesTo', type: 'select', options: enumOptions(priceSourceTypeOptions, 'PriceSourceType') },
      { field: 'sourceNo', labelKey: 'Erp::AppliesToNo', type: 'text', maxLength: 20, helpKey: 'Erp::AppliesToNoHelp' },
      codeField('currencyCode', 'Erp::CurrencyCode', 'currency', 'general'),
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', section: 'validity' },
      { field: 'endingDate', labelKey: 'Erp::EndingDate', type: 'date', section: 'validity' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(priceListStatusOptions, 'PriceListStatus'), section: 'validity' },
    ],
    getList: query => this.priceLists.getList(query),
    get: id => this.priceLists.get(id),
    create: input => this.priceLists.create(input),
    update: (id, input) => this.priceLists.update(id, input),
    delete: id => this.priceLists.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({
      priceType: PriceType.Sale,
      sourceType: PriceSourceType.AllCustomers,
      status: PriceListStatus.Draft,
      startingDate: new Date().toISOString().substring(0, 10),
    }),
    actions: [
      {
        key: 'activate',
        labelKey: 'Erp::Activate',
        icon: 'fas fa-circle-check',
        permission: `${PRICES}.Update`,
        visible: dto => dto.status !== PriceListStatus.Active,
        run: dto => this.priceLists.update(dto.id!, { ...(dto as CreateUpdatePriceListDto), status: PriceListStatus.Active }),
      },
      {
        key: 'draft',
        labelKey: 'Erp::SetToDraft',
        icon: 'fas fa-pen',
        permission: `${PRICES}.Update`,
        visible: dto => dto.status === PriceListStatus.Active,
        run: dto => this.priceLists.update(dto.id!, { ...(dto as CreateUpdatePriceListDto), status: PriceListStatus.Draft }),
      },
    ],
    parts: [
      {
        entity: 'priceListLine',
        lines: dto =>
          this.priceListLines
            .getList({ priceListId: dto.id, maxResultCount: 1000, skipCount: 0 })
            .pipe(map(r => r.items ?? [])),
        newLine: dto => ({ priceListId: dto.id, assetType: PriceAssetType.Item, minimumQuantity: 0, lineDiscountPercent: 0 }),
        columns: ['assetType', 'assetNo', 'description', 'unitOfMeasureCode', 'minimumQuantity', 'unitPrice', 'lineDiscountPercent'],
        editable: dto => dto.status !== PriceListStatus.Active,
      },
    ],
  };

  readonly priceListLine: RecordEntity<PriceListLineDto, CreateUpdatePriceListLineDto> = {
    key: 'priceListLine',
    titleKey: 'Erp::PriceListLine',
    pluralKey: 'Erp::PriceListLines',
    icon: 'fas fa-tag',
    permission: PRICES,
    listRoute: ['/erp/price-list-lines'],
    columns: [
      { field: 'assetType', labelKey: 'Erp::AssetType', type: 'select', options: enumOptions(priceAssetTypeOptions, 'PriceAssetType'), width: 130 },
      { field: 'assetNo', labelKey: 'Erp::ItemOrCategory', width: 140 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'unitOfMeasureCode', labelKey: 'Erp::UnitOfMeasureCode', width: 110 },
      { field: 'minimumQuantity', labelKey: 'Erp::MinimumQuantity', type: 'number', width: 120 },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', width: 130 },
      { field: 'lineDiscountPercent', labelKey: 'Erp::LineDiscountPercent', type: 'number', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'priceListId', labelKey: 'Erp::PriceList', type: 'lookup', lookupEntity: 'priceList', lookupValueField: 'id', required: true },
      { field: 'assetType', labelKey: 'Erp::AssetType', type: 'select', options: enumOptions(priceAssetTypeOptions, 'PriceAssetType') },
      { field: 'assetNo', labelKey: 'Erp::ItemOrCategory', type: 'text', required: true, maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      codeField('unitOfMeasureCode', 'Erp::UnitOfMeasureCode', 'unitOfMeasure', 'general'),
      { field: 'minimumQuantity', labelKey: 'Erp::MinimumQuantity', type: 'number', min: 0 },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', min: 0 },
      { field: 'lineDiscountPercent', labelKey: 'Erp::LineDiscountPercent', type: 'number', min: 0 },
    ],
    getList: query => this.priceListLines.getList(query),
    get: id => this.priceListLines.get(id),
    create: input => this.priceListLines.create(input),
    update: (id, input) => this.priceListLines.update(id, input),
    delete: id => this.priceListLines.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.assetNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ assetType: PriceAssetType.Item, minimumQuantity: 0, unitPrice: 0, lineDiscountPercent: 0 }),
  };

  readonly customerPriceGroup = codeTableEntity<CustomerPriceGroupDto, CreateUpdateCustomerPriceGroupDto>(this.priceGroups, {
    key: 'customerPriceGroup',
    titleKey: 'Erp::CustomerPriceGroup',
    pluralKey: 'Erp::CustomerPriceGroups',
    icon: 'fas fa-users-rectangle',
    permission: PRICES,
    route: '/erp/customer-price-groups',
    codeLength: 10,
    columns: [{ field: 'allowLineDiscount', labelKey: 'Erp::AllowLineDiscount', type: 'boolean', width: 140 }],
    fields: [{ field: 'allowLineDiscount', labelKey: 'Erp::AllowLineDiscount', type: 'checkbox' }],
    defaults: { allowLineDiscount: true },
  });

  get all(): RecordEntity[] {
    return [
      this.postedSalesShipment,
      this.postedSalesShipmentLine,
      this.postedSalesInvoice,
      this.postedSalesInvoiceLine,
      this.postedPurchaseReceipt,
      this.postedPurchaseReceiptLine,
      this.postedPurchaseInvoice,
      this.postedPurchaseInvoiceLine,
      this.priceList,
      this.priceListLine,
      this.customerPriceGroup,
    ];
  }
}

/**
 * Undoes every line of a posted shipment or receipt that has not been invoiced or undone yet, one
 * after the other: each undo changes the same order.
 */
function undoAll(lines: { id?: string; correction: boolean; quantityInvoiced: number }[], undo: (id: string) => Observable<void>): Observable<unknown> {
  const open = lines.filter(l => l.id && !l.correction && l.quantityInvoiced === 0);
  return open.length ? concat(...open.map(l => undo(l.id!))).pipe(toArray()) : of(null);
}
