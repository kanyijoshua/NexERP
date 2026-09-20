import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DataExportService, ExportableEntityDto } from '@proxy/exporting';
import {
  CreateUpdateWebServiceDto,
  PublishedWebServiceDto,
  WebServiceObjectType,
  WebServiceService,
} from '@proxy/integration';
import { Observable, map } from 'rxjs';
import { CrudListBase } from '../../erp-shared';

/**
 * Which tables this company exposes to other systems.
 * Mirrors Business Central's Web Services page: nothing is readable until it is published.
 */
@Component({
  selector: 'app-web-services',
  templateUrl: './web-services.component.html',
  providers: [ListService],
})
export class WebServicesComponent
  extends CrudListBase<PublishedWebServiceDto, CreateUpdateWebServiceDto>
  implements OnInit
{
  readonly objectTypeOptions = [
    { value: WebServiceObjectType.Page, label: 'Erp::ObjectTypePage' },
    { value: WebServiceObjectType.Query, label: 'Erp::ObjectTypeQuery' },
  ];

  /** The tables a service may expose are exactly the ones this user could export. */
  entities: ExportableEntityDto[] = [];

  constructor(
    private readonly service: WebServiceService,
    private readonly dataExport: DataExportService,
    private readonly fb: FormBuilder,
  ) {
    super();
  }

  override ngOnInit(): void {
    super.ngOnInit();

    this.dataExport
      .getEntities()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.entities = result.items ?? []));
  }

  protected getList = (
    _query: ABP.PageQueryParams,
  ): Observable<PagedResultDto<PublishedWebServiceDto>> =>
    this.service
      .getList()
      .pipe(map(result => ({ items: result.items ?? [], totalCount: result.items?.length ?? 0 })));

  protected create = (input: CreateUpdateWebServiceDto) => this.service.create(input);
  protected update = (id: string, input: CreateUpdateWebServiceDto) =>
    this.service.update(id, input);
  protected delete = (id: string) => this.service.delete(id);

  protected buildForm(item?: PublishedWebServiceDto): FormGroup {
    return this.fb.group({
      serviceName: [item?.serviceName ?? '', [Validators.required, Validators.maxLength(100)]],
      // The table is what the service is for, so it cannot be swapped underneath its callers.
      entityName: [
        { value: item?.entityName ?? '', disabled: !!item },
        [Validators.required, Validators.maxLength(100)],
      ],
      objectType: [item?.objectType ?? WebServiceObjectType.Page],
      excludedFields: [item?.excludedFields ?? '', Validators.maxLength(250)],
    });
  }

  setPublished(service: PublishedWebServiceDto, published: boolean): void {
    this.service
      .setPublished({ id: service.id!, published })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.toaster.success('Erp::SavedSuccessfully');
        this.list.get();
      });
  }
}
