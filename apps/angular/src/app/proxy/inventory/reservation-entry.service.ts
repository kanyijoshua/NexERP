import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetItemAvailabilityInput, GetReservationEntryListInput, ItemAvailabilityDto, ReservationEntryDto, ReservationResultDto, ReserveDocumentInput } from './warehouse-management.models';

@Injectable({
  providedIn: 'root',
})
export class ReservationEntryService {
  apiName = 'Erp';

  cancel = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/reservation-entry/${id}/cancel`,
    },
    { apiName: this.apiName,...config });

  cancelDocument = (input: ReserveDocumentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/erp/reservation-entry/cancel-document',
      body: input,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReservationEntryDto>({
      method: 'GET',
      url: `/api/erp/reservation-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getAvailability = (input: GetItemAvailabilityInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemAvailabilityDto>({
      method: 'GET',
      url: '/api/erp/reservation-entry/availability',
      params: { itemNo: input.itemNo, locationCode: input.locationCode, variantCode: input.variantCode },
    },
    { apiName: this.apiName,...config });

  getList = (input: GetReservationEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ReservationEntryDto>>({
      method: 'GET',
      url: '/api/erp/reservation-entry',
      params: { dynamicFilter: input.dynamicFilter, itemNo: input.itemNo, sourceDocumentNo: input.sourceDocumentNo, sourceId: input.sourceId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  reserveAssemblyOrder = (input: ReserveDocumentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReservationResultDto>({
      method: 'POST',
      url: '/api/erp/reservation-entry/reserve-assembly-order',
      body: input,
    },
    { apiName: this.apiName,...config });

  reserveSalesOrder = (input: ReserveDocumentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReservationResultDto>({
      method: 'POST',
      url: '/api/erp/reservation-entry/reserve-sales-order',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
