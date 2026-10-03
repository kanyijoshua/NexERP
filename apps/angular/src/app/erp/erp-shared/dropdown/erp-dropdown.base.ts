import { Directive, EventEmitter, Input, Output } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';
import { ErpDropdownOption } from './erp-dropdown.models';

/**
 * Base abstract class for dynamic ERP dropdowns.
 * Implements `ControlValueAccessor` to support both Reactive Forms and `[(ngModel)]`.
 *
 * Can be inherited by specialized dropdown components (e.g. StatusDropdown,
 * CurrencyDropdown, UserPickerDropdown, ReportPickerDropdown).
 */
@Directive()
export abstract class ErpDropdownBase<T = any> implements ControlValueAccessor {
  @Input() options: ErpDropdownOption<T>[] = [];
  @Input() placeholder = 'Select option...';
  @Input() isMulti = false;
  @Input() clearable = true;
  @Input() searchable = true;
  @Input() disabled = false;
  @Input() loading = false;
  @Input() emptyText = 'No options found';
  @Input() icon?: string;

  @Output() selectionChange = new EventEmitter<T | T[] | null>();
  @Output() opened = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  isOpen = false;
  searchQuery = '';
  activeHighlightIndex = -1;

  // Selected state
  selectedOption: ErpDropdownOption<T> | null = null;
  selectedOptions: ErpDropdownOption<T>[] = [];

  // Form hooks
  protected onChange: (val: any) => void = () => {};
  protected onTouched: () => void = () => {};

  /**
   * Filtered options based on search query.
   */
  get filteredOptions(): ErpDropdownOption<T>[] {
    if (!this.searchQuery || !this.searchQuery.trim()) {
      return this.options;
    }
    const q = this.searchQuery.trim().toLowerCase();
    return this.options.filter(opt =>
      opt.label.toLowerCase().includes(q) ||
      (opt.sublabel && opt.sublabel.toLowerCase().includes(q))
    );
  }

  /**
   * Display text for the closed dropdown trigger.
   */
  get displayLabel(): string {
    if (this.isMulti) {
      if (this.selectedOptions.length === 0) return this.placeholder;
      if (this.selectedOptions.length === 1) return this.selectedOptions[0].label;
      return `${this.selectedOptions.length} selected`;
    }
    return this.selectedOption ? this.selectedOption.label : this.placeholder;
  }

  toggleDropdown(): void {
    if (this.disabled || this.loading) return;
    if (this.isOpen) {
      this.closeDropdown();
    } else {
      this.openDropdown();
    }
  }

  openDropdown(): void {
    if (this.disabled || this.loading || this.isOpen) return;
    this.isOpen = true;
    this.searchQuery = '';
    this.activeHighlightIndex = -1;
    this.opened.emit();
  }

  closeDropdown(): void {
    if (!this.isOpen) return;
    this.isOpen = false;
    this.onTouched();
    this.closed.emit();
  }

  selectOption(option: ErpDropdownOption<T>): void {
    if (option.disabled) return;

    if (this.isMulti) {
      const idx = this.selectedOptions.findIndex(o => this.areValuesEqual(o.value, option.value));
      if (idx >= 0) {
        this.selectedOptions = this.selectedOptions.filter((_, i) => i !== idx);
      } else {
        this.selectedOptions = [...this.selectedOptions, option];
      }
      const values = this.selectedOptions.map(o => o.value);
      this.onChange(values);
      this.selectionChange.emit(values);
    } else {
      this.selectedOption = option;
      this.onChange(option.value);
      this.selectionChange.emit(option.value);
      this.closeDropdown();
    }
  }

  removeOption(option: ErpDropdownOption<T>, event?: Event): void {
    event?.stopPropagation();
    if (this.disabled) return;
    this.selectedOptions = this.selectedOptions.filter(o => !this.areValuesEqual(o.value, option.value));
    const values = this.selectedOptions.map(o => o.value);
    this.onChange(values);
    this.selectionChange.emit(values);
  }

  clearSelection(event?: Event): void {
    event?.stopPropagation();
    if (this.disabled) return;

    if (this.isMulti) {
      this.selectedOptions = [];
      this.onChange([]);
      this.selectionChange.emit([]);
    } else {
      this.selectedOption = null;
      this.onChange(null);
      this.selectionChange.emit(null);
    }
  }

  isSelected(option: ErpDropdownOption<T>): boolean {
    if (this.isMulti) {
      return this.selectedOptions.some(o => this.areValuesEqual(o.value, option.value));
    }
    return this.selectedOption ? this.areValuesEqual(this.selectedOption.value, option.value) : false;
  }

  onSearchInput(query: string): void {
    this.searchQuery = query;
    this.activeHighlightIndex = 0;
  }

  onKeyDown(event: KeyboardEvent): void {
    if (!this.isOpen) {
      if (event.key === 'ArrowDown' || event.key === 'Enter' || event.key === ' ') {
        event.preventDefault();
        this.openDropdown();
      }
      return;
    }

    const count = this.filteredOptions.length;
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.activeHighlightIndex = (this.activeHighlightIndex + 1) % count;
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.activeHighlightIndex = (this.activeHighlightIndex - 1 + count) % count;
    } else if (event.key === 'Enter') {
      event.preventDefault();
      if (this.activeHighlightIndex >= 0 && this.activeHighlightIndex < count) {
        this.selectOption(this.filteredOptions[this.activeHighlightIndex]);
      }
    } else if (event.key === 'Escape') {
      event.preventDefault();
      this.closeDropdown();
    }
  }

  // ControlValueAccessor implementations
  writeValue(value: any): void {
    if (this.isMulti) {
      const arr = Array.isArray(value) ? value : [];
      this.selectedOptions = this.options.filter(o => arr.some(v => this.areValuesEqual(o.value, v)));
    } else {
      if (value !== null && value !== undefined) {
        const found = this.options.find(o => this.areValuesEqual(o.value, value));
        this.selectedOption = found ?? { value, label: String(value) };
      } else {
        this.selectedOption = null;
      }
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    if (isDisabled && this.isOpen) {
      this.closeDropdown();
    }
  }

  protected areValuesEqual(a: any, b: any): boolean {
    if (a === b) return true;
    if (a && b && typeof a === 'object' && typeof b === 'object') {
      return a.id !== undefined && b.id !== undefined ? a.id === b.id : JSON.stringify(a) === JSON.stringify(b);
    }
    return false;
  }
}
