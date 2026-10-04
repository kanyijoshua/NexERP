import { columnName, isSpreadsheetFile, parseCsv, readSpreadsheet, reportToSheet, writeSpreadsheet } from './spreadsheet-io';

describe('spreadsheet-io', () => {
  it('names columns the way Excel does', () => {
    expect([0, 1, 25, 26, 27, 701, 702].map(columnName)).toEqual(['A', 'B', 'Z', 'AA', 'AB', 'ZZ', 'AAA']);
  });

  it('recognises the files the spreadsheet view can open', () => {
    expect(isSpreadsheetFile('Budget 2026.XLSX')).toBeTrue();
    expect(isSpreadsheetFile('rates.csv')).toBeTrue();
    expect(isSpreadsheetFile('contract.pdf')).toBeFalse();
    expect(isSpreadsheetFile(undefined)).toBeFalse();
  });

  it('turns a report into a sheet with its headings and totals in bold', () => {
    const sheet = reportToSheet(
      'Vendor - Trial Balance: 2026',
      [
        { field: 'no', labelKey: 'No.', type: 'text' },
        { field: 'postingDate', labelKey: 'Posting Date', type: 'date' },
        { field: 'balance', labelKey: 'Balance', type: 'currency' },
      ],
      [
        { no: 'V0010', postingDate: '2026-03-01T00:00:00', balance: 1200.5, __bold: false },
        { no: '', postingDate: null, balance: 1200.5, __bold: true },
      ],
      row => row['__bold'] === true,
    );

    expect(sheet.name).toBe('Vendor - Trial Balance  2026');
    expect(sheet.rows).toEqual([
      ['No.', 'Posting Date', 'Balance'],
      ['V0010', '2026-03-01', 1200.5],
      ['', null, 1200.5],
    ]);
    expect(sheet.boldRows).toEqual([0, 2]);
  });

  it('reads quoted fields, doubled quotes and numbers from a delimited file', () => {
    expect(parseCsv('No,Name,Amount\r\n10,"Rift, Valley ""A""",12.5\n')).toEqual([
      ['No', 'Name', 'Amount'],
      [10, 'Rift, Valley "A"', 12.5],
    ]);
  });

  it('writes a workbook and reads it back with its formulas intact', async () => {
    const blob = await writeSpreadsheet([
      {
        name: 'Budget',
        rows: [
          ['Account', 'Amount'],
          ['Rent', 1000],
          ['Power', 250],
          ['Total', '=SUM(B2:B3)'],
        ],
        boldRows: [0, 3],
        widths: [220, 130],
      },
    ]);

    const [sheet] = await readSpreadsheet(blob, 'budget.xlsx');

    expect(sheet.name).toBe('Budget');
    expect(sheet.rows[1]).toEqual(['Rent', 1000]);
    expect(sheet.rows[3]).toEqual(['Total', '=SUM(B2:B3)']);
    expect(sheet.boldRows).toEqual([0, 3]);
  });
});
