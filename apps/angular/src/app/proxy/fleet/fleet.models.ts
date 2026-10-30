import type { ErpPagedListInput } from '../cost-accounting/cost-accounting.models';
import type { FullAuditedEntityDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';

export enum VehicleStatus {
  Active = 0,
  InShop = 1,
  Retired = 2,
}
export const vehicleStatusOptions = mapEnumToOptions(VehicleStatus);

export enum VehicleLogType {
  Odometer = 0,
  Fuel = 1,
  Service = 2,
}
export const vehicleLogTypeOptions = mapEnumToOptions(VehicleLogType);

export enum VehicleContractType {
  Leasing = 0,
  Insurance = 1,
  Service = 2,
}
export const vehicleContractTypeOptions = mapEnumToOptions(VehicleContractType);

export enum ContractCostFrequency {
  Monthly = 0,
  Yearly = 1,
}
export const contractCostFrequencyOptions = mapEnumToOptions(ContractCostFrequency);

export interface VehicleDto extends FullAuditedEntityDto<string> {
  no?: string;
  licensePlate?: string;
  make?: string;
  model?: string;
  year: number;
  vin?: string;
  fuelType?: string;
  driverEmployeeNo?: string;
  fixedAssetNo?: string;
  acquisitionDate?: string;
  odometer: number;
  status: VehicleStatus;
}

export interface CreateUpdateVehicleDto {
  no?: string;
  licensePlate: string;
  make?: string;
  model?: string;
  year: number;
  vin?: string;
  fuelType?: string;
  driverEmployeeNo?: string;
  fixedAssetNo?: string;
  acquisitionDate?: string;
  status: VehicleStatus;
}

export interface GetFleetListInput extends ErpPagedListInput {
  filter?: string;
  vehicleNo?: string;
}

export interface GetVehicleCostsInput {
  fromDate?: string;
  toDate?: string;
}

export interface VehicleCostDto {
  vehicleNo?: string;
  licensePlate?: string;
  fuelCost: number;
  serviceCost: number;
  contractCost: number;
  totalCost: number;
  liters: number;
  distance: number;
  costPerKm: number;
}

export interface VehicleLogEntryDto extends FullAuditedEntityDto<string> {
  vehicleNo?: string;
  date?: string;
  type: VehicleLogType;
  description?: string;
  odometer: number;
  liters: number;
  cost: number;
  vendorNo?: string;
}

export interface CreateVehicleLogEntryDto {
  vehicleNo: string;
  date?: string;
  type: VehicleLogType;
  description?: string;
  odometer: number;
  liters: number;
  cost: number;
  vendorNo?: string;
}

export interface VehicleContractDto extends FullAuditedEntityDto<string> {
  vehicleNo?: string;
  type: VehicleContractType;
  vendorNo?: string;
  reference?: string;
  startDate?: string;
  endDate?: string;
  recurringCost: number;
  costFrequency: ContractCostFrequency;
}

export interface CreateUpdateVehicleContractDto {
  vehicleNo: string;
  type: VehicleContractType;
  vendorNo?: string;
  reference?: string;
  startDate?: string;
  endDate?: string;
  recurringCost: number;
  costFrequency: ContractCostFrequency;
}
