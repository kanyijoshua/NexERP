import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import {
  ConfigFieldInfoDto,
  ConfigPackageDetailDto,
  ConfigPackageFieldDto,
  ConfigPackageRecordDto,
  ConfigPackageService,
  ConfigPackageTableDto,
} from '@proxy/rapid-start';
import { combineLatest } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { CompanyService } from '../../services/company.service';
import { errorsByField, sortFieldsByProcessingOrder } from '../rapid-start.helpers';

export const RECORDS_PAGE_SIZE = 25;

/**
 * The staged records of one package table: the "Config. Package Records"
 * page, where data brought in is checked and corrected before it is applied.
 */
@Component({
    selector: 'app-config-package-records',
    templateUrl: './config-package-records.component.html',
    standalone: false
})
export class ConfigPackageRecordsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly service = inject(ConfigPackageService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly route = inject(ActivatedRoute);

  readonly pageSize = RECORDS_PAGE_SIZE;

  packageId = '';
  tableId = '';
  pkg: ConfigPackageDetailDto | null = null;
  table: ConfigPackageTableDto | null = null;
  columns: ConfigPackageFieldDto[] = [];
  fieldInfo: ConfigFieldInfoDto[] = [];

  records: ConfigPackageRecordDto[] = [];
  totalCount = 0;
  page = 0;
  errorsOnly = false;
  loading = false;

  /** Errors of each shown record, per field; `''` holds the ones that concern no field. */
  errorMap: Record<string, Record<string, string[]>> = {};

  isEditOpen = false;
  isBusy = false;
  editing: ConfigPackageRecordDto | null = null;
  editValues: Record<string, string> = {};

  get firstRow(): number {
    return this.totalCount ? this.page * this.pageSize + 1 : 0;
  }

  get lastRow(): number {
    return Math.min((this.page + 1) * this.pageSize, this.totalCount);
  }

  get hasNext(): boolean {
    return (this.page + 1) * this.pageSize < this.totalCount;
  }

  ngOnInit(): void {
    combineLatest([this.route.paramMap, this.route.queryParamMap])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(([params, query]) => {
        this.packageId = params.get('id') ?? '';
        this.tableId = params.get('tableId') ?? '';
        this.errorsOnly = query.get('errorsOnly') === 'true';
        this.page = 0;
        this.loadPackage();
      });

    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadPackage());
  }

  loadPackage(): void {
    this.service
      .get(this.packageId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(pkg => {
        this.pkg = pkg;
        this.table = (pkg.tables ?? []).find(t => t.id === this.tableId) ?? null;
        this.columns = sortFieldsByProcessingOrder(this.table?.fields).filter(f => f.includeField);

        if (this.table?.entityName) {
          this.service
            .getTableFields(this.table.entityName)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(result => (this.fieldInfo = result.items ?? []));
        }

        this.loadRecords();
      });
  }

  loadRecords(): void {
    this.loading = true;
    this.service
      .getRecords({
        packageId: this.packageId,
        tableId: this.tableId,
        errorsOnly: this.errorsOnly,
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => {
        this.records = result.items ?? [];
        this.totalCount = result.totalCount ?? 0;
        this.errorMap = {};
        for (const record of this.records) {
          this.errorMap[record.id!] = errorsByField(record.errors);
        }
      });
  }

  setErrorsOnly(value: boolean): void {
    this.errorsOnly = value;
    this.page = 0;
    this.loadRecords();
  }

  previous(): void {
    if (this.page > 0) {
      this.page--;
      this.loadRecords();
    }
  }

  next(): void {
    if (this.hasNext) {
      this.page++;
      this.loadRecords();
    }
  }

  fieldErrors(record: ConfigPackageRecordDto, fieldName: string | undefined): string[] {
    return this.errorMap[record.id!]?.[fieldName ?? ''] ?? [];
  }

  /** Errors of fields the grid does not show, and of the record as a whole. */
  otherErrors(record: ConfigPackageRecordDto): string[] {
    const shown = new Set(this.columns.map(c => c.fieldName ?? ''));
    const byField = this.errorMap[record.id!] ?? {};
    const result: string[] = [];
    for (const field of Object.keys(byField).filter(f => !shown.has(f))) {
      result.push(...byField[field].map(text => (field ? `${field}: ${text}` : text)));
    }
    return result;
  }

  enumValuesOf(fieldName: string | undefined): string[] {
    return this.fieldInfo.find(f => f.name === fieldName)?.enumValues ?? [];
  }

  openEdit(record: ConfigPackageRecordDto): void {
    this.editing = record;
    this.editValues = {};
    for (const column of this.columns) {
      const name = column.fieldName ?? '';
      this.editValues[name] = record.values?.[name] ?? '';
    }
    this.isEditOpen = true;
  }

  saveRecord(): void {
    const record = this.editing;
    if (!record?.id || this.isBusy) {
      return;
    }

    this.isBusy = true;
    this.service
      .updateRecord(record.id, { values: { ...record.values, ...this.editValues } })
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(updated => {
        this.isEditOpen = false;
        this.toaster.success('Erp::SavedSuccessfully');
        const index = this.records.findIndex(r => r.id === updated.id);
        if (index >= 0) {
          this.records[index] = updated;
          this.errorMap[updated.id!] = errorsByField(updated.errors);
        }
      });
  }

  remove(record: ConfigPackageRecordDto): void {
    if (!record.id) {
      return;
    }

    this.confirmation
      .warn('Erp::ItemWillBeDeletedMessage', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }
        this.service
          .deleteRecord(record.id!)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe(() => {
            this.toaster.success('Erp::DeletedSuccessfully');
            if (this.records.length === 1 && this.page > 0) {
              this.page--;
            }
            this.loadRecords();
          });
      });
  }
}
