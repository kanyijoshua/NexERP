import type { ActivityStreamEntryDto, ChatterEntityInput, CreateActivityTaskDto, CreateDocumentNoteDto, DocumentActivityTaskDto, DocumentNoteDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class chatterService {
  apiName = 'Erp';
  

  createNote = (input: CreateDocumentNoteDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DocumentNoteDto>({
      method: 'POST',
      url: '/api/erp/chatter/note',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createTask = (input: CreateActivityTaskDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DocumentActivityTaskDto>({
      method: 'POST',
      url: '/api/erp/chatter/task',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  getActivityStream = (input: ChatterEntityInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ActivityStreamEntryDto>>({
      method: 'GET',
      url: '/api/erp/chatter/activity-stream',
      params: { entityType: input.entityType, entityId: input.entityId },
    },
    { apiName: this.apiName,...config });
  

  getNotes = (input: ChatterEntityInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<DocumentNoteDto>>({
      method: 'GET',
      url: '/api/erp/chatter/notes',
      params: { entityType: input.entityType, entityId: input.entityId },
    },
    { apiName: this.apiName,...config });
  

  getTasks = (input: ChatterEntityInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<DocumentActivityTaskDto>>({
      method: 'GET',
      url: '/api/erp/chatter/tasks',
      params: { entityType: input.entityType, entityId: input.entityId },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
