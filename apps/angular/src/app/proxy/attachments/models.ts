import type { EntityDto } from '@abp/ng.core';
import type { DocumentAttachmentFileType } from './document-attachment-file-type.enum';

export interface DocumentAttachmentDto extends EntityDto<string> {
  entityName?: string;
  recordId?: string;
  no?: string;
  lineNo: number;
  attachmentNo: number;
  attachedDate?: string;
  attachedByUserName?: string;
  fileName?: string;
  fileExtension?: string;
  fileType: DocumentAttachmentFileType;
  contentType?: string;
  size: number;
  documentFlowPurchase: boolean;
  documentFlowSales: boolean;
}

export interface GetDocumentAttachmentListInput {
  entityName: string;
  recordId?: string;
}

export interface UploadDocumentAttachmentInput extends GetDocumentAttachmentListInput {
  fileName: string;
  contentType?: string;
  contentBase64: string;
  lineNo?: number;
  documentFlowPurchase?: boolean;
  documentFlowSales?: boolean;
}

export interface UpdateDocumentAttachmentDto {
  fileName: string;
  documentFlowPurchase: boolean;
  documentFlowSales: boolean;
}
