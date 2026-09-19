import { calculateDocumentTotals, roundHalfAwayFromZero } from './document-totals';

describe('roundHalfAwayFromZero', () => {
  it('rounds halves away from zero for positive and negative values', () => {
    expect(roundHalfAwayFromZero(2.345)).toBe(2.35);
    expect(roundHalfAwayFromZero(-2.345)).toBe(-2.35);
    expect(roundHalfAwayFromZero(2.344)).toBe(2.34);
    expect(roundHalfAwayFromZero(-2.344)).toBe(-2.34);
  });

  it('compensates floating point representation (1.005 -> 1.01)', () => {
    expect(roundHalfAwayFromZero(1.005)).toBe(1.01);
    expect(roundHalfAwayFromZero(-1.005)).toBe(-1.01);
    expect(roundHalfAwayFromZero(0.1 + 0.2)).toBe(0.3);
  });

  it('supports other rounding precisions', () => {
    expect(roundHalfAwayFromZero(1.025, 0.05)).toBe(1.05);
    expect(roundHalfAwayFromZero(12.5, 1)).toBe(13);
    expect(roundHalfAwayFromZero(-12.5, 1)).toBe(-13);
    expect(roundHalfAwayFromZero(1.23456, 0.001)).toBe(1.235);
  });

  it('never returns negative zero', () => {
    expect(Object.is(roundHalfAwayFromZero(-0.001), 0)).toBeTrue();
  });
});

describe('calculateDocumentTotals', () => {
  it('returns zeros for no lines', () => {
    expect(calculateDocumentTotals([])).toEqual({
      amountExclVat: 0,
      vatAmount: 0,
      amountInclVat: 0,
      lines: [],
    });
  });

  it('calculates a simple line with VAT', () => {
    const totals = calculateDocumentTotals([{ quantity: 2, unitPrice: 100, vatPercent: 16 }]);
    expect(totals.lines).toEqual([{ lineAmount: 200, vatAmount: 32, amountInclVat: 232 }]);
    expect(totals.amountExclVat).toBe(200);
    expect(totals.vatAmount).toBe(32);
    expect(totals.amountInclVat).toBe(232);
  });

  it('applies the line discount before VAT', () => {
    const totals = calculateDocumentTotals([
      { quantity: 3, unitPrice: 19.99, lineDiscountPercent: 10, vatPercent: 16 },
    ]);
    // 3 * 19.99 = 59.97; -10% = 53.973 -> 53.97; VAT 16% = 8.6352 -> 8.64
    expect(totals.lines[0]).toEqual({ lineAmount: 53.97, vatAmount: 8.64, amountInclVat: 62.61 });
    expect(totals.amountInclVat).toBe(62.61);
  });

  it('rounds every line half away from zero and sums the rounded lines', () => {
    const totals = calculateDocumentTotals([
      { quantity: 1, unitPrice: 1.005 }, // 1.005 -> 1.01
      { quantity: 1, unitPrice: 1.005 }, // 1.005 -> 1.01 (sum of raw values would give 2.01 too)
      { quantity: 1, unitPrice: 0.125, vatPercent: 20 }, // 0.125 -> 0.13; VAT 0.026 -> 0.03
    ]);
    expect(totals.lines.map(l => l.lineAmount)).toEqual([1.01, 1.01, 0.13]);
    expect(totals.amountExclVat).toBe(2.15);
    expect(totals.vatAmount).toBe(0.03);
    expect(totals.amountInclVat).toBe(2.18);
  });

  it('rounds negative (credit) lines away from zero', () => {
    const totals = calculateDocumentTotals([{ quantity: -1, unitPrice: 1.005, vatPercent: 50 }]);
    // VAT: -1.01 * 50% = -0.505 -> -0.51
    expect(totals.lines[0]).toEqual({ lineAmount: -1.01, vatAmount: -0.51, amountInclVat: -1.52 });
  });

  it('honours a custom rounding precision', () => {
    const totals = calculateDocumentTotals([{ quantity: 1, unitPrice: 10.33, vatPercent: 16 }], 1);
    expect(totals.lines[0]).toEqual({ lineAmount: 10, vatAmount: 2, amountInclVat: 12 });
  });

  it('treats missing or non numeric values as zero', () => {
    const totals = calculateDocumentTotals([
      { quantity: null as unknown as number, unitPrice: 10 },
      { quantity: 1, unitPrice: '5' as unknown as number },
    ]);
    expect(totals.amountExclVat).toBe(5);
    expect(totals.vatAmount).toBe(0);
  });

  it('does not accumulate floating point noise over many lines', () => {
    const lines = Array.from({ length: 1000 }, () => ({ quantity: 1, unitPrice: 0.1 }));
    expect(calculateDocumentTotals(lines).amountExclVat).toBe(100);
  });
});
