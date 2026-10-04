import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, Validators } from '@angular/forms';
import { VatCalculationType, VatEntrySelection, VatEntryType, VatReportingService, VatReturnDto } from '@proxy/finance';
import { finalize } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/** The first and last day of last month: the period a monthly return usually covers. */
function lastMonth(): { from: string; to: string } {
  const today = new Date();
  const from = new Date(Date.UTC(today.getFullYear(), today.getMonth() - 1, 1));
  const to = new Date(Date.UTC(today.getFullYear(), today.getMonth(), 0));
  return { from: from.toISOString().substring(0, 10), to: to.toISOString().substring(0, 10) };
}

/**
 * VAT Statement.with a fixed layout: the return's boxes
 * (output VAT, input VAT, the net due, sales and purchases before VAT) and the rates behind them,
 * for open, closed (settled) or all VAT entries of a period.
 */
@Component({
  selector: 'app-vat-return',
  templateUrl: './vat-return.component.html',
  standalone: false,
})
export class VatReturnComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(VatReportingService);
  private readonly companyService = inject(CompanyService);
  private readonly destroyRef = inject(DestroyRef);

  readonly vatEntryType = VatEntryType;
  readonly reverseCharge = VatCalculationType.ReverseChargeVat;
  readonly result = signal<VatReturnDto | null>(null);
  readonly busy = signal(false);

  readonly selectionOptions = [
    { value: VatEntrySelection.Open, label: 'Erp::Enum:VatEntrySelection.Open' },
    { value: VatEntrySelection.Closed, label: 'Erp::Enum:VatEntrySelection.Closed' },
    { value: VatEntrySelection.OpenAndClosed, label: 'Erp::Enum:VatEntrySelection.OpenAndClosed' },
  ];

  readonly form = this.fb.group({
    startingDate: this.fb.control<string>(lastMonth().from, Validators.required),
    endingDate: this.fb.control<string>(lastMonth().to, Validators.required),
    selection: this.fb.control(VatEntrySelection.OpenAndClosed, { nonNullable: true }),
  });

  ngOnInit(): void {
    this.calculate();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.calculate());
  }

  calculate(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.busy.set(true);
    this.service
      .calculateReturn({ startingDate: value.startingDate!, endingDate: value.endingDate!, selection: value.selection })
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => this.result.set(result));
  }

  print(): void {
    window.print();
  }
}
