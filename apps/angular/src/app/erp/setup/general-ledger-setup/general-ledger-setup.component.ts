import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, Validators } from '@angular/forms';
import { DimensionDto, DimensionService } from '@proxy/dimensions';
import { GeneralLedgerSetupDto, GeneralLedgerSetupService, RoundingType, roundingTypeOptions } from '@proxy/finance';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/**
 * General Ledger Setup. Mirrors Business Central page 118: the company's allowed posting dates,
 * local currency, rounding precisions and global dimensions, one record per company.
 */
@Component({
  selector: 'app-general-ledger-setup',
  templateUrl: './general-ledger-setup.component.html',
  standalone: false,
})
export class GeneralLedgerSetupComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly setupService = inject(GeneralLedgerSetupService);
  private readonly dimensionService = inject(DimensionService);
  private readonly noSeriesService = inject(NoSeriesService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly roundingTypeOptions = roundingTypeOptions;
  readonly dimensions = signal<DimensionDto[]>([]);
  readonly series = signal<NoSeriesDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly form = this.fb.group({
    allowPostingFrom: this.fb.control<string | null>(null),
    allowPostingTo: this.fb.control<string | null>(null),
    lcyCode: this.fb.control<string | null>(null, Validators.maxLength(10)),
    localCurrencySymbol: this.fb.control<string | null>(null, Validators.maxLength(10)),
    localCurrencyDescription: this.fb.control<string | null>(null, Validators.maxLength(60)),
    additionalReportingCurrency: this.fb.control<string | null>(null, Validators.maxLength(10)),
    amountRoundingPrecision: this.fb.control<number>(0.01, [Validators.required, Validators.min(0.00000001)]),
    unitAmountRoundingPrecision: this.fb.control<number>(0.00001, [Validators.required, Validators.min(0.00000001)]),
    invRoundingPrecisionLcy: this.fb.control<number>(0.01, [Validators.required, Validators.min(0.00000001)]),
    invRoundingType: this.fb.control<RoundingType>(RoundingType.Nearest),
    vatRoundingType: this.fb.control<RoundingType>(RoundingType.Nearest),
    pmtDiscExclVAT: this.fb.control<boolean>(false),
    unrealizedVAT: this.fb.control<boolean>(false),
    adjustForPaymentDisc: this.fb.control<boolean>(false),
    markCrMemosAsCorrections: this.fb.control<boolean>(false),
    maxVATDifferenceAllowed: this.fb.control<number>(0, [Validators.min(0)]),
    paymentTolerancePct: this.fb.control<number>(0, [Validators.min(0), Validators.max(100)]),
    maxPaymentToleranceAmount: this.fb.control<number>(0, [Validators.min(0)]),
    blockDeletionOfGLAccounts: this.fb.control<boolean>(false),
    postWithJobQueue: this.fb.control<boolean>(false),
    jobQueueCategoryCode: this.fb.control<string | null>(null, Validators.maxLength(10)),
    notifyOnSuccess: this.fb.control<boolean>(false),
    registerTime: this.fb.control<boolean>(false),
    globalDimension1Code: this.fb.control<string | null>(null),
    globalDimension2Code: this.fb.control<string | null>(null),
    bankAccountNos: this.fb.control<string | null>(null),
  });

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const input: GeneralLedgerSetupDto = {
      allowPostingFrom: value.allowPostingFrom || undefined,
      allowPostingTo: value.allowPostingTo || undefined,
      lcyCode: value.lcyCode?.trim() || undefined,
      localCurrencySymbol: value.localCurrencySymbol?.trim() || undefined,
      localCurrencyDescription: value.localCurrencyDescription?.trim() || undefined,
      additionalReportingCurrency: value.additionalReportingCurrency?.trim() || undefined,
      amountRoundingPrecision: value.amountRoundingPrecision,
      unitAmountRoundingPrecision: value.unitAmountRoundingPrecision,
      invRoundingPrecisionLcy: value.invRoundingPrecisionLcy,
      invRoundingType: value.invRoundingType,
      vatRoundingType: value.vatRoundingType,
      pmtDiscExclVAT: value.pmtDiscExclVAT,
      unrealizedVAT: value.unrealizedVAT,
      adjustForPaymentDisc: value.adjustForPaymentDisc,
      markCrMemosAsCorrections: value.markCrMemosAsCorrections,
      maxVATDifferenceAllowed: value.maxVATDifferenceAllowed,
      paymentTolerancePct: value.paymentTolerancePct,
      maxPaymentToleranceAmount: value.maxPaymentToleranceAmount,
      blockDeletionOfGLAccounts: value.blockDeletionOfGLAccounts,
      postWithJobQueue: value.postWithJobQueue,
      jobQueueCategoryCode: value.jobQueueCategoryCode?.trim() || undefined,
      notifyOnSuccess: value.notifyOnSuccess,
      registerTime: value.registerTime,
      globalDimension1Code: value.globalDimension1Code || undefined,
      globalDimension2Code: value.globalDimension2Code || undefined,
      bankAccountNos: value.bankAccountNos || undefined,
    };

    this.saving.set(true);
    this.setupService
      .update(input)
      .pipe(
        finalize(() => this.saving.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(setup => {
        this.reset(setup);
        this.toaster.success('Erp::SavedSuccessfully');
      });
  }

  private load(): void {
    this.loading.set(true);
    forkJoin({
      setup: this.setupService.get(),
      dimensions: this.dimensionService.getList(),
      series: this.noSeriesService.getList({ maxResultCount: 1000, skipCount: 0 } as never),
    })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(({ setup, dimensions, series }) => {
        this.dimensions.set(dimensions.items ?? []);
        this.series.set(series.items ?? []);
        this.reset(setup);
      });
  }

  // Dates arrive as ISO date-times; the date inputs want yyyy-MM-dd.
  private reset(setup: GeneralLedgerSetupDto): void {
    this.form.reset({
      allowPostingFrom: setup.allowPostingFrom?.substring(0, 10) ?? null,
      allowPostingTo: setup.allowPostingTo?.substring(0, 10) ?? null,
      lcyCode: setup.lcyCode ?? null,
      localCurrencySymbol: setup.localCurrencySymbol ?? null,
      localCurrencyDescription: setup.localCurrencyDescription ?? null,
      additionalReportingCurrency: setup.additionalReportingCurrency ?? null,
      amountRoundingPrecision: setup.amountRoundingPrecision,
      unitAmountRoundingPrecision: setup.unitAmountRoundingPrecision,
      invRoundingPrecisionLcy: setup.invRoundingPrecisionLcy,
      invRoundingType: setup.invRoundingType ?? RoundingType.Nearest,
      vatRoundingType: setup.vatRoundingType ?? RoundingType.Nearest,
      pmtDiscExclVAT: setup.pmtDiscExclVAT ?? false,
      unrealizedVAT: setup.unrealizedVAT ?? false,
      adjustForPaymentDisc: setup.adjustForPaymentDisc ?? false,
      markCrMemosAsCorrections: setup.markCrMemosAsCorrections ?? false,
      maxVATDifferenceAllowed: setup.maxVATDifferenceAllowed ?? 0,
      paymentTolerancePct: setup.paymentTolerancePct ?? 0,
      maxPaymentToleranceAmount: setup.maxPaymentToleranceAmount ?? 0,
      blockDeletionOfGLAccounts: setup.blockDeletionOfGLAccounts ?? false,
      postWithJobQueue: setup.postWithJobQueue ?? false,
      jobQueueCategoryCode: setup.jobQueueCategoryCode ?? null,
      notifyOnSuccess: setup.notifyOnSuccess ?? false,
      registerTime: setup.registerTime ?? false,
      globalDimension1Code: setup.globalDimension1Code ?? null,
      globalDimension2Code: setup.globalDimension2Code ?? null,
      bankAccountNos: setup.bankAccountNos ?? null,
    });
  }
}
