import { ConfigLineStatus, ConfigLineType, ConfigTableInfoDto } from '@proxy/rapid-start';
import {
  configLineIndent,
  configLineStatusClass,
  duplicateMappedFields,
  errorsByField,
  groupTablesByArea,
  sampleValues,
  sortFieldsByProcessingOrder,
  tablePageLink,
  unmappedKeyFields,
} from './rapid-start.helpers';

function table(entityName: string, area: string, displayName = entityName): ConfigTableInfoDto {
  return { entityName, displayName, area, keyFields: [], relatedTables: [], canWrite: true };
}

describe('groupTablesByArea', () => {
  it('groups by area and sorts areas and tables alphabetically', () => {
    const groups = groupTablesByArea([
      table('Vendor', 'Purchasing'),
      table('Item', 'Inventory'),
      table('Customer', 'Sales'),
      table('ItemCategory', 'Inventory', 'Item Category'),
      table('CustomerPostingGroup', 'Sales', 'Customer Posting Group'),
    ]);

    expect(groups.map(g => g.area)).toEqual(['Inventory', 'Purchasing', 'Sales']);
    expect(groups[0].tables.map(t => t.displayName)).toEqual(['Item', 'Item Category']);
    expect(groups[2].tables.map(t => t.entityName)).toEqual(['Customer', 'CustomerPostingGroup']);
  });

  it('puts tables without an area in one group of their own', () => {
    const groups = groupTablesByArea([table('Company', ''), table('Item', 'Inventory')]);

    expect(groups.map(g => g.area)).toEqual(['', 'Inventory']);
  });

  it('returns nothing for no tables', () => {
    expect(groupTablesByArea([])).toEqual([]);
  });
});

describe('worksheet presentation', () => {
  it('indents areas, groups and tables by their level', () => {
    expect(configLineIndent(ConfigLineType.Area)).toBe(0);
    expect(configLineIndent(ConfigLineType.Group)).toBe(1);
    expect(configLineIndent(ConfigLineType.Table)).toBe(2);
  });

  it('colours each status differently', () => {
    const classes = [
      ConfigLineStatus.NotStarted,
      ConfigLineStatus.InProgress,
      ConfigLineStatus.Completed,
      ConfigLineStatus.Ignored,
      ConfigLineStatus.Blocked,
    ].map(configLineStatusClass);

    expect(new Set(classes).size).toBe(5);
    expect(configLineStatusClass(ConfigLineStatus.Completed)).toContain('bg-success');
    expect(configLineStatusClass(ConfigLineStatus.Blocked)).toContain('bg-danger');
  });
});

describe('tablePageLink', () => {
  it('leads to the page a table is set up on', () => {
    expect(tablePageLink('Customer')).toEqual({ path: '/erp/customers' });
    expect(tablePageLink('GLAccount')).toEqual({ path: '/erp/chart-of-accounts' });
    expect(tablePageLink('NoSeries')).toEqual({ path: '/erp/setup/no-series' });
  });

  it('falls back to the import wizard set to the table', () => {
    expect(tablePageLink('Dimension')).toEqual({
      path: '/erp/rapid-start/import',
      queryParams: { entity: 'Dimension' },
    });
  });

  it('has no link without a table', () => {
    expect(tablePageLink(undefined)).toBeNull();
    expect(tablePageLink('')).toBeNull();
  });
});

describe('errorsByField', () => {
  it('groups a record errors per field, record level errors under an empty key', () => {
    const result = errorsByField([
      { fieldName: 'No', errorText: 'Required', recordNo: 1 },
      { fieldName: 'No', errorText: 'Too long', recordNo: 1 },
      { fieldName: 'CustomerPostingGroup', errorText: 'Not found', recordNo: 1 },
      { errorText: 'Record failed', recordNo: 1 },
    ]);

    expect(result).toEqual({
      No: ['Required', 'Too long'],
      CustomerPostingGroup: ['Not found'],
      '': ['Record failed'],
    });
  });

  it('is empty without errors', () => {
    expect(errorsByField(undefined)).toEqual({});
  });
});

describe('sortFieldsByProcessingOrder', () => {
  it('orders by processing order, then by name, without changing the input', () => {
    const fields = [
      { fieldName: 'Name', processingOrder: 2 },
      { fieldName: 'No', processingOrder: 1 },
      { fieldName: 'City', processingOrder: 2 },
    ];

    expect(sortFieldsByProcessingOrder(fields).map(f => f.fieldName)).toEqual(['No', 'City', 'Name']);
    expect(fields[0].fieldName).toBe('Name');
  });
});

describe('import mapping helpers', () => {
  const preview = {
    sheets: [],
    columns: [],
    totalRows: 4,
    sampleRows: [
      ['C001', 'Adatum', ''],
      ['C002', '', 'Nairobi'],
      ['C003', 'Contoso', 'Mombasa'],
      ['C004', 'Fabrikam', 'Kisumu'],
    ],
  };

  it('takes the first non-blank sample values of a column', () => {
    expect(sampleValues(preview, 0)).toEqual(['C001', 'C002', 'C003']);
    expect(sampleValues(preview, 1, 2)).toEqual(['Adatum', 'Contoso']);
    expect(sampleValues(preview, 2)).toEqual(['Nairobi', 'Mombasa', 'Kisumu']);
    expect(sampleValues(preview, 9)).toEqual([]);
    expect(sampleValues(null, 0)).toEqual([]);
  });

  it('finds fields more than one column is mapped to', () => {
    expect(
      duplicateMappedFields([
        { index: 0, fieldName: 'No' },
        { index: 1, fieldName: 'Name' },
        { index: 2, fieldName: 'No' },
        { index: 3, fieldName: '' },
        { index: 4, fieldName: '' },
      ]),
    ).toEqual(['No']);
  });

  it('lists key fields no column is mapped to', () => {
    const fields = [
      { name: 'No', isKey: true, isRequired: true, enumValues: [] },
      { name: 'Name', isKey: false, isRequired: true, enumValues: [] },
      { name: 'Code', isKey: true, isRequired: true, enumValues: [] },
    ];

    const missing = unmappedKeyFields(fields, [
      { index: 0, fieldName: 'No' },
      { index: 1, fieldName: '' },
    ]);

    expect(missing.map(f => f.name)).toEqual(['Code']);
  });
});
