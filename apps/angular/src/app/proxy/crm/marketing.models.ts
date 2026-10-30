import type { ContactType } from './crm.models';
import type { ErpPagedListInput } from '../cost-accounting/cost-accounting.models';
import type { EntityDto, FullAuditedEntityDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';

export enum MailingStatus {
  Sent = 0,
  Failed = 1,
  NoEmail = 2,
}
export const mailingStatusOptions = mapEnumToOptions(MailingStatus);

export interface SegmentDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  contactCount: number;
}

export interface CreateUpdateSegmentDto {
  no?: string;
  description?: string;
}

export interface GetSegmentListInput extends ErpPagedListInput {
  filter?: string;
}

export interface PickSegmentContactsInput {
  city?: string;
  salespersonCode?: string;
  type?: ContactType;
  onlyWithEmail: boolean;
}

export interface ApplySegmentInput {
  campaignNo: string;
}

export interface SegmentResultDto {
  count: number;
}

export interface SegmentLineDto extends EntityDto<string> {
  segmentId?: string;
  contactNo?: string;
  contactName?: string;
  email?: string;
}

export interface GetSegmentLineListInput extends ErpPagedListInput {
  segmentId?: string;
}

export interface CampaignMailingEntryDto extends EntityDto<string> {
  campaignNo?: string;
  contactNo?: string;
  contactName?: string;
  email?: string;
  subject?: string;
  sentAt?: string;
  status: MailingStatus;
  errorMessage?: string;
}

export interface GetCampaignMailingListInput extends ErpPagedListInput {
  campaignNo?: string;
}

export interface SendMailingInput {
  campaignNo: string;
  subject: string;
  bodyHtml: string;
}

export interface MailingResultDto {
  sent: number;
  failed: number;
  withoutEmail: number;
}
