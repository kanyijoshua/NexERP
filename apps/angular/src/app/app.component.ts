import { ConfigStateService } from '@abp/ng.core';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, switchMap } from 'rxjs/operators';
import { AppThemeService } from './erp/services/app-theme.service';
import { CompanyService } from './erp/services/company.service';
import { ModuleRoutesService } from './erp/services/module-routes.service';

@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar></abp-loader-bar>
    <abp-dynamic-layout></abp-dynamic-layout>
  `,
  standalone: false,
})
export class AppComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly config = inject(ConfigStateService);
  private readonly companyService = inject(CompanyService);
  private readonly moduleRoutes = inject(ModuleRoutesService);
  private readonly theme = inject(AppThemeService);

  /**
   * The menu shows only the modules this company runs, so it is rebuilt once the user is known
   * and again whenever they switch company — each company has its own set switched on.
   */
  ngOnInit(): void {
    // The tenant's colours, remembered from the last visit and refreshed once signed in.
    this.theme.init().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();

    this.config
      .getOne$('currentUser')
      .pipe(
        filter(user => !!user?.isAuthenticated),
        switchMap(() => this.moduleRoutes.refresh()),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();

    this.companyService.companyChanged$
      .pipe(
        switchMap(() => this.moduleRoutes.refresh()),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }
}
