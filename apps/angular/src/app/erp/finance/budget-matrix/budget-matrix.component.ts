import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AnalysisPeriodType,
  BudgetMatrixDto,
  BudgetMatrixRowDto,
  BudgetMatrixService,
  GLAccountType,
  GlBudgetNameService,
  analysisPeriodTypeOptions,
} from '@proxy/finance';
import { finalize, map } from 'rxjs';

/**
 * The G/L budget as a matrix: accounts down, periods across. A cell is typed in place and replaces
 * the budget of that account in that period. With "Show actual" each cell also shows what was
 * posted and the variance, and the chart compares the period totals.
 */
@Component({
  selector: 'app-budget-matrix',
  templateUrl: './budget-matrix.component.html',
  standalone: false,
})
export class BudgetMatrixComponent implements OnInit {
  private readonly service = inject(BudgetMatrixService);
  private readonly budgetNames = inject(GlBudgetNameService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly posting = GLAccountType.Posting;
  readonly periodTypes = analysisPeriodTypeOptions.map(o => ({ value: o.value, label: `Erp::Enum:AnalysisPeriodType.${o.key}` }));
  readonly budgets = signal<{ name: string; description?: string }[]>([]);
  readonly matrix = signal<BudgetMatrixDto | null>(null);
  readonly busy = signal(false);

  budgetName = '';
  startDate = `${new Date().getFullYear()}-01-01`;
  periodType = AnalysisPeriodType.Month;
  periodCount = 12;
  accountFilter = '';
  withActual = true;
  incomeStatementOnly = true;

  /** Budget and actual of the posting accounts summed per period, for the chart. */
  readonly totals = computed(() => {
    const m = this.matrix();
    if (!m) {
      return [];
    }
    const rows = m.rows.filter(r => r.accountType === GLAccountType.Posting);
    return m.periods.map((period, i) => ({
      name: period.name ?? '',
      budget: rows.reduce((sum, r) => sum + (r.budget[i] ?? 0), 0),
      actual: rows.reduce((sum, r) => sum + (r.actual[i] ?? 0), 0),
    }));
  });

  readonly chartMax = computed(() => Math.max(1, ...this.totals().flatMap(t => [Math.abs(t.budget), Math.abs(t.actual)])));

  ngOnInit(): void {
    this.budgetNames
      .getList({ maxResultCount: 100, skipCount: 0 } as never)
      .pipe(
        map(result => (result.items ?? []).map(b => ({ name: b.name ?? '', description: b.description ?? undefined }))),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(budgets => {
        this.budgets.set(budgets);
        if (!this.budgetName && budgets.length) {
          this.budgetName = budgets[0].name;
          this.load();
        }
      });
  }

  load(): void {
    if (!this.budgetName) {
      return;
    }

    this.busy.set(true);
    this.service
      .getMatrix({
        budgetName: this.budgetName,
        startDate: this.startDate,
        periodType: this.periodType,
        periodCount: this.periodCount,
        accountFilter: this.accountFilter || undefined,
        withActual: this.withActual,
        incomeStatementOnly: this.incomeStatementOnly,
      })
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(matrix => this.matrix.set(matrix));
  }

  /** Saves a typed cell; the row's total follows at once. */
  setCell(row: BudgetMatrixRowDto, index: number, value: string | number): void {
    const amount = Number(value) || 0;
    if (amount === row.budget[index]) {
      return;
    }

    const period = this.matrix()!.periods[index];
    this.service
      .setCell({ budgetName: this.budgetName, accountNo: row.accountNo!, periodStart: period.start, periodEnd: period.end, amount })
      .subscribe(() => {
        row.budget[index] = amount;
        row.totalBudget = row.budget.reduce((sum, b) => sum + b, 0);
        this.matrix.set({ ...this.matrix()! });
        this.toaster.success('Erp::SavedSuccessfully');
      });
  }

  variance(row: BudgetMatrixRowDto, index: number): number {
    return (row.actual[index] ?? 0) - (row.budget[index] ?? 0);
  }

  barWidth(value: number): number {
    return Math.round((Math.abs(value) / this.chartMax()) * 100);
  }
}
