import { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DocumentLineOption, LookupItem, SmartButton } from '../models';

/** The page query every record list sends: ABP's paged, sorted, filtered request. */
export interface RecordQuery {
  filter?: string;
  sorting?: string;
  skipCount: number;
  maxResultCount: number;
}

export type RecordFieldType =
  | 'text'
  | 'textarea'
  | 'email'
  | 'number'
  | 'currency'
  | 'select'
  | 'checkbox'
  | 'lookup'
  | 'readonly';

/** One field of a record card (BC card page field, Odoo form field). */
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
  /** Leaves the field out of the quick-create dialog (Odoo's quick create shows the essentials). */
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
  /** Starts collapsed on the card page, as BC does for the less used FastTabs. */
  collapsed?: boolean;
}

export type RecordColumnType = 'text' | 'number' | 'currency' | 'boolean' | 'select';

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
}

/** A value of the FactBox on the card page (BC FactBox, Odoo stat info). */
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
  /** Values of a new record; `term` is what the user typed in the lookup (Odoo "Create 'term'"). */
  newRecord(term?: string): Partial<TDto>;
  /**
   * The input for Odoo's one-click "Create 'term'". Leave it out when a record needs more than
   * a name: the lookup then offers "Create and edit..." with the term filled in instead.
   */
  quickCreate?(term: string): TInput;

  facts?(dto: TDto): RecordFact[];
  actions?: RecordAction<TDto>[];
  /** Odoo smart buttons of the card page. */
  related?(dto: TDto): Observable<SmartButton[]>;
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

export function recordPermission(entity: RecordEntity, action?: 'Create' | 'Update' | 'Delete'): string {
  return action ? `${entity.permission}.${action}` : entity.permission;
}

export function recordToInput(entity: RecordEntity, value: Record<string, unknown>): unknown {
  return entity.toInput ? entity.toInput(value) : value;
}
