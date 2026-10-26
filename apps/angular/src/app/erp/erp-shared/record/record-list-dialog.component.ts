import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, Injector, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ErpTableComponent } from '../table/erp-table.component';
import { ErpTableColumn, ErpTableSource } from '../table/erp-table.models';
import { openRecordCardDialog } from './record-card-dialog.component';
import { RecordEntity, RecordListAction, recordPermission, toRecordTableColumns } from './record-entity';

type Row = Record<string, any>;

/**
 * The full list of a record table in a dialog: "Search More..." and "Select from full
 * list". It is the same grid as the list pages (search, filter pane, columns, infinite scrolling).
 * Records can be opened, created, edited and deleted from it; with `selectable` it closes with
 * the picked record.
 */
@Component({
  selector: 'erp-record-list-dialog',
  templateUrl: './record-list-dialog.component.html',
  standalone: false,
})
export class RecordListDialogComponent implements OnInit {
  @ViewChild(ErpTableComponent) table?: ErpTableComponent<Row>;

  readonly modal = inject(NgbActiveModal);
  private readonly ngbModal = inject(NgbModal);
  private readonly injector = inject(Injector);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  entity!: RecordEntity;
  selectable = false;
  initialFilter = '';

  columns: ErpTableColumn[] = [];
  source!: ErpTableSource<Row>;
  loadedRows: Row[] = [];
  selected: Row | null = null;
  /** The grid's highlighted row, kept as one array so the grid is not re-bound on every check. */
  selection: Row[] = [];

  canCreate = false;
  canDelete = false;
  listActions: RecordListAction[] = [];
  runningAction = false;

  ngOnInit(): void {
    this.canCreate = this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Create'));
    this.canDelete = this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Delete'));
    this.listActions = (this.entity.listActions ?? []).filter(a => !a.permission || this.permissions.getGrantedPolicy(a.permission));
    this.columns = toRecordTableColumns(this.entity.columns);
    this.source = query => this.entity.getList(query);
  }

  setSelected(row: Row | null): void {
    this.selected = row;
    this.selection = row ? [row] : [];
  }

  /** A reload keeps the selection only if the record is still in the list. */
  onRowsChange(rows: Row[]): void {
    this.loadedRows = rows;
    const current = this.selected?.['id'];
    this.setSelected(rows.find(r => r['id'] === current) ?? null);
  }

  onDoubleClick(row: Row): void {
    if (this.selectable) {
      this.pick(row);
    } else {
      this.openCard(row);
    }
  }

  /** Enter in the search box picks the record when the search narrowed the list to one. */
  onEnter(event: Event): void {
    const target = event.target as HTMLElement | null;
    if (this.selectable && target?.matches('input[type="search"]') && this.loadedRows.length === 1) {
      this.pick(this.loadedRows[0]);
    }
  }

  pick(row: Row | null = this.selected): void {
    if (row) {
      this.modal.close(row);
    }
  }

  openCard(row: Row, event?: Event): void {
    event?.stopPropagation();
    this.card(row['id'], false).then(result => {
      if (result) {
        this.table?.reload();
      }
    });
  }

  /** A record created from the picker is the one wanted, so it is picked straight away. */
  createNew(): void {
    this.card(null, true, this.table?.searchTerm ?? '').then(result => {
      if (!result) {
        return;
      }
      if (this.selectable) {
        this.pick(result.record);
      } else {
        this.table?.reload();
      }
    });
  }

  remove(row: Row, event?: Event): void {
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

  /** A toolbar action of the table, e.g. "Suggest Allocation"; the list reloads after it. */
  runListAction(action: RecordListAction): void {
    const go = () => {
      this.runningAction = true;
      action.run().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.toaster.success('Erp::SavedSuccessfully');
          this.table?.reload();
        },
        error: () => (this.runningAction = false),
        complete: () => (this.runningAction = false),
      });
    };
    if (!action.confirmKey) {
      go();
      return;
    }
    this.confirmation
      .warn(action.confirmKey, action.labelKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          go();
        }
      });
  }

  private card(id: string | null, quick: boolean, term = '') {
    return openRecordCardDialog(this.ngbModal, this.injector, this.entity, { id, quick, term });
  }
}
