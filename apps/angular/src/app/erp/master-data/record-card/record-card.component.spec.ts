import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { CoreTestingModule } from '@abp/ng.core/testing';
import { ThemeSharedTestingModule } from '@abp/ng.theme.shared/testing';
import { DefaultQueueManager, PermissionService, QUEUE_MANAGER } from '@abp/ng.core';
import { ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { of, throwError } from 'rxjs';
import { RecordCardComponent } from './record-card.component';
import { RecordEntity, RecordEntityRegistry } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('RecordCardComponent', () => {
  let component: RecordCardComponent;
  let fixture: ComponentFixture<RecordCardComponent>;
  let registry: RecordEntityRegistry;
  let mockRouter: any;
  let mockToaster: any;
  let mockPermissions: any;
  let mockCompanyService: any;

  const mockEntity: RecordEntity = {
    key: 'testEntity',
    titleKey: 'Erp::TestEntity',
    pluralKey: 'Erp::TestEntities',
    icon: 'fas fa-star',
    permission: 'Erp.TestEntities',
    listRoute: ['/erp/test-entities'],
    columns: [],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true },
      { field: 'name', labelKey: 'Erp::Name', type: 'text' },
    ],
    getList: () => of({ items: [], totalCount: 0 }),
    get: (id: string) => of({ id, code: 'T001', name: 'Test Record' }),
    create: (input: any) => of({ id: 'new-id', ...input }),
    update: (id: string, input: any) => of({ id, ...input }),
    delete: () => of(undefined),
    toItem: (dto: any) => ({ code: dto.code ?? '', name: dto.name ?? '' }),
    newRecord: () => ({ code: '', name: '' }),
    facts: (dto: any) => [{ labelKey: 'Erp::Code', value: dto.code, type: 'text' }],
    actions: [],
  };

  const activatedRouteMock = {
    snapshot: {
      data: {}, // Simulating Angular's emptyOnly strategy where child has no direct data
    },
    parent: {
      snapshot: {
        data: { entity: 'testEntity' },
      },
    },
    paramMap: of(new Map([['id', '3a241931-197b-9937-8caa-b30effb6c8d8']])),
  };

  beforeEach(async () => {
    mockRouter = {
      navigate: jasmine.createSpy('navigate'),
    };
    mockToaster = {
      success: jasmine.createSpy('success'),
      error: jasmine.createSpy('error'),
    };
    mockPermissions = {
      getGrantedPolicy: jasmine.createSpy('getGrantedPolicy').and.returnValue(true),
      getGrantedPolicy$: jasmine.createSpy('getGrantedPolicy$').and.returnValue(of(true)),
    };
    mockCompanyService = {
      companyChanged$: of('company-1'),
    };

    await TestBed.configureTestingModule({
      declarations: [RecordCardComponent],
      imports: [CoreTestingModule.withConfig(), ThemeSharedTestingModule.withConfig()],
      providers: [
        { provide: QUEUE_MANAGER, useClass: DefaultQueueManager },
        { provide: ActivatedRoute, useValue: activatedRouteMock },
        { provide: Router, useValue: mockRouter },
        { provide: ToasterService, useValue: mockToaster },
        { provide: PermissionService, useValue: mockPermissions },
        { provide: ConfirmationService, useValue: {} },
        { provide: CompanyService, useValue: mockCompanyService },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    registry = TestBed.inject(RecordEntityRegistry);
    registry.register(mockEntity);

    fixture = TestBed.createComponent(RecordCardComponent);
    component = fixture.componentInstance;
  });

  it('resolves entity from parent route data and loads record details without hanging', () => {
    fixture.detectChanges();

    expect(component.entity).toBeDefined();
    expect(component.entity.key).toBe('testEntity');
    expect(component.id).toBe('3a241931-197b-9937-8caa-b30effb6c8d8');
    expect(component.record).toEqual({ id: '3a241931-197b-9937-8caa-b30effb6c8d8', code: 'T001', name: 'Test Record' });
    expect(component.title).toBe('T001 · Test Record');
    expect(component.form).not.toBeNull();
    expect(component.form?.get('code')?.value).toBe('T001');
    expect(component.facts.length).toBe(1);
  });

  it('handles record load failure gracefully without indefinite loading', () => {
    spyOn(mockEntity, 'get').and.returnValue(throwError(() => new Error('Not found')));

    fixture.detectChanges();

    expect(mockToaster.error).toHaveBeenCalledWith('Erp::RecordNotFound');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/erp/test-entities']);
  });
});
