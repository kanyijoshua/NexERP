import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, EventEmitter, Input, OnChanges, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormControl, FormGroup } from '@angular/forms';
import { EMPTY, Observable } from 'rxjs';
import { finalize, switchMap } from 'rxjs/operators';
import { DocumentLineChange, DocumentLineColumn, DocumentLineColumnType } from '../models';
import { RecordDialogService } from './record-dialog.service';
import {
  RecordColumn,
  RecordEntity,
  RecordEntityRegistry,
  RecordField,
  RecordFieldType,
  RecordPart,
  recordPermission,
  recordToInput,
} from './record-entity';
import { buildRecordForm, editableFields } from './record-form';

/** What a line row is doing with the server. */
interface RowState {
  saving: boolean;
  /** Changed again while it was being saved: save once more when that save is back. */
  again: boolean;
}

/** The grid cell a record field becomes. */
const CELL_TYPES: Partial<Record<RecordFieldType, DocumentLineColumnType>> = {
  text: 'text',
  textarea: 'text',
  email: 'text',
  number: 'number',
  currency: 'currency',
  date: 'date',
  select: 'select',
  checkbox: 'checkbox',
  lookup: 'lookup',
};

/**
 * The lines of a document, under its header on the card (see `RecordPart`): an editable grid in
 * which each line is typed straight into its row, the way a document's lines subpage works.
 * <p>
 * The fields that tie a line to its header (whatever `newLine` sets, e.g. the document number) are
 * filled in from the header and not shown. A new line is saved as soon as every required field
 * has a value, and every change after that saves the line again; the server's answer is written
 * back into the row, so the amounts it works out (names, totals, net amounts) show at once.
 * `changed` is emitted after each save or delete, since the header's totals follow its lines.
 * </p>
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
  gridColumns: DocumentLineColumn[] = [];
  rows = new FormArray<FormGroup>([]);
  totals: { field: string; labelKey: string; value: number }[] = [];
  loading = false;
  /** The header allows its lines to change (e.g. the document is still open). */
  editable = false;
  canCreate = false;
  canUpdate = false;
  canDelete = false;

  /** The header's values every line carries, e.g. `{ documentNo: 'PC-00001' }`. */
  private fixed: Record<string, unknown> = {};
  private readonly states = new WeakMap<FormGroup, RowState>();
  /** The record whose lines are showing. */
  private loadedFor: string | null = null;

  get count(): number {
    return this.rows.length;
  }

  ngOnChanges(): void {
    this.entity = this.registry.get(this.part.entity);
    this.fixed = this.part.newLine?.(this.record) ?? {};

    this.editable = !this.entity.readOnly && (!this.part.editable || this.part.editable(this.record));
    this.canCreate = this.editable && this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Create'));
    this.canUpdate = this.editable && this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Update'));
    this.canDelete = this.editable && this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Delete'));
    this.gridColumns = this.buildColumns();

    // A re-read of the same record (its totals after a line changed, its status after an action)
    // keeps the lines as they are typed; another record brings its own.
    const key = `${this.part.entity}:${this.record['id']}`;
    if (key !== this.loadedFor) {
      this.loadedFor = key;
      this.load();
    } else {
      this.rows.controls.forEach(row => this.applyAccess(row));
    }
  }

  /** A new, empty line at the bottom; it is saved once its required fields are filled in. */
  addLine(): void {
    if (!this.canCreate) {
      return;
    }

    this.rows.push(this.rowOf({ ...(this.entity.newRecord() as Record<string, unknown>), ...this.fixed }));
  }

  removeLine(index: number): void {
    const row = this.rows.at(index);
    const id = row?.get('id')?.value as string | undefined;
    if (!id) {
      this.rows.removeAt(index);
      return;
    }

    this.confirmation
      .warn('Erp::ItemWillBeDeletedMessage', 'Erp::AreYouSure')
      .pipe(
        switchMap(status => (status === Confirmation.Status.confirm ? this.entity.delete(id) : EMPTY)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.rows.removeAt(this.rows.controls.indexOf(row));
        this.updateTotals();
        this.toaster.success('Erp::DeletedSuccessfully');
        this.changed.emit();
      });
  }

  /** Opens a saved line in the line entity's card, for the fields the grid does not show. */
  openLine(index: number): void {
    const id = this.rows.at(index)?.get('id')?.value as string | undefined;
    if (!id) {
      return;
    }

    this.dialogs
      .openCard(this.entity, { id, quick: false })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        if (result) {
          this.load();
          this.changed.emit();
        }
      });
  }

  onLineChange(change: DocumentLineChange): void {
    const row = this.rows.at(change.index);
    // A lookup writes its value after it reports the pick; save once it has.
    setTimeout(() => row && this.save(row));
  }

  private save(row: FormGroup): void {
    const state = this.stateOf(row);
    if (state.saving) {
      state.again = true;
      return;
    }

    const id = row.get('id')?.value as string | null;
    if ((id && !this.canUpdate) || (!id && !this.canCreate) || !this.isComplete(row)) {
      return;
    }

    const value: Record<string, unknown> = { ...row.getRawValue(), ...this.fixed };
    delete value['id'];
    const input = recordToInput(this.entity, value);
    const call: Observable<Record<string, any>> = id ? this.entity.update(id, input) : this.entity.create(input);

    state.saving = true;
    call
      .pipe(
        finalize(() => (state.saving = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(dto => {
        this.write(row, dto);
        this.updateTotals();
        this.changed.emit();

        if (state.again) {
          state.again = false;
          setTimeout(() => this.save(row));
        }
      });
  }

  /** Every required field of the line has a value, counting those the header fills in. */
  private isComplete(row: FormGroup): boolean {
    const value = { ...row.getRawValue(), ...this.fixed };
    return editableFields(this.entity)
      .filter(f => f.required)
      .every(f => value[f.field] !== null && value[f.field] !== undefined && value[f.field] !== '');
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
        this.rows.clear({ emitEvent: false });
        lines.forEach(line => this.rows.push(this.rowOf(line), { emitEvent: false }));
        this.updateTotals();
      });
  }

  /** A line as a form row: every field of the line entity, the shown read-only values and its id. */
  private rowOf(line: Record<string, any>): FormGroup {
    const isNew = !line['id'];
    const row = buildRecordForm(this.entity, line, { isNew });
    row.addControl('id', new FormControl(line['id'] ?? null));

    for (const column of this.gridColumns) {
      if (!row.contains(column.field)) {
        row.addControl(column.field, new FormControl({ value: line[column.field] ?? null, disabled: true }));
      }
    }

    this.applyAccess(row);
    return row;
  }

  /** A saved line is editable with the update permission, a new one with the create permission. */
  private applyAccess(row: FormGroup): void {
    const isNew = !row.get('id')?.value;
    const writable = isNew ? this.canCreate : this.canUpdate;
    const fields = new Map(this.entity.fields.map(f => [f.field, f]));

    for (const [name, control] of Object.entries(row.controls)) {
      const field = fields.get(name);
      const enable = writable && !!field && field.type !== 'readonly' && (isNew || !field.createOnly);
      if (enable && control.disabled) {
        control.enable({ emitEvent: false });
      } else if (!enable && control.enabled && name !== 'id') {
        control.disable({ emitEvent: false });
      }
    }
  }

  /** Writes what the server returned back into the row, without saving it again. */
  private write(row: FormGroup, dto: Record<string, any>): void {
    const wasNew = !row.get('id')?.value;
    const types = new Map(this.entity.fields.map(f => [f.field, f.type]));

    for (const [name, control] of Object.entries(row.controls)) {
      if (!(name in dto)) {
        continue;
      }

      const raw = dto[name];
      control.setValue(types.get(name) === 'date' && typeof raw === 'string' ? raw.substring(0, 10) : raw, { emitEvent: false });
    }

    row.markAsPristine();
    if (wasNew) {
      this.applyAccess(row);
    }
  }

  private updateTotals(): void {
    const labels = new Map(this.gridColumns.map(c => [c.field, c.labelKey]));
    this.totals = (this.part.totals ?? []).map(field => ({
      field,
      labelKey: labels.get(field) ?? field,
      value: this.rows.controls.reduce((sum, row) => sum + (Number(row.get(field)?.value) || 0), 0),
    }));
  }

  private stateOf(row: FormGroup): RowState {
    let state = this.states.get(row);
    if (!state) {
      state = { saving: false, again: false };
      this.states.set(row, state);
    }
    return state;
  }

  /**
   * The grid's columns: those the part names (or the line entity's list columns), then any
   * required field the user must fill in that is not among them. The fields the header fills in
   * are left out.
   */
  private buildColumns(): DocumentLineColumn[] {
    const hidden = new Set(Object.keys(this.fixed));
    const fields = new Map(this.entity.fields.map(f => [f.field, f]));
    const listColumns = new Map(this.entity.columns.map(c => [c.field, c]));

    const names = this.part.columns ?? this.entity.columns.map(c => c.field);
    const required = editableFields(this.entity)
      .filter(f => f.required && !names.includes(f.field))
      .map(f => f.field);

    return [...required, ...names]
      .filter(name => !hidden.has(name))
      .map(name => this.columnOf(name, fields.get(name), listColumns.get(name)))
      .filter((c): c is DocumentLineColumn => !!c);
  }

  private columnOf(name: string, field: RecordField | undefined, column: RecordColumn | undefined): DocumentLineColumn | null {
    if (!field && !column) {
      return null;
    }

    const labelKey = field?.labelKey ?? column!.labelKey;
    const options = field?.options ?? column?.options;
    const editableType = field && field.type !== 'readonly' ? CELL_TYPES[field.type] : undefined;

    if (editableType) {
      return {
        field: name,
        labelKey,
        type: editableType,
        options,
        lookupEntity: field!.lookupEntity,
        lookupValueField: field!.lookupValueField,
        readonly: !this.editable,
        width: column?.width ? `${column.width}px` : undefined,
      };
    }

    // Shown only: a value the server works out, in the type its list column has.
    const shownType: DocumentLineColumnType =
      column?.type === 'currency' || column?.type === 'number' || column?.type === 'date' || column?.type === 'select'
        ? column.type
        : column?.type === 'boolean'
          ? 'checkbox'
          : 'text';
    return { field: name, labelKey, type: shownType, options, readonly: true, width: column?.width ? `${column.width}px` : undefined };
  }
}
