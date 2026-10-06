import { FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

/** An item shown by `erp-lookup`. `code` is what the user sees in the input. */
export interface LookupItem {
  id?: string;
  code: string;
  name?: string;
  /** The record behind the item (entity lookups), e.g. to copy an item's price onto a line. */
  data?: any;
}

export type LookupSource = (term: string) => Observable<LookupItem[]>;

export interface DocumentLineOption {
  value: string | number | boolean | null;
  /** Localization key (`Erp::...`) or plain text. */
  label: string;
}

export type DocumentLineColumnType =
  'text' | 'number' | 'date' | 'select' | 'lookup' | 'readonly' | 'currency' | 'checkbox';

/** Describes one editable cell of a document / journal line. */
export interface DocumentLineColumn {
  /** Name of the control inside each line `FormGroup`. */
  field: string;
  labelKey: string;
  type: DocumentLineColumnType;
  /** Any CSS width, e.g. `'120px'` or `'15%'`. */
  width?: string;
  /** `select` only. */
  options?: DocumentLineOption[];
  /** `lookup` only. */
  lookupSource?: LookupSource;
  /** `lookup` only, defaults to `'code'`. */
  lookupValueField?: 'code' | 'id';
  /** `lookup` only, defaults to `false`. */
  lookupAllowFreeText?: boolean;
  /**
   * `lookup` only: key of the record entity the cell looks up (adds "Search More...", create and
   * open-card). A function picks it per row, e.g. item or G/L account after the line type.
   */
  lookupEntity?: string | ((row: FormGroup) => string | null);
  /** `number` / `currency` only. */
  step?: number;
  /** Shown but never edited, e.g. a name the server fills in from the code next to it. */
  readonly?: boolean;
}

export interface DocumentLineChange {
  index: number;
  field: string;
  /** Set when the change comes from a lookup selection. */
  item?: LookupItem | null;
}

export interface DocumentTotal {
  labelKey: string;
  value: number;
  /** Renders the row in bold (grand total). */
  emphasis?: boolean;
}

export interface DocumentAction {
  key: string;
  labelKey: string;
  icon: string;
  /** Bootstrap button classes, defaults to `btn-outline-primary`. */
  cssClass?: string;
  permission?: string;
  disabled?: boolean;
  /** When set, the user must confirm this localized message first. */
  confirmKey?: string;
  /** Disables the action while the header form or the lines are invalid. */
  requiresValid?: boolean;
}

export interface JournalBatch {
  id: string;
  name: string;
}

export interface KanbanColumn<T> {
  id: string;
  title: string;
  cards: T[];
  aggregateLabel?: string;
  folded?: boolean;
  isWon?: boolean;
  isLost?: boolean;
}

export interface KanbanMoveEvent {
  cardId: string;
  fromColumnId: string;
  toColumnId: string;
  index: number;
  /** Rolls the optimistic move back (call it when the server rejects the move). */
  revert: () => void;
}

export interface ReportColumn {
  field: string;
  labelKey: string;
  type?: 'text' | 'number' | 'currency' | 'date';
  align?: 'start' | 'center' | 'end';
}

export interface SmartButton {
  labelKey: string;
  icon: string;
  count?: number | string;
  routerLink?: any[];
  queryParams?: Record<string, unknown>;
  permission?: string;
}
