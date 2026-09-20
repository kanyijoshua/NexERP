import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DataExportService, ExportableEntityDto } from '@proxy/exporting';
import {
  CreateUpdateWebhookSubscriptionDto,
  EntityChangeKind,
  WebhookDeliveryDto,
  WebhookDeliveryStatus,
  WebhookSubscriptionDto,
  WebhookSubscriptionService,
} from '@proxy/integration';
import { Observable, map } from 'rxjs';
import { CrudListBase } from '../../erp-shared';

/**
 * Outbound notifications to other systems, and the log of every call made.
 * Mirrors Business Central's webhook subscriptions.
 */
@Component({
  selector: 'app-webhooks',
  templateUrl: './webhooks.component.html',
  providers: [ListService],
})
export class WebhooksComponent
  extends CrudListBase<WebhookSubscriptionDto, CreateUpdateWebhookSubscriptionDto>
  implements OnInit
{
  readonly DeliveryStatus = WebhookDeliveryStatus;

  entities: ExportableEntityDto[] = [];

  /** Shown once, straight after the secret is issued: it cannot be read back afterwards. */
  newSecret: string | null = null;

  deliveriesOwner: WebhookSubscriptionDto | null = null;
  deliveries: WebhookDeliveryDto[] = [];
  isDeliveriesModalOpen = false;

  constructor(
    private readonly service: WebhookSubscriptionService,
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
  ): Observable<PagedResultDto<WebhookSubscriptionDto>> =>
    this.service
      .getList()
      .pipe(map(result => ({ items: result.items ?? [], totalCount: result.items?.length ?? 0 })));

  protected create = (input: CreateUpdateWebhookSubscriptionDto) => this.service.create(input);
  protected update = (id: string, input: CreateUpdateWebhookSubscriptionDto) =>
    this.service.update(id, input);
  protected delete = (id: string) => this.service.delete(id);

  protected buildForm(item?: WebhookSubscriptionDto): FormGroup {
    const kinds = item?.changeKinds ?? EntityChangeKind.All;

    return this.fb.group({
      name: [item?.name ?? '', [Validators.required, Validators.maxLength(100)]],
      entityName: [
        { value: item?.entityName ?? '', disabled: !!item },
        [Validators.required, Validators.maxLength(100)],
      ],
      endpointUrl: [item?.endpointUrl ?? '', [Validators.required, Validators.maxLength(500)]],
      // The flags are edited as three switches and folded back into one value on save.
      notifyCreated: [(kinds & EntityChangeKind.Created) === EntityChangeKind.Created],
      notifyUpdated: [(kinds & EntityChangeKind.Updated) === EntityChangeKind.Updated],
      notifyDeleted: [(kinds & EntityChangeKind.Deleted) === EntityChangeKind.Deleted],
      active: [item?.active ?? true],
    });
  }

  protected override toInput(form: FormGroup): CreateUpdateWebhookSubscriptionDto {
    const value = form.getRawValue();

    const changeKinds =
      (value.notifyCreated ? EntityChangeKind.Created : 0) |
      (value.notifyUpdated ? EntityChangeKind.Updated : 0) |
      (value.notifyDeleted ? EntityChangeKind.Deleted : 0);

    return {
      name: value.name,
      entityName: value.entityName,
      endpointUrl: value.endpointUrl,
      changeKinds,
      active: value.active,
    };
  }

  override save(): void {
    const isNew = !this.selected;

    // The secret only exists on a create, and only in that one response.
    if (!isNew) {
      super.save();
      return;
    }

    if (!this.form || this.form.invalid) {
      this.form?.markAllAsTouched();
      return;
    }

    this.isBusy = true;
    this.service
      .create(this.toInput(this.form))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: created => {
          this.isBusy = false;
          this.isModalOpen = false;
          this.newSecret = created.secret ?? null;
          this.toaster.success('Erp::SavedSuccessfully');
          this.list.get();
        },
        error: () => (this.isBusy = false),
      });
  }

  regenerateSecret(subscription: WebhookSubscriptionDto): void {
    this.service
      .regenerateSecret(subscription.id!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.newSecret = result.secret ?? null;
        this.list.get();
      });
  }

  sendTest(subscription: WebhookSubscriptionDto): void {
    this.service
      .sendTest(subscription.id!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.toaster.success('Erp::TestQueued'));
  }

  openDeliveries(subscription: WebhookSubscriptionDto): void {
    this.deliveriesOwner = subscription;
    this.deliveries = [];
    this.isDeliveriesModalOpen = true;

    this.service
      .getDeliveries({ subscriptionId: subscription.id, maxResultCount: 50, skipCount: 0 } as never)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.deliveries = result.items ?? []));
  }

  retry(delivery: WebhookDeliveryDto): void {
    this.service
      .retryDelivery(delivery.id!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.toaster.success('Erp::RetryQueued');
        if (this.deliveriesOwner) {
          this.openDeliveries(this.deliveriesOwner);
        }
      });
  }

  statusClass(status: WebhookDeliveryStatus): string {
    switch (status) {
      case WebhookDeliveryStatus.Delivered:
        return 'bg-success';
      case WebhookDeliveryStatus.Failed:
        return 'bg-warning text-dark';
      case WebhookDeliveryStatus.Abandoned:
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }

  changeKindText(kinds: EntityChangeKind): string {
    const parts: string[] = [];

    if ((kinds & EntityChangeKind.Created) === EntityChangeKind.Created) {
      parts.push('Erp::Created');
    }
    if ((kinds & EntityChangeKind.Updated) === EntityChangeKind.Updated) {
      parts.push('Erp::Updated');
    }
    if ((kinds & EntityChangeKind.Deleted) === EntityChangeKind.Deleted) {
      parts.push('Erp::Deleted');
    }

    return parts.join(',');
  }
}
