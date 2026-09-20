import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  DataExportService,
  EntityFilterDto,
  EntityFilterOperator,
  ExportFormat,
  ExportTemplateDto,
  ExportableEntityDto,
  ExportableFieldDto,
} from '@proxy/exporting';
import { saveBlob } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

/**
 * Exporting any table the user is allowed to read: pick a table, pick columns, filter, preview,
 * then take it away as CSV, Excel or JSON.
 * <p>
 * Mirrors Odoo's export dialog and the Excel export of a Business Central configuration package.
 * </p>
 */
@Component({
  selector: 'app-data-export',
  templateUrl: './data-export.component.html',
})
export class DataExportComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly toaster = inject(ToasterService);
  private readonly companyService = inject(CompanyService);
  private readonly service = inject(DataExportService);

  readonly ExportFormat = ExportFormat;

  readonly operatorOptions = [
    { value: EntityFilterOperator.Equals, label: 'Erp::OperatorEquals' },
    { value: EntityFilterOperator.NotEquals, label: 'Erp::OperatorNotEquals' },
    { value: EntityFilterOperator.Contains, label: 'Erp::OperatorContains' },
    { value: EntityFilterOperator.StartsWith, label: 'Erp::OperatorStartsWith' },
    { value: EntityFilterOperator.GreaterThan, label: 'Erp::OperatorGreaterThan' },
    { value: EntityFilterOperator.GreaterOrEqual, label: 'Erp::OperatorGreaterOrEqual' },
    { value: EntityFilterOperator.LessThan, label: 'Erp::OperatorLessThan' },
    { value: EntityFilterOperator.LessOrEqual, label: 'Erp::OperatorLessOrEqual' },
  ];

  entities: ExportableEntityDto[] = [];
  fields: ExportableFieldDto[] = [];
  templates: ExportTemplateDto[] = [];

  entityName = '';
  selectedFields = new Set<string>();
  filters: EntityFilterDto[] = [];
  orderBy = '';
  format = ExportFormat.Xlsx;

  previewRows: Record<string, unknown>[] = [];
  previewColumns: string[] = [];
  totalCount = 0;
  busy = false;

  templateName = '';
  shareTemplate = false;

  get canRun(): boolean {
    return !!this.entityName && this.selectedFields.size > 0 && !this.busy;
  }

  ngOnInit(): void {
    this.loadEntities();

    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadEntities());
  }

  onEntityChange(entityName: string): void {
    this.entityName = entityName;
    this.fields = [];
    this.selectedFields.clear();
    this.filters = [];
    this.previewRows = [];
    this.previewColumns = [];
    this.totalCount = 0;

    if (!entityName) {
      return;
    }

    this.service
      .getFields(entityName)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.fields = result.items ?? [];
        this.selectDefaults();
      });

    this.loadTemplates();
  }

  toggleField(name: string): void {
    if (this.selectedFields.has(name)) {
      this.selectedFields.delete(name);
    } else {
      this.selectedFields.add(name);
    }
  }

  selectAll(): void {
    this.selectedFields = new Set(this.fields.map(f => f.name!));
  }

  selectNone(): void {
    this.selectedFields.clear();
  }

  selectDefaults(): void {
    this.selectedFields = new Set(this.fields.filter(f => f.includedByDefault).map(f => f.name!));
  }

  addFilter(): void {
    this.filters.push({
      field: this.fields[0]?.name ?? '',
      operator: EntityFilterOperator.Equals,
      value: '',
    });
  }

  removeFilter(index: number): void {
    this.filters.splice(index, 1);
  }

  /** Values a filter can offer as a list, when the field is an enum. */
  enumValuesOf(fieldName: string): string[] {
    return this.fields.find(f => f.name === fieldName)?.enumValues ?? [];
  }

  preview(): void {
    if (!this.canRun) {
      return;
    }

    this.busy = true;
    this.service
      .getPreview({
        entityName: this.entityName,
        fields: this.orderedFields(),
        filters: this.filters,
        orderBy: this.orderBy || undefined,
        descending: false,
        skipCount: 0,
        maxResultCount: 25,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: result => {
          this.busy = false;
          this.totalCount = result.totalCount;
          this.previewColumns = (result.fields ?? []).map(f => f.name!);
          this.previewRows = (result.items ?? []) as Record<string, unknown>[];
        },
        error: () => (this.busy = false),
      });
  }

  run(): void {
    if (!this.canRun) {
      return;
    }

    this.busy = true;
    this.service
      .runExport({
        entityName: this.entityName,
        fields: this.orderedFields(),
        filters: this.filters,
        orderBy: this.orderBy || undefined,
        descending: false,
        format: this.format,
        maxResultCount: 50000,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: blob => {
          this.busy = false;
          saveBlob(blob, `${this.entityName}.${this.extension()}`);
        },
        error: () => (this.busy = false),
      });
  }

  saveTemplate(): void {
    const name = this.templateName.trim();
    if (!name || this.selectedFields.size === 0) {
      return;
    }

    this.service
      .createTemplate({
        name,
        entityName: this.entityName,
        fields: this.orderedFields(),
        format: this.format,
        isShared: this.shareTemplate,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.templateName = '';
        this.toaster.success('Erp::SavedSuccessfully');
        this.loadTemplates();
      });
  }

  applyTemplate(template: ExportTemplateDto): void {
    this.selectedFields = new Set(template.fields ?? []);
    this.format = template.format;
  }

  deleteTemplate(template: ExportTemplateDto): void {
    this.service
      .deleteTemplate(template.id!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadTemplates());
  }

  /** Keeps the column order of the table rather than the order they happened to be ticked in. */
  private orderedFields(): string[] {
    return this.fields.filter(f => this.selectedFields.has(f.name!)).map(f => f.name!);
  }

  private extension(): string {
    switch (this.format) {
      case ExportFormat.Csv:
        return 'csv';
      case ExportFormat.Json:
        return 'json';
      default:
        return 'xlsx';
    }
  }

  private loadEntities(): void {
    this.service
      .getEntities()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.entities = result.items ?? []));
  }

  private loadTemplates(): void {
    this.service
      .getTemplates({ entityName: this.entityName })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.templates = result.items ?? []));
  }
}
