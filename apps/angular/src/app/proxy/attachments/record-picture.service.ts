import type { GetRecordPictureInput, RecordPictureDto, UploadRecordPictureInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class RecordPictureService {
  apiName = 'Erp';


  delete = (input: GetRecordPictureInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/erp/record-picture',
      params: { entityName: input.entityName, recordId: input.recordId },
    },
    { apiName: this.apiName,...config });


  get = (input: GetRecordPictureInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RecordPictureDto>({
      method: 'GET',
      url: '/api/erp/record-picture',
      params: { entityName: input.entityName, recordId: input.recordId },
    },
    { apiName: this.apiName,...config });


  upload = (input: UploadRecordPictureInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RecordPictureDto>({
      method: 'POST',
      url: '/api/erp/record-picture/upload',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
