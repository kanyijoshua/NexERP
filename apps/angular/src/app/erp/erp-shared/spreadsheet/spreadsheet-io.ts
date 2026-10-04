import { ReportColumn } from '../models';

export type SpreadsheetCell = string | number | boolean | null;

/** One worksheet as the spreadsheet view shows it. A cell that starts with `=` is a formula. */
export interface SpreadsheetSheet {
  name: string;
  rows: SpreadsheetCell[][];
  /** Column widths in pixels, by column. */
  widths?: number[];
  /** Zero-based numbers of the rows shown in bold: headings and totals. */
  boldRows?: number[];
}

/** Largest workbook the view opens; beyond it the grid becomes too slow to be of use. */
export const MAX_SPREADSHEET_CELLS = 200_000;

const XLSX_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';

/** Loaded on first use: the workbook library is large and most sessions never open a spreadsheet. */
async function loadExcelJs(): Promise<any> {
  const module: any = await import('exceljs/dist/exceljs.min.js');
  return module.default ?? module;
}

export function isSpreadsheetFile(fileName: string | null | undefined): boolean {
  return /\.(xlsx|csv)$/i.test(fileName ?? '');
}

/** "A", "B", ... "Z", "AA": the letters of a zero-based column number. */
export function columnName(index: number): string {
  let name = '';
  for (let n = index; n >= 0; n = Math.floor(n / 26) - 1) {
    name = String.fromCharCode(65 + (n % 26)) + name;
  }
  return name;
}

/** A report as a sheet: its headings, then its rows, with headings and totals in bold. */
export function reportToSheet(
  title: string,
  columns: ReportColumn[],
  rows: Record<string, unknown>[],
  isBold: (row: Record<string, unknown>) => boolean = () => false,
): SpreadsheetSheet {
  const boldRows = [0];
  const data: SpreadsheetCell[][] = [columns.map(c => c.labelKey)];

  rows.forEach((row, index) => {
    if (isBold(row)) {
      boldRows.push(index + 1);
    }

    data.push(
      columns.map(column => {
        const value = row[column.field];
        if (value === null || value === undefined) {
          return null;
        }

        if (column.type === 'date') {
          return String(value).substring(0, 10);
        }

        return typeof value === 'number' || typeof value === 'boolean' ? value : String(value);
      }),
    );
  });

  return {
    // Excel refuses these characters in a sheet name, and anything beyond 31 of them.
    name: (title || 'Sheet1').replace(/[\\/?*[\]:]/g, ' ').substring(0, 31),
    rows: data,
    widths: columns.map(c => (c.type === 'text' || !c.type ? 220 : 130)),
    boldRows,
  };
}

function cellValue(cell: any): SpreadsheetCell {
  const value = cell.value;

  if (value === null || value === undefined) {
    return null;
  }

  if (value instanceof Date) {
    return value.toISOString().substring(0, 10);
  }

  if (typeof value === 'object') {
    // A formula, or a cell sharing the formula of another: `cell.formula` is the formula as it
    // reads in this cell either way.
    if (cell.formula) {
      return '=' + cell.formula;
    }

    if (Array.isArray(value.richText)) {
      return value.richText.map((part: { text: string }) => part.text).join('');
    }

    if (value.text !== undefined) {
      return String(value.text);
    }

    if (value.error) {
      return String(value.error);
    }

    return value.result ?? null;
  }

  return value;
}

/** A delimited file as rows; quotes may wrap a field and are doubled inside one. */
export function parseCsv(text: string): SpreadsheetCell[][] {
  const rows: SpreadsheetCell[][] = [];
  let row: SpreadsheetCell[] = [];
  let field = '';
  let quoted = false;

  const endField = () => {
    const trimmed = field.trim();
    row.push(trimmed !== '' && !isNaN(Number(trimmed)) ? Number(trimmed) : field);
    field = '';
  };

  for (let i = 0; i < text.length; i++) {
    const ch = text[i];

    if (quoted) {
      if (ch === '"' && text[i + 1] === '"') {
        field += '"';
        i++;
      } else if (ch === '"') {
        quoted = false;
      } else {
        field += ch;
      }
    } else if (ch === '"') {
      quoted = true;
    } else if (ch === ',') {
      endField();
    } else if (ch === '\n' || ch === '\r') {
      if (ch === '\r' && text[i + 1] === '\n') {
        i++;
      }
      endField();
      rows.push(row);
      row = [];
    } else {
      field += ch;
    }
  }

  if (field !== '' || row.length > 0) {
    endField();
    rows.push(row);
  }

  return rows;
}

/** Reads an .xlsx or .csv file into sheets, keeping formulas as formulas. */
export async function readSpreadsheet(blob: Blob, fileName: string): Promise<SpreadsheetSheet[]> {
  if (/\.csv$/i.test(fileName)) {
    return [{ name: 'Sheet1', rows: parseCsv(await blob.text()) }];
  }

  const ExcelJS = await loadExcelJs();
  const workbook = new ExcelJS.Workbook();
  await workbook.xlsx.load(await blob.arrayBuffer());

  const sheets: SpreadsheetSheet[] = [];
  workbook.eachSheet((worksheet: any) => {
    const rows: SpreadsheetCell[][] = [];
    const boldRows: number[] = [];
    const columnCount = worksheet.actualColumnCount ? worksheet.columnCount : 0;

    for (let r = 1; r <= worksheet.rowCount; r++) {
      const source = worksheet.getRow(r);
      const row: SpreadsheetCell[] = [];
      for (let c = 1; c <= columnCount; c++) {
        row.push(cellValue(source.getCell(c)));
      }
      if (columnCount > 0 && source.getCell(1).font?.bold) {
        boldRows.push(r - 1);
      }
      rows.push(row);
    }

    const widths: number[] = [];
    for (let c = 1; c <= columnCount; c++) {
      // Excel measures a column in characters; about seven pixels each.
      widths.push(Math.round((worksheet.getColumn(c).width ?? 12) * 7 + 10));
    }

    sheets.push({ name: worksheet.name, rows, widths, boldRows });
  });

  return sheets;
}

/** Writes sheets as an .xlsx file. Formulas are written as formulas, so Excel recalculates them. */
export async function writeSpreadsheet(sheets: SpreadsheetSheet[]): Promise<Blob> {
  const ExcelJS = await loadExcelJs();
  const workbook = new ExcelJS.Workbook();

  for (const sheet of sheets) {
    const worksheet = workbook.addWorksheet(sheet.name || 'Sheet1');
    const bold = new Set(sheet.boldRows ?? []);

    sheet.rows.forEach((row, index) => {
      const target = worksheet.addRow(
        row.map(value => (typeof value === 'string' && value.startsWith('=') && value.length > 1 ? { formula: value.substring(1) } : value)),
      );
      if (bold.has(index)) {
        target.font = { bold: true };
      }
    });

    (sheet.widths ?? []).forEach((width, index) => {
      worksheet.getColumn(index + 1).width = Math.max(4, Math.round((width - 10) / 7));
    });
  }

  return new Blob([await workbook.xlsx.writeBuffer()], { type: XLSX_TYPE });
}
