import {
  ConfigFieldInfoDto,
  ConfigLineStatus,
  ConfigLineType,
  ConfigPackageErrorDto,
  ConfigPackageFieldDto,
  ConfigTableInfoDto,
  ImportColumnDto,
  ImportFilePreviewDto,
} from '@proxy/rapid-start';

/** The largest file the service accepts (`ErpDomainConsts.MaxImportFileBytes`). */
export const MAX_IMPORT_FILE_BYTES = 10 * 1024 * 1024;

export interface TableArea {
  area: string;
  tables: ConfigTableInfoDto[];
}

/** Groups the table catalog by functional area, areas and tables in alphabetical order. */
export function groupTablesByArea(tables: ConfigTableInfoDto[]): TableArea[] {
  const byArea = new Map<string, ConfigTableInfoDto[]>();

  for (const table of tables ?? []) {
    const area = table.area || '';
    const list = byArea.get(area) ?? [];
    list.push(table);
    byArea.set(area, list);
  }

  return Array.from(byArea.entries())
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([area, list]) => ({
      area,
      tables: [...list].sort((a, b) =>
        (a.displayName || a.entityName || '').localeCompare(b.displayName || b.entityName || ''),
      ),
    }));
}

/** Bootstrap badge classes of a worksheet status. */
export function configLineStatusClass(status: ConfigLineStatus | null | undefined): string {
  switch (status) {
    case ConfigLineStatus.InProgress:
      return 'bg-warning text-dark';
    case ConfigLineStatus.Completed:
      return 'bg-success';
    case ConfigLineStatus.Ignored:
      return 'bg-light text-muted border';
    case ConfigLineStatus.Blocked:
      return 'bg-danger';
    default:
      return 'bg-secondary';
  }
}

/** How far a worksheet line is indented: areas at the margin, groups one step in, tables two. */
export function configLineIndent(lineType: ConfigLineType | null | undefined): number {
  switch (lineType) {
    case ConfigLineType.Area:
      return 0;
    case ConfigLineType.Group:
      return 1;
    default:
      return 2;
  }
}

/** The page each table is set up on, where the application has one. */
export const TABLE_PAGE_ROUTES: Readonly<Record<string, string>> = {
  GLAccount: '/erp/chart-of-accounts',
  Customer: '/erp/customers',
  Vendor: '/erp/vendors',
  Item: '/erp/items',
  UnitOfMeasure: '/erp/units-of-measure',
  ItemCategory: '/erp/item-categories',
  NoSeries: '/erp/setup/no-series',
  NoSeriesLine: '/erp/setup/no-series',
  SalesReceivablesSetup: '/erp/setup/document-numbering',
  PurchasesPayablesSetup: '/erp/setup/document-numbering',
  Workflow: '/erp/setup/workflows',
  ApprovalUserSetup: '/erp/setup/approval-users',
  GenJournalTemplate: '/erp/finance/journal-templates',
  GenJournalBatch: '/erp/finance/journal-templates',
  GenJournalLine: '/erp/finance/general-journal',
  GLRegister: '/erp/finance/registers',
  AccountSchedule: '/erp/reports/account-schedules',
  AccountScheduleLine: '/erp/reports/account-schedules',
  ColumnLayout: '/erp/reports/column-layouts',
  ColumnLayoutLine: '/erp/reports/column-layouts',
  SalesHeader: '/erp/sales-invoices',
  PurchaseHeader: '/erp/purchase-invoices',
};

export interface TableLink {
  path: string;
  queryParams?: Record<string, string>;
}

/**
 * Where a worksheet table line leads: the table's own page, or else the import wizard set to the
 * table, which is how every table can be filled even without a page of its own.
 */
export function tablePageLink(entityName: string | null | undefined): TableLink | null {
  if (!entityName) {
    return null;
  }

  const page = TABLE_PAGE_ROUTES[entityName];
  return page ? { path: page } : { path: '/erp/rapid-start/import', queryParams: { entity: entityName } };
}

/** A staged record's errors, per field. Errors that concern no field are keyed by `''`. */
export function errorsByField(errors: ConfigPackageErrorDto[] | null | undefined): Record<string, string[]> {
  const result: Record<string, string[]> = {};

  for (const error of errors ?? []) {
    const key = error.fieldName ?? '';
    (result[key] ??= []).push(error.errorText ?? '');
  }

  return result;
}

/** Package fields in the order they are processed, which is also the column order of the records. */
export function sortFieldsByProcessingOrder<T extends Pick<ConfigPackageFieldDto, 'processingOrder' | 'fieldName'>>(
  fields: T[] | null | undefined,
): T[] {
  return [...(fields ?? [])].sort(
    (a, b) => a.processingOrder - b.processingOrder || (a.fieldName ?? '').localeCompare(b.fieldName ?? ''),
  );
}

/** The first `max` values of one column of the file, blanks left out. */
export function sampleValues(preview: ImportFilePreviewDto | null | undefined, index: number, max = 3): string[] {
  const values: string[] = [];

  for (const row of preview?.sampleRows ?? []) {
    const value = row?.[index];
    if (value != null && value !== '') {
      values.push(value);
      if (values.length >= max) {
        break;
      }
    }
  }

  return values;
}

/** Fields more than one column is mapped to; the service refuses those, so the page says so first. */
export function duplicateMappedFields(columns: ImportColumnDto[]): string[] {
  const seen = new Set<string>();
  const duplicates = new Set<string>();

  for (const column of columns) {
    if (!column.fieldName) {
      continue;
    }
    if (seen.has(column.fieldName)) {
      duplicates.add(column.fieldName);
    }
    seen.add(column.fieldName);
  }

  return Array.from(duplicates);
}

/**
 * Key fields no column is mapped to. The service refuses the import without them, so the page
 * warns before sending. Required non-key fields are not included: a data template may fill them.
 */
export function unmappedKeyFields(fields: ConfigFieldInfoDto[], columns: ImportColumnDto[]): ConfigFieldInfoDto[] {
  const mapped = new Set(columns.map(c => c.fieldName).filter(Boolean));
  return fields.filter(f => f.isKey && !mapped.has(f.name));
}

/** The separator values the import wizard offers; blank lets the service detect it. */
export const SEPARATOR_OPTIONS: { value: string; label: string }[] = [
  { value: '', label: 'Erp::SeparatorAuto' },
  { value: ',', label: 'Erp::SeparatorComma' },
  { value: ';', label: 'Erp::SeparatorSemicolon' },
  { value: '\t', label: 'Erp::SeparatorTab' },
];
