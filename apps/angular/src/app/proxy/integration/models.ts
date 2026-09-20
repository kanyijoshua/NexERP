import type { WebServiceObjectType } from './web-service-object-type.enum';
import type { EntityChangeKind } from './entity-change-kind.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { WebhookDeliveryStatus } from './webhook-delivery-status.enum';
import type { EntityFilterDto } from '../exporting/models';

export interface CreateUpdateWebServiceDto {
  serviceName: string;
  entityName: string;
  objectType: WebServiceObjectType;
  excludedFields?: string;
}

export interface CreateUpdateWebhookSubscriptionDto {
  name: string;
  entityName: string;
  endpointUrl: string;
  changeKinds: EntityChangeKind;
  active: boolean;
}

export interface GetWebhookDeliveriesInput extends PagedAndSortedResultRequestDto {
  subscriptionId?: string;
  status?: WebhookDeliveryStatus;
}

export interface IntegrationQueryInput {
  serviceName: string;
  fields: string[];
  filters: EntityFilterDto[];
  orderBy?: string;
  descending: boolean;
  skipCount: number;
  maxResultCount: number;
}

export interface IntegrationQueryResultDto {
  serviceName?: string;
  entityName?: string;
  totalCount: number;
  fields: string[];
  items: object[];
}

export interface PublishedWebServiceDto extends EntityDto<string> {
  serviceName?: string;
  entityName?: string;
  objectType: WebServiceObjectType;
  published: boolean;
  excludedFields?: string;
  url?: string;
}

export interface SetWebServicePublishedInput {
  id?: string;
  published: boolean;
}

export interface WebhookDeliveryDto extends EntityDto<string> {
  subscriptionId?: string;
  entityName?: string;
  changeKind: EntityChangeKind;
  entityId?: string;
  status: WebhookDeliveryStatus;
  attemptCount: number;
  creationTime?: string;
  lastAttemptTime?: string;
  nextAttemptTime?: string;
  responseStatusCode?: number;
  error?: string;
  payload?: string;
}

export interface WebhookSubscriptionDto extends EntityDto<string> {
  name?: string;
  entityName?: string;
  endpointUrl?: string;
  changeKinds: EntityChangeKind;
  active: boolean;
  failureCount: number;
  lastError?: string;
  lastDeliveryTime?: string;
  secret?: string;
}
