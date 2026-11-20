import { actionCategoryOf, groupActions, isPromoted } from './action-bar';

describe('action bar grouping', () => {
  it('puts actions in their group by key', () => {
    expect(actionCategoryOf({ key: 'release' })).toBe('release');
    expect(actionCategoryOf({ key: 'reopen' })).toBe('release');
    expect(actionCategoryOf({ key: 'runPosting' })).toBe('posting');
    expect(actionCategoryOf({ key: 'runPrepaymentPosting' })).toBe('posting');
    expect(actionCategoryOf({ key: 'sendApprovalRequest' })).toBe('approval');
    expect(actionCategoryOf({ key: 'cancelApprovalRequest' })).toBe('approval');
    expect(actionCategoryOf({ key: 'approve' })).toBe('approval');
    expect(actionCategoryOf({ key: 'getSourceLines' })).toBe('prepare');
    expect(actionCategoryOf({ key: 'calculate' })).toBe('prepare');
    expect(actionCategoryOf({ key: 'makeOrder' })).toBe('newDocument');
    expect(actionCategoryOf({ key: 'sendEmail' })).toBe('printSend');
    expect(actionCategoryOf({ key: 'block' })).toBe('process');
  });

  it('lets the descriptor choose the group and promotion', () => {
    expect(actionCategoryOf({ key: 'release', category: 'prepare' })).toBe('prepare');
    expect(isPromoted({ key: 'runPosting' })).toBeTrue();
    expect(isPromoted({ key: 'runPosting', promoted: false })).toBeFalse();
    expect(isPromoted({ key: 'block', promoted: true })).toBeTrue();
  });

  it('orders groups by the bar and leaves out empty ones', () => {
    const groups = groupActions([
      { key: 'sendEmail', labelKey: '', icon: '' },
      { key: '$delete', labelKey: '', icon: '', category: 'manage' },
      { key: 'runPosting', labelKey: '', icon: '' },
      { key: 'release', labelKey: '', icon: '' },
      { key: 'reopen', labelKey: '', icon: '' },
    ]);
    expect(groups.map(g => g.category)).toEqual(['release', 'posting', 'printSend', 'manage']);
    expect(groups[0].actions.map(a => a.key)).toEqual(['release', 'reopen']);
  });
});
