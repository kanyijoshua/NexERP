import { ConfigStateService } from '@abp/ng.core';
import { DOCUMENT } from '@angular/common';
import { Injectable, inject, signal } from '@angular/core';
import { ErpCornerStyle, ErpNavbarStyle, ErpThemeDto, ThemeService } from '@proxy/theming';
import { Observable, filter, switchMap, tap } from 'rxjs';

/** The look the app starts from; the server holds the same defaults. */
export const DEFAULT_THEME: ErpThemeDto = {
  primaryColor: '#714B67',
  accentColor: '#017E84',
  navbarStyle: ErpNavbarStyle.Brand,
  cornerStyle: ErpCornerStyle.Rounded,
};

const STORAGE_KEY = 'erp_theme';
const HEX = /^#[0-9a-f]{6}$/i;

/**
 * Paints the app in the tenant's theme.
 * <p>
 * Everything the stylesheets colour with reads a few CSS custom properties (--erp-primary,
 * --erp-accent and what is derived from them), so a theme is applied by setting those on the root
 * element — no stylesheet is rebuilt. The last theme is remembered in the browser so the next
 * visit opens in the right colours before the server has answered.
 * </p>
 */
@Injectable({ providedIn: 'root' })
export class AppThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly config = inject(ConfigStateService);
  private readonly api = inject(ThemeService);

  /** The theme as saved; a preview does not change it. */
  readonly saved = signal<ErpThemeDto>(DEFAULT_THEME);

  /** Applies the remembered theme now, then the server's once someone is signed in. */
  init(): Observable<ErpThemeDto> {
    const cached = this.readCache();
    this.apply(cached ?? DEFAULT_THEME);
    this.saved.set(cached ?? DEFAULT_THEME);

    return this.config.getOne$('currentUser').pipe(
      filter(user => !!user?.isAuthenticated),
      switchMap(() => this.api.get()),
      tap(theme => this.commit(theme)),
    );
  }

  save(theme: ErpThemeDto): Observable<ErpThemeDto> {
    return this.api.save(theme).pipe(tap(saved => this.commit(saved)));
  }

  reset(): Observable<ErpThemeDto> {
    return this.api.reset().pipe(tap(saved => this.commit(saved)));
  }

  /** Shows a theme without saving it, for the settings page. */
  preview(theme: ErpThemeDto): void {
    this.apply(theme);
  }

  /** Back to the saved theme, when a preview is abandoned. */
  revert(): void {
    this.apply(this.saved());
  }

  private commit(theme: ErpThemeDto): void {
    this.saved.set(theme);
    this.apply(theme);

    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(theme));
    } catch {
      // Private windows may refuse storage; the theme still applies for this visit.
    }
  }

  private apply(theme: ErpThemeDto): void {
    const primary = HEX.test(theme.primaryColor) ? theme.primaryColor : DEFAULT_THEME.primaryColor;
    const accent = HEX.test(theme.accentColor) ? theme.accentColor : DEFAULT_THEME.accentColor;
    const root = this.document.documentElement;

    root.style.setProperty('--erp-primary', primary);
    root.style.setProperty('--erp-primary-rgb', rgb(primary));
    root.style.setProperty('--erp-on-primary', onColor(primary));
    root.style.setProperty('--erp-accent', accent);
    root.style.setProperty('--erp-accent-rgb', rgb(accent));
    root.style.setProperty('--erp-on-accent', onColor(accent));

    root.dataset['erpNavbar'] = theme.navbarStyle === ErpNavbarStyle.Light ? 'light' : 'brand';
    root.dataset['erpCorners'] = theme.cornerStyle === ErpCornerStyle.Square ? 'square' : 'rounded';
  }

  private readCache(): ErpThemeDto | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as ErpThemeDto) : null;
    } catch {
      return null;
    }
  }
}

/** "#017E84" → "1, 126, 132", for rgb(var(--x-rgb) / alpha). */
export function rgb(hex: string): string {
  const n = parseInt(hex.slice(1), 16);
  return `${(n >> 16) & 255}, ${(n >> 8) & 255}, ${n & 255}`;
}

/** White or near-black, whichever reads better on the colour (WCAG relative luminance). */
export function onColor(hex: string): string {
  const [r, g, b] = rgb(hex)
    .split(',')
    .map(c => {
      const v = Number(c) / 255;
      return v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4;
    });
  const luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;

  // Contrast with white is (1.05)/(L+0.05); with #1f2328 (L≈0.016) it is (L+0.05)/0.066.
  return 1.05 / (luminance + 0.05) >= (luminance + 0.05) / 0.066 ? '#ffffff' : '#1f2328';
}
