import { Component, Input, TemplateRef } from '@angular/core';
import { ErpDynamicTableBase } from './erp-dynamic-table.base';
import { ErpDynamicTableColumn } from './erp-dynamic-table.models';

@Component({
  selector: 'erp-dynamic-table',
  templateUrl: './erp-dynamic-table.component.html',
  styleUrls: ['./erp-dynamic-table.component.scss'],
  standalone: false,
})
export class ErpDynamicTableComponent<T = any> extends ErpDynamicTableBase<T> {
  @Input() tableTitle?: string;
  @Input() subtitle?: string;
  @Input() customCellTemplate?: TemplateRef<any>;

  getBadgeClass(col: ErpDynamicTableColumn<T>, val: any, row: T): string {
    if (typeof col.badgeClass === 'function') {
      return col.badgeClass(val, row);
    }
    if (col.badgeClass) {
      return col.badgeClass;
    }
    // Auto-resolve common status strings
    const str = String(val).toLowerCase();
    if (str === 'active' || str === 'posted' || str === 'approved' || str === 'true' || str === 'paid') {
      return 'bg-success-subtle text-success border border-success-subtle';
    }
    if (str === 'pending' || str === 'open' || str === 'draft') {
      return 'bg-warning-subtle text-warning-emphasis border border-warning-subtle';
    }
    if (str === 'inactive' || str === 'cancelled' || str === 'rejected' || str === 'failed' || str === 'false') {
      return 'bg-danger-subtle text-danger border border-danger-subtle';
    }
    return 'bg-secondary-subtle text-secondary border border-secondary-subtle';
  }

  formatCurrency(val: any, currencyCode = 'USD'): string {
    if (val === null || val === undefined || isNaN(Number(val))) return '—';
    const num = Number(val);
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currencyCode,
      minimumFractionDigits: 2,
    }).format(num);
  }

  formatDate(val: any): string {
    if (!val) return '—';
    const d = new Date(val);
    if (isNaN(d.getTime())) return String(val);
    return d.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
