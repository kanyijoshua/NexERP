import { Injectable, Injector, inject } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ReportColumn } from '../models';
import { SpreadsheetDialogComponent } from './spreadsheet-dialog.component';
import { SpreadsheetSheet, readSpreadsheet, reportToSheet } from './spreadsheet-io';

/**
 * Opens a workbook in the application instead of handing it to the browser as a download: a
 * report that is on screen, or an .xlsx or .csv file that has been fetched.
 */
@Injectable({ providedIn: 'root' })
export class SpreadsheetDialogService {
  private readonly modal = inject(NgbModal);
  private readonly injector = inject(Injector);

  open(title: string, sheets: SpreadsheetSheet[] | Promise<SpreadsheetSheet[]>, fileName?: string): void {
    const ref = this.modal.open(SpreadsheetDialogComponent, {
      fullscreen: true,
      injector: this.injector,
      // Escape cancels a cell edit in the grid; it must not also throw the workbook away.
      keyboard: false,
    });
    const dialog = ref.componentInstance as SpreadsheetDialogComponent;
    dialog.title = title;
    dialog.fileName = fileName ?? `${title || 'workbook'}.xlsx`;
    dialog.sheets = sheets;
    // Closing is not an error.
    ref.result.catch(() => undefined);
  }

  /** Opens an .xlsx or .csv file; its formulas stay formulas. */
  openFile(blob: Blob, fileName: string): void {
    this.open(fileName, readSpreadsheet(blob, fileName), fileName);
  }

  /** Opens the rows of a report that is on screen. */
  openReport(
    title: string,
    columns: ReportColumn[],
    rows: Record<string, unknown>[],
    isBold?: (row: Record<string, unknown>) => boolean,
  ): void {
    this.open(title, [reportToSheet(title, columns, rows, isBold)]);
  }
}
