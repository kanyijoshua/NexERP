import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { EmailDeliveryStatus, EmailDocumentType } from './emailing.enums';

export interface EmailTemplateDto extends FullAuditedEntityDto<string> {
  code?: string;
  name?: string;
  documentType: EmailDocumentType;
  subjectTemplate?: string;
  bodyHtmlTemplate?: string;
  isDefault: boolean;
  active: boolean;
}

export interface CreateUpdateEmailTemplateDto {
  code: string;
  name: string;
  documentType: EmailDocumentType;
  subjectTemplate: string;
  bodyHtmlTemplate: string;
  isDefault: boolean;
  active: boolean;
}

export interface GetEmailTemplateListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  documentType?: EmailDocumentType;
  active?: boolean;
}

export interface SentEmailLogDto extends FullAuditedEntityDto<string> {
  documentType: EmailDocumentType;
  documentId?: string;
  documentNo?: string;
  partyNo?: string;
  partyName?: string;
  recipientEmail?: string;
  ccEmail?: string;
  subject?: string;
  bodyHtml?: string;
  templateCode?: string;
  attachmentCount: number;
  status: EmailDeliveryStatus;
  sentAt?: string;
  sentByUserId?: string;
  errorMessage?: string;
}

export interface GetSentEmailLogListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  documentType?: EmailDocumentType;
  documentId?: string;
  documentNo?: string;
  status?: EmailDeliveryStatus;
}

export interface PreviewDocumentEmailInput {
  documentType: EmailDocumentType;
  documentId: string;
  templateCode?: string;
}

export interface DocumentEmailAttachmentDto {
  id?: string;
  fileName?: string;
}

export interface DocumentEmailPreviewDto {
  documentType: EmailDocumentType;
  documentId?: string;
  documentNo?: string;
  partyNo?: string;
  partyName?: string;
  recipientEmail?: string;
  templateCode?: string;
  subject?: string;
  bodyHtml?: string;
  attachments: DocumentEmailAttachmentDto[];
}

export interface SendDocumentEmailInput {
  documentType: EmailDocumentType;
  documentId: string;
  recipientEmail?: string;
  ccEmail?: string;
  customSubject?: string;
  customBodyHtml?: string;
  templateCode?: string;
  attachmentIds: string[];
}
