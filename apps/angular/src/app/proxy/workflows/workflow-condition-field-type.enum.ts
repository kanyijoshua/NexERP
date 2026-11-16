import { mapEnumToOptions } from '@abp/ng.core';

export enum WorkflowConditionFieldType {
  Text = 0,
  Number = 1,
  Date = 2,
  Boolean = 3,
  Option = 4,
}

export const workflowConditionFieldTypeOptions = mapEnumToOptions(WorkflowConditionFieldType);
