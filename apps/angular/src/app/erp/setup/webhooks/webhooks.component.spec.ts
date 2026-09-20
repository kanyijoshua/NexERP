import { ListService } from '@abp/ng.core';
import { ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { FormBuilder } from '@angular/forms';
import { TestBed } from '@angular/core/testing';
import { DataExportService } from '@proxy/exporting';
import {
  CreateUpdateWebhookSubscriptionDto,
  EntityChangeKind,
  WebhookDeliveryStatus,
  WebhookSubscriptionService,
} from '@proxy/integration';
import { of } from 'rxjs';
import { CompanyService } from '../../services/company.service';
import { WebhooksComponent } from './webhooks.component';

describe('WebhooksComponent', () => {
  let service: jasmine.SpyObj<WebhookSubscriptionService>;
  let component: WebhooksComponent;

  beforeEach(() => {
    service = jasmine.createSpyObj<WebhookSubscriptionService>('WebhookSubscriptionService', [
      'getList',
      'create',
      'update',
      'delete',
      'regenerateSecret',
      'sendTest',
      'getDeliveries',
      'retryDelivery',
    ]);

    service.getList.and.returnValue(of({ items: [] }) as never);
    service.create.and.returnValue(of({ id: 's1', secret: 'top-secret' }) as never);
    service.regenerateSecret.and.returnValue(of({ id: 's1', secret: 'fresh-secret' }) as never);
    service.sendTest.and.returnValue(of({ id: 'd1' }) as never);
    service.getDeliveries.and.returnValue(of({ items: [{ id: 'd1' }], totalCount: 1 }) as never);

    TestBed.configureTestingModule({
      providers: [
        WebhooksComponent,
        ListService,
        FormBuilder,
        { provide: WebhookSubscriptionService, useValue: service },
        { provide: DataExportService, useValue: { getEntities: () => of({ items: [] }) } },
        { provide: CompanyService, useValue: { companyChanged$: of() } },
        { provide: ToasterService, useValue: jasmine.createSpyObj('ToasterService', ['success']) },
        {
          provide: ConfirmationService,
          useValue: jasmine.createSpyObj('ConfirmationService', ['warn']),
        },
      ],
    });

    component = TestBed.inject(WebhooksComponent);
    component.ngOnInit();
  });

  /** The three switches are one flags value on the wire. */
  it('folds the notify switches into one change-kind value', () => {
    component.openCreate();
    component.form.patchValue({
      name: 'Feed',
      entityName: 'Customer',
      endpointUrl: 'https://example.test/hooks',
      notifyCreated: true,
      notifyUpdated: false,
      notifyDeleted: true,
    });

    component.save();

    const input = service.create.calls.mostRecent().args[0] as CreateUpdateWebhookSubscriptionDto;
    expect(input.changeKinds).toBe(EntityChangeKind.Created | EntityChangeKind.Deleted);
  });

  it('unfolds the change kinds back into switches when editing', () => {
    component.openEdit({
      id: 's1',
      name: 'Feed',
      entityName: 'Customer',
      endpointUrl: 'https://example.test/hooks',
      changeKinds: EntityChangeKind.Updated,
      active: true,
    } as never);

    expect(component.form.value.notifyCreated).toBeFalse();
    expect(component.form.value.notifyUpdated).toBeTrue();
    expect(component.form.value.notifyDeleted).toBeFalse();
  });

  /**
   * The secret is returned once and never read back, so the page has to catch it from the
   * create response and show it.
   */
  it('shows the secret returned when a subscription is created', () => {
    component.openCreate();
    component.form.patchValue({
      name: 'Feed',
      entityName: 'Customer',
      endpointUrl: 'https://example.test/hooks',
    });

    component.save();

    expect(component.newSecret).toBe('top-secret');
    expect(component.isModalOpen).toBeFalse();
  });

  it('shows a freshly issued secret', () => {
    component.regenerateSecret({ id: 's1' } as never);

    expect(component.newSecret).toBe('fresh-secret');
  });

  it('does not reveal a secret when an existing subscription is saved', () => {
    component.openEdit({
      id: 's1',
      name: 'Feed',
      entityName: 'Customer',
      endpointUrl: 'https://example.test/hooks',
      changeKinds: EntityChangeKind.All,
      active: true,
    } as never);
    service.update.and.returnValue(of({ id: 's1' }) as never);

    component.save();

    expect(component.newSecret).toBeNull();
    expect(service.create).not.toHaveBeenCalled();
  });

  it('loads the deliveries of one subscription', () => {
    component.openDeliveries({ id: 's1', name: 'Feed' } as never);

    expect(component.isDeliveriesModalOpen).toBeTrue();
    expect(component.deliveries.length).toBe(1);
  });

  it('colours a delivery by its status', () => {
    expect(component.statusClass(WebhookDeliveryStatus.Delivered)).toContain('success');
    expect(component.statusClass(WebhookDeliveryStatus.Failed)).toContain('warning');
    expect(component.statusClass(WebhookDeliveryStatus.Abandoned)).toContain('danger');
    expect(component.statusClass(WebhookDeliveryStatus.Pending)).toContain('secondary');
  });

  it('names the change kinds a subscription wants', () => {
    expect(component.changeKindText(EntityChangeKind.All)).toBe(
      'Erp::Created,Erp::Updated,Erp::Deleted',
    );
    expect(component.changeKindText(EntityChangeKind.Created)).toBe('Erp::Created');
  });
});
