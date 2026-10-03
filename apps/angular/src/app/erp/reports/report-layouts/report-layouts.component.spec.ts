import { ListService } from '@abp/ng.core';
import { ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { ElementRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormBuilder } from '@angular/forms';
import { ReportLayoutBlock, ReportLayoutsComponent } from './report-layouts.component';
import { ReportLayoutService, ReportLayoutType } from '@proxy/reporting';
import { of } from 'rxjs';
import { CompanyService } from '../../services/company.service';

describe('ReportLayoutsComponent', () => {
  const builtIn = '<!DOCTYPE html><html><body>{{Title}}</body></html>';

  let service: jasmine.SpyObj<ReportLayoutService>;
  let component: ReportLayoutsComponent;

  beforeEach(() => {
    service = jasmine.createSpyObj<ReportLayoutService>('ReportLayoutService', [
      'getList',
      'get',
      'getReportNames',
      'getBuiltInTemplate',
      'create',
      'update',
      'delete',
      'setDefault',
      'runPreview',
    ]);

    service.getList.and.returnValue(of({ items: [] }) as never);
    service.getBuiltInTemplate.and.returnValue(of(builtIn) as never);
    service.getReportNames.and.returnValue(
      of({
        items: [
          { name: 'TrialBalance', displayName: 'Trial balance' },
          { name: 'AccountSchedule:BALANCE', displayName: 'Balance Sheet' },
        ],
      }) as never,
    );
    service.get.and.returnValue(
      of({
        id: 'l1',
        reportName: 'TrialBalance',
        layoutName: 'House Style',
        layoutType: ReportLayoutType.Html,
        templateContent: '<p>{{Title}}</p>',
      }) as never,
    );
    service.create.and.returnValue(of({ id: 'l1' }) as never);
    service.setDefault.and.returnValue(of(undefined) as never);
    service.runPreview.and.returnValue(of('<html>rendered</html>') as never);

    TestBed.configureTestingModule({
      providers: [
        ReportLayoutsComponent,
        ListService,
        FormBuilder,
        { provide: ReportLayoutService, useValue: service },
        { provide: CompanyService, useValue: { companyChanged$: of() } },
        { provide: ToasterService, useValue: jasmine.createSpyObj('ToasterService', ['success']) },
        {
          provide: ConfirmationService,
          useValue: jasmine.createSpyObj('ConfirmationService', ['warn']),
        },
      ],
    });

    component = TestBed.inject(ReportLayoutsComponent);
    component.ngOnInit();
  });

  /** A new layout opens on the built-in one, the way BC hands you a copy to edit. */
  it('starts a new layout from the built-in one', () => {
    component.openCreate();

    expect(component.form.value.templateContent).toBe(builtIn);
    expect(component.form.value.layoutType).toBe(ReportLayoutType.Html);
  });

  it('offers every report a layout can belong to', () => {
    expect(component.reportNames.length).toBe(2);
    expect(component.displayNameOf('AccountSchedule:BALANCE')).toBe('Balance Sheet');
    expect(component.displayNameOf('Unknown')).toBe('Unknown');
  });

  /** The list carries no bodies, so editing has to fetch the layout itself. */
  it('loads the body of the layout being edited', () => {
    component.openEdit({ id: 'l1' } as never);

    expect(service.get).toHaveBeenCalledWith('l1');
    expect(component.form.value.templateContent).toBe('<p>{{Title}}</p>');
  });

  /** The report and the format are what a layout is for, so they are fixed once it exists. */
  it('will not move an existing layout to another report', () => {
    component.openEdit({ id: 'l1' } as never);

    expect(component.form.get('reportName')?.disabled).toBeTrue();
    expect(component.form.get('layoutType')?.disabled).toBeTrue();
  });

  it('puts the built-in layout back when asked', () => {
    component.openCreate();
    component.form.patchValue({ templateContent: 'something else' });

    component.resetToBuiltIn();

    expect(component.form.value.templateContent).toBe(builtIn);
  });

  it('sends the edited layout to be previewed', () => {
    component.openCreate();
    component.form.patchValue({ templateContent: '<p>{{Title}}</p>' });

    component.preview();

    expect(service.runPreview).toHaveBeenCalledWith({ templateContent: '<p>{{Title}}</p>' });
    expect(component.isPreviewOpen).toBeTrue();
    expect(component.previewDoc).not.toBeNull();
  });

  it('does not preview an empty layout', () => {
    component.openCreate();
    component.form.patchValue({ templateContent: '' });

    component.preview();

    expect(service.runPreview).not.toHaveBeenCalled();
  });

  it('makes a layout the one the company uses', () => {
    component.useThisLayout({ id: 'l1', reportName: 'TrialBalance' } as never);

    expect(service.setDefault).toHaveBeenCalledWith({
      reportName: 'TrialBalance',
      layoutId: 'l1',
    });
  });

  /** The query the list runs, called directly: `list.get()` is ABP plumbing, not this page's logic. */
  const runQuery = () =>
    (component as unknown as { getList: (q: unknown) => { subscribe: (f: () => void) => void } })
      .getList({})
      .subscribe(() => undefined);

  it('narrows the list to one report', () => {
    component.reportFilter = 'TrialBalance';

    runQuery();

    expect(service.getList).toHaveBeenCalledWith({ reportName: 'TrialBalance' });
  });

  it('lists every report when no report is chosen', () => {
    component.reportFilter = '';

    runQuery();

    expect(service.getList).toHaveBeenCalledWith({ reportName: undefined });
  });

  // --- Drag and Drop Blocks & Studio Tests ---

  it('reorders blocks upon onDropBlock', () => {
    component.openCreate();
    const originalFirstId = component.blocks[0].id;
    const originalSecondId = component.blocks[1].id;

    const event = {
      previousIndex: 0,
      currentIndex: 1,
    } as CdkDragDrop<ReportLayoutBlock[]>;

    component.onDropBlock(event);

    expect(component.blocks[0].id).toBe(originalSecondId);
    expect(component.blocks[1].id).toBe(originalFirstId);
  });

  it('ignores onDropBlock if previousIndex equals currentIndex', () => {
    component.openCreate();
    const originalFirstId = component.blocks[0].id;

    const event = {
      previousIndex: 0,
      currentIndex: 0,
    } as CdkDragDrop<ReportLayoutBlock[]>;

    component.onDropBlock(event);

    expect(component.blocks[0].id).toBe(originalFirstId);
  });

  it('adds and removes blocks dynamically', () => {
    component.openCreate();
    const initialCount = component.blocks.length;

    component.addBlock('notes');
    expect(component.blocks.length).toBe(initialCount + 1);

    const added = component.blocks[component.blocks.length - 1];
    expect(added.type).toBe('notes');

    component.removeBlock(added.id);
    expect(component.blocks.length).toBe(initialCount);
  });

  it('toggles block enabled state', () => {
    component.openCreate();
    const block = component.blocks[0];
    const initialStatus = block.enabled;

    component.toggleBlock(block);
    expect(block.enabled).toBe(!initialStatus);

    component.toggleBlock(block);
    expect(block.enabled).toBe(initialStatus);
  });

  it('moves blocks with moveBlock helper', () => {
    component.openCreate();
    const firstId = component.blocks[0].id;
    const secondId = component.blocks[1].id;

    // Move first block down
    component.moveBlock(0, 1);
    expect(component.blocks[1].id).toBe(firstId);
    expect(component.blocks[0].id).toBe(secondId);

    // Out of bounds move is ignored
    component.moveBlock(0, -1);
    expect(component.blocks[0].id).toBe(secondId);
  });

  it('toggles block expanded state', () => {
    component.openCreate();
    const block = component.blocks[0];
    expect(block.expanded).toBeFalse();

    component.toggleBlockExpand(block);
    expect(block.expanded).toBeTrue();

    component.toggleBlockExpand(block);
    expect(block.expanded).toBeFalse();
  });

  it('applies theme primary color to blocks and regenerates html', () => {
    component.openCreate();
    component.applyThemeColor('#0284c7');

    expect(component.themePrimaryColor).toBe('#0284c7');
    const tableBlock = component.blocks.find(b => b.type === 'table');
    expect(tableBlock?.config.headerBgColor).toBe('#0284c7');
    expect(component.template).toContain('#0284c7');
  });

  it('switches view mode and updates preview', () => {
    component.openCreate();

    component.setViewMode('code');
    expect(component.activeViewMode).toBe('code');

    component.setViewMode('preview');
    expect(component.activeViewMode).toBe('preview');
    expect(component.livePreviewDoc).not.toBeNull();
  });

  it('changes paper format and clamps zoom scale', () => {
    component.setPaperFormat('a4-landscape');
    expect(component.paperFormat).toBe('a4-landscape');

    component.setZoom(120);
    expect(component.previewZoom).toBe(120);

    // Clamped between 40 and 150
    component.setZoom(200);
    expect(component.previewZoom).toBe(150);

    component.setZoom(10);
    expect(component.previewZoom).toBe(40);
  });

  it('switches sample dataset and regenerates live preview with financial figures', () => {
    component.openCreate();

    component.setSampleDataset('balanceSheet');
    expect(component.sampleDataset).toBe('balanceSheet');
    expect(component.livePreviewDoc).not.toBeNull();

    component.setSampleDataset('agedReceivables');
    expect(component.sampleDataset).toBe('agedReceivables');
    expect(component.livePreviewDoc).not.toBeNull();

    component.setSampleDataset('incomeStatement');
    expect(component.sampleDataset).toBe('incomeStatement');
    expect(component.livePreviewDoc).not.toBeNull();
  });

  it('inserts placeholder tags into code textarea or appends to template', () => {
    component.openCreate();

    // Without textarea element
    component.insertPlaceholder('{{CustomTag}}');
    expect(component.template).toContain('{{CustomTag}}');

    // With mock textarea element
    const textareaEl = document.createElement('textarea');
    textareaEl.value = 'Before After';
    textareaEl.selectionStart = 7;
    textareaEl.selectionEnd = 7;
    component.codeTextarea = new ElementRef(textareaEl);

    component.insertPlaceholder('[INSERTED]');
    expect(component.template).toContain('Before [INSERTED]After');
  });

  it('triggers print on preview iframe when nativeElement exists', () => {
    const mockWindow = {
      focus: jasmine.createSpy('focus'),
      print: jasmine.createSpy('print'),
    };
    const mockIframe = {
      contentWindow: mockWindow,
    } as unknown as HTMLIFrameElement;

    component.previewIframe = new ElementRef(mockIframe);
    component.printPreview();

    expect(mockWindow.focus).toHaveBeenCalled();
    expect(mockWindow.print).toHaveBeenCalled();
  });
});
