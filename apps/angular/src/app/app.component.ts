import { ConfigStateService } from '@abp/ng.core';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { distinctUntilChanged, filter, switchMap } from 'rxjs/operators';
import { AppThemeService } from './erp/services/app-theme.service';
import { CompanyService } from './erp/services/company.service';
import { ModalResizeService } from './erp/services/modal-resize.service';
import { MobileNavigationService } from './erp/services/mobile-navigation.service';
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
  private readonly modalResize = inject(ModalResizeService);
  private readonly mobileNav = inject(MobileNavigationService);
  private readonly router = inject(Router);

  /**
   * The menu shows only the modules this company runs, so it is rebuilt once the user is known
   * and again whenever they switch company — each company has its own set switched on.
   */
  ngOnInit(): void {
    // Enable resizable and maximizable modals across the application
    this.modalResize.init();

    // Enable responsive off-canvas sidenav on mobile devices
    this.mobileNav.init();

    // Track active routes so that page reloads or return-from-login restores the previous page
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(event => {
        const url = event.urlAfterRedirects;
        if (url && url !== '/' && !url.startsWith('/account')) {
          try {
            localStorage.setItem('nexerp_last_route', url);
            sessionStorage.removeItem('nexerp_navigated_home');
          } catch {}
        } else if (url === '/') {
          try {
            sessionStorage.setItem('nexerp_navigated_home', 'true');
          } catch {}
        }
      });

    // The tenant's colours, remembered from the last visit and refreshed once signed in.
    this.theme.init().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();

    this.config
      .getOne$('currentUser')
      .pipe(
        filter(user => !!user?.isAuthenticated),
        // The configuration is re-read now and then; the modules only need reading per user.
        distinctUntilChanged((a, b) => a?.id === b?.id),
        switchMap(() => this.moduleRoutes.refresh()),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        // If reload defaulted to '/' but user was previously on a loaded page, restore it
        const currentUrl = this.router.url;
        const navigatedHome = sessionStorage.getItem('nexerp_navigated_home') === 'true';
        const lastRoute = localStorage.getItem('nexerp_last_route');
        if ((currentUrl === '/' || currentUrl === '') && lastRoute && lastRoute !== '/' && !navigatedHome) {
          this.router.navigateByUrl(lastRoute);
        }
      });

    this.companyService.companyChanged$
      .pipe(
        switchMap(() => this.moduleRoutes.refresh()),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }
}
