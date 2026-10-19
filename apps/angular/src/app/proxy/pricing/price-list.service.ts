import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdatePriceListDto, GetPriceInput, GetPriceListListInput, PriceListDto, PriceQuoteDto } from './models';

@Injectable({
  providedIn: 'root',
})
export class PriceListService {
  apiName = 'Erp';

  create = (input: CreateUpdatePriceListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PriceListDto>({
      method: 'POST',
      url: '/api/erp/price-list',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/price-list/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PriceListDto>({
      method: 'GET',
      url: `/api/erp/price-list/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPriceListListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PriceListDto>>({
      method: 'GET',
      url: '/api/erp/price-list',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, priceType: input.priceType, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePriceListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PriceListDto>({
      method: 'PUT',
      url: `/api/erp/price-list/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  getPriceQuote = (input: GetPriceInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PriceQuoteDto>({
      method: 'GET',
      url: '/api/erp/price-list/price-quote',
      params: { priceType: input.priceType, partyNo: input.partyNo, itemNo: input.itemNo, unitOfMeasureCode: input.unitOfMeasureCode, quantity: input.quantity, currencyCode: input.currencyCode, date: input.date },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
