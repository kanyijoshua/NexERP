import { onColor, rgb } from './app-theme.service';

describe('theme colours', () => {
  it('splits a hex colour into its channels', () => {
    expect(rgb('#017E84')).toBe('1, 126, 132');
  });

  it('puts white text on dark colours and dark text on light ones', () => {
    expect(onColor('#714B67')).toBe('#ffffff');
    expect(onColor('#017E84')).toBe('#ffffff');
    expect(onColor('#FFD43B')).toBe('#1f2328');
    expect(onColor('#FFFFFF')).toBe('#1f2328');
  });
});
