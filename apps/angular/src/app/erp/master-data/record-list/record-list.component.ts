import { LocalizationService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import {
  ErpExportColumn,
  ErpExportOptions,
  ErpExportService,
  ErpTableAction,
  ErpTableColumn,
  ErpTableComponent,
  ErpTableSource,
  RecordColumn,
  RecordDialogService,
  RecordEntity,
  RecordEntityRegistry,
  toRecordTableColumns,
} from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

const VIEW_MODE_KEY = 'nexerp_master_data_view_mode';

/**
 * The list page of a master-data table (BC list page, Odoo list view), driven by the route's
 * `entity`. The grid loads as the user scrolls and filters on the server; the user picks and
 * fixes columns. Card format (Odoo kanban / BC tiles) shows the same rows, and the rows loaded
 * can be exported to Excel, CSV, print or the clipboard.
 */
@Component({
  selector: 'app-record-list',
  templateUrl: './record-list.component.html',
  standalone: false,
})
export class RecordListComponent implements OnInit {
  @ViewChild(ErpTableComponent) table?: ErpTableComponent<Record<string, any>>;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly registry = inject(RecordEntityRegistry);
  private readonly dialogs = inject(RecordDialogService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly companyService = inject(CompanyService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly exportService = inject(ErpExportService);
  private readonly localization = inject(LocalizationService);

  entity!: RecordEntity;
  columns: ErpTableColumn[] = [];
  actions: ErpTableAction[] = [];
  source!: ErpTableSource<Record<string, any>>;
  initialSearch = '';

  /** The rows the table has loaded so far, for export and the empty-state check. */
  loadedRows: Record<string, any>[] = [];

  viewMode: 'table' | 'card' = this.readViewMode();

  ngOnInit(): void {
    this.entity = this.registry.get(this.route.snapshot.data['entity']);
    // A smart button may open the list already searched, e.g. an employee's absences.
    this.initialSearch = this.route.snapshot.queryParamMap.get('filter') ?? '';

    this.columns = toRecordTableColumns(this.entity.columns);
    this.actions = [
      {
        key: 'edit',
        title: 'Erp::Edit',
        icon: 'fas fa-pen',
        action: (row, event) => this.quickEdit(row, event),
      },
      {
        key: 'delete',
        title: 'Erp::Delete',
        icon: 'fas fa-trash',
        btnClass: 'btn-outline-danger',
        permission: this.entity.permission + '.Delete',
        action: (row, event) => this.remove(row, event),
      },
    ];
    this.source = query => this.entity.getList(query);

    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.table?.reload());
  }

  onViewModeChange(mode: 'table' | 'card'): void {
    this.viewMode = mode;
    try {
      localStorage.setItem(VIEW_MODE_KEY, mode);
    } catch {
      // Remembering the view is a convenience only.
    }
  }

  onExport(format: 'excel' | 'csv' | 'print' | 'clipboard'): void {
    if (!this.loadedRows.length) {
      this.toaster.info('Erp::Table:NothingToExport');
      return;
    }

    const exportColumns: ErpExportColumn[] = this.entity.columns.map(c => ({
      field: c.field,
      title: this.localization.instant(c.labelKey) || c.labelKey.replace(/^Erp::/, ''),
      type: c.type,
      formatter: (val, row) => (c.type === 'select' ? this.localization.instant(this.optionLabel(row, c)) : val),
    }));

    const options: ErpExportOptions = {
      fileName: `${this.entity.key}_${new Date().toISOString().substring(0, 10)}`,
      title: this.localization.instant(this.entity.pluralKey) || this.entity.pluralKey.replace(/^Erp::/, ''),
      sheetName: this.entity.key,
      companyName: this.companyService.getActiveCompanyName(),
    };

    switch (format) {
      case 'excel':
        this.exportService.exportToExcel(exportColumns, this.loadedRows, options);
        break;
      case 'csv':
        this.exportService.exportToCsv(exportColumns, this.loadedRows, options);
        break;
      case 'print':
        this.exportService.print(exportColumns, this.loadedRows, options);
        break;
      case 'clipboard':
        this.exportService.copyToClipboard(exportColumns, this.loadedRows);
        break;
    }
  }

  cardLink(row: Record<string, any>): string[] {
    return [...(this.entity.listRoute ?? []), row['id']];
  }

  openNew(): void {
    this.router.navigate([...(this.entity.listRoute ?? []), 'new']);
  }

  openCard(row: Record<string, any>): void {
    this.router.navigate(this.cardLink(row));
  }

  quickEdit(row: Record<string, any>, event?: Event): void {
    event?.stopPropagation();
    this.dialogs
      .openCard(this.entity, { id: row['id'], quick: false })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => result && this.table?.reload());
  }

  remove(row: Record<string, any>, event?: Event): void {
    event?.stopPropagation();
    this.confirmation
      .warn('Erp::ItemWillBeDeletedMessage', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }
        this.entity
          .delete(row['id'])
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe(() => {
            this.toaster.success('Erp::DeletedSuccessfully');
            this.table?.reload();
          });
      });
  }

  optionLabel(row: Record<string, any>, column: RecordColumn): string {
    const value = row[column.field];
    return column.options?.find(o => o.value === value)?.label ?? String(value ?? '');
  }

  getTitle(row: Record<string, any>): string {
    return row['name'] || row['description'] || row['displayName'] || row[this.entity.columns[1]?.field] || row['no'] || row['code'] || '—';
  }

  getCode(row: Record<string, any>): string {
    return row['no'] || row['code'] || row[this.entity.columns[0]?.field] || '';
  }

  getInitials(row: Record<string, any>): string {
    const title = this.getTitle(row);
    if (!title || title === '—') return '—';
    const words = title.trim().split(/\s+/);
    return words.length >= 2 ? (words[0][0] + words[1][0]).toUpperCase() : title.substring(0, 2).toUpperCase();
  }

  getAvatarColorClass(row: Record<string, any>): string {
    const str = this.getTitle(row) + this.getCode(row);
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    const colors = [
      'erp-avatar--teal',
      'erp-avatar--blue',
      'erp-avatar--purple',
      'erp-avatar--emerald',
      'erp-avatar--amber',
      'erp-avatar--rose',
      'erp-avatar--indigo',
    ];
    return colors[Math.abs(hash) % colors.length];
  }

  hasBlockedOrStatus(row: Record<string, any>): boolean {
    return !!row['blocked'];
  }

  /** Secondary columns for the card body; the code and title are already in its header. */
  getCardColumns(): RecordColumn[] {
    const shown = [this.entity.columns[0]?.field, this.entity.columns[1]?.field, 'name', 'no', 'code', 'description'];
    return this.entity.columns.filter(c => !shown.includes(c.field)).slice(0, 4);
  }

  private readViewMode(): 'table' | 'card' {
    try {
      return localStorage.getItem(VIEW_MODE_KEY) === 'card' ? 'card' : 'table';
    } catch {
      return 'table';
    }
  }
}
