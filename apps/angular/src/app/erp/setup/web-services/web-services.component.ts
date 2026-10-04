import { ABP, EnvironmentService, ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit, inject } from '@angular/core';
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
import { ErpTableAction, ErpTableColumn, ErpTableCrudBase } from '../../erp-shared';

/**
 * Which tables this company exposes to other systems.
 * Nothing is readable until it is published.
 */
@Component({
  selector: 'app-web-services',
  templateUrl: './web-services.component.html',
  providers: [ListService],
  standalone: false,
})
export class WebServicesComponent
  extends ErpTableCrudBase<PublishedWebServiceDto, CreateUpdateWebServiceDto>
  implements OnInit
{
  override readonly columns: ErpTableColumn<PublishedWebServiceDto>[] = [
    { field: 'serviceName', labelKey: 'Erp::ServiceName', type: 'code', width: 200 },
    { field: 'entityName', labelKey: 'Erp::EntityName', width: 180 },
    { field: 'objectType', labelKey: 'Erp::ObjectType', type: 'custom', width: 130 },
    {
      field: 'excludedFields',
      labelKey: 'Erp::ExcludedFields',
      type: 'text',
      width: 220,
      cellClass: 'text-secondary small font-monospace',
    },
    { field: 'published', labelKey: 'Erp::Published', type: 'switch', width: 120, sortable: false },
    // Where other systems call a published service; blank until it is published.
    { field: 'url', labelKey: 'Erp::Endpoint', type: 'custom', width: 420, sortable: false, filterable: false },
    { field: 'requestBody', labelKey: 'Erp::RequestBody', type: 'custom', width: 260, sortable: false, filterable: false },
    { field: 'fieldsUrl', labelKey: 'Erp::FieldsUrl', type: 'custom', width: 480, sortable: false, filterable: false },
  ];

  /** The API host the service's paths are relative to, e.g. `https://erp.example.com`. */
  readonly apiUrl = (inject(EnvironmentService).getApiUrl('Erp') ?? '').replace(/\/+$/, '');

  override actions: ErpTableAction<PublishedWebServiceDto>[] = [
    {
      key: 'edit',
      title: 'Erp::Edit',
      icon: 'fas fa-pen',
      btnClass: 'btn-outline-primary',
      permission: 'Erp.WebServices.Manage',
      action: row => this.openEdit(row),
    },
    {
      key: 'delete',
      title: 'Erp::Delete',
      icon: 'fas fa-trash',
      btnClass: 'btn-outline-danger',
      permission: 'Erp.WebServices.Manage',
      action: row => this.remove(row),
    },
  ];

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

  // The service returns every web service at once, so the table filters them in memory.
  protected override usesListService = true;

  protected override refresh(): void {
    this.list.get();
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

  /**
   * The query body the server builds for the service: its fields, ordering and paging. A server
   * that does not send one yet still gets a body that names the service.
   */
  requestBodyOf(service: PublishedWebServiceDto): string {
    return service.requestBody || JSON.stringify({ serviceName: service.serviceName });
  }

  /** The body indented, as it is pasted into Postman or code. */
  requestBodyToCopy(service: PublishedWebServiceDto): string {
    return JSON.stringify(JSON.parse(this.requestBodyOf(service)), null, 2);
  }

  /** Copies an address or body without opening the row. */
  copy(text: string | undefined, event: Event): void {
    event.stopPropagation();
    if (!text) {
      this.toaster.warn('Erp::NothingToCopy');
      return;
    }
    navigator.clipboard
      .writeText(text)
      .then(() => this.toaster.success('Erp::CopiedToClipboard'))
      .catch(() => this.toaster.error('AbpUi::DefaultErrorMessage'));
  }

  setPublished(service: PublishedWebServiceDto, published: boolean): void {
    this.service
      .setPublished({ id: service.id!, published })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.toaster.success('Erp::SavedSuccessfully');
        this.refresh();
      });
  }
}
