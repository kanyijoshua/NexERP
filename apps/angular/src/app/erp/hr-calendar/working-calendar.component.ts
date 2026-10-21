import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { BaseCalendarService, CalendarDateDto, CalendarWeekday, WorkingPeriodDto } from '@proxy/human-resources';
import { finalize } from 'rxjs';
import { CompanyService } from '../services/company.service';

/** One cell of the month grid. */
export interface CalendarCell {
  key: string;
  day: number;
  inMonth: boolean;
  today: boolean;
  nonworking: boolean;
  description?: string;
  weekday: CalendarWeekday;
}

/** `Date` -> `2026-12-25`, in local time. */
export function toKey(date: Date): string {
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${m}-${d}`;
}

/** The Monday on or before the first of the month: where a six-week grid starts. */
export function gridStart(year: number, month: number): Date {
  const first = new Date(year, month, 1);
  const offset = (first.getDay() + 6) % 7;
  return new Date(year, month, 1 - offset);
}

/**
 * The working calendar: a month of a base calendar (or the one an employee works by) with its
 * nonworking days, making single dates working or nonworking, and where a leave of a number of
 * working days ends and when the employee is back.
 */
@Component({
  selector: 'app-working-calendar',
  templateUrl: './working-calendar.component.html',
  styleUrls: ['./working-calendar.component.scss'],
  standalone: false,
})
export class WorkingCalendarComponent implements OnInit {
  private readonly calendars = inject(BaseCalendarService);
  private readonly fb = inject(FormBuilder);
  private readonly toaster = inject(ToasterService);
  private readonly route = inject(ActivatedRoute);
  private readonly companyService = inject(CompanyService);
  private readonly destroyRef = inject(DestroyRef);

  readonly canManage = inject(PermissionService).getGrantedPolicy('Erp.HumanResourcesSetup.Update');
  readonly weekdays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

  readonly year = signal(new Date().getFullYear());
  readonly month = signal(new Date().getMonth());
  readonly cells = signal<CalendarCell[]>([]);
  readonly resolvedCode = signal<string | null>(null);
  readonly loading = signal(false);
  readonly selected = signal<CalendarCell | null>(null);
  readonly period = signal<WorkingPeriodDto | null>(null);

  readonly monthDate = computed(() => new Date(this.year(), this.month(), 1));
  readonly monthNonworking = computed(() => this.cells().filter(c => c.inMonth && c.nonworking));
  readonly monthWorkingDays = computed(() => this.cells().filter(c => c.inMonth && !c.nonworking).length);

  readonly filter = this.fb.group({
    calendarCode: this.fb.control<string | null>(null),
    employeeNo: this.fb.control<string | null>(null),
  });

  readonly dateForm = this.fb.group({
    description: this.fb.control('', Validators.maxLength(250)),
  });

  readonly leave = this.fb.group({
    startDate: this.fb.control(toKey(new Date()), Validators.required),
    workingDays: this.fb.control(5, [Validators.required, Validators.min(1), Validators.max(3650)]),
  });

  ngOnInit(): void {
    const code = this.route.snapshot.queryParamMap.get('calendar');
    if (code) {
      this.filter.patchValue({ calendarCode: code }, { emitEvent: false });
    }
    this.filter.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.period.set(null);
      this.load();
    });
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.load());
    this.load();
  }

  move(months: number): void {
    const next = new Date(this.year(), this.month() + months, 1);
    this.year.set(next.getFullYear());
    this.month.set(next.getMonth());
    this.load();
  }

  thisMonth(): void {
    const now = new Date();
    this.year.set(now.getFullYear());
    this.month.set(now.getMonth());
    this.load();
  }

  select(cell: CalendarCell): void {
    this.selected.set(cell);
    this.dateForm.reset({ description: cell.description ?? '' });
  }

  /** Makes the selected date working or nonworking; only a calendar, not an employee view, is changed. */
  setStatus(nonworking: boolean): void {
    const cell = this.selected();
    const code = this.resolvedCode();
    if (!cell || !code || this.dateForm.invalid) {
      return;
    }
    this.loading.set(true);
    this.calendars
      .setDateStatus({ calendarCode: code, date: cell.key, nonworking, description: this.dateForm.value.description || undefined })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toaster.success('Erp::SavedSuccessfully');
          this.load(cell.key);
        },
        error: () => this.loading.set(false),
      });
  }

  calculate(): void {
    if (this.leave.invalid) {
      this.leave.markAllAsTouched();
      return;
    }
    const value = this.leave.getRawValue();
    const filter = this.filter.getRawValue();
    this.calendars
      .calculateWorkingPeriod({
        calendarCode: filter.calendarCode || undefined,
        employeeNo: filter.employeeNo || undefined,
        startDate: value.startDate!,
        workingDays: Number(value.workingDays),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(period => this.period.set(period));
  }

  /** Whether a cell falls inside the calculated leave period. */
  inPeriod(cell: CalendarCell): boolean {
    const p = this.period();
    return !!p && cell.key >= p.startDate.substring(0, 10) && cell.key <= p.endDate.substring(0, 10);
  }

  isReturnDate(cell: CalendarCell): boolean {
    return this.period()?.returnDate.substring(0, 10) === cell.key;
  }

  weekdayName(weekday: CalendarWeekday): string {
    return CalendarWeekday[weekday];
  }

  private load(keepSelected?: string): void {
    const start = gridStart(this.year(), this.month());
    const end = new Date(start.getFullYear(), start.getMonth(), start.getDate() + 41);
    const filter = this.filter.getRawValue();

    this.loading.set(true);
    this.calendars
      .getDates({
        calendarCode: filter.calendarCode || undefined,
        employeeNo: filter.employeeNo || undefined,
        fromDate: toKey(start),
        toDate: toKey(end),
      })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => {
        this.resolvedCode.set(result.calendarCode ?? null);
        const today = toKey(new Date());
        const cells = (result.dates ?? []).map((d: CalendarDateDto) => {
          const key = d.date.substring(0, 10);
          return {
            key,
            day: Number(key.substring(8, 10)),
            inMonth: Number(key.substring(5, 7)) - 1 === this.month(),
            today: key === today,
            nonworking: d.nonworking,
            description: d.description ?? undefined,
            weekday: d.weekday,
          };
        });
        this.cells.set(cells);
        const again = keepSelected ? cells.find(c => c.key === keepSelected) : undefined;
        if (again) {
          this.select(again);
        } else if (!keepSelected) {
          this.selected.set(null);
        }
      });
  }
}
