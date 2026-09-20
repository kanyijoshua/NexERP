import { TestBed } from '@angular/core/testing';
import { ExportFormat } from '@proxy/exporting';
import { AgedLedgerKind, ReportKind } from '@proxy/reporting';
import { of } from 'rxjs';
import { CompanyService } from '../../services/company.service';
import { ReportViewerComponent } from './report-viewer.component';

import {
  AccountScheduleService,
  ColumnLayoutService,
  FinancialReportService,
} from '@proxy/reporting';

describe('ReportViewerComponent', () => {
  const emptyList = { items: [] };

  const trialBalance = {
    title: 'Trial Balance',
    fromDate: '2026-01-01',
    toDate: '2026-12-31',
    columns: [
      { key: 'accountNo', header: 'Account', kind: 0 },
      { key: 'name', header: 'Name', kind: 0 },
      { key: 'netChange', header: 'Net Change', kind: 1 },
      { key: 'postingDate', header: 'Posting Date', kind: 2 },
    ],
    rows: [
      {
        values: { accountNo: '1010', name: 'Cash', netChange: 500 },
        bold: false,
        italic: false,
        indentation: 1,
        drillDownFilter: '1010',
      },
      {
        values: { accountNo: '', name: 'Total', netChange: 0 },
        bold: true,
        italic: false,
        indentation: 0,
        drillDownFilter: null,
      },
    ],
  };

  let reports: jasmine.SpyObj<FinancialReportService>;
  let component: ReportViewerComponent;

  beforeEach(() => {
    reports = jasmine.createSpyObj<FinancialReportService>('FinancialReportService', [
      'getTrialBalance',
      'getIncomeStatement',
      'getBalanceSheet',
      'getGLDetail',
      'getAgedAccounts',
      'runSchedule',
      'runExport',
    ]);

    reports.getTrialBalance.and.returnValue(of(trialBalance) as never);
    reports.getGLDetail.and.returnValue(of({ ...trialBalance, title: 'Detail' }) as never);
    reports.getAgedAccounts.and.returnValue(of(trialBalance) as never);
    reports.runSchedule.and.returnValue(of(trialBalance) as never);
    reports.runExport.and.returnValue(of(new Blob(['x'])) as never);

    TestBed.configureTestingModule({
      providers: [
        ReportViewerComponent,
        { provide: FinancialReportService, useValue: reports },
        {
          provide: AccountScheduleService,
          useValue: { getList: () => of({ items: [{ id: 'a', name: 'BALANCE' }] }) },
        },
        { provide: ColumnLayoutService, useValue: { getList: () => of(emptyList) } },
        { provide: CompanyService, useValue: { companyChanged$: of() } },
      ],
    });

    component = TestBed.inject(ReportViewerComponent);
  });

  it('flattens a report into rows the table can render', () => {
    component.ngOnInit();

    expect(component.title).toBe('Trial Balance');
    expect(component.columns.map(c => c.field)).toEqual([
      'accountNo',
      'name',
      'netChange',
      'postingDate',
    ]);
    expect(component.rows[0]['accountNo']).toBe('1010');
    expect(component.rows[0]['__indent']).toBe(1);
  });

  /** Column kinds decide how a cell is rendered, so they have to map across correctly. */
  it('maps column kinds onto cell types', () => {
    component.ngOnInit();

    expect(component.columns[0].type).toBe('text');
    expect(component.columns[2].type).toBe('currency');
    expect(component.columns[3].type).toBe('date');
  });

  it('marks total rows bold so they stand out', () => {
    component.ngOnInit();

    expect(component.rowClass(component.rows[0])['fw-bold']).toBeFalse();
    expect(component.rowClass(component.rows[1])['fw-bold']).toBeTrue();
  });

  it('runs the report the user picked', () => {
    component.report = ReportKind.AgedReceivables;
    component.run();

    expect(reports.getAgedAccounts).toHaveBeenCalled();
    const request = reports.getAgedAccounts.calls.mostRecent().args[0] as { kind: AgedLedgerKind };
    expect(request.kind).toBe(AgedLedgerKind.Receivables);
  });

  /** Drilling into a row is what makes a summary figure explainable. */
  it('drills down into the detail report filtered to the row', () => {
    component.ngOnInit();

    component.drillDown(component.rows[0]);

    expect(component.report).toBe(ReportKind.GeneralLedgerDetail);
    expect(component.accountFilter).toBe('1010');
    expect(reports.getGLDetail).toHaveBeenCalled();
  });

  it('does nothing when a row has nothing to drill into', () => {
    component.ngOnInit();
    reports.getGLDetail.calls.reset();

    component.drillDown(component.rows[1]);

    expect(reports.getGLDetail).not.toHaveBeenCalled();
  });

  it('will not run an account schedule before one is chosen', () => {
    component.report = ReportKind.AccountSchedule;
    component.scheduleName = '';

    expect(component.runDisabled).toBeTrue();

    component.run();
    expect(reports.runSchedule).not.toHaveBeenCalled();
  });

  it('exports the report with its own parameters', () => {
    component.report = ReportKind.TrialBalance;
    component.accountFilter = '1000..1999';

    component.exportReport(ExportFormat.Csv);

    const input = reports.runExport.calls.mostRecent().args[0] as {
      report: ReportKind;
      format: ExportFormat;
      accountFilter?: string;
    };
    expect(input.report).toBe(ReportKind.TrialBalance);
    expect(input.format).toBe(ExportFormat.Csv);
    expect(input.accountFilter).toBe('1000..1999');
  });

  it('only offers the account filter on the reports that read accounts', () => {
    component.report = ReportKind.TrialBalance;
    expect(component.usesAccountFilter).toBeTrue();

    component.report = ReportKind.AgedPayables;
    expect(component.usesAccountFilter).toBeFalse();
    expect(component.isAged).toBeTrue();
  });
});
