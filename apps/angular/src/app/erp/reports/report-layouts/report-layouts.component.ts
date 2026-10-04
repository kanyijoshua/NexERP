import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import {
  CreateUpdateReportLayoutDto,
  RdlcImportResultDto,
  ReportLayoutDetailDto,
  ReportLayoutDto,
  ReportLayoutService,
  ReportLayoutType,
  ReportNameDto,
} from '@proxy/reporting';
import { Observable, finalize, map } from 'rxjs';
import { CrudListBase, ErpTableColumn, saveBlob } from '../../erp-shared';

export type StudioViewMode = 'visual' | 'split' | 'code' | 'preview';
export type PaperFormat = 'a4-portrait' | 'a4-landscape' | 'letter' | 'receipt';

export interface ReportLayoutBlock {
  id: string;
  type: 'header' | 'kpi-summary' | 'table' | 'notes' | 'signatures' | 'footer' | 'custom';
  title: string;
  enabled: boolean;
  icon: string;
  expanded?: boolean;
  config: {
    // Header
    companyName?: string;
    title?: string;
    showPeriod?: boolean;
    headerAlignment?: 'left' | 'center' | 'split';
    headerSubtitle?: string;
    // KPI summary
    kpiCards?: Array<{ label: string; value: string; subtext?: string }>;
    // Table
    tableStriped?: boolean;
    tableBordered?: boolean;
    tableDense?: boolean;
    headerBgColor?: string;
    // Notes
    notesTitle?: string;
    notesContent?: string;
    notesCalloutType?: 'info' | 'warning' | 'neutral';
    // Signatures
    signatories?: Array<{ role: string; name: string }>;
    // Footer
    footerText?: string;
    showPrintedOn?: boolean;
    showPageNumber?: boolean;
    // Custom
    customHtml?: string;
  };
}

export interface PaletteItem {
  type: ReportLayoutBlock['type'];
  title: string;
  desc: string;
  icon: string;
}

@Component({
  selector: 'app-report-layouts',
  templateUrl: './report-layouts.component.html',
  styleUrls: ['./report-layouts.component.scss'],
  providers: [ListService],
  standalone: false,
})
export class ReportLayoutsComponent
  extends CrudListBase<ReportLayoutDto, CreateUpdateReportLayoutDto>
  implements OnInit
{
  private readonly sanitizer = inject(DomSanitizer);

  @ViewChild('codeTextarea') codeTextarea?: ElementRef<HTMLTextAreaElement>;
  @ViewChild('previewIframe') previewIframe?: ElementRef<HTMLIFrameElement>;

  readonly ReportLayoutType = ReportLayoutType;

  reportNames: ReportNameDto[] = [];
  reportFilter = '';

  readonly columns: ErpTableColumn<ReportLayoutDto>[] = [
    { field: 'reportID', labelKey: 'Erp::ReportID', width: 90 },
    { field: 'reportName', labelKey: 'Erp::ReportName', width: 200 },
    { field: 'code', labelKey: 'Erp::Code', width: 110 },
    { field: 'layoutName', labelKey: 'Erp::LayoutName', width: 180 },
    { field: 'fileExtension', labelKey: 'Erp::FileExtension', width: 100 },
    { field: 'description', labelKey: 'Erp::Description', width: 220 },
    { field: 'isDefault', labelKey: 'Erp::IsDefaultLayout', type: 'boolean', width: 110 },
    { field: 'builtIn', labelKey: 'Erp::BuiltIn', type: 'boolean', width: 90 },
  ];

  builtInTemplate = '';

  // Modal & Preview state
  isPreviewOpen = false;
  previewDoc: SafeHtml | null = null;
  livePreviewDoc: SafeHtml | null = null;

  // Studio Designer State
  activeViewMode: StudioViewMode = 'split';
  paperFormat: PaperFormat = 'a4-portrait';
  previewZoom = 100;
  sampleDataset: 'trialBalance' | 'balanceSheet' | 'agedReceivables' | 'incomeStatement' = 'trialBalance';

  themePrimaryColor = '#0f172a';
  themeFontFamily = 'Segoe UI, Helvetica, Arial, sans-serif';
  themeFontSize = '0.85rem';
  themeTableStyle: 'striped' | 'bordered' | 'minimal' = 'striped';

  readonly colorSwatches = [
    { name: 'Slate', hex: '#0f172a' },
    { name: 'Royal Blue', hex: '#0284c7' },
    { name: 'Emerald', hex: '#059669' },
    { name: 'Indigo', hex: '#4f46e5' },
    { name: 'Burgundy', hex: '#991b1b' },
    { name: 'Onyx', hex: '#18181b' },
  ];

  readonly availablePalettes: PaletteItem[] = [
    {
      type: 'header',
      title: 'Report Header',
      desc: 'Company name, document title, date range & rule',
      icon: 'fas fa-heading',
    },
    {
      type: 'kpi-summary',
      title: 'KPI Metric Cards',
      desc: 'Summary badges for total debits, credits, and balance',
      icon: 'fas fa-chart-line',
    },
    {
      type: 'table',
      title: 'Data Grid Table',
      desc: 'Columns and repeated lines with indentations',
      icon: 'fas fa-table',
    },
    {
      type: 'notes',
      title: 'Terms & Callout Box',
      desc: 'Important notices, audit remarks, payment terms',
      icon: 'fas fa-sticky-note',
    },
    {
      type: 'signatures',
      title: 'Signature Block',
      desc: 'Prepared by, Approved by sign-off boxes',
      icon: 'fas fa-signature',
    },
    {
      type: 'footer',
      title: 'Report Footer',
      desc: 'Printed timestamp, page number, disclaimer',
      icon: 'fas fa-shoe-prints',
    },
    {
      type: 'custom',
      title: 'Custom HTML',
      desc: 'Freeform HTML/CSS section',
      icon: 'fas fa-code',
    },
  ];

  readonly availablePlaceholders = [
    { tag: '{{CompanyName}}', desc: 'Current active company name' },
    { tag: '{{Title}}', desc: 'Report document title' },
    { tag: '{{FromDate}}', desc: 'Start period date' },
    { tag: '{{ToDate}}', desc: 'End period date' },
    { tag: '{{PrintedOn}}', desc: 'Formatted execution timestamp' },
    { tag: '{{#Columns}}<th>{{Header}}</th>{{/Columns}}', desc: 'Table column headers' },
    { tag: '{{#Rows}}<tr class="{{RowClass}}">{{#Cells}}<td>{{Value}}</td>{{/Cells}}</tr>{{/Rows}}', desc: 'Data rows and cells' },
  ];

  blocks: ReportLayoutBlock[] = [];

  // RDLC import
  isImportOpen = false;
  isImporting = false;
  importReportName = '';
  importLayoutName = '';
  importDescription = '';
  importSetAsDefault = false;
  importFile: File | null = null;
  importResult: RdlcImportResultDto | null = null;

  constructor(
    private readonly service: ReportLayoutService,
    private readonly fb: FormBuilder,
  ) {
    super();
  }

  override ngOnInit(): void {
    super.ngOnInit();

    this.service
      .getReportNames()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.reportNames = result.items ?? []));

    this.service
      .getBuiltInTemplate()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(template => {
        this.builtInTemplate = template;
      });
  }

  protected getList = (_query: ABP.PageQueryParams): Observable<PagedResultDto<ReportLayoutDto>> =>
    this.service
      .getList({ reportName: this.reportFilter || undefined })
      .pipe(map(result => ({ items: result.items ?? [], totalCount: result.items?.length ?? 0 })));

  protected create = (input: CreateUpdateReportLayoutDto) => this.service.create(input);
  protected update = (id: string, input: CreateUpdateReportLayoutDto) =>
    this.service.update(id, input);
  protected delete = (id: string) => this.service.delete(id);

  protected override load(id: string): Observable<ReportLayoutDto> {
    return this.service.get(id);
  }

  onReportFilterChange(): void {
    this.list.get();
  }

  openImport(): void {
    this.importReportName = this.reportFilter;
    this.importLayoutName = '';
    this.importDescription = '';
    this.importSetAsDefault = false;
    this.importFile = null;
    this.importResult = null;
    this.isImportOpen = true;
  }

  onImportFileChange(event: Event): void {
    this.importFile = (event.target as HTMLInputElement).files?.[0] ?? null;
    if (this.importFile && !this.importLayoutName) {
      // The layout name doubles as its code, which is at most 20 characters.
      this.importLayoutName = this.importFile.name.replace(/\.rdlc?$/i, '').slice(0, 20);
    }
  }

  get canImport(): boolean {
    return !!this.importReportName && !!this.importLayoutName.trim() && !!this.importFile;
  }

  importRdlc(): void {
    const file = this.importFile;
    if (!file || !this.canImport || this.isImporting) {
      return;
    }

    this.isImporting = true;
    const reader = new FileReader();
    reader.onerror = () => (this.isImporting = false);
    reader.onload = () => {
      // readAsDataURL gives "data:...;base64,<content>".
      const contentBase64 = String(reader.result).split(',')[1] ?? '';
      this.service
        .importRdlc({
          reportName: this.importReportName,
          layoutName: this.importLayoutName.trim(),
          description: this.importDescription.trim() || undefined,
          contentBase64,
          setAsDefault: this.importSetAsDefault,
        })
        .pipe(
          finalize(() => (this.isImporting = false)),
          takeUntilDestroyed(this.destroyRef),
        )
        .subscribe(result => {
          this.importResult = result;
          this.toaster.success(this.savedMessageKey);
          this.list.get();
        });
    };
    reader.readAsDataURL(file);
  }

  protected buildForm(item?: ReportLayoutDto): FormGroup {
    const detail = item as ReportLayoutDetailDto | undefined;
    const initialContent = detail?.templateContent ?? this.builtInTemplate;

    const group = this.fb.group({
      reportName: [
        { value: detail?.reportName ?? this.reportFilter, disabled: !!detail },
        Validators.required,
      ],
      reportID: [detail?.reportID ?? 0],
      code: [detail?.code ?? '', Validators.maxLength(20)],
      fileExtension: [detail?.fileExtension ?? 'html', Validators.maxLength(30)],
      layoutName: [detail?.layoutName ?? '', [Validators.required, Validators.maxLength(100)]],
      layoutType: [{ value: detail?.layoutType ?? ReportLayoutType.Html, disabled: !!detail }],
      description: [detail?.description ?? '', Validators.maxLength(250)],
      templateContent: [initialContent, Validators.required],
    });

    this.initBlocksFromTemplate(initialContent);

    // Watch template changes for live preview
    group.get('templateContent')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.renderLivePreview();
      });

    // Schedule initial live preview render
    setTimeout(() => this.renderLivePreview(), 50);

    return group;
  }

  // --- Drag and Drop Blocks Management ---
  initBlocksFromTemplate(html: string): void {
    // Initialize standard structured blocks
    this.blocks = [
      {
        id: 'b-header',
        type: 'header',
        title: 'Report Header',
        enabled: true,
        icon: 'fas fa-heading',
        expanded: false,
        config: {
          companyName: '{{CompanyName}}',
          title: '{{Title}}',
          showPeriod: true,
          headerAlignment: 'left',
          headerSubtitle: 'Financial & Operational Management Statement',
        },
      },
      {
        id: 'b-kpi',
        type: 'kpi-summary',
        title: 'Executive Summary Cards',
        enabled: true,
        icon: 'fas fa-chart-line',
        expanded: false,
        config: {
          kpiCards: [
            { label: 'Report Status', value: 'FINALIZED', subtext: 'Audited figures' },
            { label: 'Reporting Currency', value: 'USD ($)', subtext: 'Base ledger' },
            { label: 'Consolidation', value: 'CRONUS Group', subtext: 'Standard hierarchy' },
          ],
        },
      },
      {
        id: 'b-table',
        type: 'table',
        title: 'Primary Data Table',
        enabled: true,
        icon: 'fas fa-table',
        expanded: false,
        config: {
          tableStriped: true,
          tableBordered: true,
          tableDense: false,
          headerBgColor: this.themePrimaryColor,
        },
      },
      {
        id: 'b-notes',
        type: 'notes',
        title: 'Notes & Explanatory Remarks',
        enabled: true,
        icon: 'fas fa-sticky-note',
        expanded: false,
        config: {
          notesTitle: 'Basis of Preparation',
          notesContent: 'This statement is prepared under IFRS accounting standards. Figures reconcile directly with general ledger registers.',
          notesCalloutType: 'info',
        },
      },
      {
        id: 'b-signatures',
        type: 'signatures',
        title: 'Signatures & Authorization',
        enabled: true,
        icon: 'fas fa-signature',
        expanded: false,
        config: {
          signatories: [
            { role: 'Financial Controller', name: '______________________' },
            { role: 'Chief Financial Officer', name: '______________________' },
          ],
        },
      },
      {
        id: 'b-footer',
        type: 'footer',
        title: 'Document Footer',
        enabled: true,
        icon: 'fas fa-shoe-prints',
        expanded: false,
        config: {
          footerText: 'NexERP Enterprise Financials &bull; Confidential',
          showPrintedOn: true,
          showPageNumber: true,
        },
      },
    ];

    // If template has custom non-builtin text, preserve it
    if (html && !html.includes('{{#Columns}}')) {
      this.blocks.push({
        id: 'b-custom',
        type: 'custom',
        title: 'Custom HTML Content',
        enabled: true,
        icon: 'fas fa-code',
        expanded: true,
        config: { customHtml: html },
      });
    }
  }

  onDropBlock(event: CdkDragDrop<ReportLayoutBlock[]>): void {
    if (event.previousIndex === event.currentIndex) {
      return;
    }
    moveItemInArray(this.blocks, event.previousIndex, event.currentIndex);
    this.syncBlocksToTemplate();
  }

  addBlock(type: ReportLayoutBlock['type']): void {
    const id = `b-${Date.now()}`;
    const palette = this.availablePalettes.find(p => p.type === type);
    const newBlock: ReportLayoutBlock = {
      id,
      type,
      title: palette?.title ?? 'Report Section',
      enabled: true,
      icon: palette?.icon ?? 'fas fa-cube',
      expanded: true,
      config: {
        companyName: '{{CompanyName}}',
        title: '{{Title}}',
        showPeriod: true,
        tableStriped: true,
        notesTitle: 'Additional Information',
        notesContent: 'Custom section text and details.',
        footerText: 'NexERP Generated Document',
        customHtml: '<div class="alert alert-info p-3">Custom reporting section</div>',
      },
    };

    this.blocks.push(newBlock);
    this.syncBlocksToTemplate();
  }

  removeBlock(id: string): void {
    this.blocks = this.blocks.filter(b => b.id !== id);
    this.syncBlocksToTemplate();
  }

  toggleBlock(block: ReportLayoutBlock): void {
    block.enabled = !block.enabled;
    this.syncBlocksToTemplate();
  }

  moveBlock(index: number, direction: -1 | 1): void {
    const targetIndex = index + direction;
    if (targetIndex >= 0 && targetIndex < this.blocks.length) {
      moveItemInArray(this.blocks, index, targetIndex);
      this.syncBlocksToTemplate();
    }
  }

  toggleBlockExpand(block: ReportLayoutBlock): void {
    block.expanded = !block.expanded;
  }

  applyThemeColor(hex: string): void {
    this.themePrimaryColor = hex;
    const tableBlock = this.blocks.find(b => b.type === 'table');
    if (tableBlock) {
      tableBlock.config.headerBgColor = hex;
    }
    this.syncBlocksToTemplate();
  }

  syncBlocksToTemplate(): void {
    const generatedHtml = this.generateHtmlFromBlocks();
    this.form?.patchValue({ templateContent: generatedHtml }, { emitEvent: true });
    this.renderLivePreview();
  }

  generateHtmlFromBlocks(): string {
    const enabledBlocks = this.blocks.filter(b => b.enabled);

    // CSS Styling Header
    let styles = `
      body {
        font-family: ${this.themeFontFamily};
        font-size: ${this.themeFontSize};
        color: #1e293b;
        margin: 2rem;
        background: #ffffff;
      }
      .brand-accent { color: ${this.themePrimaryColor}; }
      header {
        border-bottom: 2px solid ${this.themePrimaryColor};
        padding-bottom: 0.75rem;
        margin-bottom: 1.25rem;
      }
      .company {
        font-size: 1.35rem;
        font-weight: 700;
        color: ${this.themePrimaryColor};
      }
      h1 {
        font-size: 1.15rem;
        font-weight: 700;
        margin: 0.35rem 0 0;
        color: #0f172a;
      }
      .period {
        color: #64748b;
        font-size: 0.85rem;
        margin-top: 0.25rem;
      }
      .kpi-grid {
        display: flex;
        gap: 1rem;
        margin-bottom: 1.5rem;
      }
      .kpi-card {
        flex: 1;
        background: #f8fafc;
        border: 1px solid #e2e8f0;
        border-radius: 6px;
        padding: 0.75rem 1rem;
        border-top: 3px solid ${this.themePrimaryColor};
      }
      .kpi-label { font-size: 0.75rem; text-transform: uppercase; color: #64748b; font-weight: 600; }
      .kpi-val { font-size: 1.1rem; font-weight: 700; color: #0f172a; margin-top: 2px; }
      .kpi-sub { font-size: 0.725rem; color: #94a3b8; }
      table {
        width: 100%;
        border-collapse: collapse;
        font-size: ${this.themeFontSize};
        margin-bottom: 1.5rem;
      }
      th {
        text-align: left;
        background: ${this.themePrimaryColor};
        color: #ffffff;
        padding: 0.5rem 0.65rem;
        white-space: nowrap;
        font-weight: 600;
      }
      td {
        padding: 0.4rem 0.65rem;
        border-bottom: 1px solid #e2e8f0;
      }
      td.number { text-align: right; font-variant-numeric: tabular-nums; }
      td.date { white-space: nowrap; }
      tr:nth-child(even) { background: ${this.themeTableStyle === 'striped' ? '#f8fafc' : 'transparent'}; }
      tr.total td {
        font-weight: 700;
        border-top: 2px solid ${this.themePrimaryColor};
        background: #f1f5f9;
        color: #0f172a;
      }
      tr.reversed td { font-style: italic; color: #64748b; }
      tr.indent-1 td:first-child { padding-left: 1.5rem; }
      tr.indent-2 td:first-child { padding-left: 3rem; }
      tr.indent-3 td:first-child { padding-left: 4.5rem; }
      .callout-box {
        background: #f8fafc;
        border-left: 4px solid ${this.themePrimaryColor};
        padding: 0.75rem 1rem;
        border-radius: 4px;
        margin-bottom: 1.5rem;
        font-size: 0.825rem;
      }
      .callout-title { font-weight: 600; margin-bottom: 0.25rem; color: #0f172a; }
      .signatures-grid {
        display: flex;
        justify-content: space-between;
        margin-top: 2rem;
        margin-bottom: 1.5rem;
      }
      .sig-box {
        width: 45%;
        border-top: 1px solid #94a3b8;
        padding-top: 0.5rem;
        text-align: center;
        font-size: 0.8rem;
        color: #475569;
      }
      footer {
        margin-top: 2rem;
        border-top: 1px solid #e2e8f0;
        padding-top: 0.75rem;
        color: #64748b;
        font-size: 0.75rem;
        display: flex;
        justify-content: space-between;
      }
      @media print {
        body { margin: 0; }
        footer { position: fixed; bottom: 0; left: 0; right: 0; }
      }
    `;

    let bodyHtml = '';

    for (const b of enabledBlocks) {
      if (b.type === 'header') {
        bodyHtml += `
<header style="text-align: ${b.config.headerAlignment || 'left'}">
  <div class="company">${b.config.companyName || '{{CompanyName}}'}</div>
  <h1>${b.config.title || '{{Title}}'}</h1>
  ${b.config.showPeriod ? '<div class="period">{{FromDate}} &ndash; {{ToDate}}</div>' : ''}
  ${b.config.headerSubtitle ? `<div style="font-size: 0.8rem; color: #94a3b8; margin-top: 2px;">${b.config.headerSubtitle}</div>` : ''}
</header>
`;
      } else if (b.type === 'kpi-summary') {
        const cards = b.config.kpiCards || [
          { label: 'Status', value: 'FINALIZED' },
          { label: 'Currency', value: 'USD ($)' },
        ];
        bodyHtml += `<div class="kpi-grid">\n`;
        for (const c of cards) {
          bodyHtml += `  <div class="kpi-card">\n    <div class="kpi-label">${c.label}</div>\n    <div class="kpi-val">${c.value}</div>\n    ${c.subtext ? `<div class="kpi-sub">${c.subtext}</div>` : ''}\n  </div>\n`;
        }
        bodyHtml += `</div>\n`;
      } else if (b.type === 'table') {
        bodyHtml += `
<table>
  <thead>
    <tr>{{#Columns}}<th>{{Header}}</th>{{/Columns}}</tr>
  </thead>
  <tbody>
    {{#Rows}}<tr class="{{RowClass}} indent-{{Indent}}">{{#Cells}}<td class="{{Kind}}">{{Value}}</td>{{/Cells}}</tr>
    {{/Rows}}
  </tbody>
</table>
`;
      } else if (b.type === 'notes') {
        bodyHtml += `
<div class="callout-box">
  <div class="callout-title">${b.config.notesTitle || 'Important Notes'}</div>
  <div>${b.config.notesContent || 'All amounts stated above are verified against ledger balances.'}</div>
</div>
`;
      } else if (b.type === 'signatures') {
        const sigs = b.config.signatories || [
          { role: 'Prepared By', name: '' },
          { role: 'Approved By', name: '' },
        ];
        bodyHtml += `<div class="signatures-grid">\n`;
        for (const s of sigs) {
          bodyHtml += `  <div class="sig-box">\n    <div><strong>${s.role}</strong></div>\n    <div style="margin-top: 1.5rem; font-style: italic;">${s.name || 'Authorized Signatory'}</div>\n  </div>\n`;
        }
        bodyHtml += `</div>\n`;
      } else if (b.type === 'footer') {
        bodyHtml += `
<footer>
  <div>${b.config.footerText || 'Generated by NexERP Financial Engine'}</div>
  ${b.config.showPrintedOn ? '<div>Printed {{PrintedOn}}</div>' : ''}
</footer>
`;
      } else if (b.type === 'custom') {
        bodyHtml += `\n${b.config.customHtml || ''}\n`;
      }
    }

    return `<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<title>{{Title}}</title>
<style>
${styles}
</style>
</head>
<body>
${bodyHtml}
</body>
</html>`;
  }

  // --- Live Interactive Preview Engine ---
  renderLivePreview(): void {
    const rawTemplate = this.template;
    if (!rawTemplate) {
      this.livePreviewDoc = null;
      return;
    }

    const sample = this.getSampleData(this.sampleDataset);
    const rendered = this.interpolateSample(rawTemplate, sample);
    this.livePreviewDoc = this.sanitizer.bypassSecurityTrustHtml(rendered);
  }

  getSampleData(dataset: string) {
    if (dataset === 'balanceSheet') {
      return {
        title: 'Balance Sheet',
        companyName: 'CRONUS International Ltd.',
        fromDate: '01/01/2026',
        toDate: '31/12/2026',
        printedOn: new Date().toLocaleString(),
        columns: [
          { header: 'Account / Section', kind: 'text' },
          { header: 'Current Period (USD)', kind: 'number' },
          { header: 'Prior Period (USD)', kind: 'number' },
        ],
        rows: [
          { rowClass: 'header', indent: 0, cells: [{ value: 'ASSETS', kind: 'text' }, { value: '', kind: 'number' }, { value: '', kind: 'number' }] },
          { rowClass: '', indent: 1, cells: [{ value: '1010 Cash & Equivalents', kind: 'text' }, { value: '$245,000.00', kind: 'number' }, { value: '$180,000.00', kind: 'number' }] },
          { rowClass: '', indent: 1, cells: [{ value: '1200 Accounts Receivable', kind: 'text' }, { value: '$128,450.00', kind: 'number' }, { value: '$95,200.00', kind: 'number' }] },
          { rowClass: '', indent: 1, cells: [{ value: '1300 Inventory', kind: 'text' }, { value: '$340,000.00', kind: 'number' }, { value: '$290,000.00', kind: 'number' }] },
          { rowClass: 'total', indent: 0, cells: [{ value: 'Total Assets', kind: 'text' }, { value: '$713,450.00', kind: 'number' }, { value: '$565,200.00', kind: 'number' }] },
          { rowClass: 'header', indent: 0, cells: [{ value: 'LIABILITIES & EQUITY', kind: 'text' }, { value: '', kind: 'number' }, { value: '', kind: 'number' }] },
          { rowClass: '', indent: 1, cells: [{ value: '2000 Accounts Payable', kind: 'text' }, { value: '$92,300.00', kind: 'number' }, { value: '$84,000.00', kind: 'number' }] },
          { rowClass: '', indent: 1, cells: [{ value: '3000 Retained Earnings', kind: 'text' }, { value: '$621,150.00', kind: 'number' }, { value: '$481,200.00', kind: 'number' }] },
          { rowClass: 'total', indent: 0, cells: [{ value: 'Total Liabilities & Equity', kind: 'text' }, { value: '$713,450.00', kind: 'number' }, { value: '$565,200.00', kind: 'number' }] },
        ],
      };
    }

    if (dataset === 'agedReceivables') {
      return {
        title: 'Aged Accounts Receivable Summary',
        companyName: 'CRONUS International Ltd.',
        fromDate: '01/10/2026',
        toDate: '03/10/2026',
        printedOn: new Date().toLocaleString(),
        columns: [
          { header: 'Customer No.', kind: 'text' },
          { header: 'Customer Name', kind: 'text' },
          { header: 'Current', kind: 'number' },
          { header: '31-60 Days', kind: 'number' },
          { header: '61-90 Days', kind: 'number' },
          { header: '90+ Days', kind: 'number' },
          { header: 'Total Balance', kind: 'number' },
        ],
        rows: [
          { rowClass: '', indent: 0, cells: [{ value: 'CUST-001', kind: 'text' }, { value: 'Acme Global Logistics', kind: 'text' }, { value: '$12,400.00', kind: 'number' }, { value: '$4,200.00', kind: 'number' }, { value: '$0.00', kind: 'number' }, { value: '$0.00', kind: 'number' }, { value: '$16,600.00', kind: 'number' }] },
          { rowClass: '', indent: 0, cells: [{ value: 'CUST-002', kind: 'text' }, { value: 'Contoso Cloud Services', kind: 'text' }, { value: '$28,900.00', kind: 'number' }, { value: '$1,150.00', kind: 'number' }, { value: '$850.00', kind: 'number' }, { value: '$0.00', kind: 'number' }, { value: '$30,900.00', kind: 'number' }] },
          { rowClass: '', indent: 0, cells: [{ value: 'CUST-003', kind: 'text' }, { value: 'Northwind Trading Co', kind: 'text' }, { value: '$6,500.00', kind: 'number' }, { value: '$3,200.00', kind: 'number' }, { value: '$2,100.00', kind: 'number' }, { value: '$1,800.00', kind: 'number' }, { value: '$13,600.00', kind: 'number' }] },
          { rowClass: 'total', indent: 0, cells: [{ value: '', kind: 'text' }, { value: 'Total Outstanding', kind: 'text' }, { value: '$47,800.00', kind: 'number' }, { value: '$8,550.00', kind: 'number' }, { value: '$2,950.00', kind: 'number' }, { value: '$1,800.00', kind: 'number' }, { value: '$61,100.00', kind: 'number' }] },
        ],
      };
    }

    if (dataset === 'incomeStatement') {
      return {
        title: 'Income Statement (Profit & Loss)',
        companyName: 'CRONUS International Ltd.',
        fromDate: '01/01/2026',
        toDate: '31/12/2026',
        printedOn: new Date().toLocaleString(),
        columns: [
          { header: 'Account / Description', kind: 'text' },
          { header: 'FY 2026 Actual', kind: 'number' },
          { header: 'Budget', kind: 'number' },
          { header: 'Variance', kind: 'number' },
        ],
        rows: [
          { rowClass: 'header', indent: 0, cells: [{ value: 'Operating Revenue', kind: 'text' }, { value: '$850,000.00', kind: 'number' }, { value: '$800,000.00', kind: 'number' }, { value: '+$50,000.00', kind: 'number' }] },
          { rowClass: '', indent: 1, cells: [{ value: '4010 Product Sales', kind: 'text' }, { value: '$620,000.00', kind: 'number' }, { value: '$600,000.00', kind: 'number' }, { value: '+$20,000.00', kind: 'number' }] },
          { rowClass: '', indent: 1, cells: [{ value: '4020 Consulting Services', kind: 'text' }, { value: '$230,000.00', kind: 'number' }, { value: '$200,000.00', kind: 'number' }, { value: '+$30,000.00', kind: 'number' }] },
          { rowClass: '', indent: 0, cells: [{ value: '5000 Cost of Goods Sold', kind: 'text' }, { value: '($420,000.00)', kind: 'number' }, { value: '($410,000.00)', kind: 'number' }, { value: '-$10,000.00', kind: 'number' }] },
          { rowClass: 'total', indent: 0, cells: [{ value: 'Gross Margin', kind: 'text' }, { value: '$430,000.00', kind: 'number' }, { value: '$390,000.00', kind: 'number' }, { value: '+$40,000.00', kind: 'number' }] },
          { rowClass: '', indent: 0, cells: [{ value: '6000 Operating Expenses', kind: 'text' }, { value: '($185,000.00)', kind: 'number' }, { value: '($190,000.00)', kind: 'number' }, { value: '+$5,000.00', kind: 'number' }] },
          { rowClass: 'total', indent: 0, cells: [{ value: 'Net Operating Income', kind: 'text' }, { value: '$245,000.00', kind: 'number' }, { value: '$200,000.00', kind: 'number' }, { value: '+$45,000.00', kind: 'number' }] },
        ],
      };
    }

    // Default: Trial Balance
    return {
      title: 'Trial Balance',
      companyName: 'CRONUS International Ltd.',
      fromDate: '01/01/2026',
      toDate: '31/12/2026',
      printedOn: new Date().toLocaleString(),
      columns: [
        { header: 'Account No.', kind: 'text' },
        { header: 'Account Name', kind: 'text' },
        { header: 'Debit (USD)', kind: 'number' },
        { header: 'Credit (USD)', kind: 'number' },
      ],
      rows: [
        { rowClass: '', indent: 1, cells: [{ value: '1010', kind: 'text' }, { value: 'Cash at Bank', kind: 'text' }, { value: '$12,500.00', kind: 'number' }, { value: '—', kind: 'number' }] },
        { rowClass: '', indent: 1, cells: [{ value: '1020', kind: 'text' }, { value: 'Petty Cash', kind: 'text' }, { value: '$500.00', kind: 'number' }, { value: '—', kind: 'number' }] },
        { rowClass: '', indent: 1, cells: [{ value: '1200', kind: 'text' }, { value: 'Accounts Receivable', kind: 'text' }, { value: '$45,820.00', kind: 'number' }, { value: '—', kind: 'number' }] },
        { rowClass: '', indent: 1, cells: [{ value: '1300', kind: 'text' }, { value: 'Inventory Finished Goods', kind: 'text' }, { value: '$128,400.00', kind: 'number' }, { value: '—', kind: 'number' }] },
        { rowClass: '', indent: 1, cells: [{ value: '2000', kind: 'text' }, { value: 'Accounts Payable', kind: 'text' }, { value: '—', kind: 'number' }, { value: '$34,250.00', kind: 'number' }] },
        { rowClass: '', indent: 1, cells: [{ value: '2100', kind: 'text' }, { value: 'VAT Output Tax', kind: 'text' }, { value: '—', kind: 'number' }, { value: '$9,650.00', kind: 'number' }] },
        { rowClass: '', indent: 1, cells: [{ value: '3000', kind: 'text' }, { value: 'Retained Earnings', kind: 'text' }, { value: '—', kind: 'number' }, { value: '$143,320.00', kind: 'number' }] },
        { rowClass: 'total', indent: 0, cells: [{ value: '', kind: 'text' }, { value: 'Grand Total', kind: 'text' }, { value: '$187,220.00', kind: 'number' }, { value: '$187,220.00', kind: 'number' }] },
      ],
    };
  }

  interpolateSample(template: string, sample: ReturnType<typeof this.getSampleData>): string {
    let out = template;

    // Root variables
    out = out.replace(/\{\{Title\}\}/g, sample.title);
    out = out.replace(/\{\{CompanyName\}\}/g, sample.companyName);
    out = out.replace(/\{\{FromDate\}\}/g, sample.fromDate);
    out = out.replace(/\{\{ToDate\}\}/g, sample.toDate);
    out = out.replace(/\{\{PrintedOn\}\}/g, sample.printedOn);

    // Columns loop
    const columnsRegex = /\{\{#Columns\}\}([\s\S]*?)\{\{\/Columns\}\}/;
    const colMatch = columnsRegex.exec(out);
    if (colMatch) {
      const colTpl = colMatch[1];
      const colsHtml = sample.columns.map(c =>
        colTpl.replace(/\{\{Header\}\}/g, c.header).replace(/\{\{Kind\}\}/g, c.kind)
      ).join('');
      out = out.replace(columnsRegex, colsHtml);
    }

    // Rows loop
    const rowsRegex = /\{\{#Rows\}\}([\s\S]*?)\{\{\/Rows\}\}/;
    const rowMatch = rowsRegex.exec(out);
    if (rowMatch) {
      const rowTpl = rowMatch[1];
      const rowsHtml = sample.rows.map(r => {
        let singleRow = rowTpl
          .replace(/\{\{RowClass\}\}/g, r.rowClass)
          .replace(/\{\{Indent\}\}/g, String(r.indent));

        // Cells loop inside row
        const cellsRegex = /\{\{#Cells\}\}([\s\S]*?)\{\{\/Cells\}\}/;
        const cellMatch = cellsRegex.exec(singleRow);
        if (cellMatch) {
          const cellTpl = cellMatch[1];
          const cellsHtml = r.cells.map(c =>
            cellTpl.replace(/\{\{Value\}\}/g, c.value).replace(/\{\{Kind\}\}/g, c.kind)
          ).join('');
          singleRow = singleRow.replace(cellsRegex, cellsHtml);
        }
        return singleRow;
      }).join('');
      out = out.replace(rowsRegex, rowsHtml);
    }

    return out;
  }

  // --- View Mode & Paper Controls ---
  setViewMode(mode: StudioViewMode): void {
    this.activeViewMode = mode;
    if (mode === 'preview' || mode === 'split') {
      this.renderLivePreview();
    }
  }

  setPaperFormat(format: PaperFormat): void {
    this.paperFormat = format;
  }

  setZoom(zoom: number): void {
    this.previewZoom = Math.min(150, Math.max(40, zoom));
  }

  setSampleDataset(dataset: typeof this.sampleDataset): void {
    this.sampleDataset = dataset;
    this.renderLivePreview();
  }

  printPreview(): void {
    const iframe = this.previewIframe?.nativeElement;
    if (iframe && iframe.contentWindow) {
      iframe.contentWindow.focus();
      iframe.contentWindow.print();
    }
  }

  insertPlaceholder(tag: string): void {
    const textarea = this.codeTextarea?.nativeElement;
    if (textarea) {
      const start = textarea.selectionStart;
      const end = textarea.selectionEnd;
      const val = textarea.value;
      const updated = val.substring(0, start) + tag + val.substring(end);
      this.form?.patchValue({ templateContent: updated });
      setTimeout(() => {
        textarea.selectionStart = textarea.selectionEnd = start + tag.length;
        textarea.focus();
      }, 0);
    } else {
      const current = this.template;
      this.form?.patchValue({ templateContent: current + '\n' + tag });
    }
    this.renderLivePreview();
  }

  resetToBuiltIn(): void {
    this.form?.patchValue({ templateContent: this.builtInTemplate });
    this.initBlocksFromTemplate(this.builtInTemplate);
    this.renderLivePreview();
  }

  download(): void {
    const name = (this.form?.getRawValue().layoutName as string) || 'layout';
    saveBlob(new Blob([this.template], { type: 'text/html' }), `${name}.html`);
  }

  upload(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    file.text().then(text => {
      this.form?.patchValue({ templateContent: text });
      this.initBlocksFromTemplate(text);
      this.renderLivePreview();
    });

    input.value = '';
  }

  preview(): void {
    if (!this.template) {
      return;
    }

    this.isBusy = true;
    this.service
      .runPreview({ templateContent: this.template })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: html => {
          this.isBusy = false;
          this.previewDoc = this.sanitizer.bypassSecurityTrustHtml(html);
          this.isPreviewOpen = true;
        },
        error: () => (this.isBusy = false),
      });
  }

  useThisLayout(layout: ReportLayoutDto): void {
    this.isBusy = true;
    this.service
      .setDefault({ reportName: layout.reportName!, layoutId: layout.id! })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isBusy = false;
          this.toaster.success(this.savedMessageKey);
          this.list.get();
        },
        error: () => (this.isBusy = false),
      });
  }

  displayNameOf(reportName?: string): string {
    return this.reportNames.find(r => r.name === reportName)?.displayName ?? reportName ?? '';
  }

  get template(): string {
    return (this.form?.getRawValue().templateContent as string) ?? '';
  }
}
