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
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { IsActiveMatchOptions, NavigationEnd, Router, RouterModule } from '@angular/router';
import { filter } from 'rxjs';
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
 * The role center navigation bar, modeled after Microsoft Dynamics 365 Business Central (BC).
 * <p>
 * Tier 1 (Main Navigation Bar): Company, top-level groups (Finance ▾, Setup ▾, Reports ▾)
 * and direct links, scrolled with arrows, role switcher, and Menu Suite explorer.
 * <p>
 * Tier 2 (Business Central Sublinks Bar): Displays the sublinks (children) of the currently selected
 * or active group directly beneath the top bar in Business Central accent styling.
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
  @ViewChild('subScroller') private subScrollerRef?: ElementRef<HTMLElement>;

  readonly profile = this.navigation.profile;
  readonly items = computed(() => this.profile()?.navigation ?? []);
  readonly canScrollBack = signal(false);
  readonly canScrollOn = signal(false);
  readonly canScrollSubBack = signal(false);
  readonly canScrollSubOn = signal(false);
  readonly busy = signal(false);

  /** Explicitly selected top group; null falls back to route-based or first menu group. */
  readonly selectedGroupKey = signal<string | null>(null);

  /** The top group whose sublinks are currently displayed in the BC sublink bar. */
  readonly selectedGroup = computed<RoleCenterNavItemDto | null>(() => {
    const all = this.items();
    const key = this.selectedGroupKey();
    if (key) {
      const found = all.find(i => i.key === key);
      if (found && this.isMenu(found)) {
        return found;
      }
    }
    // Auto-detect based on active route
    const active = all.find(i => this.isMenu(i) && this.isMenuActive(i));
    if (active) {
      return active;
    }
    // Default to first menu item with children
    return all.find(i => this.isMenu(i)) ?? null;
  });

  /** The sublinks to show in the second row (BC sublink bar). */
  readonly sublinks = computed<RoleCenterNavItemDto[]>(() => {
    return this.selectedGroup()?.children ?? [];
  });

  readonly ProfileSource = ProfileSource;

  ngOnInit(): void {
    this.navigation.start();

    // When navigating to a new route, update the active group if the route is inside one
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        const matchingGroup = this.items().find(i => this.isMenu(i) && this.isMenuActive(i));
        if (matchingGroup) {
          this.selectedGroupKey.set(matchingGroup.key ?? null);
        }
        setTimeout(() => this.updateSubArrows());
      });
  }

  ngAfterViewInit(): void {
    const element = this.scrollerRef?.nativeElement;
    const subElement = this.subScrollerRef?.nativeElement;
    if (typeof ResizeObserver === 'undefined') {
      return;
    }

    if (element) {
      const observer = new ResizeObserver(() => this.updateArrows());
      observer.observe(element);
      const content = new MutationObserver(() => this.updateArrows());
      content.observe(element, { childList: true, subtree: true });
      this.destroyRef.onDestroy(() => {
        observer.disconnect();
        content.disconnect();
      });
    }

    if (subElement) {
      const subObserver = new ResizeObserver(() => this.updateSubArrows());
      subObserver.observe(subElement);
      const subContent = new MutationObserver(() => this.updateSubArrows());
      subContent.observe(subElement, { childList: true, subtree: true });
      this.destroyRef.onDestroy(() => {
        subObserver.disconnect();
        subContent.disconnect();
      });
    }
  }

  isMenu(item: RoleCenterNavItemDto): boolean {
    return (item.children?.length ?? 0) > 0;
  }

  /** A menu reads as current when the page shown is one of its links. */
  isMenuActive(item: RoleCenterNavItemDto): boolean {
    return (item.children ?? []).some(child => {
      if (child.route && this.router.isActive(child.route, ACTIVE_MATCH)) {
        return true;
      }
      if (child.children?.length) {
        return child.children.some(c => !!c.route && this.router.isActive(c.route, ACTIVE_MATCH));
      }
      return false;
    });
  }

  /** Whether a top-level item is selected or its sublinks match the active route. */
  isItemActive(item: RoleCenterNavItemDto): boolean {
    return this.selectedGroup()?.key === item.key || this.isMenuActive(item);
  }

  /** Whether a sublink in the second row matches the active route. */
  isSublinkActive(link: RoleCenterNavItemDto): boolean {
    if (link.route && this.router.isActive(link.route, ACTIVE_MATCH)) {
      return true;
    }
    if (link.children?.length) {
      return link.children.some(c => !!c.route && this.router.isActive(c.route, ACTIVE_MATCH));
    }
    return false;
  }

  selectGroup(item: RoleCenterNavItemDto): void {
    if (this.isMenu(item)) {
      this.selectedGroupKey.set(item.key ?? null);
      setTimeout(() => this.updateSubArrows());
    }
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

  /** Moves the sublinks by most of the visible width. */
  scrollSub(direction: -1 | 1): void {
    const element = this.subScrollerRef?.nativeElement;
    element?.scrollBy({ left: direction * element.clientWidth * 0.8, behavior: 'smooth' });
  }

  updateSubArrows(): void {
    const element = this.subScrollerRef?.nativeElement;
    if (!element) {
      return;
    }

    this.canScrollSubBack.set(element.scrollLeft > 1);
    this.canScrollSubOn.set(element.scrollLeft + element.clientWidth < element.scrollWidth - 1);
  }
}

