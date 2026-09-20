import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ExportFormat } from '@proxy/exporting';
import {
  AccountScheduleDto,
  AccountScheduleService,
  AgedLedgerKind,
  AgingMethod,
  ColumnLayoutDto,
  ColumnLayoutService,
  FinancialReportService,
  ReportKind,
  ReportResultDto,
} from '@proxy/reporting';
import { Observable } from 'rxjs';
import { ReportColumn, saveBlob } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

/** A report row flattened for the generic table, with its formatting kept alongside. */
interface ViewerRow {
  [key: string]: unknown;
  __bold: boolean;
  __italic: boolean;
  __indent: number;
  __drill: string | null;
}

/**
 * Runs any of the financial reports and exports it.
 * <p>
 * Every report comes back in the same shape — columns and rows — so one screen serves the trial
 * balance, the statements, the aged analyses and any account schedule a user has defined.
 * </p>
 */
@Component({
  selector: 'app-report-viewer',
  templateUrl: './report-viewer.component.html',
})
export class ReportViewerComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly companyService = inject(CompanyService);
  private readonly reports = inject(FinancialReportService);
  private readonly schedules = inject(AccountScheduleService);
  private readonly layouts = inject(ColumnLayoutService);

  readonly ReportKind = ReportKind;

  readonly reportOptions = [
    { value: ReportKind.TrialBalance, label: 'Erp::TrialBalance' },
    { value: ReportKind.IncomeStatement, label: 'Erp::IncomeStatement' },
    { value: ReportKind.BalanceSheet, label: 'Erp::BalanceSheet' },
    { value: ReportKind.GeneralLedgerDetail, label: 'Erp::GeneralLedgerDetail' },
    { value: ReportKind.AgedReceivables, label: 'Erp::AgedReceivables' },
    { value: ReportKind.AgedPayables, label: 'Erp::AgedPayables' },
    { value: ReportKind.AccountSchedule, label: 'Erp::AccountSchedule' },
  ];

  readonly agingOptions = [
    { value: AgingMethod.DueDate, label: 'Erp::DueDate' },
    { value: AgingMethod.PostingDate, label: 'Erp::PostingDate' },
  ];

  report = ReportKind.TrialBalance;
  fromDate = ReportViewerComponent.startOfYear();
  toDate = ReportViewerComponent.today();
  accountFilter = '';
  excludeZeroBalances = true;
  agingMethod = AgingMethod.DueDate;
  periodLengthDays = 30;
  scheduleName = '';
  columnLayoutName = '';

  scheduleList: AccountScheduleDto[] = [];
  layoutList: ColumnLayoutDto[] = [];

  columns: ReportColumn[] = [];
  rows: ViewerRow[] = [];
  title = '';
  busy = false;

  get isAged(): boolean {
    return this.report === ReportKind.AgedReceivables || this.report === ReportKind.AgedPayables;
  }

  get isSchedule(): boolean {
    return this.report === ReportKind.AccountSchedule;
  }

  /** The account filter only applies to the reports that read accounts directly. */
  get usesAccountFilter(): boolean {
    return (
      this.report === ReportKind.TrialBalance || this.report === ReportKind.GeneralLedgerDetail
    );
  }

  get runDisabled(): boolean {
    return this.isSchedule && !this.scheduleName;
  }

  ngOnInit(): void {
    this.loadSetup();
    this.run();

    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.loadSetup();
      this.run();
    });
  }

  onReportChange(): void {
    this.columns = [];
    this.rows = [];
  }

  run(): void {
    if (this.runDisabled) {
      return;
    }

    this.busy = true;
    this.request()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: result => {
          this.busy = false;
          this.apply(result);
        },
        error: () => (this.busy = false),
      });
  }

  /** The parameters the running report was asked for, reused by every way of taking it away. */
  private exportRequest() {
    return {
      report: this.report,
      fromDate: this.fromDate,
      toDate: this.toDate,
      accountFilter: this.accountFilter || undefined,
      excludeZeroBalances: this.excludeZeroBalances,
      agingMethod: this.agingMethod,
      periodLengthDays: this.periodLengthDays,
      scheduleName: this.scheduleName || undefined,
      columnLayoutName: this.columnLayoutName || undefined,
    };
  }

  exportReport(format: ExportFormat): void {
    this.busy = true;
    this.reports
      .runExport({ ...this.exportRequest(), format })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: blob => {
          this.busy = false;
          saveBlob(blob, `${this.title || 'report'}.${ReportViewerComponent.extensionOf(format)}`);
        },
        error: () => (this.busy = false),
      });
  }

  /**
   * Prints through the layout this company has chosen for the report, which is the one thing the
   * on-screen grid cannot show.
   */
  print(): void {
    this.busy = true;
    this.reports
      .runExport({ ...this.exportRequest(), format: ExportFormat.Html })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: blob => {
          this.busy = false;
          this.openForPrinting(blob);
        },
        error: () => (this.busy = false),
      });
  }

  /**
   * Opened in its own window rather than a frame: the layout is a whole document with its own
   * styling, and the browser's print dialog belongs to it, not to the application.
   */
  private openForPrinting(blob: Blob): void {
    const url = URL.createObjectURL(blob);
    const printWindow = window.open(url, '_blank');

    if (!printWindow) {
      // Pop-ups blocked: fall back to saving the file, so the report is never simply lost.
      saveBlob(blob, `${this.title || 'report'}.html`);
      URL.revokeObjectURL(url);
      return;
    }

    printWindow.addEventListener('load', () => {
      printWindow.print();
      URL.revokeObjectURL(url);
    });
  }

  private static extensionOf(format: ExportFormat): string {
    switch (format) {
      case ExportFormat.Csv:
        return 'csv';
      case ExportFormat.Json:
        return 'json';
      case ExportFormat.Html:
        return 'html';
      default:
        return 'xlsx';
    }
  }

  /** Bold totals and italic reversals, and indentation shown as padding on the first cell. */
  rowClass = (row: ViewerRow): Record<string, boolean> => ({
    'fw-bold': row['__bold'] === true,
    'table-light': row['__bold'] === true,
    'fst-italic': row['__italic'] === true,
  });

  /** Drilling into a row switches to the detail report filtered to the row's accounts. */
  drillDown(row: ViewerRow): void {
    const filter = row['__drill'] as string | null;
    if (!filter) {
      return;
    }

    this.report = ReportKind.GeneralLedgerDetail;
    this.accountFilter = filter;
    this.run();
  }

  private request(): Observable<ReportResultDto> {
    const period = {
      fromDate: this.fromDate,
      toDate: this.toDate,
      accountFilter: this.accountFilter || undefined,
      excludeZeroBalances: this.excludeZeroBalances,
    };

    switch (this.report) {
      case ReportKind.IncomeStatement:
        return this.reports.getIncomeStatement(period);
      case ReportKind.BalanceSheet:
        return this.reports.getBalanceSheet(period);
      case ReportKind.GeneralLedgerDetail:
        return this.reports.getGLDetail(period);
      case ReportKind.AgedReceivables:
      case ReportKind.AgedPayables:
        return this.reports.getAgedAccounts({
          kind:
            this.report === ReportKind.AgedReceivables
              ? AgedLedgerKind.Receivables
              : AgedLedgerKind.Payables,
          asOfDate: this.toDate,
          agingMethod: this.agingMethod,
          periodLengthDays: this.periodLengthDays,
          excludeZeroBalances: this.excludeZeroBalances,
        });
      case ReportKind.AccountSchedule:
        return this.reports.runSchedule({
          scheduleName: this.scheduleName,
          columnLayoutName: this.columnLayoutName || undefined,
          fromDate: this.fromDate,
          toDate: this.toDate,
        });
      default:
        return this.reports.getTrialBalance(period);
    }
  }

  private apply(result: ReportResultDto): void {
    this.title = result.title ?? '';

    this.columns = (result.columns ?? []).map(column => ({
      field: column.key ?? '',
      // Headers come from the report or from the user's own column layout, so they are shown
      // as they are rather than looked up in the localization file.
      labelKey: column.header ?? '',
      type: column.kind === 0 ? 'text' : column.kind === 2 ? 'date' : 'currency',
    }));

    this.rows = (result.rows ?? []).map(row => ({
      ...((row.values ?? {}) as Record<string, unknown>),
      __bold: row.bold,
      __italic: row.italic,
      __indent: row.indentation,
      __drill: row.drillDownFilter ?? null,
    }));
  }

  private loadSetup(): void {
    this.schedules
      .getList()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.scheduleList = result.items ?? [];
        this.scheduleName ||= this.scheduleList[0]?.name ?? '';
      });

    this.layouts
      .getList()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.layoutList = result.items ?? []));
  }

  private static today(): string {
    return new Date().toISOString().substring(0, 10);
  }

  private static startOfYear(): string {
    return `${new Date().getFullYear()}-01-01`;
  }

  protected readonly ExportFormat = ExportFormat;
}
