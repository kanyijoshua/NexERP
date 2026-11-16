import { Component, DestroyRef, Input, OnChanges, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { StandardReportDto } from '@proxy/reporting';
import { Subscription } from 'rxjs';
import { standardReportQuery } from './record-reports';
import { StandardReportCatalogService } from './standard-report-catalog.service';

/**
 * The "Report" menu of a card or list: the standard reports that belong to the record, opened on
 * the standard reports page and run for the record's number. It can also print the page. Nothing
 * shows when there is neither a report the user may run nor printing.
 */
@Component({
  selector: 'erp-report-menu',
  templateUrl: './report-menu.component.html',
  standalone: false,
})
export class ReportMenuComponent implements OnChanges {
  private readonly catalog = inject(StandardReportCatalogService);
  private readonly destroyRef = inject(DestroyRef);

  /** Codes of the standard reports to offer, in menu order. */
  @Input() codes: readonly string[] = [];
  /** The record's number; the reports then run for this record only. */
  @Input() no?: string | null;
  /** Adds "Print Page" at the top of the menu. */
  @Input() showPrint = false;

  reports: StandardReportDto[] = [];
  private sub?: Subscription;

  ngOnChanges(): void {
    this.sub?.unsubscribe();
    this.sub = this.catalog
      .available(this.codes)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(reports => (this.reports = reports));
  }

  query(report: StandardReportDto): Record<string, string> {
    return standardReportQuery(report.code ?? '', this.no);
  }

  print(): void {
    window.print();
  }
}
