import { Component, EventEmitter, HostBinding, Input, Output } from '@angular/core';
import { ReportColumn } from '../models';

/**
 * Shell of a report: parameter form (`[parameters]` slot), Run / Export buttons, a generic
 * table and an optional `[chart]` slot.
 */
@Component({
  selector: 'erp-report-page',
  templateUrl: './report-page.component.html',
})
export class ReportPageComponent {
  @HostBinding('attr.title') readonly hostTitle = null;

  @Input() title = '';
  @Input() subtitle?: string;
  @Input() columns: ReportColumn[] = [];
  @Input() rows: any[] | null = [];
  @Input() totalsRow?: Record<string, any> | null;
  /** Extra CSS classes of a row, e.g. `row => row.isTotal ? 'fw-bold table-light' : ''`. */
  @Input() rowClass?: (row: any) => string | string[] | Record<string, boolean>;
  @Input() busy = false;
  /** Disables Run, e.g. while the parameter form is invalid. */
  @Input() runDisabled = false;
  @Input() showExport = true;
  @Input() exportPermission?: string;
  @Input() currencyCode?: string;
  @Input() dateFormat = 'mediumDate';
  @Input() emptyKey = 'Erp::NoDataAvailable';

  @Output() run = new EventEmitter<void>();
  @Output() exportExcel = new EventEmitter<void>();

  get hasRows(): boolean {
    return !!this.rows?.length;
  }

  alignClass(column: ReportColumn): string {
    const align =
      column.align ?? (column.type === 'number' || column.type === 'currency' ? 'end' : 'start');
    return `text-${align}`;
  }

  trackByField(_index: number, column: ReportColumn): string {
    return column.field;
  }
}
