import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Component } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  ColumnLayoutDto,
  ColumnLayoutLineDto,
  ColumnLayoutService,
  ColumnLayoutType,
  CreateUpdateColumnLayoutDto,
} from '@proxy/reporting';
import { Observable, forkJoin, map } from 'rxjs';
import { CrudListBase, DocumentLineColumn } from '../../erp-shared';

/**
 * Column layouts: the periods a financial report is shown across.
 * Mirrors Business Central pages 331 and 332.
 */
@Component({
  selector: 'app-column-layouts',
  templateUrl: './column-layouts.component.html',
  providers: [ListService],
})
export class ColumnLayoutsComponent extends CrudListBase<
  ColumnLayoutDto,
  CreateUpdateColumnLayoutDto
> {
  readonly columnTypeOptions = [
    { value: ColumnLayoutType.NetChange, label: 'Erp::ColumnNetChange' },
    { value: ColumnLayoutType.BalanceAtDate, label: 'Erp::ColumnBalanceAtDate' },
    { value: ColumnLayoutType.BeginningBalance, label: 'Erp::ColumnBeginningBalance' },
    { value: ColumnLayoutType.YearToDateNetChange, label: 'Erp::ColumnYearToDate' },
  ];

  readonly lineColumns: DocumentLineColumn[] = [
    { field: 'columnNo', labelKey: 'Erp::ColumnNo', type: 'text', width: '110px' },
    { field: 'columnHeader', labelKey: 'Erp::ColumnHeader', type: 'text' },
    {
      field: 'columnType',
      labelKey: 'Erp::ColumnType',
      type: 'select',
      width: '190px',
      options: this.columnTypeOptions,
    },
    {
      field: 'comparisonDateFormula',
      labelKey: 'Erp::ComparisonDateFormula',
      type: 'text',
      width: '160px',
    },
  ];

  layoutOwner: ColumnLayoutDto | null = null;
  layoutLines = new FormArray<FormGroup>([]);
  lineIds: (string | null)[] = [];
  isLinesModalOpen = false;

  constructor(
    private readonly service: ColumnLayoutService,
    private readonly fb: FormBuilder,
    private readonly toasterService: ToasterService,
  ) {
    super();
  }

  protected getList = (_query: ABP.PageQueryParams): Observable<PagedResultDto<ColumnLayoutDto>> =>
    this.service
      .getList()
      .pipe(map(result => ({ items: result.items ?? [], totalCount: result.items?.length ?? 0 })));

  protected create = (input: CreateUpdateColumnLayoutDto) => this.service.create(input);
  protected update = (id: string, input: CreateUpdateColumnLayoutDto) =>
    this.service.update(id, input);
  protected delete = (id: string) => this.service.delete(id);

  protected buildForm(item?: ColumnLayoutDto): FormGroup {
    return this.fb.group({
      name: [
        { value: item?.name ?? '', disabled: !!item },
        [Validators.required, Validators.maxLength(100)],
      ],
      description: [item?.description ?? '', Validators.maxLength(250)],
    });
  }

  openLines(layout: ColumnLayoutDto): void {
    this.layoutOwner = layout;
    this.layoutLines = new FormArray<FormGroup>([]);
    this.lineIds = [];
    this.isLinesModalOpen = true;

    this.service
      .getLines(layout.id!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        const items = result.items ?? [];

        this.layoutLines = new FormArray<FormGroup>(
          items.length ? items.map(line => this.buildLine(line)) : [this.buildLine()],
        );
        this.lineIds = items.length ? items.map(line => line.id ?? null) : [null];
      });
  }

  addLine(): void {
    this.layoutLines.push(this.buildLine());
    this.lineIds.push(null);
  }

  removeLine(index: number): void {
    const id = this.lineIds[index];

    this.layoutLines.removeAt(index);
    this.lineIds.splice(index, 1);

    if (id) {
      this.service
        .deleteLine(id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.openLines(this.layoutOwner!));
    }
  }

  saveLines(): void {
    if (!this.layoutOwner || this.layoutLines.invalid) {
      this.layoutLines.markAllAsTouched();
      return;
    }

    const requests = this.layoutLines.controls.map((control, index) => {
      const raw = control.getRawValue();
      const input = {
        ...raw,
        columnLayoutId: this.layoutOwner!.id!,
        // A blank formula means the column keeps the report's own period.
        comparisonDateFormula: raw.comparisonDateFormula || undefined,
      };
      const id = this.lineIds[index];
      return id ? this.service.updateLine(id, input) : this.service.createLine(input);
    });

    if (requests.length === 0) {
      return;
    }

    this.isBusy = true;
    forkJoin(requests)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isBusy = false;
          this.isLinesModalOpen = false;
          this.toasterService.success('Erp::SavedSuccessfully');
          this.list.get();
        },
        error: () => (this.isBusy = false),
      });
  }

  private buildLine(line?: ColumnLayoutLineDto): FormGroup {
    return this.fb.group({
      columnNo: [line?.columnNo ?? '', [Validators.required, Validators.maxLength(10)]],
      columnHeader: [line?.columnHeader ?? '', Validators.maxLength(50)],
      columnType: [line?.columnType ?? ColumnLayoutType.NetChange],
      comparisonDateFormula: [line?.comparisonDateFormula ?? '', Validators.maxLength(32)],
      showOppositeSign: [line?.showOppositeSign ?? false],
    });
  }
}
