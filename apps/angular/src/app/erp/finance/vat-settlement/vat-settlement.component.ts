import { LocalizationService } from '@abp/ng.core';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, Validators } from '@angular/forms';
import { VatEntryType, VatReportingService, VatSettlementDto, VatSettlementInput } from '@proxy/finance';
import { filter, finalize } from 'rxjs';

function lastMonth(): { from: string; to: string } {
  const today = new Date();
  const from = new Date(Date.UTC(today.getFullYear(), today.getMonth() - 1, 1));
  const to = new Date(Date.UTC(today.getFullYear(), today.getMonth(), 0));
  return { from: from.toISOString().substring(0, 10), to: to.toISOString().substring(0, 10) };
}

/**
 * Calc. and Post VAT Settlement. show what the open VAT
 * entries of a period add up to per VAT posting setup, then close them and move the net to the
 * settlement account, which is what is owed to (or reclaimable from) the tax authority.
 */
@Component({
  selector: 'app-vat-settlement',
  templateUrl: './vat-settlement.component.html',
  standalone: false,
})
export class VatSettlementComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(VatReportingService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly localization = inject(LocalizationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly vatEntryType = VatEntryType;
  readonly result = signal<VatSettlementDto | null>(null);
  readonly busy = signal(false);

  readonly form = this.fb.group({
    startingDate: this.fb.control<string | null>(lastMonth().from),
    endingDate: this.fb.control<string>(lastMonth().to, Validators.required),
    postingDate: this.fb.control<string>(lastMonth().to, Validators.required),
    documentNo: this.fb.control<string>('', [Validators.required, Validators.maxLength(20)]),
    settlementAccountNo: this.fb.control<string | null>(null, Validators.required),
  });

  preview(): void {
    if (this.form.controls.endingDate.invalid) {
      this.form.controls.endingDate.markAsTouched();
      return;
    }

    this.busy.set(true);
    this.service
      .calculateSettlement(this.input())
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => this.result.set(result));
  }

  /** Settling closes the period's VAT entries for good, so it asks first. */
  post(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.confirmation
      .warn('Erp::VatSettlementConfirm', 'Erp::CalcAndPostVatSettlement')
      .pipe(
        filter(status => status === Confirmation.Status.confirm),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.busy.set(true);
        this.service
          .settle(this.input())
          .pipe(
            finalize(() => this.busy.set(false)),
            takeUntilDestroyed(this.destroyRef),
          )
          .subscribe(result => {
            this.result.set(result);
            this.toaster.success(this.localization.instant('Erp::PostedSuccessfully', String(result.registerNo)));
          });
      });
  }

  private input(): VatSettlementInput {
    const value = this.form.getRawValue();
    return {
      startingDate: value.startingDate || undefined,
      endingDate: value.endingDate!,
      postingDate: value.postingDate || value.endingDate!,
      documentNo: value.documentNo?.trim() || undefined,
      settlementAccountNo: value.settlementAccountNo || undefined,
    };
  }
}
