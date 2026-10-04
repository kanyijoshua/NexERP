import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, Injector, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormGroup } from '@angular/forms';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { finalize } from 'rxjs/operators';
import { RecordEntity, RecordFact, recordPermission, recordToInput } from './record-entity';
import { buildRecordForm } from './record-form';

/** What a card dialog closed with. `null` when it was cancelled. */
export interface RecordCardResult<TDto = any> {
  record: TDto;
  /** The record was deleted from the dialog. */
  deleted?: boolean;
}

export interface OpenCardOptions {
  /** Existing record; a new one when omitted. */
  id?: string | null;
  /** What the user typed in the lookup, handed to `entity.newRecord`. */
  term?: string;
  /** The quick form: only the essential fields. Defaults to `true` for a new record. */
  quick?: boolean;
  /** Values set on a new record over `entity.newRecord`, e.g. the document a line belongs to. */
  values?: Record<string, unknown>;
}

/** Opens a record card dialog; resolves with `null` when it is cancelled. */
export function openRecordCardDialog<TDto = any>(
  modal: NgbModal,
  injector: Injector,
  entity: RecordEntity,
  options: OpenCardOptions = {},
): Promise<RecordCardResult<TDto> | null> {
  const ref = modal.open(RecordCardDialogComponent, {
    size: 'lg',
    scrollable: true,
    backdrop: 'static',
    injector,
  });
  const card = ref.componentInstance as RecordCardDialogComponent;
  card.entity = entity;
  card.id = options.id ?? null;
  card.term = options.term ?? '';
  card.quick = options.quick ?? !options.id;
  card.values = options.values ?? {};
  // A dismissed dialog rejects its promise; to callers that is simply "nothing".
  return (ref.result as Promise<RecordCardResult<TDto>>).then(
    result => result ?? null,
    () => null,
  );
}

/**
 * A record in a dialog: the "Create and edit..." / internal-link form, the card opened from a
 * lookup. Opened through `RecordDialogService.openCard`; closes with `{ record }` after a save and
 * `{ record, deleted: true }` after a delete.
 */
@Component({
    selector: 'erp-record-card-dialog',
    templateUrl: './record-card-dialog.component.html',
    standalone: false
})
export class RecordCardDialogComponent implements OnInit {
  readonly modal = inject(NgbActiveModal);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  entity!: RecordEntity;
  id: string | null = null;
  term = '';
  quick = false;
  values: Record<string, unknown> = {};

  record: Record<string, any> | null = null;
  form: FormGroup | null = null;
  isBusy = false;
  canEdit = false;
  canDelete = false;

  get isNew(): boolean {
    return !this.id;
  }

  /** "More fields" only makes sense when the quick form hides some. */
  get hasMoreFields(): boolean {
    return this.quick && this.entity.fields.some(f => f.cardOnly);
  }

  get heading(): string {
    const item = this.record && !this.isNew ? this.entity.toItem(this.record) : null;
    return item ? [item.code, item.name].filter(Boolean).join(' · ') : '';
  }

  get facts(): RecordFact[] {
    return this.record && !this.isNew && this.entity.facts ? this.entity.facts(this.record) : [];
  }

  get cardLink(): string[] | null {
    return this.id && this.entity.listRoute ? [...this.entity.listRoute, this.id] : null;
  }

  ngOnInit(): void {
    this.canEdit = this.permissions.getGrantedPolicy(
      recordPermission(this.entity, this.isNew ? 'Create' : 'Update'),
    );
    this.canDelete =
      !this.isNew && this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Delete'));

    if (this.isNew) {
      this.show({ ...this.entity.newRecord(this.term || undefined), ...this.values });
      return;
    }

    this.isBusy = true;
    this.entity
      .get(this.id!)
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(record => this.show(record));
  }

  save(): void {
    if (!this.form || this.isBusy || !this.canEdit) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const input = recordToInput(this.entity, this.form.getRawValue());
    const request$ = this.id
      ? this.entity.update(this.id, input)
      : this.entity.create(input);

    this.isBusy = true;
    request$
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(record => {
        this.toaster.success('Erp::SavedSuccessfully');
        this.modal.close({ record });
      });
  }

  remove(): void {
    if (!this.id || !this.record) {
      return;
    }
    const id = this.id;
    const record = this.record;
    this.confirmation
      .warn('Erp::ItemWillBeDeletedMessage', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }
        this.isBusy = true;
        this.entity
          .delete(id)
          .pipe(
            finalize(() => (this.isBusy = false)),
            takeUntilDestroyed(this.destroyRef),
          )
          .subscribe(() => {
            this.toaster.success('Erp::DeletedSuccessfully');
            this.modal.close({ record, deleted: true });
          });
      });
  }

  showAllFields(): void {
    this.quick = false;
  }

  private show(record: Record<string, any>): void {
    this.record = record;
    this.form = buildRecordForm(this.entity, record, {
      isNew: this.isNew,
      readonly: !this.canEdit,
    });
  }
}
