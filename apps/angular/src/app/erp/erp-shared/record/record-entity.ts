import { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DocumentLineOption, LookupItem, SmartButton } from '../models';
import { ErpTableColumn } from '../table/erp-table.models';

/** The page query every record list sends: ABP's paged, sorted, filtered request. */
export interface RecordQuery {
  filter?: string;
  sorting?: string;
  skipCount: number;
  maxResultCount: number;
  /** The list's filter pane conditions as JSON (see `erp-table`). */
  dynamicFilter?: string;
}

export type RecordFieldType =
  | 'text'
  | 'textarea'
  | 'email'
  | 'number'
  | 'currency'
  | 'date'
  | 'select'
  | 'checkbox'
  | 'lookup'
  | 'readonly';

/** One field of a record card. */
export interface RecordField {
  /** Property of the DTO and name of the form control. */
  field: string;
  labelKey: string;
  type: RecordFieldType;
  /** Key of the FastTab the field sits on; the first section when omitted. */
  section?: string;
  required?: boolean;
  maxLength?: number;
  min?: number;
  /** `select` only. */
  options?: DocumentLineOption[];
  /** `lookup` only: key of the registered entity to look the value up in. */
  lookupEntity?: string;
  /** `lookup` only, defaults to `'code'`. */
  lookupValueField?: 'code' | 'id';
  /** `lookup` with `lookupValueField: 'id'`: DTO property holding the code to show for the id. */
  lookupDisplayField?: string;
  /** Localization key of a hint under the field. */
  helpKey?: string;
  /** Localization key of the placeholder. */
  placeholderKey?: string;
  /** Leaves the field out of the quick-create dialog. */
  cardOnly?: boolean;
  /** Editable on a new record only, e.g. a code other records point at. */
  createOnly?: boolean;
  /** Full row instead of half a row. */
  wide?: boolean;
}

/** A FastTab of the card page. */
export interface RecordSection {
  key: string;
  labelKey: string;
  /** Starts collapsed on the card page for the less used FastTabs. */
  collapsed?: boolean;
}

export type RecordColumnType = 'text' | 'number' | 'currency' | 'boolean' | 'select' | 'date';

/** One column of the full list. */
export interface RecordColumn {
  field: string;
  labelKey: string;
  type?: RecordColumnType;
  /** `select` only: value to label. */
  options?: DocumentLineOption[];
  /** Server-side sortable; defaults to `true`. */
  sortable?: boolean;
  width?: number;
  /** Offered in the list's filter pane; defaults to `true`. Turn off for values the server computes. */
  filterable?: boolean;
}

/** A record table's columns as grid columns: the first one links to the card. */
export function toRecordTableColumns(columns: RecordColumn[]): ErpTableColumn[] {
  return columns.map((column, index) => ({
    field: column.field,
    labelKey: column.labelKey,
    type: index === 0 ? 'code' : (column.type ?? 'text'),
    width: column.width,
    sortable: column.sortable !== false,
    filterable: column.filterable !== false,
    options: column.options,
  }));
}

/** A value of the FactBox on the card page. */
export interface RecordFact {
  labelKey: string;
  value: string | number | boolean | null | undefined;
  type?: 'text' | 'currency' | 'number' | 'boolean';
  /** Colours a boolean fact red when true, e.g. "Blocked". */
  warnWhenTrue?: boolean;
}

/** An extra action on the card page, e.g. Block / Unblock. */
export interface RecordAction<TDto> {
  key: string;
  labelKey: string;
  icon: string;
  /** Full permission name, e.g. `Erp.Customers.Update`. */
  permission?: string;
  visible?: (dto: TDto) => boolean;
  confirmKey?: string;
  run: (dto: TDto) => Observable<unknown>;
}

/**
 * A table of lines shown on a record's card under its fields, e.g. a voucher's lines. The lines are
 * records of another registered entity, opened, added and deleted through its card dialog.
 */
export interface RecordPart<TDto> {
  /** Registry key of the line entity. */
  entity: string;
  /** Heading; the line entity's plural when omitted. */
  titleKey?: string;
  /** The lines of the record. */
  lines(dto: TDto): Observable<Record<string, any>[]>;
  /** Values a new line starts with, e.g. the number of the document it belongs to. */
  newLine?(dto: TDto): Record<string, unknown>;
  /** Fields of the line entity's columns to show, in order; all of them when omitted. */
  columns?: string[];
  /** Currency or number fields totalled under the table. */
  totals?: string[];
  /** Whether lines can be added, changed and deleted; always, when omitted. */
  editable?(dto: TDto): boolean;
}

/**
 * Everything the generic record UI (lookup dropdown, full list, quick card dialog and the card page)
 * needs to know about one master-data table. A descriptor is the only place a table is described:
 * the lookup, the "Search More" list, the dialog and the card page all read it.
 */
export interface RecordEntity<TDto extends { id?: string } = any, TInput = any> {
  /** Registry key, e.g. `customer`. */
  key: string;
  /** Singular and plural localization keys. */
  titleKey: string;
  pluralKey: string;
  icon: string;
  /** Permission prefix; `.Create`, `.Update` and `.Delete` are appended. */
  permission: string;
  /** Route of the list page; the card page is `[...listRoute, id]`. */
  listRoute?: string[];
  /**
   * Name of the table in the server's entity registry, e.g. `FixedAsset`: the card page then shows
   * the files attached to the record. Defaults to `chatterEntityType`.
   */
  attachmentEntityType?: string;
  /** A table only posting writes (ledger entries, posted statements): no New, Edit or Delete. */
  readOnly?: boolean;
  /** Chatter thread key for the card page. No chatter when omitted. */
  chatterEntityType?: string;

  columns: RecordColumn[];
  fields: RecordField[];
  sections: RecordSection[];

  getList(query: RecordQuery): Observable<PagedResultDto<TDto>>;
  get(id: string): Observable<TDto>;
  create(input: TInput): Observable<TDto>;
  update(id: string, input: TInput): Observable<TDto>;
  delete(id: string): Observable<unknown>;

  /** How a record shows in a lookup: `code` goes in the input, `name` next to it in the list. */
  toItem(dto: TDto): LookupItem;
  /** Form value to create/update DTO. Defaults to the form value itself. */
  toInput?(value: Record<string, unknown>): TInput;
  /** Values of a new record; `term` is what the user typed in the lookup. */
  newRecord(term?: string): Partial<TDto>;
  /**
   * The input for the one-click "Create 'term'". Leave it out when a record needs more than
   * a name: the lookup then offers "Create and edit..." with the term filled in instead.
   */
  quickCreate?(term: string): TInput;

  facts?(dto: TDto): RecordFact[];
  actions?: RecordAction<TDto>[];
  /** Smart buttons of the card page. */
  related?(dto: TDto): Observable<SmartButton[]>;
  /** Line tables of the card page, under the fields. */
  parts?: RecordPart<TDto>[];
}

/**
 * The record entities the running app knows, by key. Feature code registers its descriptors once
 * (see `MasterDataEntities`); generic components only ever resolve a key.
 */
@Injectable({ providedIn: 'root' })
export class RecordEntityRegistry {
  private readonly entities = new Map<string, RecordEntity>();

  register(...entities: RecordEntity[]): void {
    entities.forEach(entity => this.entities.set(entity.key, entity));
  }

  find(key: string | null | undefined): RecordEntity | undefined {
    return key ? this.entities.get(key) : undefined;
  }

  get(key: string): RecordEntity {
    const entity = this.find(key);
    if (!entity) {
      throw new Error(`No record entity is registered under "${key}".`);
    }
    return entity;
  }

  get all(): RecordEntity[] {
    return [...this.entities.values()];
  }
}

/** A policy name nobody holds; what a read-only table asks for to change a record. */
export const READ_ONLY_PERMISSION = 'Erp.ReadOnlyTable';

export function recordPermission(entity: RecordEntity, action?: 'Create' | 'Update' | 'Delete'): string {
  if (action && entity.readOnly) {
    // Never granted, so every create, edit and delete affordance stays hidden.
    return READ_ONLY_PERMISSION;
  }

  return action ? `${entity.permission}.${action}` : entity.permission;
}

export function recordToInput(entity: RecordEntity, value: Record<string, unknown>): unknown {
  return entity.toInput ? entity.toInput(value) : value;
}
