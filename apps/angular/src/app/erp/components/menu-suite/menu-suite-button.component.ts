import { ConfigStateService, CoreModule } from '@abp/ng.core';
import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MenuSuiteService } from './menu-suite.service';

/** The top bar button for the menu suite. */
@Component({
  selector: 'app-menu-suite-button',
  template: `
    @if (signedIn()?.isAuthenticated) {
      <button
        type="button"
        class="menu-suite-button"
        (click)="suite.open()"
        [matTooltip]="'Erp::ExploreMenu' | abpLocalization"
        [attr.aria-label]="'Erp::MenuSuite' | abpLocalization"
      >
        <i class="fas fa-bars" aria-hidden="true"></i>
        <span>{{ 'Erp::MenuSuite' | abpLocalization }}</span>
      </button>
    }
  `,
  styles: `
    .menu-suite-button {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 0.75rem;
      border: 0;
      background: none;
      color: var(--bs-navbar-color);
      font-size: 0.95rem;
    }

    .menu-suite-button:hover,
    .menu-suite-button:focus-visible {
      color: var(--bs-navbar-hover-color);
    }
  `,
  imports: [CoreModule, MatTooltipModule],
})
export class MenuSuiteButtonComponent {
  readonly suite = inject(MenuSuiteService);
  readonly signedIn = toSignal(inject(ConfigStateService).getOne$('currentUser'));
}
