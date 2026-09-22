import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { ModuleRoutesService } from './module-routes.service';

/**
 * Keeps a page of a switched-off module from being opened by its URL.
 * <p>
 * The API refuses it anyway, so this is not the guard that protects the data. It is what turns a
 * page of errors into being sent back to the home page.
 * </p>
 * <p>
 * Used as `canActivate: [moduleGuard], data: { module: 'Sales' }`.
 * </p>
 */
export const moduleGuard: CanActivateFn = route => {
  const service = inject(ModuleRoutesService);
  const router = inject(Router);

  const code = route.data?.['module'] as string | undefined;
  if (!code) {
    return true;
  }

  // Read the modules once if they have not been read yet, so a deep link on a cold start is
  // judged on the real state rather than on an empty list.
  const decide = (): boolean | ReturnType<Router['createUrlTree']> =>
    service.isEnabled(code) ? true : router.createUrlTree(['/']);

  return service.modules.length > 0 ? decide() : service.refresh().pipe(map(() => decide()));
};
