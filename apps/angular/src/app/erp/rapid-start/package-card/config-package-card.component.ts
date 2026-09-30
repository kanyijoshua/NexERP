import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { EntityFilterDto, EntityFilterOperator } from '@proxy/exporting';
import {
  ConfigFieldInfoDto,
  ConfigFieldMappingDto,
  ConfigPackageDetailDto,
  ConfigPackageErrorDto,
  ConfigPackageFileFormat,
  ConfigPackageRunResultDto,
  ConfigPackageService,
  ConfigPackageTableDto,
  ConfigTableInfoDto,
  ConfigTemplateDto,
  ConfigTemplateService,
} from '@proxy/rapid-start';
import { Observable } from 'rxjs';
import { finalize, switchMap } from 'rxjs/operators';
import { ErpFactBoxGroup, readUploadFile, saveBlob } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';
import { MAX_IMPORT_FILE_BYTES, TableArea, groupTablesByArea, sortFieldsByProcessingOrder } from '../rapid-start.helpers';

/** A package field as the fields dialog edits it. */
interface EditableField {
  fieldName: string;
  displayName: string;
  dataType: string;
  primaryKey: boolean;
  relatedTable?: string;
  includeField: boolean;
  validateField: boolean;
  processingOrder: number;
  mappings: ConfigFieldMappingDto[];
  showMappings: boolean;
}

interface RunSummary {
  kind: 'validate' | 'apply';
  result: ConfigPackageRunResultDto;
}

/**
 * One configuration package. Mirrors Business Central page 8614 "Config. Package Card" with its
 * "Config. Package Subform" and the Config. Package Fields page.
 */
@Component({
    selector: 'app-config-package-card',
    templateUrl: './config-package-card.component.html',
    standalone: false
})
export class ConfigPackageCardComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly service = inject(ConfigPackageService);
  private readonly templateService = inject(ConfigTemplateService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly route = inject(ActivatedRoute);

  readonly canManage = inject(PermissionService).getGrantedPolicy('Erp.RapidStart.Manage');

  readonly ConfigPackageFileFormat = ConfigPackageFileFormat;

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

  packageId = '';
  pkg: ConfigPackageDetailDto | null = null;
  loading = false;
  showFactBox = true;

  get factBoxGroups(): ErpFactBoxGroup[] {
    if (!this.pkg) return [];
    const totalRecords = this.tables.reduce((sum, t) => sum + (t.noOfRecords || 0), 0);
    return [
      {
        id: 'statistics',
        titleKey: 'Erp::Statistics',
        icon: 'fas fa-chart-pie',
        tiles: [
          {
            titleKey: 'Erp::NoOfTables',
            value: this.tables.length,
            icon: 'fas fa-table',
            color: 'primary',
          },
          {
            titleKey: 'Erp::NoOfRecords',
            value: totalRecords,
            icon: 'fas fa-database',
            color: 'info',
          },
          {
            titleKey: 'Erp::NoOfErrors',
            value: this.pkg.noOfErrors || 0,
            icon: 'fas fa-triangle-exclamation',
            color: (this.pkg.noOfErrors || 0) > 0 ? 'danger' : 'success',
            action: () => this.showErrors(),
          },
        ],
      },
      {
        id: 'details',
        titleKey: 'Erp::Details',
        icon: 'fas fa-circle-info',
        facts: [
          { labelKey: 'Erp::Code', value: this.pkg.code, type: 'text' },
          { labelKey: 'Erp::ProductVersion', value: this.pkg.productVersion || '—', type: 'text' },
          { labelKey: 'Erp::LastImported', value: this.pkg.lastImportedTime, type: 'date' },
          { labelKey: 'Erp::LastApplied', value: this.pkg.lastAppliedTime, type: 'date' },
        ],
      },
    ];
  }
  busy = false;

  packageName = '';
  productVersion = '';

  /** Tables the next action applies to. None ticked means all of them. */
  selected = new Set<string>();

  runSummary: RunSummary | null = null;
  errors: ConfigPackageErrorDto[] | null = null;

  // "Add tables" dialog.
  isAddOpen = false;
  catalogAreas: TableArea[] = [];
  catalogFilter = '';
  toInclude = new Set<string>();
  includeRelated = true;

  // Table settings dialog.
  isTableOpen = false;
  tableTab = 1;
  editTable: ConfigPackageTableDto | null = null;
  editProcessingOrder = 0;
  editDeleteBefore = false;
  editTemplateCode = '';
  editFilters: EntityFilterDto[] = [];
  editFields: EditableField[] = [];
  tableFieldInfo: ConfigFieldInfoDto[] = [];
  tableTemplates: ConfigTemplateDto[] = [];

  get tables(): ConfigPackageTableDto[] {
    return this.pkg?.tables ?? [];
  }

  get allSelected(): boolean {
    return this.tables.length > 0 && this.tables.every(t => this.selected.has(t.id!));
  }

  get headerChanged(): boolean {
    return (
      !!this.pkg &&
      (this.packageName.trim() !== (this.pkg.packageName ?? '') ||
        this.productVersion.trim() !== (this.pkg.productVersion ?? ''))
    );
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      this.packageId = params.get('id') ?? '';
      this.selected.clear();
      this.runSummary = null;
      this.errors = null;
      this.load();
    });

    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  load(): void {
    if (!this.packageId) {
      return;
    }

    this.loading = true;
    this.service
      .get(this.packageId)
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(pkg => this.setPackage(pkg));
  }

  // ----- header -----

  saveHeader(): void {
    if (!this.pkg || !this.packageName.trim()) {
      return;
    }

    this.run(
      this.service.update(this.packageId, {
        packageName: this.packageName.trim(),
        productVersion: this.productVersion.trim() || undefined,
      }),
    ).subscribe(pkg => {
      this.setPackage(pkg);
      this.toaster.success('Erp::SavedSuccessfully');
    });
  }

  // ----- selection -----

  toggle(table: ConfigPackageTableDto): void {
    if (this.selected.has(table.id!)) {
      this.selected.delete(table.id!);
    } else {
      this.selected.add(table.id!);
    }
  }

  toggleAll(): void {
    this.selected = this.allSelected ? new Set() : new Set(this.tables.map(t => t.id!));
  }

  // ----- adding and removing tables -----

  openAddTables(): void {
    this.toInclude.clear();
    this.catalogFilter = '';
    this.includeRelated = true;
    this.isAddOpen = true;

    this.service
      .getTableCatalog()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.catalogAreas = groupTablesByArea(result.items ?? [])));
  }

  isInPackage(table: ConfigTableInfoDto): boolean {
    return this.tables.some(t => t.entityName === table.entityName);
  }

  matchesCatalogFilter(table: ConfigTableInfoDto): boolean {
    const term = this.catalogFilter.trim().toLowerCase();
    return (
      !term ||
      (table.displayName ?? '').toLowerCase().includes(term) ||
      (table.entityName ?? '').toLowerCase().includes(term)
    );
  }

  areaHasMatches(area: TableArea): boolean {
    return area.tables.some(t => this.matchesCatalogFilter(t));
  }

  toggleInclude(entityName: string): void {
    if (this.toInclude.has(entityName)) {
      this.toInclude.delete(entityName);
    } else {
      this.toInclude.add(entityName);
    }
  }

  includeTables(): void {
    if (!this.toInclude.size) {
      return;
    }

    this.run(
      this.service.includeTables(this.packageId, {
        entityNames: Array.from(this.toInclude),
        includeRelatedTables: this.includeRelated,
      }),
    ).subscribe(pkg => {
      this.isAddOpen = false;
      this.setPackage(pkg);
      this.toaster.success('Erp::SavedSuccessfully');
    });
  }

  excludeTable(table: ConfigPackageTableDto): void {
    this.confirmation
      .warn('Erp::ExcludeTableConfirmation', 'Erp::AreYouSure', {
        messageLocalizationParams: [table.displayName ?? table.entityName ?? ''],
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }
        this.run(this.service.excludeTable(this.packageId, { tableId: table.id })).subscribe(pkg => {
          this.selected.delete(table.id!);
          this.setPackage(pkg);
        });
      });
  }

  // ----- table settings -----

  openTable(table: ConfigPackageTableDto, tab = 1): void {
    this.editTable = table;
    this.tableTab = tab;
    this.editProcessingOrder = table.processingOrder;
    this.editDeleteBefore = table.deleteRecordsBeforeProcessing;
    this.editTemplateCode = table.dataTemplateCode ?? '';
    this.editFilters = (table.filters ?? []).map(f => ({ ...f }));
    this.editFields = sortFieldsByProcessingOrder(table.fields).map(f => ({
      fieldName: f.fieldName ?? '',
      displayName: f.displayName || f.fieldName || '',
      dataType: f.dataType ?? '',
      primaryKey: f.primaryKey,
      relatedTable: f.relatedTable,
      includeField: f.includeField,
      validateField: f.validateField,
      processingOrder: f.processingOrder,
      mappings: (f.mappings ?? []).map(m => ({ ...m })),
      showMappings: false,
    }));
    this.tableFieldInfo = [];
    this.tableTemplates = [];
    this.isTableOpen = true;

    const entityName = table.entityName ?? '';
    this.service
      .getTableFields(entityName)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.tableFieldInfo = result.items ?? []));
    this.templateService
      .getList({ entityName })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.tableTemplates = result.items ?? []));
  }

  addFilter(): void {
    this.editFilters.push({
      field: this.editFields[0]?.fieldName ?? '',
      operator: EntityFilterOperator.Equals,
      value: '',
    });
  }

  removeFilter(index: number): void {
    this.editFilters.splice(index, 1);
  }

  /** Values a filter or a mapping can offer as a list, when the field is an enum. */
  enumValuesOf(fieldName: string): string[] {
    return this.tableFieldInfo.find(f => f.name === fieldName)?.enumValues ?? [];
  }

  setAllIncluded(included: boolean): void {
    // A key field is always carried: without it a record cannot be found or created.
    this.editFields.forEach(f => (f.includeField = f.primaryKey || included));
  }

  addMapping(field: EditableField): void {
    field.mappings.push({ oldValue: '', newValue: '' });
    field.showMappings = true;
  }

  removeMapping(field: EditableField, index: number): void {
    field.mappings.splice(index, 1);
  }

  saveTable(): void {
    const table = this.editTable;
    if (!table) {
      return;
    }

    this.run(
      this.service.updateTable(this.packageId, {
        tableId: table.id,
        processingOrder: Number(this.editProcessingOrder) || 0,
        deleteRecordsBeforeProcessing: this.editDeleteBefore,
        dataTemplateCode: this.editTemplateCode || undefined,
        filters: this.editFilters.filter(f => !!f.field),
        fields: this.editFields.map(f => ({
          fieldName: f.fieldName,
          includeField: f.includeField,
          validateField: f.validateField,
          processingOrder: Number(f.processingOrder) || 0,
          mappings: f.mappings.filter(m => (m.oldValue ?? '') !== '' || (m.newValue ?? '') !== ''),
        })),
      }),
    ).subscribe(pkg => {
      this.isTableOpen = false;
      this.setPackage(pkg);
      this.toaster.success('Erp::SavedSuccessfully');
    });
  }

  // ----- package actions -----

  fillFromDatabase(): void {
    this.confirmThen('Erp::GetDataFromDatabaseConfirmation', () =>
      this.run(this.service.fillFromDatabase(this.packageId, this.target())).subscribe(pkg => {
        this.setPackage(pkg);
        this.toaster.success('Erp::DataStaged', undefined, {
          messageLocalizationParams: [String(pkg.noOfRecords)],
        });
      }),
    );
  }

  importFromExcel(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) {
      return;
    }
    if (file.size > MAX_IMPORT_FILE_BYTES) {
      this.toaster.error('Erp::FileTooLargeToUpload', undefined, {
        messageLocalizationParams: [String(MAX_IMPORT_FILE_BYTES / 1024 / 1024)],
      });
      return;
    }

    this.run(
      readUploadFile(file).pipe(switchMap(upload => this.service.importData(this.packageId, upload))),
    ).subscribe(result => {
      this.toaster.success('Erp::DataStaged', undefined, {
        messageLocalizationParams: [String(result.noOfRecords)],
      });
      this.load();
    });
  }

  exportPackage(format: ConfigPackageFileFormat): void {
    this.run(this.service.exportPackage(this.packageId, { format })).subscribe(blob =>
      saveBlob(blob, `${this.pkg?.code ?? 'package'}.${format === ConfigPackageFileFormat.Json ? 'json' : 'xlsx'}`),
    );
  }

  validatePackage(): void {
    this.run(this.service.validatePackage(this.packageId, this.target())).subscribe(result =>
      this.showRunResult('validate', result),
    );
  }

  applyPackage(): void {
    this.confirmThen('Erp::ApplyPackageConfirmation', () =>
      this.run(this.service.applyPackage(this.packageId, this.target())).subscribe(result =>
        this.showRunResult('apply', result),
      ),
    );
  }

  clearPackageData(): void {
    this.confirmThen('Erp::ClearPackageDataConfirmation', () =>
      this.run(this.service.clearPackageData(this.packageId, this.target())).subscribe(pkg => {
        this.setPackage(pkg);
        this.errors = null;
        this.toaster.success('Erp::PackageDataCleared');
      }),
    );
  }

  showErrors(): void {
    this.run(this.service.getErrors(this.packageId)).subscribe(result => (this.errors = result.items ?? []));
  }

  tableName(result: { tableId?: string; entityName?: string }): string {
    const table = this.tables.find(t => t.id === result.tableId);
    return table?.displayName || result.entityName || '';
  }

  private showRunResult(kind: 'validate' | 'apply', result: ConfigPackageRunResultDto): void {
    this.runSummary = { kind, result };
    const params = [String(result.inserted), String(result.modified), String(result.errors)];
    const key = kind === 'validate' ? 'Erp::PackageValidatedSummary' : 'Erp::PackageAppliedSummary';

    if (result.errors > 0) {
      this.toaster.warn(key, undefined, { messageLocalizationParams: params });
    } else {
      this.toaster.success(key, undefined, { messageLocalizationParams: params });
    }

    this.errors = null;
    this.load();
  }

  private target(): { tableIds: string[] } {
    return { tableIds: Array.from(this.selected) };
  }

  private setPackage(pkg: ConfigPackageDetailDto): void {
    this.pkg = pkg;
    this.packageName = pkg.packageName ?? '';
    this.productVersion = pkg.productVersion ?? '';

    const ids = new Set((pkg.tables ?? []).map(t => t.id));
    this.selected = new Set(Array.from(this.selected).filter(id => ids.has(id)));
  }

  private confirmThen(messageKey: string, action: () => void): void {
    const targetParam =
      this.selected.size > 0 ? String(this.selected.size) : String(this.tables.length);

    this.confirmation
      .warn(messageKey, 'Erp::AreYouSure', { messageLocalizationParams: [targetParam] })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          action();
        }
      });
  }

  /** Runs one call with the page marked busy. HTTP errors are left to ABP's error handler. */
  private run<T>(request$: Observable<T>): Observable<T> {
    this.busy = true;
    return request$.pipe(
      finalize(() => (this.busy = false)),
      takeUntilDestroyed(this.destroyRef),
    );
  }
}
