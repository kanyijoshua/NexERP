import { ABP, ListService } from '@abp/ng.core';
import { Component } from '@angular/core';
import {
  CustomerService,
  SalesDocumentService,
  SalesDocumentType,
  SalesHeaderDto,
} from '@proxy/sales';
import { Observable, map } from 'rxjs';
import { LookupItem } from '../../erp-shared';
import {
  DocumentAction,
  DocumentListBase,
  NewDocumentInput,
} from '../documents/document-list.base';

/** Sales invoices (Business Central table 36, document type Invoice). */
@Component({
  selector: 'app-sales-invoices',
  templateUrl: '../documents/document-list.component.html',
  providers: [ListService],
})
export class SalesInvoicesComponent extends DocumentListBase<SalesHeaderDto> {
  readonly titleKey = 'Erp::SalesInvoices';
  readonly icon = 'fas fa-file-invoice-dollar';
  readonly partyLabelKey = 'Erp::Customer';
  readonly unitAmountLabelKey = 'Erp::UnitPrice';
  readonly permissionPrefix = 'Erp.SalesDocuments';
  readonly chatterEntityType = 'SalesHeader';

  constructor(
    private readonly documents: SalesDocumentService,
    private readonly customers: CustomerService,
  ) {
    super();
  }

  protected getList = (query: ABP.PageQueryParams) =>
    this.documents.getList({ ...query, documentType: SalesDocumentType.Invoice } as never);

  protected partyName = (row: SalesHeaderDto) =>
    `${row.sellToCustomerNo ?? ''} ${row.sellToCustomerName ?? ''}`.trim();

  protected searchParties(term: string): Observable<LookupItem[]> {
    return this.customers
      .getList({ filter: term, blocked: false, maxResultCount: 20, skipCount: 0 } as never)
      .pipe(
        map(result =>
          (result.items ?? []).map(c => ({
            id: c.id,
            code: c.no ?? '',
            name: c.name ?? undefined,
          })),
        ),
      );
  }

  protected createDocument(input: NewDocumentInput): Observable<SalesHeaderDto> {
    return this.documents.create({
      documentType: SalesDocumentType.Invoice,
      no: input.no,
      customerId: input.partyId,
      postingDate: input.postingDate,
      lines: input.lines.map(l => ({
        type: l.type,
        no: l.no,
        description: l.description,
        quantity: l.quantity,
        unitPrice: l.unitAmount,
        lineDiscountPercent: 0,
      })),
    } as never);
  }

  protected run(action: DocumentAction, id: string): Observable<unknown> {
    return action === 'delete' ? this.documents.delete(id) : this.documents[action](id);
  }
}
