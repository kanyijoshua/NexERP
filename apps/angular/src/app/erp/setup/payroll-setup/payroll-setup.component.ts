import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder } from '@angular/forms';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import { PayrollSetupDto, PayrollSetupService } from '@proxy/payroll';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/** Payroll Setup: the series payroll runs are numbered from and the tax relief every employee gets. */
@Component({
  selector: 'app-payroll-setup',
  templateUrl: './payroll-setup.component.html',
  standalone: false,
})
export class PayrollSetupComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly setupService = inject(PayrollSetupService);
  private readonly noSeriesService = inject(NoSeriesService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly series = signal<NoSeriesDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly form = this.fb.group({
    payrollRunNos: this.fb.control<string | null>(null),
    personalRelief: this.fb.nonNullable.control(0),
  });

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.load());
  }

  save(): void {
    const value = this.form.getRawValue();
    const input: PayrollSetupDto = { payrollRunNos: value.payrollRunNos || undefined, personalRelief: Number(value.personalRelief) || 0 };

    this.saving.set(true);
    this.setupService
      .update(input)
      .pipe(
        finalize(() => this.saving.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(setup => {
        this.show(setup);
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
        this.show(setup);
      });
  }

  private show(setup: PayrollSetupDto): void {
    this.form.reset({ payrollRunNos: setup.payrollRunNos ?? null, personalRelief: setup.personalRelief ?? 0 });
  }
}
