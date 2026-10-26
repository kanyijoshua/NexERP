import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  CustomerService,
  SalesDocumentService,
  SalesDocumentType,
  SalesHeaderDto,
  SalesLineDto,
} from '@proxy/sales';
import { Observable, map } from 'rxjs';
import { DocumentLineType } from '@proxy/documents';
import { ItemChargeSourceType, ItemTrackingSourceType } from '@proxy/inventory';
import { EmailDocumentType } from '@proxy/emailing';
import { ErpTableQuery, LookupItem } from '../../erp-shared';
import {
  DocumentAction,
  DocumentCardData,
  DocumentCardField,
  DocumentCardLine,
  DocumentLineRow,
  DocumentListBase,
  DocumentMode,
  NewDocumentInput,
  PREPAYMENT_FIELDS,
  OpenSourceLine,
  PostingLine,
  PostingResult,
} from '../documents/document-list.base';

/** The wording of each kind of sales document. */
const SALES_PAGES: Record<SalesDocumentType, { titleKey: string; icon: string; mode: DocumentMode }> = {
  [SalesDocumentType.Quote]: { titleKey: 'Erp::SalesQuotes', icon: 'fas fa-file-lines', mode: 'quote' },
  [SalesDocumentType.Order]: { titleKey: 'Erp::SalesOrders', icon: 'fas fa-cart-shopping', mode: 'order' },
  [SalesDocumentType.Invoice]: { titleKey: 'Erp::SalesInvoices', icon: 'fas fa-file-invoice-dollar', mode: 'invoice' },
  [SalesDocumentType.CreditMemo]: { titleKey: 'Erp::SalesCreditMemos', icon: 'fas fa-file-circle-minus', mode: 'invoice' },
  [SalesDocumentType.BlanketOrder]: { titleKey: 'Erp::SalesBlanketOrders', icon: 'fas fa-file-contract', mode: 'blanket' },
  [SalesDocumentType.ReturnOrder]: { titleKey: 'Erp::SalesReturnOrders', icon: 'fas fa-rotate-left', mode: 'order' },
};

/** The documents that can be emailed to the customer, as the email templates know them. */
const EMAIL_TYPES: Partial<Record<SalesDocumentType, EmailDocumentType>> = {
  [SalesDocumentType.Quote]: EmailDocumentType.SalesQuote,
  [SalesDocumentType.Order]: EmailDocumentType.SalesOrder,
  [SalesDocumentType.Invoice]: EmailDocumentType.SalesInvoice,
  [SalesDocumentType.CreditMemo]: EmailDocumentType.SalesCreditMemo,
};

/**
 * Sales quotes, orders, invoices, credit memos, blanket orders and return orders: one page,
 * the document type comes from the route's `data.documentType`.
 */
@Component({
  selector: 'app-sales-documents',
  templateUrl: '../documents/document-list.component.html',
  standalone: false,
})
export class SalesDocumentsComponent extends DocumentListBase<SalesHeaderDto> {
  readonly documentType: SalesDocumentType;
  readonly titleKey: string;
  readonly icon: string;
  readonly partyLabelKey = 'Erp::Customer';
  readonly unitAmountLabelKey = 'Erp::UnitPrice';
  readonly permissionPrefix = 'Erp.SalesDocuments';
  readonly chatterEntityType = 'SalesHeader';
  readonly partyNoField = 'sellToCustomerNo';
  readonly partyNameField = 'sellToCustomerName';
  readonly partyEntity = 'customer';
  readonly externalDocumentNoLabelKey = 'Erp::ExternalDocumentNo';
  readonly itemAmountField = 'unitPrice' as const;
  readonly cardFields: DocumentCardField[];

  constructor(
    private readonly documents: SalesDocumentService,
    private readonly customers: CustomerService,
    route: ActivatedRoute,
  ) {
    super();
    this.trackingSourceType = ItemTrackingSourceType.SalesLine;
    this.chargeSourceType = ItemChargeSourceType.SalesLine;
    this.documentType = (route.snapshot.data['documentType'] as SalesDocumentType) ?? SalesDocumentType.Invoice;
    const page = SALES_PAGES[this.documentType];
    this.titleKey = page.titleKey;
    this.icon = page.icon;
    this.mode = page.mode;
    this.emailDocumentType = EMAIL_TYPES[this.documentType] ?? null;
    this.supportsPrepayment = this.documentType === SalesDocumentType.Order;

    const isReturn = this.documentType === SalesDocumentType.ReturnOrder || this.documentType === SalesDocumentType.CreditMemo;
    this.shipLabelKey = isReturn ? 'Erp::ReceiveReturn' : 'Erp::Ship';
    this.shippedLabelKey = isReturn ? 'Erp::QuantityReturned' : 'Erp::QuantityShipped';
    this.sourceLinesLabelKey = isReturn ? 'Erp::GetReturnReceiptLines' : 'Erp::GetShipmentLines';
    this.qtyToShipLabelKey = isReturn ? 'Erp::QtyToReceive' : 'Erp::QtyToShip';

    this.cardFields = [
      { field: 'sellToCustomerNo', labelKey: 'Erp::Customer', type: 'readonly' },
      { field: 'sellToCustomerName', labelKey: 'Erp::Name', type: 'readonly' },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date' },
      { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date' },
      ...(this.mode === 'invoice' ? [] : [{ field: 'shipmentDate', labelKey: 'Erp::ShipmentDate', type: 'date' } as DocumentCardField]),
      { field: 'externalDocumentNo', labelKey: 'Erp::ExternalDocumentNo', type: 'text', maxLength: 35 },
      { field: 'locationCode', labelKey: 'Erp::LocationCode', type: 'lookup', entity: 'location' },
      { field: 'paymentTermsCode', labelKey: 'Erp::PaymentTermsCode', type: 'lookup', entity: 'paymentTerms' },
      { field: 'currencyCode', labelKey: 'Erp::CurrencyCode', type: 'lookup', entity: 'currency' },
      ...(this.supportsPrepayment ? PREPAYMENT_FIELDS : []),
    ];
  }

  protected loadDocument(id: string): Observable<DocumentCardData> {
    return this.documents.get(id) as unknown as Observable<DocumentCardData>;
  }

  protected updateDocument(id: string, input: Record<string, unknown>): Observable<unknown> {
    return this.documents.update(id, input as never);
  }

  protected toCardLine(raw: Record<string, unknown>): DocumentCardLine {
    const l = raw as unknown as SalesLineDto;
    return {
      id: l.id ?? null,
      type: l.type,
      no: l.no ?? '',
      description: l.description ?? '',
      locationCode: l.locationCode ?? null,
      quantity: l.quantity,
      unitOfMeasureCode: l.unitOfMeasureCode ?? null,
      unitAmount: l.unitPrice,
      lineDiscountPercent: l.lineDiscountPercent,
      lineAmount: l.lineAmount,
      vatPercent: l.vatPercent,
      qtyToShip: l.qtyToShip,
      quantityShipped: l.quantityShipped,
      qtyToInvoice: l.qtyToInvoice,
      quantityInvoiced: l.quantityInvoiced,
    };
  }

  protected fromCardLine(l: DocumentCardLine, original: Record<string, unknown> | undefined): Record<string, unknown> {
    // Quantities to post only for saved lines: a new line starts with everything outstanding.
    const toPost = this.isOrderMode && l.id ? { qtyToShip: Number(l.qtyToShip) || 0, qtyToInvoice: Number(l.qtyToInvoice) || 0 } : {};
    return {
      ...(original ?? {}),
      id: l.id ?? undefined,
      type: l.type,
      no: l.no,
      description: l.description,
      quantity: Number(l.quantity) || 0,
      unitPrice: Number(l.unitAmount) || 0,
      lineDiscountPercent: Number(l.lineDiscountPercent) || 0,
      unitOfMeasureCode: l.unitOfMeasureCode || undefined,
      locationCode: l.locationCode || undefined,
      // The header's prepayment % applies to every line not prepaid yet.
      prepaymentPct: undefined,
      qtyToShip: undefined,
      qtyToInvoice: undefined,
      ...toPost,
    };
  }

  protected getList = (query: ErpTableQuery) =>
    this.documents.getList({
      ...query,
      documentType: this.documentType,
      customerId: this.partyFilter ?? undefined,
    } as never);

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
      documentType: this.documentType,
      no: input.no,
      customerId: input.partyId,
      postingDate: input.postingDate,
      locationCode: input.locationCode,
      externalDocumentNo: input.externalDocumentNo,
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
          qtyToShip: l.qtyToShip,
          quantityShipped: l.quantityShipped,
          qtyToInvoice: l.qtyToInvoice,
          quantityInvoiced: l.quantityInvoiced,
          sourceNo: l.shipmentNo,
          isItem: l.type === DocumentLineType.Item,
          isCharge: l.type === DocumentLineType.ChargeItem,
        })),
      ),
    );
  }

  protected override postOrder(id: string, ship: boolean, invoice: boolean, lines: PostingLine[]): Observable<PostingResult> {
    return this.documents.runOrderPosting(id, {
      ship,
      invoice,
      lines: lines.map(l => ({ lineId: l.lineId, qtyToShip: l.qtyToShip, qtyToInvoice: l.qtyToInvoice })),
    });
  }

  protected override openSourceLines(id: string): Observable<OpenSourceLine[]> {
    return this.documents.getOpenShipmentLines(id).pipe(
      map(lines =>
        lines.map(l => ({
          id: l.shipmentLineId,
          documentNo: l.shipmentNo,
          orderNo: l.orderNo,
          postingDate: l.postingDate,
          lineNo: l.lineNo,
          no: l.no,
          description: l.description,
          qtyNotInvoiced: l.qtyNotInvoiced,
          unitAmount: l.unitPrice,
        })),
      ),
    );
  }

  protected override collectSourceLines(id: string, lineIds: string[]): Observable<unknown> {
    return this.documents.collectShipmentLines(id, { shipmentLineIds: lineIds });
  }
}
