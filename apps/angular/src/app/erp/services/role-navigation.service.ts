import { ConfigStateService } from '@abp/ng.core';
import { Injectable, inject, signal } from '@angular/core';
import { MyProfileDto, ProfileService } from '@proxy/profiles';
import { Observable, catchError, filter, map, merge, of, switchMap, tap } from 'rxjs';
import { CompanyService } from './company.service';

/**
 * The profile (role) the signed-in user works as, and the role center navigation it gives them.
 * <p>
 * Loaded once at sign-in and again when the company changes, because modules are switched on per
 * company and the server leaves out links to modules that are off. Changing the profile reloads
 * it from the answer the server sends back.
 * </p>
 */
@Injectable({ providedIn: 'root' })
export class RoleNavigationService {
  private readonly config = inject(ConfigStateService);
  private readonly companies = inject(CompanyService);
  private readonly profiles = inject(ProfileService);

  /** Null until the first answer arrives, and while nobody is signed in. */
  readonly profile = signal<MyProfileDto | null>(null);

  private started = false;

  /** Starts loading; safe to call from every bar that is drawn. */
  start(): void {
    if (this.started) {
      return;
    }
    this.started = true;

    const signedIn$ = this.config.getOne$('currentUser').pipe(
      map(user => !!user?.isAuthenticated),
      tap(signedIn => signedIn || this.profile.set(null)),
      filter(Boolean),
    );

    // A failed load leaves the bar without links but keeps listening for the next sign-in or company.
    merge(signedIn$, this.companies.companyChanged$)
      .pipe(switchMap(() => this.profiles.getMy().pipe(catchError(() => of(null)))))
      .subscribe(profile => this.profile.set(profile));
  }

  /** Asks again, e.g. after an administrator changed which profile a role gets. */
  reload(): void {
    this.profiles
      .getMy()
      .pipe(catchError(() => of(null)))
      .subscribe(profile => this.profile.set(profile));
  }

  /** Works as another profile from now on; null goes back to what the user's roles give. */
  change(profileId: string | null): Observable<MyProfileDto> {
    return this.profiles.setMy({ profileId }).pipe(tap(profile => this.profile.set(profile)));
  }
}
