import { Component } from '@angular/core';
import {
  PurchaseDocumentService,
  PurchaseDocumentType,
  PurchaseHeaderDto,
  VendorService,
} from '@proxy/purchasing';
import { Observable, map } from 'rxjs';
import { ErpTableQuery, LookupItem } from '../../erp-shared';
import {
  DocumentAction,
  DocumentListBase,
  NewDocumentInput,
} from '../documents/document-list.base';

/** Purchase invoices (Business Central table 38, document type Invoice). */
@Component({
  selector: 'app-purchase-invoices',
  templateUrl: '../documents/document-list.component.html',
  standalone: false,
})
export class PurchaseInvoicesComponent extends DocumentListBase<PurchaseHeaderDto> {
  readonly titleKey = 'Erp::PurchaseInvoices';
  readonly icon = 'fas fa-shopping-cart';
  readonly partyLabelKey = 'Erp::Vendor';
  readonly unitAmountLabelKey = 'Erp::DirectUnitCost';
  readonly permissionPrefix = 'Erp.PurchaseDocuments';
  readonly chatterEntityType = 'PurchaseHeader';
  readonly partyNoField = 'buyFromVendorNo';
  readonly partyNameField = 'buyFromVendorName';
  readonly partyEntity = 'vendor';
  readonly externalDocumentNoLabelKey = 'Erp::VendorInvoiceNo';
  readonly itemAmountField = 'unitCost' as const;

  constructor(
    private readonly documents: PurchaseDocumentService,
    private readonly vendors: VendorService,
  ) {
    super();
  }

  protected getList = (query: ErpTableQuery) =>
    this.documents.getList({
      ...query,
      documentType: PurchaseDocumentType.Invoice,
      vendorId: this.partyFilter ?? undefined,
    } as never);

  protected partyName = (row: PurchaseHeaderDto) =>
    `${row.buyFromVendorNo ?? ''} ${row.buyFromVendorName ?? ''}`.trim();

  protected searchParties(term: string): Observable<LookupItem[]> {
    return this.vendors
      .getList({ filter: term, blocked: false, maxResultCount: 20, skipCount: 0 } as never)
      .pipe(
        map(result =>
          (result.items ?? []).map(v => ({
            id: v.id,
            code: v.no ?? '',
            name: v.name ?? undefined,
          })),
        ),
      );
  }

  protected createDocument(input: NewDocumentInput): Observable<PurchaseHeaderDto> {
    return this.documents.create({
      documentType: PurchaseDocumentType.Invoice,
      no: input.no,
      vendorId: input.partyId,
      postingDate: input.postingDate,
      locationCode: input.locationCode,
      vendorInvoiceNo: input.externalDocumentNo,
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
