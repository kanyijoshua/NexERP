import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface DocumentNoteDto {
  id: string;
  entityType: string;
  entityId: string;
  entityNo: string;
  noteText: string;
  authorName: string;
  creationTime: string;
}

export interface ActivityStreamDto {
  id: string;
  entityType: string;
  entityId: string;
  actionDescription: string;
  creationTime: string;
}

@Injectable({
  providedIn: 'root',
})
export class ChatterService {
  constructor(private restService: RestService) {}

  getNotes(entityType: string, entityId: string): Observable<DocumentNoteDto[]> {
    return this.restService.request<void, DocumentNoteDto[]>({
      method: 'GET',
      url: `/api/erp/chatter/notes?entityType=${entityType}&entityId=${entityId}`,
    });
  }

  addNote(entityType: string, entityId: string, entityNo: string, noteText: string): Observable<DocumentNoteDto> {
    return this.restService.request<any, DocumentNoteDto>({
      method: 'POST',
      url: '/api/erp/chatter/notes',
      body: { entityType, entityId, entityNo, noteText },
    });
  }

  getActivityStream(entityType: string, entityId: string): Observable<ActivityStreamDto[]> {
    return this.restService.request<void, ActivityStreamDto[]>({
      method: 'GET',
      url: `/api/erp/chatter/activities?entityType=${entityType}&entityId=${entityId}`,
    });
  }
}
