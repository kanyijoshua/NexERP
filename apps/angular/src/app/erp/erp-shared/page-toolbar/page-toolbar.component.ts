import { Component, EventEmitter, HostBinding, Input, Output } from '@angular/core';

/**
 * Title + search + "New" button strip used on top of every list page.
 *
 * ```html
 * <erp-page-toolbar title="Erp::Customers" [(filter)]="filter" permission="Erp.Customers.Create"
 *                   (create)="openCreate()">
 *   <button class="btn btn-sm btn-outline-secondary">extra action</button>
 * </erp-page-toolbar>
 * ```
 */
@Component({
  selector: 'erp-page-toolbar',
  templateUrl: './page-toolbar.component.html',
})
export class PageToolbarComponent {
  /** Removes the native tooltip that a static `title="..."` attribute would put on the host. */
  @HostBinding('attr.title') readonly hostTitle = null;

  /** Localization key (or plain text) of the page title. */
  @Input() title = '';
  /** Optional localization key (or plain text) shown above the title. */
  @Input() breadcrumb?: string;
  /** Optional FontAwesome class, e.g. `fas fa-users`. */
  @Input() icon?: string;

  @Input() filter = '';
  @Output() filterChange = new EventEmitter<string>();

  @Input() showSearch = true;
  @Input() showCreate = true;
  @Input() createLabelKey = 'Erp::New';
  /** Permission required to see the "New" button. Empty means always visible. */
  @Input() permission?: string;

  @Output() create = new EventEmitter<void>();

  onFilterChange(value: string): void {
    this.filter = value;
    this.filterChange.emit(value);
  }

  clearFilter(): void {
    this.onFilterChange('');
  }
}
