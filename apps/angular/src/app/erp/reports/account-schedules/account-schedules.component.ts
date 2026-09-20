import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  AccountScheduleDto,
  AccountScheduleLineDto,
  AccountScheduleService,
  AccountScheduleTotalingType,
  ColumnLayoutDto,
  ColumnLayoutService,
  CreateUpdateAccountScheduleDto,
} from '@proxy/reporting';
import { Observable, forkJoin, map } from 'rxjs';
import { CrudListBase, DocumentLineColumn } from '../../erp-shared';

/**
 * Account schedules: the rows of a financial report, defined by the user rather than in code.
 * Mirrors Business Central pages 103 and 104.
 */
@Component({
  selector: 'app-account-schedules',
  templateUrl: './account-schedules.component.html',
  providers: [ListService],
})
export class AccountSchedulesComponent
  extends CrudListBase<AccountScheduleDto, CreateUpdateAccountScheduleDto>
  implements OnInit
{
  readonly totalingTypeOptions = [
    { value: AccountScheduleTotalingType.PostingAccounts, label: 'Erp::TotalingPostingAccounts' },
    { value: AccountScheduleTotalingType.TotalAccounts, label: 'Erp::TotalingTotalAccounts' },
    { value: AccountScheduleTotalingType.Formula, label: 'Erp::TotalingFormula' },
    { value: AccountScheduleTotalingType.Description, label: 'Erp::TotalingDescription' },
  ];

  readonly lineColumns: DocumentLineColumn[] = [
    { field: 'rowNo', labelKey: 'Erp::RowNo', type: 'text', width: '100px' },
    { field: 'description', labelKey: 'Erp::Description', type: 'text' },
    {
      field: 'totalingType',
      labelKey: 'Erp::TotalingType',
      type: 'select',
      width: '170px',
      options: this.totalingTypeOptions,
    },
    { field: 'totaling', labelKey: 'Erp::Totaling', type: 'text', width: '220px' },
    { field: 'indentation', labelKey: 'Erp::Indentation', type: 'number', width: '90px', step: 1 },
  ];

  layouts: ColumnLayoutDto[] = [];

  /** The schedule whose rows are being edited, and the rows themselves. */
  rowsOwner: AccountScheduleDto | null = null;
  scheduleLines = new FormArray<FormGroup>([]);
  lineIds: (string | null)[] = [];
  isRowsModalOpen = false;

  constructor(
    private readonly service: AccountScheduleService,
    private readonly layoutService: ColumnLayoutService,
    private readonly fb: FormBuilder,
    private readonly toasterService: ToasterService,
  ) {
    super();
  }

  override ngOnInit(): void {
    super.ngOnInit();

    this.layoutService
      .getList()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.layouts = result.items ?? []));
  }

  protected getList = (
    _query: ABP.PageQueryParams,
  ): Observable<PagedResultDto<AccountScheduleDto>> =>
    this.service
      .getList()
      .pipe(map(result => ({ items: result.items ?? [], totalCount: result.items?.length ?? 0 })));

  protected create = (input: CreateUpdateAccountScheduleDto) => this.service.create(input);
  protected update = (id: string, input: CreateUpdateAccountScheduleDto) =>
    this.service.update(id, input);
  protected delete = (id: string) => this.service.delete(id);

  protected buildForm(item?: AccountScheduleDto): FormGroup {
    return this.fb.group({
      // The name is how a report asks for the schedule, so it is fixed once created.
      name: [
        { value: item?.name ?? '', disabled: !!item },
        [Validators.required, Validators.maxLength(100)],
      ],
      description: [item?.description ?? '', Validators.maxLength(250)],
      defaultColumnLayoutName: [item?.defaultColumnLayoutName ?? ''],
    });
  }

  openRows(schedule: AccountScheduleDto): void {
    this.rowsOwner = schedule;
    this.scheduleLines = new FormArray<FormGroup>([]);
    this.lineIds = [];
    this.isRowsModalOpen = true;

    this.service
      .getLines(schedule.id!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        const items = result.items ?? [];

        this.scheduleLines = new FormArray<FormGroup>(
          items.length ? items.map(line => this.buildLine(line)) : [this.buildLine()],
        );
        this.lineIds = items.length ? items.map(line => line.id ?? null) : [null];
      });
  }

  addLine(): void {
    this.scheduleLines.push(this.buildLine());
    this.lineIds.push(null);
  }

  removeLine(index: number): void {
    const id = this.lineIds[index];

    this.scheduleLines.removeAt(index);
    this.lineIds.splice(index, 1);

    if (id) {
      this.service
        .deleteLine(id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.openRows(this.rowsOwner!));
    }
  }

  saveRows(): void {
    if (!this.rowsOwner || this.scheduleLines.invalid) {
      this.scheduleLines.markAllAsTouched();
      return;
    }

    const requests = this.scheduleLines.controls.map((control, index) => {
      const input = { ...control.getRawValue(), accountScheduleId: this.rowsOwner!.id! };
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
          this.isRowsModalOpen = false;
          this.toasterService.success('Erp::SavedSuccessfully');
          this.list.get();
        },
        error: () => (this.isBusy = false),
      });
  }

  private buildLine(line?: AccountScheduleLineDto): FormGroup {
    return this.fb.group({
      rowNo: [line?.rowNo ?? '', [Validators.required, Validators.maxLength(10)]],
      description: [line?.description ?? '', Validators.maxLength(250)],
      totalingType: [line?.totalingType ?? AccountScheduleTotalingType.PostingAccounts],
      totaling: [line?.totaling ?? '', Validators.maxLength(250)],
      showOppositeSign: [line?.showOppositeSign ?? false],
      bold: [line?.bold ?? false],
      italic: [line?.italic ?? false],
      indentation: [line?.indentation ?? 0, [Validators.min(0), Validators.max(5)]],
      hideIfZero: [line?.hideIfZero ?? false],
    });
  }
}
