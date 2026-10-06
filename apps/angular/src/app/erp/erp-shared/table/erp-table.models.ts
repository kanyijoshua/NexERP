import { PagedResultDto } from '@abp/ng.core';
import { TemplateRef } from '@angular/core';
import { Observable } from 'rxjs';

export type ErpTableColumnType =
  | 'text'
  | 'code'
  | 'link'
  | 'date'
  | 'datetime'
  | 'currency'
  | 'number'
  | 'boolean'
  | 'badge'
  | 'select'
  | 'switch'
  | 'custom';

export type ErpColumnPin = 'left' | 'right' | 'none';

export interface ErpTableOption {
  value: any;
  /** Localization key or plain text. */
  label: string;
}

export interface ErpTableColumn<T = any> {
  /** Field name on the row object */
  field: string;
  /** Localization key (e.g. 'Erp::Code') or plain string */
  labelKey?: string;
  /** Presentation format of the cell; also decides the filter operators offered. */
  type?: ErpTableColumnType;
  /** Width in pixels */
  width?: number;
  /** Min-width in pixels */
  minWidth?: number;
  /** Max-width in pixels */
  maxWidth?: number;
  /** Whether column is sortable (default true) */
  sortable?: boolean;
  /** Custom header CSS class */
  headerClass?: string;
  /** Custom cell CSS class or class function */
  cellClass?: string | ((data: { row: T; column?: any; value?: any } | T) => string | Record<string, boolean>);
  /** For code/link: whether the value is a link that emits `codeClick`. */
  clickable?: boolean | ((row: T) => boolean);
  /** Optional field name providing a Business Central-style secondary sublink value */
  sublinkField?: string;
  /** Optional formatter or extractor for the secondary sublink */
  sublinkFormat?: (row: T) => string | null | undefined;
  /** For badge type: function returning CSS classes or static class */
  badgeClass?: string | ((row: T) => string);
  /** Custom formatter function */
  format?: (value: any, row: T) => string;
  /** Optional icon prefix for cell */
  icon?: string | ((row: T) => string);
  /** Optional custom template ref */
  template?: TemplateRef<any>;
  /** Visual indentation settings: when true, indentation is applied based on row[indentField || 'indentation'] */
  indent?: boolean;
  /** Property on row providing indentation level (defaults to 'indentation') */
  indentField?: string;
  /** `select` / `badge`: the values the field takes, shown by label and offered as filter values. */
  options?: ErpTableOption[];

  /** Pin column on horizontal scrolling: 'left' | 'right' | 'none' */
  pinned?: ErpColumnPin;
  /** Shorthand for pinned === 'left' */
  frozenLeft?: boolean;
  /** Shorthand for pinned === 'right' */
  frozenRight?: boolean;
  /** Whether column is visible (default true) */
  visible?: boolean;
  /** Whether column can be hidden by user in column chooser (default true) */
  hideable?: boolean;
  /** Whether column can be dynamically filtered (default true) */
  filterable?: boolean;
  /** The field name the server filters on, when it differs from `field`. */
  filterField?: string;
  /** The field name the server sorts on, when it differs from `field`. */
  sortField?: string;
}

export interface ErpTableAction<T = any> {
  key?: string;
  labelKey?: string;
  title?: string;
  icon: string;
  btnClass?: string;
  permission?: string;
  visible?: boolean | ((row: T) => boolean);
  disabled?: boolean | ((row: T) => boolean);
  action: (row: T, event?: MouseEvent) => void;
}

export interface ErpTableSwitchEvent<T = any> {
  row: T;
  field: string;
  checked: boolean;
}

export type ErpFilterOperator =
  // Text operators
  | 'contains'
  | 'notContains'
  | 'equals'
  | 'notEquals'
  | 'startsWith'
  | 'endsWith'
  | 'isEmpty'
  | 'isNotEmpty'
  // Number / Currency operators
  | 'gt'
  | 'gte'
  | 'lt'
  | 'lte'
  | 'between'
  // Date operators
  | 'before'
  | 'after'
  | 'today'
  | 'thisWeek'
  | 'thisMonth'
  | 'thisYear'
  // Boolean operators
  | 'isTrue'
  | 'isFalse'
  // A filter expression, e.g. `1000..2000|3000`, `A*`, `<>0`
  | 'expression';

export interface ErpFilterCriterion {
  id: string;
  field: string;
  operator: ErpFilterOperator;
  value?: any;
  valueTo?: any; // For 'between' ranges
}

export type ErpFilterLogic = 'and' | 'or';

export interface ErpFilterState {
  searchTerm: string;
  criteria: ErpFilterCriterion[];
  logic: ErpFilterLogic;
}

export interface ErpSavedColumnConfig {
  field: string;
  visible: boolean;
  pinned: ErpColumnPin;
  width?: number;
}

/** A saved filter: search, conditions and match logic under a name. */
export interface ErpSavedView {
  name: string;
  state: ErpFilterState;
}

/** What `erp-table` asks its `source` for: one page of rows under the current search, filter and sort. */
export interface ErpTableQuery {
  filter?: string;
  sorting?: string;
  skipCount: number;
  maxResultCount: number;
  /** The filter pane's conditions as JSON; see `toDynamicFilter`. */
  dynamicFilter?: string;
}

/** Loads rows for `erp-table`; typically `query => this.service.getList({ ...query, ...pageFilters })`. */
export type ErpTableSource<T> = (query: ErpTableQuery) => Observable<PagedResultDto<T>>;
