import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';

export enum ProjectStatus {
  Planning = 0,
  Quote = 1,
  Open = 2,
  Completed = 3,
}
export const projectStatusOptions = mapEnumToOptions(ProjectStatus);

export enum ProjectWipMethod {
  CostValue = 0,
  SalesValue = 1,
}
export const projectWipMethodOptions = mapEnumToOptions(ProjectWipMethod);

export enum ProjectTaskType {
  Posting = 0,
  Heading = 1,
  Total = 2,
  BeginTotal = 3,
  EndTotal = 4,
}
export const projectTaskTypeOptions = mapEnumToOptions(ProjectTaskType);

export enum ProjectPlanningLineType {
  Budget = 0,
  Billable = 1,
  BothBudgetAndBillable = 2,
}
export const projectPlanningLineTypeOptions = mapEnumToOptions(ProjectPlanningLineType);

export enum ProjectLineType {
  Resource = 0,
  Item = 1,
  GLAccount = 2,
}
export const projectLineTypeOptions = mapEnumToOptions(ProjectLineType);

export enum ProjectLedgerEntryType {
  Usage = 0,
  Sale = 1,
}
export const projectLedgerEntryTypeOptions = mapEnumToOptions(ProjectLedgerEntryType);

export enum ResourceType {
  Person = 0,
  Machine = 1,
}
export const resourceTypeOptions = mapEnumToOptions(ResourceType);

export enum TimeSheetStatus {
  Open = 0,
  Submitted = 1,
  Approved = 2,
  Rejected = 3,
}
export const timeSheetStatusOptions = mapEnumToOptions(TimeSheetStatus);

export interface PagedProjectInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
}

export interface ProjectsSetupDto {
  projectNos?: string;
  resourceNos?: string;
  timeSheetNos?: string;
  defaultPostingGroup?: string;
}

export interface ProjectPostingGroupDto extends CodeTableDto {
  projectCostsAccount?: string;
  projectSalesAccount?: string;
  wipCostsAccount?: string;
  projectCostsAppliedAccount?: string;
  wipSalesAccount?: string;
  recognizedSalesAccount?: string;
}

export interface CreateUpdateProjectPostingGroupDto extends CreateUpdateCodeTableDto {
  projectCostsAccount?: string;
  projectSalesAccount?: string;
  wipCostsAccount?: string;
  projectCostsAppliedAccount?: string;
  wipSalesAccount?: string;
  recognizedSalesAccount?: string;
}

export interface ResourceDto extends FullAuditedEntityDto<string> {
  no?: string;
  name?: string;
  type: ResourceType;
  employeeNo?: string;
  unitOfMeasureCode?: string;
  unitCost: number;
  unitPrice: number;
  capacityPerDay: number;
  blocked: boolean;
}

export interface CreateUpdateResourceDto {
  no?: string;
  name: string;
  type: ResourceType;
  employeeNo?: string;
  unitOfMeasureCode?: string;
  unitCost: number;
  unitPrice: number;
  capacityPerDay: number;
  blocked: boolean;
}

export interface ProjectDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  customerId?: string;
  customerNo?: string;
  customerName?: string;
  status: ProjectStatus;
  postingGroup?: string;
  wipMethod: ProjectWipMethod;
  startingDate?: string;
  endingDate?: string;
  personResponsible?: string;
}

export interface CreateUpdateProjectDto {
  no?: string;
  description: string;
  customerNo?: string;
  status: ProjectStatus;
  postingGroup?: string;
  wipMethod: ProjectWipMethod;
  startingDate?: string;
  endingDate?: string;
  personResponsible?: string;
}

export interface ProjectTaskDto extends EntityDto<string> {
  projectId?: string;
  taskNo?: string;
  description?: string;
  taskType: ProjectTaskType;
  indentation: number;
}

export interface CreateUpdateProjectTaskDto {
  projectId?: string;
  taskNo: string;
  description?: string;
  taskType: ProjectTaskType;
  indentation: number;
}

export interface GetProjectTaskListInput extends PagedProjectInput {
  projectId?: string;
}

export interface ProjectPlanningLineDto extends EntityDto<string> {
  projectNo?: string;
  taskNo?: string;
  lineNo: number;
  lineType: ProjectPlanningLineType;
  type: ProjectLineType;
  no?: string;
  description?: string;
  planningDate?: string;
  quantity: number;
  unitCost: number;
  unitPrice: number;
  totalCost: number;
  totalPrice: number;
  qtyTransferredToInvoice: number;
  invoiceNo?: string;
}

export interface CreateUpdateProjectPlanningLineDto {
  projectNo: string;
  taskNo: string;
  lineType: ProjectPlanningLineType;
  type: ProjectLineType;
  no: string;
  description?: string;
  planningDate?: string;
  quantity: number;
  unitCost?: number;
  unitPrice?: number;
}

export interface GetProjectPlanningLineListInput extends PagedProjectInput {
  projectNo?: string;
}

export interface ProjectTaskStatisticsDto {
  taskNo?: string;
  description?: string;
  taskType: ProjectTaskType;
  indentation: number;
  budgetCost: number;
  budgetPrice: number;
  usageCost: number;
  usagePrice: number;
  invoicedPrice: number;
}

export interface ProjectPostingDateInput {
  postingDate?: string;
}

export interface ProjectWipResultDto {
  wip: number;
}

export interface ProjectInvoiceResultDto {
  invoiceId?: string;
  invoiceNo?: string;
  lineCount: number;
}

export interface ProjectJournalBatchDto extends EntityDto<string> {
  name?: string;
  description?: string;
}

export interface CreateUpdateProjectJournalBatchDto {
  name: string;
  description?: string;
}

export interface ProjectJournalLineDto extends EntityDto<string> {
  projectJournalBatchId?: string;
  lineNo: number;
  postingDate?: string;
  documentNo?: string;
  projectNo?: string;
  taskNo?: string;
  type: ProjectLineType;
  no?: string;
  description?: string;
  quantity: number;
  unitCost?: number;
  unitPrice?: number;
  chargeable: boolean;
  locationCode?: string;
  balAccountNo?: string;
}

export interface CreateUpdateProjectJournalLineDto {
  projectJournalBatchId?: string;
  postingDate?: string;
  documentNo: string;
  projectNo: string;
  taskNo: string;
  type: ProjectLineType;
  no: string;
  description?: string;
  quantity: number;
  unitCost?: number;
  unitPrice?: number;
  chargeable: boolean;
  locationCode?: string;
  balAccountNo?: string;
}

export interface GetProjectJournalLineListInput extends PagedProjectInput {
  projectJournalBatchId?: string;
}

export interface ProjectLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  projectNo?: string;
  taskNo?: string;
  postingDate?: string;
  documentNo?: string;
  entryType: ProjectLedgerEntryType;
  type: ProjectLineType;
  no?: string;
  description?: string;
  quantity: number;
  unitCost: number;
  totalCost: number;
  unitPrice: number;
  totalPrice: number;
  chargeable: boolean;
}

export interface ResourceLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  resourceNo?: string;
  postingDate?: string;
  documentNo?: string;
  projectNo?: string;
  taskNo?: string;
  description?: string;
  quantity: number;
  totalCost: number;
  totalPrice: number;
  chargeable: boolean;
}

export interface GetProjectLedgerListInput extends PagedProjectInput {
  projectNo?: string;
  resourceNo?: string;
}

export interface TimeSheetLineDto extends EntityDto<string> {
  lineNo: number;
  projectNo?: string;
  taskNo?: string;
  description?: string;
  chargeable: boolean;
  days: number[];
  totalHours: number;
  posted: boolean;
}

export interface TimeSheetDto extends EntityDto<string> {
  no?: string;
  resourceNo?: string;
  resourceName?: string;
  startingDate?: string;
  endingDate?: string;
  status: TimeSheetStatus;
  approverComment?: string;
  totalHours: number;
  lines: TimeSheetLineDto[];
}

export interface TimeSheetLineInputDto {
  id?: string;
  projectNo: string;
  taskNo: string;
  description?: string;
  chargeable: boolean;
  days: number[];
}

export interface CreateUpdateTimeSheetDto {
  no?: string;
  resourceNo: string;
  startingDate?: string;
  lines: TimeSheetLineInputDto[];
}

export interface GetTimeSheetListInput extends PagedProjectInput {
  resourceNo?: string;
  status?: TimeSheetStatus;
}
