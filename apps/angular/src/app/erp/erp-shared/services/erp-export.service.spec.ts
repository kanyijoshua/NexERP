import { TestBed } from '@angular/core/testing';
import { ToasterService } from '@abp/ng.theme.shared';
import { ErpExportColumn, ErpExportService } from './erp-export.service';

describe('ErpExportService', () => {
  let service: ErpExportService;
  let toasterSpy: jasmine.SpyObj<ToasterService>;

  beforeEach(() => {
    toasterSpy = jasmine.createSpyObj('ToasterService', ['success', 'warn', 'error', 'info']);

    TestBed.configureTestingModule({
      providers: [
        ErpExportService,
        { provide: ToasterService, useValue: toasterSpy },
      ],
    });

    service = TestBed.inject(ErpExportService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  const testColumns: ErpExportColumn[] = [
    { field: 'no', title: 'Customer No' },
    { field: 'name', title: 'Customer Name' },
    { field: 'balance', title: 'Balance', type: 'currency' },
    { field: 'blocked', title: 'Blocked', type: 'boolean' },
  ];

  const testData = [
    { no: 'CUST-001', name: 'Acme Corp, Inc.', balance: 1250.5, blocked: false },
    { no: 'CUST-002', name: 'Contoso Ltd.', balance: 3400.0, blocked: true },
  ];

  it('exportToExcel should generate XML spreadsheet and show success toast', () => {
    service.exportToExcel(testColumns, testData, { fileName: 'customers_test', title: 'Customers' });
    expect(toasterSpy.success).toHaveBeenCalledWith(jasmine.stringMatching(/Exported 2 records to Excel/));
  });

  it('exportToCsv should generate valid CSV content with UTF-8 BOM and show success toast', () => {
    service.exportToCsv(testColumns, testData, { fileName: 'customers_test' });
    expect(toasterSpy.success).toHaveBeenCalledWith(jasmine.stringMatching(/Exported 2 records to CSV/));
  });

  it('copyToClipboard should format TSV and attempt to write to navigator clipboard', async () => {
    spyOn(navigator.clipboard, 'writeText').and.returnValue(Promise.resolve());
    const result = await service.copyToClipboard(testColumns, testData);
    expect(result).toBeTrue();
    expect(navigator.clipboard.writeText).toHaveBeenCalled();
    expect(toasterSpy.success).toHaveBeenCalledWith(jasmine.stringMatching(/Copied 2 rows to clipboard/));
  });
});
