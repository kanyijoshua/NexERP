import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { ErpPagedListInput } from '../cost-accounting/cost-accounting.models';
import type { FullAuditedEntityDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';

export enum QualityOperation {
  Receipt = 0,
  Production = 1,
}
export const qualityOperationOptions = mapEnumToOptions(QualityOperation);

export enum QualityTestType {
  PassFail = 0,
  Measure = 1,
}
export const qualityTestTypeOptions = mapEnumToOptions(QualityTestType);

export enum QualityCheckResult {
  Pending = 0,
  Pass = 1,
  Fail = 2,
}
export const qualityCheckResultOptions = mapEnumToOptions(QualityCheckResult);

export interface QualityControlPointDto extends CodeTableDto {
  operation: QualityOperation;
  itemNo?: string;
  itemCategoryCode?: string;
  testType: QualityTestType;
  minValue: number;
  maxValue: number;
  unitOfMeasure?: string;
  instructions?: string;
  blocked: boolean;
}

export interface CreateUpdateQualityControlPointDto extends CreateUpdateCodeTableDto {
  operation: QualityOperation;
  itemNo?: string;
  itemCategoryCode?: string;
  testType: QualityTestType;
  minValue: number;
  maxValue: number;
  unitOfMeasure?: string;
  instructions?: string;
  blocked: boolean;
}

export interface QualityCheckDto extends FullAuditedEntityDto<string> {
  no?: string;
  controlPointCode?: string;
  operation: QualityOperation;
  itemNo?: string;
  itemDescription?: string;
  sourceDocumentNo?: string;
  lotNo?: string;
  quantity: number;
  testType: QualityTestType;
  minValue: number;
  maxValue: number;
  measuredValue?: number;
  result: QualityCheckResult;
  checkedBy?: string;
  checkedDate?: string;
  note?: string;
}

export interface CreateQualityCheckDto {
  controlPointCode: string;
  itemNo: string;
  sourceDocumentNo?: string;
  lotNo?: string;
  quantity: number;
}

export interface GetQualityCheckListInput extends ErpPagedListInput {
  sourceDocumentNo?: string;
  result?: QualityCheckResult;
}

export interface RecordQualityCheckInput {
  passed?: boolean;
  measuredValue?: number;
  note?: string;
}
