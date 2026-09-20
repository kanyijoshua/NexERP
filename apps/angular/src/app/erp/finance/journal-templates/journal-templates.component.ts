import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  CreateUpdateGenJournalTemplateDto,
  GenJournalTemplateDto,
  GenJournalTemplateType,
  JournalTemplateService,
} from '@proxy/finance';
import { NoSeriesService } from '@proxy/numbering';
import { Observable, map } from 'rxjs';
import { CrudListBase } from '../../erp-shared';

/**
 * Journal templates. Mirrors Business Central page 100 "Gen. Journal Templates": the kinds of
 * journal a company keeps, and whether their lines recur.
 */
@Component({
  selector: 'app-journal-templates',
  templateUrl: './journal-templates.component.html',
  providers: [ListService],
})
export class JournalTemplatesComponent
  extends CrudListBase<GenJournalTemplateDto, CreateUpdateGenJournalTemplateDto>
  implements OnInit
{
  readonly typeOptions = [
    { value: GenJournalTemplateType.General, label: 'Erp::TemplateTypeGeneral' },
    { value: GenJournalTemplateType.Sales, label: 'Erp::TemplateTypeSales' },
    { value: GenJournalTemplateType.Purchases, label: 'Erp::TemplateTypePurchases' },
    { value: GenJournalTemplateType.CashReceipts, label: 'Erp::TemplateTypeCashReceipts' },
    { value: GenJournalTemplateType.Payments, label: 'Erp::TemplateTypePayments' },
  ];

  seriesCodes: string[] = [];

  constructor(
    private readonly service: JournalTemplateService,
    private readonly noSeries: NoSeriesService,
    private readonly fb: FormBuilder,
  ) {
    super();
  }

  override ngOnInit(): void {
    super.ngOnInit();

    this.noSeries
      .getList({ maxResultCount: 1000, skipCount: 0 } as never)
      .subscribe(result => (this.seriesCodes = (result.items ?? []).map(s => s.code ?? '')));
  }

  /** The list is short and unpaged on the server, so it is wrapped to fit the paged table. */
  protected getList = (
    _query: ABP.PageQueryParams,
  ): Observable<PagedResultDto<GenJournalTemplateDto>> =>
    this.service
      .getList()
      .pipe(map(result => ({ items: result.items ?? [], totalCount: result.items?.length ?? 0 })));

  protected create = (input: CreateUpdateGenJournalTemplateDto) => this.service.create(input);
  protected update = (id: string, input: CreateUpdateGenJournalTemplateDto) =>
    this.service.update(id, input);
  protected delete = (id: string) => this.service.delete(id);

  protected buildForm(item?: GenJournalTemplateDto): FormGroup {
    return this.fb.group({
      // The name is the key batches and standard journals refer to, so it is fixed once created.
      name: [
        { value: item?.name ?? '', disabled: !!item },
        [Validators.required, Validators.maxLength(10)],
      ],
      description: [item?.description ?? '', Validators.maxLength(250)],
      type: [item?.type ?? GenJournalTemplateType.General],
      recurring: [item?.recurring ?? false],
      sourceCode: [item?.sourceCode ?? '', Validators.maxLength(10)],
      noSeriesCode: [item?.noSeriesCode ?? ''],
    });
  }
}
