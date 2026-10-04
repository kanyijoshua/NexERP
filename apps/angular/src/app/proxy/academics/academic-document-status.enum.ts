import { mapEnumToOptions } from '@abp/ng.core';

export enum AcademicDocumentStatus {
  Open = 0,
  Posted = 1,
}

export const academicDocumentStatusOptions = mapEnumToOptions(AcademicDocumentStatus);
