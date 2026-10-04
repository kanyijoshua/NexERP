import { ToasterService } from '@abp/ng.theme.shared';
import { AfterViewInit, Component, ElementRef, NgZone, OnDestroy, ViewChild, inject, signal } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { saveBlob } from '../report-page/save-blob';
import { MAX_SPREADSHEET_CELLS, SpreadsheetCell, SpreadsheetSheet, columnName, writeSpreadsheet } from './spreadsheet-io';

/** The parts of a jspreadsheet worksheet this view uses. */
interface Worksheet {
  getData(highlighted?: boolean, processed?: boolean): SpreadsheetCell[][];
  getValueFromCoords(x: number, y: number, processed?: boolean): SpreadsheetCell;
  setValueFromCoords(x: number, y: number, value: SpreadsheetCell): void;
  getStyle(cell: string, key?: string): string | null;
  setStyle(cell: string, key: string, value: string): void;
  getWidth(column: number): number | string;
  getSelected(columnNameOnly?: boolean): unknown[];
  insertRow(count?: number, row?: number): void;
  insertColumn(count?: number, column?: number): void;
  deleteRow(row?: number, count?: number): void;
  deleteColumn(column?: number, count?: number): void;
  undo(): void;
  redo(): void;
  getConfig(): { worksheetName?: string };
}

/**
 * A workbook opened inside the application (the Excel view): the cells can be edited, formulas
 * typed and recalculated, rows and columns added, and the result saved as an .xlsx file. Nothing
 * is written back to the ERP; it is a scratch copy of a report or of an attached file.
 */
@Component({
  selector: 'erp-spreadsheet-dialog',
  templateUrl: './spreadsheet-dialog.component.html',
  styleUrls: ['./spreadsheet-dialog.component.scss'],
  standalone: false,
})
export class SpreadsheetDialogComponent implements AfterViewInit, OnDestroy {
  readonly modal = inject(NgbActiveModal);
  private readonly zone = inject(NgZone);
  private readonly toaster = inject(ToasterService);

  @ViewChild('host', { static: true }) private host!: ElementRef<HTMLDivElement>;

  /** Set by `SpreadsheetDialogService` before the view initialises. */
  title = '';
  fileName = 'workbook.xlsx';
  sheets: SpreadsheetSheet[] | Promise<SpreadsheetSheet[]> = [];

  readonly loading = signal(true);
  readonly failed = signal(false);
  readonly saving = signal(false);
  /** Address of the selected cell, e.g. "B4". */
  readonly cellAddress = signal('');
  /** What the selected cell holds: the formula, not its result. */
  readonly cellContent = signal('');

  private library: any;
  private worksheets: Worksheet[] = [];
  private active = 0;
  private selection: { x: number; y: number; x2: number; y2: number } | null = null;

  async ngAfterViewInit(): Promise<void> {
    try {
      const [sheets, module] = await Promise.all([Promise.resolve(this.sheets), import('jspreadsheet-ce')]);
      this.library = (module as any).default ?? module;

      const cells = sheets.reduce((total, sheet) => total + sheet.rows.length * (sheet.rows[0]?.length ?? 0), 0);
      if (cells > MAX_SPREADSHEET_CELLS) {
        this.toaster.warn('Erp::SpreadsheetTooLarge');
        this.failed.set(true);
        return;
      }

      // Outside the zone: the grid listens to every mouse move and key press, and none of them
      // changes anything Angular renders.
      this.zone.runOutsideAngular(() => this.render(sheets.length ? sheets : [{ name: 'Sheet1', rows: [] }]));
    } catch {
      this.failed.set(true);
      this.toaster.error('Erp::SpreadsheetNotReadable');
    } finally {
      this.loading.set(false);
    }
  }

  ngOnDestroy(): void {
    if (!this.library) {
      return;
    }

    try {
      this.library.destroy(this.host.nativeElement, true);
    } catch {
      // The grid was never created; nothing to release.
    }

    // The library keeps the last worksheet that was clicked as its "current" one, and its
    // document-level mouse and key handlers act on it. Left pointing at a grid that is gone, the
    // next click anywhere in the application would be handled as if it were still open.
    this.library.current = null;
  }

  /** Writes the formula bar's text into the selected cell. */
  applyContent(value: string): void {
    const sheet = this.sheet;
    if (!sheet || !this.selection) {
      return;
    }

    const typed = value.trim();
    const numeric = typed !== '' && !typed.startsWith('=') && !isNaN(Number(typed));
    sheet.setValueFromCoords(this.selection.x, this.selection.y, numeric ? Number(typed) : value);
    this.cellContent.set(value);
  }

  undo(): void {
    this.sheet?.undo();
  }

  redo(): void {
    this.sheet?.redo();
  }

  insertRow(): void {
    this.sheet?.insertRow(1, this.selection?.y2);
  }

  insertColumn(): void {
    this.sheet?.insertColumn(1, this.selection?.x2);
  }

  deleteRow(): void {
    if (this.selection) {
      this.sheet?.deleteRow(this.selection.y, this.selection.y2 - this.selection.y + 1);
    }
  }

  deleteColumn(): void {
    if (this.selection) {
      this.sheet?.deleteColumn(this.selection.x, this.selection.x2 - this.selection.x + 1);
    }
  }

  toggleBold(): void {
    const sheet = this.sheet;
    if (!sheet || !this.selection) {
      return;
    }

    const first = columnName(this.selection.x) + (this.selection.y + 1);
    const bold = (sheet.getStyle(first, 'font-weight') ?? '') !== 'bold';

    for (let y = this.selection.y; y <= this.selection.y2; y++) {
      for (let x = this.selection.x; x <= this.selection.x2; x++) {
        sheet.setStyle(columnName(x) + (y + 1), 'font-weight', bold ? 'bold' : 'normal');
      }
    }
  }

  /** Sums the selected cells into the first empty cell below them, as Excel's AutoSum does. */
  autoSum(): void {
    const sheet = this.sheet;
    if (!sheet || !this.selection) {
      return;
    }

    const { x, y, x2, y2 } = this.selection;
    const data = sheet.getData();
    let target = y2 + 1;
    while (target < data.length && data[target].slice(x, x2 + 1).some(value => value !== null && value !== '')) {
      target++;
    }

    if (target >= data.length) {
      sheet.insertRow(1);
    }

    for (let column = x; column <= x2; column++) {
      const name = columnName(column);
      sheet.setValueFromCoords(column, target, `=SUM(${name}${y + 1}:${name}${y2 + 1})`);
    }
  }

  async download(): Promise<void> {
    this.saving.set(true);
    try {
      saveBlob(await writeSpreadsheet(this.snapshot()), this.fileName.replace(/\.(csv|xlsx)$/i, '') + '.xlsx');
    } finally {
      this.saving.set(false);
    }
  }

  private get sheet(): Worksheet | undefined {
    return this.worksheets[this.active];
  }

  /** The workbook as it stands, formulas included, for saving. */
  private snapshot(): SpreadsheetSheet[] {
    return this.worksheets.map((worksheet, index) => {
      const rows = worksheet.getData();
      const boldRows: number[] = [];
      rows.forEach((_, row) => {
        if (worksheet.getStyle('A' + (row + 1), 'font-weight') === 'bold') {
          boldRows.push(row);
        }
      });

      return {
        name: worksheet.getConfig().worksheetName || `Sheet${index + 1}`,
        rows,
        widths: (rows[0] ?? []).map((_, column) => Number(worksheet.getWidth(column)) || 100),
        boldRows,
      };
    });
  }

  private render(sheets: SpreadsheetSheet[]): void {
    this.worksheets = this.library(this.host.nativeElement, {
      tabs: sheets.length > 1,
      toolbar: false,
      about: false,
      parseFormulas: true,
      worksheets: sheets.map(sheet => {
        const columns = Math.max(sheet.rows.reduce((max, row) => Math.max(max, row.length), 0), 8);
        const style: Record<string, string> = {};
        for (const row of sheet.boldRows ?? []) {
          for (let column = 0; column < columns; column++) {
            style[columnName(column) + (row + 1)] = 'font-weight: bold;';
          }
        }

        return {
          worksheetName: sheet.name,
          data: sheet.rows.map(row => [...row]),
          minDimensions: [columns, Math.max(sheet.rows.length + 5, 20)],
          columns: Array.from({ length: columns }, (_, index) => ({ width: sheet.widths?.[index] ?? 120 })),
          style,
          tableOverflow: true,
          tableWidth: '100%',
          tableHeight: 'calc(100vh - 210px)',
          defaultColAlign: 'left',
          columnSorting: false,
        };
      }),
      onselection: (worksheet: Worksheet, x: number, y: number, x2: number, y2: number) => {
        this.active = Math.max(this.worksheets.indexOf(worksheet), 0);
        this.selection = { x: Math.min(x, x2), y: Math.min(y, y2), x2: Math.max(x, x2), y2: Math.max(y, y2) };
        const content = worksheet.getValueFromCoords(this.selection.x, this.selection.y);
        this.zone.run(() => {
          this.cellAddress.set(columnName(this.selection!.x) + (this.selection!.y + 1));
          this.cellContent.set(content === null || content === undefined ? '' : String(content));
        });
      },
      onchange: (worksheet: Worksheet, _cell: unknown, x: number | string, y: number | string, value: SpreadsheetCell) => {
        if (this.selection && Number(x) === this.selection.x && Number(y) === this.selection.y) {
          this.zone.run(() => this.cellContent.set(value === null || value === undefined ? '' : String(value)));
        }
      },
    }) as Worksheet[];
  }
}
