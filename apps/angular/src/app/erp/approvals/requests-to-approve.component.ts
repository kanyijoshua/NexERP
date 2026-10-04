import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ApprovalDocumentKind,
  ApprovalEntryDto,
  ApprovalEntryService,
  ApprovalStatus,
  GetApprovalEntriesInput,
  approvalDocumentKindOptions,
  approvalStatusOptions,
} from '@proxy/workflows';
import { finalize } from 'rxjs';
import { ErpTableColumn, ErpTableComponent, ErpTableQuery, ErpTableSource } from '../erp-shared';
import { CompanyService } from '../services/company.service';

type ApprovalView = 'toApprove' | 'sentByMe' | 'all';
type PendingAction = 'approve' | 'reject';

/**
 * The approver's work list.,
 * with "Requests Sent for Approval" and the full log as further views.
 */
@Component({
  selector: 'app-requests-to-approve',
  templateUrl: './requests-to-approve.component.html',
  standalone: false,
})
export class RequestsToApproveComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild(ErpTableComponent) table?: ErpTableComponent<ApprovalEntryDto>;

  readonly ApprovalStatus = ApprovalStatus;

  readonly columns: ErpTableColumn<ApprovalEntryDto>[] = [
    { field: 'documentNo', labelKey: 'Erp::Document', width: 150 },
    {
      field: 'documentKind',
      labelKey: 'Erp::Type',
      type: 'select',
      width: 140,
      options: approvalDocumentKindOptions.map(o => ({ value: o.value, label: 'Erp::Enum:ApprovalDocumentKind.' + o.key })),
    },
    { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 130 },
    { field: 'senderUserName', labelKey: 'Erp::Sender', width: 130 },
    { field: 'approverUserName', labelKey: 'Erp::Approver', width: 140 },
    {
      field: 'status',
      labelKey: 'Erp::Status',
      type: 'badge',
      width: 120,
      options: approvalStatusOptions.map(o => ({ value: o.value, label: 'Erp::Enum:ApprovalStatus.' + o.key })),
      badgeClass: row => this.statusClass(row.status),
    },
    { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date', width: 120 },
    { field: 'comment', labelKey: 'Erp::Comment', width: 180, sortable: false },
  ];

  readonly source: ErpTableSource<ApprovalEntryDto> = query => this.service.getList(this.toInput(query));

  view: ApprovalView = 'toApprove';

  // Approve and reject share one dialog: both may carry a comment, and a rejection should.
  isModalOpen = false;
  isBusy = false;
  action: PendingAction = 'approve';
  entry: ApprovalEntryDto | null = null;
  comment = '';

  constructor(
    private readonly service: ApprovalEntryService,
    private readonly companyService: CompanyService,
    private readonly toaster: ToasterService,
    private readonly confirmation: ConfirmationService,
  ) {}

  ngOnInit(): void {
    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.table?.reload());
  }

  setView(view: ApprovalView): void {
    this.view = view;
    this.table?.reload();
  }

  statusClass(status?: ApprovalStatus): string {
    switch (status) {
      case ApprovalStatus.Open:
        return 'bg-warning text-dark';
      case ApprovalStatus.Approved:
        return 'bg-success';
      case ApprovalStatus.Rejected:
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }

  documentRoute(entry: ApprovalEntryDto): string[] {
    return entry.documentKind === ApprovalDocumentKind.PurchaseDocument
      ? ['/erp/purchase-invoices']
      : ['/erp/sales-invoices'];
  }

  isOverdue(entry: ApprovalEntryDto): boolean {
    return (
      entry.status === ApprovalStatus.Open &&
      !!entry.dueDate &&
      new Date(entry.dueDate) < new Date()
    );
  }

  open(action: PendingAction, entry: ApprovalEntryDto): void {
    this.action = action;
    this.entry = entry;
    this.comment = '';
    this.isModalOpen = true;
  }

  confirmAction(): void {
    if (!this.entry?.id || this.isBusy) {
      return;
    }

    const input = { comment: this.comment.trim() || undefined };
    const request$ =
      this.action === 'approve'
        ? this.service.approve(this.entry.id, input)
        : this.service.reject(this.entry.id, input);

    this.isBusy = true;
    request$
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.isModalOpen = false;
        this.toaster.success(
          this.action === 'approve' ? 'Erp::RequestApproved' : 'Erp::RequestRejected',
        );
        this.table?.reload();
      });
  }

  delegate(entry: ApprovalEntryDto): void {
    if (!entry.id) {
      return;
    }
    const id = entry.id;

    this.confirmation
      .info('Erp::DelegateConfirmation', 'Erp::Delegate')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.service
            .delegate(id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(() => {
              this.toaster.success('Erp::RequestDelegated');
              this.table?.reload();
            });
        }
      });
  }

  private toInput(query: ErpTableQuery): GetApprovalEntriesInput {
    return {
      ...query,
      onlyMine: this.view === 'toApprove',
      sentByMe: this.view === 'sentByMe',
      // The work list shows what is waiting; the other views show the whole history.
      status: this.view === 'toApprove' ? ApprovalStatus.Open : undefined,
      allStatuses: this.view !== 'toApprove',
      documentId: undefined,
    } as GetApprovalEntriesInput;
  }
}
