import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ApprovalDocumentKind,
  ApprovalEntryDto,
  ApprovalEntryService,
  ApprovalStatus,
  GetApprovalEntriesInput,
} from '@proxy/workflows';
import { finalize } from 'rxjs';
import { CompanyService } from '../services/company.service';

type ApprovalView = 'toApprove' | 'sentByMe' | 'all';
type PendingAction = 'approve' | 'reject';

/**
 * The approver's work list. Mirrors Business Central page 654 "Requests to Approve",
 * with "Requests Sent for Approval" and the full log as further views.
 */
@Component({
  selector: 'app-requests-to-approve',
  templateUrl: './requests-to-approve.component.html',
  providers: [ListService],
})
export class RequestsToApproveComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  readonly ApprovalStatus = ApprovalStatus;
  readonly list = inject<ListService<ABP.PageQueryParams>>(ListService);

  data: PagedResultDto<ApprovalEntryDto> = { items: [], totalCount: 0 };
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
    this.list
      .hookToQuery(query => this.service.getList(this.toInput(query)))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.data = result));

    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.list.get());
  }

  setView(view: ApprovalView): void {
    this.view = view;
    this.list.page = 0;
    this.list.get();
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

  statusName(status?: ApprovalStatus): string {
    return ApprovalStatus[status ?? ApprovalStatus.Created];
  }

  kindName(entry: ApprovalEntryDto): string {
    return ApprovalDocumentKind[entry.documentKind ?? ApprovalDocumentKind.SalesDocument];
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
        this.list.get();
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
              this.list.get();
            });
        }
      });
  }

  private toInput(query: ABP.PageQueryParams): GetApprovalEntriesInput {
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
