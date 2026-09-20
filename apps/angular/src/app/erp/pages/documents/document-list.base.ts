import { ABP, ListService, PagedResultDto, PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { DestroyRef, Directive, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DocumentLineType, DocumentStatus, documentLineTypeOptions } from '@proxy/documents';
import {
  ApprovalEntryDto,
  ApprovalEntryService,
  ApprovalRequestResultDto,
  ApprovalStatus,
} from '@proxy/workflows';
import { Observable, finalize } from 'rxjs';
import { ChatterWidgetComponent } from '../../components/chatter-widget/chatter-widget.component';
import { DocumentLineColumn, LookupItem, calculateDocumentTotals } from '../../erp-shared';
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
  private readonly permissions = inject(PermissionService);

  readonly list = inject<ListService<ABP.PageQueryParams>>(ListService);
  readonly DocumentStatus = DocumentStatus;
  readonly ApprovalStatus = ApprovalStatus;

  /** Wording and permissions, e.g. 'Erp::SalesInvoices' and 'Erp.SalesDocuments'. */
  abstract readonly titleKey: string;
  abstract readonly icon: string;
  abstract readonly partyLabelKey: string;
  abstract readonly unitAmountLabelKey: string;
  abstract readonly permissionPrefix: string;
  /** "SalesHeader" / "PurchaseHeader": the key of the record's chatter thread. */
  abstract readonly chatterEntityType: string;

  @ViewChild(ChatterWidgetComponent) private chatter?: ChatterWidgetComponent;

  data: PagedResultDto<TRow> = { items: [], totalCount: 0 };
  selected: TRow | null = null;
  history: ApprovalEntryDto[] = [];
  busyId: string | null = null;

  isModalOpen = false;
  isBusy = false;
  form!: FormGroup;
  lineColumns: DocumentLineColumn[] = [];

  readonly canSeeApprovals = this.permissions.getGrantedPolicy('Erp.Workflows');

  private _filter = '';

  get filter(): string {
    return this._filter;
  }

  set filter(value: string) {
    this._filter = value ?? '';
    this.list.filter = this._filter;
  }

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

  protected abstract getList(query: ABP.PageQueryParams): Observable<PagedResultDto<TRow>>;
  protected abstract partyName(row: TRow): string;
  protected abstract searchParties(term: string): Observable<LookupItem[]>;
  protected abstract createDocument(input: NewDocumentInput): Observable<TRow>;
  protected abstract run(action: DocumentAction, id: string): Observable<unknown>;

  readonly partySource = (term: string) => this.searchParties(term);
  readonly nameOf = (row: TRow) => this.partyName(row);

  ngOnInit(): void {
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
      { field: 'no', labelKey: 'Erp::No', type: 'text', width: '130px' },
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

    this.list
      .hookToQuery(query => this.getList(query))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.data = result;
        // Keep the selection on the same document, with its fresh status.
        const current = this.selected?.id;
        this.select(result.items?.find(r => r.id === current) ?? result.items?.[0] ?? null);
      });

    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.selected = null;
      this.list.page = 0;
      this.list.get();
    });
  }

  select(row: TRow | null): void {
    this.selected = row;
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
        this.selected = created;
        this.list.get();
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
          this.selected = null;
        }
        this.list.get();
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
