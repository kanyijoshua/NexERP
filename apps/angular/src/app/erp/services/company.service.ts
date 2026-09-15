import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface CompanyDto {
  id: string;
  name: string;
  displayName: string;
  evaluationCompany: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class CompanyService {
  private activeCompanyId$ = new BehaviorSubject<string | null>(localStorage.getItem('active_company_id'));

  constructor(private restService: RestService) {}

  getActiveCompanyId(): string | null {
    return this.activeCompanyId$.getValue();
  }

  setActiveCompany(companyId: string): void {
    localStorage.setItem('active_company_id', companyId);
    this.activeCompanyId$.next(companyId);
  }

  getCompanies(): Observable<CompanyDto[]> {
    return this.restService.request<void, CompanyDto[]>({
      method: 'GET',
      url: '/api/erp/company',
    });
  }

  copyCompany(sourceCompanyId: string, newCompanyName: string, newDisplayName: string): Observable<CompanyDto> {
    return this.restService.request<any, CompanyDto>({
      method: 'POST',
      url: `/api/erp/company/copy`,
      body: { sourceCompanyId, newCompanyName, newDisplayName },
    });
  }
}
