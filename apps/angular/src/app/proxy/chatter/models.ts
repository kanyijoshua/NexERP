import type { EntityDto } from '@abp/ng.core';

export interface ActivityStreamEntryDto extends EntityDto<string> {
  entityType?: string;
  entityId?: string;
  entityNo?: string;
  fieldName?: string;
  oldValue?: string;
  newValue?: string;
  actionDescription?: string;
  creationTime?: string;
}

export interface ChatterEntityInput {
  entityType: string;
  entityId?: string;
}

export interface CreateActivityTaskDto {
  entityType: string;
  entityId?: string;
  entityNo?: string;
  activityType: string;
  summary: string;
  dueDate?: string;
  assignedUserId?: string;
}

export interface CreateDocumentNoteDto {
  entityType: string;
  entityId?: string;
  entityNo?: string;
  noteText: string;
}

export interface DocumentActivityTaskDto extends EntityDto<string> {
  entityType?: string;
  entityId?: string;
  entityNo?: string;
  activityType?: string;
  summary?: string;
  dueDate?: string;
  assignedUserId?: string;
  completed: boolean;
}

export interface DocumentNoteDto extends EntityDto<string> {
  entityType?: string;
  entityId?: string;
  entityNo?: string;
  noteText?: string;
  authorName?: string;
  creationTime?: string;
}
