import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  PurchaseDocumentService,
  PurchaseDocumentType,
  PurchaseHeaderDto,
  PurchaseLineDto,
  VendorService,
} from '@proxy/purchasing';
import { Observable, map } from 'rxjs';
import { DocumentLineType, DocumentStatus } from '@proxy/documents';
import { InsertStandardLinesInput } from '@proxy/sales';
import { RecordDialogService } from '../../erp-shared/record/record-dialog.service';
import { runWithInput } from '../../master-data/entity-helpers';
import { ItemChargeSourceType, ItemTrackingSourceType } from '@proxy/inventory';
import { EmailDocumentType } from '@proxy/emailing';
import { DocumentAction as CardAction, ErpTableQuery, LookupItem } from '../../erp-shared';
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

/** The wording of each kind of purchase document. */
const PURCHASE_PAGES: Record<PurchaseDocumentType, { titleKey: string; icon: string; mode: DocumentMode }> = {
  [PurchaseDocumentType.Quote]: { titleKey: 'Erp::PurchaseQuotes', icon: 'fas fa-file-lines', mode: 'quote' },
  [PurchaseDocumentType.Order]: { titleKey: 'Erp::PurchaseOrders', icon: 'fas fa-truck-ramp-box', mode: 'order' },
  [PurchaseDocumentType.Invoice]: { titleKey: 'Erp::PurchaseInvoices', icon: 'fas fa-file-invoice', mode: 'invoice' },
  [PurchaseDocumentType.CreditMemo]: { titleKey: 'Erp::PurchaseCreditMemos', icon: 'fas fa-file-circle-minus', mode: 'invoice' },
  [PurchaseDocumentType.BlanketOrder]: { titleKey: 'Erp::PurchaseBlanketOrders', icon: 'fas fa-file-contract', mode: 'blanket' },
  [PurchaseDocumentType.ReturnOrder]: { titleKey: 'Erp::PurchaseReturnOrders', icon: 'fas fa-rotate-left', mode: 'order' },
};

/** The documents that can be emailed to the vendor, as the email templates know them. */
const EMAIL_TYPES: Partial<Record<PurchaseDocumentType, EmailDocumentType>> = {
  [PurchaseDocumentType.Quote]: EmailDocumentType.PurchaseQuote,
  [PurchaseDocumentType.Order]: EmailDocumentType.PurchaseOrder,
  [PurchaseDocumentType.Invoice]: EmailDocumentType.PurchaseInvoice,
};

/** An address block (buy-from, ship-to or pay-to) of the purchase card: `prefix` + Address, City, ... */
function addressFields(prefix: string, section: DocumentCardField['section'], groupKey: string): DocumentCardField[] {
  return [
    { field: `${prefix}Address`, labelKey: 'Erp::Address', type: 'text', maxLength: 100, section, groupKey },
    { field: `${prefix}Address2`, labelKey: 'Erp::Address2', type: 'text', maxLength: 50, section, groupKey },
    { field: `${prefix}PostCode`, labelKey: 'Erp::PostCode', type: 'lookup', entity: 'postCode', allowFreeText: true, section, groupKey },
    { field: `${prefix}City`, labelKey: 'Erp::City', type: 'text', maxLength: 30, section, groupKey },
    { field: `${prefix}County`, labelKey: 'Erp::County', type: 'text', maxLength: 30, section, groupKey },
    { field: `${prefix}CountryRegionCode`, labelKey: 'Erp::CountryRegionCode', type: 'lookup', entity: 'countryRegion', section, groupKey },
    { field: `${prefix}Contact`, labelKey: 'Erp::Contact', type: 'text', maxLength: 100, section, groupKey },
  ];
}

/**
 * The header of a purchase document's card, by section: General, Invoice Details, Shipping and
 * Payment, Foreign Trade and, on orders, Prepayment. Fields that only make sense for some kinds
 * of document (the vendor's invoice or credit memo no., receipt dates) follow the document type.
 */
export function purchaseCardFields(type: PurchaseDocumentType, mode: DocumentMode, prepayment: boolean): DocumentCardField[] {
  const isCreditMemo = type === PurchaseDocumentType.CreditMemo;
  const isReturn = isCreditMemo || type === PurchaseDocumentType.ReturnOrder;
  const isOrder = mode !== 'invoice';
  const general: DocumentCardField[] = [
    { field: 'buyFromVendorNo', labelKey: 'Erp::VendorNo', type: 'readonly' },
    { field: 'buyFromVendorName', labelKey: 'Erp::Name', type: 'readonly' },
    { field: 'buyFromVendorName2', labelKey: 'Erp::Name2', type: 'text', maxLength: 50 },
    ...addressFields('buyFrom', 'general', 'Erp::BuyFrom'),
    ...(isOrder ? [{ field: 'orderAddressCode', labelKey: 'Erp::OrderAddressCode', type: 'lookup', entity: 'orderAddress', allowFreeText: true, groupKey: 'Erp::BuyFrom' } as DocumentCardField] : []),
    { field: 'documentDate', labelKey: 'Erp::DocumentDate', type: 'date' },
    { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date' },
    { field: 'vatReportingDate', labelKey: 'Erp::VatReportingDate', type: 'date' },
    { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date' },
    ...(isOrder ? [{ field: 'orderDate', labelKey: 'Erp::OrderDate', type: 'date' } as DocumentCardField] : []),
    ...(isCreditMemo || type === PurchaseDocumentType.ReturnOrder
      ? [{ field: 'vendorCrMemoNo', labelKey: 'Erp::VendorCrMemoNo', type: 'text', maxLength: 35 } as DocumentCardField]
      : []),
    ...(!isCreditMemo ? [{ field: 'vendorInvoiceNo', labelKey: 'Erp::VendorInvoiceNo', type: 'text', maxLength: 35 } as DocumentCardField] : []),
    ...(isOrder
      ? [
          { field: 'vendorOrderNo', labelKey: 'Erp::VendorOrderNo', type: 'text', maxLength: 35 } as DocumentCardField,
          { field: 'vendorShipmentNo', labelKey: 'Erp::VendorShipmentNo', type: 'text', maxLength: 35 } as DocumentCardField,
        ]
      : []),
    ...(type === PurchaseDocumentType.Order ? [{ field: 'quoteNo', labelKey: 'Erp::QuoteNo', type: 'readonly' } as DocumentCardField] : []),
    { field: 'yourReference', labelKey: 'Erp::YourReference', type: 'text', maxLength: 35 },
    { field: 'postingDescription', labelKey: 'Erp::PostingDescription', type: 'text', maxLength: 100 },
    { field: 'purchaserCode', labelKey: 'Erp::PurchaserCode', type: 'lookup', entity: 'salespersonPurchaser' },
    { field: 'campaignNo', labelKey: 'Erp::CampaignNo', type: 'lookup', entity: 'campaign' },
    { field: 'responsibilityCenter', labelKey: 'Erp::ResponsibilityCenter', type: 'lookup', entity: 'responsibilityCenter' },
    { field: 'assignedUserId', labelKey: 'Erp::AssignedUserId', type: 'text', maxLength: 50 },
    ...(isReturn ? [{ field: 'reasonCode', labelKey: 'Erp::ReasonCode', type: 'lookup', entity: 'reasonCode' } as DocumentCardField] : []),
    ...(type === PurchaseDocumentType.Order
      ? [
          { field: 'requisitionNo', labelKey: 'Erp::RequisitionNo', type: 'readonly' } as DocumentCardField,
          { field: 'tenderNo', labelKey: 'Erp::TenderNo', type: 'readonly' } as DocumentCardField,
        ]
      : []),
  ];

  const invoice: DocumentCardField[] = [
    { field: 'currencyCode', labelKey: 'Erp::CurrencyCode', type: 'lookup', entity: 'currency' },
    { field: 'pricesIncludingVat', labelKey: 'Erp::PricesIncludingVat', type: 'checkbox' },
    { field: 'vendorPostingGroup', labelKey: 'Erp::VendorPostingGroup', type: 'lookup', entity: 'vendorPostingGroup' },
    { field: 'genBusPostingGroup', labelKey: 'Erp::GenBusPostingGroup', type: 'lookup', entity: 'genBusPostingGroup' },
    { field: 'vatBusPostingGroup', labelKey: 'Erp::VatBusPostingGroup', type: 'lookup', entity: 'vatBusPostingGroup' },
    { field: 'paymentTermsCode', labelKey: 'Erp::PaymentTermsCode', type: 'lookup', entity: 'paymentTerms' },
    { field: 'paymentMethodCode', labelKey: 'Erp::PaymentMethodCode', type: 'lookup', entity: 'paymentMethod' },
    { field: 'shortcutDimension1Code', labelKey: 'Erp::ShortcutDimension1Code', type: 'lookup', entity: 'globalDimension1', allowFreeText: true },
    { field: 'shortcutDimension2Code', labelKey: 'Erp::ShortcutDimension2Code', type: 'lookup', entity: 'globalDimension2', allowFreeText: true },
    { field: 'paymentDiscountPct', labelKey: 'Erp::PaymentDiscountPct', type: 'number' },
    { field: 'pmtDiscountDate', labelKey: 'Erp::PmtDiscountDate', type: 'date' },
    { field: 'vatBaseDiscountPct', labelKey: 'Erp::VatBaseDiscountPct', type: 'number' },
    { field: 'paymentReference', labelKey: 'Erp::PaymentReference', type: 'text', maxLength: 50 },
    { field: 'creditorNo', labelKey: 'Erp::CreditorNo', type: 'text', maxLength: 20 },
    { field: 'onHold', labelKey: 'Erp::OnHold', type: 'text', maxLength: 3 },
    ...(mode === 'invoice'
      ? [
          { field: 'appliesToDocNo', labelKey: 'Erp::AppliesToDocNo', type: 'text', maxLength: 20 } as DocumentCardField,
          { field: 'invoiceReceivedDate', labelKey: 'Erp::InvoiceReceivedDate', type: 'date' } as DocumentCardField,
        ]
      : []),
    { field: 'vatRegistrationNo', labelKey: 'Erp::VatRegistrationNo', type: 'text', maxLength: 20 },
    { field: 'taxLiable', labelKey: 'Erp::TaxLiable', type: 'checkbox' },
    { field: 'taxAreaCode', labelKey: 'Erp::TaxAreaCode', type: 'text', maxLength: 20 },
    { field: 'languageCode', labelKey: 'Erp::LanguageCode', type: 'text', maxLength: 10 },
  ].map(f => ({ ...f, section: 'invoice' }) as DocumentCardField);

  const shipping: DocumentCardField[] = [
    { field: 'locationCode', labelKey: 'Erp::LocationCode', type: 'lookup', entity: 'location', section: 'shipping', groupKey: 'Erp::ShipTo' },
    { field: 'shipToCode', labelKey: 'Erp::Code', type: 'text', maxLength: 10, section: 'shipping', groupKey: 'Erp::ShipTo' },
    { field: 'shipToName', labelKey: 'Erp::Name', type: 'text', maxLength: 100, section: 'shipping', groupKey: 'Erp::ShipTo' },
    { field: 'shipToName2', labelKey: 'Erp::Name2', type: 'text', maxLength: 50, section: 'shipping', groupKey: 'Erp::ShipTo' },
    ...addressFields('shipTo', 'shipping', 'Erp::ShipTo'),
    { field: 'shipmentMethodCode', labelKey: 'Erp::ShipmentMethodCode', type: 'lookup', entity: 'shipmentMethod', section: 'shipping', groupKey: 'Erp::ShipTo' },
    ...(isOrder
      ? ([
          { field: 'expectedReceiptDate', labelKey: 'Erp::ExpectedReceiptDate', type: 'date' },
          { field: 'requestedReceiptDate', labelKey: 'Erp::RequestedReceiptDate', type: 'date' },
          { field: 'promisedReceiptDate', labelKey: 'Erp::PromisedReceiptDate', type: 'date' },
          { field: 'leadTimeCalculation', labelKey: 'Erp::LeadTimeCalculation', type: 'text', maxLength: 32 },
          { field: 'inboundWhseHandlingTime', labelKey: 'Erp::InboundWhseHandlingTime', type: 'text', maxLength: 32 },
        ] as DocumentCardField[]).map(f => ({ ...f, section: 'shipping', groupKey: 'Erp::ShipTo' }) as DocumentCardField)
      : []),
    { field: 'payToVendorNo', labelKey: 'Erp::PayToVendorNo', type: 'lookup', entity: 'vendor', section: 'shipping', groupKey: 'Erp::PayTo' },
    { field: 'payToName', labelKey: 'Erp::Name', type: 'text', maxLength: 100, section: 'shipping', groupKey: 'Erp::PayTo' },
    { field: 'payToName2', labelKey: 'Erp::Name2', type: 'text', maxLength: 50, section: 'shipping', groupKey: 'Erp::PayTo' },
    ...addressFields('payTo', 'shipping', 'Erp::PayTo'),
  ];

  const foreignTrade: DocumentCardField[] = [
    { field: 'transactionType', labelKey: 'Erp::TransactionType', type: 'text', maxLength: 10 },
    { field: 'transactionSpecification', labelKey: 'Erp::TransactionSpecification', type: 'text', maxLength: 10 },
    { field: 'transportMethod', labelKey: 'Erp::TransportMethod', type: 'text', maxLength: 10 },
    { field: 'entryPoint', labelKey: 'Erp::EntryPoint', type: 'text', maxLength: 10 },
    { field: 'area', labelKey: 'Erp::Area', type: 'text', maxLength: 10 },
  ].map(f => ({ ...f, section: 'foreignTrade' }) as DocumentCardField);

  const prepaymentFields: DocumentCardField[] = prepayment
    ? [
        PREPAYMENT_FIELDS[0],
        ...([
          { field: 'compressPrepayment', labelKey: 'Erp::CompressPrepayment', type: 'checkbox' },
          { field: 'prepmtPaymentTermsCode', labelKey: 'Erp::PrepmtPaymentTermsCode', type: 'lookup', entity: 'paymentTerms' },
          { field: 'prepaymentDueDate', labelKey: 'Erp::PrepaymentDueDate', type: 'date' },
          { field: 'prepmtPaymentDiscountPct', labelKey: 'Erp::PrepmtPaymentDiscountPct', type: 'number' },
          { field: 'prepmtPmtDiscountDate', labelKey: 'Erp::PrepmtPmtDiscountDate', type: 'date' },
        ] as DocumentCardField[]).map(f => ({ ...f, section: 'prepayment' }) as DocumentCardField),
        ...PREPAYMENT_FIELDS.slice(1),
        { field: 'lastPrepmtCrMemoNo', labelKey: 'Erp::LastPrepmtCrMemoNo', type: 'readonly', section: 'prepayment' },
      ]
    : [];

  return [...general, ...invoice, ...shipping, ...foreignTrade, ...prepaymentFields];
}

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
  readonly cardFields: DocumentCardField[];
  private readonly recordDialogs = inject(RecordDialogService);

  constructor(
    private readonly documents: PurchaseDocumentService,
    private readonly vendors: VendorService,
    route: ActivatedRoute,
  ) {
    super();
    this.trackingSourceType = ItemTrackingSourceType.PurchaseLine;
    this.chargeSourceType = ItemChargeSourceType.PurchaseLine;
    this.documentType = (route.snapshot.data['documentType'] as PurchaseDocumentType) ?? PurchaseDocumentType.Invoice;
    const page = PURCHASE_PAGES[this.documentType];
    this.titleKey = page.titleKey;
    this.icon = page.icon;
    this.mode = page.mode;
    this.emailDocumentType = EMAIL_TYPES[this.documentType] ?? null;
    this.supportsPrepayment = this.documentType === PurchaseDocumentType.Order;

    const isReturn = this.documentType === PurchaseDocumentType.ReturnOrder || this.documentType === PurchaseDocumentType.CreditMemo;
    this.shipLabelKey = isReturn ? 'Erp::ShipReturn' : 'Erp::Receive';
    this.shippedLabelKey = isReturn ? 'Erp::QuantityReturned' : 'Erp::QuantityReceived';
    this.qtyToShipLabelKey = isReturn ? 'Erp::QtyToShip' : 'Erp::QtyToReceive';
    this.sourceLinesLabelKey = isReturn ? 'Erp::GetReturnShipmentLines' : 'Erp::GetReceiptLines';
    // An invoice carries the vendor's invoice number; anything else the vendor's order number.
    this.externalDocumentNoLabelKey = this.mode === 'invoice' ? 'Erp::VendorInvoiceNo' : 'Erp::VendorOrderNo';

    this.cardFields = purchaseCardFields(this.documentType, this.mode, this.supportsPrepayment);
  }

  protected loadDocument(id: string): Observable<DocumentCardData> {
    return this.documents.get(id) as unknown as Observable<DocumentCardData>;
  }

  protected updateDocument(id: string, input: Record<string, unknown>): Observable<unknown> {
    return this.documents.update(id, input as never);
  }

  protected toCardLine(raw: Record<string, unknown>): DocumentCardLine {
    const l = raw as unknown as PurchaseLineDto;
    return {
      id: l.id ?? null,
      type: l.type,
      no: l.no ?? '',
      description: l.description ?? '',
      locationCode: l.locationCode ?? null,
      quantity: l.quantity,
      unitOfMeasureCode: l.unitOfMeasureCode ?? null,
      unitAmount: l.directUnitCost,
      lineDiscountPercent: l.lineDiscountPercent,
      lineAmount: l.lineAmount,
      vatPercent: l.vatPercent,
      qtyToShip: l.qtyToReceive,
      quantityShipped: l.quantityReceived,
      qtyToInvoice: l.qtyToInvoice,
      quantityInvoiced: l.quantityInvoiced,
    };
  }

  protected fromCardLine(l: DocumentCardLine, original: Record<string, unknown> | undefined): Record<string, unknown> {
    // Quantities to post only for saved lines: a new line starts with everything outstanding.
    const toPost = this.isOrderMode && l.id ? { qtyToReceive: Number(l.qtyToShip) || 0, qtyToInvoice: Number(l.qtyToInvoice) || 0 } : {};
    return {
      ...(original ?? {}),
      id: l.id ?? undefined,
      type: l.type,
      no: l.no,
      description: l.description,
      quantity: Number(l.quantity) || 0,
      directUnitCost: Number(l.unitAmount) || 0,
      lineDiscountPercent: Number(l.lineDiscountPercent) || 0,
      unitOfMeasureCode: l.unitOfMeasureCode || undefined,
      locationCode: l.locationCode || undefined,
      // The header's prepayment % applies to every line not prepaid yet.
      prepaymentPct: undefined,
      qtyToReceive: undefined,
      qtyToInvoice: undefined,
      ...toPost,
    };
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

  protected override extraCardActions(card: DocumentCardData): CardAction[] {
    return card.status === DocumentStatus.Open
      ? [{ key: 'insertStandardLines', labelKey: 'Erp::GetStandardLines', icon: 'fas fa-repeat', cssClass: 'btn-outline-secondary', permission: 'Erp.PurchaseDocuments.Update' }]
      : [];
  }

  protected override runExtraCardAction(_key: string, row: PurchaseHeaderDto): Observable<string> {
    return runWithInput<InsertStandardLinesInput>(this.recordDialogs, {
      titleKey: 'Erp::GetStandardLines',
      icon: 'fas fa-repeat',
      permission: 'Erp.PurchaseDocuments.Update',
      fields: [{ field: 'code', labelKey: 'Erp::StandardCode', type: 'lookup', lookupEntity: 'standardPurchaseCode', required: true }],
      submit: input => this.documents.insertStandardLines(row.id!, input),
    }).pipe(map(() => 'Erp::StandardLinesInserted'));
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
          isCharge: l.type === DocumentLineType.ChargeItem,
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
