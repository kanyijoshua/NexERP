import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';

export enum ContactType {
  Company = 0,
  Person = 1,
}
export const contactTypeOptions = mapEnumToOptions(ContactType);

export enum OpportunityKind {
  Lead = 0,
  Opportunity = 1,
}
export const opportunityKindOptions = mapEnumToOptions(OpportunityKind);

export enum OpportunityStatus {
  Open = 0,
  Won = 1,
  Lost = 2,
}
export const opportunityStatusOptions = mapEnumToOptions(OpportunityStatus);

export enum CloseOpportunityType {
  Won = 0,
  Lost = 1,
}
export const closeOpportunityTypeOptions = mapEnumToOptions(CloseOpportunityType);

export enum OpportunityPriority {
  Low = 0,
  Normal = 1,
  High = 2,
}
export const opportunityPriorityOptions = mapEnumToOptions(OpportunityPriority);

export interface PagedCrmInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
}

export interface MarketingSetupDto {
  contactNos?: string;
  opportunityNos?: string;
  campaignNos?: string;
  customerPostingGroup?: string;
  genBusPostingGroup?: string;
  vatBusPostingGroup?: string;
  paymentTermsCode?: string;
}

export interface ContactDto extends FullAuditedEntityDto<string> {
  no?: string;
  type: ContactType;
  name?: string;
  companyNo?: string;
  companyName?: string;
  jobTitle?: string;
  email?: string;
  phoneNo?: string;
  mobilePhoneNo?: string;
  address?: string;
  city?: string;
  countryRegionCode?: string;
  salespersonCode?: string;
  customerNo?: string;
  vendorNo?: string;
}

export interface CreateUpdateContactDto {
  no?: string;
  type: ContactType;
  name: string;
  companyNo?: string;
  jobTitle?: string;
  email?: string;
  phoneNo?: string;
  mobilePhoneNo?: string;
  address?: string;
  city?: string;
  countryRegionCode?: string;
  salespersonCode?: string;
  vendorNo?: string;
}

export interface GetContactListInput extends PagedCrmInput {
  type?: ContactType;
  companyNo?: string;
}

export interface OpportunityDto extends FullAuditedEntityDto<string> {
  no?: string;
  kind: OpportunityKind;
  description?: string;
  contactNo?: string;
  contactName?: string;
  contactCompanyName?: string;
  email?: string;
  phoneNo?: string;
  salespersonCode?: string;
  campaignNo?: string;
  priority: OpportunityPriority;
  stageId?: string;
  stageName?: string;
  probability: number;
  expectedRevenue: number;
  weightedRevenue: number;
  expectedCloseDate?: string;
  status: OpportunityStatus;
  closeOpportunityCode?: string;
  closedDate?: string;
  salesQuoteNo?: string;
}

export interface CreateUpdateOpportunityDto {
  no?: string;
  kind: OpportunityKind;
  description: string;
  contactNo?: string;
  contactCompanyName?: string;
  email?: string;
  phoneNo?: string;
  salespersonCode?: string;
  campaignNo?: string;
  priority: OpportunityPriority;
  probability?: number;
  expectedRevenue: number;
  expectedCloseDate?: string;
}

export interface GetOpportunityListInput extends PagedCrmInput {
  kind?: OpportunityKind;
  status?: OpportunityStatus;
  contactNo?: string;
  salespersonCode?: string;
}

export interface MoveOpportunityInput {
  stageId: string;
  closeCode?: string;
}

export interface CloseOpportunityInput {
  closeCode?: string;
}

export interface OpportunityEntryDto extends EntityDto<string> {
  date?: string;
  stageName?: string;
  status: OpportunityStatus;
  probability: number;
  expectedRevenue: number;
  note?: string;
}

export interface PipelineStageDto {
  stageId: string;
  name?: string;
  sequence: number;
  isWon: boolean;
  isLost: boolean;
  count: number;
  expectedRevenue: number;
  weightedRevenue: number;
}

export interface PipelineDto {
  pipeline?: string;
  stages: PipelineStageDto[];
  cards: OpportunityDto[];
}

export interface CloseOpportunityCodeDto extends CodeTableDto {
  type: CloseOpportunityType;
}

export interface CreateUpdateCloseOpportunityCodeDto extends CreateUpdateCodeTableDto {
  type: CloseOpportunityType;
}

export interface ActivityTypeDto extends CodeTableDto {
  icon?: string;
}

export interface CreateUpdateActivityTypeDto extends CreateUpdateCodeTableDto {
  icon?: string;
}

export interface PipelineStageSetupDto extends EntityDto<string> {
  pipelineType?: string;
  sequence: number;
  name?: string;
  foldedInKanban: boolean;
  isWonStage: boolean;
  isLostStage: boolean;
  defaultProbability: number;
}

export interface CreateUpdatePipelineStageDto {
  pipelineType: string;
  sequence: number;
  name: string;
  foldedInKanban: boolean;
  isWonStage: boolean;
  isLostStage: boolean;
  defaultProbability: number;
}

export interface GetPipelineStageListInput extends PagedCrmInput {
  pipelineType?: string;
}

export interface CampaignDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  startingDate?: string;
  endingDate?: string;
  salespersonCode?: string;
  budget: number;
}

export interface CreateUpdateCampaignDto {
  no?: string;
  description?: string;
  startingDate?: string;
  endingDate?: string;
  salespersonCode?: string;
  budget: number;
}

export interface CampaignTargetDto extends EntityDto<string> {
  campaignNo?: string;
  contactNo?: string;
  contactName?: string;
}

export interface CreateUpdateCampaignTargetDto {
  campaignNo: string;
  contactNo: string;
}

export interface GetCampaignTargetListInput extends PagedCrmInput {
  campaignNo?: string;
}

export interface InteractionLogEntryDto extends FullAuditedEntityDto<string> {
  date?: string;
  contactNo?: string;
  contactName?: string;
  opportunityNo?: string;
  campaignNo?: string;
  activityTypeCode?: string;
  description?: string;
  durationMinutes: number;
  cost: number;
  salespersonCode?: string;
}

export interface CreateUpdateInteractionLogEntryDto {
  date?: string;
  contactNo: string;
  opportunityNo?: string;
  campaignNo?: string;
  activityTypeCode?: string;
  description: string;
  durationMinutes: number;
  cost: number;
  salespersonCode?: string;
}

export interface GetInteractionLogEntryListInput extends PagedCrmInput {
  contactNo?: string;
  opportunityNo?: string;
}
