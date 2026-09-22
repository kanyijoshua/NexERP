import { AuthService } from '@abp/ng.core';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivityCueDto, ActivityCueTone, HomeService, HomeSummaryDto } from '@proxy/home';
import { ErpModuleDto } from '@proxy/modules';

/** One heading of the app grid, with the modules that sit under it. */
interface AppGroup {
  name: string;
  apps: ErpModuleDto[];
}

/**
 * The landing page.
 * <p>
 * A Business Central Role Center opens on what needs doing rather than on a menu, and Odoo opens
 * on the apps you have installed. This is both: the activity cues first, then the modules this
 * company has switched on, as tiles.
 * </p>
 */
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly authService = inject(AuthService);
  private readonly home = inject(HomeService);

  readonly ActivityCueTone = ActivityCueTone;

  summary: HomeSummaryDto | null = null;
  groups: AppGroup[] = [];
  busy = false;

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated;
  }

  /** A greeting by the clock, the way a Role Center headline reads. */
  get greetingKey(): string {
    const hour = new Date().getHours();

    if (hour < 12) {
      return 'Erp::GoodMorning';
    }

    return hour < 18 ? 'Erp::GoodAfternoon' : 'Erp::GoodEvening';
  }

  get hasAttention(): boolean {
    return (this.summary?.cues ?? []).some(cue => cue.value > 0);
  }

  ngOnInit(): void {
    if (this.hasLoggedIn) {
      this.load();
    }
  }

  login(): void {
    this.authService.navigateToLogin();
  }

  /** Overdue reads red, work waiting reads amber, and a zero always reads quiet. */
  toneClass(cue: ActivityCueDto): string {
    if (cue.value <= 0) {
      return 'border-secondary-subtle';
    }

    switch (cue.tone) {
      case ActivityCueTone.Overdue:
        return 'border-danger';
      case ActivityCueTone.Attention:
        return 'border-warning';
      default:
        return 'border-primary';
    }
  }

  toneTextClass(cue: ActivityCueDto): string {
    if (cue.value <= 0) {
      return 'text-secondary';
    }

    switch (cue.tone) {
      case ActivityCueTone.Overdue:
        return 'text-danger';
      case ActivityCueTone.Attention:
        return 'text-warning';
      default:
        return 'text-primary';
    }
  }

  private load(): void {
    this.busy = true;
    this.home
      .getSummary()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: summary => {
          this.busy = false;
          this.summary = summary;
          this.groups = HomeComponent.group(summary.apps ?? []);
        },
        error: () => (this.busy = false),
      });
  }

  /** Apps are laid out under their group heading, in the order the server sent them. */
  private static group(apps: ErpModuleDto[]): AppGroup[] {
    const groups: AppGroup[] = [];

    for (const app of apps) {
      const name = app.group ?? '';
      const existing = groups.find(g => g.name === name);

      if (existing) {
        existing.apps.push(app);
      } else {
        groups.push({ name, apps: [app] });
      }
    }

    return groups;
  }
}
