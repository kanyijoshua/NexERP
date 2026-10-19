import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { BankEntryStatementStatus, BankRecWorksheetDto, BankReconciliationService } from '@proxy/finance';
import { Observable, finalize } from 'rxjs';

/**
 * The bank reconciliation worksheet: statement lines on the left, the bank ledger entries waiting
 * for a statement on the right. Lines are imported from the bank's file, matched to entries by
 * amount and date or by hand, unmatched charges are posted to the account a line names, and the
 * statement is posted once every line is matched and it balances.
 */
@Component({
  selector: 'app-bank-reconciliation-worksheet',
  templateUrl: './bank-reconciliation-worksheet.component.html',
  standalone: false,
})
export class BankReconciliationWorksheetComponent implements OnInit {
  private readonly service = inject(BankReconciliationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly matched = BankEntryStatementStatus.BankAccEntryApplied;
  readonly worksheet = signal<BankRecWorksheetDto | null>(null);
  readonly busy = signal(false);
  readonly selectedLineId = signal<string | null>(null);
  readonly selectedEntryIds = signal<Set<string>>(new Set());
  readonly dateTolerance = signal(3);

  /** The entries ticked for matching, summed, to compare with the selected line. */
  readonly selectedTotal = computed(() => {
    const ids = this.selectedEntryIds();
    return (this.worksheet()?.entries ?? []).filter(e => ids.has(e.id!)).reduce((sum, e) => sum + e.amount, 0);
  });

  private id = '';

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id') ?? '';
    this.load();
  }

  load(): void {
    this.busy.set(true);
    this.service
      .getWorksheet(this.id)
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(worksheet => {
        this.worksheet.set(worksheet);
        this.selectedEntryIds.set(new Set());
      });
  }

  selectLine(lineId: string): void {
    this.selectedLineId.set(this.selectedLineId() === lineId ? null : lineId);
  }

  toggleEntry(entryId: string): void {
    const next = new Set(this.selectedEntryIds());
    if (next.has(entryId)) {
      next.delete(entryId);
    } else {
      next.add(entryId);
    }
    this.selectedEntryIds.set(next);
  }

  /** Reads the bank's statement file (date, description, amount, document no.) and adds its lines. */
  importFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    file.text().then(content => {
      input.value = '';
      this.run(this.service.importStatement(this.id, { content }));
    });
  }

  matchAutomatically(): void {
    this.run(this.service.matchAutomatically(this.id, { dateTolerance: this.dateTolerance() }));
  }

  matchManually(): void {
    const lineId = this.selectedLineId();
    if (!lineId || this.selectedEntryIds().size === 0) {
      return;
    }
    this.run(this.service.matchManually(this.id, { lineId, entryIds: [...this.selectedEntryIds()] }));
  }

  unmatch(lineId: string): void {
    this.run(this.service.unmatch(this.id, { lineId }));
  }

  transferDifferences(): void {
    this.run(this.service.transferDifferences(this.id));
  }

  post(): void {
    this.confirmation.warn('Erp::PostBankStatementConfirmation', 'Erp::PostStatement').subscribe(status => {
      if (status !== Confirmation.Status.confirm) {
        return;
      }

      this.busy.set(true);
      this.service
        .runPosting(this.id)
        .pipe(finalize(() => this.busy.set(false)))
        .subscribe(() => {
          this.toaster.success('Erp::StatementPosted');
          void this.router.navigate(['/erp/bank-account-statements']);
        });
    });
  }

  private run(call: Observable<unknown>): void {
    this.busy.set(true);
    call.pipe(finalize(() => this.busy.set(false))).subscribe(() => {
      this.toaster.success('Erp::SavedSuccessfully');
      this.load();
    });
  }
}
