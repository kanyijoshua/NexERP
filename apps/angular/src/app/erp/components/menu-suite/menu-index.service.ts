import { LocalizationService, PermissionService, RoutesService } from '@abp/ng.core';
import { Injectable, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { SuitePage, SuiteSection, buildMenuSuite, indexPages } from './menu-suite.model';

/**
 * Every page the user can open, read once from the live menu. The menu suite lays it out in
 * columns; the page search looks things up in it. Both follow permissions and the modules this
 * company has switched on, because the menu itself does.
 */
@Injectable({ providedIn: 'root' })
export class MenuIndexService {
  private readonly routes = inject(RoutesService);
  private readonly permissions = inject(PermissionService);
  private readonly localization = inject(LocalizationService);

  private readonly tree = toSignal(this.routes.visible$, { initialValue: this.routes.visible });

  readonly sections = computed<SuiteSection[]>(() =>
    buildMenuSuite(this.tree(), {
      isGranted: policy => !policy || this.permissions.getGrantedPolicy(policy),
      translate: key => this.localization.instant(key),
    }),
  );

  readonly pages = computed<SuitePage[]>(() => indexPages(this.sections()));
}
