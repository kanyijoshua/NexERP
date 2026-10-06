import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder } from '@angular/forms';
import { DimensionDto, DimensionService } from '@proxy/dimensions';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import {
  ExcessContributionAllocation,
  PensionSetupDto,
  PensionSetupService,
  PensionerPayModeDto,
  PensionerPayModeService,
  excessContributionAllocationOptions,
} from '@proxy/pensions';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/**
 * Pension Setup: the dimension whose values are the schemes, the number series and the G/L
 * accounts the pension processes post to.
 */
@Component({
  selector: 'app-pension-setup',
  templateUrl: './pension-setup.component.html',
  standalone: false,
})
export class PensionSetupComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly setupService = inject(PensionSetupService);
  private readonly noSeriesService = inject(NoSeriesService);
  private readonly dimensionService = inject(DimensionService);
  private readonly payModeService = inject(PensionerPayModeService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly series = signal<NoSeriesDto[]>([]);
  readonly dimensions = signal<DimensionDto[]>([]);
  readonly payModes = signal<PensionerPayModeDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly allocationOptions = excessContributionAllocationOptions;

  /** The number series fields, in the order the page lists them. */
  readonly seriesFields = [
    { control: 'memberNos', labelKey: 'Erp::MemberNos' },
    { control: 'sponsorNos', labelKey: 'Erp::SponsorNos' },
    { control: 'contributionNos', labelKey: 'Erp::ContributionNos' },
    { control: 'interestBatchNos', labelKey: 'Erp::InterestBatchNos' },
    { control: 'exitNos', labelKey: 'Erp::ExitNos' },
    { control: 'pensionerNos', labelKey: 'Erp::PensionerNos' },
    { control: 'payrollNos', labelKey: 'Erp::PayrollNos' },
    { control: 'benefitCalculationNos', labelKey: 'Erp::BenefitCalculationNos' },
    { control: 'incrementNos', labelKey: 'Erp::IncrementNos' },
  ] as const;

  /** The posting accounts, in the order the page lists them. */
  readonly accountFields = [
    { control: 'memberFundsAccountNo', labelKey: 'Erp::MemberFundsAccountNo' },
    { control: 'contributionAccrualAccountNo', labelKey: 'Erp::ContributionAccrualAccountNo' },
    { control: 'interestAccountNo', labelKey: 'Erp::InterestAccountNo' },
    { control: 'benefitsPayableAccountNo', labelKey: 'Erp::BenefitsPayableAccountNo' },
    { control: 'taxAccountNo', labelKey: 'Erp::TaxAccountNo' },
    { control: 'pensionsPaidAccountNo', labelKey: 'Erp::PensionsPaidAccountNo' },
    { control: 'transfersInAccountNo', labelKey: 'Erp::TransfersInAccountNo' },
  ] as const;

  readonly form = this.fb.group({
    schemeDimensionCode: this.fb.control<string | null>(null),
    memberNos: this.fb.control<string | null>(null),
    sponsorNos: this.fb.control<string | null>(null),
    contributionNos: this.fb.control<string | null>(null),
    interestBatchNos: this.fb.control<string | null>(null),
    exitNos: this.fb.control<string | null>(null),
    memberFundsAccountNo: this.fb.control<string | null>(null),
    contributionAccrualAccountNo: this.fb.control<string | null>(null),
    interestAccountNo: this.fb.control<string | null>(null),
    benefitsPayableAccountNo: this.fb.control<string | null>(null),
    taxAccountNo: this.fb.control<string | null>(null),
    pensionerNos: this.fb.control<string | null>(null),
    payrollNos: this.fb.control<string | null>(null),
    benefitCalculationNos: this.fb.control<string | null>(null),
    pensionsPaidAccountNo: this.fb.control<string | null>(null),
    allowContributionDuplication: this.fb.nonNullable.control(false),
    noOfDaysInAYear: this.fb.nonNullable.control(365),
    incrementNos: this.fb.control<string | null>(null),
    transfersInAccountNo: this.fb.control<string | null>(null),
    excessContributionAllocation: this.fb.nonNullable.control<ExcessContributionAllocation>(ExcessContributionAllocation.EmployeePriority),
    lifeCertificateFrequencyMonths: this.fb.nonNullable.control(12),
    defaultPayModeCode: this.fb.control<string | null>(null),
    trivialPensionLimit: this.fb.nonNullable.control(0),
  });

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.load());
  }

  save(): void {
    const value = this.form.getRawValue();
    const input: PensionSetupDto = {
      schemeDimensionCode: value.schemeDimensionCode || undefined,
      memberNos: value.memberNos || undefined,
      sponsorNos: value.sponsorNos || undefined,
      contributionNos: value.contributionNos || undefined,
      interestBatchNos: value.interestBatchNos || undefined,
      exitNos: value.exitNos || undefined,
      memberFundsAccountNo: value.memberFundsAccountNo || undefined,
      contributionAccrualAccountNo: value.contributionAccrualAccountNo || undefined,
      interestAccountNo: value.interestAccountNo || undefined,
      benefitsPayableAccountNo: value.benefitsPayableAccountNo || undefined,
      taxAccountNo: value.taxAccountNo || undefined,
      pensionerNos: value.pensionerNos || undefined,
      payrollNos: value.payrollNos || undefined,
      benefitCalculationNos: value.benefitCalculationNos || undefined,
      pensionsPaidAccountNo: value.pensionsPaidAccountNo || undefined,
      allowContributionDuplication: value.allowContributionDuplication,
      noOfDaysInAYear: Number(value.noOfDaysInAYear),
      incrementNos: value.incrementNos || undefined,
      transfersInAccountNo: value.transfersInAccountNo || undefined,
      excessContributionAllocation: Number(value.excessContributionAllocation),
      lifeCertificateFrequencyMonths: Number(value.lifeCertificateFrequencyMonths) || 0,
      defaultPayModeCode: value.defaultPayModeCode || undefined,
      trivialPensionLimit: Number(value.trivialPensionLimit) || 0,
    };

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
      dimensions: this.dimensionService.getList(),
      payModes: this.payModeService.getList({ maxResultCount: 1000, skipCount: 0 }),
    })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(({ setup, series, dimensions, payModes }) => {
        this.series.set(series.items ?? []);
        this.dimensions.set(dimensions.items ?? []);
        this.payModes.set(payModes.items ?? []);
        this.show(setup);
      });
  }

  private show(setup: PensionSetupDto): void {
    this.form.reset({
      schemeDimensionCode: setup.schemeDimensionCode ?? null,
      memberNos: setup.memberNos ?? null,
      sponsorNos: setup.sponsorNos ?? null,
      contributionNos: setup.contributionNos ?? null,
      interestBatchNos: setup.interestBatchNos ?? null,
      exitNos: setup.exitNos ?? null,
      memberFundsAccountNo: setup.memberFundsAccountNo ?? null,
      contributionAccrualAccountNo: setup.contributionAccrualAccountNo ?? null,
      interestAccountNo: setup.interestAccountNo ?? null,
      benefitsPayableAccountNo: setup.benefitsPayableAccountNo ?? null,
      taxAccountNo: setup.taxAccountNo ?? null,
      pensionerNos: setup.pensionerNos ?? null,
      payrollNos: setup.payrollNos ?? null,
      benefitCalculationNos: setup.benefitCalculationNos ?? null,
      pensionsPaidAccountNo: setup.pensionsPaidAccountNo ?? null,
      allowContributionDuplication: setup.allowContributionDuplication,
      noOfDaysInAYear: setup.noOfDaysInAYear || 365,
      incrementNos: setup.incrementNos ?? null,
      transfersInAccountNo: setup.transfersInAccountNo ?? null,
      excessContributionAllocation: setup.excessContributionAllocation ?? ExcessContributionAllocation.EmployeePriority,
      lifeCertificateFrequencyMonths: setup.lifeCertificateFrequencyMonths ?? 12,
      defaultPayModeCode: setup.defaultPayModeCode ?? null,
      trivialPensionLimit: setup.trivialPensionLimit ?? 0,
    });
  }
}
