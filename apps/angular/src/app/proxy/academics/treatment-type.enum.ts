import { mapEnumToOptions } from '@abp/ng.core';

export enum TreatmentType {
  Outpatient = 0,
  Inpatient = 1,
}

export const treatmentTypeOptions = mapEnumToOptions(TreatmentType);
