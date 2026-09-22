import { ConfigStateService } from '@abp/ng.core';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, switchMap } from 'rxjs/operators';
import { CompanyService } from './erp/services/company.service';
import { ModuleRoutesService } from './erp/services/module-routes.service';

@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar></abp-loader-bar>
    <abp-dynamic-layout></abp-dynamic-layout>
  `,
})
export class AppComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly config = inject(ConfigStateService);
  private readonly companyService = inject(CompanyService);
  private readonly moduleRoutes = inject(ModuleRoutesService);

  /**
   * The menu shows only the modules this company runs, so it is rebuilt once the user is known
   * and again whenever they switch company — each company has its own set switched on.
   */
  ngOnInit(): void {
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
