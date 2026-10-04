import { CoreModule, LocalizationService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import {
  AfterViewInit,
  Component,
  DestroyRef,
  ElementRef,
  OnInit,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { IsActiveMatchOptions, Router, RouterModule } from '@angular/router';
import { ProfileSource, RoleCenterNavItemDto } from '@proxy/profiles';
import { RoleNavigationService } from '../../services/role-navigation.service';
import { CompanyMenuComponent } from '../company-menu/company-menu.component';
import { MenuSuiteService } from '../menu-suite/menu-suite.service';

/** A link is current when the page shown is it or one of its cards (`/erp/customers/…`). */
const ACTIVE_MATCH: IsActiveMatchOptions = {
  paths: 'subset',
  queryParams: 'ignored',
  fragment: 'ignored',
  matrixParams: 'ignored',
};

/**
 * The role center navigation bar, shown under the top bar on every ERP page.
 * <p>
 * The bar at the top of a role center: the company, then the links and
 * menus of the user's profile (Customers, Vendors… Finance ▾, Cash Management ▾), scrolled with
 * arrows when they do not fit, then the role the user works as and the full menu. The server
 * sends only the links the user may open in this company.
 * </p>
 */
@Component({
  selector: 'app-role-nav-bar',
  templateUrl: './role-nav-bar.component.html',
  styleUrls: ['./role-nav-bar.component.scss'],
  imports: [CoreModule, RouterModule, MatMenuModule, MatDividerModule, MatTooltipModule, CompanyMenuComponent],
})
export class RoleNavBarComponent implements OnInit, AfterViewInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);
  private readonly localization = inject(LocalizationService);
  private readonly menuSuite = inject(MenuSuiteService);
  readonly navigation = inject(RoleNavigationService);

  @ViewChild('scroller') private scrollerRef?: ElementRef<HTMLElement>;

  readonly profile = this.navigation.profile;
  readonly items = computed(() => this.profile()?.navigation ?? []);
  readonly canScrollBack = signal(false);
  readonly canScrollOn = signal(false);
  readonly busy = signal(false);

  readonly ProfileSource = ProfileSource;

  ngOnInit(): void {
    this.navigation.start();
  }

  ngAfterViewInit(): void {
    const element = this.scrollerRef?.nativeElement;
    if (!element || typeof ResizeObserver === 'undefined') {
      return;
    }

    // The arrows depend on the width of the bar and on how many links it holds: watch both.
    const observer = new ResizeObserver(() => this.updateArrows());
    observer.observe(element);
    const content = new MutationObserver(() => this.updateArrows());
    content.observe(element, { childList: true, subtree: true });
    this.destroyRef.onDestroy(() => {
      observer.disconnect();
      content.disconnect();
    });
  }

  isMenu(item: RoleCenterNavItemDto): boolean {
    return (item.children?.length ?? 0) > 0;
  }

  /** A menu reads as current when the page shown is one of its links. */
  isMenuActive(item: RoleCenterNavItemDto): boolean {
    return item.children.some(child => !!child.route && this.router.isActive(child.route, ACTIVE_MATCH));
  }

  /** Where the profile came from, under the role menu's heading. */
  sourceText(): string {
    const profile = this.profile();
    switch (profile?.source) {
      case ProfileSource.User:
        return this.localization.instant('Erp::RoleFromYourChoice');
      case ProfileSource.Role:
        return this.localization.instant('Erp::RoleFromRole', profile.roleName ?? '');
      default:
        return this.localization.instant('Erp::RoleFromDefault');
    }
  }

  changeRole(profileId: string | null): void {
    if (this.busy()) {
      return;
    }

    this.busy.set(true);
    this.navigation.change(profileId).subscribe({
      next: profile => {
        this.busy.set(false);
        this.toaster.success(this.localization.instant('Erp::RoleChanged', profile.displayName ?? ''));
      },
      error: () => this.busy.set(false),
    });
  }

  openMenuSuite(): void {
    this.menuSuite.open();
  }

  /** Moves the links by most of the visible width. */
  scroll(direction: -1 | 1): void {
    const element = this.scrollerRef?.nativeElement;
    element?.scrollBy({ left: direction * element.clientWidth * 0.8, behavior: 'smooth' });
  }

  updateArrows(): void {
    const element = this.scrollerRef?.nativeElement;
    if (!element) {
      return;
    }

    this.canScrollBack.set(element.scrollLeft > 1);
    this.canScrollOn.set(element.scrollLeft + element.clientWidth < element.scrollWidth - 1);
  }
}
