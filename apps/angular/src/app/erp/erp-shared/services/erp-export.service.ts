import { Injectable, inject } from '@angular/core';
import { ToasterService } from '@abp/ng.theme.shared';
import { saveBlob } from '../report-page/save-blob';

export interface ErpExportColumn {
  field: string;
  title: string;
  type?:
    | 'text'
    | 'number'
    | 'currency'
    | 'date'
    | 'datetime'
    | 'boolean'
    | 'select'
    | 'badge'
    | 'code'
    | 'link'
    | 'switch'
    | 'custom';
  formatter?: (value: any, row: any) => string | number;
}

export interface ErpExportOptions {
  fileName?: string;
  sheetName?: string;
  title?: string;
  companyName?: string;
}

@Injectable({ providedIn: 'root' })
export class ErpExportService {
  private readonly toaster = inject(ToasterService);

  /**
   * Exports data to Microsoft Excel format using standard XML Spreadsheet 2003 workbook.
   * Compatible with Microsoft Excel, Google Sheets, Apple Numbers, and LibreOffice.
   */
  exportToExcel(columns: ErpExportColumn[], data: any[], options?: ErpExportOptions): void {
    const fileName = (options?.fileName || 'export') + '.xls';
    const sheetName = this.escapeXml(options?.sheetName || 'Data');
    const title = options?.title || 'Export';
    const company = options?.companyName || 'NexERP';

    const xml = `<?xml version="1.0" encoding="UTF-8"?>
<?mso-application progid="Excel.Sheet"?>
<Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet"
 xmlns:o="urn:schemas-microsoft-com:office:office"
 xmlns:x="urn:schemas-microsoft-com:office:excel"
 xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
 xmlns:html="http://www.w3.org/TR/REC-html40">
 <DocumentProperties xmlns="urn:schemas-microsoft-com:office:office">
  <Author>${this.escapeXml(company)}</Author>
  <Created>${new Date().toISOString()}</Created>
  <Company>${this.escapeXml(company)}</Company>
 </DocumentProperties>
 <Styles>
  <Style ss:ID="Default" ss:Name="Normal">
   <Alignment ss:Vertical="Center"/>
   <Borders/>
   <Font ss:FontName="Segoe UI" x:Family="Swiss" ss:Size="10" ss:Color="#1F2328"/>
   <Interior/>
   <NumberFormat/>
   <Protection/>
  </Style>
  <Style ss:ID="Header">
   <Alignment ss:Vertical="Center" ss:Horizontal="Center"/>
   <Borders>
    <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1" ss:Color="#CBD5E1"/>
    <Border ss:Position="Top" ss:LineStyle="Continuous" ss:Weight="1" ss:Color="#CBD5E1"/>
   </Borders>
   <Font ss:FontName="Segoe UI" x:Family="Swiss" ss:Size="10" ss:Color="#0F172A" ss:Bold="1"/>
   <Interior ss:Color="#F1F5F9" ss:Pattern="Solid"/>
  </Style>
  <Style ss:ID="Title">
   <Alignment ss:Vertical="Center" ss:Horizontal="Left"/>
   <Font ss:FontName="Segoe UI" x:Family="Swiss" ss:Size="14" ss:Color="#0F172A" ss:Bold="1"/>
  </Style>
  <Style ss:ID="Meta">
   <Alignment ss:Vertical="Center" ss:Horizontal="Left"/>
   <Font ss:FontName="Segoe UI" x:Family="Swiss" ss:Size="9" ss:Color="#64748B"/>
  </Style>
  <Style ss:ID="CellText">
   <Alignment ss:Vertical="Center" ss:Horizontal="Left"/>
   <Borders>
    <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1" ss:Color="#F1F5F9"/>
   </Borders>
  </Style>
  <Style ss:ID="CellNumber">
   <Alignment ss:Vertical="Center" ss:Horizontal="Right"/>
   <Borders>
    <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1" ss:Color="#F1F5F9"/>
   </Borders>
   <NumberFormat ss:Format="#,##0.00"/>
  </Style>
  <Style ss:ID="CellInteger">
   <Alignment ss:Vertical="Center" ss:Horizontal="Right"/>
   <Borders>
    <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1" ss:Color="#F1F5F9"/>
   </Borders>
   <NumberFormat ss:Format="#,##0"/>
  </Style>
  <Style ss:ID="CellCurrency">
   <Alignment ss:Vertical="Center" ss:Horizontal="Right"/>
   <Borders>
    <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1" ss:Color="#F1F5F9"/>
   </Borders>
   <NumberFormat ss:Format="#,##0.00"/>
  </Style>
  <Style ss:ID="CellDate">
   <Alignment ss:Vertical="Center" ss:Horizontal="Center"/>
   <Borders>
    <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1" ss:Color="#F1F5F9"/>
   </Borders>
   <NumberFormat ss:Format="yyyy-mm-dd"/>
  </Style>
  <Style ss:ID="CellCenter">
   <Alignment ss:Vertical="Center" ss:Horizontal="Center"/>
   <Borders>
    <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1" ss:Color="#F1F5F9"/>
   </Borders>
  </Style>
 </Styles>
 <Worksheet ss:Name="${sheetName}">
  <Table ss:DefaultRowHeight="20">
   ${columns.map(() => '<Column ss:AutoFitWidth="1" ss:Width="130"/>').join('\n   ')}
   <Row ss:Height="26">
    <Cell ss:MergeAcross="${Math.max(0, columns.length - 1)}" ss:StyleID="Title">
     <Data ss:Type="String">${this.escapeXml(title)}</Data>
    </Cell>
   </Row>
   <Row ss:Height="18">
    <Cell ss:MergeAcross="${Math.max(0, columns.length - 1)}" ss:StyleID="Meta">
     <Data ss:Type="String">Exported on: ${new Date().toLocaleString()} | Total Records: ${data.length}</Data>
    </Cell>
   </Row>
   <Row ss:Height="8"/>
   <Row ss:Height="24">
    ${columns.map(c => `<Cell ss:StyleID="Header"><Data ss:Type="String">${this.escapeXml(c.title)}</Data></Cell>`).join('')}
   </Row>
   ${data
     .map(row => {
       const cells = columns
         .map(col => {
           const raw = col.formatter ? col.formatter(row[col.field], row) : row[col.field];
           return this.buildExcelCell(raw, col);
         })
         .join('');
       return `<Row ss:Height="20">${cells}</Row>`;
     })
     .join('\n   ')}
  </Table>
 </Worksheet>
</Workbook>`;

    const blob = new Blob([xml], { type: 'application/vnd.ms-excel;charset=utf-8' });
    saveBlob(blob, fileName);
    this.toaster.success(`Exported ${data.length} records to Excel.`);
  }

  /**
   * Exports data as standard RFC 4180 CSV with UTF-8 BOM.
   */
  exportToCsv(columns: ErpExportColumn[], data: any[], options?: ErpExportOptions): void {
    const fileName = (options?.fileName || 'export') + '.csv';

    const headerRow = columns.map(c => this.escapeCsv(c.title)).join(',');
    const dataRows = data.map(row =>
      columns
        .map(col => {
          const val = col.formatter ? col.formatter(row[col.field], row) : row[col.field];
          return this.escapeCsv(this.formatCellValue(val, col));
        })
        .join(',')
    );

    const csvContent = '\uFEFF' + [headerRow, ...dataRows].join('\r\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8' });
    saveBlob(blob, fileName);
    this.toaster.success(`Exported ${data.length} records to CSV.`);
  }

  /**
   * Generates a clean, professional print/PDF layout.
   */
  print(columns: ErpExportColumn[], data: any[], options?: ErpExportOptions): void {
    const title = options?.title || 'Report';
    const company = options?.companyName || 'NexERP';
    const printWindow = window.open('', '_blank');

    if (!printWindow) {
      this.toaster.warn('Please allow popups to view the printable report.');
      return;
    }

    const rowsHtml = data
      .map(
        (row, idx) => `
      <tr class="${idx % 2 === 1 ? 'odd' : ''}">
        ${columns
          .map(col => {
            const val = col.formatter ? col.formatter(row[col.field], row) : row[col.field];
            const alignClass =
              col.type === 'currency' || col.type === 'number'
                ? 'text-end'
                : col.type === 'boolean' || col.type === 'date'
                ? 'text-center'
                : 'text-start';
            return `<td class="${alignClass}">${this.escapeHtml(this.formatCellValue(val, col))}</td>`;
          })
          .join('')}
      </tr>`
      )
      .join('');

    const html = `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>${this.escapeHtml(title)} - ${this.escapeHtml(company)}</title>
  <style>
    @page { size: A4 landscape; margin: 15mm 10mm 15mm 10mm; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
      color: #1e293b;
      margin: 0;
      padding: 24px;
      font-size: 12px;
      line-height: 1.4;
      background: #fff;
    }
    .report-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      border-bottom: 2px solid #0284c7;
      padding-bottom: 12px;
      margin-bottom: 16px;
    }
    .report-title h1 {
      margin: 0 0 4px 0;
      font-size: 20px;
      color: #0f172a;
      font-weight: 700;
    }
    .report-title .company {
      font-size: 13px;
      font-weight: 600;
      color: #475569;
    }
    .report-meta {
      text-align: right;
      font-size: 11px;
      color: #64748b;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      margin-bottom: 20px;
    }
    th {
      background-color: #f1f5f9;
      color: #334155;
      font-weight: 600;
      font-size: 11px;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      border-top: 1px solid #cbd5e1;
      border-bottom: 2px solid #94a3b8;
      padding: 8px 10px;
    }
    td {
      padding: 7px 10px;
      border-bottom: 1px solid #e2e8f0;
      color: #1e293b;
    }
    tr.odd {
      background-color: #f8fafc;
    }
    .text-start { text-align: left; }
    .text-end { text-align: right; }
    .text-center { text-align: center; }
    .footer-bar {
      margin-top: 24px;
      font-size: 10px;
      color: #94a3b8;
      text-align: center;
      border-top: 1px solid #e2e8f0;
      padding-top: 8px;
    }
    @media print {
      body { padding: 0; }
      .no-print { display: none; }
      th { background-color: #f1f5f9 !important; -webkit-print-color-adjust: exact; print-color-adjust: exact; }
      tr.odd { background-color: #f8fafc !important; -webkit-print-color-adjust: exact; print-color-adjust: exact; }
    }
  </style>
</head>
<body>
  <div class="report-header">
    <div class="report-title">
      <h1>${this.escapeHtml(title)}</h1>
      <div class="company">${this.escapeHtml(company)}</div>
    </div>
    <div class="report-meta">
      <div><strong>Date:</strong> ${new Date().toLocaleDateString()} ${new Date().toLocaleTimeString()}</div>
      <div><strong>Total Records:</strong> ${data.length}</div>
    </div>
  </div>

  <table>
    <thead>
      <tr>
        ${columns
          .map(col => {
            const alignClass =
              col.type === 'currency' || col.type === 'number'
                ? 'text-end'
                : col.type === 'boolean' || col.type === 'date'
                ? 'text-center'
                : 'text-start';
            return `<th class="${alignClass}">${this.escapeHtml(col.title)}</th>`;
          })
          .join('')}
      </tr>
    </thead>
    <tbody>
      ${rowsHtml}
    </tbody>
  </table>

  <div class="footer-bar">
    Generated by NexERP
  </div>

  <script>
    window.onload = function() {
      setTimeout(function() {
        window.print();
      }, 250);
    };
  </script>
</body>
</html>`;

    printWindow.document.open();
    printWindow.document.write(html);
    printWindow.document.close();
  }

  /**
   * Copies table content to clipboard as Tab-Separated Values (TSV).
   * Ready for direct pasting into Microsoft Excel or Google Sheets.
   */
  async copyToClipboard(columns: ErpExportColumn[], data: any[]): Promise<boolean> {
    try {
      const headerRow = columns.map(c => c.title).join('\t');
      const dataRows = data.map(row =>
        columns
          .map(col => {
            const val = col.formatter ? col.formatter(row[col.field], row) : row[col.field];
            return this.formatCellValue(val, col).replace(/\t|\r|\n/g, ' ');
          })
          .join('\t')
      );

      const tsv = [headerRow, ...dataRows].join('\r\n');
      await navigator.clipboard.writeText(tsv);
      this.toaster.success(`Copied ${data.length} rows to clipboard. Paste directly into Excel!`);
      return true;
    } catch {
      this.toaster.error('Failed to copy data to clipboard.');
      return false;
    }
  }

  private buildExcelCell(val: any, col: ErpExportColumn): string {
    if (val === null || val === undefined || val === '') {
      return '<Cell ss:StyleID="CellText"><Data ss:Type="String"></Data></Cell>';
    }

    if (col.type === 'currency' || col.type === 'number') {
      const num = Number(val);
      if (!isNaN(num)) {
        const style = col.type === 'currency' ? 'CellCurrency' : Number.isInteger(num) ? 'CellInteger' : 'CellNumber';
        return `<Cell ss:StyleID="${style}"><Data ss:Type="Number">${num}</Data></Cell>`;
      }
    }

    if (col.type === 'boolean') {
      const boolVal = Boolean(val) ? 'Yes' : 'No';
      return `<Cell ss:StyleID="CellCenter"><Data ss:Type="String">${boolVal}</Data></Cell>`;
    }

    if (col.type === 'date' && val) {
      const d = new Date(val);
      if (!isNaN(d.getTime())) {
        const dateStr = d.toISOString().substring(0, 10);
        return `<Cell ss:StyleID="CellDate"><Data ss:Type="String">${dateStr}</Data></Cell>`;
      }
    }

    return `<Cell ss:StyleID="CellText"><Data ss:Type="String">${this.escapeXml(String(val))}</Data></Cell>`;
  }

  private formatCellValue(val: any, col: ErpExportColumn): string {
    if (val === null || val === undefined) {
      return '';
    }
    if (col.type === 'boolean') {
      return val ? 'Yes' : 'No';
    }
    if (col.type === 'currency') {
      const num = Number(val);
      return !isNaN(num) ? num.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : String(val);
    }
    if (col.type === 'date' && val) {
      const d = new Date(val);
      return !isNaN(d.getTime()) ? d.toLocaleDateString() : String(val);
    }
    return String(val);
  }

  private escapeXml(str: string): string {
    return String(str || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&apos;');
  }

  private escapeCsv(str: string): string {
    const val = String(str ?? '');
    if (val.includes(',') || val.includes('"') || val.includes('\n') || val.includes('\r')) {
      return `"${val.replace(/"/g, '""')}"`;
    }
    return val;
  }

  private escapeHtml(str: string): string {
    return String(str || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');
  }
}
