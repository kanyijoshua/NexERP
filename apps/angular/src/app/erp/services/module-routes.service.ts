import { ConfigStateService, RoutesService } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import { ErpModuleDto, ModuleService } from '@proxy/modules';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { ERP_ROUTES, ERP_ROUTE_MODULE } from '../../route.provider';

/**
 * Keeps the menu in step with the modules this company runs.
 * <p>
 * The whole ERP menu is removed and rebuilt from one definition rather than entries being taken
 * out one at a time: a module switched back on has to bring its entries back, and rebuilding is
 * the only way that cannot drift.
 * </p>
 */
@Injectable({ providedIn: 'root' })
export class ModuleRoutesService {
  private readonly routes = inject(RoutesService);
  private readonly service = inject(ModuleService);
  private readonly config = inject(ConfigStateService);

  private readonly modules$ = new BehaviorSubject<ErpModuleDto[]>([]);

  /** The modules as last read, for anything that needs to know without asking again. */
  get modules(): ErpModuleDto[] {
    return this.modules$.value;
  }

  get changed$(): Observable<ErpModuleDto[]> {
    return this.modules$.asObservable();
  }

  isEnabled(code: string): boolean {
    const module = this.modules.find(m => m.code === code);

    // Unknown until the list has been read: showing a menu entry that then disappears is better
    // than hiding one that should have been there.
    return module ? module.enabled : true;
  }

  /**
   * Reads the modules and rebuilds the menu. Safe to call before sign-in: an unauthenticated
   * request simply leaves the menu as it is.
   */
  refresh(): Observable<ErpModuleDto[]> {
    if (!this.config.getDeep('currentUser.isAuthenticated')) {
      return of([]);
    }

    return this.service.getList().pipe(
      map(result => result.items ?? []),
      catchError(() => of([] as ErpModuleDto[])),
      tap(modules => {
        this.modules$.next(modules);
        this.apply(modules);
      }),
    );
  }

  private apply(modules: ErpModuleDto[]): void {
    if (modules.length === 0) {
      return;
    }

    const off = new Set(modules.filter(m => !m.enabled).map(m => m.code));

    this.routes.remove(ERP_ROUTES.map(route => route.name));
    this.routes.add(ERP_ROUTES.filter(route => !off.has(ERP_ROUTE_MODULE[route.name])));
  }
}
