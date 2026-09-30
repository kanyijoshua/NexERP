import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { BehaviorSubject, Observable, Subject, combineLatest, map, tap } from 'rxjs';

export interface CompanyDto {
  id: string;
  name: string;
  displayName: string;
  evaluationCompany: boolean;
  isDefault: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class CompanyService {
  private activeCompanyId$ = new BehaviorSubject<string | null>(
    localStorage.getItem('active_company_id'),
  );

  private companyChangedSubject = new Subject<string>();

  /**
   * Emits the new company id every time the active company actually changes.
   * Pages subscribe to this to re-query their data instead of reloading the browser.
   */
  readonly companyChanged$: Observable<string> = this.companyChangedSubject.asObservable();

  /** The active company id as a stream (replays the current value). */
  readonly activeCompany$: Observable<string | null> = this.activeCompanyId$.asObservable();

  private companiesSubject = new BehaviorSubject<CompanyDto[]>([]);

  /** The companies as last read by {@link loadCompanies}. */
  readonly companies$: Observable<CompanyDto[]> = this.companiesSubject.asObservable();

  /** The active company itself, once the list has been read; null before that. */
  readonly currentCompany$: Observable<CompanyDto | null> = combineLatest([
    this.companiesSubject,
    this.activeCompanyId$,
  ]).pipe(map(([companies, id]) => companies.find(c => c.id === id) ?? null));

  constructor(private restService: RestService) {}

  getActiveCompanyId(): string | null {
    return this.activeCompanyId$.getValue();
  }

  getActiveCompanyName(): string {
    const activeId = this.getActiveCompanyId();
    const companies = this.companiesSubject.getValue();
    const active = companies.find(c => c.id === activeId);
    return active?.displayName || active?.name || 'NexERP';
  }

  setActiveCompany(companyId: string): void {
    const changed = companyId !== this.activeCompanyId$.getValue();
    localStorage.setItem('active_company_id', companyId);
    this.activeCompanyId$.next(companyId);
    if (changed) {
      this.companyChangedSubject.next(companyId);
    }
  }

  /** The tenant's companies, default first (the API returns an ABP list result). */
  getCompanies(): Observable<CompanyDto[]> {
    return this.restService
      .request<void, { items: CompanyDto[] }>(
        { method: 'GET', url: '/api/erp/company' },
        { apiName: 'Erp' },
      )
      .pipe(map(result => result.items ?? []));
  }

  /**
   * Reads the companies once for everything that shows them (the company menu, the home page),
   * and makes sure the active one still exists.
   */
  loadCompanies(): Observable<CompanyDto[]> {
    return this.getCompanies().pipe(
      tap(list => {
        this.companiesSubject.next(list);
        this.ensureValidActiveCompany(list);
      }),
    );
  }

  /**
   * Keeps the remembered company if it still exists, otherwise falls back to the default one.
   * A remembered id goes stale when the database is recreated or the company is deleted.
   */
  ensureValidActiveCompany(companies: CompanyDto[]): string | null {
    const current = this.getActiveCompanyId();
    if (current && companies.some(c => c.id === current)) {
      return current;
    }

    const fallback = companies.find(c => c.isDefault) ?? companies[0];
    if (!fallback) {
      return null;
    }

    this.setActiveCompany(fallback.id);
    return fallback.id;
  }

  copyCompany(
    sourceCompanyId: string,
    newCompanyName: string,
    newDisplayName: string,
  ): Observable<CompanyDto> {
    return this.restService.request<unknown, CompanyDto>(
      {
        method: 'POST',
        url: '/api/erp/company/copy',
        body: { sourceCompanyId, newCompanyName, newDisplayName },
      },
      { apiName: 'Erp' },
    );
  }
}
