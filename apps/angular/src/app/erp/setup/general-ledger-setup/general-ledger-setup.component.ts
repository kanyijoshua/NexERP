import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, Validators } from '@angular/forms';
import { DimensionDto, DimensionService } from '@proxy/dimensions';
import { GeneralLedgerSetupDto, GeneralLedgerSetupService, RoundingType, roundingTypeOptions } from '@proxy/finance';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';
import { DimensionStateService } from '../../services/dimension-state.service';

/**
 * General Ledger Setup. the company's allowed posting dates,
 * local currency, rounding precisions and up to 8 dimensions (2 global + 6 shortcut), one record per company.
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
  private readonly dimensionState = inject(DimensionStateService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly roundingTypeOptions = roundingTypeOptions;
  readonly dimensions = signal<DimensionDto[]>([]);
  readonly series = signal<NoSeriesDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly globalDimensionControls = [
    { control: 'globalDimension1Code', labelKey: 'Erp::Setup:GlobalDimension1Code', slot: 1 },
    { control: 'globalDimension2Code', labelKey: 'Erp::Setup:GlobalDimension2Code', slot: 2 },
  ] as const;

  readonly shortcutDimensionControls = [
    { control: 'shortcutDimension3Code', labelKey: 'Erp::Setup:ShortcutDimension3Code', slot: 3 },
    { control: 'shortcutDimension4Code', labelKey: 'Erp::Setup:ShortcutDimension4Code', slot: 4 },
    { control: 'shortcutDimension5Code', labelKey: 'Erp::Setup:ShortcutDimension5Code', slot: 5 },
    { control: 'shortcutDimension6Code', labelKey: 'Erp::Setup:ShortcutDimension6Code', slot: 6 },
    { control: 'shortcutDimension7Code', labelKey: 'Erp::Setup:ShortcutDimension7Code', slot: 7 },
    { control: 'shortcutDimension8Code', labelKey: 'Erp::Setup:ShortcutDimension8Code', slot: 8 },
  ] as const;

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
    shortcutDimension3Code: this.fb.control<string | null>(null),
    shortcutDimension4Code: this.fb.control<string | null>(null),
    shortcutDimension5Code: this.fb.control<string | null>(null),
    shortcutDimension6Code: this.fb.control<string | null>(null),
    shortcutDimension7Code: this.fb.control<string | null>(null),
    shortcutDimension8Code: this.fb.control<string | null>(null),
    bankAccountNos: this.fb.control<string | null>(null),
  });

  /** The setup as last read, so a save sends back the fields this page has no control for. */
  private current?: GeneralLedgerSetupDto;

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
      // Fields this page does not show keep what the server holds.
      ...this.current,
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
      shortcutDimension3Code: value.shortcutDimension3Code || undefined,
      shortcutDimension4Code: value.shortcutDimension4Code || undefined,
      shortcutDimension5Code: value.shortcutDimension5Code || undefined,
      shortcutDimension6Code: value.shortcutDimension6Code || undefined,
      shortcutDimension7Code: value.shortcutDimension7Code || undefined,
      shortcutDimension8Code: value.shortcutDimension8Code || undefined,
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
        this.dimensionState.reload();
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
    this.current = setup;
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
      shortcutDimension3Code: setup.shortcutDimension3Code ?? null,
      shortcutDimension4Code: setup.shortcutDimension4Code ?? null,
      shortcutDimension5Code: setup.shortcutDimension5Code ?? null,
      shortcutDimension6Code: setup.shortcutDimension6Code ?? null,
      shortcutDimension7Code: setup.shortcutDimension7Code ?? null,
      shortcutDimension8Code: setup.shortcutDimension8Code ?? null,
      bankAccountNos: setup.bankAccountNos ?? null,
    });
  }
}
