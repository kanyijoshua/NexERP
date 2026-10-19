import { Component, EventEmitter, Input, Output } from '@angular/core';

/** One row of a week grid: a label and seven values, Monday to Sunday. */
export interface WeekGridRow {
  label: string;
  sublabel?: string;
  values: number[];
  readonly?: boolean;
}

export interface WeekGridChange {
  rowIndex: number;
  dayIndex: number;
  value: number;
}

/**
 * A week as a grid: one row per line, a column per day with its date, totals per row, per day and
 * for the week. Cells are typed in place unless the grid or the row is read-only.
 */
@Component({
  selector: 'erp-week-grid',
  templateUrl: './week-grid.component.html',
  standalone: false,
})
export class WeekGridComponent {
  /** The first day of the week, as yyyy-MM-dd. */
  @Input() startDate = '';
  @Input() rows: WeekGridRow[] = [];
  @Input() readonly = false;
  /** Largest value a cell takes, 24 for hours. */
  @Input() max = 24;
  @Output() valueChange = new EventEmitter<WeekGridChange>();

  get days(): Date[] {
    const start = this.startDate ? new Date(this.startDate + 'T00:00:00') : new Date();
    return Array.from({ length: 7 }, (_, i) => new Date(start.getFullYear(), start.getMonth(), start.getDate() + i));
  }

  rowTotal(row: WeekGridRow): number {
    return row.values.reduce((sum, v) => sum + (Number(v) || 0), 0);
  }

  dayTotal(dayIndex: number): number {
    return this.rows.reduce((sum, row) => sum + (Number(row.values[dayIndex]) || 0), 0);
  }

  get total(): number {
    return this.rows.reduce((sum, row) => sum + this.rowTotal(row), 0);
  }

  isWeekend(day: Date): boolean {
    return day.getDay() === 0 || day.getDay() === 6;
  }

  change(rowIndex: number, dayIndex: number, raw: string | number): void {
    const parsed = Number(raw);
    const value = Number.isFinite(parsed) ? Math.min(Math.max(parsed, 0), this.max) : 0;
    this.rows[rowIndex].values[dayIndex] = value;
    this.valueChange.emit({ rowIndex, dayIndex, value });
  }
}
