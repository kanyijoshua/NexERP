import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

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

  private companyChangedSubject = new Subject<string>();

  /**
   * Emits the new company id every time the active company actually changes.
   * Pages subscribe to this to re-query their data instead of reloading the browser.
   */
  readonly companyChanged$: Observable<string> = this.companyChangedSubject.asObservable();

  /** The active company id as a stream (replays the current value). */
  readonly activeCompany$: Observable<string | null> = this.activeCompanyId$.asObservable();

  constructor(private restService: RestService) {}

  getActiveCompanyId(): string | null {
    return this.activeCompanyId$.getValue();
  }

  setActiveCompany(companyId: string): void {
    const changed = companyId !== this.activeCompanyId$.getValue();
    localStorage.setItem('active_company_id', companyId);
    this.activeCompanyId$.next(companyId);
    if (changed) {
      this.companyChangedSubject.next(companyId);
    }
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
