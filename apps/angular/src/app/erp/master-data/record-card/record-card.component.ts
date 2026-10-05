import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, HostListener, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, finalize, switchMap } from 'rxjs/operators';
import {
  RecordAction,
  RecordEntity,
  RecordEntityRegistry,
  RecordFact,
  SmartButton,
  buildRecordForm,
  recordPermission,
  recordToInput,
} from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

/** The id segment of the card route that opens an empty card. */
export const NEW_RECORD_ID = 'new';

/**
 * The card page of a master-data record, driven by the route's
 * `entity`: FastTabs of fields, a FactBox of figures, actions (Block, Delete, New), smart buttons
 * to related documents and the record's chatter. `/new` opens an empty card.
 */
@Component({
  selector: 'app-record-card',
  templateUrl: './record-card.component.html',
  standalone: false,
})
export class RecordCardComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly registry = inject(RecordEntityRegistry);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly companyService = inject(CompanyService);
  private readonly destroyRef = inject(DestroyRef);

  entity!: RecordEntity;
  id: string | null = null;
  record: Record<string, any> | null = null;
  form: FormGroup | null = null;
  related: SmartButton[] = [];

  title = '';
  facts: RecordFact[] = [];
  actions: RecordAction<any>[] = [];

  isBusy = false;
  canEdit = false;
  canDelete = false;

  get createPermission(): string {
    return recordPermission(this.entity, 'Create');
  }

  get isNew(): boolean {
    return !this.id;
  }

  get listRoute(): string[] {
    return this.entity.listRoute ?? ['/erp'];
  }

  ngOnInit(): void {
    const entityKey = this.resolveEntityKey();
    if (!entityKey) {
      this.toaster.error('Erp::RecordNotFound');
      this.router.navigate(['/erp']);
      return;
    }
    this.entity = this.registry.get(entityKey);

    // The same component serves every id of the table, so it follows the route rather than reading it once.
    this.route.paramMap
      .pipe(
        switchMap(params => {
          const id = params.get('id');
          this.id = !id || id === NEW_RECORD_ID ? null : id;
          return this.load();
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(record => {
        if (record) {
          this.show(record);
        }
      });

    // A record belongs to one company; after switching there is nothing to show here.
    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.router.navigate(this.listRoute));
  }

  /** Ctrl+S saves. */
  @HostListener('document:keydown.control.s', ['$event'])
  onSaveShortcut(event: Event): void {
    event.preventDefault();
    this.save();
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
    const request$ = this.id ? this.entity.update(this.id, input) : this.entity.create(input);

    this.isBusy = true;
    request$
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(saved => {
        this.toaster.success('Erp::SavedSuccessfully');
        if (this.isNew) {
          // The URL now names the record, so a refresh reopens it.
          this.router.navigate([...this.listRoute, saved.id], { replaceUrl: true });
        } else {
          this.show(saved);
        }
      });
  }

  discard(): void {
    this.show(this.record ?? this.entity.newRecord());
  }

  newRecord(): void {
    this.router.navigate([...this.listRoute, NEW_RECORD_ID]);
  }

  remove(): void {
    const id = this.id;
    if (!id) {
      return;
    }
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
            this.router.navigate(this.listRoute);
          });
      });
  }

  run(action: RecordAction<any>): void {
    const record = this.record;
    if (!record || this.isBusy) {
      return;
    }
    const confirmed$: Observable<Confirmation.Status> = action.confirmKey
      ? this.confirmation.warn(action.confirmKey, 'Erp::AreYouSure')
      : of(Confirmation.Status.confirm);

    confirmed$
      .pipe(
        switchMap(status => {
          if (status !== Confirmation.Status.confirm) {
            return of(null);
          }
          this.isBusy = true;
          return action.run(record).pipe(
            switchMap(() => this.load()),
            finalize(() => (this.isBusy = false)),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(reloaded => {
        if (reloaded) {
          this.toaster.success('Erp::SavedSuccessfully');
          this.show(reloaded);
        }
      });
  }

  /**
   * Re-reads the record after one of its lines changed, for the totals. Unsaved edits on the
   * card are kept: only the figures beside them are refreshed then.
   */
  refreshRecord(): void {
    this.load()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(record => {
        if (!record) return;
        if (this.form?.dirty) {
          this.record = record;
          this.updateComputedProperties();
        } else {
          this.show(record);
        }
      });
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

  private updateComputedProperties(): void {
    if (!this.record || this.isNew) {
      this.title = '';
      this.facts = [];
      this.actions = [];
      return;
    }
    const item = this.entity.toItem(this.record);
    this.title = [item.code, item.name].filter(Boolean).join(' · ');
    this.facts = this.entity.facts ? this.entity.facts(this.record) : [];
    this.actions = (this.entity.actions ?? []).filter(a => !a.visible || a.visible(this.record));
  }

  private load(): Observable<Record<string, any> | null> {
    if (!this.id) {
      return of(this.entity.newRecord());
    }
    return this.entity.get(this.id).pipe(
      catchError(() => {
        this.toaster.error('Erp::RecordNotFound');
        this.router.navigate(this.listRoute);
        return of(null);
      }),
    );
  }

  private show(record: Record<string, any>): void {
    this.record = record;
    this.canEdit = this.permissions.getGrantedPolicy(
      recordPermission(this.entity, this.isNew ? 'Create' : 'Update'),
    );
    this.canDelete =
      !this.isNew && this.permissions.getGrantedPolicy(recordPermission(this.entity, 'Delete'));
    this.form = buildRecordForm(this.entity, record, {
      isNew: this.isNew,
      readonly: !this.canEdit,
    });
    this.updateComputedProperties();

    this.related = [];
    if (!this.isNew && this.entity.related) {
      this.entity
        .related(record)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(buttons => (this.related = buttons));
    }
  }
}
