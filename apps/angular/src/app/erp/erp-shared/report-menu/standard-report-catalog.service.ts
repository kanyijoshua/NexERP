import { Injectable, inject } from '@angular/core';
import { StandardReportDto, StandardReportService } from '@proxy/reporting';
import { Observable, catchError, map, of, shareReplay } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/**
 * The standard reports the current user may run, read once and shared by every Report menu. The
 * list is read again after a company switch, since modules (and so reports) differ per company.
 */
@Injectable({ providedIn: 'root' })
export class StandardReportCatalogService {
  private readonly service = inject(StandardReportService);
  private reports$?: Observable<StandardReportDto[]>;

  constructor() {
    inject(CompanyService).companyChanged$.subscribe(() => (this.reports$ = undefined));
  }

  /** The reports of `codes` the user may run, in the order given. */
  available(codes: readonly string[]): Observable<StandardReportDto[]> {
    if (!codes.length) {
      return of([]);
    }
    return this.all().pipe(
      map(reports => {
        const byCode = new Map(reports.map(r => [(r.code ?? '').toLowerCase(), r]));
        return codes.map(code => byCode.get(code.toLowerCase())).filter((r): r is StandardReportDto => !!r);
      }),
    );
  }

  private all(): Observable<StandardReportDto[]> {
    this.reports$ ??= this.service.getList().pipe(
      map(reports => reports ?? []),
      catchError(() => {
        this.reports$ = undefined;
        return of([]);
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
    return this.reports$;
  }
}
