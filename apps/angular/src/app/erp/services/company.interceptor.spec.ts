import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CompanyInterceptor } from './company.interceptor';
import { CompanyService } from './company.service';

describe('CompanyInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let activeCompanyId: string | null;

  beforeEach(() => {
    activeCompanyId = 'company-1';
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: CompanyService, useValue: { getActiveCompanyId: () => activeCompanyId } },
        { provide: HTTP_INTERCEPTORS, useClass: CompanyInterceptor, multi: true },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('adds X-Company-Id to /api/erp requests when a company is active', () => {
    http.get('https://localhost:44300/api/erp/customer').subscribe();
    const req = httpMock.expectOne('https://localhost:44300/api/erp/customer');
    expect(req.request.headers.get('X-Company-Id')).toBe('company-1');
    req.flush([]);
  });

  it('does not add the header to non ERP requests', () => {
    http.get('/api/identity/users').subscribe();
    const req = httpMock.expectOne('/api/identity/users');
    expect(req.request.headers.has('X-Company-Id')).toBeFalse();
    req.flush([]);
  });

  it('does not add the header when no company is active', () => {
    activeCompanyId = null;
    http.get('/api/erp/customer').subscribe();
    const req = httpMock.expectOne('/api/erp/customer');
    expect(req.request.headers.has('X-Company-Id')).toBeFalse();
    req.flush([]);
  });
});
