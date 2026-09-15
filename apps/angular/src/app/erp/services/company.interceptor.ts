import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CompanyService } from './company.service';

@Injectable()
export class CompanyInterceptor implements HttpInterceptor {
  constructor(private companyService: CompanyService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
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
