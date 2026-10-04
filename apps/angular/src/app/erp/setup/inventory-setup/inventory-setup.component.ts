import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder } from '@angular/forms';
import { InventorySetupDto, InventorySetupService } from '@proxy/inventory';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/** Inventory Setup. item numbering and the rules stock moves by. */
@Component({
  selector: 'app-inventory-setup',
  templateUrl: './inventory-setup.component.html',
  standalone: false,
})
export class InventorySetupComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly setupService = inject(InventorySetupService);
  private readonly noSeriesService = inject(NoSeriesService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly series = signal<NoSeriesDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly form = this.fb.group({
    itemNos: this.fb.control<string | null>(null),
    locationMandatory: this.fb.control(false, { nonNullable: true }),
    preventNegativeInventory: this.fb.control(false, { nonNullable: true }),
    automaticCostPosting: this.fb.control(true, { nonNullable: true }),
  });

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.load());
  }

  save(): void {
    const value = this.form.getRawValue();
    this.saving.set(true);
    this.setupService
      .update({ ...value, itemNos: value.itemNos || undefined } as InventorySetupDto)
      .pipe(
        finalize(() => this.saving.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(setup => {
        this.form.reset(setup);
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
        this.form.reset({ ...setup, itemNos: setup.itemNos ?? null });
      });
  }
}
