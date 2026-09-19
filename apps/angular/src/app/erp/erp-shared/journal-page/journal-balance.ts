export interface JournalDocumentBalance {
  documentNo: string;
  balance: number;
}

export interface JournalBalance {
  /** Sum of all lines, rounded to 2 decimals. */
  total: number;
  /** One entry per document number, in order of first appearance. */
  byDocument: JournalDocumentBalance[];
  /** True when every document number nets to zero. */
  isBalanced: boolean;
}

function round2(value: number): number {
  const rounded = Math.sign(value) * (Math.round(Math.abs(value) * 100 + 1e-7) / 100);
  return rounded === 0 ? 0 : rounded;
}

/**
 * Balance of a journal: debits are positive amounts and credits negative ones, a document is
 * balanced when its lines net to zero. Sums are rounded to 2 decimals so that binary floating
 * point noise (0.1 + 0.2 - 0.3) does not flag a document as out of balance.
 */
export function computeJournalBalance(
  lines: ReadonlyArray<Record<string, unknown>>,
  balanceField = 'amount',
  documentNoField = 'documentNo',
): JournalBalance {
  const sums = new Map<string, number>();
  let total = 0;

  for (const line of lines ?? []) {
    const raw = line?.[balanceField];
    const parsed = typeof raw === 'number' ? raw : Number(raw);
    const amount = isFinite(parsed) ? parsed : 0;
    const documentNo = String(line?.[documentNoField] ?? '').trim();

    sums.set(documentNo, (sums.get(documentNo) ?? 0) + amount);
    total += amount;
  }

  const byDocument: JournalDocumentBalance[] = [];
  sums.forEach((balance, documentNo) => byDocument.push({ documentNo, balance: round2(balance) }));

  return {
    total: round2(total),
    byDocument,
    isBalanced: byDocument.every(d => d.balance === 0),
  };
}
