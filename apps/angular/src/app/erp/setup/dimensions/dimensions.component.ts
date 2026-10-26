import { ABP, ListService } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  CreateDimensionDto,
  DimensionDto,
  DimensionService,
  DimensionValueDto,
  UpdateDimensionDto,
} from '@proxy/dimensions';
import { finalize, map } from 'rxjs';
import { ErpTableAction, ErpTableColumn, ErpTableCrudBase } from '../../erp-shared';

/**
 * Financial dimensions and their dimension values.
 */
@Component({
  selector: 'app-dimensions',
  templateUrl: './dimensions.component.html',
  providers: [ListService],
  standalone: false,
})
export class DimensionsComponent extends ErpTableCrudBase<DimensionDto, CreateDimensionDto & UpdateDimensionDto> {
  protected override permissionPrefix = 'Erp.Dimensions';

  private readonly service = inject(DimensionService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmationService = inject(ConfirmationService);

  override readonly columns: ErpTableColumn<DimensionDto>[] = [
    { field: 'code', labelKey: 'Erp::Code', type: 'code', width: 160 },
    { field: 'name', labelKey: 'Erp::Name', width: 240 },
    { field: 'description', labelKey: 'Erp::Description', width: 320 },
    { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 110 },
  ];

  // Dimension Values modal state
  isValuesModalOpen = false;
  isValuesBusy = false;
  activeDimension: DimensionDto | null = null;
  readonly dimensionValues = signal<DimensionValueDto[]>([]);
  editingValue: DimensionValueDto | null = null;
  valueForm: FormGroup = this.buildValueForm();

  protected override buildDefaultActions(): ErpTableAction<DimensionDto>[] {
    return [
      {
        key: 'values',
        title: 'Erp::DimensionValues',
        icon: 'fas fa-list-ul',
        btnClass: 'btn-outline-primary',
        action: (row, event) => {
          event?.stopPropagation();
          this.openValues(row);
        },
      },
      ...super.buildDefaultActions(),
    ];
  }

  protected getList = (query: ABP.PageQueryParams) =>
    this.service.getList().pipe(
      map(res => {
        const filter = (query?.filter ?? '').trim().toLowerCase();
        let items = res.items ?? [];
        if (filter) {
          items = items.filter(
            d =>
              (d.code ?? '').toLowerCase().includes(filter) ||
              (d.name ?? '').toLowerCase().includes(filter) ||
              (d.description ?? '').toLowerCase().includes(filter),
          );
        }
        return { items, totalCount: items.length };
      }),
    );

  protected create = (input: CreateDimensionDto) => this.service.create(input);
  protected update = (id: string, input: UpdateDimensionDto) => this.service.update(id, input);
  protected delete = (id: string) => this.service.delete(id);

  protected buildForm(item?: DimensionDto): FormGroup {
    return this.fb.group({
      code: [
        { value: item?.code ?? '', disabled: !!item },
        [Validators.required, Validators.maxLength(20)],
      ],
      name: [item?.name ?? '', [Validators.required, Validators.maxLength(100)]],
      description: [item?.description ?? '', Validators.maxLength(250)],
      blocked: [item?.blocked ?? false],
    });
  }

  openValues(dimension: DimensionDto): void {
    this.activeDimension = dimension;
    this.editingValue = null;
    this.valueForm = this.buildValueForm();
    this.isValuesModalOpen = true;
    this.loadValues();
  }

  startEditValue(val: DimensionValueDto): void {
    this.editingValue = val;
    this.valueForm = this.buildValueForm(val);
  }

  cancelEditValue(): void {
    this.editingValue = null;
    this.valueForm = this.buildValueForm();
  }

  saveValue(): void {
    if (!this.activeDimension?.id || this.valueForm.invalid) {
      this.valueForm.markAllAsTouched();
      return;
    }

    const raw = this.valueForm.getRawValue();
    this.isValuesBusy = true;

    const req$ = this.editingValue?.id
      ? this.service.updateValue(this.editingValue.id, {
          name: raw.name?.trim(),
          blocked: !!raw.blocked,
        })
      : this.service.createValue({
          dimensionId: this.activeDimension.id,
          code: raw.code?.trim().toUpperCase(),
          name: raw.name?.trim(),
          blocked: !!raw.blocked,
        });

    req$
      .pipe(
        finalize(() => (this.isValuesBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.editingValue = null;
        this.valueForm = this.buildValueForm();
        this.loadValues();
        this.toaster.success('Erp::SavedSuccessfully');
      });
  }

  removeValue(val: DimensionValueDto): void {
    if (!val.id) {
      return;
    }
    this.confirmationService
      .warn('AbpUi::ItemWillBeDeletedMessage', 'AbpUi::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm || !val.id) {
          return;
        }
        this.isValuesBusy = true;
        this.service
          .deleteValue(val.id)
          .pipe(
            finalize(() => (this.isValuesBusy = false)),
            takeUntilDestroyed(this.destroyRef),
          )
          .subscribe(() => {
            if (this.editingValue?.id === val.id) {
              this.cancelEditValue();
            }
            this.loadValues();
          });
      });
  }

  private loadValues(): void {
    if (!this.activeDimension?.id) {
      return;
    }
    this.isValuesBusy = true;
    this.service
      .getValues(this.activeDimension.id)
      .pipe(
        finalize(() => (this.isValuesBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(res => this.dimensionValues.set(res.items ?? []));
  }

  private buildValueForm(item?: DimensionValueDto): FormGroup {
    return this.fb.group({
      code: [
        { value: item?.code ?? '', disabled: !!item },
        [Validators.required, Validators.maxLength(20)],
      ],
      name: [item?.name ?? '', [Validators.required, Validators.maxLength(100)]],
      blocked: [item?.blocked ?? false],
    });
  }
}
