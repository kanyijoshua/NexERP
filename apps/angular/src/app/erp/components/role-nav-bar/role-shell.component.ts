import { AuthService } from '@abp/ng.core';
import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { RoleNavBarComponent } from './role-nav-bar.component';

/**
 * Wraps the home page and the ERP pages so the role navigation bar sits above each of them.
 * Signed-out visitors see the page alone.
 */
@Component({
  selector: 'app-role-shell',
  template: `
    @if (signedIn) {
      <app-role-nav-bar />
    }
    <router-outlet />
  `,
  imports: [RouterOutlet, RoleNavBarComponent],
})
export class RoleShellComponent {
  private readonly auth = inject(AuthService);

  get signedIn(): boolean {
    return this.auth.isAuthenticated;
  }
}
