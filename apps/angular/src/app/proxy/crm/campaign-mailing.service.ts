import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CampaignMailingEntryDto, GetCampaignMailingListInput, MailingResultDto, SendMailingInput } from './marketing.models';

@Injectable({
  providedIn: 'root',
})
export class CampaignMailingService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CampaignMailingEntryDto>({
      method: 'GET',
      url: `/api/erp/campaign-mailing/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCampaignMailingListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CampaignMailingEntryDto>>({
      method: 'GET',
      url: '/api/erp/campaign-mailing',
      params: { dynamicFilter: input.dynamicFilter, campaignNo: input.campaignNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  send = (input: SendMailingInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MailingResultDto>({
      method: 'POST',
      url: '/api/erp/campaign-mailing/send',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
