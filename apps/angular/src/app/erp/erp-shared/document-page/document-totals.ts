export interface DocumentTotalsLineInput {
  quantity: number;
  unitPrice: number;
  lineDiscountPercent?: number;
  vatPercent?: number;
}

export interface DocumentLineTotals {
  lineAmount: number;
  vatAmount: number;
  amountInclVat: number;
}

export interface DocumentTotals {
  amountExclVat: number;
  vatAmount: number;
  amountInclVat: number;
  lines: DocumentLineTotals[];
}

function decimalsOf(precision: number): number {
  const text = precision.toString();
  if (text.includes('e-')) {
    return Number(text.split('e-')[1]);
  }
  const dot = text.indexOf('.');
  return dot < 0 ? 0 : text.length - dot - 1;
}

/**
 * Rounds to the nearest multiple of `precision`, half AWAY from zero
 * (2.345 -> 2.35, -2.345 -> -2.35), compensating binary floating point noise
 * (1.005 is stored as 1.00499999999999989...).
 */
export function roundHalfAwayFromZero(value: number, precision = 0.01): number {
  if (!isFinite(value) || !(precision > 0)) {
    return 0;
  }
  const units = Number((Math.abs(value) / precision).toPrecision(12));
  const rounded = Math.round(units) * precision;
  const clean = Number(rounded.toFixed(Math.min(decimalsOf(precision), 20)));
  return clean === 0 ? 0 : Math.sign(value) * clean;
}

function toNumber(value: unknown): number {
  const n = typeof value === 'number' ? value : Number(value);
  return isFinite(n) ? n : 0;
}

/**
 * Client side preview of the document totals (the server stays the source of truth).
 * Every line is rounded on its own (half away from zero) and the totals are the sums of the
 * rounded lines, so that the footer always equals the sum of what the user sees per line.
 */
export function calculateDocumentTotals(
  lines: DocumentTotalsLineInput[],
  rounding = 0.01,
): DocumentTotals {
  const result: DocumentTotals = { amountExclVat: 0, vatAmount: 0, amountInclVat: 0, lines: [] };

  for (const line of lines ?? []) {
    const gross = toNumber(line.quantity) * toNumber(line.unitPrice);
    const discount = toNumber(line.lineDiscountPercent);
    const lineAmount = roundHalfAwayFromZero(gross * (1 - discount / 100), rounding);
    const vatAmount = roundHalfAwayFromZero(
      (lineAmount * toNumber(line.vatPercent)) / 100,
      rounding,
    );
    const amountInclVat = roundHalfAwayFromZero(lineAmount + vatAmount, rounding);

    result.lines.push({ lineAmount, vatAmount, amountInclVat });
    result.amountExclVat += lineAmount;
    result.vatAmount += vatAmount;
  }

  result.amountExclVat = roundHalfAwayFromZero(result.amountExclVat, rounding);
  result.vatAmount = roundHalfAwayFromZero(result.vatAmount, rounding);
  result.amountInclVat = roundHalfAwayFromZero(result.amountExclVat + result.vatAmount, rounding);
  return result;
}
