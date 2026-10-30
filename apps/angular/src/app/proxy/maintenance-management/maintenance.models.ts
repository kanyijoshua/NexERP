import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { ErpPagedListInput } from '../cost-accounting/cost-accounting.models';
import type { FullAuditedEntityDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';

export enum EquipmentStatus {
  Active = 0,
  Scrapped = 1,
}
export const equipmentStatusOptions = mapEnumToOptions(EquipmentStatus);

export enum MaintenanceType {
  Corrective = 0,
  Preventive = 1,
}
export const maintenanceTypeOptions = mapEnumToOptions(MaintenanceType);

export enum MaintenancePriority {
  Low = 0,
  Normal = 1,
  High = 2,
}
export const maintenancePriorityOptions = mapEnumToOptions(MaintenancePriority);

export enum MaintenanceStage {
  New = 0,
  InProgress = 1,
  Repaired = 2,
  Scrapped = 3,
}
export const maintenanceStageOptions = mapEnumToOptions(MaintenanceStage);

export interface MaintenanceTeamDto extends CodeTableDto {
}

export interface CreateUpdateMaintenanceTeamDto extends CreateUpdateCodeTableDto {
}

export interface MaintenanceEquipmentDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  category?: string;
  serialNo?: string;
  fixedAssetNo?: string;
  locationCode?: string;
  teamCode?: string;
  technicianEmployeeNo?: string;
  preventiveIntervalDays: number;
  nextPreventiveDate?: string;
  lastMaintenanceDate?: string;
  status: EquipmentStatus;
}

export interface CreateUpdateMaintenanceEquipmentDto {
  no?: string;
  description: string;
  category?: string;
  serialNo?: string;
  fixedAssetNo?: string;
  locationCode?: string;
  teamCode?: string;
  technicianEmployeeNo?: string;
  preventiveIntervalDays: number;
  nextPreventiveDate?: string;
}

export interface GetMaintenanceListInput extends ErpPagedListInput {
  filter?: string;
  equipmentNo?: string;
}

export interface MaintenanceRequestDto extends FullAuditedEntityDto<string> {
  no?: string;
  equipmentNo?: string;
  subject?: string;
  type: MaintenanceType;
  priority: MaintenancePriority;
  stage: MaintenanceStage;
  requestDate?: string;
  scheduledDate?: string;
  completedDate?: string;
  teamCode?: string;
  technicianEmployeeNo?: string;
  durationHours: number;
  cost: number;
  notes?: string;
}

export interface CreateUpdateMaintenanceRequestDto {
  equipmentNo: string;
  subject: string;
  type: MaintenanceType;
  priority: MaintenancePriority;
  requestDate?: string;
  scheduledDate?: string;
  teamCode?: string;
  technicianEmployeeNo?: string;
  durationHours: number;
  cost: number;
  notes?: string;
}

export interface GeneratePreventiveInput {
  asOfDate?: string;
}

export interface GeneratePreventiveResultDto {
  requestsCreated: number;
}
