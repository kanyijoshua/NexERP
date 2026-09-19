import { computeJournalBalance } from './journal-balance';

describe('computeJournalBalance', () => {
  it('is balanced with a zero total when there are no lines', () => {
    expect(computeJournalBalance([])).toEqual({ total: 0, byDocument: [], isBalanced: true });
  });

  it('groups by document number in order of first appearance', () => {
    const result = computeJournalBalance([
      { documentNo: 'G001', amount: 100 },
      { documentNo: 'G002', amount: 50 },
      { documentNo: 'G001', amount: -100 },
      { documentNo: 'G002', amount: -50 },
    ]);
    expect(result.byDocument).toEqual([
      { documentNo: 'G001', balance: 0 },
      { documentNo: 'G002', balance: 0 },
    ]);
    expect(result.total).toBe(0);
    expect(result.isBalanced).toBeTrue();
  });

  it('flags the out of balance document', () => {
    const result = computeJournalBalance([
      { documentNo: 'G001', amount: 100 },
      { documentNo: 'G001', amount: -100 },
      { documentNo: 'G002', amount: 75.5 },
      { documentNo: 'G002', amount: -70 },
    ]);
    expect(result.byDocument).toEqual([
      { documentNo: 'G001', balance: 0 },
      { documentNo: 'G002', balance: 5.5 },
    ]);
    expect(result.total).toBe(5.5);
    expect(result.isBalanced).toBeFalse();
  });

  it('is not balanced when documents only offset each other', () => {
    const result = computeJournalBalance([
      { documentNo: 'A', amount: 10 },
      { documentNo: 'B', amount: -10 },
    ]);
    expect(result.total).toBe(0);
    expect(result.isBalanced).toBeFalse();
  });

  it('rounds to 2 decimals so that 0.1 + 0.2 - 0.3 is balanced', () => {
    const result = computeJournalBalance([
      { documentNo: 'G001', amount: 0.1 },
      { documentNo: 'G001', amount: 0.2 },
      { documentNo: 'G001', amount: -0.3 },
    ]);
    expect(result.total).toBe(0);
    expect(Object.is(result.total, 0)).toBeTrue();
    expect(result.byDocument).toEqual([{ documentNo: 'G001', balance: 0 }]);
    expect(result.isBalanced).toBeTrue();
  });

  it('supports custom balance and document number fields', () => {
    const result = computeJournalBalance(
      [
        { docNo: 'X', amountLcy: 12.34 },
        { docNo: 'X', amountLcy: -12.34 },
      ],
      'amountLcy',
      'docNo',
    );
    expect(result.isBalanced).toBeTrue();
  });

  it('treats missing amounts as zero and missing document numbers as one blank document', () => {
    const result = computeJournalBalance([
      { amount: 5 },
      { documentNo: null, amount: '-5' },
      { documentNo: '  ', amount: undefined },
    ]);
    expect(result.byDocument).toEqual([{ documentNo: '', balance: 0 }]);
    expect(result.isBalanced).toBeTrue();
  });
});
