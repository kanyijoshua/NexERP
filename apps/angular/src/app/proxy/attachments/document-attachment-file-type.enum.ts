import { mapEnumToOptions } from '@abp/ng.core';

export enum DocumentAttachmentFileType {
  None = 0,
  Image = 1,
  Pdf = 2,
  Word = 3,
  Excel = 4,
  PowerPoint = 5,
  Email = 6,
  Xml = 7,
  Other = 8,
}

export const documentAttachmentFileTypeOptions = mapEnumToOptions(DocumentAttachmentFileType);
