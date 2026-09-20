import { ListService } from '@abp/ng.core';
import { ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { TestBed } from '@angular/core/testing';
import { FormBuilder } from '@angular/forms';
import { ReportLayoutService, ReportLayoutType } from '@proxy/reporting';
import { of } from 'rxjs';
import { CompanyService } from '../../services/company.service';
import { ReportLayoutsComponent } from './report-layouts.component';

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
});
