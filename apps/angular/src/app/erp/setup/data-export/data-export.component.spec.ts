import { TestBed } from '@angular/core/testing';
import { ToasterService } from '@abp/ng.theme.shared';
import { DataExportService, EntityFilterOperator, ExportFormat } from '@proxy/exporting';
import { of } from 'rxjs';
import { CompanyService } from '../../services/company.service';
import { DataExportComponent } from './data-export.component';

describe('DataExportComponent', () => {
  const fields = [
    {
      name: 'No',
      displayName: 'No',
      dataType: 'string',
      includedByDefault: true,
      enumValues: null,
    },
    {
      name: 'Name',
      displayName: 'Name',
      dataType: 'string',
      includedByDefault: true,
      enumValues: null,
    },
    {
      name: 'Type',
      displayName: 'Type',
      dataType: 'enum',
      includedByDefault: true,
      enumValues: ['Inventory', 'Service'],
    },
    {
      name: 'CreationTime',
      displayName: 'Creation Time',
      dataType: 'date',
      includedByDefault: false,
      enumValues: null,
    },
  ];

  let service: jasmine.SpyObj<DataExportService>;
  let component: DataExportComponent;

  beforeEach(() => {
    service = jasmine.createSpyObj<DataExportService>('DataExportService', [
      'getEntities',
      'getFields',
      'getPreview',
      'runExport',
      'getTemplates',
      'createTemplate',
      'deleteTemplate',
    ]);

    service.getEntities.and.returnValue(
      of({ items: [{ name: 'Item', displayName: 'Item' }] }) as never,
    );
    service.getFields.and.returnValue(of({ items: fields }) as never);
    service.getTemplates.and.returnValue(of({ items: [] }) as never);
    service.getPreview.and.returnValue(
      of({ totalCount: 3, fields, items: [{ No: '1000', Name: 'Bicycle' }] }) as never,
    );
    service.runExport.and.returnValue(of(new Blob(['x'])) as never);
    service.createTemplate.and.returnValue(of({ id: 't1' }) as never);

    TestBed.configureTestingModule({
      providers: [
        DataExportComponent,
        { provide: DataExportService, useValue: service },
        { provide: CompanyService, useValue: { companyChanged$: of() } },
        { provide: ToasterService, useValue: jasmine.createSpyObj('ToasterService', ['success']) },
      ],
    });

    component = TestBed.inject(DataExportComponent);
    component.ngOnInit();
  });

  /** Picking a table offers its standard columns, the way Odoo's export dialog opens. */
  it('selects the standard columns when a table is chosen', () => {
    component.onEntityChange('Item');

    expect(component.fields.length).toBe(4);
    expect([...component.selectedFields]).toEqual(['No', 'Name', 'Type']);
  });

  it('can select every column or none', () => {
    component.onEntityChange('Item');

    component.selectAll();
    expect(component.selectedFields.size).toBe(4);

    component.selectNone();
    expect(component.selectedFields.size).toBe(0);
    expect(component.canRun).toBeFalse();
  });

  it('toggles a single column', () => {
    component.onEntityChange('Item');

    component.toggleField('CreationTime');
    expect(component.selectedFields.has('CreationTime')).toBeTrue();

    component.toggleField('CreationTime');
    expect(component.selectedFields.has('CreationTime')).toBeFalse();
  });

  /**
   * Columns go out in the table's own order, not the order they happened to be ticked in:
   * a file whose columns move about between exports is not usable downstream.
   */
  it('exports columns in the order of the table', () => {
    component.onEntityChange('Item');
    component.selectNone();
    component.toggleField('Type');
    component.toggleField('No');

    component.run();

    const input = service.runExport.calls.mostRecent().args[0] as { fields: string[] };
    expect(input.fields).toEqual(['No', 'Type']);
  });

  it('offers the values of an enum field as a list', () => {
    component.onEntityChange('Item');

    expect(component.enumValuesOf('Type')).toEqual(['Inventory', 'Service']);
    expect(component.enumValuesOf('Name')).toEqual([]);
  });

  it('adds and removes filters', () => {
    component.onEntityChange('Item');

    component.addFilter();
    expect(component.filters.length).toBe(1);
    expect(component.filters[0].field).toBe('No');
    expect(component.filters[0].operator).toBe(EntityFilterOperator.Equals);

    component.removeFilter(0);
    expect(component.filters.length).toBe(0);
  });

  it('shows how many rows the export would contain', () => {
    component.onEntityChange('Item');

    component.preview();

    expect(component.totalCount).toBe(3);
    expect(component.previewColumns).toEqual(['No', 'Name', 'Type', 'CreationTime']);
    expect(component.previewRows[0]['Name']).toBe('Bicycle');
  });

  it('will not run without a table and at least one column', () => {
    expect(component.canRun).toBeFalse();

    component.run();
    expect(service.runExport).not.toHaveBeenCalled();
  });

  it('saves the chosen columns as a template', () => {
    component.onEntityChange('Item');
    component.format = ExportFormat.Csv;
    component.templateName = 'Item list';

    component.saveTemplate();

    const input = service.createTemplate.calls.mostRecent().args[0] as {
      name: string;
      entityName: string;
      fields: string[];
      format: ExportFormat;
    };
    expect(input.name).toBe('Item list');
    expect(input.entityName).toBe('Item');
    expect(input.fields).toEqual(['No', 'Name', 'Type']);
    expect(input.format).toBe(ExportFormat.Csv);
  });

  it('applies a saved template over the current selection', () => {
    component.onEntityChange('Item');

    component.applyTemplate({
      id: 't1',
      name: 'Names',
      fields: ['Name'],
      format: ExportFormat.Json,
    } as never);

    expect([...component.selectedFields]).toEqual(['Name']);
    expect(component.format).toBe(ExportFormat.Json);
  });
});
