import { Injectable, inject } from '@angular/core';
import {
  AssemblyBomComponentDto,
  AssemblyBomComponentService,
  AssemblyLineDto,
  AssemblyLineService,
  AssemblyOrderDto,
  AssemblyOrderService,
  AssemblyOrderStatus,
  BinContentDto,
  BinDto,
  BinService,
  CalculatePhysInvtLinesInput,
  CreateUpdateAssemblyBomComponentDto,
  CreateUpdateAssemblyOrderDto,
  CreateUpdateBinDto,
  CreateUpdatePhysInvtCountingPeriodDto,
  CreateUpdatePhysInvtOrderDto,
  CreateUpdatePhysInvtOrderLineDto,
  CreateUpdateWarehouseActivityDto,
  CreateUpdateWarehouseActivityLineDto,
  MakeWarehouseDocumentInput,
  PhysInvtCountingPeriodDto,
  PhysInvtCountingPeriodService,
  PhysInvtOrderDto,
  PhysInvtOrderLineDto,
  PhysInvtOrderLineService,
  PhysInvtOrderService,
  PhysInvtOrderStatus,
  ReservationEntryDto,
  ReservationEntryService,
  ReserveDocumentInput,
  WarehouseActivityDto,
  WarehouseActivityLineDto,
  WarehouseActivityLineService,
  WarehouseActivityService,
  WarehouseActivityStatus,
  WarehouseActivityType,
  WarehouseEntryDto,
  WarehouseEntryService,
  assemblyOrderStatusOptions,
  physInvtOrderStatusOptions,
  reservationSourceTypeOptions,
  warehouseActivityStatusOptions,
  warehouseActivityTypeOptions,
  warehouseEntryTypeOptions,
  warehouseSourceTypeOptions,
} from '@proxy/inventory';
import { EMPTY, Observable, map } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { RecordDialogService } from '../erp-shared/record/record-dialog.service';
import { codeField, codeTableEntity, enumOptions, runWithInput } from './entity-helpers';

const WAREHOUSE = 'Erp.Warehouse';
const RESERVATIONS = 'Erp.Reservations';
const PHYS = 'Erp.PhysInventory';
const ASSEMBLY = 'Erp.Assembly';
const today = () => new Date().toISOString().substring(0, 10);

const activityTypes = enumOptions(warehouseActivityTypeOptions, 'WarehouseActivityType');
const activityStatuses = enumOptions(warehouseActivityStatusOptions, 'WarehouseActivityStatus');
const assemblyStatuses = enumOptions(assemblyOrderStatusOptions, 'AssemblyOrderStatus');
const physStatuses = enumOptions(physInvtOrderStatusOptions, 'PhysInvtOrderStatus');

function readonly(field: string, labelKey: string, section?: string): RecordField {
  return { field, labelKey, type: 'readonly', section };
}

/** A bin content has no id of its own; its key is where it is and what it is. */
const contentId = (c: BinContentDto) => [c.locationCode, c.binCode, c.itemNo, c.variantCode ?? ''].join('|');

/**
 * Bins, bin contents and warehouse entries, put-aways, picks and movements, reservations,
 * physical inventory orders with counting periods, and assembly BOMs and orders.
 */
@Injectable({ providedIn: 'root' })
export class WarehouseManagementEntities {
  private readonly dialogs = inject(RecordDialogService);
  private readonly bins = inject(BinService);
  private readonly entries = inject(WarehouseEntryService);
  private readonly activities = inject(WarehouseActivityService);
  private readonly activityLines = inject(WarehouseActivityLineService);
  private readonly reservations = inject(ReservationEntryService);
  private readonly countingPeriods = inject(PhysInvtCountingPeriodService);
  private readonly physOrders = inject(PhysInvtOrderService);
  private readonly physLines = inject(PhysInvtOrderLineService);
  private readonly bom = inject(AssemblyBomComponentService);
  private readonly assemblyOrders = inject(AssemblyOrderService);
  private readonly assemblyLines = inject(AssemblyLineService);

  readonly bin: RecordEntity<BinDto, CreateUpdateBinDto> = {
    key: 'bin',
    titleKey: 'Erp::Bin',
    pluralKey: 'Erp::Bins',
    icon: 'fas fa-table-cells',
    permission: WAREHOUSE,
    listRoute: ['/erp/bins'],
    columns: [
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 120 },
      { field: 'code', labelKey: 'Erp::Code', width: 130 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('locationCode', 'Erp::LocationCode', 'location', 'general', { required: true, createOnly: true }),
      { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true, maxLength: 20, createOnly: true },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    getList: query => this.bins.getList(query),
    get: id => this.bins.get(id),
    create: input => this.bins.create(input),
    update: (id, input) => this.bins.update(id, input),
    delete: id => this.bins.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: [dto.locationCode, dto.description].filter(Boolean).join(' - ') }),
    newRecord: term => ({ code: (term ?? '').trim().toUpperCase(), blocked: false }),
  };

  readonly binContent: RecordEntity<BinContentDto & { id?: string }, never> = {
    key: 'binContent',
    titleKey: 'Erp::BinContent',
    pluralKey: 'Erp::BinContents',
    icon: 'fas fa-boxes-stacked',
    permission: WAREHOUSE,
    listRoute: ['/erp/bin-contents'],
    readOnly: true,
    columns: [
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 120, sortable: false },
      { field: 'binCode', labelKey: 'Erp::BinCode', width: 130, sortable: false },
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 130, sortable: false },
      { field: 'variantCode', labelKey: 'Erp::VariantCode', width: 110, sortable: false },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 110, sortable: false },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('locationCode', 'Erp::LocationCode'), readonly('binCode', 'Erp::BinCode'), readonly('itemNo', 'Erp::ItemNo'), readonly('quantity', 'Erp::Quantity')],
    getList: query =>
      this.bins
        .getContents({ skipCount: query.skipCount, maxResultCount: query.maxResultCount, itemNo: query.filter || undefined })
        .pipe(map(page => ({ totalCount: page.totalCount, items: (page.items ?? []).map(c => ({ ...c, id: contentId(c) })) }))),
    get: id => {
      const [locationCode, binCode, itemNo] = id.split('|');
      return this.bins
        .getContents({ locationCode, binCode, itemNo, skipCount: 0, maxResultCount: 100 })
        .pipe(map(page => ({ ...(page.items ?? []).find(c => contentId(c) === id)!, id })));
    },
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.binCode ?? '', name: dto.itemNo ?? undefined }),
    newRecord: () => ({}),
  };

  readonly warehouseEntry: RecordEntity<WarehouseEntryDto, never> = {
    key: 'warehouseEntry',
    titleKey: 'Erp::WarehouseEntry',
    pluralKey: 'Erp::WarehouseEntries',
    icon: 'fas fa-receipt',
    permission: WAREHOUSE,
    listRoute: ['/erp/warehouse-entries'],
    readOnly: true,
    columns: [
      { field: 'entryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 90 },
      { field: 'registeringDate', labelKey: 'Erp::RegisteringDate', type: 'date', width: 120 },
      { field: 'entryType', labelKey: 'Erp::EntryType', type: 'select', options: enumOptions(warehouseEntryTypeOptions, 'WarehouseEntryType'), width: 130 },
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 110 },
      { field: 'binCode', labelKey: 'Erp::BinCode', width: 110 },
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 120 },
      { field: 'variantCode', labelKey: 'Erp::VariantCode', width: 100 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'description', labelKey: 'Erp::Description' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('documentNo', 'Erp::DocumentNo'), readonly('binCode', 'Erp::BinCode'), readonly('quantity', 'Erp::Quantity')],
    getList: query => this.entries.getList(query),
    get: id => this.entries.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.entryNo), name: dto.documentNo ?? undefined }),
    newRecord: () => ({}),
  };

  readonly warehouseActivity: RecordEntity<WarehouseActivityDto, CreateUpdateWarehouseActivityDto> = {
    key: 'warehouseActivity',
    titleKey: 'Erp::WarehouseActivity',
    pluralKey: 'Erp::WarehouseActivities',
    icon: 'fas fa-dolly',
    permission: WAREHOUSE,
    listRoute: ['/erp/warehouse-activities'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: activityTypes, width: 110 },
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 110 },
      { field: 'sourceType', labelKey: 'Erp::SourceType', type: 'select', options: enumOptions(warehouseSourceTypeOptions, 'WarehouseSourceType'), width: 140 },
      { field: 'sourceNo', labelKey: 'Erp::SourceNo', width: 130 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: activityStatuses, width: 110 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('no', 'Erp::No'),
      codeField('locationCode', 'Erp::LocationCode', 'location', 'general', { required: true, createOnly: true }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      readonly('sourceNo', 'Erp::SourceNo'),
      readonly('registeredDate', 'Erp::RegisteredDate'),
    ],
    getList: query => this.activities.getList(query),
    get: id => this.activities.get(id),
    create: input => this.activities.create(input),
    update: (id, input) => this.activities.update(id, input),
    delete: id => this.activities.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? dto.sourceNo ?? undefined }),
    newRecord: () => ({}),
    facts: dto => [
      { labelKey: 'Erp::Type', value: WarehouseActivityType[dto.type] },
      { labelKey: 'Erp::Status', value: WarehouseActivityStatus[dto.status] },
    ],
    actions: [
      {
        // Moves the quantity to handle of every line; registered once nothing is left.
        key: 'register',
        labelKey: 'Erp::Register',
        icon: 'fas fa-check-double',
        permission: `${WAREHOUSE}.Register`,
        confirmKey: 'Erp::RegisterWarehouseActivityConfirmation',
        visible: dto => dto.status === WarehouseActivityStatus.Open,
        run: dto => this.activities.register(dto.id!, { registeringDate: today() }),
      },
    ],
    listActions: [
      {
        key: 'makePutAway',
        labelKey: 'Erp::MakePutAway',
        icon: 'fas fa-arrow-down-short-wide',
        permission: `${WAREHOUSE}.Create`,
        run: () =>
          runWithInput<MakeWarehouseDocumentInput>(this.dialogs, {
            titleKey: 'Erp::MakePutAway',
            icon: 'fas fa-arrow-down-short-wide',
            permission: `${WAREHOUSE}.Create`,
            fields: [{ field: 'sourceNo', labelKey: 'Erp::PostedReceiptNo', type: 'text', required: true, maxLength: 20 }],
            submit: input => this.activities.makePutAway(input),
          }),
      },
      {
        key: 'makePick',
        labelKey: 'Erp::MakePick',
        icon: 'fas fa-hand-holding',
        permission: `${WAREHOUSE}.Create`,
        run: () =>
          runWithInput<MakeWarehouseDocumentInput>(this.dialogs, {
            titleKey: 'Erp::MakePick',
            icon: 'fas fa-hand-holding',
            permission: `${WAREHOUSE}.Create`,
            fields: [{ field: 'sourceNo', labelKey: 'Erp::SalesOrderNo', type: 'text', required: true, maxLength: 20 }],
            submit: input => this.activities.makePick(input),
          }),
      },
    ],
    parts: [
      {
        entity: 'warehouseActivityLine',
        lines: dto => this.activityLines.getList({ warehouseActivityHeaderId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ warehouseActivityHeaderId: dto.id, quantity: 1 }),
        columns: ['itemNo', 'variantCode', 'description', 'fromBinCode', 'toBinCode', 'quantity', 'qtyToHandle', 'qtyHandled'],
        totals: ['quantity'],
        editable: dto => dto.status === WarehouseActivityStatus.Open,
      },
    ],
  };

  readonly warehouseActivityLine: RecordEntity<WarehouseActivityLineDto, CreateUpdateWarehouseActivityLineDto> = {
    key: 'warehouseActivityLine',
    titleKey: 'Erp::WarehouseActivityLine',
    pluralKey: 'Erp::WarehouseActivityLines',
    icon: 'fas fa-list',
    permission: WAREHOUSE,
    listRoute: ['/erp/warehouse-activities'],
    columns: [
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 120 },
      { field: 'variantCode', labelKey: 'Erp::VariantCode', width: 100 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'fromBinCode', labelKey: 'Erp::FromBinCode', width: 120 },
      { field: 'toBinCode', labelKey: 'Erp::ToBinCode', width: 120 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'qtyToHandle', labelKey: 'Erp::QtyToHandle', type: 'number', width: 110 },
      { field: 'qtyHandled', labelKey: 'Erp::QtyHandled', type: 'number', width: 110 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('itemNo', 'Erp::ItemNo', 'item', 'general', { required: true, createOnly: true }),
      { field: 'variantCode', labelKey: 'Erp::VariantCode', type: 'text', maxLength: 20, createOnly: true },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, createOnly: true },
      codeField('fromBinCode', 'Erp::FromBinCode', 'bin'),
      codeField('toBinCode', 'Erp::ToBinCode', 'bin'),
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', required: true, min: 0 },
      { field: 'qtyToHandle', labelKey: 'Erp::QtyToHandle', type: 'number', min: 0 },
      readonly('qtyHandled', 'Erp::QtyHandled'),
    ],
    getList: query => this.activityLines.getList(query),
    get: id => this.activityLines.get(id),
    create: input => this.activityLines.create(input),
    update: (id, input) => this.activityLines.update(id, input),
    delete: id => this.activityLines.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.itemNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ quantity: 1 }),
  };

  readonly reservationEntry: RecordEntity<ReservationEntryDto, never> = {
    key: 'reservationEntry',
    titleKey: 'Erp::ReservationEntry',
    pluralKey: 'Erp::ReservationEntries',
    icon: 'fas fa-lock',
    permission: RESERVATIONS,
    listRoute: ['/erp/reservation-entries'],
    readOnly: true,
    columns: [
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 120 },
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 110 },
      { field: 'variantCode', labelKey: 'Erp::VariantCode', width: 100 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'sourceType', labelKey: 'Erp::SourceType', type: 'select', options: enumOptions(reservationSourceTypeOptions, 'ReservationSourceType'), width: 150 },
      { field: 'sourceDocumentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'sourceLineNo', labelKey: 'Erp::LineNo', type: 'number', width: 90 },
      { field: 'shipmentDate', labelKey: 'Erp::ShipmentDate', type: 'date', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('itemNo', 'Erp::ItemNo'), readonly('sourceDocumentNo', 'Erp::DocumentNo'), readonly('quantity', 'Erp::Quantity')],
    getList: query => this.reservations.getList(query),
    get: id => this.reservations.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.itemNo ?? '', name: dto.sourceDocumentNo ?? undefined }),
    newRecord: () => ({}),
    actions: [
      {
        key: 'cancel',
        labelKey: 'Erp::CancelReservation',
        icon: 'fas fa-lock-open',
        permission: `${RESERVATIONS}.Reserve`,
        confirmKey: 'Erp::CancelReservationConfirmation',
        run: dto => this.reservations.cancel(dto.id!),
      },
    ],
    listActions: [
      {
        key: 'reserveSalesOrder',
        labelKey: 'Erp::ReserveSalesOrder',
        icon: 'fas fa-lock',
        permission: `${RESERVATIONS}.Reserve`,
        run: () => this.askDocument('Erp::ReserveSalesOrder', 'Erp::SalesOrderNo', input => this.reservations.reserveSalesOrder(input)),
      },
      {
        key: 'reserveAssemblyOrder',
        labelKey: 'Erp::ReserveAssemblyOrder',
        icon: 'fas fa-lock',
        permission: `${RESERVATIONS}.Reserve`,
        run: () => this.askDocument('Erp::ReserveAssemblyOrder', 'Erp::AssemblyOrderNo', input => this.reservations.reserveAssemblyOrder(input)),
      },
      {
        key: 'cancelDocument',
        labelKey: 'Erp::CancelDocumentReservations',
        icon: 'fas fa-lock-open',
        permission: `${RESERVATIONS}.Reserve`,
        run: () => this.askDocument('Erp::CancelDocumentReservations', 'Erp::DocumentNo', input => this.reservations.cancelDocument(input)),
      },
    ],
  };

  readonly physInvtCountingPeriod = codeTableEntity<PhysInvtCountingPeriodDto, CreateUpdatePhysInvtCountingPeriodDto>(this.countingPeriods, {
    key: 'physInvtCountingPeriod',
    titleKey: 'Erp::PhysInvtCountingPeriod',
    pluralKey: 'Erp::PhysInvtCountingPeriods',
    icon: 'fas fa-calendar-check',
    permission: PHYS,
    route: '/erp/phys-invt-counting-periods',
    columns: [{ field: 'countFrequencyPerYear', labelKey: 'Erp::CountFrequencyPerYear', type: 'number', width: 150 }],
    fields: [{ field: 'countFrequencyPerYear', labelKey: 'Erp::CountFrequencyPerYear', type: 'number', required: true, min: 1 }],
    defaults: { countFrequencyPerYear: 1 },
  });

  readonly physInvtOrder: RecordEntity<PhysInvtOrderDto, CreateUpdatePhysInvtOrderDto> = {
    key: 'physInvtOrder',
    titleKey: 'Erp::PhysInvtOrder',
    pluralKey: 'Erp::PhysInvtOrders',
    icon: 'fas fa-clipboard-check',
    permission: PHYS,
    listRoute: ['/erp/phys-invt-orders'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 110 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'lineCount', labelKey: 'Erp::Lines', type: 'number', width: 80, sortable: false },
      { field: 'recordedLineCount', labelKey: 'Erp::Counted', type: 'number', width: 90, sortable: false },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: physStatuses, width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, placeholderKey: 'Erp::NextFromSeries' },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      codeField('locationCode', 'Erp::LocationCode', 'location', 'general', { required: true }),
      { field: 'orderDate', labelKey: 'Erp::OrderDate', type: 'date' },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date' },
      readonly('status', 'Erp::Status'),
    ],
    getList: query => this.physOrders.getList(query),
    get: id => this.physOrders.get(id),
    create: input => this.physOrders.create(input),
    update: (id, input) => this.physOrders.update(id, input),
    delete: id => this.physOrders.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ orderDate: today(), postingDate: today() }),
    facts: dto => [
      { labelKey: 'Erp::Lines', value: dto.lineCount, type: 'number' },
      { labelKey: 'Erp::Counted', value: dto.recordedLineCount, type: 'number' },
    ],
    actions: [
      {
        key: 'calculate',
        labelKey: 'Erp::CalculateCountLines',
        icon: 'fas fa-calculator',
        permission: `${PHYS}.Update`,
        visible: dto => dto.status === PhysInvtOrderStatus.Open,
        run: dto =>
          runWithInput<CalculatePhysInvtLinesInput>(this.dialogs, {
            titleKey: 'Erp::CalculateCountLines',
            icon: 'fas fa-calculator',
            permission: `${PHYS}.Update`,
            fields: [
              { field: 'onlyItemsDueForCounting', labelKey: 'Erp::OnlyItemsDueForCounting', type: 'checkbox' },
              codeField('itemCategoryCode', 'Erp::ItemCategoryCode', 'itemCategory'),
              { field: 'includeItemsNotOnInventory', labelKey: 'Erp::IncludeItemsNotOnInventory', type: 'checkbox' },
            ],
            values: { onlyItemsDueForCounting: false, includeItemsNotOnInventory: false },
            submit: input => this.physOrders.calculateLines(dto.id!, input),
          }),
      },
      {
        // Posts every counted difference as an adjustment and marks the items counted.
        key: 'finish',
        labelKey: 'Erp::FinishCount',
        icon: 'fas fa-flag-checkered',
        permission: `${PHYS}.Post`,
        confirmKey: 'Erp::PostDocumentConfirmation',
        visible: dto => dto.status === PhysInvtOrderStatus.Open,
        run: dto => this.physOrders.finish(dto.id!),
      },
    ],
    parts: [
      {
        entity: 'physInvtOrderLine',
        lines: dto => this.physLines.getList({ physInvtOrderHeaderId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ physInvtOrderHeaderId: dto.id, qtyRecorded: 0, recorded: false }),
        columns: ['itemNo', 'description', 'variantCode', 'binCode', 'lotNo', 'serialNo', 'qtyExpected', 'qtyRecorded', 'recorded', 'qtyDifference'],
        editable: dto => dto.status === PhysInvtOrderStatus.Open,
      },
    ],
  };

  readonly physInvtOrderLine: RecordEntity<PhysInvtOrderLineDto, CreateUpdatePhysInvtOrderLineDto> = {
    key: 'physInvtOrderLine',
    titleKey: 'Erp::PhysInvtOrderLine',
    pluralKey: 'Erp::PhysInvtOrderLines',
    icon: 'fas fa-list',
    permission: PHYS,
    listRoute: ['/erp/phys-invt-orders'],
    columns: [
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'variantCode', labelKey: 'Erp::VariantCode', width: 100 },
      { field: 'binCode', labelKey: 'Erp::BinCode', width: 100 },
      { field: 'lotNo', labelKey: 'Erp::LotNo', width: 110 },
      { field: 'serialNo', labelKey: 'Erp::SerialNo', width: 110 },
      { field: 'qtyExpected', labelKey: 'Erp::QtyExpected', type: 'number', width: 110 },
      { field: 'qtyRecorded', labelKey: 'Erp::QtyRecorded', type: 'number', width: 110 },
      { field: 'recorded', labelKey: 'Erp::Counted', type: 'boolean', width: 90 },
      { field: 'qtyDifference', labelKey: 'Erp::QtyDifference', type: 'number', width: 110 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('itemNo', 'Erp::ItemNo', 'item', 'general', { required: true, createOnly: true }),
      readonly('description', 'Erp::Description'),
      { field: 'variantCode', labelKey: 'Erp::VariantCode', type: 'text', maxLength: 20, createOnly: true },
      codeField('binCode', 'Erp::BinCode', 'bin', 'general', { createOnly: true }),
      { field: 'lotNo', labelKey: 'Erp::LotNo', type: 'text', maxLength: 20, createOnly: true },
      { field: 'serialNo', labelKey: 'Erp::SerialNo', type: 'text', maxLength: 20, createOnly: true },
      readonly('qtyExpected', 'Erp::QtyExpected'),
      { field: 'qtyRecorded', labelKey: 'Erp::QtyRecorded', type: 'number', min: 0 },
      { field: 'recorded', labelKey: 'Erp::Counted', type: 'checkbox' },
      readonly('qtyDifference', 'Erp::QtyDifference'),
    ],
    getList: query => this.physLines.getList(query),
    get: id => this.physLines.get(id),
    create: input => this.physLines.create(input),
    update: (id, input) => this.physLines.update(id, input),
    delete: id => this.physLines.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.itemNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ qtyRecorded: 0, recorded: false }),
  };

  readonly assemblyBomComponent: RecordEntity<AssemblyBomComponentDto, CreateUpdateAssemblyBomComponentDto> = {
    key: 'assemblyBomComponent',
    titleKey: 'Erp::AssemblyBomComponent',
    pluralKey: 'Erp::AssemblyBomComponents',
    icon: 'fas fa-puzzle-piece',
    permission: ASSEMBLY,
    listRoute: ['/erp/assembly-boms'],
    columns: [
      { field: 'parentItemNo', labelKey: 'Erp::AssembledItemNo', width: 140 },
      { field: 'lineNo', labelKey: 'Erp::LineNo', type: 'number', width: 90 },
      { field: 'componentItemNo', labelKey: 'Erp::ComponentItemNo', width: 140 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantityPer', labelKey: 'Erp::QuantityPer', type: 'number', width: 110 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('parentItemNo', 'Erp::AssembledItemNo', 'item', 'general', { required: true }),
      codeField('componentItemNo', 'Erp::ComponentItemNo', 'item', 'general', { required: true }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      { field: 'quantityPer', labelKey: 'Erp::QuantityPer', type: 'number', required: true, min: 0 },
    ],
    getList: query => this.bom.getList(query),
    get: id => this.bom.get(id),
    create: input => this.bom.create(input),
    update: (id, input) => this.bom.update(id, input),
    delete: id => this.bom.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.componentItemNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ quantityPer: 1 }),
  };

  readonly assemblyOrder: RecordEntity<AssemblyOrderDto, CreateUpdateAssemblyOrderDto> = {
    key: 'assemblyOrder',
    titleKey: 'Erp::AssemblyOrder',
    pluralKey: 'Erp::AssemblyOrders',
    icon: 'fas fa-screwdriver',
    permission: ASSEMBLY,
    listRoute: ['/erp/assembly-orders'],
    chatterEntityType: 'AssemblyHeader',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 110 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'assembledQuantity', labelKey: 'Erp::AssembledQuantity', type: 'number', width: 130 },
      { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date', width: 120 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: assemblyStatuses, width: 100 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'posting', labelKey: 'Erp::Posting' },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, placeholderKey: 'Erp::NextFromSeries' },
      codeField('itemNo', 'Erp::ItemNo', 'item', 'general', { required: true, createOnly: true }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      { field: 'variantCode', labelKey: 'Erp::VariantCode', type: 'text', maxLength: 20 },
      codeField('locationCode', 'Erp::LocationCode', 'location'),
      codeField('binCode', 'Erp::BinCode', 'bin'),
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', required: true, min: 0 },
      { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date' },
      { field: 'quantityToAssemble', labelKey: 'Erp::QuantityToAssemble', type: 'number', min: 0, section: 'posting' },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', section: 'posting' },
      readonly('assembledQuantity', 'Erp::AssembledQuantity', 'posting'),
      readonly('unitCost', 'Erp::UnitCost', 'posting'),
    ],
    getList: query => this.assemblyOrders.getList(query),
    get: id => this.assemblyOrders.get(id),
    create: input => this.assemblyOrders.create(input),
    update: (id, input) => this.assemblyOrders.update(id, input),
    delete: id => this.assemblyOrders.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ quantity: 1, dueDate: today(), postingDate: today() }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: AssemblyOrderStatus[dto.status] },
      { labelKey: 'Erp::RemainingQuantity', value: dto.remainingQuantity, type: 'number' },
      { labelKey: 'Erp::UnitCost', value: dto.unitCost, type: 'currency' },
    ],
    actions: [
      {
        key: 'release',
        labelKey: 'Erp::Release',
        icon: 'fas fa-lock',
        permission: `${ASSEMBLY}.Update`,
        visible: dto => dto.status === AssemblyOrderStatus.Open,
        run: dto => this.assemblyOrders.release(dto.id!),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-lock-open',
        permission: `${ASSEMBLY}.Update`,
        visible: dto => dto.status === AssemblyOrderStatus.Released,
        run: dto => this.assemblyOrders.reopen(dto.id!),
      },
      {
        key: 'refresh',
        labelKey: 'Erp::RefreshComponents',
        icon: 'fas fa-rotate',
        permission: `${ASSEMBLY}.Update`,
        visible: dto => dto.assembledQuantity === 0 && dto.status !== AssemblyOrderStatus.Finished,
        run: dto => this.assemblyOrders.refreshLines(dto.id!),
      },
      {
        key: 'reserve',
        labelKey: 'Erp::ReserveComponents',
        icon: 'fas fa-lock',
        permission: `${RESERVATIONS}.Reserve`,
        visible: dto => dto.status !== AssemblyOrderStatus.Finished,
        run: dto => this.reservations.reserveAssemblyOrder({ documentNo: dto.no! }),
      },
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${ASSEMBLY}.Post`,
        confirmKey: 'Erp::PostDocumentConfirmation',
        visible: dto => dto.status !== AssemblyOrderStatus.Finished,
        run: dto => this.assemblyOrders.runPosting(dto.id!),
      },
    ],
    parts: [
      {
        entity: 'assemblyLine',
        lines: dto => this.assemblyLines.getList({ assemblyHeaderId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        columns: ['itemNo', 'description', 'locationCode', 'quantityPer', 'quantity', 'consumedQuantity', 'remainingQuantity'],
        editable: () => false,
      },
    ],
  };

  readonly assemblyLine: RecordEntity<AssemblyLineDto, never> = {
    key: 'assemblyLine',
    titleKey: 'Erp::AssemblyLine',
    pluralKey: 'Erp::AssemblyLines',
    icon: 'fas fa-list',
    permission: ASSEMBLY,
    listRoute: ['/erp/assembly-orders'],
    readOnly: true,
    columns: [
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'locationCode', labelKey: 'Erp::LocationCode', width: 110 },
      { field: 'quantityPer', labelKey: 'Erp::QuantityPer', type: 'number', width: 100 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'consumedQuantity', labelKey: 'Erp::ConsumedQuantity', type: 'number', width: 120 },
      { field: 'remainingQuantity', labelKey: 'Erp::RemainingQuantity', type: 'number', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('itemNo', 'Erp::ItemNo'), readonly('quantity', 'Erp::Quantity'), readonly('consumedQuantity', 'Erp::ConsumedQuantity')],
    getList: query => this.assemblyLines.getList(query),
    get: id => this.assemblyLines.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.itemNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  get all(): RecordEntity[] {
    return [
      this.bin,
      this.binContent,
      this.warehouseEntry,
      this.warehouseActivity,
      this.warehouseActivityLine,
      this.reservationEntry,
      this.physInvtCountingPeriod,
      this.physInvtOrder,
      this.physInvtOrderLine,
      this.assemblyBomComponent,
      this.assemblyOrder,
      this.assemblyLine,
    ];
  }

  private askDocument(titleKey: string, labelKey: string, submit: (input: ReserveDocumentInput) => Observable<unknown>) {
    return runWithInput<ReserveDocumentInput>(this.dialogs, {
      titleKey,
      icon: 'fas fa-lock',
      permission: `${RESERVATIONS}.Reserve`,
      fields: [{ field: 'documentNo', labelKey, type: 'text', required: true, maxLength: 20 }],
      submit,
    });
  }
}
