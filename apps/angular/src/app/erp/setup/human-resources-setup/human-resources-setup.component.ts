import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder } from '@angular/forms';
import { HumanResourceUnitOfMeasureDto, HumanResourceUnitOfMeasureService, HumanResourcesSetupService } from '@proxy/human-resources';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/** Human Resources Setup. */
@Component({
  selector: 'app-human-resources-setup',
  templateUrl: './human-resources-setup.component.html',
  standalone: false,
})
export class HumanResourcesSetupComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly setupService = inject(HumanResourcesSetupService);
  private readonly noSeriesService = inject(NoSeriesService);
  private readonly unitService = inject(HumanResourceUnitOfMeasureService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly series = signal<NoSeriesDto[]>([]);
  readonly units = signal<HumanResourceUnitOfMeasureDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly form = this.fb.group({
    employeeNos: this.fb.control<string | null>(null),
    baseUnitOfMeasure: this.fb.control<string | null>(null),
  });

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.load());
  }

  save(): void {
    const value = this.form.getRawValue();
    this.saving.set(true);
    this.setupService
      .update({ employeeNos: value.employeeNos || undefined, baseUnitOfMeasure: value.baseUnitOfMeasure || undefined })
      .pipe(
        finalize(() => this.saving.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(setup => {
        this.form.reset({ employeeNos: setup.employeeNos ?? null, baseUnitOfMeasure: setup.baseUnitOfMeasure ?? null });
        this.toaster.success('Erp::SavedSuccessfully');
      });
  }

  private load(): void {
    this.loading.set(true);
    forkJoin({
      setup: this.setupService.get(),
      series: this.noSeriesService.getList({ maxResultCount: 1000, skipCount: 0 } as never),
      units: this.unitService.getList({ maxResultCount: 1000, skipCount: 0 }),
    })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(({ setup, series, units }) => {
        this.series.set(series.items ?? []);
        this.units.set(units.items ?? []);
        this.form.reset({ employeeNos: setup.employeeNos ?? null, baseUnitOfMeasure: setup.baseUnitOfMeasure ?? null });
      });
  }
}
