import { ConfigStateService, RoutesService } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import { ErpModuleDto, ModuleService } from '@proxy/modules';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, finalize, map, shareReplay, tap } from 'rxjs/operators';
import { ERP_ROUTES, ERP_ROUTE_MODULE } from '../../route.provider';

/**
 * Keeps the menu in step with the modules this company runs.
 * <p>
 * The whole ERP menu is removed and rebuilt from one definition rather than entries being taken
 * out one at a time: a module switched back on has to bring its entries back, and rebuilding is
 * the only way that cannot drift. The rebuild is skipped when the set of switched-off modules has
 * not changed, since it redraws every menu entry.
 * </p>
 */
@Injectable({ providedIn: 'root' })
export class ModuleRoutesService {
  private readonly routes = inject(RoutesService);
  private readonly service = inject(ModuleService);
  private readonly config = inject(ConfigStateService);

  private readonly modules$ = new BehaviorSubject<ErpModuleDto[]>(this.loadCachedModules());

  /** The read in flight, shared by everyone who asks while it runs. */
  private pending: Observable<ErpModuleDto[]> | null = null;

  /** The switched-off modules the menu was last built for; null until it has been built. */
  private appliedOff: string | null = null;

  private loadCachedModules(): ErpModuleDto[] {
    try {
      const raw = localStorage.getItem('nexerp_cached_modules');
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }

  private saveCachedModules(modules: ErpModuleDto[]): void {
    try {
      localStorage.setItem('nexerp_cached_modules', JSON.stringify(modules));
    } catch {}
  }

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
   * request simply leaves the menu as it is. Calls made while a read is running share it.
   */
  refresh(): Observable<ErpModuleDto[]> {
    if (!this.config.getDeep('currentUser.isAuthenticated')) {
      return of([]);
    }

    if (!this.pending) {
      this.pending = this.service.getList().pipe(
        map(result => result.items ?? []),
        catchError(() => of([] as ErpModuleDto[])),
        tap(modules => {
          if (modules.length) {
            this.saveCachedModules(modules);
            this.modules$.next(modules);
            this.apply(modules);
          }
        }),
        finalize(() => (this.pending = null)),
        shareReplay({ bufferSize: 1, refCount: false }),
      );
    }

    return this.pending;
  }

  private apply(modules: ErpModuleDto[]): void {
    const off = modules.filter(m => !m.enabled).map(m => m.code!);
    const key = [...off].sort().join('|');

    if (key === this.appliedOff) {
      return;
    }

    this.appliedOff = key;

    const offSet = new Set(off);
    this.routes.remove(ERP_ROUTES.map(route => route.name));
    this.routes.add(ERP_ROUTES.filter(route => !offSet.has(ERP_ROUTE_MODULE[route.name])));
  }
}
