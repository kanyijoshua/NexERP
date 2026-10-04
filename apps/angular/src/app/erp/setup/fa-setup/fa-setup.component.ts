import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder } from '@angular/forms';
import { FASetupDto, FaSetupService } from '@proxy/fixed-assets';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/** FA Setup. */
@Component({
  selector: 'app-fa-setup',
  templateUrl: './fa-setup.component.html',
  standalone: false,
})
export class FaSetupComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly setupService = inject(FaSetupService);
  private readonly noSeriesService = inject(NoSeriesService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly series = signal<NoSeriesDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly form = this.fb.group({
    fixedAssetNos: this.fb.control<string | null>(null),
    defaultDeprBook: this.fb.control<string | null>(null),
    allowPostingToMainAssets: this.fb.nonNullable.control(false),
    allowFAPostingFrom: this.fb.control<string | null>(null),
    allowFAPostingTo: this.fb.control<string | null>(null),
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
        fixedAssetNos: value.fixedAssetNos || undefined,
        defaultDeprBook: value.defaultDeprBook || undefined,
        allowPostingToMainAssets: value.allowPostingToMainAssets,
        allowFAPostingFrom: value.allowFAPostingFrom || undefined,
        allowFAPostingTo: value.allowFAPostingTo || undefined,
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

  private show(setup: FASetupDto): void {
    this.form.reset({
      fixedAssetNos: setup.fixedAssetNos ?? null,
      defaultDeprBook: setup.defaultDeprBook ?? null,
      allowPostingToMainAssets: setup.allowPostingToMainAssets,
      allowFAPostingFrom: setup.allowFAPostingFrom?.substring(0, 10) ?? null,
      allowFAPostingTo: setup.allowFAPostingTo?.substring(0, 10) ?? null,
    });
  }
}
