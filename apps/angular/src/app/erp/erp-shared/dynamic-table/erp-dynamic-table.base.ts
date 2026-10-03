import { Directive, EventEmitter, Input, Output } from '@angular/core';
import { ErpDynamicTableAction, ErpDynamicTableColumn } from './erp-dynamic-table.models';

/**
 * Base abstract class for dynamic ERP tables.
 * Provides client-side filtering, multi-column sorting,
 * selection tracking, pagination, and CSV export.
 *
 * Can be inherited by specialized grid components or used directly
 * via `<erp-dynamic-table>`.
 */
@Directive()
export abstract class ErpDynamicTableBase<T = any> {
  @Input() items: T[] = [];
  @Input() columns: ErpDynamicTableColumn<T>[] = [];
  @Input() actions: ErpDynamicTableAction<T>[] = [];
  @Input() selectable = false;
  @Input() searchable = true;
  @Input() paginated = true;
  @Input() pageSize = 10;
  @Input() pageSizeOptions = [5, 10, 25, 50];
  @Input() emptyMessage = 'No records found';
  @Input() emptyIcon = 'fas fa-inbox';
  @Input() loading = false;
  @Input() striped = true;
  @Input() bordered = true;
  @Input() hover = true;
  @Input() dense = false;

  @Output() rowClick = new EventEmitter<T>();
  @Output() selectionChange = new EventEmitter<T[]>();
  @Output() actionClick = new EventEmitter<{ action: ErpDynamicTableAction<T>; row: T }>();

  sortField: string | null = null;
  sortOrder: 'asc' | 'desc' = 'asc';
  searchTerm = '';
  currentPage = 1;
  selectedRows = new Set<T>();

  get visibleColumns(): ErpDynamicTableColumn<T>[] {
    return this.columns.filter(c => c.visible !== false);
  }

  get filteredAndSortedItems(): T[] {
    let result = [...(this.items || [])];

    // Filter
    if (this.searchTerm && this.searchTerm.trim()) {
      const q = this.searchTerm.trim().toLowerCase();
      result = result.filter(row => {
        return this.visibleColumns.some(col => {
          const val = this.getCellValue(row, col.field);
          if (val === null || val === undefined) return false;
          return String(val).toLowerCase().includes(q);
        });
      });
    }

    // Sort
    if (this.sortField) {
      const field = this.sortField;
      const order = this.sortOrder === 'asc' ? 1 : -1;
      result.sort((a, b) => {
        const valA = this.getCellValue(a, field);
        const valB = this.getCellValue(b, field);
        if (valA === valB) return 0;
        if (valA === null || valA === undefined) return 1;
        if (valB === null || valB === undefined) return -1;
        if (typeof valA === 'number' && typeof valB === 'number') {
          return (valA - valB) * order;
        }
        return String(valA).localeCompare(String(valB)) * order;
      });
    }

    return result;
  }

  get totalPages(): number {
    if (!this.paginated || this.pageSize <= 0) return 1;
    return Math.max(1, Math.ceil(this.filteredAndSortedItems.length / this.pageSize));
  }

  get paginatedItems(): T[] {
    if (!this.paginated) return this.filteredAndSortedItems;
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredAndSortedItems.slice(start, start + this.pageSize);
  }

  get startRecord(): number {
    if (this.filteredAndSortedItems.length === 0) return 0;
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get endRecord(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredAndSortedItems.length);
  }

  toggleSort(column: ErpDynamicTableColumn<T>): void {
    if (column.sortable === false) return;
    if (this.sortField === column.field) {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = column.field;
      this.sortOrder = 'asc';
    }
  }

  onFilterChange(query: string): void {
    this.searchTerm = query;
    this.currentPage = 1;
  }

  setPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  setPageSize(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
  }

  toggleSelectRow(row: T, event?: Event): void {
    event?.stopPropagation();
    if (this.selectedRows.has(row)) {
      this.selectedRows.delete(row);
    } else {
      this.selectedRows.add(row);
    }
    this.selectionChange.emit(Array.from(this.selectedRows));
  }

  toggleSelectAll(): void {
    const displayed = this.paginatedItems;
    if (this.isAllSelected()) {
      for (const row of displayed) {
        this.selectedRows.delete(row);
      }
    } else {
      for (const row of displayed) {
        this.selectedRows.add(row);
      }
    }
    this.selectionChange.emit(Array.from(this.selectedRows));
  }

  isRowSelected(row: T): boolean {
    return this.selectedRows.has(row);
  }

  isAllSelected(): boolean {
    const displayed = this.paginatedItems;
    return displayed.length > 0 && displayed.every(r => this.selectedRows.has(r));
  }

  isIndeterminate(): boolean {
    const displayed = this.paginatedItems;
    const count = displayed.filter(r => this.selectedRows.has(r)).length;
    return count > 0 && count < displayed.length;
  }

  getCellValue(row: T, field: string): any {
    if (!row || !field) return '';
    if (field.includes('.')) {
      return field.split('.').reduce((acc: any, part) => (acc ? acc[part] : undefined), row);
    }
    return (row as any)[field];
  }

  handleRowClick(row: T): void {
    this.rowClick.emit(row);
  }

  handleAction(action: ErpDynamicTableAction<T>, row: T, event?: MouseEvent): void {
    event?.stopPropagation();
    if (action.disabled && action.disabled(row)) return;
    action.action(row, event);
    this.actionClick.emit({ action, row });
  }

  toggleColumnVisibility(col: ErpDynamicTableColumn<T>): void {
    col.visible = col.visible === undefined ? false : !col.visible;
  }

  exportToCsv(filename = 'export.csv'): void {
    const cols = this.visibleColumns;
    const rows = this.filteredAndSortedItems;

    const headerLine = cols.map(c => `"${c.label.replace(/"/g, '""')}"`).join(',');
    const dataLines = rows.map(r => {
      return cols.map(c => {
        const val = this.getCellValue(r, c.field);
        const str = val !== null && val !== undefined ? String(val) : '';
        return `"${str.replace(/"/g, '""')}"`;
      }).join(',');
    });

    const csvContent = [headerLine, ...dataLines].join('\r\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.setAttribute('download', filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
