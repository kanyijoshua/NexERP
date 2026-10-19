import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder } from '@angular/forms';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import { ServiceMgtSetupDto, ServiceMgtSetupService } from '@proxy/service-management';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/** Service setup: the number series, the revenue accounts and the default warranty. */
@Component({
  selector: 'app-service-setup',
  templateUrl: './service-setup.component.html',
  standalone: false,
})
export class ServiceSetupComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly setupService = inject(ServiceMgtSetupService);
  private readonly noSeriesService = inject(NoSeriesService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly series = signal<NoSeriesDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly form = this.fb.group({
    serviceOrderNos: this.fb.control<string | null>(null),
    serviceItemNos: this.fb.control<string | null>(null),
    serviceContractNos: this.fb.control<string | null>(null),
    serviceRevenueAccountNo: this.fb.control<string | null>(null),
    contractRevenueAccountNo: this.fb.control<string | null>(null),
    defaultWarrantyMonths: this.fb.nonNullable.control(12),
  });

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.load());
  }

  save(): void {
    const value = this.form.getRawValue();
    this.saving.set(true);
    this.setupService
      .update({
        serviceOrderNos: value.serviceOrderNos || undefined,
        serviceItemNos: value.serviceItemNos || undefined,
        serviceContractNos: value.serviceContractNos || undefined,
        serviceRevenueAccountNo: value.serviceRevenueAccountNo || undefined,
        contractRevenueAccountNo: value.contractRevenueAccountNo || undefined,
        defaultWarrantyMonths: value.defaultWarrantyMonths ?? 0,
      })
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

  private show(setup: ServiceMgtSetupDto): void {
    this.form.reset({
      serviceOrderNos: setup.serviceOrderNos ?? null,
      serviceItemNos: setup.serviceItemNos ?? null,
      serviceContractNos: setup.serviceContractNos ?? null,
      serviceRevenueAccountNo: setup.serviceRevenueAccountNo ?? null,
      contractRevenueAccountNo: setup.contractRevenueAccountNo ?? null,
      defaultWarrantyMonths: setup.defaultWarrantyMonths ?? 0,
    });
  }
}
