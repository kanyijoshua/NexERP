import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import {
  AnalysisPeriodType,
  CashFlowByPeriodDto,
  CashFlowForecastDto,
  CashFlowForecastService,
  CashFlowSourceType,
  analysisPeriodTypeOptions,
} from '@proxy/finance';
import { finalize } from 'rxjs';

/**
 * A registered cash flow forecast by period: one row per source (liquid funds, receivables,
 * payables, orders, budget, manual), the net flow of each period and the cash expected at its end.
 */
@Component({
  selector: 'app-cash-flow-by-period',
  templateUrl: './cash-flow-by-period.component.html',
  standalone: false,
})
export class CashFlowByPeriodComponent implements OnInit {
  private readonly service = inject(CashFlowForecastService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly sourceType = CashFlowSourceType;
  readonly periodTypes = analysisPeriodTypeOptions.map(o => ({ value: o.value, label: `Erp::Enum:AnalysisPeriodType.${o.key}` }));
  readonly forecast = signal<CashFlowForecastDto | null>(null);
  readonly result = signal<CashFlowByPeriodDto | null>(null);
  readonly busy = signal(false);

  startDate = new Date().toISOString().substring(0, 8) + '01';
  periodType = AnalysisPeriodType.Month;
  periodCount = 12;

  /** The largest absolute balance, to scale the bars of the balance line. */
  readonly maxBalance = computed(() => Math.max(1, ...(this.result()?.balances ?? []).map(b => Math.abs(b))));

  private id = '';

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id') ?? '';
    this.service.get(this.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(f => this.forecast.set(f));
    this.load();
  }

  load(): void {
    this.busy.set(true);
    this.service
      .getByPeriod(this.id, { startDate: this.startDate, periodType: this.periodType, periodCount: this.periodCount })
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => this.result.set(result));
  }

  barHeight(value: number): number {
    return Math.round((Math.abs(value) / this.maxBalance()) * 100);
  }
}
