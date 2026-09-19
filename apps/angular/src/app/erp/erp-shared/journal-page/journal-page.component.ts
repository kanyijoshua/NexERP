import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import {
  Component,
  DestroyRef,
  EventEmitter,
  HostBinding,
  Input,
  OnChanges,
  OnDestroy,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormGroup } from '@angular/forms';
import { Subscription } from 'rxjs';
import { DocumentLineChange, DocumentLineColumn, JournalBatch } from '../models';
import { JournalBalance, computeJournalBalance } from './journal-balance';

/**
 * Shell of the journals (general, cash receipt, payment, item, fixed asset, job):
 * batch selector + editable lines + running balance + Save / Post.
 */
@Component({
  selector: 'erp-journal-page',
  templateUrl: './journal-page.component.html',
})
export class JournalPageComponent implements OnChanges, OnDestroy {
  @HostBinding('attr.title') readonly hostTitle = null;

  @Input() title = '';
  @Input() batches: JournalBatch[] = [];
  @Input() selectedBatchId: string | null = null;
  @Output() selectedBatchIdChange = new EventEmitter<string | null>();

  @Input() lines: FormArray = new FormArray<FormGroup>([]);
  @Input() lineColumns: DocumentLineColumn[] = [];
  @Input() balanceField = 'amount';
  @Input() documentNoField = 'documentNo';
  /** Set to false for journals without a balance concept (item journal). */
  @Input() requireBalance = true;
  @Input() savePermission?: string;
  @Input() postPermission?: string;
  @Input() busy = false;
  @Input() readonly = false;
  @Input() postConfirmationKey = 'Erp::PostJournalConfirmation';
  @Input() currencyCode?: string;

  @Output() addLine = new EventEmitter<void>();
  @Output() removeLine = new EventEmitter<number>();
  @Output() lineChange = new EventEmitter<DocumentLineChange>();
  @Output() save = new EventEmitter<void>();
  /** Emits after the user confirmed the posting. */
  @Output() post = new EventEmitter<void>();
  @Output() batchChange = new EventEmitter<string | null>();

  balance: JournalBalance = { total: 0, byDocument: [], isBalanced: true };

  private linesSubscription?: Subscription;
  private readonly confirmation = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  get canPost(): boolean {
    return (
      !this.busy &&
      !this.readonly &&
      this.lines.length > 0 &&
      this.lines.valid &&
      (!this.requireBalance || this.balance.isBalanced)
    );
  }

  get canSave(): boolean {
    return !this.busy && !this.readonly;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['lines']) {
      this.linesSubscription?.unsubscribe();
      this.linesSubscription = this.lines?.valueChanges.subscribe(() => this.recalculate());
    }
    if (changes['lines'] || changes['balanceField'] || changes['documentNoField']) {
      this.recalculate();
    }
  }

  ngOnDestroy(): void {
    this.linesSubscription?.unsubscribe();
  }

  /** Public so that a parent can force a recalculation after patching disabled controls. */
  recalculate(): void {
    this.balance = computeJournalBalance(
      this.lines?.getRawValue() ?? [],
      this.balanceField,
      this.documentNoField,
    );
  }

  onBatchChange(batchId: string | null): void {
    this.selectedBatchId = batchId;
    this.selectedBatchIdChange.emit(batchId);
    this.batchChange.emit(batchId);
  }

  requestPost(): void {
    if (!this.canPost) {
      return;
    }
    this.confirmation
      .warn(this.postConfirmationKey, 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.post.emit();
        }
      });
  }
}
