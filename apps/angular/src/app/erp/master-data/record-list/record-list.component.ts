import { LocalizationService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, forkJoin } from 'rxjs';
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
  recordPermission,
  toRecordTableColumns,
} from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

const VIEW_MODE_KEY = 'nexerp_master_data_view_mode';

/**
 * The list page of a master-data table, driven by the route's
 * `entity`. The grid loads as the user scrolls and filters on the server; the user picks and
 * fixes columns. Card format shows the same rows, and the rows loaded
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
  isIndenting = false;

  /** The policy an action needs; one nobody holds when the table is read-only. */
  permission(action: 'Create' | 'Update' | 'Delete'): string {
    return recordPermission(this.entity, action);
  }

  ngOnInit(): void {
    const entityKey = this.resolveEntityKey();
    this.entity = this.registry.get(entityKey);
    // A smart button may open the list already searched, e.g. an employee's absences.
    this.initialSearch = this.route.snapshot.queryParamMap.get('filter') ?? '';

    this.columns = toRecordTableColumns(this.entity.columns);

    const baseActions: ErpTableAction[] = [
      {
        key: 'edit',
        title: 'Erp::Edit',
        icon: 'fas fa-pen',
        action: (row, event) => this.quickEdit(row, event),
      },
    ];

    if (this.entity.key === 'glAccount') {
      baseActions.push(
        {
          key: 'indent',
          title: 'Erp::Indent',
          icon: 'fas fa-indent',
          permission: this.permission('Update'),
          action: (row, event) => this.adjustIndent(row, 1, event),
        },
        {
          key: 'outdent',
          title: 'Erp::Outdent',
          icon: 'fas fa-outdent',
          permission: this.permission('Update'),
          disabled: row => !row['indentation'] || row['indentation'] <= 0,
          action: (row, event) => this.adjustIndent(row, -1, event),
        }
      );
    }

    baseActions.push({
      key: 'delete',
      title: 'Erp::Delete',
      icon: 'fas fa-trash',
      btnClass: 'btn-outline-danger',
      permission: this.permission('Delete'),
      action: (row, event) => this.remove(row, event),
    });

    this.actions = baseActions;
    this.source = query => this.entity.getList(query);

    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.table?.reload());
  }

  adjustIndent(row: Record<string, any>, delta: number, event?: Event): void {
    event?.stopPropagation();
    const current = Number(row['indentation']) || 0;
    const next = Math.max(0, current + delta);
    if (next === current) return;

    const input = { ...row, indentation: next };
    this.entity.update(row['id'], input).subscribe({
      next: () => {
        row['indentation'] = next;
        this.toaster.success('Erp::IndentationUpdated');
        this.table?.reload();
      },
      error: () => this.toaster.error('Erp::FailedToUpdateIndentation'),
    });
  }

  indentChartOfAccounts(): void {
    this.confirmation
      .warn('Erp::IndentChartOfAccountsConfirm', 'Erp::IndentChartOfAccounts')
      .subscribe((status: Confirmation.Status) => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }

        this.isIndenting = true;
        this.entity.getList({ skipCount: 0, maxResultCount: 1000, sorting: 'no asc' }).subscribe({
          next: result => {
            const items = [...(result.items || [])].sort((a, b) =>
              (a['no'] || '').localeCompare(b['no'] || '', undefined, { numeric: true })
            );

            let currentIndent = 0;
            const stack: any[] = [];
            const updates: Observable<any>[] = [];

            for (const acc of items) {
              let targetIndent = currentIndent;
              let autoTotaling = acc['totaling'];
              const type = Number(acc['accountType']);

              if (type === 3 /* BeginTotal */) {
                targetIndent = currentIndent;
                stack.push(acc);
                currentIndent++;
              } else if (type === 4 /* EndTotal */) {
                const beginAcc = stack.pop();
                currentIndent = Math.max(0, currentIndent - 1);
                targetIndent = currentIndent;
                if (!autoTotaling && beginAcc) {
                  autoTotaling = `${beginAcc['no']}..${acc['no']}`;
                }
              } else {
                targetIndent = currentIndent;
              }

              if (acc['indentation'] !== targetIndent || (autoTotaling && autoTotaling !== acc['totaling'])) {
                const input = {
                  ...acc,
                  indentation: targetIndent,
                  totaling: autoTotaling,
                };
                updates.push(this.entity.update(acc['id'], input));
              }
            }

            if (updates.length === 0) {
              this.isIndenting = false;
              this.toaster.info('Erp::ChartOfAccountsAlreadyIndented');
              return;
            }

            forkJoin(updates).subscribe({
              next: () => {
                this.isIndenting = false;
                this.toaster.success('Erp::ChartOfAccountsIndentedSuccess');
                this.table?.reload();
              },
              error: () => {
                this.isIndenting = false;
                this.toaster.error('Erp::FailedToUpdateIndentation');
              },
            });
          },
          error: () => {
            this.isIndenting = false;
            this.toaster.error('Erp::FailedToLoadAccounts');
          },
        });
      });
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
      formatter: (val, row) => {
        if (c.format) {
          return c.format(val, row);
        }
        if (c.type === 'select' || c.type === 'badge') {
          return this.localization.instant(this.optionLabel(row, c));
        }
        return val;
      },
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
    if (column.format) {
      return column.format(value, row);
    }
    return column.options?.find(o => o.value === value)?.label ?? String(value ?? '');
  }

  getBadgeClass(column: RecordColumn, row: Record<string, any>): string {
    if (typeof column.badgeClass === 'function') {
      return column.badgeClass(row);
    }
    return column.badgeClass || 'bg-light text-secondary border';
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

  private resolveEntityKey(): string {
    let curr: ActivatedRoute | null = this.route;
    while (curr) {
      const key = curr.snapshot?.data?.['entity'];
      if (key) {
        return key;
      }
      curr = curr.parent;
    }
    return '';
  }

  private readViewMode(): 'table' | 'card' {
    try {
      return localStorage.getItem(VIEW_MODE_KEY) === 'card' ? 'card' : 'table';
    } catch {
      return 'table';
    }
  }
}
