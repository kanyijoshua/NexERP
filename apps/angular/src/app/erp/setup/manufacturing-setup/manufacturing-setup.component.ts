import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder } from '@angular/forms';
import { ManufacturingSetupDto, ManufacturingSetupService } from '@proxy/manufacturing';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/** Manufacturing setup: the production order series and the WIP, capacity and variance accounts. */
@Component({
  selector: 'app-manufacturing-setup',
  templateUrl: './manufacturing-setup.component.html',
  standalone: false,
})
export class ManufacturingSetupComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly setupService = inject(ManufacturingSetupService);
  private readonly noSeriesService = inject(NoSeriesService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly series = signal<NoSeriesDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly form = this.fb.group({
    productionOrderNos: this.fb.control<string | null>(null),
    wipAccountNo: this.fb.control<string | null>(null),
    capacityAppliedAccountNo: this.fb.control<string | null>(null),
    varianceAccountNo: this.fb.control<string | null>(null),
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
        productionOrderNos: value.productionOrderNos || undefined,
        wipAccountNo: value.wipAccountNo || undefined,
        capacityAppliedAccountNo: value.capacityAppliedAccountNo || undefined,
        varianceAccountNo: value.varianceAccountNo || undefined,
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

  private show(setup: ManufacturingSetupDto): void {
    this.form.reset({
      productionOrderNos: setup.productionOrderNos ?? null,
      wipAccountNo: setup.wipAccountNo ?? null,
      capacityAppliedAccountNo: setup.capacityAppliedAccountNo ?? null,
      varianceAccountNo: setup.varianceAccountNo ?? null,
    });
  }
}
