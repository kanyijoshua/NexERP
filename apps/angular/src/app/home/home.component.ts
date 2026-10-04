import { AuthService } from '@abp/ng.core';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivityCueDto, ActivityCueTone, HomeService, HomeSummaryDto } from '@proxy/home';
import { MenuSuiteService } from '../erp/components/menu-suite/menu-suite.service';
import { CompanyService } from '../erp/services/company.service';

/** Icon colours for the app grid, all dark enough for a white glyph. */
const APP_COLORS = [
  '#C2185B',
  '#E65100',
  '#2E7D32',
  '#1565C0',
  '#6A1B9A',
  '#00838F',
  '#AD1457',
  '#4E342E',
  '#283593',
  '#00695C',
  '#BF360C',
  '#5D4037',
];

/** One heading of the Activities area, with the cues that sit under it. */
interface CueGroup {
  name: string;
  cues: ActivityCueDto[];
}

/**
 * The landing page, laid out as a Role Center.
 * <p>
 * The company and the actions you can take sit at the top; under them, the activity cues as
 * coloured tiles, grouped by the area they count (Approvals, Finance, Sales…). A tile says how
 * many things are waiting and opens the list they were counted from.
 * </p>
 */
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  standalone: false,
})
export class HomeComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly authService = inject(AuthService);
  private readonly home = inject(HomeService);
  private readonly companies = inject(CompanyService);
  private readonly menuSuite = inject(MenuSuiteService);

  // Signals, so the page redraws when the data arrives whatever change detection its layout uses.
  readonly summary = signal<HomeSummaryDto | null>(null);
  readonly cueGroups = signal<CueGroup[]>([]);
  readonly busy = signal(false);

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

  ngOnInit(): void {
    if (this.hasLoggedIn) {
      this.load();

      // Every figure belongs to one company: switching company redraws the page.
      this.companies.companyChanged$
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.load());
    }
  }

  openMenuSuite(): void {
    this.menuSuite.open();
  }

  login(): void {
    const returnUrl = localStorage.getItem('nexerp_last_route');
    if (returnUrl && returnUrl !== '/') {
      this.authService.navigateToLogin({ returnUrl });
    } else {
      this.authService.navigateToLogin();
    }
  }

  /**
   * A zero reads quiet (grey); otherwise the tone picks the colour: overdue red, waiting amber,
   * anything else the brand colour.
   */
  toneClass(cue: ActivityCueDto): string {
    if (cue.value <= 0) {
      return 'cue--quiet';
    }

    switch (cue.tone) {
      case ActivityCueTone.Overdue:
        return 'cue--overdue';
      case ActivityCueTone.Attention:
        return 'cue--attention';
      default:
        return 'cue--neutral';
    }
  }

  /**
   * Each app keeps its own colour, so apps can be told apart at a glance. The
   * colour comes from the app's code, so it never moves when apps are switched on or off.
   */
  appColor(code: string | undefined): string {
    let hash = 0;

    for (const ch of code ?? '') {
      hash = (hash * 31 + ch.charCodeAt(0)) | 0;
    }

    return APP_COLORS[Math.abs(hash) % APP_COLORS.length];
  }

  /** Large amounts get a smaller figure so they still fit on one line of the tile. */
  isLongValue(cue: ActivityCueDto): boolean {
    return Math.abs(cue.value) >= 1_000_000 || (cue.isAmount && Math.abs(cue.value) >= 100_000);
  }

  private load(): void {
    this.busy.set(true);
    this.home
      .getSummary()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: summary => {
          this.busy.set(false);
          this.summary.set(summary);
          this.cueGroups.set(HomeComponent.group(summary.cues ?? []));
        },
        error: () => this.busy.set(false),
      });
  }

  /** Cues are laid out under their group heading, in the order the server sent them. */
  private static group(cues: ActivityCueDto[]): CueGroup[] {
    const groups: CueGroup[] = [];

    for (const cue of cues) {
      const name = cue.group ?? '';
      const existing = groups.find(g => g.name === name);

      if (existing) {
        existing.cues.push(cue);
      } else {
        groups.push({ name, cues: [cue] });
      }
    }

    return groups;
  }
}
