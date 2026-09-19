import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats an amount with thousands separators and two decimals, WITHOUT a currency symbol.
 * `{{ 1234.5 | erpAmount }}` -> `1,234.50`, `{{ 1234.5 | erpAmount: 'KES' }}` -> `1,234.50 KES`.
 */
@Pipe({ name: 'erpAmount' })
export class ErpAmountPipe implements PipeTransform {
  private static readonly formatter = new Intl.NumberFormat('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

  transform(value: number | string | null | undefined, currencyCode?: string | null): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }
    const amount = typeof value === 'number' ? value : Number(value);
    if (!isFinite(amount)) {
      return '';
    }
    // Avoid "-0.00" for tiny negative values.
    const rounded = Math.round(Math.abs(amount) * 100) / 100 === 0 ? 0 : amount;
    const text = ErpAmountPipe.formatter.format(rounded);
    return currencyCode ? `${text} ${currencyCode}` : text;
  }
}
