import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ConfigLineDto,
  ConfigLineStatus,
  ConfigLineType,
  ConfigPackageDto,
  ConfigPackageService,
  ConfigTableInfoDto,
  ConfigWorksheetService,
  configLineStatusOptions,
  configLineTypeOptions,
} from '@proxy/rapid-start';
import { Observable } from 'rxjs';
import { finalize, switchMap } from 'rxjs/operators';
import { CompanyService } from '../../services/company.service';
import {
  TableArea,
  TableLink,
  configLineIndent,
  configLineStatusClass,
  groupTablesByArea,
  tablePageLink,
} from '../rapid-start.helpers';

interface LineDraft {
  lineType: ConfigLineType;
  name: string;
  entityName: string;
  packageCode: string;
  status: ConfigLineStatus;
  responsibleUserName: string;
  comments: string;
}

/**
 * A company's set-up checklist. Mirrors Business Central page 8632 "Configuration Worksheet":
 * areas, groups and the tables to set up under them, each with its status and owner.
 */
@Component({
    selector: 'app-config-worksheet',
    templateUrl: './config-worksheet.component.html',
    standalone: false
})
export class ConfigWorksheetComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly service = inject(ConfigWorksheetService);
  private readonly packageService = inject(ConfigPackageService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);

  readonly ConfigLineType = ConfigLineType;
  readonly ConfigLineStatus = ConfigLineStatus;
  readonly lineTypes = configLineTypeOptions;
  readonly statuses = configLineStatusOptions;

  lines: ConfigLineDto[] = [];
  packages: ConfigPackageDto[] = [];
  catalogAreas: TableArea[] = [];
  private catalog: ConfigTableInfoDto[] = [];
  loading = false;
  busy = false;

  isModalOpen = false;
  isBusy = false;
  selected: ConfigLineDto | null = null;
  draft: LineDraft = this.emptyDraft();

  get canSave(): boolean {
    return (
      !!this.draft.name.trim() &&
      (this.draft.lineType !== ConfigLineType.Table || !!this.draft.entityName || !!this.selected) &&
      !this.isBusy
    );
  }

  ngOnInit(): void {
    this.packageService
      .getTableCatalog()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.catalog = result.items ?? [];
        this.catalogAreas = groupTablesByArea(this.catalog);
      });

    this.load();
    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  load(): void {
    this.loading = true;
    this.service
      .getList()
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => this.setLines(result.items));

    this.packageService
      .getList({})
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.packages = result.items ?? []));
  }

  indent(line: ConfigLineDto): number {
    return configLineIndent(line.lineType);
  }

  statusClass(line: ConfigLineDto): string {
    return configLineStatusClass(line.status);
  }

  statusName(status: ConfigLineStatus): string {
    return ConfigLineStatus[status] ?? ConfigLineStatus[ConfigLineStatus.NotStarted];
  }

  link(line: ConfigLineDto): TableLink | null {
    return line.lineType === ConfigLineType.Table ? tablePageLink(line.entityName) : null;
  }

  packageIdOf(code: string | undefined): string | undefined {
    return code ? this.packages.find(p => p.code === code)?.id : undefined;
  }

  suggestLines(): void {
    // Whether the answer holds the whole worksheet or only the new lines is not part of the
    // contract, so the worksheet is read again and the difference counted.
    const before = this.lines.length;
    this.run(this.service.suggestLines().pipe(switchMap(() => this.service.getList()))).subscribe(result => {
      this.setLines(result.items);
      this.toaster.success('Erp::LinesSuggested', undefined, {
        messageLocalizationParams: [String(Math.max(0, this.lines.length - before))],
      });
    });
  }

  move(line: ConfigLineDto, up: boolean): void {
    this.run(this.service.move(line.id!, { up })).subscribe(result => this.setLines(result.items));
  }

  isFirst(line: ConfigLineDto): boolean {
    return this.lines[0]?.id === line.id;
  }

  isLast(line: ConfigLineDto): boolean {
    return this.lines[this.lines.length - 1]?.id === line.id;
  }

  openCreate(): void {
    this.selected = null;
    this.draft = this.emptyDraft();
    this.isModalOpen = true;
  }

  openEdit(line: ConfigLineDto): void {
    this.selected = line;
    this.draft = {
      lineType: line.lineType,
      name: line.name ?? '',
      entityName: line.entityName ?? '',
      packageCode: line.packageCode ?? '',
      status: line.status,
      responsibleUserName: line.responsibleUserName ?? '',
      comments: line.comments ?? '',
    };
    this.isModalOpen = true;
  }

  /** Picking a table names the line after it, unless a name was typed already. */
  onEntityChange(entityName: string): void {
    const previous = this.catalog.find(t => t.entityName === this.draft.entityName)?.displayName ?? '';
    this.draft.entityName = entityName;
    if (!this.draft.name.trim() || this.draft.name === previous) {
      this.draft.name = this.catalog.find(t => t.entityName === entityName)?.displayName ?? entityName;
    }
  }

  save(): void {
    if (!this.canSave) {
      return;
    }

    const common = {
      name: this.draft.name.trim(),
      packageCode: this.draft.packageCode || undefined,
      status: Number(this.draft.status) as ConfigLineStatus,
      responsibleUserName: this.draft.responsibleUserName.trim() || undefined,
      comments: this.draft.comments.trim() || undefined,
    };
    const id = this.selected?.id;
    const lineType = Number(this.draft.lineType) as ConfigLineType;
    const request$: Observable<unknown> = id
      ? this.service.update(id, common)
      : this.service.create({
          ...common,
          lineType,
          entityName: lineType === ConfigLineType.Table ? this.draft.entityName : undefined,
        });

    this.isBusy = true;
    request$
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.isModalOpen = false;
        this.toaster.success('Erp::SavedSuccessfully');
        this.load();
      });
  }

  remove(line: ConfigLineDto): void {
    this.confirmation
      .warn('Erp::ItemWillBeDeletedMessage', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }
        this.run(this.service.delete(line.id!)).subscribe(() => {
          this.toaster.success('Erp::DeletedSuccessfully');
          this.load();
        });
      });
  }

  private setLines(items: ConfigLineDto[] | undefined): void {
    this.lines = [...(items ?? [])].sort((a, b) => a.sortOrder - b.sortOrder);
  }

  private run<T>(request$: Observable<T>): Observable<T> {
    this.busy = true;
    return request$.pipe(
      finalize(() => (this.busy = false)),
      takeUntilDestroyed(this.destroyRef),
    );
  }

  private emptyDraft(): LineDraft {
    return {
      lineType: ConfigLineType.Table,
      name: '',
      entityName: '',
      packageCode: '',
      status: ConfigLineStatus.NotStarted,
      responsibleUserName: '',
      comments: '',
    };
  }
}
