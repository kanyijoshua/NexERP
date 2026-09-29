import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ConfigFieldInfoDto,
  ConfigPackageService,
  ConfigTableInfoDto,
  ConfigTemplateDto,
  ConfigTemplateLineDto,
  ConfigTemplateService,
} from '@proxy/rapid-start';
import { finalize } from 'rxjs/operators';
import { CompanyService } from '../../services/company.service';
import { TableArea, groupTablesByArea } from '../rapid-start.helpers';

interface TemplateDraft {
  code: string;
  description: string;
  entityName: string;
  enabled: boolean;
  lines: ConfigTemplateLineDto[];
}

/**
 * Configuration templates: default values a new record takes. Mirrors Business Central page 8618
 * "Config. Template Header" with its lines.
 */
@Component({
    selector: 'app-config-templates',
    templateUrl: './config-templates.component.html',
    standalone: false
})
export class ConfigTemplatesComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly service = inject(ConfigTemplateService);
  private readonly packageService = inject(ConfigPackageService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);

  templates: ConfigTemplateDto[] = [];
  catalog: ConfigTableInfoDto[] = [];
  catalogAreas: TableArea[] = [];
  entityFilter = '';
  loading = false;

  isModalOpen = false;
  isBusy = false;
  selected: ConfigTemplateDto | null = null;
  draft: TemplateDraft = this.emptyDraft();
  fields: ConfigFieldInfoDto[] = [];

  get canSave(): boolean {
    return (
      !!this.draft.code.trim() &&
      !!this.draft.entityName &&
      this.draft.lines.every(l => !!l.fieldName) &&
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
      .getList({ entityName: this.entityFilter || undefined })
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => (this.templates = result.items ?? []));
  }

  tableName(entityName: string | undefined): string {
    return this.catalog.find(t => t.entityName === entityName)?.displayName || entityName || '';
  }

  openCreate(): void {
    this.selected = null;
    this.draft = this.emptyDraft();
    this.draft.entityName = this.entityFilter;
    this.fields = [];
    this.loadFields();
    this.isModalOpen = true;
  }

  openEdit(template: ConfigTemplateDto): void {
    this.selected = template;
    this.draft = {
      code: template.code ?? '',
      description: template.description ?? '',
      entityName: template.entityName ?? '',
      enabled: template.enabled,
      lines: (template.lines ?? []).map(l => ({ ...l })),
    };
    this.fields = [];
    this.loadFields();
    this.isModalOpen = true;
  }

  onEntityChange(entityName: string): void {
    // The lines name fields of the table, so a new table starts them over.
    if (entityName !== this.draft.entityName) {
      this.draft.lines = [];
    }
    this.draft.entityName = entityName;
    this.loadFields();
  }

  addLine(): void {
    this.draft.lines.push({ fieldName: '', defaultValue: '', mandatory: false });
  }

  removeLine(index: number): void {
    this.draft.lines.splice(index, 1);
  }

  fieldOf(fieldName: string): ConfigFieldInfoDto | undefined {
    return this.fields.find(f => f.name === fieldName);
  }

  /** The values a line can choose from: an enum's values, or yes / no for a boolean. */
  valueOptions(fieldName: string): string[] {
    const field = this.fieldOf(fieldName);
    if (!field) {
      return [];
    }
    if (field.enumValues?.length) {
      return field.enumValues;
    }
    return field.dataType === 'boolean' ? ['true', 'false'] : [];
  }

  isUsed(fieldName: string | undefined, index: number): boolean {
    return this.draft.lines.some((l, i) => i !== index && l.fieldName === fieldName);
  }

  save(): void {
    if (!this.canSave) {
      return;
    }

    const input = {
      code: this.draft.code.trim().toUpperCase(),
      description: this.draft.description.trim() || undefined,
      entityName: this.draft.entityName,
      enabled: this.draft.enabled,
      lines: this.draft.lines.map(l => ({
        fieldName: l.fieldName,
        defaultValue: l.defaultValue ?? '',
        mandatory: l.mandatory,
      })),
    };
    const id = this.selected?.id;
    const request$ = id ? this.service.update(id, input) : this.service.create(input);

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

  remove(template: ConfigTemplateDto): void {
    if (!template.id) {
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
          .delete(template.id!)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe(() => {
            this.toaster.success('Erp::DeletedSuccessfully');
            this.load();
          });
      });
  }

  private loadFields(): void {
    const entityName = this.draft.entityName;
    if (!entityName) {
      this.fields = [];
      return;
    }

    this.packageService
      .getTableFields(entityName)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.fields = result.items ?? []));
  }

  private emptyDraft(): TemplateDraft {
    return { code: '', description: '', entityName: '', enabled: true, lines: [] };
  }
}
