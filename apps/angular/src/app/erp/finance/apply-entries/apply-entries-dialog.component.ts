import { ToasterService } from '@abp/ng.theme.shared';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { DetailedLedgerEntryDto, EntryApplicationService, PartyLedgerEntryDto, PartyLedgerType } from '@proxy/finance';
import { DetailedCVLedgerEntryType } from '@proxy/sales';
import { finalize } from 'rxjs';

/** One open entry the applying entry can be applied to, with the amount the user wants to apply. */
interface ApplicableRow {
  entry: PartyLedgerEntryDto;
  selected: boolean;
  amountToApply: number;
}

/**
 * Apply Entries: the open entries of the same customer or vendor that point the other way. Tick
 * them, change the amount to apply where only part of an entry is settled, and post the
 * application. A payment within an invoice's discount date takes the discount. The detailed
 * entries of the applying entry show what has been applied, and the last application can be undone.
 */
@Component({
  selector: 'app-apply-entries-dialog',
  templateUrl: './apply-entries-dialog.component.html',
  standalone: false,
})
export class ApplyEntriesDialogComponent implements OnInit {
  readonly modal = inject(NgbActiveModal);
  private readonly service = inject(EntryApplicationService);
  private readonly toaster = inject(ToasterService);

  /** Set by the opener. */
  partyType = PartyLedgerType.Customer;
  applying!: PartyLedgerEntryDto;

  readonly detailType = DetailedCVLedgerEntryType;
  readonly rows = signal<ApplicableRow[]>([]);
  readonly details = signal<DetailedLedgerEntryDto[]>([]);
  readonly busy = signal(false);
  readonly tab = signal<'apply' | 'details'>('apply');
  postingDate = new Date().toISOString().substring(0, 10);

  readonly appliedTotal = computed(() => this.rows().filter(r => r.selected).reduce((sum, r) => sum + r.amountToApply, 0));
  readonly hasApplication = computed(() => this.details().some(d => d.entryType === DetailedCVLedgerEntryType.Application && !d.unapplied));

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.busy.set(true);
    this.service
      .getApplicableEntries({ partyType: this.partyType, applyingEntryId: this.applying.id })
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe(entries => this.rows.set(entries.map(entry => ({ entry, selected: false, amountToApply: entry.remainingAmount }))));
    this.service
      .getDetailedEntries({ partyType: this.partyType, ledgerEntryNo: this.applying.entryNo })
      .subscribe(details => this.details.set(details));
  }

  toggle(row: ApplicableRow): void {
    row.selected = !row.selected;
    this.rows.set([...this.rows()]);
  }

  setAmount(row: ApplicableRow, value: string | number): void {
    row.amountToApply = Number(value) || 0;
    this.rows.set([...this.rows()]);
  }

  apply(): void {
    const entries = this.rows()
      .filter(r => r.selected)
      .map(r => ({ entryId: r.entry.id, amountToApply: r.amountToApply === r.entry.remainingAmount ? undefined : Math.abs(r.amountToApply) }));
    if (!entries.length) {
      return;
    }

    this.busy.set(true);
    this.service
      .apply({ partyType: this.partyType, applyingEntryId: this.applying.id, postingDate: this.postingDate, entries })
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe(result => {
        this.toaster.success('Erp::EntriesApplied');
        this.modal.close(result);
      });
  }

  unapply(): void {
    this.busy.set(true);
    this.service
      .unapply({ partyType: this.partyType, entryId: this.applying.id, postingDate: this.postingDate })
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe(() => {
        this.toaster.success('Erp::EntriesUnapplied');
        this.modal.close(true);
      });
  }
}
