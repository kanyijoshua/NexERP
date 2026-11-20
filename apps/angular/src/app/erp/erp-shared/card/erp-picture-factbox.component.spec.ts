import { avatarColourOf, initialsOf } from './erp-picture-factbox.component';
import { FactBoxVisibilityService } from './factbox-visibility.service';

describe('picture FactBox helpers', () => {
  it('takes the first and last initials of a name', () => {
    expect(initialsOf('Lakeside Stationers')).toBe('LS');
    expect(initialsOf('  mary anne  wanjiru ')).toBe('MW');
    expect(initialsOf('Acme')).toBe('AC');
    expect(initialsOf('')).toBe('?');
    expect(initialsOf(null)).toBe('?');
  });

  it('gives a name the same colour every time', () => {
    expect(avatarColourOf('Lakeside Stationers')).toBe(avatarColourOf('Lakeside Stationers'));
  });
});

describe('FactBoxVisibilityService', () => {
  beforeEach(() => localStorage.removeItem('erp.factbox.visible'));
  afterEach(() => localStorage.removeItem('erp.factbox.visible'));

  it('is shown by default and remembers a hide', () => {
    const first = new FactBoxVisibilityService();
    expect(first.visible()).toBeTrue();

    first.toggle();
    expect(first.visible()).toBeFalse();
    expect(new FactBoxVisibilityService().visible()).toBeFalse();
  });

  it('recognises Alt+F2 only', () => {
    expect(FactBoxVisibilityService.isToggleShortcut(new KeyboardEvent('keydown', { key: 'F2', altKey: true }))).toBeTrue();
    expect(FactBoxVisibilityService.isToggleShortcut(new KeyboardEvent('keydown', { key: 'F2' }))).toBeFalse();
  });
});
