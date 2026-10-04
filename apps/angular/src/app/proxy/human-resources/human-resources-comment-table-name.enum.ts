import { mapEnumToOptions } from '@abp/ng.core';

export enum HumanResourcesCommentTableName {
  Employee = 0,
  AlternativeAddress = 1,
  EmployeeQualification = 2,
  EmployeeRelative = 3,
  EmployeeAbsence = 4,
  MiscArticleInformation = 5,
  ConfidentialInformation = 6,
}

export const humanResourcesCommentTableNameOptions = mapEnumToOptions(HumanResourcesCommentTableName);
