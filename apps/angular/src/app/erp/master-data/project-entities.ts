import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import {
  CreateUpdateProjectDto,
  CreateUpdateProjectJournalBatchDto,
  CreateUpdateProjectJournalLineDto,
  CreateUpdateProjectPlanningLineDto,
  CreateUpdateProjectPostingGroupDto,
  CreateUpdateProjectTaskDto,
  CreateUpdateResourceDto,
  ProjectDto,
  ProjectJournalBatchDto,
  ProjectJournalBatchService,
  ProjectJournalLineDto,
  ProjectJournalLineService,
  ProjectLedgerEntryDto,
  ProjectLedgerEntryService,
  ProjectLineType,
  ProjectPlanningLineDto,
  ProjectPlanningLineService,
  ProjectPlanningLineType,
  ProjectPostingGroupDto,
  ProjectPostingGroupService,
  ProjectService,
  ProjectStatus,
  ProjectTaskDto,
  ProjectTaskService,
  ProjectTaskType,
  ProjectWipMethod,
  ResourceDto,
  ResourceLedgerEntryDto,
  ResourceLedgerEntryService,
  ResourceService,
  ResourceType,
  TimeSheetDto,
  TimeSheetService,
  TimeSheetStatus,
  projectLedgerEntryTypeOptions,
  projectLineTypeOptions,
  projectPlanningLineTypeOptions,
  projectStatusOptions,
  projectTaskTypeOptions,
  projectWipMethodOptions,
  resourceTypeOptions,
  timeSheetStatusOptions,
} from '@proxy/projects';
import { EMPTY, map } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { accountField, codeField, codeTableEntity, enumOptions } from './entity-helpers';

const PROJECTS = 'Erp.Projects';
const SETUP = 'Erp.ProjectsSetup';
const today = () => new Date().toISOString().substring(0, 10);

function readonly(field: string, labelKey: string): RecordField {
  return { field, labelKey, type: 'readonly' };
}

/** Projects with their tasks and plans, resources, project journals, the ledgers and time sheets. */
@Injectable({ providedIn: 'root' })
export class ProjectEntities {
  private readonly router = inject(Router);
  private readonly projects = inject(ProjectService);
  private readonly tasks = inject(ProjectTaskService);
  private readonly planning = inject(ProjectPlanningLineService);
  private readonly resources = inject(ResourceService);
  private readonly groups = inject(ProjectPostingGroupService);
  private readonly batches = inject(ProjectJournalBatchService);
  private readonly journalLines = inject(ProjectJournalLineService);
  private readonly projectLedger = inject(ProjectLedgerEntryService);
  private readonly resourceLedger = inject(ResourceLedgerEntryService);
  private readonly timeSheets = inject(TimeSheetService);

  readonly project: RecordEntity<ProjectDto, CreateUpdateProjectDto> = {
    key: 'project',
    titleKey: 'Erp::Project',
    pluralKey: 'Erp::Projects',
    icon: 'fas fa-diagram-project',
    permission: PROJECTS,
    listRoute: ['/erp/projects'],
    chatterEntityType: 'Project',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'customerName', labelKey: 'Erp::Customer', width: 180 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(projectStatusOptions, 'ProjectStatus'), width: 120 },
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', width: 120 },
      { field: 'endingDate', labelKey: 'Erp::EndingDate', type: 'date', width: 120 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'posting', labelKey: 'Erp::Posting', collapsed: true },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, placeholderKey: 'Erp::NextFromSeries' },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', required: true, maxLength: 250, wide: true },
      codeField('customerNo', 'Erp::CustomerNo', 'customer'),
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(projectStatusOptions, 'ProjectStatus') },
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date' },
      { field: 'endingDate', labelKey: 'Erp::EndingDate', type: 'date' },
      codeField('personResponsible', 'Erp::PersonResponsible', 'salespersonPurchaser'),
      codeField('postingGroup', 'Erp::ProjectPostingGroup', 'projectPostingGroup', 'posting'),
      { field: 'wipMethod', labelKey: 'Erp::WipMethod', type: 'select', options: enumOptions(projectWipMethodOptions, 'ProjectWipMethod'), section: 'posting' },
    ],
    getList: query => this.projects.getList(query),
    get: id => this.projects.get(id),
    create: input => this.projects.create(input),
    update: (id, input) => this.projects.update(id, input),
    delete: id => this.projects.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: term => ({ description: term ?? '', status: ProjectStatus.Planning, wipMethod: ProjectWipMethod.CostValue, startingDate: today() }),
    actions: [
      {
        key: 'statistics',
        labelKey: 'Erp::ProjectStatistics',
        icon: 'fas fa-chart-column',
        run: dto => {
          void this.router.navigate(['/erp/project-work/statistics', dto.id]);
          return EMPTY;
        },
      },
      {
        key: 'invoice',
        labelKey: 'Erp::MakeSalesInvoice',
        icon: 'fas fa-file-invoice',
        permission: `${PROJECTS}.Post`,
        visible: dto => !!dto.customerNo,
        run: dto => this.projects.makeSalesInvoice(dto.id!, { postingDate: today() }),
      },
      {
        key: 'wip',
        labelKey: 'Erp::CalculateWip',
        icon: 'fas fa-hourglass-half',
        permission: `${PROJECTS}.Post`,
        confirmKey: 'Erp::PostDocumentConfirmation',
        run: dto => this.projects.calculateWip(dto.id!, { postingDate: today() }),
      },
    ],
    parts: [
      {
        entity: 'projectTask',
        lines: dto => this.tasks.getList({ projectId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ projectId: dto.id, taskType: ProjectTaskType.Posting, indentation: 0 }),
        columns: ['taskNo', 'description', 'taskType', 'indentation'],
      },
      {
        entity: 'projectPlanningLine',
        lines: dto => this.planning.getList({ projectNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ projectNo: dto.no, lineType: ProjectPlanningLineType.BothBudgetAndBillable, type: ProjectLineType.Resource, planningDate: today(), quantity: 1 }),
        columns: ['taskNo', 'lineType', 'type', 'no', 'description', 'quantity', 'unitCost', 'unitPrice', 'totalCost', 'totalPrice', 'qtyTransferredToInvoice'],
        totals: ['totalCost', 'totalPrice'],
      },
    ],
  };

  readonly projectTask: RecordEntity<ProjectTaskDto, CreateUpdateProjectTaskDto> = {
    key: 'projectTask',
    titleKey: 'Erp::ProjectTask',
    pluralKey: 'Erp::ProjectTasks',
    icon: 'fas fa-list-ol',
    permission: PROJECTS,
    listRoute: ['/erp/projects'],
    columns: [
      { field: 'taskNo', labelKey: 'Erp::TaskNo', width: 110, indent: true, indentField: 'indentation' },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'taskType', labelKey: 'Erp::TaskType', type: 'select', options: enumOptions(projectTaskTypeOptions, 'ProjectTaskType'), width: 130 },
      { field: 'indentation', labelKey: 'Erp::Indentation', type: 'number', width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'taskNo', labelKey: 'Erp::TaskNo', type: 'text', required: true, maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'taskType', labelKey: 'Erp::TaskType', type: 'select', options: enumOptions(projectTaskTypeOptions, 'ProjectTaskType') },
      { field: 'indentation', labelKey: 'Erp::Indentation', type: 'number', min: 0 },
    ],
    getList: query => this.tasks.getList(query),
    get: id => this.tasks.get(id),
    create: input => this.tasks.create(input),
    update: (id, input) => this.tasks.update(id, input),
    delete: id => this.tasks.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.taskNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ taskType: ProjectTaskType.Posting, indentation: 0 }),
  };

  readonly projectPlanningLine: RecordEntity<ProjectPlanningLineDto, CreateUpdateProjectPlanningLineDto> = {
    key: 'projectPlanningLine',
    titleKey: 'Erp::ProjectPlanningLine',
    pluralKey: 'Erp::ProjectPlanningLines',
    icon: 'fas fa-list',
    permission: PROJECTS,
    listRoute: ['/erp/projects'],
    columns: [
      { field: 'taskNo', labelKey: 'Erp::TaskNo', width: 100 },
      { field: 'lineType', labelKey: 'Erp::LineType', type: 'select', options: enumOptions(projectPlanningLineTypeOptions, 'ProjectPlanningLineType'), width: 160 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(projectLineTypeOptions, 'ProjectLineType'), width: 120 },
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'unitCost', labelKey: 'Erp::UnitCost', type: 'currency', width: 110 },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', width: 110 },
      { field: 'totalCost', labelKey: 'Erp::TotalCost', type: 'currency', width: 120 },
      { field: 'totalPrice', labelKey: 'Erp::TotalPrice', type: 'currency', width: 120 },
      { field: 'qtyTransferredToInvoice', labelKey: 'Erp::QtyTransferredToInvoice', type: 'number', width: 140 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'taskNo', labelKey: 'Erp::TaskNo', type: 'text', required: true, maxLength: 20 },
      { field: 'lineType', labelKey: 'Erp::LineType', type: 'select', options: enumOptions(projectPlanningLineTypeOptions, 'ProjectPlanningLineType') },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(projectLineTypeOptions, 'ProjectLineType') },
      { field: 'no', labelKey: 'Erp::No', type: 'text', required: true, maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'planningDate', labelKey: 'Erp::PlanningDate', type: 'date' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number' },
      { field: 'unitCost', labelKey: 'Erp::UnitCost', type: 'currency' },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency' },
      { field: 'totalCost', labelKey: 'Erp::TotalCost', type: 'readonly' },
      { field: 'totalPrice', labelKey: 'Erp::TotalPrice', type: 'readonly' },
    ],
    getList: query => this.planning.getList(query),
    get: id => this.planning.get(id),
    create: input => this.planning.create(input),
    update: (id, input) => this.planning.update(id, input),
    delete: id => this.planning.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ lineType: ProjectPlanningLineType.BothBudgetAndBillable, type: ProjectLineType.Resource, quantity: 1, planningDate: today() }),
  };

  readonly resource: RecordEntity<ResourceDto, CreateUpdateResourceDto> = {
    key: 'resource',
    titleKey: 'Erp::Resource',
    pluralKey: 'Erp::Resources',
    icon: 'fas fa-user-gear',
    permission: SETUP,
    listRoute: ['/erp/resources'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'name', labelKey: 'Erp::Name' },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(resourceTypeOptions, 'ResourceType'), width: 110 },
      { field: 'unitCost', labelKey: 'Erp::UnitCost', type: 'currency', width: 110 },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', width: 110 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, placeholderKey: 'Erp::NextFromSeries' },
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 100 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(resourceTypeOptions, 'ResourceType') },
      codeField('employeeNo', 'Erp::EmployeeNo', 'employee'),
      codeField('unitOfMeasureCode', 'Erp::UnitOfMeasureCode', 'unitOfMeasure'),
      { field: 'unitCost', labelKey: 'Erp::UnitCost', type: 'currency', min: 0 },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', min: 0 },
      { field: 'capacityPerDay', labelKey: 'Erp::CapacityPerDay', type: 'number', min: 0 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    getList: query => this.resources.getList(query),
    get: id => this.resources.get(id),
    create: input => this.resources.create(input),
    update: (id, input) => this.resources.update(id, input),
    delete: id => this.resources.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.name ?? undefined }),
    newRecord: term => ({ name: term ?? '', type: ResourceType.Person, unitCost: 0, unitPrice: 0, capacityPerDay: 8, blocked: false }),
  };

  readonly projectPostingGroup = codeTableEntity<ProjectPostingGroupDto, CreateUpdateProjectPostingGroupDto>(this.groups, {
    key: 'projectPostingGroup',
    titleKey: 'Erp::ProjectPostingGroup',
    pluralKey: 'Erp::ProjectPostingGroups',
    icon: 'fas fa-tags',
    permission: SETUP,
    route: '/erp/project-posting-groups',
    fields: [
      accountField('projectCostsAccount', 'Erp::ProjectCostsAccount', 'general'),
      accountField('projectSalesAccount', 'Erp::ProjectSalesAccount', 'general'),
      accountField('wipCostsAccount', 'Erp::WipCostsAccount', 'general'),
      accountField('projectCostsAppliedAccount', 'Erp::ProjectCostsAppliedAccount', 'general'),
      accountField('wipSalesAccount', 'Erp::WipSalesAccount', 'general'),
      accountField('recognizedSalesAccount', 'Erp::RecognizedSalesAccount', 'general'),
    ],
    quickCreate: false,
  });

  readonly projectJournalBatch: RecordEntity<ProjectJournalBatchDto, CreateUpdateProjectJournalBatchDto> = {
    key: 'projectJournalBatch',
    titleKey: 'Erp::ProjectJournalBatch',
    pluralKey: 'Erp::ProjectJournals',
    icon: 'fas fa-book',
    permission: PROJECTS,
    listRoute: ['/erp/project-journals'],
    columns: [
      { field: 'name', labelKey: 'Erp::Name', width: 150 },
      { field: 'description', labelKey: 'Erp::Description' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 10 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
    ],
    getList: query => this.batches.getList(query),
    get: id => this.batches.get(id),
    create: input => this.batches.create(input),
    update: (id, input) => this.batches.update(id, input),
    delete: id => this.batches.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.name ?? '', name: dto.description ?? undefined }),
    newRecord: term => ({ name: (term ?? '').toUpperCase() }),
    actions: [
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${PROJECTS}.Post`,
        confirmKey: 'Erp::PostDocumentConfirmation',
        run: dto => this.batches.runPosting(dto.id!),
      },
    ],
    parts: [
      {
        entity: 'projectJournalLine',
        lines: dto => this.journalLines.getList({ projectJournalBatchId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ projectJournalBatchId: dto.id, postingDate: today(), documentNo: `${dto.name ?? 'PJ'}-${today()}`, type: ProjectLineType.Resource, quantity: 1, chargeable: true }),
        columns: ['postingDate', 'documentNo', 'projectNo', 'taskNo', 'type', 'no', 'description', 'quantity', 'unitCost', 'unitPrice', 'chargeable'],
      },
    ],
  };

  readonly projectJournalLine: RecordEntity<ProjectJournalLineDto, CreateUpdateProjectJournalLineDto> = {
    key: 'projectJournalLine',
    titleKey: 'Erp::ProjectJournalLine',
    pluralKey: 'Erp::ProjectJournalLines',
    icon: 'fas fa-list',
    permission: PROJECTS,
    listRoute: ['/erp/project-journals'],
    columns: [
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'projectNo', labelKey: 'Erp::ProjectNo', width: 110 },
      { field: 'taskNo', labelKey: 'Erp::TaskNo', width: 90 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(projectLineTypeOptions, 'ProjectLineType'), width: 120 },
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'unitCost', labelKey: 'Erp::UnitCost', type: 'currency', width: 110 },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', width: 110 },
      { field: 'chargeable', labelKey: 'Erp::Chargeable', type: 'boolean', width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', type: 'text', required: true, maxLength: 20 },
      codeField('projectNo', 'Erp::ProjectNo', 'project', 'general', { required: true }),
      { field: 'taskNo', labelKey: 'Erp::TaskNo', type: 'text', required: true, maxLength: 20 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(projectLineTypeOptions, 'ProjectLineType') },
      { field: 'no', labelKey: 'Erp::No', type: 'text', required: true, maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number' },
      { field: 'unitCost', labelKey: 'Erp::UnitCost', type: 'currency' },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency' },
      { field: 'chargeable', labelKey: 'Erp::Chargeable', type: 'checkbox' },
      codeField('locationCode', 'Erp::LocationCode', 'location'),
      accountField('balAccountNo', 'Erp::BalAccountNo', 'general'),
    ],
    getList: query => this.journalLines.getList(query),
    get: id => this.journalLines.get(id),
    create: input => this.journalLines.create(input),
    update: (id, input) => this.journalLines.update(id, input),
    delete: id => this.journalLines.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ postingDate: today(), type: ProjectLineType.Resource, quantity: 1, chargeable: true }),
  };

  readonly projectLedgerEntry: RecordEntity<ProjectLedgerEntryDto, never> = {
    key: 'projectLedgerEntry',
    titleKey: 'Erp::ProjectLedgerEntry',
    pluralKey: 'Erp::ProjectLedgerEntries',
    icon: 'fas fa-receipt',
    permission: PROJECTS,
    listRoute: ['/erp/project-ledger-entries'],
    readOnly: true,
    columns: [
      { field: 'entryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 90 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'projectNo', labelKey: 'Erp::ProjectNo', width: 110 },
      { field: 'taskNo', labelKey: 'Erp::TaskNo', width: 90 },
      { field: 'entryType', labelKey: 'Erp::EntryType', type: 'select', options: enumOptions(projectLedgerEntryTypeOptions, 'ProjectLedgerEntryType'), width: 110 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(projectLineTypeOptions, 'ProjectLineType'), width: 110 },
      { field: 'no', labelKey: 'Erp::No', width: 100 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'totalCost', labelKey: 'Erp::TotalCost', type: 'currency', width: 120 },
      { field: 'totalPrice', labelKey: 'Erp::TotalPrice', type: 'currency', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('projectNo', 'Erp::ProjectNo'), readonly('taskNo', 'Erp::TaskNo'), readonly('description', 'Erp::Description'), readonly('totalCost', 'Erp::TotalCost'), readonly('totalPrice', 'Erp::TotalPrice')],
    getList: query => this.projectLedger.getList(query),
    get: id => this.projectLedger.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.entryNo), name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  readonly resourceLedgerEntry: RecordEntity<ResourceLedgerEntryDto, never> = {
    key: 'resourceLedgerEntry',
    titleKey: 'Erp::ResourceLedgerEntry',
    pluralKey: 'Erp::ResourceLedgerEntries',
    icon: 'fas fa-user-clock',
    permission: PROJECTS,
    listRoute: ['/erp/resource-ledger-entries'],
    readOnly: true,
    columns: [
      { field: 'entryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 90 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'resourceNo', labelKey: 'Erp::ResourceNo', width: 110 },
      { field: 'projectNo', labelKey: 'Erp::ProjectNo', width: 110 },
      { field: 'taskNo', labelKey: 'Erp::TaskNo', width: 90 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'totalCost', labelKey: 'Erp::TotalCost', type: 'currency', width: 120 },
      { field: 'totalPrice', labelKey: 'Erp::TotalPrice', type: 'currency', width: 120 },
      { field: 'chargeable', labelKey: 'Erp::Chargeable', type: 'boolean', width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('resourceNo', 'Erp::ResourceNo'), readonly('projectNo', 'Erp::ProjectNo'), readonly('quantity', 'Erp::Quantity')],
    getList: query => this.resourceLedger.getList(query),
    get: id => this.resourceLedger.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.entryNo), name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  readonly timeSheet: RecordEntity<TimeSheetDto, never> = {
    key: 'timeSheet',
    titleKey: 'Erp::TimeSheet',
    pluralKey: 'Erp::TimeSheets',
    icon: 'fas fa-calendar-week',
    permission: 'Erp.TimeSheets',
    listRoute: ['/erp/time-sheets'],
    readOnly: true,
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'resourceNo', labelKey: 'Erp::ResourceNo', width: 110 },
      { field: 'resourceName', labelKey: 'Erp::Name' },
      { field: 'startingDate', labelKey: 'Erp::Week', type: 'date', width: 120 },
      { field: 'totalHours', labelKey: 'Erp::TotalHours', type: 'number', width: 110, sortable: false, filterable: false },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(timeSheetStatusOptions, 'TimeSheetStatus'), width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('no', 'Erp::No'), readonly('resourceNo', 'Erp::ResourceNo'), readonly('startingDate', 'Erp::Week'), readonly('totalHours', 'Erp::TotalHours')],
    getList: query => this.timeSheets.getList(query),
    get: id => this.timeSheets.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: id => this.timeSheets.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.resourceName ?? undefined }),
    newRecord: () => ({}),
    facts: dto => [
      { labelKey: 'Erp::Status', value: TimeSheetStatus[dto.status] },
      { labelKey: 'Erp::TotalHours', value: dto.totalHours, type: 'number' },
    ],
    listActions: [
      {
        key: 'new',
        labelKey: 'Erp::NewTimeSheet',
        icon: 'fas fa-plus',
        permission: 'Erp.TimeSheets.Create',
        run: () => {
          void this.router.navigate(['/erp/project-work/time-sheet', 'new']);
          return EMPTY;
        },
      },
    ],
    actions: [
      {
        key: 'open',
        labelKey: 'Erp::OpenWeek',
        icon: 'fas fa-calendar-week',
        run: dto => {
          void this.router.navigate(['/erp/project-work/time-sheet', dto.id]);
          return EMPTY;
        },
      },
    ],
  };

  get all(): RecordEntity[] {
    return [
      this.project,
      this.projectTask,
      this.projectPlanningLine,
      this.resource,
      this.projectPostingGroup,
      this.projectJournalBatch,
      this.projectJournalLine,
      this.projectLedgerEntry,
      this.resourceLedgerEntry,
      this.timeSheet,
    ];
  }
}
