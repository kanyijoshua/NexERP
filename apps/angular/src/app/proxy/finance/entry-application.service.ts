import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { PartyLedgerEntryDto } from './periodic-activities.models';
import type { ApplicationResultDto, ApplyEntriesInput, DetailedLedgerEntryDto, GetApplicableEntriesInput, GetDetailedEntriesInput, UnapplyEntryInput } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class EntryApplicationService {
  apiName = 'Erp';

  getApplicableEntries = (input: GetApplicableEntriesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PartyLedgerEntryDto[]>({
      method: 'GET',
      url: '/api/erp/entry-application/applicable-entries',
      params: { partyType: input.partyType, applyingEntryId: input.applyingEntryId },
    },
    { apiName: this.apiName,...config });

  apply = (input: ApplyEntriesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApplicationResultDto>({
      method: 'POST',
      url: '/api/erp/entry-application/apply',
      body: input,
    },
    { apiName: this.apiName,...config });

  unapply = (input: UnapplyEntryInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: '/api/erp/entry-application/unapply',
      body: input,
    },
    { apiName: this.apiName,...config });

  getDetailedEntries = (input: GetDetailedEntriesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DetailedLedgerEntryDto[]>({
      method: 'GET',
      url: '/api/erp/entry-application/detailed-entries',
      params: { partyType: input.partyType, ledgerEntryNo: input.ledgerEntryNo },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
