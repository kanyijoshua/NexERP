import { Injectable, Injector } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CompanyService } from './company.service';

/**
 * Adds the `X-Company-Id` header to every ERP API call.
 * Registered once in the root `AppModule` so that all lazy ERP modules share it.
 */
@Injectable()
export class CompanyInterceptor implements HttpInterceptor {
  // CompanyService depends on RestService -> HttpClient, so it is resolved lazily
  // to keep the root interceptor out of HttpClient's construction chain.
  private companyService?: CompanyService;

  constructor(private injector: Injector) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    this.companyService = this.companyService ?? this.injector.get(CompanyService);
    const activeCompanyId = this.companyService.getActiveCompanyId();
    if (activeCompanyId && request.url.includes('/api/erp')) {
      request = request.clone({
        setHeaders: {
          'X-Company-Id': activeCompanyId,
        },
      });
    }
    return next.handle(request);
  }
}
