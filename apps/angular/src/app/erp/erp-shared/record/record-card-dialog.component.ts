import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, Injector, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormGroup } from '@angular/forms';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { finalize } from 'rxjs/operators';
import { RecordEntity, RecordEntityRegistry, RecordFact, recordPermission, recordToInput } from './record-entity';
import { buildRecordForm, patchRecordForm } from './record-form';

/** What a card dialog closed with. `null` when it was cancelled. */
export interface RecordCardResult<TDto = any> {
  record: TDto;
  /** The record was deleted from the dialog. */
  deleted?: boolean;
}

export interface OpenCardOptions {
  /** Existing record; a new one when omitted. */
  id?: string | null;
  /** Optional already-loaded record to render immediately while refreshing from the server. */
  record?: Record<string, any> | null;
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
  card.initialRecord = options.record ?? null;
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
  private readonly registry = inject(RecordEntityRegistry, { optional: true });
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  entity!: RecordEntity;
  id: string | null = null;
  initialRecord: Record<string, any> | null = null;
  term = '';
  quick = false;
  values: Record<string, unknown> = {};

  record: Record<string, any> | null = null;
  form: FormGroup | null = null;
  isBusy = false;
  canEdit = false;
  canDelete = false;

  heading = '';
  facts: RecordFact[] = [];
  cardLink: string[] | null = null;
  hasMoreFields = false;

  get isNew(): boolean {
    return !this.id;
  }

  ngOnInit(): void {
    this.canEdit = this.permissions.getGrantedPolicy(
      recordPermission(this.entity, this.isNew ? 'Create' : 'Update'),
    );
    this.canDelete =
      !this.isNew && this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Delete'));
    this.cardLink = this.id && this.entity.listRoute ? [...this.entity.listRoute, this.id] : null;
    this.hasMoreFields = this.quick && this.entity.fields.some(f => f.cardOnly);

    if (this.isNew) {
      this.show({ ...this.entity.newRecord(this.term || undefined), ...this.values });
      return;
    }

    const preview = this.initialRecord ?? this.registry?.getCachedRecord(this.entity.key, this.id);
    if (preview) {
      this.show(preview);
    } else {
      this.isBusy = true;
    }

    this.entity
      .get(this.id!)
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(record => {
        if (record) {
          this.registry?.cacheRecord(this.entity, record);
          this.show(record);
        }
      });
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
        this.registry?.cacheRecord(this.entity, record);
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
            this.registry?.invalidate(this.entity.key, id);
            this.toaster.success('Erp::DeletedSuccessfully');
            this.modal.close({ record, deleted: true });
          });
      });
  }

  showAllFields(): void {
    this.quick = false;
    this.hasMoreFields = false;
  }

  private show(record: Record<string, any>): void {
    this.record = record;
    const item = record && !this.isNew ? this.entity.toItem(record) : null;
    this.heading = item ? [item.code, item.name].filter(Boolean).join(' · ') : '';
    this.facts = record && !this.isNew && this.entity.facts ? this.entity.facts(record) : [];

    if (this.form && !this.form.dirty) {
      patchRecordForm(this.entity, this.form, record, {
        isNew: this.isNew,
        readonly: !this.canEdit,
      });
    } else if (!this.form) {
      this.form = buildRecordForm(this.entity, record, {
        isNew: this.isNew,
        readonly: !this.canEdit,
      });
    }
  }
}
