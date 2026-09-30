import { LocalizationService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, Validators } from '@angular/forms';
import {
  ExchRateAdjmtAccountType,
  ExchRateAdjmtRegisterDto,
  ExchRateAdjustmentDto,
  ExchRateAdjustmentInput,
  ExchRateAdjustmentService,
} from '@proxy/finance';
import { finalize } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/** The last day of the month before today: the usual date a month-end revaluation runs to. */
function lastMonthEnd(): string {
  const today = new Date();
  return new Date(Date.UTC(today.getFullYear(), today.getMonth(), 0)).toISOString().substring(0, 10);
}

/**
 * Adjust Exchange Rates. Mirrors Business Central report 596 with page 106 underneath: revalue
 * what is open in foreign currencies at the rate on the ending date, preview the gains and losses,
 * post them, and see what earlier runs adjusted.
 */
@Component({
  selector: 'app-exch-rate-adjustment',
  templateUrl: './exch-rate-adjustment.component.html',
  standalone: false,
})
export class ExchRateAdjustmentComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ExchRateAdjustmentService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly localization = inject(LocalizationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly accountType = ExchRateAdjmtAccountType;
  readonly result = signal<ExchRateAdjustmentDto | null>(null);
  readonly registers = signal<ExchRateAdjmtRegisterDto[]>([]);
  readonly busy = signal(false);

  readonly form = this.fb.group({
    endingDate: this.fb.control<string>(lastMonthEnd(), Validators.required),
    postingDate: this.fb.control<string>(lastMonthEnd(), Validators.required),
    documentNo: this.fb.control<string>('', [Validators.required, Validators.maxLength(20)]),
    currencyCode: this.fb.control<string | null>(null),
    adjustCustomers: this.fb.control(true, { nonNullable: true }),
    adjustVendors: this.fb.control(true, { nonNullable: true }),
    adjustBankAccounts: this.fb.control(true, { nonNullable: true }),
  });

  ngOnInit(): void {
    this.loadRegisters();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.result.set(null);
      this.loadRegisters();
    });
  }

  preview(): void {
    this.run(false);
  }

  post(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.run(true);
  }

  accountTypeKey(type: ExchRateAdjmtAccountType): string {
    return 'Erp::Enum:ExchRateAdjmtAccountType.' + ExchRateAdjmtAccountType[type];
  }

  private run(post: boolean): void {
    const value = this.form.getRawValue();
    const input: ExchRateAdjustmentInput = {
      endingDate: value.endingDate,
      postingDate: value.postingDate || value.endingDate,
      documentNo: value.documentNo?.trim() || undefined,
      currencyCode: value.currencyCode || undefined,
      adjustCustomers: value.adjustCustomers,
      adjustVendors: value.adjustVendors,
      adjustBankAccounts: value.adjustBankAccounts,
    };

    this.busy.set(true);
    (post ? this.service.adjust(input) : this.service.calculate(input))
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => {
        this.result.set(result);
        if (result.posted) {
          this.toaster.success(this.localization.instant('Erp::PostedSuccessfully', String(result.registerNo)));
          this.loadRegisters();
        }
      });
  }

  private loadRegisters(): void {
    this.service
      .getRegisters({ maxResultCount: 50, skipCount: 0 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => this.registers.set(result.items ?? []));
  }
}
