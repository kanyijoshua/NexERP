import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder } from '@angular/forms';
import { CashManagementSetupDto, CashManagementSetupService } from '@proxy/cash-management';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/** Cash Management Setup: the number series payment vouchers take their numbers from. */
@Component({
  selector: 'app-cash-management-setup',
  templateUrl: './cash-management-setup.component.html',
  standalone: false,
})
export class CashManagementSetupComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly setupService = inject(CashManagementSetupService);
  private readonly noSeriesService = inject(NoSeriesService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly series = signal<NoSeriesDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly form = this.fb.group({
    paymentVoucherNos: this.fb.control<string | null>(null),
  });

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.load());
  }

  save(): void {
    const input: CashManagementSetupDto = { paymentVoucherNos: this.form.getRawValue().paymentVoucherNos || undefined };

    this.saving.set(true);
    this.setupService
      .update(input)
      .pipe(
        finalize(() => this.saving.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(setup => {
        this.form.reset({ paymentVoucherNos: setup.paymentVoucherNos ?? null });
        this.toaster.success('Erp::SavedSuccessfully');
      });
  }

  private load(): void {
    this.loading.set(true);
    forkJoin({
      setup: this.setupService.get(),
      series: this.noSeriesService.getList({ maxResultCount: 1000, skipCount: 0 } as never),
    })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(({ setup, series }) => {
        this.series.set(series.items ?? []);
        this.form.reset({ paymentVoucherNos: setup.paymentVoucherNos ?? null });
      });
  }
}
