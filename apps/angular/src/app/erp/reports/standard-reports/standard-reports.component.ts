import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { ExportFormat } from '@proxy/exporting';
import { ReportResultDto, RunStandardReportInput, StandardReportDto, StandardReportService } from '@proxy/reporting';
import { finalize } from 'rxjs';
import { ReportColumn, SpreadsheetDialogService, saveBlob } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

/** A report row flattened for the table, with its formatting kept alongside. */
interface StandardReportRow {
  [key: string]: unknown;
  __bold: boolean;
  __italic: boolean;
  __indent: number;
}

interface ReportGroup {
  area: string;
  reports: StandardReportDto[];
}

/**
 * The standard reports: the list, balance and register reports of finance,
 * receivables, payables, cash management, human resources and fixed assets. The server says which
 * reports the user may run and what each one asks for, so a report added there shows up here.
 */
@Component({
  selector: 'app-standard-reports',
  templateUrl: './standard-reports.component.html',
  standalone: false,
})
export class StandardReportsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly companyService = inject(CompanyService);
  private readonly service = inject(StandardReportService);
  private readonly spreadsheets = inject(SpreadsheetDialogService);

  protected readonly ExportFormat = ExportFormat;

  readonly reports = signal<StandardReportDto[]>([]);
  readonly selected = signal<StandardReportDto | null>(null);
  readonly columns = signal<ReportColumn[]>([]);
  readonly rows = signal<StandardReportRow[]>([]);
  readonly title = signal('');
  readonly busy = signal(false);
  readonly hasRun = signal(false);

  /** The reports under their area, in the order the server lists them. */
  readonly groups = computed<ReportGroup[]>(() => {
    const groups: ReportGroup[] = [];
    for (const report of this.reports()) {
      const area = report.area ?? '';
      let group = groups.find(g => g.area === area);
      if (!group) {
        group = { area, reports: [] };
        groups.push(group);
      }
      group.reports.push(report);
    }
    return groups;
  });

  fromDate = `${new Date().getFullYear()}-01-01`;
  toDate = new Date().toISOString().substring(0, 10);
  noFilter = '';
  budgetName = '';
  depreciationBookCode = '';
  schemeCode = '';

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.clear());
  }

  select(report: StandardReportDto): void {
    this.selected.set(report);
    this.clear();
  }

  run(): void {
    const report = this.selected();
    if (!report) {
      return;
    }

    this.busy.set(true);
    this.service
      .run(this.request(report))
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => this.apply(result));
  }

  /** Opens the report in the spreadsheet view, where it can be worked on without downloading it. */
  openInSpreadsheet(): void {
    this.spreadsheets.openReport(this.title(), this.columns(), this.rows(), row => row['__bold'] === true);
  }

  exportReport(format: ExportFormat): void {
    const report = this.selected();
    if (!report) {
      return;
    }

    this.busy.set(true);
    this.service
      .runExport({ ...this.request(report), format })
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(blob => {
        if (format === ExportFormat.Html) {
          this.openForPrinting(blob, report);
        } else {
          saveBlob(blob, `${report.name ?? 'report'}.${format === ExportFormat.Csv ? 'csv' : 'xlsx'}`);
        }
      });
  }

  rowClass = (row: StandardReportRow): Record<string, boolean> => ({
    'fw-bold': row.__bold,
    'table-light': row.__bold,
    'fst-italic': row.__italic,
  });

  private request(report: StandardReportDto): RunStandardReportInput {
    return {
      code: report.code ?? '',
      fromDate: report.hasPeriod ? this.fromDate || undefined : undefined,
      toDate: report.hasPeriod || report.hasAsOfDate ? this.toDate || undefined : undefined,
      noFilter: report.hasNoFilter ? this.noFilter || undefined : undefined,
      budgetName: report.hasBudgetName ? this.budgetName || undefined : undefined,
      depreciationBookCode: report.hasDepreciationBook ? this.depreciationBookCode || undefined : undefined,
      schemeCode: report.hasScheme ? this.schemeCode || undefined : undefined,
    };
  }

  private apply(result: ReportResultDto): void {
    this.title.set(result.title ?? '');
    this.columns.set(
      (result.columns ?? []).map(column => ({
        field: column.key ?? '',
        // Headers come from the report itself, so they are shown as they are.
        labelKey: column.header ?? '',
        type: column.kind === 0 ? 'text' : column.kind === 2 ? 'date' : 'currency',
      })),
    );
    this.rows.set(
      (result.rows ?? []).map(row => ({
        ...((row.values ?? {}) as Record<string, unknown>),
        __bold: row.bold,
        __italic: row.italic,
        __indent: row.indentation,
      })),
    );
    this.hasRun.set(true);
  }

  private clear(): void {
    this.columns.set([]);
    this.rows.set([]);
    this.title.set('');
    this.hasRun.set(false);
  }

  private load(): void {
    this.service
      .getList()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(reports => {
        this.reports.set(reports ?? []);

        // A link from another page, or from Report Selections, can name the report to open.
        const wanted = this.route.snapshot.queryParamMap.get('report');
        const match = wanted ? reports.find(r => r.code?.toLowerCase() === wanted.toLowerCase() || String(r.id) === wanted) : null;
        this.selected.set(match ?? reports[0] ?? null);
      });
  }

  /** Opened in its own window: the layout is a whole document, and the print dialog belongs to it. */
  private openForPrinting(blob: Blob, report: StandardReportDto): void {
    const url = URL.createObjectURL(blob);
    const printWindow = window.open(url, '_blank');

    if (!printWindow) {
      saveBlob(blob, `${report.name ?? 'report'}.html`);
      URL.revokeObjectURL(url);
      return;
    }

    printWindow.addEventListener('load', () => {
      printWindow.print();
      URL.revokeObjectURL(url);
    });
  }
}
