import type {
  AwardTenderInput,
  CreateUpdateTenderDto,
  EvaluateTenderBidInput,
  GetTenderListInput,
  SubmitTenderBidInput,
  TenderHeaderDto,
} from './models';
import type { ApprovalRequestResultDto } from '../workflows/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class TenderService {
  apiName = 'Erp';

  create = (input: CreateUpdateTenderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TenderHeaderDto>(
      {
        method: 'POST',
        url: '/api/erp/tender',
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  update = (id: string, input: CreateUpdateTenderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TenderHeaderDto>(
      {
        method: 'PUT',
        url: `/api/erp/tender/${id}`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/erp/tender/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TenderHeaderDto>(
      {
        method: 'GET',
        url: `/api/erp/tender/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  getList = (input: GetTenderListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<TenderHeaderDto>>(
      {
        method: 'GET',
        url: '/api/erp/tender',
        params: {
          filter: input.filter,
          tenderType: input.tenderType,
          status: input.status,
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config }
    );

  publish = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TenderHeaderDto>(
      {
        method: 'POST',
        url: `/api/erp/tender/${id}/publish`,
      },
      { apiName: this.apiName, ...config }
    );

  moveToOpening = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TenderHeaderDto>(
      {
        method: 'POST',
        url: `/api/erp/tender/${id}/move-to-opening`,
      },
      { apiName: this.apiName, ...config }
    );

  moveToEvaluation = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TenderHeaderDto>(
      {
        method: 'POST',
        url: `/api/erp/tender/${id}/move-to-evaluation`,
      },
      { apiName: this.apiName, ...config }
    );

  cancelTender = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TenderHeaderDto>(
      {
        method: 'POST',
        url: `/api/erp/tender/${id}/cancel-tender`,
      },
      { apiName: this.apiName, ...config }
    );

  submitBid = (id: string, input: SubmitTenderBidInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TenderHeaderDto>(
      {
        method: 'POST',
        url: `/api/erp/tender/${id}/submit-bid`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  evaluateBid = (id: string, input: EvaluateTenderBidInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TenderHeaderDto>(
      {
        method: 'POST',
        url: `/api/erp/tender/${id}/evaluate-bid`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  award = (id: string, input: AwardTenderInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TenderHeaderDto>(
      {
        method: 'POST',
        url: `/api/erp/tender/${id}/award`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  sendApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalRequestResultDto>(
      {
        method: 'POST',
        url: `/api/erp/tender/${id}/send-approval-request`,
      },
      { apiName: this.apiName, ...config }
    );

  cancelApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'POST',
        url: `/api/erp/tender/${id}/cancel-approval-request`,
      },
      { apiName: this.apiName, ...config }
    );

  constructor(private restService: RestService) {}
}
