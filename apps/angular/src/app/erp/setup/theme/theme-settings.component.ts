import { ToasterService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { ErpCornerStyle, ErpNavbarStyle, ErpThemeDto } from '@proxy/theming';
import { AppThemeService, DEFAULT_THEME } from '../../services/app-theme.service';

/** A ready-made theme the admin can start from. */
interface ThemePreset {
  key: string;
  theme: ErpThemeDto;
}

const HEX = /^#[0-9a-f]{6}$/i;

export const THEME_PRESETS: ThemePreset[] = [
  { key: 'Plum', theme: DEFAULT_THEME },
  {
    key: 'Teal',
    theme: {
      primaryColor: '#005A64',
      accentColor: '#00707A',
      navbarStyle: ErpNavbarStyle.Light,
      cornerStyle: ErpCornerStyle.Square,
    },
  },
  {
    key: 'Ocean',
    theme: {
      primaryColor: '#1E3A5F',
      accentColor: '#1971C2',
      navbarStyle: ErpNavbarStyle.Brand,
      cornerStyle: ErpCornerStyle.Rounded,
    },
  },
  {
    key: 'Forest',
    theme: {
      primaryColor: '#1B4332',
      accentColor: '#2B8A3E',
      navbarStyle: ErpNavbarStyle.Brand,
      cornerStyle: ErpCornerStyle.Rounded,
    },
  },
  {
    key: 'Graphite',
    theme: {
      primaryColor: '#2B2F36',
      accentColor: '#5F3DC4',
      navbarStyle: ErpNavbarStyle.Brand,
      cornerStyle: ErpCornerStyle.Rounded,
    },
  },
  {
    key: 'Sunset',
    theme: {
      primaryColor: '#7C2D12',
      accentColor: '#C2410C',
      navbarStyle: ErpNavbarStyle.Light,
      cornerStyle: ErpCornerStyle.Rounded,
    },
  },
];

/**
 * Setup → Theme: the tenant's colours, top bar and corners. Every change previews live across the
 * whole app; nothing is kept until it is saved, and leaving the page unsaved puts the saved theme
 * back.
 */
@Component({
  selector: 'app-theme-settings',
  templateUrl: './theme-settings.component.html',
  styleUrls: ['./theme-settings.component.scss'],
  standalone: false,
})
export class ThemeSettingsComponent implements OnInit, OnDestroy {
  private readonly themes = inject(AppThemeService);
  private readonly toaster = inject(ToasterService);

  readonly ErpNavbarStyle = ErpNavbarStyle;
  readonly ErpCornerStyle = ErpCornerStyle;
  readonly presets = THEME_PRESETS;

  readonly draft = signal<ErpThemeDto>({ ...this.themes.saved() });
  readonly busy = signal(false);

  readonly valid = computed(() => HEX.test(this.draft().primaryColor) && HEX.test(this.draft().accentColor));

  readonly dirty = computed(() => !sameTheme(this.draft(), this.themes.saved()));

  ngOnInit(): void {
    this.draft.set({ ...this.themes.saved() });
  }

  ngOnDestroy(): void {
    if (this.dirty()) {
      this.themes.revert();
    }
  }

  isPreset(preset: ThemePreset): boolean {
    return sameTheme(preset.theme, this.draft());
  }

  usePreset(preset: ThemePreset): void {
    this.update({ ...preset.theme });
  }

  setColor(field: 'primaryColor' | 'accentColor', value: string): void {
    const color = value.trim().startsWith('#') ? value.trim() : `#${value.trim()}`;
    this.update({ ...this.draft(), [field]: color.toUpperCase() });
  }

  setNavbar(style: ErpNavbarStyle): void {
    this.update({ ...this.draft(), navbarStyle: style });
  }

  setCorners(style: ErpCornerStyle): void {
    this.update({ ...this.draft(), cornerStyle: style });
  }

  discard(): void {
    this.draft.set({ ...this.themes.saved() });
    this.themes.revert();
  }

  save(): void {
    this.busy.set(true);
    this.themes.save(this.draft()).subscribe({
      next: saved => {
        this.busy.set(false);
        this.draft.set({ ...saved });
        this.toaster.success('Erp::ThemeSaved');
      },
      error: () => this.busy.set(false),
    });
  }

  reset(): void {
    this.busy.set(true);
    this.themes.reset().subscribe({
      next: saved => {
        this.busy.set(false);
        this.draft.set({ ...saved });
        this.toaster.success('Erp::ThemeReset');
      },
      error: () => this.busy.set(false),
    });
  }

  /** Every change shows at once; an invalid colour is simply not shown until it is complete. */
  private update(theme: ErpThemeDto): void {
    this.draft.set(theme);

    if (HEX.test(theme.primaryColor) && HEX.test(theme.accentColor)) {
      this.themes.preview(theme);
    }
  }
}

function sameTheme(a: ErpThemeDto, b: ErpThemeDto): boolean {
  return (
    a.primaryColor.toUpperCase() === b.primaryColor.toUpperCase() &&
    a.accentColor.toUpperCase() === b.accentColor.toUpperCase() &&
    a.navbarStyle === b.navbarStyle &&
    a.cornerStyle === b.cornerStyle
  );
}
