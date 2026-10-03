import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { ErpDynamicTableBase } from './erp-dynamic-table.base';
import { ErpDynamicTableComponent } from './erp-dynamic-table.component';
import { ErpDynamicTableAction, ErpDynamicTableColumn } from './erp-dynamic-table.models';

interface TestRecord {
  id: string;
  name: string;
  amount: number;
  status: string;
  active: boolean;
}

// Subclass proving inheritance from ErpDynamicTableBase
class CustomOrderLinesTable extends ErpDynamicTableBase<TestRecord> {
  constructor() {
    super();
    this.columns = [
      { field: 'name', label: 'Item Name' },
      { field: 'amount', label: 'Unit Price', type: 'currency' },
    ];
  }
}

describe('ErpDynamicTableComponent & ErpDynamicTableBase', () => {
  let fixture: ComponentFixture<ErpDynamicTableComponent<TestRecord>>;
  let component: ErpDynamicTableComponent<TestRecord>;

  const testColumns: ErpDynamicTableColumn<TestRecord>[] = [
    { field: 'name', label: 'Name', sortable: true },
    { field: 'amount', label: 'Amount', type: 'currency', currencyCode: 'USD', sortable: true },
    { field: 'status', label: 'Status', type: 'badge' },
    { field: 'active', label: 'Active', type: 'boolean' },
  ];

  const testData: TestRecord[] = [
    { id: '1', name: 'Widget Alpha', amount: 150.5, status: 'Active', active: true },
    { id: '2', name: 'Gadget Beta', amount: 89.0, status: 'Pending', active: true },
    { id: '3', name: 'Sprocket Gamma', amount: 240.0, status: 'Closed', active: false },
    { id: '4', name: 'Bolt Delta', amount: 12.25, status: 'Active', active: true },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ErpDynamicTableComponent],
      imports: [FormsModule, NgbDropdownModule],
    }).compileComponents();

    fixture = TestBed.createComponent(ErpDynamicTableComponent<TestRecord>);
    component = fixture.componentInstance;
    component.columns = [...testColumns];
    component.items = [...testData];
    component.pageSize = 2; // Test pagination with 2 items per page
    fixture.detectChanges();
  });

  it('allows subclasses to inherit from ErpDynamicTableBase', () => {
    const custom = new CustomOrderLinesTable();
    custom.items = [...testData];
    expect(custom.visibleColumns.length).toBe(2);
    expect(custom.filteredAndSortedItems.length).toBe(4);
  });

  it('filters rows based on search query', () => {
    component.onFilterChange('Beta');
    expect(component.filteredAndSortedItems.length).toBe(1);
    expect(component.filteredAndSortedItems[0].name).toBe('Gadget Beta');

    component.onFilterChange('');
    expect(component.filteredAndSortedItems.length).toBe(4);
  });

  it('sorts numeric and string columns ascending and descending', () => {
    // Sort by name asc
    component.toggleSort(testColumns[0]);
    expect(component.sortField).toBe('name');
    expect(component.sortOrder).toBe('asc');
    expect(component.filteredAndSortedItems[0].name).toBe('Bolt Delta');

    // Sort by name desc
    component.toggleSort(testColumns[0]);
    expect(component.sortOrder).toBe('desc');
    expect(component.filteredAndSortedItems[0].name).toBe('Widget Alpha');

    // Sort by amount asc
    component.toggleSort(testColumns[1]);
    expect(component.sortField).toBe('amount');
    expect(component.sortOrder).toBe('asc');
    expect(component.filteredAndSortedItems[0].amount).toBe(12.25);
  });

  it('paginates data properly', () => {
    expect(component.totalPages).toBe(2);
    expect(component.paginatedItems.length).toBe(2);
    expect(component.startRecord).toBe(1);
    expect(component.endRecord).toBe(2);

    // Page 2
    component.setPage(2);
    expect(component.currentPage).toBe(2);
    expect(component.paginatedItems.length).toBe(2);
    expect(component.startRecord).toBe(3);
    expect(component.endRecord).toBe(4);

    // Page size change
    component.setPageSize(5);
    expect(component.totalPages).toBe(1);
    expect(component.paginatedItems.length).toBe(4);
  });

  it('tracks row selection and select all', () => {
    component.selectable = true;
    spyOn(component.selectionChange, 'emit');

    const firstRow = component.paginatedItems[0];
    component.toggleSelectRow(firstRow);

    expect(component.isRowSelected(firstRow)).toBeTrue();
    expect(component.selectedRows.size).toBe(1);
    expect(component.isAllSelected()).toBeFalse();
    expect(component.isIndeterminate()).toBeTrue();

    // Select all on current page
    component.toggleSelectAll();
    expect(component.isAllSelected()).toBeTrue();
    expect(component.isIndeterminate()).toBeFalse();

    // Unselect all
    component.toggleSelectAll();
    expect(component.isAllSelected()).toBeFalse();
    expect(component.selectedRows.size).toBe(0);
  });

  it('handles row actions correctly', () => {
    const actionSpy = jasmine.createSpy('action');
    const testAction: ErpDynamicTableAction<TestRecord> = {
      key: 'edit',
      label: 'Edit',
      action: actionSpy,
    };
    component.actions = [testAction];

    spyOn(component.actionClick, 'emit');
    const row = testData[0];
    component.handleAction(testAction, row);

    expect(actionSpy).toHaveBeenCalledWith(row, undefined);
    expect(component.actionClick.emit).toHaveBeenCalledWith({ action: testAction, row });
  });

  it('formats currency and dates correctly', () => {
    expect(component.formatCurrency(1500.5, 'USD')).toContain('1,500.50');
    expect(component.formatCurrency(null)).toBe('—');

    const formattedDate = component.formatDate('2026-05-15T10:00:00Z');
    expect(formattedDate).toContain('2026');
    expect(component.formatDate(null)).toBe('—');
  });

  it('toggles column visibility', () => {
    const col = component.columns[0];
    expect(component.visibleColumns.length).toBe(4);

    component.toggleColumnVisibility(col);
    expect(component.visibleColumns.length).toBe(3);

    component.toggleColumnVisibility(col);
    expect(component.visibleColumns.length).toBe(4);
  });

  it('generates CSV export without errors', () => {
    spyOn(document.body, 'appendChild');
    spyOn(document.body, 'removeChild');

    expect(() => component.exportToCsv('test.csv')).not.toThrow();
  });
});
