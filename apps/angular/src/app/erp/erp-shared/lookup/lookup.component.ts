import { PermissionService } from '@abp/ng.core';
import {
  ChangeDetectorRef,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
  ViewChild,
  forwardRef,
  inject,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { NgbTypeaheadSelectItemEvent } from '@ng-bootstrap/ng-bootstrap';
import { Observable, OperatorFunction, Subject, Subscription, merge, of } from 'rxjs';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  filter,
  map,
  switchMap,
  tap,
} from 'rxjs/operators';
import { LookupItem, LookupSource } from '../models';
import { RecordDialogService } from '../record/record-dialog.service';
import { RecordEntity, RecordEntityRegistry, recordPermission } from '../record/record-entity';

export const LOOKUP_DEBOUNCE_MS = 250;
/** Records shown in the dropdown of an entity lookup before "Search More..." (Odoo shows 8). */
export const LOOKUP_DROPDOWN_LIMIT = 8;

/** The rows under the records of an entity lookup (Odoo's many2one dropdown footer). */
export type LookupAction = 'searchMore' | 'create' | 'createEdit';

/** A row of the dropdown: a record, or one of the actions under them. */
export interface LookupOption extends LookupItem {
  action?: LookupAction;
  /** What was typed when the dropdown was built; the actions work on it. */
  term?: string;
  /** The first action under the records. */
  first?: boolean;
}

/**
 * Typeahead lookup (customer no., item no., G/L account no. ...) usable with
 * `formControlName`, `[formControl]` and `[(ngModel)]`, also inside a `FormArray` row.
 *
 * The form value is the item's `code` (default) or `id`, see `valueField`.
 *
 * With `entity` (a key of `RecordEntityRegistry`) it becomes a full Odoo many2one / BC table
 * relation: the dropdown ends in "Search More..." (the full list), "Create 'term'" and
 * "Create and edit...", the "…" button opens the full list (BC "Select from full list") and the
 * arrow button opens the chosen record's card, where it can be edited or deleted.
 * `source`, when also given, still decides what the dropdown offers (e.g. only unblocked customers).
 */
@Component({
  selector: 'erp-lookup',
  templateUrl: './lookup.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => LookupComponent),
      multi: true,
    },
  ],
})
export class LookupComponent implements ControlValueAccessor, OnDestroy {
  /** Returns the items matching the typed term (an empty term means "first page"). */
  @Input() source?: LookupSource;
  /** Key of the registered record entity; turns on the list, create and open-card actions. */
  @Input() set entity(key: string | null | undefined) {
    this.recordEntity = this.registry.find(key);
  }
  /** Records shown in an entity lookup's dropdown before "Search More...". */
  @Input() dropdownLimit = LOOKUP_DROPDOWN_LIMIT;
  /** Which property of the selected item is written to the form control. */
  @Input() valueField: 'code' | 'id' = 'code';
  /** Localization key or plain text. */
  @Input() placeholder = '';
  /** 0 opens the list on focus. */
  @Input() minLength = 0;
  /** When true, text that matches no item is written as-is instead of `null`. */
  @Input() allowFreeText = false;
  /** Extra classes of the inner input, e.g. `form-control-sm`. */
  @Input() inputClass = '';
  @Input() inputId?: string;
  /**
   * Text to show for the current value when `valueField` is `'id'` (the id itself is not
   * meaningful to the user). Ignored once the user picks an item.
   */
  @Input() set displayText(value: string | null | undefined) {
    this._displayText = value ?? null;
    if (this.valueField === 'id' && this.value !== null && !this.selectedItem) {
      this.setInputText(this._displayText ?? '');
    }
  }
  /** Resolves a written `id` to its item so that the code can be shown. Optional. */
  @Input() resolve?: (value: string) => Observable<LookupItem | null>;

  /** Standalone (non forms) disabling. With reactive forms use `control.disable()`. */
  @Input() set disabled(value: boolean) {
    this.isDisabled = !!value;
  }

  /** Emits the full item on selection and `null` when the value is cleared. */
  @Output() selected = new EventEmitter<LookupItem | null>();

  @ViewChild('input', { static: true }) inputRef!: ElementRef<HTMLInputElement>;

  isDisabled = false;
  value: string | null = null;
  selectedItem: LookupItem | null = null;
  recordEntity?: RecordEntity;

  private _displayText: string | null = null;
  private lastItems: LookupItem[] = [];
  private readonly focus$ = new Subject<string>();
  private resolveSubscription?: Subscription;
  private blurSubscription?: Subscription;
  private onChange: (value: string | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  private readonly registry = inject(RecordEntityRegistry);
  private readonly dialogs = inject(RecordDialogService);
  private readonly permissions = inject(PermissionService);
  private actionSubscription?: Subscription;

  constructor(private cdr: ChangeDetectorRef) {}

  /** The operator handed to `ngbTypeahead`. */
  search: OperatorFunction<string, readonly LookupOption[]> = text$ =>
    merge(text$.pipe(debounceTime(LOOKUP_DEBOUNCE_MS), distinctUntilChanged()), this.focus$).pipe(
      filter(term => (term ?? '').length >= this.minLength),
      switchMap(term => {
        const source = this.effectiveSource;
        const items$ = source
          ? // Errors are already reported by ABP's HTTP error handler; keep the stream alive.
            source(term ?? '').pipe(catchError(() => of([] as LookupItem[])))
          : of([] as LookupItem[]);
        return items$.pipe(map(items => ({ term: (term ?? '').trim(), items: items ?? [] })));
      }),
      tap(({ items }) => (this.lastItems = [...items])),
      map(({ term, items }) => this.withActions(term, items)),
    );

  inputFormatter = (item: LookupItem | string): string =>
    typeof item === 'string' ? item : (item?.code ?? '');

  resultFormatter = (item: LookupItem): string =>
    item.name ? `${item.code} — ${item.name}` : item.code;

  get canCreate(): boolean {
    return (
      !!this.recordEntity &&
      this.permissions.getGrantedPolicy(recordPermission(this.recordEntity, 'Create'))
    );
  }

  /** `source`, or the entity's own list when only `entity` is given. */
  get effectiveSource(): LookupSource | undefined {
    const entity = this.recordEntity;
    if (this.source || !entity) {
      return this.source;
    }
    return term =>
      entity
        .getList({ filter: term, skipCount: 0, maxResultCount: this.dropdownLimit + 1 })
        .pipe(map(result => (result.items ?? []).map(dto => this.toItem(entity, dto))));
  }

  // ---- ControlValueAccessor -------------------------------------------------------------

  writeValue(value: string | null | undefined): void {
    this.resolveSubscription?.unsubscribe();
    this.blurSubscription?.unsubscribe();
    this.value = value === undefined || value === '' ? null : value;
    this.selectedItem = null;

    if (this.value === null) {
      this.setInputText('');
      return;
    }
    if (this.valueField === 'code') {
      this.setInputText(this.value);
      return;
    }

    // valueField === 'id'
    this.setInputText(this._displayText ?? '');
    const resolve = this.resolve ?? (this._displayText ? undefined : this.resolveByEntity);
    if (resolve) {
      const id = this.value;
      this.resolveSubscription = resolve(id)
        .pipe(catchError(() => of(null)))
        .subscribe(item => {
          if (item && this.value === id) {
            this.selectedItem = item;
            this.setInputText(item.code);
            this.cdr.markForCheck();
          }
        });
    }
  }

  registerOnChange(fn: (value: string | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
    this.cdr.markForCheck();
  }

  // ---- DOM events -----------------------------------------------------------------------

  onFocus(): void {
    this.blurSubscription?.unsubscribe();
    if (!this.isDisabled && this.minLength === 0) {
      this.focus$.next(this.inputRef.nativeElement.value);
    }
  }

  onSelect(event: NgbTypeaheadSelectItemEvent<LookupOption>): void {
    const option = event.item;
    if (option.action) {
      // Keep the typed text in the input: the action works on it.
      event.preventDefault();
      this.runAction(option.action, option.term ?? '');
      return;
    }
    this.select(option);
  }

  /** The "…" button, BC's "Select from full list". */
  openList(): void {
    this.runAction('searchMore', '');
  }

  /** The arrow button: opens the chosen record's card (Odoo's internal link). */
  openCard(): void {
    const entity = this.recordEntity;
    if (!entity || this.value === null) {
      return;
    }
    this.actionSubscription?.unsubscribe();
    this.actionSubscription = this.currentId(entity)
      .pipe(
        switchMap(id => (id ? this.dialogs.openCard(entity, { id, quick: false }) : of(null))),
      )
      .subscribe(result => {
        if (!result) {
          return;
        }
        if (result.deleted) {
          this.clear();
        } else {
          this.refresh(this.toItem(entity, result.record));
        }
        this.cdr.markForCheck();
      });
  }

  runAction(action: LookupAction, term: string): void {
    const entity = this.recordEntity;
    if (!entity || this.isDisabled) {
      return;
    }

    let record$: Observable<unknown>;
    switch (action) {
      case 'searchMore':
        record$ = this.dialogs.openList(entity, { filter: term, selectable: true });
        break;
      case 'create':
        record$ = entity.quickCreate ? entity.create(entity.quickCreate(term)) : of(null);
        break;
      default:
        record$ = this.dialogs
          .openCard(entity, { term, quick: true })
          .pipe(map(result => (result && !result.deleted ? result.record : null)));
    }

    this.actionSubscription?.unsubscribe();
    this.actionSubscription = record$.subscribe(record => {
      if (record) {
        this.select(this.toItem(entity, record));
      }
      this.inputRef.nativeElement.focus();
      this.cdr.markForCheck();
    });
  }

  /** Resolves whatever was typed: exact code match, free text, or `null`. */
  onBlur(): void {
    this.onTouched();
    const text = this.inputRef.nativeElement.value.trim();

    if (!text) {
      if (this.value !== null) {
        this.clear();
      }
      return;
    }
    if (this.selectedItem && text === this.selectedItem.code) {
      return;
    }
    if (!this.selectedItem && this.valueField === 'code' && text === this.value) {
      return; // untouched value written by the form
    }
    if (
      !this.selectedItem &&
      this.valueField === 'id' &&
      this.value !== null &&
      text === (this._displayText ?? '')
    ) {
      return; // untouched display text of a written id
    }

    const match = this.findByCode(this.lastItems, text);
    if (match) {
      this.select(match);
    } else if (this.source) {
      // Fast keyboard entry (type a code, press Tab) can blur before the debounced search ran:
      // ask the source once for an exact code match before giving up.
      this.blurSubscription?.unsubscribe();
      this.blurSubscription = this.source(text)
        .pipe(catchError(() => of([] as LookupItem[])))
        .subscribe(items => {
          this.applyTyped(text, this.findByCode(items ?? [], text));
          this.cdr.markForCheck();
        });
    } else {
      this.applyTyped(text, undefined);
    }
  }

  ngOnDestroy(): void {
    this.resolveSubscription?.unsubscribe();
    this.blurSubscription?.unsubscribe();
    this.actionSubscription?.unsubscribe();
    this.focus$.complete();
  }

  // ---- helpers --------------------------------------------------------------------------

  /** The records of the dropdown, cut to the limit, followed by the entity's actions. */
  private withActions(term: string, items: LookupItem[]): LookupOption[] {
    const entity = this.recordEntity;
    if (!entity) {
      return items;
    }

    const options: LookupOption[] = items.slice(0, this.dropdownLimit);
    // `first` draws the rule between the records and the actions; nothing to separate from without records.
    const actions: LookupOption[] = [{ code: '', action: 'searchMore', term, first: options.length > 0 }];
    if (this.canCreate) {
      const exists = items.some(i => i.code.toLowerCase() === term.toLowerCase());
      if (term && entity.quickCreate && !exists) {
        actions.push({ code: '', action: 'create', term });
      }
      actions.push({ code: '', action: 'createEdit', term });
    }
    return [...options, ...actions];
  }

  private toItem(entity: RecordEntity, dto: any): LookupItem {
    return { ...entity.toItem(dto), data: dto };
  }

  private readonly resolveByEntity = (id: string): Observable<LookupItem | null> => {
    const entity = this.recordEntity;
    return entity ? entity.get(id).pipe(map(dto => this.toItem(entity, dto))) : of(null);
  };

  /** The id of the current value; a code value is looked up by its code. */
  private currentId(entity: RecordEntity): Observable<string | null> {
    if (this.valueField === 'id') {
      return of(this.value);
    }
    if (this.selectedItem?.id) {
      return of(this.selectedItem.id);
    }
    const code = (this.value ?? '').toLowerCase();
    return entity.getList({ filter: this.value ?? '', skipCount: 0, maxResultCount: 20 }).pipe(
      map(result => {
        const dto = (result.items ?? []).find(r => entity.toItem(r).code.toLowerCase() === code);
        return dto?.id ?? null;
      }),
      catchError(() => of(null)),
    );
  }

  /** Shows an edited record without telling the form it was picked again, unless its key changed. */
  private refresh(item: LookupItem): void {
    const value = this.valueField === 'id' ? (item.id ?? null) : item.code;
    const changed = value !== this.value;
    this.selectedItem = item;
    this.setInputText(item.code);
    if (changed) {
      this.commit(value);
      this.selected.emit(item);
    }
  }

  private findByCode(items: LookupItem[], text: string): LookupItem | undefined {
    return items.find(item => item.code.toLowerCase() === text.toLowerCase());
  }

  private applyTyped(text: string, match: LookupItem | undefined): void {
    if (match) {
      this.select(match);
    } else if (this.allowFreeText) {
      this.selectedItem = null;
      this.commit(text);
      this.selected.emit(null);
    } else {
      this.clear();
    }
  }

  private select(item: LookupItem): void {
    this.selectedItem = item;
    this.setInputText(item.code);
    this.commit(this.valueField === 'id' ? (item.id ?? null) : item.code);
    this.selected.emit(item);
  }

  private clear(): void {
    this.selectedItem = null;
    this.setInputText('');
    this.commit(null);
    this.selected.emit(null);
  }

  private commit(value: string | null): void {
    if (value === this.value) {
      return;
    }
    this.value = value;
    this.onChange(value);
  }

  private setInputText(text: string): void {
    this.inputRef.nativeElement.value = text;
  }
}
