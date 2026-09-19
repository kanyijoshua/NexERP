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
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { NgbTypeaheadSelectItemEvent } from '@ng-bootstrap/ng-bootstrap';
import { Observable, OperatorFunction, Subject, Subscription, merge, of } from 'rxjs';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  filter,
  switchMap,
  tap,
} from 'rxjs/operators';
import { LookupItem, LookupSource } from '../models';

export const LOOKUP_DEBOUNCE_MS = 250;

/**
 * Typeahead lookup (customer no., item no., G/L account no. ...) usable with
 * `formControlName`, `[formControl]` and `[(ngModel)]`, also inside a `FormArray` row.
 *
 * The form value is the item's `code` (default) or `id`, see `valueField`.
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

  private _displayText: string | null = null;
  private lastItems: LookupItem[] = [];
  private readonly focus$ = new Subject<string>();
  private resolveSubscription?: Subscription;
  private blurSubscription?: Subscription;
  private onChange: (value: string | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  constructor(private cdr: ChangeDetectorRef) {}

  /** The operator handed to `ngbTypeahead`. */
  search: OperatorFunction<string, readonly LookupItem[]> = text$ =>
    merge(text$.pipe(debounceTime(LOOKUP_DEBOUNCE_MS), distinctUntilChanged()), this.focus$).pipe(
      filter(term => (term ?? '').length >= this.minLength),
      switchMap(term =>
        this.source
          ? // Errors are already reported by ABP's HTTP error handler; keep the stream alive.
            this.source(term ?? '').pipe(catchError(() => of([] as LookupItem[])))
          : of([] as LookupItem[]),
      ),
      tap(items => (this.lastItems = [...(items ?? [])])),
    );

  inputFormatter = (item: LookupItem | string): string =>
    typeof item === 'string' ? item : item?.code ?? '';

  resultFormatter = (item: LookupItem): string =>
    item.name ? `${item.code} — ${item.name}` : item.code;

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
    if (this.resolve) {
      const id = this.value;
      this.resolveSubscription = this.resolve(id)
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

  onSelect(event: NgbTypeaheadSelectItemEvent<LookupItem>): void {
    this.select(event.item);
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
    this.focus$.complete();
  }

  // ---- helpers --------------------------------------------------------------------------

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
    this.commit(this.valueField === 'id' ? item.id ?? null : item.code);
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
