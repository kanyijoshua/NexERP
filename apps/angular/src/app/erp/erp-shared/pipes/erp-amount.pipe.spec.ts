import { ErpAmountPipe } from './erp-amount.pipe';

describe('ErpAmountPipe', () => {
  const pipe = new ErpAmountPipe();

  it('formats with two decimals and thousands separators, without a currency symbol', () => {
    expect(pipe.transform(1234567.891)).toBe('1,234,567.89');
    expect(pipe.transform(5)).toBe('5.00');
    expect(pipe.transform(0)).toBe('0.00');
  });

  it('formats negative amounts', () => {
    expect(pipe.transform(-1234.5)).toBe('-1,234.50');
    expect(pipe.transform(-0.001)).toBe('0.00');
  });

  it('appends the optional currency code', () => {
    expect(pipe.transform(1000, 'KES')).toBe('1,000.00 KES');
  });

  it('accepts numeric strings', () => {
    expect(pipe.transform('42.1')).toBe('42.10');
  });

  it('returns blank for null, undefined, empty and non numeric values', () => {
    expect(pipe.transform(null)).toBe('');
    expect(pipe.transform(undefined)).toBe('');
    expect(pipe.transform('')).toBe('');
    expect(pipe.transform('abc')).toBe('');
  });
});
