import { TestBed } from '@angular/core/testing';
import { RestService } from '@abp/ng.core';
import { CompanyDto, CompanyService } from './company.service';

describe('CompanyService', () => {
  const cronus: CompanyDto = {
    id: 'a',
    name: 'CRONUS',
    displayName: 'CRONUS',
    evaluationCompany: false,
    isDefault: true,
  };
  const us: CompanyDto = {
    id: 'b',
    name: 'CRONUS US',
    displayName: 'CRONUS US',
    evaluationCompany: true,
    isDefault: false,
  };

  let service: CompanyService;

  beforeEach(() => {
    localStorage.removeItem('active_company_id');
    TestBed.configureTestingModule({
      providers: [
        CompanyService,
        { provide: RestService, useValue: jasmine.createSpyObj('RestService', ['request']) },
      ],
    });
    service = TestBed.inject(CompanyService);
  });

  afterEach(() => localStorage.removeItem('active_company_id'));

  it('keeps a remembered company that still exists', () => {
    service.setActiveCompany('b');

    expect(service.ensureValidActiveCompany([cronus, us])).toBe('b');
  });

  it('falls back to the default company when the remembered one is gone', () => {
    // Happens after the database is recreated, or when the company is deleted.
    service.setActiveCompany('deleted-company');

    expect(service.ensureValidActiveCompany([cronus, us])).toBe('a');
    expect(service.getActiveCompanyId()).toBe('a');
  });

  it('takes the first company when none is marked default', () => {
    expect(service.ensureValidActiveCompany([us])).toBe('b');
  });

  it('returns null when the tenant has no company at all', () => {
    expect(service.ensureValidActiveCompany([])).toBeNull();
  });

  it('announces a change only when the company really changes', () => {
    const seen: string[] = [];
    service.companyChanged$.subscribe(id => seen.push(id));

    service.setActiveCompany('a');
    service.setActiveCompany('a');
    service.setActiveCompany('b');

    expect(seen).toEqual(['a', 'b']);
  });
});
