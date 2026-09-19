import { ABP, ListService } from '@abp/ng.core';
import { Component } from '@angular/core';
import { PurchaseDocumentService, PurchaseDocumentType, PurchaseHeaderDto, VendorService } from '@proxy/purchasing';
import { Observable, map } from 'rxjs';
import { LookupItem } from '../../erp-shared';
import { DocumentAction, DocumentListBase, NewDocumentInput } from '../documents/document-list.base';

/** Purchase invoices (Business Central table 38, document type Invoice). */
@Component({
  selector: 'app-purchase-invoices',
  templateUrl: '../documents/document-list.component.html',
  providers: [ListService],
})
export class PurchaseInvoicesComponent extends DocumentListBase<PurchaseHeaderDto> {
  readonly titleKey = 'Erp::PurchaseInvoices';
  readonly icon = 'fas fa-shopping-cart';
  readonly partyLabelKey = 'Erp::Vendor';
  readonly unitAmountLabelKey = 'Erp::DirectUnitCost';
  readonly permissionPrefix = 'Erp.PurchaseDocuments';
  readonly chatterEntityType = 'PurchaseHeader';

  constructor(
    private readonly documents: PurchaseDocumentService,
    private readonly vendors: VendorService,
  ) {
    super();
  }

  protected getList = (query: ABP.PageQueryParams) =>
    this.documents.getList({ ...query, documentType: PurchaseDocumentType.Invoice } as never);

  protected partyName = (row: PurchaseHeaderDto) => `${row.buyFromVendorNo ?? ''} ${row.buyFromVendorName ?? ''}`.trim();

  protected searchParties(term: string): Observable<LookupItem[]> {
    return this.vendors
      .getList({ filter: term, blocked: false, maxResultCount: 20, skipCount: 0 } as never)
      .pipe(map(result => (result.items ?? []).map(v => ({ id: v.id, code: v.no ?? '', name: v.name ?? undefined }))));
  }

  protected createDocument(input: NewDocumentInput): Observable<PurchaseHeaderDto> {
    return this.documents.create({
      documentType: PurchaseDocumentType.Invoice,
      no: input.no,
      vendorId: input.partyId,
      postingDate: input.postingDate,
      lines: input.lines.map(l => ({
        type: l.type,
        no: l.no,
        description: l.description,
        quantity: l.quantity,
        directUnitCost: l.unitAmount,
        lineDiscountPercent: 0,
      })),
    } as never);
  }

  protected run(action: DocumentAction, id: string): Observable<unknown> {
    return action === 'delete' ? this.documents.delete(id) : this.documents[action](id);
  }
}
