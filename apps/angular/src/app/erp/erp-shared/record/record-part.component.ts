import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, EventEmitter, Input, OnChanges, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, switchMap } from 'rxjs/operators';
import { EMPTY } from 'rxjs';
import { RecordDialogService } from './record-dialog.service';
import {
  RecordColumn,
  RecordEntity,
  RecordEntityRegistry,
  RecordPart,
  recordPermission,
} from './record-entity';

/**
 * One line table of a record's card (see `RecordPart`): the lines in a table with totals under it,
 * each opened in the line entity's card dialog, and New / Delete when the record allows changes.
 * Emits `changed` after any change, since the record's own totals usually follow its lines.
 */
@Component({
  selector: 'erp-record-part',
  templateUrl: './record-part.component.html',
  standalone: false,
})
export class RecordPartComponent implements OnChanges {
  private readonly registry = inject(RecordEntityRegistry);
  private readonly dialogs = inject(RecordDialogService);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  @Input({ required: true }) part!: RecordPart<any>;
  @Input({ required: true }) record!: Record<string, any>;
  @Output() readonly changed = new EventEmitter<void>();

  entity!: RecordEntity;
  columns: RecordColumn[] = [];
  lines: Record<string, any>[] = [];
  totals: Record<string, number> = {};
  loading = false;
  editable = false;
  canCreate = false;
  canDelete = false;

  /** The record whose lines are showing. */
  private loadedFor: string | null = null;

  ngOnChanges(): void {
    this.entity = this.registry.get(this.part.entity);

    const byField = new Map(this.entity.columns.map(c => [c.field, c]));
    this.columns = this.part.columns
      ? this.part.columns.map(f => byField.get(f)).filter((c): c is RecordColumn => !!c)
      : this.entity.columns;

    this.editable = !this.part.editable || this.part.editable(this.record);
    this.canCreate = this.editable && this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Create'));
    this.canDelete = this.editable && this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Delete'));

    // A re-read of the same record (its totals after a line changed, its status after an action)
    // leaves the lines as they are; another record brings its own.
    const key = `${this.part.entity}:${this.record['id']}`;
    if (key !== this.loadedFor) {
      this.loadedFor = key;
      this.load();
    }
  }

  isTotalled(field: string): boolean {
    return !!this.part.totals?.includes(field);
  }

  optionLabel(column: RecordColumn, value: unknown): string {
    return column.options?.find(o => o.value === value)?.label ?? String(value ?? '');
  }

  open(line: Record<string, any>): void {
    this.dialogs
      .openCard(this.entity, { id: line['id'], quick: false })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => result && this.afterChange());
  }

  add(): void {
    this.dialogs
      .openCard(this.entity, { values: this.part.newLine?.(this.record) ?? {} })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => result && this.afterChange());
  }

  remove(line: Record<string, any>, event: Event): void {
    event.stopPropagation();

    this.confirmation
      .warn('Erp::ItemWillBeDeletedMessage', 'Erp::AreYouSure')
      .pipe(
        switchMap(status => (status === Confirmation.Status.confirm ? this.entity.delete(line['id']) : EMPTY)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.toaster.success('Erp::DeletedSuccessfully');
        this.afterChange();
      });
  }

  private afterChange(): void {
    this.load();
    this.changed.emit();
  }

  private load(): void {
    this.loading = true;
    this.part
      .lines(this.record)
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(lines => {
        this.lines = lines;
        this.totals = Object.fromEntries(
          (this.part.totals ?? []).map(f => [f, lines.reduce((sum, l) => sum + (Number(l[f]) || 0), 0)]),
        );
      });
  }
}
