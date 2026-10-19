import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  PurchaseDocumentService,
  PurchaseDocumentType,
  PurchaseHeaderDto,
  VendorService,
} from '@proxy/purchasing';
import { Observable, map } from 'rxjs';
import { DocumentLineType } from '@proxy/documents';
import { ItemTrackingSourceType } from '@proxy/inventory';
import { ErpTableQuery, LookupItem } from '../../erp-shared';
import {
  DocumentAction,
  DocumentLineRow,
  DocumentListBase,
  DocumentMode,
  NewDocumentInput,
  OpenSourceLine,
  PostingLine,
  PostingResult,
} from '../documents/document-list.base';

/** The wording of each kind of purchase document. */
const PURCHASE_PAGES: Record<PurchaseDocumentType, { titleKey: string; icon: string; mode: DocumentMode }> = {
  [PurchaseDocumentType.Quote]: { titleKey: 'Erp::PurchaseQuotes', icon: 'fas fa-file-lines', mode: 'quote' },
  [PurchaseDocumentType.Order]: { titleKey: 'Erp::PurchaseOrders', icon: 'fas fa-truck-ramp-box', mode: 'order' },
  [PurchaseDocumentType.Invoice]: { titleKey: 'Erp::PurchaseInvoices', icon: 'fas fa-file-invoice', mode: 'invoice' },
  [PurchaseDocumentType.CreditMemo]: { titleKey: 'Erp::PurchaseCreditMemos', icon: 'fas fa-file-circle-minus', mode: 'invoice' },
  [PurchaseDocumentType.BlanketOrder]: { titleKey: 'Erp::PurchaseBlanketOrders', icon: 'fas fa-file-contract', mode: 'blanket' },
  [PurchaseDocumentType.ReturnOrder]: { titleKey: 'Erp::PurchaseReturnOrders', icon: 'fas fa-rotate-left', mode: 'order' },
};

/**
 * Purchase quotes, orders, invoices, credit memos, blanket orders and return orders: one page,
 * the document type comes from the route's `data.documentType`.
 */
@Component({
  selector: 'app-purchase-documents',
  templateUrl: '../documents/document-list.component.html',
  standalone: false,
})
export class PurchaseDocumentsComponent extends DocumentListBase<PurchaseHeaderDto> {
  readonly documentType: PurchaseDocumentType;
  readonly titleKey: string;
  readonly icon: string;
  readonly partyLabelKey = 'Erp::Vendor';
  readonly unitAmountLabelKey = 'Erp::DirectUnitCost';
  readonly permissionPrefix = 'Erp.PurchaseDocuments';
  readonly chatterEntityType = 'PurchaseHeader';
  readonly partyNoField = 'buyFromVendorNo';
  readonly partyNameField = 'buyFromVendorName';
  readonly partyEntity = 'vendor';
  readonly externalDocumentNoLabelKey: string;
  readonly itemAmountField = 'unitCost' as const;

  constructor(
    private readonly documents: PurchaseDocumentService,
    private readonly vendors: VendorService,
    route: ActivatedRoute,
  ) {
    super();
    this.trackingSourceType = ItemTrackingSourceType.PurchaseLine;
    this.documentType = (route.snapshot.data['documentType'] as PurchaseDocumentType) ?? PurchaseDocumentType.Invoice;
    const page = PURCHASE_PAGES[this.documentType];
    this.titleKey = page.titleKey;
    this.icon = page.icon;
    this.mode = page.mode;

    const isReturn = this.documentType === PurchaseDocumentType.ReturnOrder || this.documentType === PurchaseDocumentType.CreditMemo;
    this.shipLabelKey = isReturn ? 'Erp::ShipReturn' : 'Erp::Receive';
    this.shippedLabelKey = isReturn ? 'Erp::QuantityReturned' : 'Erp::QuantityReceived';
    this.sourceLinesLabelKey = isReturn ? 'Erp::GetReturnShipmentLines' : 'Erp::GetReceiptLines';
    // An invoice carries the vendor's invoice number; anything else the vendor's order number.
    this.externalDocumentNoLabelKey = this.mode === 'invoice' ? 'Erp::VendorInvoiceNo' : 'Erp::VendorOrderNo';
  }

  protected getList = (query: ErpTableQuery) =>
    this.documents.getList({
      ...query,
      documentType: this.documentType,
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
      documentType: this.documentType,
      no: input.no,
      vendorId: input.partyId,
      postingDate: input.postingDate,
      locationCode: input.locationCode,
      ...(this.mode === 'invoice'
        ? { vendorInvoiceNo: input.externalDocumentNo }
        : { vendorOrderNo: input.externalDocumentNo }),
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
    switch (action) {
      case 'delete':
        return this.documents.delete(id);
      case 'makeOrder':
        return this.documents.makeOrder(id, {});
      default:
        return this.documents[action](id);
    }
  }

  protected override loadLines(id: string): Observable<DocumentLineRow[]> {
    return this.documents.get(id).pipe(
      map(document =>
        (document.lines ?? []).map(l => ({
          id: l.id as string,
          lineNo: l.lineNo,
          no: l.no,
          description: l.description,
          quantity: l.quantity,
          qtyToShip: l.qtyToReceive,
          quantityShipped: l.quantityReceived,
          qtyToInvoice: l.qtyToInvoice,
          quantityInvoiced: l.quantityInvoiced,
          sourceNo: l.receiptNo,
          isItem: l.type === DocumentLineType.Item,
        })),
      ),
    );
  }

  protected override postOrder(id: string, ship: boolean, invoice: boolean, lines: PostingLine[]): Observable<PostingResult> {
    return this.documents
      .runOrderPosting(id, {
        receive: ship,
        invoice,
        lines: lines.map(l => ({ lineId: l.lineId, qtyToReceive: l.qtyToShip, qtyToInvoice: l.qtyToInvoice })),
      })
      .pipe(map(result => ({ invoiceNo: result.invoiceNo, shipmentNo: result.receiptNo })));
  }

  protected override openSourceLines(id: string): Observable<OpenSourceLine[]> {
    return this.documents.getOpenReceiptLines(id).pipe(
      map(lines =>
        lines.map(l => ({
          id: l.receiptLineId,
          documentNo: l.receiptNo,
          orderNo: l.orderNo,
          postingDate: l.postingDate,
          lineNo: l.lineNo,
          no: l.no,
          description: l.description,
          qtyNotInvoiced: l.qtyNotInvoiced,
          unitAmount: l.directUnitCost,
        })),
      ),
    );
  }

  protected override collectSourceLines(id: string, lineIds: string[]): Observable<unknown> {
    return this.documents.collectReceiptLines(id, { receiptLineIds: lineIds });
  }
}
