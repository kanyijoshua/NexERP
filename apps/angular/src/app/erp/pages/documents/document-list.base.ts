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
import { Observable, finalize } from 'rxjs';
import { ChatterWidgetComponent } from '../../components/chatter-widget/chatter-widget.component';
import {
  DocumentLineChange,
  DocumentLineColumn,
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
  'release' | 'reopen' | 'sendApprovalRequest' | 'cancelApprovalRequest' | 'runPosting' | 'delete';

/**
 * Sales and purchase invoice lists behave identically: list, create, release, reopen,
 * ask for approval, post. Subclasses supply the API and the wording; they share one template.
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

  readonly partySource = (term: string) => this.searchParties(term);
  readonly nameOf = (row: TRow) => this.partyName(row);

  ngOnInit(): void {
    this.partyFilter = this.route.snapshot.queryParamMap.get('partyId');
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
    const line = this.lines.at(change.index) as FormGroup | undefined;
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

  select(row: TRow | null): void {
    this.selected = row;
    this.selection = row ? [row] : [];
    this.history = [];

    if (row?.id && this.canSeeApprovals) {
      this.approvalEntries
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

    if (action === 'runPosting' || action === 'delete') {
      this.confirmation
        .warn(
          action === 'delete' ? 'Erp::ItemWillBeDeletedMessage' : 'Erp::PostDocumentConfirmation',
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
        this.select(created);
        this.table?.reload();
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
        this.table?.reload();
        this.chatter?.load();
      });
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
