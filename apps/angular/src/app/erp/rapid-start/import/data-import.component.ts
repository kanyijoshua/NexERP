import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import {
  ConfigFieldInfoDto,
  ConfigPackageService,
  ConfigTableInfoDto,
  ConfigTemplateDto,
  ConfigTemplateService,
  DataImportService,
  ImportColumnDto,
  ImportFileInput,
  ImportFilePreviewDto,
  ImportResultDto,
  RunImportInput,
} from '@proxy/rapid-start';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { Base64File, readUploadFile, saveBlob } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';
import {
  MAX_IMPORT_FILE_BYTES,
  SEPARATOR_OPTIONS,
  TableArea,
  duplicateMappedFields,
  groupTablesByArea,
  sampleValues,
  unmappedKeyFields,
} from '../rapid-start.helpers';

/**
 * The import wizard: upload a CSV or Excel file, match each column
 * to a field, test, then import. The import is all or nothing: one row in error and nothing is written.
 */
@Component({
    selector: 'app-data-import',
    templateUrl: './data-import.component.html',
    standalone: false
})
export class DataImportComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly service = inject(DataImportService);
  private readonly packageService = inject(ConfigPackageService);
  private readonly templateService = inject(ConfigTemplateService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly route = inject(ActivatedRoute);

  readonly separators = SEPARATOR_OPTIONS;

  entities: ConfigTableInfoDto[] = [];
  entityAreas: TableArea[] = [];
  entityName = '';
  fields: ConfigFieldInfoDto[] = [];
  templates: ConfigTemplateDto[] = [];

  upload: Base64File | null = null;
  sheetName = '';
  separator = '';
  hasHeaders = true;

  preview: ImportFilePreviewDto | null = null;
  columns: ImportColumnDto[] = [];
  dataTemplateCode = '';

  result: ImportResultDto | null = null;
  busy = false;

  private requestedEntity = '';

  get step(): 1 | 2 {
    return this.preview ? 2 : 1;
  }

  get isCsv(): boolean {
    return !!this.upload && !this.upload.fileName.toLowerCase().endsWith('.xlsx');
  }

  get duplicates(): string[] {
    return duplicateMappedFields(this.columns);
  }

  get missingKeys(): ConfigFieldInfoDto[] {
    return unmappedKeyFields(this.fields, this.columns);
  }

  get missingKeyNames(): string {
    return this.missingKeys.map(f => f.displayName || f.name).join(', ');
  }

  get mappedCount(): number {
    return this.columns.filter(c => !!c.fieldName).length;
  }

  get canRun(): boolean {
    return !!this.preview && this.mappedCount > 0 && this.duplicates.length === 0 && !this.busy;
  }

  ngOnInit(): void {
    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(query => {
      this.requestedEntity = query.get('entity') ?? '';
      this.applyRequestedEntity();
    });

    this.loadEntities();
    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadEntities());
  }

  onEntityChange(entityName: string): void {
    this.entityName = entityName;
    this.fields = [];
    this.templates = [];
    this.dataTemplateCode = '';
    this.result = null;

    if (!entityName) {
      this.preview = null;
      this.columns = [];
      return;
    }

    this.packageService
      .getTableFields(entityName)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.fields = result.items ?? []));
    this.templateService
      .getList({ entityName })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.templates = (result.items ?? []).filter(t => t.enabled)));

    // The columns are matched to the table's fields, so a file already read is matched again.
    if (this.preview) {
      this.parse();
    }
  }

  onFileChange(event: Event): void {
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

    this.busy = true;
    readUploadFile(file)
      .pipe(
        finalize(() => (this.busy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(upload => {
        this.upload = upload;
        this.sheetName = '';
        this.preview = null;
        this.columns = [];
        this.result = null;
        if (this.entityName) {
          this.parse();
        }
      });
  }

  /** Reads the file with the current options and takes the service's proposed field per column. */
  parse(): void {
    if (!this.upload || !this.entityName) {
      return;
    }

    this.result = null;
    this.run(this.service.parseFile(this.fileInput())).subscribe(preview => {
      this.preview = preview;
      this.sheetName = preview.sheetName ?? '';
      this.columns = (preview.columns ?? []).map(c => ({ ...c, fieldName: c.fieldName ?? '' }));
    });
  }

  samples(column: ImportColumnDto): string[] {
    return sampleValues(this.preview, column.index);
  }

  fieldOf(name: string | undefined): ConfigFieldInfoDto | undefined {
    return name ? this.fields.find(f => f.name === name) : undefined;
  }

  isDuplicate(column: ImportColumnDto): boolean {
    return !!column.fieldName && this.duplicates.includes(column.fieldName);
  }

  downloadTemplate(): void {
    if (!this.entityName) {
      return;
    }
    this.run(this.service.getTemplateFile(this.entityName)).subscribe(blob =>
      saveBlob(blob, `${this.entityName}.xlsx`),
    );
  }

  testImport(): void {
    if (!this.canRun) {
      return;
    }
    this.run(this.service.testImport(this.runInput())).subscribe(result => (this.result = result));
  }

  runImport(): void {
    if (!this.canRun) {
      return;
    }

    this.confirmation
      .warn('Erp::RunImportConfirmation', 'Erp::AreYouSure', {
        messageLocalizationParams: [String(this.preview?.totalRows ?? 0)],
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }
        this.run(this.service.runImport(this.runInput())).subscribe(result => {
          this.result = result;
          if (result.succeeded && !result.dryRun) {
            this.toaster.success('Erp::ImportSucceeded', undefined, {
              messageLocalizationParams: [String(result.inserted), String(result.modified)],
            });
          }
        });
      });
  }

  startOver(): void {
    this.upload = null;
    this.preview = null;
    this.columns = [];
    this.result = null;
    this.sheetName = '';
  }

  private fileInput(): ImportFileInput {
    return {
      fileName: this.upload!.fileName,
      contentBase64: this.upload!.contentBase64,
      sheetName: this.sheetName || undefined,
      separator: this.separator || undefined,
      hasHeaders: this.hasHeaders,
      entityName: this.entityName,
    };
  }

  private runInput(): RunImportInput {
    return {
      ...this.fileInput(),
      columns: this.columns.map(c => ({ index: c.index, header: c.header, fieldName: c.fieldName || undefined })),
      dataTemplateCode: this.dataTemplateCode || undefined,
    };
  }

  private loadEntities(): void {
    this.service
      .getEntities()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.entities = (result.items ?? []).filter(e => e.canWrite);
        this.entityAreas = groupTablesByArea(this.entities);
        this.applyRequestedEntity();
      });
  }

  private applyRequestedEntity(): void {
    const wanted = this.requestedEntity;
    if (!wanted || wanted === this.entityName) {
      return;
    }

    const match = this.entities.find(e => e.entityName?.toLowerCase() === wanted.toLowerCase());
    if (match?.entityName) {
      this.onEntityChange(match.entityName);
    }
  }

  private run<T>(request$: Observable<T>): Observable<T> {
    this.busy = true;
    return request$.pipe(
      finalize(() => (this.busy = false)),
      takeUntilDestroyed(this.destroyRef),
    );
  }
}
