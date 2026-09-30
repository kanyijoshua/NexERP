import { DOCUMENT } from '@angular/common';
import { Injectable, NgZone, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MobileNavigationService {
  private readonly document = inject(DOCUMENT);
  private readonly zone = inject(NgZone);

  private isInitialized = false;
  private resizeListener: (() => void) | null = null;
  private clickListener: ((e: MouseEvent) => void) | null = null;
  private keydownListener: ((e: KeyboardEvent) => void) | null = null;
  private touchStartX = 0;
  private touchStartY = 0;

  public init(): void {
    if (this.isInitialized || typeof window === 'undefined') {
      return;
    }

    this.isInitialized = true;
    this.enhanceMobileNavbar();

    // Listen to window resize and orientation change
    this.zone.runOutsideAngular(() => {
      this.resizeListener = () => {
        this.enhanceMobileNavbar();
      };
      window.addEventListener('resize', this.resizeListener);

      // Global click listener for backdrop and nav-link close behavior
      this.clickListener = (e: MouseEvent) => {
        const target = e.target as HTMLElement;
        if (!target) return;

        const navbar = this.document.querySelector('#main-navbar');
        const toggler = this.document.querySelector<HTMLButtonElement>(
          '#main-navbar .navbar-toggler',
        );
        const collapse = this.document.querySelector('#main-navbar-collapse');

        if (!navbar || !toggler || !collapse) return;

        // 1. Click on custom drawer close button
        if (target.closest('.erp-mobile-sidenav-close')) {
          toggler.click();
          return;
        }

        const isSmallScreen = window.innerWidth < 992;
        if (!isSmallScreen) return;

        const isOpen =
          toggler.getAttribute('aria-expanded') === 'true' ||
          collapse.classList.contains('show') ||
          !collapse.querySelector('.abp-collapse-margin-collapsed');

        if (!isOpen) return;

        // 2. Click on a navigation route link inside the drawer
        if (target.closest('a.nav-link:not(.dropdown-toggle), a.dropdown-item, [routerLink]')) {
          // Allow routing, then close drawer
          setTimeout(() => {
            if (toggler.getAttribute('aria-expanded') === 'true') {
              toggler.click();
            }
          }, 150);
          return;
        }

        // 3. Click outside the sidenav drawer (backdrop)
        if (
          !collapse.contains(target) &&
          !toggler.contains(target) &&
          !target.closest('#main-navbar-collapse')
        ) {
          toggler.click();
        }
      };
      this.document.addEventListener('click', this.clickListener, true);

      // Keyboard: Escape key closes the sidenav
      this.keydownListener = (e: KeyboardEvent) => {
        if (e.key === 'Escape') {
          const toggler = this.document.querySelector<HTMLButtonElement>(
            '#main-navbar .navbar-toggler',
          );
          if (toggler && toggler.getAttribute('aria-expanded') === 'true') {
            toggler.click();
          }
        }
      };
      this.document.addEventListener('keydown', this.keydownListener);

      // Touch gesture: swipe left on sidenav to close
      this.document.addEventListener(
        'touchstart',
        (e: TouchEvent) => {
          if (e.touches.length === 1) {
            this.touchStartX = e.touches[0].clientX;
            this.touchStartY = e.touches[0].clientY;
          }
        },
        { passive: true },
      );

      this.document.addEventListener(
        'touchend',
        (e: TouchEvent) => {
          const collapse = this.document.querySelector('#main-navbar-collapse');
          const toggler = this.document.querySelector<HTMLButtonElement>(
            '#main-navbar .navbar-toggler',
          );
          if (!collapse || !toggler) return;

          const isOpen = toggler.getAttribute('aria-expanded') === 'true';
          if (!isOpen) return;

          const touchEndX = e.changedTouches[0].clientX;
          const touchEndY = e.changedTouches[0].clientY;

          const deltaX = touchEndX - this.touchStartX;
          const deltaY = Math.abs(touchEndY - this.touchStartY);

          // Swipe left by at least 60px with minimal vertical movement
          if (deltaX < -60 && deltaY < 50) {
            toggler.click();
          }
        },
        { passive: true },
      );
    });
  }

  public destroy(): void {
    if (this.resizeListener && typeof window !== 'undefined') {
      window.removeEventListener('resize', this.resizeListener);
      this.resizeListener = null;
    }
    if (this.clickListener) {
      this.document.removeEventListener('click', this.clickListener, true);
      this.clickListener = null;
    }
    if (this.keydownListener) {
      this.document.removeEventListener('keydown', this.keydownListener);
      this.keydownListener = null;
    }
    this.isInitialized = false;
  }

  /**
   * Enhances `#main-navbar-collapse` by injecting a top branding header with close button.
   */
  public enhanceMobileNavbar(): void {
    const collapse = this.document.querySelector<HTMLElement>('#main-navbar-collapse');
    if (!collapse) {
      return;
    }

    let drawerHeader = collapse.querySelector<HTMLElement>('.erp-mobile-sidenav-header');
    if (!drawerHeader) {
      drawerHeader = this.document.createElement('div');
      drawerHeader.className = 'erp-mobile-sidenav-header';
      drawerHeader.innerHTML = `
        <div class="erp-mobile-sidenav-brand">
          <div class="erp-mobile-sidenav-logo-badge">N</div>
          <span class="erp-mobile-sidenav-title">NexERP</span>
        </div>
        <button type="button" class="btn btn-sm btn-icon erp-mobile-sidenav-close" aria-label="Close menu" title="Close">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>
      `;

      if (collapse.firstChild) {
        collapse.insertBefore(drawerHeader, collapse.firstChild);
      } else {
        collapse.appendChild(drawerHeader);
      }
    }
  }
}
