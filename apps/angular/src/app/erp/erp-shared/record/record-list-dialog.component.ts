import { ABP, ListService, PagedResultDto, PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, Injector, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { openRecordCardDialog } from './record-card-dialog.component';
import { RecordColumn, RecordEntity, recordPermission } from './record-entity';

export const RECORD_LIST_FILTER_DEBOUNCE_MS = 300;

/**
 * The full, paged, sortable list of a record table in a dialog: Odoo's "Search More..." and BC's
 * "Select from full list". Records can be opened, created, edited and deleted from it; with
 * `selectable` it closes with the picked record.
 */
@Component({
  selector: 'erp-record-list-dialog',
  templateUrl: './record-list-dialog.component.html',
  providers: [ListService],
})
export class RecordListDialogComponent implements OnInit {
  readonly modal = inject(NgbActiveModal);
  readonly list = inject<ListService<ABP.PageQueryParams>>(ListService);
  private readonly ngbModal = inject(NgbModal);
  private readonly injector = inject(Injector);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  entity!: RecordEntity;
  selectable = false;
  initialFilter = '';

  data: PagedResultDto<Record<string, any>> = { items: [], totalCount: 0 };
  selected: Record<string, any> | null = null;
  filter = '';

  canCreate = false;
  canDelete = false;

  private readonly filter$ = new Subject<string>();

  ngOnInit(): void {
    this.canCreate = this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Create'));
    this.canDelete = this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Delete'));

    this.filter = this.initialFilter;
    this.list.filter = this.initialFilter;
    this.list.maxResultCount = 10;

    this.list
      .hookToQuery(query => this.entity.getList(query as never))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.data = result;
        const current = this.selected?.['id'];
        this.selected = result.items?.find(r => r['id'] === current) ?? null;
      });

    this.filter$
      .pipe(
        debounceTime(RECORD_LIST_FILTER_DEBOUNCE_MS),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(value => {
        this.list.page = 0;
        this.list.filter = value;
      });
  }

  onFilterChange(value: string): void {
    this.filter = value ?? '';
    this.filter$.next(this.filter);
  }

  onActivate(event: { type: string; row: Record<string, any> }): void {
    if (event.type === 'click') {
      this.selected = event.row;
    } else if (event.type === 'dblclick') {
      if (this.selectable) {
        this.pick(event.row);
      } else {
        this.openCard(event.row);
      }
    }
  }

  pick(row: Record<string, any> | null = this.selected): void {
    if (row) {
      this.modal.close(row);
    }
  }

  openCard(row: Record<string, any>, event?: Event): void {
    event?.stopPropagation();
    this.card(row['id'], false).then(result => {
      if (result) {
        this.list.get();
      }
    });
  }

  /** A record created from the picker is the one wanted, so it is picked straight away (as Odoo does). */
  createNew(): void {
    this.card(null, true, this.filter).then(result => {
      if (!result) {
        return;
      }
      if (this.selectable) {
        this.pick(result.record);
      } else {
        this.list.get();
      }
    });
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
            this.list.get();
          });
      });
  }

  cellText(row: Record<string, any>, column: RecordColumn): unknown {
    const value = row[column.field];
    if (column.type === 'select') {
      return column.options?.find(o => o.value === value)?.label ?? value;
    }
    return value;
  }

  private card(id: string | null, quick: boolean, term = '') {
    return openRecordCardDialog(this.ngbModal, this.injector, this.entity, { id, quick, term });
  }
}
