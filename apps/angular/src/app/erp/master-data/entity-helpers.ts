import { PagedResultDto } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { DocumentLineOption, RecordAction, RecordColumn, RecordEntity, RecordField, RecordSection } from '../erp-shared';

/** Enum options labelled `Erp::Enum:<EnumName>.<Member>`. */
export function enumOptions(
  options: { key: string; value: number }[],
  enumName: string,
): DocumentLineOption[] {
  return options.map(o => ({ value: o.value, label: `Erp::Enum:${enumName}.${o.key}` }));
}

/** A typed term as a BC code: upper case, no spaces at the ends, cut to the field length. */
export function codeOf(term: string | undefined, maxLength: number): string {
  return (term ?? '').trim().toUpperCase().substring(0, maxLength);
}

/** BC's Block / Unblock on customers, vendors, items, G/L and bank accounts. */
export function blockActions<T extends { id?: string; blocked: boolean }>(
  permission: string,
  service: { block(id: string): Observable<void>; unblock(id: string): Observable<void> },
): RecordAction<T>[] {
  return [
    {
      key: 'block',
      labelKey: 'Erp::Block',
      icon: 'fas fa-ban',
      permission: `${permission}.Update`,
      visible: dto => !dto.blocked,
      confirmKey: 'Erp::BlockConfirmation',
      run: dto => service.block(dto.id!),
    },
    {
      key: 'unblock',
      labelKey: 'Erp::Unblock',
      icon: 'fas fa-circle-check',
      permission: `${permission}.Update`,
      visible: dto => dto.blocked,
      run: dto => service.unblock(dto.id!),
    },
  ];
}

/** A G/L account lookup on a setup table; the account number is what is stored. */
export function accountField(field: string, labelKey: string, section: string, required = false): RecordField {
  return { field, labelKey, type: 'lookup', lookupEntity: 'glAccount', section, required };
}

/** A lookup into a code table; the code is what is stored. */
export function codeField(
  field: string,
  labelKey: string,
  lookupEntity: string,
  section?: string,
  extra: Partial<RecordField> = {},
): RecordField {
  return { field, labelKey, type: 'lookup', lookupEntity, section, ...extra };
}

/** What a code table service offers: the ABP CRUD endpoints with a filtered, paged list. */
export interface CodeTableApi<TDto, TInput> {
  getList(input: { filter?: string; sorting?: string; skipCount?: number; maxResultCount?: number }): Observable<PagedResultDto<TDto>>;
  get(id: string): Observable<TDto>;
  create(input: TInput): Observable<TDto>;
  update(id: string, input: TInput): Observable<TDto>;
  delete(id: string): Observable<void>;
}

export interface CodeTableOptions<TDto> {
  key: string;
  titleKey: string;
  pluralKey: string;
  icon: string;
  /** Permission prefix, e.g. `Erp.PostingSetup`. */
  permission: string;
  route: string;
  /** BC Code length: 20 by default, 10 for currencies, payment terms and locations. */
  codeLength?: number;
  /** Label of the description, e.g. `Erp::Name` where BC calls it a name. */
  descriptionKey?: string;
  columns?: RecordColumn[];
  fields?: RecordField[];
  sections?: RecordSection[];
  /** Values of a new record beyond its code. */
  defaults?: Partial<TDto>;
  /** A table whose rows need more than a code and description gets "Create and edit..." instead. */
  quickCreate?: boolean;
}

/**
 * A code table (posting groups, payment terms, causes of absence and the like): a code, a
 * description and whatever else the table adds. Codes are upper case on the server.
 */
export function codeTableEntity<TDto extends { id?: string; code?: string; description?: string }, TInput>(
  service: CodeTableApi<TDto, TInput>,
  options: CodeTableOptions<TDto>,
): RecordEntity<TDto, TInput> {
  const codeLength = options.codeLength ?? 20;
  const descriptionKey = options.descriptionKey ?? 'Erp::Description';
  const quickCreate = options.quickCreate ?? !(options.fields ?? []).some(f => f.required);

  return {
    key: options.key,
    titleKey: options.titleKey,
    pluralKey: options.pluralKey,
    icon: options.icon,
    permission: options.permission,
    listRoute: [options.route],
    columns: [
      { field: 'code', labelKey: 'Erp::Code', width: 130 },
      { field: 'description', labelKey: descriptionKey, width: 280 },
      ...(options.columns ?? []),
    ],
    sections: options.sections ?? [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true, maxLength: codeLength },
      { field: 'description', labelKey: descriptionKey, type: 'text', maxLength: 250 },
      ...(options.fields ?? []),
    ],
    getList: query => service.getList(query),
    get: id => service.get(id),
    create: input => service.create(input),
    update: (id, input) => service.update(id, input),
    delete: id => service.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: dto.description ?? undefined }),
    newRecord: term => ({ ...(options.defaults ?? {}), code: codeOf(term, codeLength) }) as Partial<TDto>,
    quickCreate: quickCreate
      ? term => ({ ...(options.defaults ?? {}), code: codeOf(term, codeLength), description: '' }) as TInput
      : undefined,
  };
}

/**
 * A BC posting group table (Gen. Business, Gen. Product, VAT, Customer, Vendor, Inventory, Bank
 * Account): a code, a description and, for some, the account the group posts to.
 */
export function postingGroupEntity<TDto extends { id?: string; code?: string; description?: string }, TInput>(
  key: string,
  titleKey: string,
  pluralKey: string,
  icon: string,
  route: string,
  service: CodeTableApi<TDto, TInput>,
  account?: { field: string; labelKey: string },
  permission = 'Erp.PostingSetup',
): RecordEntity<TDto, TInput> {
  return codeTableEntity(service, {
    key,
    titleKey,
    pluralKey,
    icon,
    permission,
    route,
    columns: account ? [{ field: account.field, labelKey: account.labelKey, width: 160 }] : [],
    fields: account ? [accountField(account.field, account.labelKey, 'general', true)] : [],
  });
}
