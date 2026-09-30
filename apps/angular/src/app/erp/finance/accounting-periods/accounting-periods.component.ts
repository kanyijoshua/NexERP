import { LocalizationService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, Validators } from '@angular/forms';
import { AccountingPeriodDto, AccountingPeriodService } from '@proxy/finance';
import { filter, finalize, switchMap } from 'rxjs';
import { ErpTableAction, ErpTableColumn, ErpTableComponent, ErpTableSource } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

/**
 * Accounting Periods. Mirrors Business Central page 100 with its Create Year and Close Year actions.
 */
@Component({
  selector: 'app-accounting-periods',
  templateUrl: './accounting-periods.component.html',
  standalone: false,
})
export class AccountingPeriodsComponent implements OnInit {
  @ViewChild(ErpTableComponent) table?: ErpTableComponent<AccountingPeriodDto>;

  private readonly periods = inject(AccountingPeriodService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly localization = inject(LocalizationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  readonly columns: ErpTableColumn<AccountingPeriodDto>[] = [
    { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', width: 150 },
    { field: 'name', labelKey: 'Erp::Name', width: 180 },
    { field: 'newFiscalYear', labelKey: 'Erp::NewFiscalYear', type: 'boolean', width: 150 },
    { field: 'closed', labelKey: 'Erp::Closed', type: 'boolean', width: 120 },
    { field: 'dateLocked', labelKey: 'Erp::DateLocked', type: 'boolean', width: 130 },
  ];

  readonly actions: ErpTableAction<AccountingPeriodDto>[] = [
    {
      key: 'delete',
      title: 'Erp::Delete',
      icon: 'fas fa-trash',
      btnClass: 'btn-outline-danger',
      permission: 'Erp.FinanceSetup.Delete',
      visible: row => !row.closed,
      action: row => this.delete(row),
    },
  ];

  readonly source: ErpTableSource<AccountingPeriodDto> = query => this.periods.getList(query);

  readonly busy = signal(false);
  readonly creating = signal(false);

  readonly yearForm = this.fb.group({
    startingDate: this.fb.control<string>(`${new Date().getFullYear() + 1}-01-01`, Validators.required),
    noOfPeriods: this.fb.control<number>(12, [Validators.required, Validators.min(1), Validators.max(366)]),
    periodLength: this.fb.control<string>('1M', Validators.required),
  });

  ngOnInit(): void {
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.table?.reload());
  }

  /** The grid may not have loaded the last period, so it is asked for. */
  openCreateYear(): void {
    this.periods
      .getList({ sorting: 'startingDate desc', skipCount: 0, maxResultCount: 1 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        const last = result.items?.[0];
        if (last?.startingDate && last.newFiscalYear) {
          this.yearForm.patchValue({ startingDate: last.startingDate.substring(0, 10) });
        }
        this.creating.set(true);
      });
  }

  createYear(): void {
    if (this.yearForm.invalid) {
      this.yearForm.markAllAsTouched();
      return;
    }

    const value = this.yearForm.getRawValue();
    this.busy.set(true);
    this.periods
      .newFiscalYear({ startingDate: value.startingDate!, noOfPeriods: value.noOfPeriods!, periodLength: value.periodLength! })
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.creating.set(false);
        this.toaster.success('Erp::SavedSuccessfully');
        this.table?.reload();
      });
  }

  closeYear(): void {
    this.confirmation
      .warn('Erp::CloseYearConfirmation', 'Erp::CloseYear')
      .pipe(
        filter(status => status === Confirmation.Status.confirm),
        switchMap(() => {
          this.busy.set(true);
          return this.periods.closeFiscalYear().pipe(finalize(() => this.busy.set(false)));
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => {
        this.toaster.success(
          this.localization.instant(
            'Erp::FiscalYearClosed',
            result.fromDate?.substring(0, 10) ?? '',
            result.toDate?.substring(0, 10) ?? '',
          ),
        );
        this.table?.reload();
      });
  }

  delete(period: AccountingPeriodDto): void {
    this.confirmation
      .warn('AbpUi::ItemWillBeDeletedMessage', 'AbpUi::AreYouSure')
      .pipe(
        filter(status => status === Confirmation.Status.confirm),
        switchMap(() => this.periods.delete(period.id!)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.table?.reload());
  }
}
