import { LocalizationService, PagedResultDto, PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { DestroyRef, Directive, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DocumentLineType, DocumentStatus, documentLineTypeOptions, documentStatusOptions } from '@proxy/documents';
import {
  ApprovalEntryDto,
  ApprovalEntryService,
  ApprovalRequestResultDto,
  ApprovalStatus,
} from '@proxy/workflows';
import { Observable, Subscription, finalize } from 'rxjs';
import { ChatterWidgetComponent } from '../../components/chatter-widget/chatter-widget.component';
import {
  DocumentAction as CardAction,
  DocumentLineChange,
  DocumentLineColumn,
  DocumentTotal,
  ErpExportColumn,
  ErpExportOptions,
  ErpExportService,
  ErpTableColumn,
  ErpTableComponent,
  ErpTableQuery,
  ErpTableSource,
  LookupItem,
  calculateDocumentTotals,
} from '../../erp-shared';
import { CompanyService } from '../../services/company.service';
import { RecordDialogService } from '../../erp-shared/record/record-dialog.service';
import { ItemTrackingLineService, ItemTrackingSourceType } from '@proxy/inventory';
import { itemTrackingLinesFor } from '../../master-data/warehouse-entities';

/** The fields the list page needs from a sales or purchase header. */
export interface DocumentRow {
  id?: string;
  no?: string;
  postingDate?: string;
  status?: DocumentStatus;
  totalAmount?: number;
  totalAmountIncludingVat?: number;
  posted?: boolean;
  postedDocumentNo?: string;
}

export interface NewDocumentInput {
  no: string | null;
  partyId: string;
  postingDate: string;
  /** Where the stock is shipped from or received at; blank uses the location-less setup. */
  locationCode: string | null;
  /** The party's own reference: the customer's order no. or the vendor's invoice no. */
  externalDocumentNo: string | null;
  lines: {
    type: DocumentLineType;
    no: string;
    description: string;
    quantity: number;
    unitAmount: number;
  }[];
}

export type DocumentAction =
  'release' | 'reopen' | 'sendApprovalRequest' | 'cancelApprovalRequest' | 'runPosting' | 'delete' | 'makeOrder';

/**
 * How a kind of document is posted:
 * - `invoice`: an invoice or credit memo, shipped and invoiced in one go;
 * - `order`: an order or return order, shipped (received) and invoiced in parts;
 * - `quote` / `blanket`: never posted, made into an order instead.
 */
export type DocumentMode = 'invoice' | 'order' | 'quote' | 'blanket';

/** A line of the selected document with what has been shipped (received) and invoiced on it. */
export interface DocumentLineRow {
  id: string;
  lineNo: number;
  no?: string;
  description?: string;
  quantity: number;
  qtyToShip: number;
  quantityShipped: number;
  qtyToInvoice: number;
  quantityInvoiced: number;
  /** The posted shipment (receipt) line an invoice line was taken from. */
  sourceNo?: string;
  /** An item line, which can carry lot and serial numbers. */
  isItem?: boolean;
}

/** A posted shipment (receipt) line still waiting for an invoice. */
export interface OpenSourceLine {
  id: string;
  documentNo?: string;
  orderNo?: string;
  postingDate?: string;
  lineNo: number;
  no?: string;
  description?: string;
  qtyNotInvoiced: number;
  unitAmount: number;
}

/** One line of the order posting dialog. */
export interface PostingLine {
  lineId: string;
  lineNo: number;
  no?: string;
  description?: string;
  quantity: number;
  shipped: number;
  invoiced: number;
  qtyToShip: number;
  qtyToInvoice: number;
}

export interface PostingResult {
  invoiceNo?: string;
  shipmentNo?: string;
}

/** A header field on the document card's General section. */
export interface DocumentCardField {
  field: string;
  labelKey: string;
  type: 'text' | 'date' | 'lookup' | 'readonly';
  /** `lookup` only: the record entity looked up, e.g. `location`. */
  entity?: string;
  maxLength?: number;
}

/** A sales or purchase header as the card loads it: every field, with its lines. */
export type DocumentCardData = Record<string, unknown> & DocumentRow & { lines?: Record<string, unknown>[] };

/** One line on the document card, the same for sales and purchases. */
export interface DocumentCardLine {
  id: string | null;
  type: DocumentLineType;
  no: string;
  description: string;
  locationCode: string | null;
  quantity: number;
  unitOfMeasureCode: string | null;
  unitAmount: number;
  lineDiscountPercent: number;
  lineAmount: number;
  vatPercent: number;
  /** Qty. to Ship / Qty. to Receive. */
  qtyToShip: number;
  /** Quantity Shipped / Quantity Received. */
  quantityShipped: number;
  qtyToInvoice: number;
  quantityInvoiced: number;
}

/**
 * Sales and purchase documents behave identically: list, create, release, reopen, ask for
 * approval, post. Orders also ship (receive) and invoice in parts, quotes and blanket orders are
 * made into orders, and invoices take their lines from posted shipments (receipts). Subclasses
 * supply the API and the wording; they share one template.
 */
@Directive()
export abstract class DocumentListBase<TRow extends DocumentRow> implements OnInit {
  protected readonly destroyRef = inject(DestroyRef);
  protected readonly fb = inject(FormBuilder);
  protected readonly toaster = inject(ToasterService);
  protected readonly confirmation = inject(ConfirmationService);
  protected readonly companyService = inject(CompanyService);
  protected readonly approvalEntries = inject(ApprovalEntryService);
  protected readonly exportService = inject(ErpExportService);
  protected readonly localization = inject(LocalizationService);
  private readonly permissions = inject(PermissionService);
  private readonly dialogs = inject(RecordDialogService);
  private readonly trackingLines = inject(ItemTrackingLineService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly DocumentStatus = DocumentStatus;
  readonly ApprovalStatus = ApprovalStatus;

  viewMode: 'table' | 'card' = (localStorage.getItem('nexerp_doc_list_view_mode') as 'table' | 'card') || 'table';

  onViewModeChange(mode: 'table' | 'card'): void {
    this.viewMode = mode;
    try {
      localStorage.setItem('nexerp_doc_list_view_mode', mode);
    } catch {}
  }

  onExport(format: 'excel' | 'csv' | 'print' | 'clipboard'): void {
    if (!this.loadedRows.length) {
      this.toaster.info('Erp::Table:NothingToExport');
      return;
    }

    const exportColumns: ErpExportColumn[] = [
      { field: 'no', title: this.localization.instant('Erp::No') || 'Document No' },
      { field: 'party', title: this.localization.instant(this.partyLabelKey) || 'Party', formatter: (_, row) => this.nameOf(row) },
      { field: 'postingDate', title: this.localization.instant('Erp::PostingDate') || 'Posting Date', type: 'date' },
      { field: 'totalAmount', title: this.localization.instant('Erp::Amount') || 'Amount', type: 'currency' },
      { field: 'status', title: this.localization.instant('Erp::Status') || 'Status', formatter: (_, row) => this.statusName(row.status) },
    ];

    const options: ErpExportOptions = {
      fileName: `${this.titleKey.replace(/^Erp::/, '')}_${new Date().toISOString().substring(0, 10)}`,
      title: this.localization.instant(this.titleKey) || this.titleKey.replace(/^Erp::/, ''),
      sheetName: 'Documents',
      companyName: this.companyService.getActiveCompanyName(),
    };

    switch (format) {
      case 'excel':
        this.exportService.exportToExcel(exportColumns, this.loadedRows, options);
        break;
      case 'csv':
        this.exportService.exportToCsv(exportColumns, this.loadedRows, options);
        break;
      case 'print':
        this.exportService.print(exportColumns, this.loadedRows, options);
        break;
      case 'clipboard':
        this.exportService.copyToClipboard(exportColumns, this.loadedRows);
        break;
    }
  }

  /** Wording and permissions, e.g. 'Erp::SalesInvoices' and 'Erp.SalesDocuments'. */
  abstract readonly titleKey: string;
  abstract readonly icon: string;
  abstract readonly partyLabelKey: string;
  abstract readonly unitAmountLabelKey: string;
  abstract readonly permissionPrefix: string;
  /** "SalesHeader" / "PurchaseHeader": the key of the record's chatter thread. */
  abstract readonly chatterEntityType: string;
  /** Header fields holding the party's number and name, e.g. `sellToCustomerNo` / `sellToCustomerName`. */
  abstract readonly partyNoField: string;
  abstract readonly partyNameField: string;
  /** Record entity of the party lookup: `customer` / `vendor`. */
  abstract readonly partyEntity: string;
  /** Label of the party's own reference: External Document No. or Vendor Invoice No. */
  abstract readonly externalDocumentNoLabelKey: string;
  /** What an item line takes as its unit amount: the item's price on sales, its cost on purchases. */
  abstract readonly itemAmountField: 'unitPrice' | 'unitCost';

  /** How the documents of this page are posted. */
  mode: DocumentMode = 'invoice';
  /** "Ship" on sales, "Receive" on purchases; on a return document the goods move the other way. */
  shipLabelKey = 'Erp::Ship';
  shippedLabelKey = 'Erp::QuantityShipped';
  /** "Get Shipment Lines" / "Get Receipt Lines". */
  sourceLinesLabelKey = 'Erp::GetShipmentLines';
  /** "Qty. to Ship" / "Qty. to Receive". */
  qtyToShipLabelKey = 'Erp::QtyToShip';

  /** The lines of the selected document, with what has been posted on them. */
  selectedLines: DocumentLineRow[] = [];

  /** Where the lots and serial numbers of this page's lines are kept. */
  protected trackingSourceType: ItemTrackingSourceType | null = null;

  get canTrack(): boolean {
    return this.trackingSourceType !== null;
  }

  /** Opens the lot and serial numbers of an item line. */
  openTracking(line: DocumentLineRow): void {
    if (this.trackingSourceType === null || !line.no) {
      return;
    }
    this.dialogs
      .openList(itemTrackingLinesFor(this.trackingLines, this.trackingSourceType, line.id, line.no))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  isPostOpen = false;
  postChoice: 'ship' | 'invoice' | 'both' = 'both';
  postLines: PostingLine[] = [];
  postingRow: TRow | null = null;

  isSourceOpen = false;
  sourceLines: OpenSourceLine[] = [];
  sourceSelection = new Set<string>();
  sourceRow: TRow | null = null;

  get isOrderMode(): boolean {
    return this.mode === 'order';
  }

  get makesOrders(): boolean {
    return this.mode === 'quote' || this.mode === 'blanket';
  }

  /** Lines of the selected document, with what has been shipped and invoiced on them. */
  protected loadLines(_id: string): Observable<DocumentLineRow[]> | null {
    return null;
  }

  /** Posts an order: ship (receive), invoice or both, with the quantities per line. */
  protected postOrder(_id: string, _ship: boolean, _invoice: boolean, _lines: PostingLine[]): Observable<PostingResult> | null {
    return null;
  }

  /** The posted shipment (receipt) lines an invoice can take. */
  protected openSourceLines(_id: string): Observable<OpenSourceLine[]> | null {
    return null;
  }

  /** Adds posted shipment (receipt) lines to an invoice. */
  protected collectSourceLines(_id: string, _lineIds: string[]): Observable<unknown> | null {
    return null;
  }

  /**
   * Only the documents of this customer / vendor (`?partyId=` from the smart button on its card).
   * Subclasses pass it to their list query.
   */
  partyFilter: string | null = null;

  @ViewChild(ChatterWidgetComponent) private chatter?: ChatterWidgetComponent;
  @ViewChild(ErpTableComponent) protected table?: ErpTableComponent<TRow>;

  columns: ErpTableColumn<TRow>[] = [];
  readonly source: ErpTableSource<TRow> = query => this.getList(query);

  /** The rows the grid has loaded so far: what is exported. */
  loadedRows: TRow[] = [];
  selected: TRow | null = null;
  /** The grid's highlighted row, kept as one array so the grid is not re-bound on every check. */
  selection: TRow[] = [];
  history: ApprovalEntryDto[] = [];
  busyId: string | null = null;
  private linesSub?: Subscription;
  private historySub?: Subscription;

  isModalOpen = false;
  isBusy = false;
  form!: FormGroup;
  lineColumns: DocumentLineColumn[] = [];

  readonly canSeeApprovals = this.permissions.getGrantedPolicy('Erp.Workflows');

  get lines(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  get totals() {
    return calculateDocumentTotals(
      this.lines.getRawValue().map((l: { quantity: number; unitAmount: number }) => ({
        quantity: Number(l.quantity) || 0,
        unitPrice: Number(l.unitAmount) || 0,
      })),
    );
  }

  protected abstract getList(query: ErpTableQuery): Observable<PagedResultDto<TRow>>;
  protected abstract partyName(row: TRow): string;
  protected abstract searchParties(term: string): Observable<LookupItem[]>;
  protected abstract createDocument(input: NewDocumentInput): Observable<TRow>;
  protected abstract run(action: DocumentAction, id: string): Observable<unknown>;

  /** The header fields of the card's General section. */
  abstract readonly cardFields: DocumentCardField[];
  /** The whole document, header and lines. */
  protected abstract loadDocument(id: string): Observable<DocumentCardData>;
  /** Saves the whole document; lines missing from `input.lines` are deleted. */
  protected abstract updateDocument(id: string, input: Record<string, unknown>): Observable<unknown>;
  protected abstract toCardLine(line: Record<string, unknown>): DocumentCardLine;
  /** The line as the update takes it; `original` is the saved line, so fields the card does not show are kept. */
  protected abstract fromCardLine(line: DocumentCardLine, original: Record<string, unknown> | undefined): Record<string, unknown>;

  // --- Document card: the header above, its lines below ---

  /** The document open on its card; null shows the list. */
  card: DocumentCardData | null = null;
  cardForm: FormGroup = new FormGroup({});
  cardLines = new FormArray<FormGroup>([]);
  cardLineColumns: DocumentLineColumn[] = [];
  cardTotals: DocumentTotal[] = [];
  cardBusy = false;
  private cardOriginalLines = new Map<string, Record<string, unknown>>();
  private cardSubs: Subscription[] = [];

  readonly cardStatusColors: Record<string, string> = {
    'Erp::Enum:DocumentStatus.Open': 'secondary',
    'Erp::Enum:DocumentStatus.PendingApproval': 'warning',
    'Erp::Enum:DocumentStatus.Released': 'primary',
    'Erp::Enum:DocumentStatus.Posted': 'success',
  };

  /** Header and lines can be changed only while the document is open. */
  get cardEditable(): boolean {
    return !!this.card && this.card.status === DocumentStatus.Open && !this.card.posted && this.canUpdate;
  }

  get cardDirty(): boolean {
    return this.cardForm.dirty || this.cardLines.dirty;
  }

  get canUpdate(): boolean {
    return this.permissions.getGrantedPolicy(this.permissionPrefix + '.Update');
  }

  /** The card's action bar, following the document's status. */
  get cardActions(): CardAction[] {
    const card = this.card;
    if (!card || card.posted) {
      return [];
    }

    const status = card.status;
    const update = this.permissionPrefix + '.Update';
    const actions: CardAction[] = [
      { key: 'save', labelKey: 'Erp::Save', icon: 'fas fa-save', cssClass: 'btn-primary', permission: update,
        disabled: !this.cardEditable || !this.cardDirty },
    ];
    if (status === DocumentStatus.Open) {
      actions.push(
        { key: 'release', labelKey: 'Erp::Release', icon: 'fas fa-lock', permission: update },
        { key: 'sendApprovalRequest', labelKey: 'Erp::SendApprovalRequest', icon: 'fas fa-paper-plane', cssClass: 'btn-outline-warning', permission: update },
      );
      if (this.mode === 'invoice') {
        actions.push({ key: 'getSourceLines', labelKey: this.sourceLinesLabelKey, icon: 'fas fa-truck-ramp-box', cssClass: 'btn-outline-info', permission: update });
      }
    }
    if (status === DocumentStatus.PendingApproval) {
      actions.push({ key: 'cancelApprovalRequest', labelKey: 'Erp::CancelApprovalRequest', icon: 'fas fa-undo', cssClass: 'btn-outline-secondary', permission: update });
    }
    if (status === DocumentStatus.Released) {
      actions.push({ key: 'reopen', labelKey: 'Erp::Reopen', icon: 'fas fa-lock-open', cssClass: 'btn-outline-secondary', permission: update });
    }
    if (this.makesOrders) {
      actions.push({ key: 'makeOrder', labelKey: 'Erp::MakeOrder', icon: 'fas fa-file-signature', cssClass: 'btn-outline-success', permission: this.permissionPrefix + '.Create' });
    } else if (status !== DocumentStatus.PendingApproval) {
      actions.push({ key: 'runPosting', labelKey: 'Erp::Post', icon: 'fas fa-check-circle', cssClass: 'btn-success', permission: this.permissionPrefix + '.Post' });
    }
    if (status === DocumentStatus.Open) {
      actions.push({ key: 'delete', labelKey: 'Erp::Delete', icon: 'fas fa-trash', cssClass: 'btn-outline-danger', permission: this.permissionPrefix + '.Delete' });
    }
    return actions;
  }

  /** Opens a document on its card, as a link to it (`?open=<id>`) does. */
  openCard(row: TRow | null, event?: Event): void {
    event?.stopPropagation();
    if (!row?.id) {
      return;
    }
    if (this.card?.id !== row.id) {
      this.router.navigate([], { relativeTo: this.route, queryParams: { open: row.id }, queryParamsHandling: 'merge', replaceUrl: true });
    }
    this.select(row, true);
    this.reloadCard(row.id);
  }

  /** Back to the list; unsaved changes are confirmed first. */
  closeCard(): void {
    const leave = () => this.dropCard();
    if (!this.cardEditable || !this.cardDirty) {
      leave();
      return;
    }
    this.confirmation
      .warn('Erp::DiscardChangesConfirmation', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          leave();
        }
      });
  }

  /** Leaves the card without asking, e.g. once its document is gone. */
  protected dropCard(): void {
    this.card = null;
    this.cardSubs.forEach(s => s.unsubscribe());
    this.router.navigate([], { relativeTo: this.route, queryParams: { open: null }, queryParamsHandling: 'merge', replaceUrl: true });
  }

  protected reloadCard(id: string): void {
    this.cardBusy = true;
    this.loadDocument(id)
      .pipe(
        finalize(() => (this.cardBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(document => this.showCard(document));
  }

  private showCard(document: DocumentCardData): void {
    this.cardSubs.forEach(s => s.unsubscribe());
    this.card = document;
    this.selected = document as unknown as TRow;
    this.selection = [this.selected];

    const header: Record<string, unknown> = {};
    for (const f of this.cardFields) {
      const value = document[f.field];
      header[f.field] = [
        f.type === 'date' ? toDateInput(value) : (value ?? null),
        f.maxLength ? Validators.maxLength(f.maxLength) : [],
      ];
    }
    this.cardForm = this.fb.group(header);

    this.cardOriginalLines = new Map();
    const lines = (document.lines ?? [])
      .slice()
      .sort((a, b) => Number(a['lineNo']) - Number(b['lineNo']));
    for (const line of lines) {
      this.cardOriginalLines.set(String(line['id']), line);
    }
    this.cardLines = this.fb.array(lines.map(l => this.buildCardLine(this.toCardLine(l))));
    this.cardLineColumns = this.buildCardLineColumns();

    if (!this.cardEditable) {
      this.cardForm.disable({ emitEvent: false });
      this.cardLines.disable({ emitEvent: false });
    }

    this.recalculateCard();
    this.cardSubs = [this.cardLines.valueChanges.subscribe(() => this.recalculateCard())];
  }

  addCardLine(): void {
    this.cardLines.push(this.buildCardLine());
    this.cardLines.markAsDirty();
  }

  removeCardLine(index: number): void {
    this.cardLines.removeAt(index);
    this.cardLines.markAsDirty();
  }

  /**
   * A picked item or account fills the line; a changed quantity resets what is to be shipped
   * (received) and invoiced to what is still outstanding.
   */
  onCardLineChange(change: DocumentLineChange): void {
    this.applyLineChange(this.cardLines, change);
    const line = this.cardLines.at(change.index) as FormGroup | undefined;
    if (!line) {
      return;
    }
    if (change.field === 'no' && change.item?.data && line.get('type')?.value === DocumentLineType.Item) {
      const unit = change.item.data['baseUnitOfMeasureCode'];
      if (unit) {
        line.patchValue({ unitOfMeasureCode: unit });
      }
    }
    if (change.field === 'quantity' && this.isOrderMode) {
      const v = line.getRawValue() as DocumentCardLine;
      line.patchValue({
        qtyToShip: Math.max((Number(v.quantity) || 0) - (Number(v.quantityShipped) || 0), 0),
        qtyToInvoice: Math.max((Number(v.quantity) || 0) - (Number(v.quantityInvoiced) || 0), 0),
      });
    }
  }

  /** Opens the lot and serial numbers of a saved item line on the card. */
  openCardLine(index: number): void {
    const v = this.cardLines.at(index)?.getRawValue() as DocumentCardLine | undefined;
    if (!v?.id || v.type !== DocumentLineType.Item || !this.canTrack) {
      return;
    }
    this.openTracking({ id: v.id, lineNo: index + 1, no: v.no, quantity: v.quantity } as DocumentLineRow);
  }

  /** Runs an action of the card's bar; changes are saved first, as leaving a field would. */
  onCardAction(key: string): void {
    const card = this.card;
    if (!card?.id) {
      return;
    }
    if (key === 'save') {
      this.saveCard();
      return;
    }

    const row = card as unknown as TRow;
    const run = () => (key === 'getSourceLines' ? this.openSource(row) : this.act(key as DocumentAction, row));
    if (key !== 'delete' && this.cardEditable && this.cardDirty) {
      this.saveCard(run);
    } else {
      run();
    }
  }

  saveCard(then?: () => void): void {
    const card = this.card;
    if (!card?.id || this.cardBusy || this.cardForm.invalid || this.cardLines.invalid) {
      this.cardForm.markAllAsTouched();
      this.cardLines.markAllAsTouched();
      return;
    }

    const header = this.cardForm.getRawValue() as Record<string, unknown>;
    for (const f of this.cardFields) {
      if (f.type === 'readonly') {
        delete header[f.field];
      } else if (f.type === 'date' || header[f.field] === '') {
        header[f.field] = header[f.field] || null;
      }
    }

    const lines = (this.cardLines.getRawValue() as DocumentCardLine[])
      .filter(l => l.id || l.no || l.description)
      .map(l => this.fromCardLine(l, l.id ? this.cardOriginalLines.get(l.id) : undefined));

    this.cardBusy = true;
    this.updateDocument(card.id, { ...card, ...header, lines })
      .pipe(
        finalize(() => (this.cardBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.toaster.success('Erp::SavedSuccessfully');
        this.table?.reload();
        this.reloadCard(card.id as string);
        then?.();
      });
  }

  private recalculateCard(): void {
    const totals = calculateDocumentTotals(
      (this.cardLines.getRawValue() as DocumentCardLine[]).map(l => ({
        quantity: Number(l.quantity) || 0,
        unitPrice: Number(l.unitAmount) || 0,
        lineDiscountPercent: Number(l.lineDiscountPercent) || 0,
        vatPercent: Number(l.vatPercent) || 0,
      })),
    );
    totals.lines.forEach((t, i) => {
      const control = this.cardLines.at(i)?.get('lineAmount');
      if (control && control.value !== t.lineAmount) {
        control.setValue(t.lineAmount, { emitEvent: false });
      }
    });
    this.cardTotals = [
      { labelKey: 'Erp::TotalExclVat', value: totals.amountExclVat },
      { labelKey: 'Erp::TotalVat', value: totals.vatAmount },
      { labelKey: 'Erp::TotalInclVat', value: totals.amountInclVat, emphasis: true },
    ];
  }

  private buildCardLine(value?: DocumentCardLine): FormGroup {
    const v: DocumentCardLine = value ?? {
      id: null,
      type: DocumentLineType.Item,
      no: '',
      description: '',
      locationCode: (this.card?.['locationCode'] as string) ?? null,
      quantity: 0,
      unitOfMeasureCode: null,
      unitAmount: 0,
      lineDiscountPercent: 0,
      lineAmount: 0,
      vatPercent: 0,
      qtyToShip: 0,
      quantityShipped: 0,
      qtyToInvoice: 0,
      quantityInvoiced: 0,
    };
    return this.fb.group({
      ...v,
      no: [v.no, Validators.maxLength(20)],
      description: [v.description, Validators.maxLength(250)],
      quantity: [v.quantity, Validators.min(0)],
      unitAmount: [v.unitAmount, Validators.min(0)],
      lineDiscountPercent: [v.lineDiscountPercent, [Validators.min(0), Validators.max(100)]],
      qtyToShip: [v.qtyToShip, Validators.min(0)],
      qtyToInvoice: [v.qtyToInvoice, Validators.min(0)],
    });
  }

  /** The lines section: what the document is for, then what has been shipped (received) and invoiced. */
  private buildCardLineColumns(): DocumentLineColumn[] {
    const [type, no, description] = this.lineColumns;
    const columns: DocumentLineColumn[] = [
      type,
      no,
      description,
      { field: 'locationCode', labelKey: 'Erp::LocationCode', type: 'lookup', width: '120px', lookupEntity: 'location' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: '100px', step: 1 },
      { field: 'unitOfMeasureCode', labelKey: 'Erp::UnitOfMeasureCode', type: 'lookup', width: '110px', lookupEntity: 'unitOfMeasure' },
      { field: 'unitAmount', labelKey: this.unitAmountLabelKey, type: 'currency', width: '130px', step: 0.01 },
      { field: 'lineDiscountPercent', labelKey: 'Erp::LineDiscountPercent', type: 'number', width: '90px', step: 0.01 },
      { field: 'lineAmount', labelKey: 'Erp::LineAmount', type: 'currency', width: '130px', readonly: true },
    ];
    if (this.mode === 'order') {
      columns.push(
        { field: 'qtyToShip', labelKey: this.qtyToShipLabelKey, type: 'number', width: '100px', step: 1 },
        { field: 'quantityShipped', labelKey: this.shippedLabelKey, type: 'number', width: '100px', readonly: true },
        { field: 'qtyToInvoice', labelKey: 'Erp::QtyToInvoice', type: 'number', width: '100px', step: 1 },
        { field: 'quantityInvoiced', labelKey: 'Erp::QuantityInvoiced', type: 'number', width: '100px', readonly: true },
      );
    } else if (this.mode === 'blanket') {
      columns.push({ field: 'quantityShipped', labelKey: 'Erp::QuantityOrdered', type: 'number', width: '100px', readonly: true });
    }
    return columns;
  }

  readonly partySource = (term: string) => this.searchParties(term);
  readonly nameOf = (row: TRow) => this.partyName(row);

  ngOnInit(): void {
    this.partyFilter = this.route.snapshot.queryParamMap.get('partyId');
    const open = this.route.snapshot.queryParamMap.get('open');
    if (open) {
      this.openCard({ id: open } as TRow);
    }
    this.lineColumns = [
      {
        field: 'type',
        labelKey: 'Erp::Type',
        type: 'select',
        width: '140px',
        options: documentLineTypeOptions.map(o => ({
          value: o.value,
          label: 'Erp::Enum:DocumentLineType.' + o.key,
        })),
      },
      {
        field: 'no',
        labelKey: 'Erp::No',
        type: 'lookup',
        width: '170px',
        // Lines of other types (resource, charge...) have no table here yet: plain text.
        lookupEntity: row => lineEntityOf(row.get('type')?.value),
        lookupAllowFreeText: true,
      },
      { field: 'description', labelKey: 'Erp::Description', type: 'text' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: '110px', step: 1 },
      {
        field: 'unitAmount',
        labelKey: this.unitAmountLabelKey,
        type: 'currency',
        width: '140px',
        step: 0.01,
      },
    ];

    this.columns = [
      { field: 'no', labelKey: 'Erp::No', type: 'code', width: 150 },
      { field: this.partyNoField, labelKey: this.partyLabelKey, width: 120 },
      { field: this.partyNameField, labelKey: 'Erp::Name', width: 200 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'totalAmount', labelKey: 'Erp::Amount', type: 'currency', width: 130 },
      { field: 'totalAmountIncludingVat', labelKey: 'Erp::AmountIncludingVat', type: 'currency', width: 150 },
      {
        field: 'status',
        labelKey: 'Erp::Status',
        type: 'badge',
        width: 140,
        options: documentStatusOptions.map(o => ({ value: o.value, label: 'Erp::Enum:DocumentStatus.' + o.key })),
        badgeClass: row => this.statusClass(row.status),
      },
    ];

    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.select(null);
      if (this.card) {
        this.dropCard();
      }
      this.table?.reload();
    });
  }

  /**
   * Keeps the selection on the same document with its fresh status, or moves it to the first
   * row when that document is gone. A document whose status is unchanged keeps its history.
   */
  onRowsChange(rows: TRow[]): void {
    this.loadedRows = rows;
    const current = this.selected;
    const next = rows.find(r => r.id === current?.id) ?? rows[0] ?? null;
    if (next && current && next.id === current.id && next.status === current.status) {
      this.selected = next;
      this.selection = [next];
    } else {
      this.select(next);
    }
  }

  clearPartyFilter(): void {
    this.partyFilter = null;
    this.router.navigate([], { relativeTo: this.route, queryParams: {} });
    this.table?.reload();
  }

  /**
   * Picking an item or G/L account fills the line, as validating "No." does:
   * description, and for an item its price (sales) or cost (purchases). A new line type clears
   * the number, which pointed into the other table.
   */
  onLineChange(change: DocumentLineChange): void {
    this.applyLineChange(this.lines, change);
  }

  private applyLineChange(lines: FormArray, change: DocumentLineChange): void {
    const line = lines.at(change.index) as FormGroup | undefined;
    if (!line) {
      return;
    }
    if (change.field === 'type') {
      line.patchValue({ no: '', description: '' });
      return;
    }
    const record = change.item?.data;
    if (change.field !== 'no' || !record) {
      return;
    }
    line.patchValue({ description: change.item?.name ?? '' });
    if (line.get('type')?.value === DocumentLineType.Item) {
      line.patchValue({ unitAmount: Number(record[this.itemAmountField]) || 0 });
    }
  }

  select(row: TRow | null, force = false): void {
    if (
      !force &&
      row &&
      this.selected &&
      row.id === this.selected.id &&
      row.status === this.selected.status
    ) {
      this.selected = row;
      this.selection = [row];
      return;
    }

    this.selected = row;
    this.selection = row ? [row] : [];
    this.history = [];
    this.selectedLines = [];
    this.linesSub?.unsubscribe();
    this.historySub?.unsubscribe();

    const lines = row?.id ? this.loadLines(row.id) : null;
    this.linesSub = lines
      ?.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.selectedLines = result));

    if (row?.id && this.canSeeApprovals) {
      this.historySub = this.approvalEntries
        .getList({ documentId: row.id, maxResultCount: 50, skipCount: 0 } as never)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(result => (this.history = result.items ?? []));
    }
  }

  statusName(status?: DocumentStatus): string {
    return DocumentStatus[status ?? DocumentStatus.Open];
  }

  statusClass(status?: DocumentStatus): string {
    switch (status) {
      case DocumentStatus.Released:
        return 'bg-primary';
      case DocumentStatus.PendingApproval:
        return 'bg-warning text-dark';
      case DocumentStatus.Posted:
        return 'bg-success';
      default:
        return 'bg-secondary';
    }
  }

  approvalStatusName(status?: ApprovalStatus): string {
    return ApprovalStatus[status ?? ApprovalStatus.Created];
  }

  /** Posting is irreversible, so it is confirmed; the other transitions can be undone. */
  act(action: DocumentAction, row: TRow, event?: Event): void {
    event?.stopPropagation();
    if (!row.id || this.busyId) {
      return;
    }

    // An order is posted through the dialog that says what to ship and invoice.
    if (action === 'runPosting' && this.isOrderMode) {
      this.openPost(row);
      return;
    }

    if (action === 'runPosting' || action === 'delete' || action === 'makeOrder') {
      this.confirmation
        .warn(
          action === 'delete'
            ? 'Erp::ItemWillBeDeletedMessage'
            : action === 'makeOrder'
              ? 'Erp::MakeOrderConfirmation'
              : 'Erp::PostDocumentConfirmation',
          'Erp::AreYouSure',
        )
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(status => {
          if (status === Confirmation.Status.confirm) {
            this.execute(action, row);
          }
        });
      return;
    }

    this.execute(action, row);
  }

  /** Opens the order posting dialog with each line's outstanding quantities. */
  openPost(row: TRow): void {
    const lines = this.loadLines(row.id as string);
    if (!lines) {
      return;
    }

    lines.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
      this.postingRow = row;
      this.postChoice = 'both';
      this.postLines = result.map(l => ({
        lineId: l.id,
        lineNo: l.lineNo,
        no: l.no,
        description: l.description,
        quantity: l.quantity,
        shipped: l.quantityShipped,
        invoiced: l.quantityInvoiced,
        qtyToShip: l.qtyToShip,
        qtyToInvoice: l.qtyToInvoice,
      }));
      this.isPostOpen = true;
    });
  }

  /** Invoicing cannot go beyond what has been shipped, or is being shipped now. */
  maxToInvoice(line: PostingLine): number {
    const shipping = this.postChoice === 'invoice' ? 0 : Number(line.qtyToShip) || 0;
    return line.shipped - line.invoiced + shipping;
  }

  confirmPost(): void {
    const row = this.postingRow;
    if (!row?.id || this.isBusy) {
      return;
    }

    const ship = this.postChoice !== 'invoice';
    const invoice = this.postChoice !== 'ship';
    const request = this.postOrder(row.id, ship, invoice, this.postLines.map(l => ({
      ...l,
      qtyToShip: ship ? Number(l.qtyToShip) || 0 : 0,
      qtyToInvoice: invoice ? Math.min(Number(l.qtyToInvoice) || 0, this.maxToInvoice(l)) : 0,
    })));
    if (!request) {
      return;
    }

    this.isBusy = true;
    request
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => {
        this.isPostOpen = false;
        this.toaster.success('Erp::OrderPosted', undefined, {
          messageLocalizationParams: [[result.shipmentNo, result.invoiceNo].filter(Boolean).join(', ')],
        });
        this.table?.reload();
        this.chatter?.load();
        this.refreshCard(row.id as string);
      });
  }

  /** Lists the posted shipment (receipt) lines the selected invoice can take. */
  openSource(row: TRow, event?: Event): void {
    event?.stopPropagation();
    const request = row.id ? this.openSourceLines(row.id) : null;
    if (!request) {
      return;
    }

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(lines => {
      this.sourceRow = row;
      this.sourceLines = lines;
      this.sourceSelection = new Set<string>();
      this.isSourceOpen = true;
    });
  }

  toggleSource(id: string): void {
    if (this.sourceSelection.has(id)) {
      this.sourceSelection.delete(id);
    } else {
      this.sourceSelection.add(id);
    }
  }

  collectSelected(): void {
    const row = this.sourceRow;
    const request = row?.id ? this.collectSourceLines(row.id, [...this.sourceSelection]) : null;
    if (!request || this.isBusy) {
      return;
    }

    this.isBusy = true;
    request
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.isSourceOpen = false;
        this.toaster.success('Erp::LinesAdded');
        this.table?.reload();
        if (row) {
          this.select(row, true);
          this.refreshCard(row.id as string);
        }
      });
  }

  openCreate(): void {
    this.form = this.fb.group({
      // Blank: the server takes the next number of the series set up for invoices.
      no: ['', Validators.maxLength(20)],
      partyId: [null as string | null, Validators.required],
      postingDate: [new Date().toISOString().substring(0, 10), Validators.required],
      locationCode: [null as string | null],
      externalDocumentNo: ['', Validators.maxLength(35)],
      lines: this.fb.array([this.buildLine()]),
    });
    this.isModalOpen = true;
  }

  addLine(): void {
    this.lines.push(this.buildLine());
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  save(): void {
    if (this.form.invalid || this.isBusy) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const input: NewDocumentInput = {
      no: value.no?.trim() || null,
      partyId: value.partyId,
      postingDate: value.postingDate,
      locationCode: value.locationCode || null,
      externalDocumentNo: value.externalDocumentNo?.trim() || null,
      lines: value.lines,
    };

    this.isBusy = true;
    this.createDocument(input)
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(created => {
        this.isModalOpen = false;
        this.toaster.success('Erp::DocumentCreated', undefined, {
          messageLocalizationParams: [created.no ?? ''],
        });
        this.table?.reload();
        // A new document opens on its card, ready for its lines.
        this.openCard(created);
      });
  }

  private execute(action: DocumentAction, row: TRow): void {
    const id = row.id as string;
    this.busyId = id;

    this.run(action, id)
      .pipe(
        finalize(() => (this.busyId = null)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => {
        this.toaster.success(this.successKey(action, result), undefined, {
          messageLocalizationParams: [
            (result as ApprovalRequestResultDto)?.firstApproverUserName ?? '',
          ],
        });
        if (action === 'delete' && this.selected?.id === id) {
          this.select(null);
        }
        if (this.card?.id === id && (action === 'delete' || action === 'makeOrder')) {
          // Deleted, or made into an order: back to the list, which shows where it went.
          this.dropCard();
        } else {
          this.refreshCard(id);
        }
        this.table?.reload();
        this.chatter?.load();
      });
  }

  /** Reloads the card when it shows this document. */
  private refreshCard(id: string): void {
    if (this.card?.id === id) {
      this.reloadCard(id);
    }
  }

  private successKey(action: DocumentAction, result: unknown): string {
    switch (action) {
      case 'sendApprovalRequest':
        return (result as ApprovalRequestResultDto)?.autoApproved
          ? 'Erp::ApprovalAutoApproved'
          : 'Erp::ApprovalRequestSent';
      case 'cancelApprovalRequest':
        return 'Erp::ApprovalRequestCanceled';
      case 'release':
        return 'Erp::DocumentReleased';
      case 'reopen':
        return 'Erp::DocumentReopened';
      case 'runPosting':
        return 'Erp::DocumentPosted';
      case 'makeOrder':
        return 'Erp::OrderMade';
      default:
        return 'Erp::DeletedSuccessfully';
    }
  }

  private buildLine(): FormGroup {
    return this.fb.group({
      type: [DocumentLineType.Item, Validators.required],
      no: ['', [Validators.required, Validators.maxLength(20)]],
      description: ['', Validators.maxLength(250)],
      quantity: [1, [Validators.required, Validators.min(0.00001)]],
      unitAmount: [0, [Validators.required, Validators.min(0)]],
    });
  }
}

/** `2026-10-21T00:00:00` -> `2026-10-21`, as a date input takes it. */
function toDateInput(value: unknown): string | null {
  return typeof value === 'string' && value ? value.substring(0, 10) : null;
}

/** The record table the "No." of a document line points into, by line type. */
export function lineEntityOf(type: DocumentLineType | null | undefined): string | null {
  switch (type) {
    case DocumentLineType.Item:
      return 'item';
    case DocumentLineType.GLAccount:
      return 'glAccount';
    default:
      return null;
  }
}
