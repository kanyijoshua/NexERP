import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import {
  CreateUpdateReportLayoutDto,
  ReportLayoutDetailDto,
  ReportLayoutDto,
  ReportLayoutService,
  ReportLayoutType,
  ReportNameDto,
} from '@proxy/reporting';
import { Observable, map } from 'rxjs';
import { CrudListBase, saveBlob } from '../../erp-shared';

/**
 * Report layouts: what a report looks like when it is printed.
 * <p>
 * Mirrors the Business Central Report Layouts page. A layout is text, so the same three moves work
 * here as there: start from the built-in one, edit it, and make it the one this company uses.
 * </p>
 */
@Component({
  selector: 'app-report-layouts',
  templateUrl: './report-layouts.component.html',
  providers: [ListService],
})
export class ReportLayoutsComponent
  extends CrudListBase<ReportLayoutDto, CreateUpdateReportLayoutDto>
  implements OnInit
{
  private readonly sanitizer = inject(DomSanitizer);

  readonly ReportLayoutType = ReportLayoutType;

  reportNames: ReportNameDto[] = [];

  /** Blank shows every report's layouts, which is how the page opens. */
  reportFilter = '';

  builtInTemplate = '';

  isPreviewOpen = false;
  previewDoc: SafeHtml | null = null;

  constructor(
    private readonly service: ReportLayoutService,
    private readonly fb: FormBuilder,
  ) {
    super();
  }

  override ngOnInit(): void {
    super.ngOnInit();

    this.service
      .getReportNames()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.reportNames = result.items ?? []));

    this.service
      .getBuiltInTemplate()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(template => (this.builtInTemplate = template));
  }

  protected getList = (_query: ABP.PageQueryParams): Observable<PagedResultDto<ReportLayoutDto>> =>
    this.service
      .getList({ reportName: this.reportFilter || undefined })
      .pipe(map(result => ({ items: result.items ?? [], totalCount: result.items?.length ?? 0 })));

  protected create = (input: CreateUpdateReportLayoutDto) => this.service.create(input);
  protected update = (id: string, input: CreateUpdateReportLayoutDto) =>
    this.service.update(id, input);
  protected delete = (id: string) => this.service.delete(id);

  /** The list carries no bodies, so editing fetches the layout itself first. */
  protected override load(id: string): Observable<ReportLayoutDto> {
    return this.service.get(id);
  }

  onReportFilterChange(): void {
    this.list.get();
  }

  /** A new layout starts from the built-in one, the way BC hands you a copy to edit. */
  protected buildForm(item?: ReportLayoutDto): FormGroup {
    const detail = item as ReportLayoutDetailDto | undefined;

    return this.fb.group({
      // The report and the format are what a layout is for, so they are fixed once it exists.
      reportName: [
        { value: detail?.reportName ?? this.reportFilter, disabled: !!detail },
        Validators.required,
      ],
      layoutName: [detail?.layoutName ?? '', [Validators.required, Validators.maxLength(100)]],
      layoutType: [{ value: detail?.layoutType ?? ReportLayoutType.Html, disabled: !!detail }],
      description: [detail?.description ?? '', Validators.maxLength(250)],
      templateContent: [detail?.templateContent ?? this.builtInTemplate, Validators.required],
    });
  }

  resetToBuiltIn(): void {
    this.form?.patchValue({ templateContent: this.builtInTemplate });
  }

  download(): void {
    const name = (this.form?.getRawValue().layoutName as string) || 'layout';
    saveBlob(new Blob([this.template], { type: 'text/html' }), `${name}.html`);
  }

  upload(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    file.text().then(text => this.form?.patchValue({ templateContent: text }));

    // Clearing it lets the same file be picked again after an edit.
    input.value = '';
  }

  /** Renders against sample figures, so a layout can be checked before it is saved. */
  preview(): void {
    if (!this.template) {
      return;
    }

    this.isBusy = true;
    this.service
      .runPreview({ templateContent: this.template })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: html => {
          this.isBusy = false;
          // Shown in a sandboxed frame: a layout is a whole document with its own styling, and
          // it must not reach into the application around it.
          this.previewDoc = this.sanitizer.bypassSecurityTrustHtml(html);
          this.isPreviewOpen = true;
        },
        error: () => (this.isBusy = false),
      });
  }

  useThisLayout(layout: ReportLayoutDto): void {
    this.isBusy = true;
    this.service
      .setDefault({ reportName: layout.reportName!, layoutId: layout.id! })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isBusy = false;
          this.toaster.success(this.savedMessageKey);
          this.list.get();
        },
        error: () => (this.isBusy = false),
      });
  }

  displayNameOf(reportName?: string): string {
    return this.reportNames.find(r => r.name === reportName)?.displayName ?? reportName ?? '';
  }

  get template(): string {
    return (this.form?.getRawValue().templateContent as string) ?? '';
  }
}
