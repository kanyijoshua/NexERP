import { Injectable, inject } from '@angular/core';
import {
  CapacityLedgerEntryDto,
  CapacityLedgerEntryService,
  CreateUpdateMachineCenterDto,
  CreateUpdateProductionBomDto,
  CreateUpdateProductionBomLineDto,
  CreateUpdateProductionOrderDto,
  CreateUpdateRoutingDto,
  CreateUpdateRoutingLineDto,
  CreateUpdateWorkCenterDto,
  MachineCenterDto,
  MachineCenterService,
  PlanningLineDto,
  PlanningService,
  ProdOrderComponentDto,
  ProdOrderRoutingLineDto,
  ProductionBomDto,
  ProductionBomLineDto,
  ProductionBomLineService,
  ProductionBomLineType,
  ProductionBomService,
  ProductionBomStatus,
  ProductionOrderDto,
  ProductionOrderService,
  ProductionOrderStatus,
  RoutingDto,
  RoutingLineDto,
  RoutingLineService,
  RoutingLineType,
  RoutingService,
  UpdatePlanningLineDto,
  WorkCenterDto,
  WorkCenterService,
  productionBomLineTypeOptions,
  productionBomStatusOptions,
  productionOrderStatusOptions,
  replenishmentSystemOptions,
  routingLineTypeOptions,
} from '@proxy/manufacturing';
import { EMPTY, map, of } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { codeField, codeTableEntity, enumOptions } from './entity-helpers';

const MANUFACTURING = 'Erp.Manufacturing';
const SETUP = 'Erp.ManufacturingSetup';
const today = () => new Date().toISOString().substring(0, 10);

function readonly(field: string, labelKey: string): RecordField {
  return { field, labelKey, type: 'readonly' };
}

/** Lines that live inside their production order: shown on its card, never edited on their own. */
function orderLines<TDto extends { id?: string }>(key: string, titleKey: string, pluralKey: string, columns: RecordEntity['columns']): RecordEntity<TDto, never> {
  return {
    key,
    titleKey,
    pluralKey,
    icon: 'fas fa-list',
    permission: MANUFACTURING,
    listRoute: ['/erp/production-orders'],
    readOnly: true,
    columns,
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: columns.map(c => readonly(c.field, c.labelKey)),
    getList: () => of({ totalCount: 0, items: [] }),
    get: () => EMPTY,
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.id ?? '' }),
    newRecord: () => ({}),
  };
}

/** Work and machine centers, production BOMs and routings, production orders, the capacity ledger and the plan. */
@Injectable({ providedIn: 'root' })
export class ManufacturingEntities {
  private readonly workCenters = inject(WorkCenterService);
  private readonly machineCenters = inject(MachineCenterService);
  private readonly boms = inject(ProductionBomService);
  private readonly bomLines = inject(ProductionBomLineService);
  private readonly routings = inject(RoutingService);
  private readonly routingLines = inject(RoutingLineService);
  private readonly orders = inject(ProductionOrderService);
  private readonly capacityLedger = inject(CapacityLedgerEntryService);
  private readonly planning = inject(PlanningService);

  readonly workCenter = codeTableEntity<WorkCenterDto, CreateUpdateWorkCenterDto>(this.workCenters, {
    key: 'workCenter',
    titleKey: 'Erp::WorkCenter',
    pluralKey: 'Erp::WorkCenters',
    icon: 'fas fa-warehouse',
    permission: SETUP,
    route: '/erp/work-centers',
    columns: [
      { field: 'code', labelKey: 'Erp::Code', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'unitCostPerHour', labelKey: 'Erp::UnitCostPerHour', type: 'currency', width: 130 },
      { field: 'capacityHoursPerDay', labelKey: 'Erp::CapacityHoursPerDay', type: 'number', width: 130 },
    ],
    fields: [
      { field: 'unitCostPerHour', labelKey: 'Erp::UnitCostPerHour', type: 'currency', min: 0, section: 'general' },
      { field: 'capacityHoursPerDay', labelKey: 'Erp::CapacityHoursPerDay', type: 'number', min: 0, section: 'general' },
    ],
    defaults: { unitCostPerHour: 0, capacityHoursPerDay: 8 },
  });

  readonly machineCenter = codeTableEntity<MachineCenterDto, CreateUpdateMachineCenterDto>(this.machineCenters, {
    key: 'machineCenter',
    titleKey: 'Erp::MachineCenter',
    pluralKey: 'Erp::MachineCenters',
    icon: 'fas fa-gears',
    permission: SETUP,
    route: '/erp/machine-centers',
    columns: [
      { field: 'code', labelKey: 'Erp::Code', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'workCenterCode', labelKey: 'Erp::WorkCenter', width: 130 },
      { field: 'unitCostPerHour', labelKey: 'Erp::UnitCostPerHour', type: 'currency', width: 130 },
    ],
    fields: [
      codeField('workCenterCode', 'Erp::WorkCenter', 'workCenter', 'general', { required: true }),
      { field: 'unitCostPerHour', labelKey: 'Erp::UnitCostPerHour', type: 'currency', min: 0, section: 'general' },
    ],
    defaults: { unitCostPerHour: 0 },
    quickCreate: false,
  });

  readonly productionBom: RecordEntity<ProductionBomDto, CreateUpdateProductionBomDto> = {
    key: 'productionBom',
    titleKey: 'Erp::ProductionBom',
    pluralKey: 'Erp::ProductionBoms',
    icon: 'fas fa-sitemap',
    permission: MANUFACTURING,
    listRoute: ['/erp/production-boms'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 130 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(productionBomStatusOptions, 'ProductionBomStatus'), width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', required: true, maxLength: 20, createOnly: true },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(productionBomStatusOptions, 'ProductionBomStatus') },
    ],
    getList: query => this.boms.getList(query),
    get: id => this.boms.get(id),
    create: input => this.boms.create(input),
    update: (id, input) => this.boms.update(id, input),
    delete: id => this.boms.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: term => ({ no: (term ?? '').toUpperCase(), status: ProductionBomStatus.New }),
    parts: [
      {
        entity: 'productionBomLine',
        lines: dto => this.bomLines.getList({ productionBomId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ productionBomId: dto.id, type: ProductionBomLineType.Item, quantityPer: 1, scrapPct: 0 }),
        columns: ['type', 'no', 'description', 'quantityPer', 'scrapPct'],
        editable: dto => dto.status === ProductionBomStatus.New,
      },
    ],
  };

  readonly productionBomLine: RecordEntity<ProductionBomLineDto, CreateUpdateProductionBomLineDto> = {
    key: 'productionBomLine',
    titleKey: 'Erp::ProductionBomLine',
    pluralKey: 'Erp::ProductionBomLines',
    icon: 'fas fa-list',
    permission: MANUFACTURING,
    listRoute: ['/erp/production-boms'],
    columns: [
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(productionBomLineTypeOptions, 'ProductionBomLineType'), width: 140 },
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantityPer', labelKey: 'Erp::QuantityPer', type: 'number', width: 110 },
      { field: 'scrapPct', labelKey: 'Erp::ScrapPct', type: 'number', width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(productionBomLineTypeOptions, 'ProductionBomLineType') },
      { field: 'no', labelKey: 'Erp::No', type: 'text', required: true, maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'quantityPer', labelKey: 'Erp::QuantityPer', type: 'number', min: 0 },
      { field: 'scrapPct', labelKey: 'Erp::ScrapPct', type: 'number', min: 0 },
    ],
    getList: query => this.bomLines.getList(query),
    get: id => this.bomLines.get(id),
    create: input => this.bomLines.create(input),
    update: (id, input) => this.bomLines.update(id, input),
    delete: id => this.bomLines.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ type: ProductionBomLineType.Item, quantityPer: 1, scrapPct: 0 }),
  };

  readonly routing: RecordEntity<RoutingDto, CreateUpdateRoutingDto> = {
    key: 'routing',
    titleKey: 'Erp::Routing',
    pluralKey: 'Erp::Routings',
    icon: 'fas fa-route',
    permission: MANUFACTURING,
    listRoute: ['/erp/routings'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 130 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(productionBomStatusOptions, 'ProductionBomStatus'), width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', required: true, maxLength: 20, createOnly: true },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(productionBomStatusOptions, 'ProductionBomStatus') },
    ],
    getList: query => this.routings.getList(query),
    get: id => this.routings.get(id),
    create: input => this.routings.create(input),
    update: (id, input) => this.routings.update(id, input),
    delete: id => this.routings.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: term => ({ no: (term ?? '').toUpperCase(), status: ProductionBomStatus.New }),
    parts: [
      {
        entity: 'routingLine',
        lines: dto => this.routingLines.getList({ routingId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ routingId: dto.id, type: RoutingLineType.WorkCenter, setupTime: 0, runTime: 0 }),
        columns: ['operationNo', 'type', 'no', 'description', 'setupTime', 'runTime'],
        editable: dto => dto.status === ProductionBomStatus.New,
      },
    ],
  };

  readonly routingLine: RecordEntity<RoutingLineDto, CreateUpdateRoutingLineDto> = {
    key: 'routingLine',
    titleKey: 'Erp::RoutingLine',
    pluralKey: 'Erp::RoutingLines',
    icon: 'fas fa-list',
    permission: MANUFACTURING,
    listRoute: ['/erp/routings'],
    columns: [
      { field: 'operationNo', labelKey: 'Erp::OperationNo', width: 110 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(routingLineTypeOptions, 'RoutingLineType'), width: 140 },
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'setupTime', labelKey: 'Erp::SetupTimeMinutes', type: 'number', width: 120 },
      { field: 'runTime', labelKey: 'Erp::RunTimeMinutes', type: 'number', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'operationNo', labelKey: 'Erp::OperationNo', type: 'text', required: true, maxLength: 10 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(routingLineTypeOptions, 'RoutingLineType') },
      { field: 'no', labelKey: 'Erp::No', type: 'text', required: true, maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'setupTime', labelKey: 'Erp::SetupTimeMinutes', type: 'number', min: 0 },
      { field: 'runTime', labelKey: 'Erp::RunTimeMinutes', type: 'number', min: 0 },
    ],
    getList: query => this.routingLines.getList(query),
    get: id => this.routingLines.get(id),
    create: input => this.routingLines.create(input),
    update: (id, input) => this.routingLines.update(id, input),
    delete: id => this.routingLines.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.operationNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ type: RoutingLineType.WorkCenter, setupTime: 0, runTime: 0 }),
  };

  readonly productionOrder: RecordEntity<ProductionOrderDto, CreateUpdateProductionOrderDto> = {
    key: 'productionOrder',
    titleKey: 'Erp::ProductionOrder',
    pluralKey: 'Erp::ProductionOrders',
    icon: 'fas fa-industry',
    permission: MANUFACTURING,
    listRoute: ['/erp/production-orders'],
    chatterEntityType: 'ProductionOrder',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(productionOrderStatusOptions, 'ProductionOrderStatus'), width: 120 },
      { field: 'sourceItemNo', labelKey: 'Erp::ItemNo', width: 110 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'finishedQuantity', labelKey: 'Erp::FinishedQuantity', type: 'number', width: 120 },
      { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date', width: 120 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'costs', labelKey: 'Erp::Costs', collapsed: true },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, placeholderKey: 'Erp::NextFromSeries' },
      codeField('sourceItemNo', 'Erp::ItemNo', 'item', 'general', { required: true }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(productionOrderStatusOptions, 'ProductionOrderStatus'), createOnly: true },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', required: true, min: 0 },
      { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date', required: true },
      codeField('locationCode', 'Erp::LocationCode', 'location'),
      { field: 'finishedQuantity', labelKey: 'Erp::FinishedQuantity', type: 'readonly' },
      { field: 'materialCost', labelKey: 'Erp::MaterialCost', type: 'readonly', section: 'costs' },
      { field: 'capacityCost', labelKey: 'Erp::CapacityCost', type: 'readonly', section: 'costs' },
      { field: 'outputCost', labelKey: 'Erp::OutputCost', type: 'readonly', section: 'costs' },
      { field: 'varianceCost', labelKey: 'Erp::VarianceCost', type: 'readonly', section: 'costs' },
    ],
    getList: query => this.orders.getList(query),
    get: id => this.orders.get(id),
    create: input => this.orders.create(input),
    update: (id, input) => this.orders.update(id, input),
    delete: id => this.orders.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ status: ProductionOrderStatus.FirmPlanned, quantity: 1, dueDate: today() }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: ProductionOrderStatus[dto.status] },
      { labelKey: 'Erp::MaterialCost', value: dto.materialCost, type: 'currency' },
      { labelKey: 'Erp::CapacityCost', value: dto.capacityCost, type: 'currency' },
      { labelKey: 'Erp::Wip', value: dto.wip, type: 'currency' },
    ],
    related: dto =>
      of([
        {
          labelKey: 'Erp::CapacityLedgerEntries',
          icon: 'fas fa-stopwatch',
          routerLink: ['/erp/capacity-ledger-entries'],
          queryParams: { filter: dto.no },
          permission: MANUFACTURING,
        },
      ]),
    actions: [
      {
        key: 'refresh',
        labelKey: 'Erp::RefreshProductionOrder',
        icon: 'fas fa-rotate',
        permission: `${MANUFACTURING}.Update`,
        visible: dto => dto.status !== ProductionOrderStatus.Finished && dto.finishedQuantity === 0,
        run: dto => this.orders.refresh(dto.id!),
      },
      {
        key: 'release',
        labelKey: 'Erp::ReleaseProductionOrder',
        icon: 'fas fa-play',
        permission: `${MANUFACTURING}.Update`,
        visible: dto => dto.status < ProductionOrderStatus.Released,
        run: dto => this.orders.changeStatus(dto.id!, { status: ProductionOrderStatus.Released }),
      },
      {
        // Consumes what is still expected of every component (backward flushing).
        key: 'consume',
        labelKey: 'Erp::PostConsumption',
        icon: 'fas fa-dolly',
        permission: `${MANUFACTURING}.Post`,
        confirmKey: 'Erp::PostDocumentConfirmation',
        visible: dto => dto.status === ProductionOrderStatus.Released && (dto.components ?? []).some(c => c.remainingQuantity > 0),
        run: dto =>
          this.orders.consume(dto.id!, {
            postingDate: today(),
            lines: (dto.components ?? []).filter(c => c.remainingQuantity > 0).map(c => ({ componentId: c.id!, quantity: c.remainingQuantity })),
          }),
      },
      {
        // Reports the rest of the order as output, with the minutes each operation still expects.
        key: 'output',
        labelKey: 'Erp::PostOutput',
        icon: 'fas fa-box-open',
        permission: `${MANUFACTURING}.Post`,
        confirmKey: 'Erp::PostDocumentConfirmation',
        visible: dto => dto.status === ProductionOrderStatus.Released && dto.finishedQuantity < dto.quantity,
        run: dto =>
          this.orders.reportOutput(dto.id!, {
            postingDate: today(),
            quantity: dto.quantity - dto.finishedQuantity,
            capacity: (dto.routingLines ?? [])
              .filter(r => r.expectedMinutes > r.actualMinutes)
              .map(r => ({ operationNo: r.operationNo!, minutes: r.expectedMinutes - r.actualMinutes })),
          }),
      },
      {
        key: 'finish',
        labelKey: 'Erp::FinishProductionOrder',
        icon: 'fas fa-flag-checkered',
        permission: `${MANUFACTURING}.Post`,
        confirmKey: 'Erp::PostDocumentConfirmation',
        visible: dto => dto.status === ProductionOrderStatus.Released,
        run: dto => this.orders.finish(dto.id!, { postingDate: today() }),
      },
    ],
    parts: [
      {
        entity: 'prodOrderComponent',
        lines: dto => of(dto.components ?? []),
        columns: ['itemNo', 'description', 'quantityPer', 'expectedQuantity', 'actConsumption', 'remainingQuantity', 'costAmount'],
        totals: ['costAmount'],
        editable: () => false,
      },
      {
        entity: 'prodOrderRoutingLine',
        lines: dto => of(dto.routingLines ?? []),
        columns: ['operationNo', 'type', 'no', 'description', 'expectedMinutes', 'actualMinutes', 'costAmount'],
        totals: ['costAmount'],
        editable: () => false,
      },
    ],
  };

  readonly prodOrderComponent = orderLines<ProdOrderComponentDto>('prodOrderComponent', 'Erp::ProdOrderComponent', 'Erp::ProdOrderComponents', [
    { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 110 },
    { field: 'description', labelKey: 'Erp::Description' },
    { field: 'quantityPer', labelKey: 'Erp::QuantityPer', type: 'number', width: 100 },
    { field: 'expectedQuantity', labelKey: 'Erp::ExpectedQuantity', type: 'number', width: 120 },
    { field: 'actConsumption', labelKey: 'Erp::ActConsumption', type: 'number', width: 120 },
    { field: 'remainingQuantity', labelKey: 'Erp::RemainingQuantity', type: 'number', width: 120 },
    { field: 'costAmount', labelKey: 'Erp::CostAmount', type: 'currency', width: 120 },
  ]);

  readonly prodOrderRoutingLine = orderLines<ProdOrderRoutingLineDto>('prodOrderRoutingLine', 'Erp::ProdOrderRoutingLine', 'Erp::ProdOrderRoutingLines', [
    { field: 'operationNo', labelKey: 'Erp::OperationNo', width: 100 },
    { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(routingLineTypeOptions, 'RoutingLineType'), width: 130 },
    { field: 'no', labelKey: 'Erp::No', width: 110 },
    { field: 'description', labelKey: 'Erp::Description' },
    { field: 'expectedMinutes', labelKey: 'Erp::ExpectedMinutes', type: 'number', width: 120 },
    { field: 'actualMinutes', labelKey: 'Erp::ActualMinutes', type: 'number', width: 120 },
    { field: 'costAmount', labelKey: 'Erp::CostAmount', type: 'currency', width: 120 },
  ]);

  readonly capacityLedgerEntry: RecordEntity<CapacityLedgerEntryDto, never> = {
    key: 'capacityLedgerEntry',
    titleKey: 'Erp::CapacityLedgerEntry',
    pluralKey: 'Erp::CapacityLedgerEntries',
    icon: 'fas fa-stopwatch',
    permission: MANUFACTURING,
    listRoute: ['/erp/capacity-ledger-entries'],
    readOnly: true,
    columns: [
      { field: 'entryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 90 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'productionOrderNo', labelKey: 'Erp::ProductionOrderNo', width: 130 },
      { field: 'operationNo', labelKey: 'Erp::OperationNo', width: 100 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(routingLineTypeOptions, 'RoutingLineType'), width: 130 },
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 110 },
      { field: 'outputQuantity', labelKey: 'Erp::OutputQuantity', type: 'number', width: 110 },
      { field: 'minutes', labelKey: 'Erp::Minutes', type: 'number', width: 100 },
      { field: 'costAmount', labelKey: 'Erp::CostAmount', type: 'currency', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('productionOrderNo', 'Erp::ProductionOrderNo'), readonly('operationNo', 'Erp::OperationNo'), readonly('minutes', 'Erp::Minutes'), readonly('costAmount', 'Erp::CostAmount')],
    getList: query => this.capacityLedger.getList(query),
    get: id => this.capacityLedger.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.entryNo), name: dto.productionOrderNo ?? undefined }),
    newRecord: () => ({}),
  };

  readonly planningLine: RecordEntity<PlanningLineDto, UpdatePlanningLineDto> = {
    key: 'planningLine',
    titleKey: 'Erp::PlanningLine',
    pluralKey: 'Erp::PlanningWorksheet',
    icon: 'fas fa-clipboard-list',
    permission: MANUFACTURING,
    listRoute: ['/erp/planning-worksheet'],
    noCreate: true,
    columns: [
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 110 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'replenishmentSystem', labelKey: 'Erp::ReplenishmentSystem', type: 'select', options: enumOptions(replenishmentSystemOptions, 'ReplenishmentSystem'), width: 140 },
      { field: 'vendorNo', labelKey: 'Erp::VendorNo', width: 110 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date', width: 120 },
      { field: 'acceptActionMessage', labelKey: 'Erp::AcceptActionMessage', type: 'boolean', width: 110 },
      { field: 'carriedOutNo', labelKey: 'Erp::CarriedOutNo', width: 130 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('itemNo', 'Erp::ItemNo'),
      readonly('description', 'Erp::Description'),
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', min: 0 },
      { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date' },
      codeField('vendorNo', 'Erp::VendorNo', 'vendor'),
      { field: 'acceptActionMessage', labelKey: 'Erp::AcceptActionMessage', type: 'checkbox' },
    ],
    getList: query => this.planning.getList(query),
    get: id => this.planning.get(id),
    create: () => EMPTY,
    update: (id, input) => this.planning.update(id, input),
    delete: id => this.planning.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.itemNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({}),
    listActions: [
      {
        key: 'calculate',
        labelKey: 'Erp::CalculatePlan',
        icon: 'fas fa-calculator',
        permission: `${MANUFACTURING}.Update`,
        run: () => this.planning.calculate({}),
      },
      {
        key: 'carryOut',
        labelKey: 'Erp::CarryOutActionMessages',
        icon: 'fas fa-person-running',
        permission: `${MANUFACTURING}.Post`,
        confirmKey: 'Erp::CarryOutConfirmation',
        run: () => this.planning.carryOut(),
      },
    ],
  };

  get all(): RecordEntity[] {
    return [
      this.workCenter,
      this.machineCenter,
      this.productionBom,
      this.productionBomLine,
      this.routing,
      this.routingLine,
      this.productionOrder,
      this.prodOrderComponent,
      this.prodOrderRoutingLine,
      this.capacityLedgerEntry,
      this.planningLine,
    ];
  }
}
