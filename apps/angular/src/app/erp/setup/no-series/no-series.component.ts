import { ABP, ListService } from '@abp/ng.core';
import { Component } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  CreateUpdateNoSeriesDto,
  NoSeriesDto,
  NoSeriesLineDto,
  NoSeriesService,
} from '@proxy/numbering';
import { CrudListBase, DocumentLineColumn } from '../../erp-shared';

/** No. Series list and card. Mirrors Business Central pages 456 "No. Series" and 457 "No. Series Lines". */
@Component({
  selector: 'app-no-series',
  templateUrl: './no-series.component.html',
  providers: [ListService],
})
export class NoSeriesComponent extends CrudListBase<NoSeriesDto, CreateUpdateNoSeriesDto> {
  readonly lineColumns: DocumentLineColumn[] = [
    { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', width: '150px' },
    { field: 'startingNo', labelKey: 'Erp::StartingNo', type: 'text' },
    { field: 'endingNo', labelKey: 'Erp::EndingNo', type: 'text' },
    { field: 'warningNo', labelKey: 'Erp::WarningNo', type: 'text' },
    {
      field: 'incrementByNo',
      labelKey: 'Erp::IncrementByNo',
      type: 'number',
      width: '110px',
      step: 1,
    },
    { field: 'lastNoUsed', labelKey: 'Erp::LastNoUsed', type: 'readonly' },
  ];

  constructor(
    private readonly service: NoSeriesService,
    private readonly fb: FormBuilder,
  ) {
    super();
  }

  get lines(): FormArray {
    return this.form.get('lines') as FormArray;
  }

  protected getList = (query: ABP.PageQueryParams) =>
    this.service.getList({ ...query, filter: query.filter } as never);

  protected create = (input: CreateUpdateNoSeriesDto) => this.service.create(input);
  protected update = (id: string, input: CreateUpdateNoSeriesDto) => this.service.update(id, input);
  protected delete = (id: string) => this.service.delete(id);

  protected buildForm(item?: NoSeriesDto): FormGroup {
    const lines = (item?.lines ?? []).map(line => this.buildLine(line));

    return this.fb.group({
      // The code is the key other tables refer to, so it is fixed once created.
      code: [
        { value: item?.code ?? '', disabled: !!item },
        [Validators.required, Validators.maxLength(20)],
      ],
      description: [item?.description ?? '', Validators.maxLength(250)],
      defaultNos: [item?.defaultNos ?? true],
      manualNos: [item?.manualNos ?? false],
      dateOrder: [item?.dateOrder ?? false],
      lines: this.fb.array(lines.length ? lines : [this.buildLine()]),
    });
  }

  addLine(): void {
    this.lines.push(this.buildLine());
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  private buildLine(line?: NoSeriesLineDto): FormGroup {
    return this.fb.group({
      id: [line?.id ?? null],
      startingDate: [line?.startingDate ? line.startingDate.substring(0, 10) : null],
      // Needs at least one digit, otherwise there is nothing to increment.
      startingNo: [
        line?.startingNo ?? '',
        [Validators.required, Validators.maxLength(20), Validators.pattern(/.*\d.*/)],
      ],
      endingNo: [line?.endingNo ?? '', Validators.maxLength(20)],
      warningNo: [line?.warningNo ?? '', Validators.maxLength(20)],
      incrementByNo: [line?.incrementByNo ?? 1, [Validators.required, Validators.min(1)]],
      // Shown for information; the server never takes it from the client.
      lastNoUsed: [{ value: line?.lastNoUsed ?? '', disabled: true }],
    });
  }
}
